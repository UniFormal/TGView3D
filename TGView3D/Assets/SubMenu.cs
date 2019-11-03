using System.Collections;
using System.Collections.Generic;
using TGraph;
using UnityEngine;
using UnityEngine.UI;
using static TGraph.ReadJSON;

public class SubMenu : MonoBehaviour
{

    public GameObject FieldInput;
    public TGraph.GraphManager Gm;
    public int ID = 0;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        if (Gm == null) Gm = TGraph.GlobalVariables.GraphManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartEdge()
    {
        Debug.Log("startedge");
        string startId = TGraph.GlobalVariables.Graph.nodes[transform.parent.GetSiblingIndex()].id;
        Camera.main.GetComponent<FlyCamera>().Startid = startId;
    }

    public void UpdateNodeField(GameObject field)
    {
        var node = Gm.Graph.nodes[transform.parent.GetSiblingIndex()];
     
        var fieldType = field.GetComponent<Text>().text;
        Debug.Log("handling " + node.label+ " " + fieldType + " " + node.GetType().GetField(fieldType) +" " + field.GetComponentInChildren<InputField>().text);

        if (node.GetType().GetField(fieldType).GetValue(node).ToString() != field.GetComponentInChildren<InputField>().text)
        {
            node.GetType().GetField(fieldType).SetValue(node, field.GetComponentInChildren<InputField>().text);
        }

        GlobalVariables.JsonManager.UpdateJSON();

    }

    public void UpdateEdgeField(GameObject field)
    {
        var edge = Gm.Graph.edges[ID];

        var fieldType = field.GetComponent<Text>().text;
        Debug.Log("handling " + edge.label + " " + fieldType + " " + edge.GetType().GetField(fieldType) + " " + field.GetComponentInChildren<InputField>().text);

        if (edge.GetType().GetField(fieldType).GetValue(edge).ToString() != field.GetComponentInChildren<InputField>().text)
        {
            edge.GetType().GetField(fieldType).SetValue(edge, field.GetComponentInChildren<InputField>().text);
        }

        GlobalVariables.JsonManager.UpdateJSON();

    }

    public void RemoveEdge()
    {
        var edge = Gm.Graph.edges[ID];
        graph.edges.Remove(edge);
        var json = JsonUtility.ToJson(graph);

        CurrentJSON = json;
        GlobalVariables.JsonManager.UpdateLayout();
    }


    public void RemoveNode()
    {
        //TODO: fix
        GlobalVariables.JsonManager.UpdateLayout();
        MyNode pNode = graph.nodes[transform.parent.GetSiblingIndex()];

        pNode.nodeObject.SetActive(false);

        GameObject.Destroy(pNode.nodeEdgeObject);

        MyGraph tmpGraph = graph;// MyGraph.CreateFromJSON(JsonUtility.ToJson(graph));

         MyNode node = graph.nodes[transform.parent.GetSiblingIndex()];

        List<MyEdge> edges = new List<MyEdge>();
        
        foreach (var edgeidx in pNode.edgeIndicesIn)
        {
            Debug.LogWarning(edgeidx+ " "+tmpGraph.edges.Count);

            var edge = tmpGraph.edges[edgeidx];
            edges.Add(edge);
         
            GlobalVariables.GraphManager.BlendEdge(pNode,graph.nodes[graph.nodeDict[edge.from]]);
            

        }
        foreach (var edgeidx in pNode.edgeIndicesOut)
        {
            var edge = tmpGraph.edges[edgeidx];
            edges.Add(edge);
            GlobalVariables.GraphManager.BlendEdge(pNode, graph.nodes[graph.nodeDict[edge.to]]);
           
        }
        foreach(var edge in edges)
        {
            graph.edges.Remove(edge);
        }


        graph.nodes.Remove(node);
        var json = JsonUtility.ToJson(graph);


        CurrentJSON = json;
        GlobalVariables.JsonManager.UpdateLayout();



    }



}
