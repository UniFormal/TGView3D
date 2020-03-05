using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterText : MonoBehaviour
{
    // Start is called before the first frame update

    public List<TGraph.ReadJSON.MyNode> Nodes;
    Transform CamTransform;
    Renderer R;

     void Start()
    {
        R = GetComponent<Renderer>();
        CamTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        var avgPos = Vector3.zero;
        foreach(var node in Nodes)
        {
            avgPos += node.nodeObject.transform.position;
        }
        if(Nodes.Count>0)transform.position = avgPos / Nodes.Count;

        if ((CamTransform.position - transform.position).magnitude < 4) R.enabled = false;
        else R.enabled = true;
    }
}
