using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using TGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInteracton : MonoBehaviour {

    bool outside = true;
    Dictionary<string,bool> Edgeticked;

    private void Start()
    {
        Edgeticked = new Dictionary<string, bool>();
        Edgeticked["meta"] = true;
        Edgeticked["view"] = true;
        Edgeticked["structure"] = true;
        Edgeticked["alignment"] = true;
    }




    private void OnTriggerEnter(Collider other)
    {

        // this.GetComponent<Button>().Select();

       // Debug.Log("enter"+ other.gameObject.name);
        Toggle t = other.GetComponent<Toggle>();
 
        if (t != null&& outside)
        {
        
            t.Select();
            EventSystem eS = GlobalVariables.EventSystem;
            var data = new PointerEventData(eS);
            ExecuteEvents.Execute(eS.currentSelectedGameObject, data, ExecuteEvents.pointerClickHandler);
            outside = false;

        }
        else
        {
            Button b = other.GetComponent<Button>();
            if (b != null && outside)
            {
                b.Select();
                EventSystem eS = GlobalVariables.EventSystem;
                var data = new PointerEventData(eS);
                ExecuteEvents.Execute(eS.currentSelectedGameObject, data, ExecuteEvents.pointerClickHandler);
                outside = false;
            }
            else
            {
               
            }        
          
        }


        Dropdown d = other.GetComponent<Dropdown>();
        if (d != null && outside)
        {
            d.Select();
            EventSystem eS = GlobalVariables.EventSystem;
            var data = new PointerEventData(eS);
            ExecuteEvents.Execute(eS.currentSelectedGameObject, data, ExecuteEvents.pointerClickHandler);
            outside = false;
        }


        /*  var colors = this.GetComponent<Button>().colors;
          colors.normalColor = Color.gray;
          this.GetComponent<Button>().colors = colors;
          */


    }
    private void OnTriggerExit(Collider other)
    {

        outside = true;
       
        
    }

    public void ChangeUrl(Dropdown d)
    {
        GlobalVariables.Url = "file:///" + Application.dataPath + "/" + d.captionText.text + ".json";
        Debug.Log(GlobalVariables.Url);
    }

    public void ChangeClipPlane(){
        if (Camera.main.farClipPlane == 12) Camera.main.farClipPlane = 100;
        else Camera.main.farClipPlane = 12;
    }

            
    public void ActivateEdges()
    {
        var graph = GlobalVariables.Graph;
        graph.edgeObject.SetActive(!graph.edgeObject.activeSelf);
    }
    
          
    public void EnableBeam()
    {
        var grabbers = transform.parent.GetComponentsInChildren<DistanceGrabber>();
        

        if (grabbers[0].m_maxGrabDistance == 0)
        {
            grabbers[0].m_maxGrabDistance = 10;
            grabbers[1].m_maxGrabDistance = 10;

        }
        else
        {
            GameObject.Find("LCone").GetComponent<MeshRenderer>().enabled = false;
            GameObject.Find("RCone").GetComponent<MeshRenderer>().enabled = false;
            grabbers[0].m_maxGrabDistance = 0;
            grabbers[1].m_maxGrabDistance = 0;
        }

    }

    public void EnableEdgeType(string type)
    {

        var graph = GlobalVariables.Graph;  
        var edges = graph.edges;
        var mesh = graph.edgeObject.GetComponent<MeshFilter>().mesh;
        Color[] vertexColors = mesh.colors;
        
        if (Edgeticked[type])
        {
            for (int i = 0; i < edges.Count; i++)
            {

              
                if (edges[i].style == type)
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = new Color(0, 0, 0, 0);

            }
            Edgeticked[type] = false;

        }
        else
        {
            for (int i = 0; i < edges.Count; i++)
            {

                if (edges[i].style == type)
                {
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = graph.colorDict[type];
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = graph.colorDict[type] + new Color(0, 0, 255);
                }
                   
            }
            Edgeticked[type] = true;
        }

        mesh.colors = vertexColors;
        

    }




}
