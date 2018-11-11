using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUpdater : MonoBehaviour {

    Text Origin;
    Text Target;
    Text EdgeInfo;
	// Use this for initialization
	void Start () {
        Text[] texts = GetComponentsInChildren<Text>();
        Origin = texts[0];
        Target = texts[1];
        EdgeInfo = texts[2];
        OVRTouchSample.DistanceGrabber.OnSelectionChanged += UpdateText;
	}
	
	// Update is called once per frame



    private void UpdateText()
    {
        Origin.text = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].label;
        if (TGraph.GlobalVariables.Graph.currentTarget >= 0 && TGraph.GlobalVariables.Graph.currentTarget != TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].connectedNodes.Count)
            Target.text = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].connectedNodes[TGraph.GlobalVariables.Graph.currentTarget]].label;
        else
            Target.text = "-";
       // Debug.Log(TGraph.GlobalVariables.Graph.edges[TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].edgeIndicesOut[0]]);
    }

}
