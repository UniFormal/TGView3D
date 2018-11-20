using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gestures : MonoBehaviour {


    Vector3 LeftStart = Vector3.zero;
    Vector3 RightStart = Vector3.zero;
    Transform LeftHand;
    Transform RightHand;
    Vector3 AvgStart = Vector3.zero;
    float StartDist;
    public GameObject GraphParent;
    Vector3 CurScale;
    Vector3 CurPos;
    Vector3 CurRot;
    bool Manipulated = false;
    float CurDist = 0;
    int j = 0;
    int f = 0;
    int e = 0;
    Transform NodeParent;
    TGraph.ReadJSON.MyGraph graph;
    int vertexCount = 0;
    int nodeCount = 0;
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] vertexCopies;

    float StartAngle;

    // Update is called once per frame
    float factor;


    private void Rescale()
    {

        CurDist = Vector3.Magnitude(LeftHand.position - RightHand.position);
        factor = CurDist / StartDist;
        for (int i = 0; i < nodeCount; ++i)
        {
            var node = graph.nodes[j];
            graph.nodes[j].nodeObject.transform.localPosition = factor * node.pos;
            j = (j + 1) % nodeCount;
        }
        for (int k = 0; k < vertexCount / 8; ++k)
        {
            Vector3 offset = -.5f * (1 - factor) * (vertexCopies[4 + e] - vertexCopies[0 + e]);
            Vector3 offsetOrtho = -.5f * (1 - factor) * (vertexCopies[2 + e] - vertexCopies[0 + e]);

            vertices[0 + e] = vertexCopies[0 + e] * factor + offset + offsetOrtho;
            vertices[1 + e] = vertexCopies[1 + e] * factor + offset + offsetOrtho;

            vertices[2 + e] = vertexCopies[2 + e] * factor + offset - offsetOrtho;
            vertices[3 + e] = vertexCopies[3 + e] * factor + offset - offsetOrtho;

            vertices[4 + e] = vertexCopies[4 + e] * factor - offset + offsetOrtho;
            vertices[5 + e] = vertexCopies[5 + e] * factor - offset + offsetOrtho;

            vertices[6 + e] = vertexCopies[6 + e] * factor - offset - offsetOrtho;
            vertices[7 + e] = vertexCopies[7 + e] * factor - offset - offsetOrtho;

            e = (e + 8) % vertexCount;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        Manipulated = true;
        TGraph.GlobalVariables.Recalculate = true;
    }

    private void Rescale(int dim)
    {

        CurDist = Vector3.Magnitude(LeftHand.position - RightHand.position);
        factor = CurDist / StartDist;
        for (int i = 0; i < nodeCount; ++i)
        {
            var node = graph.nodes[j];
            var pos = NodeParent.GetChild(j).transform.localPosition;
            pos[dim] = factor * node.pos[dim];
            NodeParent.GetChild(j).transform.localPosition = pos;
            j = (j + 1) % nodeCount;
        }
        for (int k = 0; k < vertexCount / 8; ++k)
        {
            float offset = -.5f * (1 - factor) * (vertexCopies[4 + e][dim] - vertexCopies[0 + e][dim]);
            float offsetOrtho = -.5f * (1 - factor) * (vertexCopies[2 + e][dim] - vertexCopies[0 + e][dim]);

            vertices[0 + e][dim] = vertexCopies[0 + e][dim] * factor + offset + offsetOrtho;
            vertices[1 + e][dim] = vertexCopies[1 + e][dim] * factor + offset + offsetOrtho;

            vertices[2 + e][dim] = vertexCopies[2 + e][dim] * factor + offset - offsetOrtho;
            vertices[3 + e][dim] = vertexCopies[3 + e][dim] * factor + offset - offsetOrtho;

            vertices[4 + e][dim] = vertexCopies[4 + e][dim] * factor - offset + offsetOrtho;
            vertices[5 + e][dim] = vertexCopies[5 + e][dim] * factor - offset + offsetOrtho;

            vertices[6 + e][dim] = vertexCopies[6 + e][dim] * factor - offset - offsetOrtho;
            vertices[7 + e][dim] = vertexCopies[7 + e][dim] * factor - offset - offsetOrtho;

            e = (e + 8) % vertexCount;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        Manipulated = true;
        TGraph.GlobalVariables.Recalculate = true;
    }


    void Update()
    {

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            if(TGraph.GlobalVariables.Beam)
                GameObject.Find("LCone").GetComponent<MeshRenderer>().enabled = true;
        }


        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            if (TGraph.GlobalVariables.Beam)
                GameObject.Find("RCone").GetComponent<MeshRenderer>().enabled = true;
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            LeftStart = LeftHand.position;
            AvgStart = (LeftStart + RightStart) / 2;
            StartDist = Vector3.Magnitude(LeftStart-RightStart);
            CurScale = GraphParent.transform.localScale;
            CurRot = GraphParent.transform.localEulerAngles;
            CurPos = GraphParent.transform.localPosition;
            Manipulated = false;

          

        }
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {

            //  if (graph == null)
            {
                graph = TGraph.GlobalVariables.Graph;
                nodeCount = graph.nodes.Count;
                mesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
                vertexCount = mesh.vertexCount;
                vertices = mesh.vertices;
                vertexCopies = (Vector3[])vertices.Clone();

            }
            Manipulated = false;
            CurScale = GraphParent.transform.localScale;
            CurRot = GraphParent.transform.localEulerAngles;
            CurPos = GraphParent.transform.localPosition;
            RightStart = RightHand.position;
            AvgStart = (LeftStart + RightStart) / 2;
            StartDist = Vector3.Magnitude(LeftStart - RightStart);

            var a = RightStart - GraphParent.transform.position;
            var b = RightHand.position - GraphParent.transform.position;
            StartAngle = Vector3.SignedAngle(a, b, Vector3.up);

        }
     
    
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)&& OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)){


            if (StartDist>=0.01f)
            {
                float angle = Vector3.Angle(Vector3.up, RightStart - LeftStart);
                if (angle > 90) angle = 180 - angle;
                if (angle <= 25) Rescale(1);
                else if (angle <= 65) Rescale();
                else
                {
                    Rescale(0);
                    Rescale(2);
                }
            }
        }
        
        else if (!Manipulated&&OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            GraphParent.transform.localPosition = CurPos + (LeftHand.transform.position - LeftStart) * 30;

            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
            {
                CurPos = GraphParent.transform.localPosition;
            }
        }
        else if (!Manipulated&&OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {

            var yRot = CurRot.y;


            //if (yRot < 0) yRot = 360 + yRot;


            var a = RightStart - GraphParent.transform.position;
            var b = RightHand.position - GraphParent.transform.position;
            //var perp = Vector3.Cross(a, Vector3.up);
            //b = Vector3.Project(perp, b);
            a.y = 0;
            b.y = 0;

            var newYRot = (yRot + 50*Vector3.SignedAngle(a,b,Vector3.up) )% 360;


            if (newYRot < 0) newYRot = 360 + newYRot;

            GraphParent.transform.localEulerAngles = new Vector3(0, newYRot, 0);
           // Debug.Log(CurRot.y + " new: " + newYRot+a+" "+b+" dangle "+( 50 * Vector3.SignedAngle(a, b, Vector3.up)));

            if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                CurRot = GraphParent.transform.localEulerAngles;
            }

        }

        if (Manipulated&&(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger)))
        {
          
            /*

            for (; j < nodeCount; ++j)
            {
                NodeParent.GetChild(j).transform.localPosition = factor * graph.nodes[j].pos;
            }
            for (; e < vertexCount; ++e)
            {
                vertices[e] = vertexCopies[e] * factor;
            }
            j = e = 0;
            */
            vertexCopies = (Vector3[])vertices.Clone();

            for (int i = 0; i < nodeCount; ++i)
            {
                graph.nodes[i].pos = graph.nodes[i].nodeObject.transform.localPosition;
            }

            
        }




    }



    // Use this for initialization
    void Start()
    {
        RightHand = transform.GetChild(0);
        LeftHand = transform.GetChild(1);
        NodeParent = GraphParent.transform.GetChild(0);
    }
}
