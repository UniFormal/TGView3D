using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private void OnDisable()
    {
        OVRTouchSample.DistanceGrabber.OnSelectionChanged -= UpdateText;
    }


    private void UpdateText()
    {
        var node = TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection];
        Origin.text = node.label;
        List <int> edgeIndices = node.edgeIndicesIn.Concat<int>(node.edgeIndicesOut).ToList<int>();

        if (TGraph.GlobalVariables.Graph.currentTarget >= 0 && TGraph.GlobalVariables.Graph.currentTarget != node.connectedNodes.Count)
        {
            Target.text = TGraph.GlobalVariables.Graph.nodes[node.connectedNodes[TGraph.GlobalVariables.Graph.currentTarget]].label;
            EdgeInfo.text = TGraph.GlobalVariables.Graph.edges[edgeIndices[TGraph.GlobalVariables.Graph.currentTarget]].label;
          //  Debug.Log(edgeIndices[TGraph.GlobalVariables.Graph.currentTarget]);
        }

        else
        {
            Target.text = EdgeInfo.text= "-";
        }
            
        // Debug.Log(TGraph.GlobalVariables.Graph.edges[TGraph.GlobalVariables.Graph.nodes[TGraph.GlobalVariables.Graph.latestSelection].edgeIndicesOut[0]]);
    }

}
