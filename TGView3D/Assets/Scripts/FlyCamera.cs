using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using TGraph;

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


    float mainSpeed = 4.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = .2f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;
    private Vector3 LastMou;
    public string Startid = "";
    [SerializeField]
    GameObject VR;
    Color Col;

    private float screenDist;
    private List<Vector3> StartPositions = new List<Vector3>();
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void openWindow(string url);

    [SerializeField]
    private GameObject nodeText;

    public Toggle EditToggle;

    void OnGUI()
    {
        if (Event.current.isMouse && Event.current.button == 0 && Event.current.clickCount > 1)
        {
            Debug.Log("double");
            RaycastHit hit;

            // Does the ray intersect any objects excluding the player layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.transform.tag == "Node")
            {
                if (GlobalVariables.JsonManager.OpenChapter(GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].id)) return;

#if UNITY_WEBGL && !UNITY_EDITOR
                     
                openWindow("https://mmt.mathhub.info" + TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].url);
#else
                Application.OpenURL("https://mmt.mathhub.info" + TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].url);
#endif

            }
        }

    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {

        Debug.Log(eventData.clickCount);
        if (eventData.clickCount == 2)
        {
            Debug.Log("double");
            RaycastHit hit;

            // Does the ray intersect any objects excluding the player layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {

#if UNITY_WEBGL && !UNITY_EDITOR
                     
                openWindow("https://mmt.mathhub.info" + TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].url);
#else
                Application.OpenURL("https://mmt.mathhub.info" + TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].url);
