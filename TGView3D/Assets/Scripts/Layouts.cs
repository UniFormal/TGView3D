using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TGraph
{
    public static class Layouts
    {



        static int success = 0;
        static bool flat = false;
        static float sliceWidth = 0;
        public static ReadJSON.MyGraph graph;
        static float energy = 1000000f;
        static float step = 30f;// initialStep;

        public static void Init()
        {
           graph = GlobalVariables.Graph;
        }



        public static void Spiral(){
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                float angle = (float)i*10.0f / graph.nodes.Count*2*Mathf.PI ;
               // Debug.Log(angle);
                Vector3 pos = graph.nodes.Count*(float)i/(float)graph.nodes.Count/10*new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
                graph.nodes[i].pos = pos;
                graph.nodes[i].nodeObject.transform.localPosition = pos;
            }

        }
        public static void BaseLayout(int iterations, float globalWeight, float spaceScale)
        {
            Init();
            Spiral();
            BuildHierarchy();
            SolveUsingForces(iterations, 1.13f, useWeights: true, globalWeight: globalWeight);
            Normalize(spaceScale);
        }

        public static void Normalize(float spaceScale)
        {
            Vector3 maxVec = Vector3.one * float.MinValue;
            Vector3 minVec = Vector3.one * float.MaxValue;

            Vector3 avgPos = Vector3.zero;
            foreach (ReadJSON.MyNode node in graph.nodes)
            {
                avgPos += node.pos;
            }
            avgPos /= graph.nodes.Count;
            foreach (ReadJSON.MyNode node in graph.nodes)
            {
                node.pos -= avgPos;
            }

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];
                maxVec = Vector3.Max(node.pos, maxVec);
                minVec = Vector3.Min(node.pos, minVec);
            }

            Vector3 scaleVec = (maxVec - minVec);
            Debug.Log("beforeScale:"+ maxVec+ " "+ minVec);

            if (scaleVec.y == 0) scaleVec.y++;
            Vector3 realScale = spaceScale * Vector3.Scale(new Vector3(1f / scaleVec.x, .5f / scaleVec.y, 1f / scaleVec.z),
                Vector3.one * (Mathf.Pow(graph.nodes.Count, 1f / 2f)));


            Debug.Log("scale: " + realScale);

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];

                Vector3 pos = Vector3.Scale(node.pos, realScale);
                node.pos = pos;
                node.nodeObject.transform.localPosition = pos;
            }
            avgPos = Vector3.zero;
    

        }

        public static void BuildHierarchy()
        {
            float maxHeight = -10;
            float minHeight = 10;
            Color[] vertices;
            if (graph.edgeObject == null)
            {
                vertices = new Color[graph.edges.Count * 8];
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = Color.red;
                }
            }
            else
                vertices = graph.edgeObject.GetComponent<MeshFilter>().mesh.colors;

            for (int n = 0; n < 1; n++)
            {
                List<int> rootIndices = new List<int>();
                bool[] visited = new bool[graph.nodes.Count];
                float maxConnections = 0;
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    graph.nodes[i].weight = graph.nodes[i].height = 0;
                    if (n == 0)
                    {
                        if (graph.nodes[i].edgeIndicesOut.Count == 0) rootIndices.Add(i);
                    }
                    else
                    {
                        if (graph.nodes[i].edgeIndicesIn.Count == 0) rootIndices.Add(i);
                    }
                    maxConnections = Mathf.Max(graph.nodes[i].connectedNodes.Count, maxConnections);

                }

                Debug.Log("#root nodes: " + rootIndices.Count);


                for (int i = 0; i < rootIndices.Count; i++)
                {
                    for (int j = 0; j < graph.nodes.Count; j++)
                    {
                        visited[j] = false;
                    }
                    float curHeight = 1;
                    Stack<int> nodeIndexStack = new Stack<int>();
                    nodeIndexStack.Push(rootIndices[i]);
                    Stack<float> heightStack = new Stack<float>();
                    heightStack.Push(curHeight);
                    visited[rootIndices[i]] = true;

                    while (nodeIndexStack.Count > 0)
                    {
                        int curIndex = nodeIndexStack.Pop();
                        curHeight = heightStack.Pop();
                        maxHeight = Mathf.Max(curHeight, maxHeight);
                        minHeight = Mathf.Min(curHeight, minHeight);

                        ReadJSON.MyNode curNode = graph.nodes[curIndex];

                        int childCount;
                        if (n == 0) childCount = curNode.edgeIndicesIn.Count;
                        else childCount = curNode.edgeIndicesOut.Count;

                        int edgeIndex;
                            
                        for (int j = 0; j < childCount; j++)
                        {
                            if (n == 0)
                                edgeIndex = curNode.edgeIndicesIn[j];
                            else
                                edgeIndex = curNode.edgeIndicesOut[j];

                            if (vertices[edgeIndex * 8].a == 0) continue;

                            int childNodeIndex;
                            if (n == 0)
                            {
                                childNodeIndex = graph.nodeDict[graph.edges[curNode.edgeIndicesIn[j]].from];
                                // graph.nodes[childNodeIndex].height -=curHeight;
                                graph.nodes[childNodeIndex].weight = Mathf.Max(curHeight, graph.nodes[childNodeIndex].weight);//+= curHeight;
                              


                            }
                            else
                            {
                                childNodeIndex = graph.nodeDict[graph.edges[curNode.edgeIndicesOut[j]].to];
                                graph.nodes[childNodeIndex].height = Mathf.Max(curHeight, graph.nodes[childNodeIndex].height);//+= curHeight;
                              
                            }


                            if (!visited[childNodeIndex])
                            {
                                visited[childNodeIndex] = true;
                                nodeIndexStack.Push(childNodeIndex);
                                if (n == 0) heightStack.Push(curHeight + curNode.inWeights[j]);
                                else heightStack.Push(curHeight + curNode.outWeights[j]);
                            }
                        }

                    }
                }
            }

            sliceWidth = 0.2f;
           // sliceWidth = Mathf.Max(0.1f, 5.0f * maxHeight / Mathf.Sqrt(graph.nodes.Count));

          
            //Debug.Log("maxHeight: " + maxHeight);


            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];

                //Debug.Log(node.label + " " + node.height);
                //var y = (node.height - 0.8f * node.weight) * sliceWidth;
                

                var y = node.weight + node.height;
               // Debug.Log(i + " weight: " + node.weight + " height: " + node.height+" y " + y * Mathf.Max(1, (graph.nodes.Count / 200.0f)));
                /*  float x =(maxConnections/10- node.connectedNodes.Count)*20 ;
                  if (x < -vol) x = 0;

                  if (i % 2 == 0) x *= -1;*/

                // y = Mathf.Sign(y) * (Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(y * graph.nodes.Count / 100))));

                Vector3 pos = new Vector3(node.pos.x, y * Mathf.Max(1, (graph.nodes.Count / 200.0f)), node.pos.z) / Mathf.Max(1, (graph.nodes.Count / 200.0f));



                node.pos = pos;
                node.nodeObject.transform.localPosition = pos;



            }
        }


        // Function to calculate forces driven layout
        // ported from Marcel's Javascript versions
        public static 
            //IEnumerator 
            void
            SolveUsingForces(int iterations, float spacingValue, bool resetForcesFixed = false, bool usingMinMax = false, float currTemperature = 0.9f, float initialStep = 3.0f, float globalWeight = 1.0f, bool useWeights=false, bool reset = false)
        {
            /*
            if (resetForcesFixed == true)
            {
                for (var j = 0; j < graph.nodes.Count; j++)
                {
                    var n = graph.nodes[j];
                    n.forcesFixed = false;
                }
 a           }*/
            Color[] vertices;
            if (graph.edgeObject == null)
            {
                vertices = new Color[graph.edges.Count * 8];
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = Color.red;
                }
            }
            else
                vertices = graph.edgeObject.GetComponent<MeshFilter>().mesh.colors;


           // if (reset)
            {
                energy = 1000000f;
                step = 30f;// initialStep;
            }



            for (var i = 0; i < iterations; i++)
            {
               /* if (i % 100 == 0)
                {
                    Debug.Log("Beautify Layout: Iteration " + i + " of " + iterations + "...");
                }*/

                float kVal = Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 30), 0.1f);
                float kSquared = kVal * kVal;

                var energyBefore = energy;
                energy = 0;
                for (var j = 0; j < graph.nodes.Count; j++)
                {
      
                    var n = graph.nodes[j];
                   // if (!n.forcesFixed) //&& !n.hidden)
                    {
                        n.disp.x = 0;
                        n.disp.y = 0;
                        n.disp.z = 0;

                        // calculate global (repulsive) forces
                        for (var k = 0; k < graph.nodes.Count; k++)
                        {
                            var u = graph.nodes[k];
                            if (u.graphNumber == n.graphNumber && n != u && (u.edgeIndicesIn != null || u.edgeIndicesOut != null))
                            {
                                var differenceNodesX = u.pos.x - n.pos.x;
                                var differenceNodesY =  u.pos.y - n.pos.y;
                                var differenceNodesZ = u.pos.z - n.pos.z;

                                //var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY) + 0.001;

                                var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX +
                                   differenceNodesY * differenceNodesY +
                                    differenceNodesZ * differenceNodesZ) + 0.0001f;
                                var repulsiveForce = -(kSquared / lengthDiff);

                                n.disp.x += (differenceNodesX / lengthDiff) * repulsiveForce;
                                n.disp.y += (differenceNodesY / lengthDiff) * repulsiveForce;
                                n.disp.z += (differenceNodesZ / lengthDiff) * repulsiveForce;
                            }
                        }

                        // calculate local (spring) forces
                        List<int> edgeIndices = n.edgeIndicesIn.Concat<int>(n.edgeIndicesOut).ToList<int>();
                        for (var k = 0; k < n.connectedNodes.Count; k++)
                        {
                           // Debug.Log(vertices.Length + " " + edgeIndices[k] * 8 + " " + edgeIndices.Count + " " + n.connectedNodes.Count + " " + k+" "+vertices[edgeIndices[k] * 8].a);
                            if (vertices[edgeIndices[k] * 8].a == 0) continue;
                            var u = graph.nodes[n.connectedNodes[k]];
                            var differenceNodesX = u.pos.x - n.pos.x;
                            var differenceNodesY = 0;// u.pos.y - n.pos.y;
                            var differenceNodesZ = u.pos.z - n.pos.z;

                            var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX +
                                differenceNodesY * differenceNodesY +
                                differenceNodesZ * differenceNodesZ) + 0.0001f;
                            var attractiveForce = (lengthDiff * lengthDiff / kVal);
                            if (useWeights)
                            {
                                if(n.weights[k]<=.9f) attractiveForce *= n.weights[k]* globalWeight;
                                if (n.weights[k] <= .1f) attractiveForce =0;
                            }
                            n.disp.x += (differenceNodesX / lengthDiff) * attractiveForce;
                            n.disp.y += (differenceNodesY / lengthDiff) * attractiveForce;
                            n.disp.z += (differenceNodesZ / lengthDiff) * attractiveForce;
                        }




                        // Limit max displacement to temperature currTemperature

                        var dispLength = Mathf.Sqrt(n.disp.x * n.disp.x + n.disp.y * n.disp.y + n.disp.z * n.disp.z) + 0.0001f;
                        n.pos.x = ((n.pos.x + (n.disp.x / dispLength) * step));
                        n.pos.y = ((n.pos.y + 0.01f* (n.disp.y / dispLength) * step));
                        n.pos.z = ((n.pos.z + (n.disp.z / dispLength) * step));

    
                        energy += dispLength * dispLength;
                    }
                    n.nodeObject.transform.localPosition = n.pos;



                }
                // Reduce the temperature as the layout approaches a better configuration

                if (energy < energyBefore)
                {
                    success++;
                    if (success >= 5)
                    {
                        success = 0;
                        step /= currTemperature;
                    }
                }
                else
                {
                    success = 0;
                    step *= currTemperature;
                }
              //  yield return null;
            }

       
        }
    }
}
