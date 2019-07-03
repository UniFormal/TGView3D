using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAlignText : MonoBehaviour {

    // Use this for initialization

    public int childCount;
    void Start()
    {
      
        childCount = TGraph.GlobalVariables.Graph.nodes.Count ;
        
    }


    // Update is called once per frame
    void Update () {


        Debug.LogWarning(TGraph.GlobalVariables.Init);
        if (TGraph.GlobalVariables.Init)
        {

            Vector3 camPos = Camera.main.transform.position;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = this.transform.GetChild(i).GetChild(0);
                t.rotation = Quaternion.LookRotation(t.position - camPos);
            }

        }


    }
}