#endif

            }
        }
    }


    public void DeselectNode(int nodeId)
    {
        var graph = TGraph.GlobalVariables.Graph;
        var graphNode = graph.nodes[nodeId];

        graphNode.labelObject.GetComponent<TextMeshPro>().color = TGraph.ReadJSON.BaseColor;
        graphNode.labelObject.layer = 18;
        foreach (int nidx in graphNode.connectedNodes)
        {
            graph.nodes[nidx].labelObject.layer = 18;
            graph.nodes[nidx].labelObject.GetComponent<TextMeshPro>().color = TGraph.ReadJSON.BaseColor;
        }
        GameObject.Destroy(graphNode.nodeEdgeObject);
        graphNode.nodeEdgeObject = null;

        int idx = graph.selectedNodes.FindIndex(x => x == nodeId);
        if (idx >= 0) graph.selectedNodes.RemoveAt(idx);

        DestroySubMenu(graphNode);

    }


    public void SelectEdge(int edgeId, RaycastHit hit)
    {
        var graph = TGraph.GlobalVariables.Graph;

        // if (graph.SelectedEdges.Contains(edgeId))
        {
            foreach (var edge in graph.SelectedEdges)
                DeselectEdge();

        }
        graph.selectedNodes.Clear();
        graph.SelectedEdges.Add(edgeId);


        ActivateSubMenu(edgeId, hit);
    }

    public void DeselectEdge()
    {

        DestroySubMenu();
    }




    public bool SelectNode(int nodeId)
    {

        var graph = TGraph.GlobalVariables.Graph;
        TGraph.ReadJSON.MyNode node = graph.nodes[nodeId];

 
        //edge drawing
        if (Startid != "")
        {
            Debug.Log(Startid);
            TGraph.GlobalVariables.JsonManager.AddEdge(graph.nodes[graph.nodeDict[Startid]], graph.nodes[nodeId]);
            Startid = "";
            return true;
        }




        Debug.Log(graph.latestSelection);
        //TODO: look into this
        if (graph.selectedNodes.Contains(nodeId))
        {
            DeselectNode(nodeId);
            return false;

        }


        //new node selected
        Debug.Log(nodeId);
        if (graph.latestSelection != -1 && graph.latestSelection != nodeId) DeselectNode(graph.latestSelection);



        //current Selection exists
        /*  if (graph.selectedNodes[0] != -1)
          {

              foreach(var idx in graph.selectedNodes)
              {
                  if (idx != -1)
                  {
                      var graphNode = graph.nodes[idx];

                      graphNode.labelObject.GetComponent<TextMesh>().color = TGraph.ReadJSON.BaseColor;
                      graphNode.labelObject.layer = 18;
                      foreach (int nidx in graphNode.connectedNodes)
                      {
                          graph.nodes[nidx].labelObject.layer = 18;
                          graph.nodes[nidx].labelObject.GetComponent<TextMesh>().color = TGraph.ReadJSON.BaseColor;
                      }
                      GameObject.Destroy(graphNode.nodeEdgeObject);
                      graphNode.nodeEdgeObject = null;
                  }

              }

          }*/









        // Debug.Log(nodeId + " other has"+ graph.selectedNodes[(handIndex + 1) % 2]);

        //select node and highlight accordingly
        //if (nodeId != graph.selectedNodes[(handIndex + 1) % 2])


        if (graph.latestSelection != nodeId)
        {

            //Activate hidden chapters

         


  



            var edges = new List<TGraph.ReadJSON.MyEdge>();
            node.labelObject.GetComponent<TextMeshPro>().color = TGraph.ReadJSON.SelectedColor;
            node.labelObject.layer = 0;
            foreach (int nidx in node.connectedNodes)
            {
                graph.nodes[nidx].labelObject.layer = 0;

                if (graph.nodes[nidx].labelObject.GetComponent<TextMeshPro>().color != TGraph.ReadJSON.SelectedColor) graph.nodes[nidx].labelObject.GetComponent<TextMeshPro>().color = TGraph.ReadJSON.ConnectedColor;
            }
            foreach (int idx in node.edgeIndicesIn)
            {
                edges.Add(graph.edges[idx]);
            }
            foreach (int idx in node.edgeIndicesOut)
            {
                edges.Add(graph.edges[idx]);
            }


            graph.nodes[nodeId].nodeEdgeObject = TGraph.GraphManager.BuildEdges(edges, ref graph, graph.edgeObject.GetComponent<MeshRenderer>().sharedMaterial);
            graph.nodes[nodeId].nodeEdgeObject.transform.parent = graph.edgeObject.transform.parent;
            graph.nodes[nodeId].nodeEdgeObject.transform.localPosition = Vector3.zero;
            graph.nodes[nodeId].nodeEdgeObject.transform.localEulerAngles = Vector3.zero;


            //  graph.currentTarget = -1;
            ActivateSubMenu(node);
        }
        graph.selectedNodes.Add((nodeId));
        graph.latestSelection = nodeId;
        /*
        else
        {
            graph.selectedNodes[handIndex] = graph.nodes[graph.selectedNodes[(handIndex + 1) % 2]].nr;
            graph.selectedNodes[(handIndex + 1) % 2] = -1;
        }*/
        graph.movingNodes.Add(nodeId);







        return true;

    }

    public void ActivateSubMenu(TGraph.ReadJSON.MyNode node)
    {
        if (!EditToggle.isOn) return;
        GameObject menu = GameObject.Instantiate(Resources.Load("NodeMenu")) as GameObject;
        menu.transform.parent = node.nodeObject.transform;
        menu.transform.localPosition = Vector3.zero - Camera.main.transform.forward;
        //   menu.transform.LookAt(menu.transform.position*2-Camera.main.transform.position);
        menu.transform.forward = Camera.main.transform.forward;
        menu.transform.localScale = Vector3.one * .02f;

        int offset = 0;
        var field = menu.GetComponent<SubMenu>().FieldInput;
        foreach (FieldInfo info in node.GetType().GetFields())
        {
            if (!info.IsNotSerialized && info.GetValue(node) != null)
            {

                var f = GameObject.Instantiate(field);
                f.transform.parent = menu.transform;
                f.transform.position = field.transform.position;
                f.transform.forward = menu.transform.forward;
                f.transform.localScale = Vector3.one;
                f.transform.localPosition += Vector3.down * offset * 25f;
                var texts = f.GetComponentsInChildren<Text>();
                texts[0].text = info.Name;
                f.GetComponentInChildren<InputField>().text = info.GetValue(node).ToString();

                offset++;
            }
        }
        GameObject.Destroy(field);



    }


    public void ActivateSubMenu(int edgeId, RaycastHit hit)
    {
        if (!EditToggle.isOn) return;
        var edge = TGraph.GlobalVariables.Graph.edges[edgeId];
        GameObject menu = GameObject.Instantiate(Resources.Load("EdgeMenu")) as GameObject;
        menu.GetComponent<SubMenu>().ID = edgeId;

        // menu.transform.parent = edge.GetObject().transform;
        menu.transform.position = hit.point;
        //   menu.transform.LookAt(menu.transform.position*2-Camera.main.transform.position);
        menu.transform.forward = Camera.main.transform.forward;
        menu.transform.SetParent(edge.GetObject().transform);
        menu.transform.localScale = Vector3.one * .002f;

        int offset = 0;
        var field = menu.GetComponent<SubMenu>().FieldInput;
        foreach (FieldInfo info in edge.GetType().GetFields())
        {
            if (!info.IsNotSerialized && info.GetValue(edge) != null)
            {

                var f = GameObject.Instantiate(field);
                f.transform.parent = menu.transform;
                f.transform.position = field.transform.position;
                f.transform.forward = menu.transform.forward;
                f.transform.localScale = Vector3.one;
                f.transform.localPosition += Vector3.down * offset * 25f;
                var texts = f.GetComponentsInChildren<Text>();
                texts[0].text = info.Name;
                f.GetComponentInChildren<InputField>().text = info.GetValue(edge).ToString();

                offset++;
            }
        }
        GameObject.Destroy(field);

    }

    public void DestroySubMenu(TGraph.ReadJSON.MyNode node)
    {
        if (!EditToggle.isOn) return;
        GameObject.Destroy(node.nodeObject.GetComponentInChildren<SubMenu>().gameObject);
    }

    public void DestroySubMenu()
    {
        if (!EditToggle.isOn) return;
        GameObject.Destroy(TGraph.GlobalVariables.Graph.edgeObject.GetComponentInChildren<SubMenu>().gameObject);
    }



    void Update()
    {


        Transform transform = this.transform.parent.transform;

        if (Input.GetMouseButton(2))
        {
            /*
           var cP = Input.mousePosition;
           cP.z = Camera.main.transform.position.z; 

           var lP = lastMouse;
           lP.z = Camera.main.transform.position.z;
           this.transform.position += Camera.main.ScreenToWorldPoint(cP)-Camera.main.ScreenToWorldPoint(lP);


           */
            var g = GameObject.Find("Main").transform.position;
            g = Camera.main.WorldToScreenPoint(g);
            var cP = Input.mousePosition;
            cP.z = g.z;

            var lP = lastMouse;
            lP.z = g.z;
            Vector3 diff = Vector3.zero;
            if (lP.z > 0)
                diff = Camera.main.ScreenToWorldPoint(cP) - Camera.main.ScreenToWorldPoint(lP);
            else
                diff = -(Camera.main.ScreenToWorldPoint(cP) - Camera.main.ScreenToWorldPoint(lP));

            transform.position -= diff;

        }

        if (Input.GetMouseButton(1))
        {
            var graphTransform = TGraph.GlobalVariables.JsonManager.transform;
            lastMouse = -(Input.mousePosition - lastMouse);
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(graphTransform.eulerAngles.x + lastMouse.x, graphTransform.eulerAngles.y + lastMouse.y, 0);
            graphTransform.eulerAngles = TGraph.GlobalVariables.Rotation = lastMouse;



            //Mouse  camera angle done.  
        }
        lastMouse = Input.mousePosition;

        if (TGraph.GlobalVariables.Init == true && Input.GetMouseButtonDown(0))
        {
            Debug.Log("shoot");
            RaycastHit hit;
            Physics.queriesHitBackfaces = true;
           // var mC = TGraph.GlobalVariables.Graph.edgeObject.AddComponent<MeshCollider>();
            //TODO: activate this for edge collision
          //  mC.sharedMesh = TGraph.GlobalVariables.Graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Physics.autoSimulation = true;
            LastMou = Input.mousePosition;
            //  Debug.Log(LastMou);
            // Does the ray intersect any objects excluding the player layer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Did Hit " + hit.transform.gameObject+ " id: "+ TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].id+" parent: "+ TGraph.GlobalVariables.Graph.nodes[hit.transform.GetSiblingIndex()].parentId);
                // Debug.Log(ray.origin + " " + hit.point + " " + (ray.origin + ray.direction.normalized * hit.distance)+" " + Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,Camera.main.nearClipPlane)));

                //  Debug.Log(Startid);


                if (hit.transform.tag == "Node" && SelectNode(hit.transform.GetSiblingIndex()))
                {

                    screenDist = Camera.main.WorldToScreenPoint(hit.point).z;
                    Debug.Log(TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].nodeObject.transform.position + " " + hit.point);
                }
                else if (hit.transform.tag == "Edge")
                {
                    var edge = TGraph.GlobalVariables.Graph.edges[hit.triangleIndex / 8];
                    //   TGraph.GlobalVariables.Graph.edges.Remove(edge);
                    if (edge.active)
                        SelectEdge(hit.triangleIndex / 8, hit);


                    //  TGraph.GlobalVariables.JsonManager.UpdateJSON();
                    //  TGraph.GlobalVariables.JsonManager.UpdateLayout();
                }

                /*    StartPositions.Clear();
                     if(TGraph.GlobalVariables.Graph.latestSelection!=-1)
                        StartPositions.Add(TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].nodeObject.transform.position);// - hit.point);
                   */ /*

                    if (TGraph.GlobalVariables.Graph.latestSelection != -1)
                        TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].nodeObject.GetComponent<MeshRenderer>().material.color = Col;


                    TGraph.GlobalVariables.Graph.selectedNodes[0] = TGraph.GlobalVariables.Graph.latestSelection = hit.transform.GetSiblingIndex();
                    TGraph.GlobalVariables.Graph.movingNodes.Add(hit.transform.GetSiblingIndex());
                    Col = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.GetComponent<MeshRenderer>().material.color;
                    TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[0]].nodeObject.GetComponent<MeshRenderer>().material.color = OVRTouchSample.ColorGrabbable.COLOR_GRAB;
                    */




                // Debug.Log(ray.direction.normalized.ToString("F4") + " " + (hit.transform.position - Camera.main.transform.position).normalized.ToString("F4"));

            }
            else
            {
                /*
                Debug.Log("miss");
                if (Physics.SphereCast(ray, .1f, out hit, Mathf.Infinity))
                {
                    Debug.Log("Did Hit with spherecast " + hit.transform.gameObject);



                    if (hit.transform.tag=="Node"&&SelectNode(hit.transform.GetSiblingIndex()))
                    {

                        screenDist = Camera.main.WorldToScreenPoint(hit.point).z;
                        Debug.Log(TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].nodeObject.transform.position + " " + hit.point);
                    }
                    else
                    {
                        Debug.Log(TGraph.GlobalVariables.Graph.edges[hit.triangleIndex / 8].from + " "+ TGraph.GlobalVariables.Graph.edges[hit.triangleIndex / 8].to);

                    }

                    // TGraph.GlobalVariables.Graph.selectedNodes[0] = -1;
                    //if(TGraph.GlobalVariables.Graph.selectedNodes.Count>0) DeselectNode(TGraph.GlobalVariables.Graph.latestSelection);
                   

                }else
                Debug.Log("miss2");
                Physics.autoSimulation = false;
                Destroy(mC);
                */
            }


        }

        else if (TGraph.GlobalVariables.Init == true && Input.GetMouseButton(0) && TGraph.GlobalVariables.Graph.movingNodes.Count > 0)
        {

            var mou = Input.mousePosition;
            mou.z = screenDist;
            LastMou.z = screenDist;
            var mouseDelta = (Camera.main.ScreenToWorldPoint(mou) - Camera.main.ScreenToWorldPoint(LastMou));
            //  Debug.Log(Camera.main.ScreenToWorldPoint(mou)+" "+mou +" "+mouseDelta+" "+ Camera.main.ScreenToWorldPoint(LastMou)+" "+LastMou);
            // mou.z = screenDist;
            for (int i = 0; i < TGraph.GlobalVariables.Graph.selectedNodes.Count; ++i)
            {

                TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.selectedNodes[i]].nodeObject.transform.position += mouseDelta;
                LastMou = mou;
                //  Debug.Log(StartPositions[i]);
            }

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
        if (TGraph.GlobalVariables.Init == true && Input.GetMouseButtonUp(0) && TGraph.GlobalVariables.Graph.movingNodes.Count > 0)
        {

            // TGraph.GlobalVariables.Graph.movingNodes.Clear();
            TGraph.GlobalVariables.Graph.selectedNodes.Clear();

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

        if (Input.GetMouseButton(0))
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                p_Velocity += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                p_Velocity += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                p_Velocity += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
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
        }
        Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * Screen.width);
        p_Velocity += Input.mouseScrollDelta.y * (target - Camera.main.transform.position).normalized * 10;
        //  p_Velocity += Input.mouseScrollDelta.x * Vector3.right * 4;




        return p_Velocity;
    }

    public IEnumerator ZoomIn()
    {
        for (int i = 0; i < 40; i++)
        {
            transform.parent.position += transform.forward * 0.5f;
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < 30; i++)
        {
            transform.parent.position -= transform.forward * 0.5f;
            yield return new WaitForEndOfFrame();
        }

    }




    public void InvertColors()
    {
        var texts = GetComponentsInChildren<Text>();

        Camera.main.backgroundColor = Color.white - Camera.main.backgroundColor;

        var tcol = new Color();
        tcol = TGraph.GlobalVariables.GraphManager.NodeText.GetComponent<TextMeshPro>().color;
        Debug.Log(tcol);

        tcol = Color.white - tcol;
        tcol.a = 1;
        Debug.Log(tcol);

        TGraph.GlobalVariables.GraphManager.NodeText.GetComponent<TextMeshPro>().color = tcol;

        List<string> keys = new List<string>(TGraph.ReadJSON.ColorDict.Keys);

        foreach (var key in keys)
        {
            var col = TGraph.ReadJSON.ColorDict[key];
            col.a = 4 - col.a;
            TGraph.ReadJSON.ColorDict[key] = col;
        }
        if (TGraph.GlobalVariables.Init)
        {
            TGraph.GlobalVariables.UIInteractonManager.Init();
            UIInteracton.ReColor();
        }

        var no = GameObject.Find("Nodes");
        Debug.Log(no);
        if (no != null)
        {
            var textMs = no.GetComponentsInChildren<TextMeshPro>();

            foreach (var text in textMs)
            {

                var col = Color.white - new Color(text.color.r, text.color.g, text.color.b, 0);
                col.a = text.color.a;
                text.color = col;
            }

        }

        /*
         * 
        foreach (var text in texts)
        {

            var col = Color.white - new Color(text.color.r, text.color.g, text.color.b, 0);
            col.a = text.color.a;
            text.color = col;
        }
        var images = GetComponentsInChildren<InputField>();
        foreach (var image in images)
        {
            var col = Color.white - new Color(image.colors.normalColor.r, image.colors.normalColor.g, image.colors.normalColor.b, 0);
            col.a = image.colors.normalColor.a;
            var cols = image.colors;
            cols.normalColor = col;

            image.colors = cols;
        }

        var bImages = GetComponentsInChildren<Button>();
        foreach (var image in bImages)
        {
            var col = Color.white - new Color(image.colors.normalColor.r, image.colors.normalColor.g, image.colors.normalColor.b, 0);
            col.a = image.colors.normalColor.a;
            var cols = image.colors;
            cols.normalColor = col;

            image.colors = cols;
        }
        */
    }

    private void Start()
    {
        GlobalVariables.MouseManager = this;
        if(TGraph.GlobalVariables.GraphManager!=null)
        TGraph.GlobalVariables.GraphManager.NodeText.GetComponent<TextMeshPro>().color = nodeText.GetComponent<TextMesh>().color;
        if (VR.activeSelf)
            VR.SetActive(false);


        //   XRSettings.enabled = false;
    }
}