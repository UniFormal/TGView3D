using UnityEngine;
using System.Collections;
using UnityEngine.XR;

public class FlyCamera : MonoBehaviour
{

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/


    float mainSpeed = 1.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = 1f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;
    [SerializeField]
    GameObject VR;
    Color Col;
    
    private float screenDist;
    private Vector3 startPos = new Vector3(255, 255, 255);
    void Update()
    {
        Transform transform = this.transform.parent.transform;
        if (Input.GetMouseButton(0))
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            //Mouse  camera angle done.  
        }
        lastMouse = Input.mousePosition;

        if (TGraph.GlobalVariables.Init==true && Input.GetMouseButtonDown(1))
        {
            Debug.Log("shoot");
            RaycastHit hit;

            // Does the ray intersect any objects excluding the player layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Did Hit"+ hit.transform.gameObject);
                // Debug.Log(ray.origin + " " + hit.point + " " + (ray.origin + ray.direction.normalized * hit.distance)+" " + Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,Camera.main.nearClipPlane)));

                if (TGraph.GlobalVariables.Graph.latestSelection != -1)
                    TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].nodeObject.GetComponent<MeshRenderer>().material.color = Col;
                TGraph.GlobalVariables.Graph.selectedNodes[0] = TGraph.GlobalVariables.Graph.latestSelection = hit.transform.GetSiblingIndex();
                TGraph.GlobalVariables.Graph.movingNodes.Add(hit.transform.GetSiblingIndex());
                Col = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.GetComponent<MeshRenderer>().material.color;
                TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.GetComponent<MeshRenderer>().material.color = OVRTouchSample.ColorGrabbable.COLOR_GRAB;

                startPos = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.position-hit.point;

                // Debug.Log(ray.direction.normalized.ToString("F4") + " " + (hit.transform.position - Camera.main.transform.position).normalized.ToString("F4"));

            
                screenDist= Camera.main.WorldToScreenPoint(hit.point).z;

            }
            else
            {
               // TGraph.GlobalVariables.Graph.selectedNodes[0] = -1;

            }
        }

        if (TGraph.GlobalVariables.Init == true && Input.GetMouseButton(1) && TGraph.GlobalVariables.Graph.movingNodes.Count>0)
        {
           
            var mou = Input.mousePosition;
            mou.z = screenDist;
            TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.position = Camera.main.ScreenToWorldPoint(mou) +startPos;
           // Debug.Log((TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.position.z - transform.position.z));
                /*    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  var m_Plane = new Plane(this.transform.forward, TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.position);
            float enter = 0.0f;
            Debug.Log("drag");
    if (m_Plane.Raycast(ray, out enter))
            {
                
                //Get the point that is clicked 
                Vector3 hitPoint = ray.GetPoint(enter);
              
                Debug.Log(screenDist - this.transform.position);

                //Move your cube GameObject to the point where you clicked
                TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.position = startPos-screenDist+transform.position;
            }*/
           
           // Debug.Log(TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.localPosition + " " + lastMouse + " " + nodeMouse);
           // TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.transform.localPosition = startPos + Camera.main.ScreenToWorldPoint(new Vector3(lastMouse.x,lastMouse.y,nodeMouse.z)) - Camera.main.ScreenToWorldPoint(nodeMouse);
        }
        if (TGraph.GlobalVariables.Init == true && Input.GetMouseButtonUp(1) && TGraph.GlobalVariables.Graph.movingNodes.Count > 0)
        {
            TGraph.GlobalVariables.Graph.movingNodes.Clear();

        }


            //Keyboard commands
            float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space))
        { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }

    }

    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();

     
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        

 

        return p_Velocity;
    }
    private void Start()
    {
         if(VR.activeSelf)
            VR.SetActive(false);
      //   XRSettings.enabled = false;
    }
}