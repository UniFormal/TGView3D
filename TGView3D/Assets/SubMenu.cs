using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{

    public GameObject FieldInput;
    public TGraph.GraphManager Gm;
    

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

    public void UpdateField(GameObject field)
    {
        var node = Gm.Graph.nodes[transform.parent.GetSiblingIndex()];
     
        var fieldType = field.GetComponent<Text>().text;
        Debug.Log("handling " + node.label+ " " + fieldType + " " + node.GetType().GetField(fieldType) +" " + field.GetComponentInChildren<InputField>().text);

        if (node.GetType().GetField(fieldType).GetValue(node).ToString() != field.GetComponentInChildren<InputField>().text)
        {
            node.GetType().GetField(fieldType).SetValue(node, field.GetComponentInChildren<InputField>().text);


           
        }

    }


}
