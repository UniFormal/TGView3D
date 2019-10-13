using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
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

}
