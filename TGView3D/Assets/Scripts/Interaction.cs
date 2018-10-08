using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TGraph
{
    public class Interaction : MonoBehaviour
    {

        // Use this for initialization
        private ReadJSON.MyGraph graph;

        private void Start()
        {
            graph = GlobalVariables.Graph;
        }

        private void UpdatePosition()
        {
            foreach (int n in graph.selectedNodes)
            {
                if (n < 0) continue;
                var node = graph.nodes[n];

                List<int> edgeIndices = node.edgeIndicesIn.Union<int>(node.edgeIndicesOut).ToList<int>();

                Mesh mesh = node.nodeEdgeObject.GetComponent<MeshFilter>().sharedMesh;
                Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
                Vector3[] vertices = mesh.vertices;
                Vector3[] bigVertices = bigMesh.vertices;

                for (int i = 0; i < edgeIndices.Count; i++)
                {
                    var sourcePos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].from]].nodeObject.transform.localPosition;
                    var targetPos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].to]].nodeObject.transform.localPosition;

                    //if (sourcePos != graph.nodes[graph.nodeDict[edges[i].from]].pos || targetPos != graph.nodes[graph.nodeDict[edges[i].to]].pos)
                    {
                        // Debug.Log("work");
                        Vector3 dir = targetPos - sourcePos;
                        Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                        Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                        ReadJSON.createEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);
                        ReadJSON.createEdge(edgeIndices[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                        //changed = true;
                    }

                }
                //if (changed)
                {
                    mesh.vertices = vertices;
                    bigMesh.vertices = bigVertices;
                    mesh.RecalculateBounds();
                }
            }
        }
    

        // Update is called once per frame
        void Update()
        {

            if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two))
            {
                if (Camera.main.farClipPlane == 12) Camera.main.farClipPlane = 100;
                else Camera.main.farClipPlane = 12;
            }
            if (OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four))
            {
                graph.edgeObject.SetActive(!graph.edgeObject.activeSelf);
            }
           // if(Input.GetKeyDown(KeyCode.Space)) Layouts.SolveUsingForces(1, 0.13f);

            UpdatePosition();
           

        }
    }

    
}
