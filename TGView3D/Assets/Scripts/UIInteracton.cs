﻿using OVRTouchSample;
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
    public TextAsset[] GraphFiles;
    public ColorPicker ColorPicker;
    public UnityEngine.UI.Dropdown Left;
    public UnityEngine.UI.Dropdown Right;
    public Toggle CustomToggle;
  

    void Start()
    {
     
        //if(ReadJSON.CurrentJSON == null)
            ReadJSON.CurrentJSON = GraphFiles[GameObject.Find("UIDropdown").GetComponent<Dropdown>().value].text;
        GlobalVariables.UIInteractonManager =  GetComponent<UIInteracton>();
    }

    public void Init()
    {
        ChainAttribute();
        ChangeAttribute();
        ChangeColor();
    }
 


    public void EnableDesktop()
    {
       Desktop.SetActive(true);
    }


    public void ChainAttribute()
    {
        var left = Left;
        CustomToggle.isOn = ReadJSON.EdgeTypes[left.options[left.value].text].active;
       
        Dropdown right = Right;
        right.value = right.options.FindIndex((i) => { return i.text.Equals(ReadJSON.EdgeTypes[left.options[left.value].text].type); }); 
        var col = TGraph.ReadJSON.ColorDict[left.options[left.value].text] / 255f;
        col.a = col.a*255f/4;
        ColorPicker.CurrentColor = col;
   
    }

   
  /*  public void MetaConsistency(Toggle md)
    {
        if(Left.options[Left.value].text == "meta")
        {
            CustomToggle.isOn = md.isOn;
        }


    }

    public void MetaConsistency2(Toggle md)
    {
        if (Left.options[Left.value].text == "meta")
        {
            md.isOn = CustomToggle.isOn;
        }
    }
    */

    public void ChangeAttribute()
    {
        var left = Left;
        Dropdown right = Right;
        ReadJSON.EdgeTypes[left.options[left.value].text].type = right.options[right.value].text;
    }

    public void ChangeColor()
    {
        if (GlobalVariables.Graph != null)
        {
            var cp = ColorPicker;
            var type = Left.options[Left.value].text;
            TGraph.ReadJSON.ColorDict[type] = new Color (cp.CurrentColor.r * 255 ,cp.CurrentColor.g * 255, cp.CurrentColor.b * 255 ,cp.CurrentColor.a*4) ;

            // this.EnableEdgeType(type); this.EnableEdgeType(type);
            
            ReColor();

        }

    }



    public void Switch2D()
    {
        Debug.Log(GlobalVariables.Init);
        GlobalVariables.TwoD = !GlobalVariables.TwoD;
        if (GlobalVariables.Init&&GlobalVariables.Graph.nodes.Count>0) { 
            Layouts.Init(GlobalVariables.TwoD);
    
                GlobalVariables.JsonManager.ResetLayout();
        }
    }

    public void SetUrlMode(Dropdown d)
    {
        GlobalVariables.UrlMode = d.value;
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

    public void ChangeJSON(Dropdown d)
    {
        //  GlobalVariables.Url = "file:///" + Application.dataPath + "/" + d.captionText.text + ".json";
        //GlobalVariables.SelectionIndex = d.value;
        //GlobalVariables.Url = "";

        ReadJSON.CurrentJSON = GraphFiles[d.value].text;
        Debug.Log("select "+d.value);

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
        var targetUI = UIOverlay;
        if (targetUI.GetComponent<RectTransform>().localScale.x == 1)
            targetUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        else
            targetUI.GetComponent<RectTransform>().localScale = Vector3.one;
    }
    public void EnableUI(GameObject targetUI)
    {
        if (targetUI == null) targetUI = UIOverlay;
        if (targetUI.GetComponent<Canvas>().enabled)
            targetUI.GetComponent<Canvas>().enabled = false;
        else
            targetUI.GetComponent<Canvas>().enabled = true;
    }

    public void EnableUIScale(GameObject targetUI)
    {
        if (targetUI == null) targetUI = UIOverlay;
        if (targetUI.GetComponent<RectTransform>().localScale.x == 1)
            targetUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        else
            targetUI.GetComponent<RectTransform>().localScale = Vector3.one;
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


    public static void ReColor()
    {
    
            var graph = GlobalVariables.Graph;
            var edges = graph.edges;
            var mesh = graph.edgeObject.GetComponent<MeshFilter>().mesh;
            Color[] vertexColors = mesh.colors;


            for (int i = 0; i < edges.Count; i++)
            {

           
                    if (!edges[i].active)
                    {
                       
                        vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] =
                        vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = new Color(0, 0, 0, 0);
                    }
                    else
                    {
                  
                        vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = TGraph.GraphManager.GenerateOriginColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                        vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = TGraph.GraphManager.GenerateTargetColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                    }
                

            }


            mesh.colors = vertexColors;


        

    }

    public void Cluster()
    {
        Clustering.DBScan();
    }


   public void EnableEdgeType(string type)
    {
        if (type == "")
            type = Left.options[Left.value].text;

        if (ReadJSON.EdgeTypes[type].active != CustomToggle.isOn)
            SEnableEdgeType(type);
        /*
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
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = TGraph.GraphManager.GenerateOriginColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = TGraph.GraphManager.GenerateTargetColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                }
            }

        }


        mesh.colors = vertexColors;*/


    }



    public static void SEnableEdgeType(string type)
    {

    
       if(ReadJSON.EdgeTypes.ContainsKey(type))
            ReadJSON.EdgeTypes[type].active = !ReadJSON.EdgeTypes[type].active;



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
                        vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = TGraph.GraphManager.GenerateOriginColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                        vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = TGraph.GraphManager.GenerateTargetColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                    }
                }
                   
            }
      

        mesh.colors = vertexColors;
        

    }






}
