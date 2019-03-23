using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using TGraph;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;


public class UIInteracton : MonoBehaviour {

    bool outside = true;
    public GameObject Desktop;
    public GameObject UIOverlay;

    private void Start()
    {
       

    }


    public void EnableDesktop()
    {
       Desktop.SetActive(true);
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
      //  GlobalVariables.Url = "file:///" + Application.dataPath + "/" + d.captionText.text + ".json";
        GlobalVariables.SelectionIndex = d.value;
        //GlobalVariables.Url = "";
        Debug.Log(d.value);

    }

    public void ChangeClipPlane(){
        if (Camera.main.farClipPlane == 3) Camera.main.farClipPlane = 100;
        else Camera.main.farClipPlane = 3;
    }

            
    public void ActivateEdges()
    {
        var graph = GlobalVariables.Graph;
        graph.edgeObject.SetActive(!graph.edgeObject.activeSelf);
    }
    public void EnableUI()
    {
        Debug.Log(0);
        if (UIOverlay.GetComponent<RectTransform>().localScale.x == 1)
            UIOverlay.GetComponent<RectTransform>().localScale = Vector3.zero;
        else
            UIOverlay.GetComponent<RectTransform>().localScale = Vector3.one;
    }


    public void EnableBeam()
    {
        var grabbers = transform.parent.GetComponentsInChildren<DistanceGrabber>();
        //TODO: better solution
        GameObject.Find("LCone").GetComponent<MeshRenderer>().enabled = GameObject.Find("RCone").GetComponent<MeshRenderer>().enabled =
        GameObject.Find("LCone").GetComponent<MeshCollider>().enabled = GameObject.Find("RCone").GetComponent<MeshCollider>().enabled = 
            !GameObject.Find("RCone").GetComponent<MeshCollider>().enabled;

        GlobalVariables.Beam = !GlobalVariables.Beam;
    }


    public void ReColor(string type)
    {


    }

    public void Cluster()
    {
        Clustering.DBScan();
    }


        public void EnableEdgeType(string type)
    {
        Debug.Log(type);
        var graph = GlobalVariables.Graph;
        var edges = graph.edges;
        var mesh = graph.edgeObject.GetComponent<MeshFilter>().mesh;
        Color[] vertexColors = mesh.colors;


        for (int i = 0; i < edges.Count; i++)
        {

            if (edges[i].style == type)
            {
                if (edges[i].active)
                {
                    edges[i].active = false;
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] =
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = new Color(0, 0, 0, 0);
                }
                else
                {
                    edges[i].active = true;
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = TGraph.ReadJSON.GenerateOriginColor(graph.colorDict[edges[i].style]);
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = TGraph.ReadJSON.GenerateTargetColor(graph.colorDict[edges[i].style]);
                }
            }

        }


        mesh.colors = vertexColors;


    }


    public static void SEnableEdgeType(string type)
    {
        Debug.Log(type);
        var graph = GlobalVariables.Graph;
        var edges = graph.edges;
        var mesh = graph.edgeObject.GetComponent<MeshFilter>().mesh;
        Color[] vertexColors = mesh.colors;
        
       
            for (int i = 0; i < edges.Count; i++)
            {

                if (edges[i].style == type)
                {
                    if (edges[i].active)
                    {
                        edges[i].active = false;
                        vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] =
                        vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = new Color(0, 0, 0, 0);
                    }
                    else
                    {
                        edges[i].active = true;
                        vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = TGraph.ReadJSON.GenerateOriginColor(graph.colorDict[edges[i].style]);
                        vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = TGraph.ReadJSON.GenerateTargetColor(graph.colorDict[edges[i].style]);
                    }
                }
                   
            }
      

        mesh.colors = vertexColors;
        

    }




}
