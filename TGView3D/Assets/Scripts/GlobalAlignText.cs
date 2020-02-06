using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAlignText : MonoBehaviour {

    // Use this for initialization

    public int childCount;
    public Transform CamTransform;
    void Start()
    {
      
        childCount = TGraph.GlobalVariables.Graph.nodes.Count ;
        CamTransform = Camera.main.transform;
    }


    // Update is called once per frame
    void Update () {


    //s    Debug.LogWarning(TGraph.GlobalVariables.Init);
      //  if (TGraph.GlobalVariables.Init)
        {

            Vector3 camPos = CamTransform.position;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = this.transform.GetChild(i);//.GetChild(0);
                // t.rotation = Quaternion.LookRotation(t.position - camPos);
                t.forward = CamTransform.forward;
            }

        }


    }
}
