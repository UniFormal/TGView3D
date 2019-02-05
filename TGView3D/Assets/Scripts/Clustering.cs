using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGraph
{
    public static class Clustering{

       
        public static float Epsilon = 0;

        public static void DBScan()
        {

            GameObject edgeObject = GlobalVariables.Graph.edgeObject;
            Mesh mesh = edgeObject.GetComponent<MeshFilter>().sharedMesh;
            var vertexColors = mesh.colors;

            List<ReadJSON.MyNode> nodes = GlobalVariables.Graph.nodes;
            var edges = GlobalVariables.Graph.edges;
            List<Color> colors = new List<Color>();

            float avgDist = 0;
            foreach(var node in nodes)
            {
                avgDist += (node.pos - nodes[Random.Range(0, nodes.Count)].pos).magnitude;
                avgDist += (node.pos - nodes[Random.Range(0, nodes.Count)].pos).magnitude;
                avgDist += (node.pos - nodes[Random.Range(0, nodes.Count)].pos).magnitude;
            }
            avgDist /= (3*nodes.Count);

            Epsilon = 0.2f * avgDist;

            var nodestack = new Stack<ReadJSON.MyNode>();


            int clusterId = 0;
            colors.Add(Random.ColorHSV(.0f, 1, .1f, 1, .1f, 1, 1, 1));
            foreach (var startNode in nodes)
            {
                if (startNode.ClusterId == -1)
                {
                    nodestack.Push(startNode);
                    while (nodestack.Count > 0)
                    {
                        var node = nodestack.Pop();
                        node.ClusterId = clusterId;

                        foreach (var otherNodeIdx in node.connectedNodes)
                        {
                            var otherNode = nodes[otherNodeIdx];
                            if (otherNode.ClusterId==-1&&(node.pos - otherNode.pos).magnitude < Epsilon)
                            {
                                nodestack.Push(otherNode);
                            }
                        }
                    }
                    colors.Add(Random.ColorHSV(.0f, 1, .1f, 1, .1f, 1, 1, 1));
                    clusterId++;
                }
            }
            Debug.Log(clusterId+" clusters");

            for (int i = 0; i < edges.Count; ++i)
            {
                if (edges[i].active)
                {
                    var col = colors[nodes[GlobalVariables.Graph.nodeDict[edges[i].to]].ClusterId] * 255;
                    col.a = 1;
                   // Debug.Log(col);
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] =
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = col;
                }
        
            }
            mesh.colors = vertexColors;





            foreach(var node in nodes)
            {
                var col = colors[node.ClusterId];

                node.nodeObject.GetComponent<MeshRenderer>().material.color = col;
            }

        }

    }
}
