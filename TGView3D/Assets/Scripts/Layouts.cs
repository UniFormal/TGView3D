using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace TGraph
{
    public static class Layouts
    {
        

       // static bool flat = false;
       // static float sliceWidth = 0;
        public static ReadJSON.MyGraph graph;
        static float energy;
        static float energyBefore;
        static int success;
        static float step;// initialStep;
        static float currTemperature;
        //   static float maxHeight = -10;
        //   static float minHeight = 10;

        static float epsilon = 0.00001f;
        static float diameter = 1f;

        public static void Init()
        {
            graph = GlobalVariables.Graph;
        }


        public static void Spiral()
        {
            for (var i = 0; i < graph.nodes.Count; i++)
            {

                var y = (float)(10 * i) % graph.nodes.Count() * 10.0f / graph.nodes.Count * 100 - 0;
                y = 0;
              //  var y= graph.nodes[i].nodeObject.transform.localPosition.y;
                if (graph.FlatInit) y = 0;
                float angle = (float)(10*i)%graph.nodes.Count() * 10.0f / graph.nodes.Count * 2 * Mathf.PI;
                // Debug.Log(angle);
                Vector3 pos = graph.nodes.Count * (float)i / (float)graph.nodes.Count / 10 * new Vector3(Mathf.Sin(angle), y, Mathf.Cos(angle));
                graph.nodes[i].pos = pos;
                graph.nodes[i].nodeObject.transform.localPosition = pos;
            }

        }

        public static JobHandle ActivateForces( float globalWeight, int iterations,NativeArray<float> Energies, JobHandle hHandle)
        {
            currTemperature = 0.95f;
            energyBefore = 0;
            energy = 1000000f;
            step = 10.0f;// initialStep;
            success = 0;
            var jt = new JobTest(iterations,   0.0213f, Energies, useWeights: true, globalWeight: globalWeight);
            var updateEnergies = new UpdateEnergies(Energies);
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(iterations * 2, Allocator.Persistent);

            //var handle = jt.Schedule();

            int batchCount = graph.nodes.Count / 256;// Mathf.Max(iterations * graph.nodes.Count / (100 * 100), 1);
            var handle = jt.Schedule(
                graph.nodes.Count,batchCount,
                hHandle);
            for (int i = 1; i < iterations * 2; i += 2)
            {
                //handle.Complete();


                handles[i - 1] = handle;
                handle = jt.Schedule(
                    graph.nodes.Count,  batchCount,
                    handles[i - 1]);
                handles[i] = handle;
                handle = updateEnergies.Schedule(handles[i]);


                // Reduce the temperature as the layout approaches a better configuration
            }

            JobHandle.ScheduleBatchedJobs();

            // handle.Complete();
            handles.Dispose();
            //Energies.Dispose();
            //handle.Complete();
            //Normalize(spaceScale); //done outside
            return handle;
        }


        public static JobHandle BaseLayout(int iterations, float globalWeight, float spaceScale, NativeArray<float> Energies)
        {
      
            Init();
            Spiral();

            var bH = new BuildHierarchy();
            var handle = bH.Schedule();
            return ActivateForces(globalWeight, iterations, Energies, handle);
     
        }


        public static JobHandle UpdateLayout(int iterations, float globalWeight, float spaceScale, NativeArray<float> Energies)
        {

            var bH = new BuildHierarchy();
            var handle = bH.Schedule();
            return ActivateForces(globalWeight, iterations, Energies, handle);

        }

        public static void Normalize(float spaceScale, bool temp=false)
        {

            /*
                 float forceCeiling = float.MinValue;
                 float forceFloor = float.MaxValue;


                 foreach (ReadJSON.MyNode node in graph.nodes)
                 {

                     if (node.weight == -1)//||node.height==-1)
                     {
                         forceCeiling = Mathf.Max(forceCeiling, node.pos.y);
                         forceFloor = Mathf.Min(forceFloor, node.pos.y);
                     }
                 }

                foreach (ReadJSON.MyNode node in graph.nodes)
                {
                    if (node.weight == -1&&!temp) node.pos.y /= (forceCeiling - forceFloor) / maxHeight;
                }

            */

     
            if (!temp)
          {
          
                
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

                    float d = 0;
                    int hv = 0;
                Material loadedMat = new Material(graph.nodes[0].nodeObject.GetComponent<MeshRenderer>().material);
                loadedMat.color = Color.yellow;
                foreach (ReadJSON.MyNode node in graph.nodes)
                    {
                        foreach (int edge in node.edgeIndicesIn)
                        {



                            if (graph.edges[edge].style == "include")
                            {

                                d += (node.pos - graph.nodes[graph.nodeDict[graph.edges[edge].from]].pos).magnitude;
                                if (graph.nodes[graph.nodeDict[graph.edges[edge].from]].pos.y < node.pos.y)
                                {
                                //   node.nodeObject.transform.localScale = Vector3.one*.2f;

                                //  node.nodeObject.GetComponent<MeshRenderer>().material.color = Color.red;

                                //    Debug.Log(node.id +", height "+node.weight+ " from " + graph.edges[edge].from+ ", height " + graph.nodes[graph.nodeDict[graph.edges[edge].from]].weight);
                                graph.nodes[graph.nodeDict[graph.edges[edge].from]].nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;
                                node.nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;
                                hv++;
                                
                                }
                                else
                                {
                                 //   node.nodeObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
                                    //   Debug.Log("Consistent Height");
                                }
                            }

                        }
                        /*
                        foreach (int edge in node.edgeIndicesOut)
                        {
                            if (graph.edges[edge].style == "include")
                                if (graph.nodes[graph.nodeDict[graph.edges[edge].to]].pos.y > node.pos.y)
                                {
                                    Debug.Log("Height Violation2");
                                }
                                else
                                {
                                    Debug.Log("Consistent Height2");
                                }
                        }*/

            }
            Debug.Log(hv + " Height Violations, "+d/graph.nodes.Count+" average edgelength");
            }
   


            Vector3 maxVec = Vector3.one * float.MinValue;
            Vector3 minVec = Vector3.one * float.MaxValue;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                
                var node = graph.nodes[i];
                maxVec = Vector3.Max(node.pos, maxVec);
                minVec = Vector3.Min(node.pos, minVec);
            }

            Vector3 scaleVec = (maxVec - minVec);
           

            if (scaleVec.y == 0) scaleVec.y++;
            Vector3 realScale = spaceScale/(Mathf.Pow(graph.nodes.Count,1f/10f)) * Vector3.Scale(new Vector3(1f / scaleVec.x, 1f / scaleVec.y, 1f / scaleVec.z),
                Vector3.one * (Mathf.Pow(graph.nodes.Count, 1f / 2f)));

            // Debug.Log("beforeScale:" + maxVec + " " + minVec);
           // Debug.Log("scale: " + realScale);

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];
                Vector3 pos = 
                    Vector3.Scale(node.pos, realScale);
                if(!temp) node.pos = pos;
                node.nodeObject.transform.localPosition = pos;
            }
         //   avgPos = Vector3.zero;


        }




        public struct BuildHierarchy : IJob
        {
        
                public void Execute()
            {
                /*Color[] vertexColors;
                if (graph.edgeObject == null)
                {
                    vertexColors = new Color[graph.edges.Count * 8];
                    for (int i = 0; i < vertexColors.Length; ++i)
                    {
                        vertexColors[i] = Color.red;
                    }
                }
                else
                    vertexColors = graph.edgeObject.GetComponent<MeshFilter>().mesh.colors;

                */


                int start = graph.modus / 2; //0,1-> 0 ,2->1
                int end = (graph.modus + 1) / 2 + 1;//0->1 , 1-> 2, 2 ->2
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    graph.nodes[i].weight = graph.nodes[i].height = 0;
                }

               
                //  Debug.Log("modus: " + graph.modus + " -> " + start + " " + end);
                for (int n = start; n < end; n++)
                {
                    if (!graph.FlatInit)
                    {
                        List<int> rootIndices = new List<int>();
                        //   float maxConnections = 0;
                        for (int i = 0; i < graph.nodes.Count; i++)
                        {

                            if (n == 0)
                            {


                                //not included -> root --> lowest
                                // if (graph.nodes[i].edgeIndicesOut.Count == 1&&graph.nodes[i].edgeIndicesOut[0]==graph.nodes[i].nr)
                                if (graph.nodes[i].edgeIndicesOut.Where(idx => graph.edges[idx].style == "include").ToList().Count == 0
                                    && graph.nodes[i].edgeIndicesIn.Where(idx => graph.edges[idx].style == "include").ToList().Count > 0)
                                {

                                    rootIndices.Add(i);
                                    if (!graph.HeightInit) graph.nodes[i].weight = -2;
                                    // Debug.Log(graph.nodes[i].label);
                                }
                                else
                                {
                                    graph.nodes[i].weight = -1;
                                }

                            }
                            else
                            {

                                // if (graph.nodes[i].edgeIndicesIn.Count == 0) rootIndices.Add(i);
                                if (graph.nodes[i].edgeIndicesIn.Where(idx => graph.edges[idx].style == "include").ToList().Count == 0
                                 && graph.nodes[i].edgeIndicesOut.Where(idx => graph.edges[idx].style == "include").ToList().Count > 0)
                                {
                                    rootIndices.Add(i);
                                    if (!graph.HeightInit) graph.nodes[i].height = 2;
                                    // Debug.Log(graph.nodes[i].label);
                                }
                                else
                                {
                                    graph.nodes[i].height = -1;
                                }
                            }
                            //    maxConnections = Mathf.Max(graph.nodes[i].connectedNodes.Count, maxConnections);

                        }



                        //   Debug.Log(rootIndices.Count + " root nodes");
                        //  rootIndices = rootIndices.GetRange(0, Mathf.Min(rootIndices.Count,200));
                        // if (rootIndices.Count > 200) continue;

                        if (graph.HeightInit)
                        {

                            for (int i = 0; i < rootIndices.Count; i++)
                            {

                                float curHeight = 1;
                                Stack<int> nodeIndexStack = new Stack<int>();
                                nodeIndexStack.Push(rootIndices[i]);
                                Stack<float> heightStack = new Stack<float>();
                                heightStack.Push(curHeight);

                                // visited[rootIndices[i]] = true;

                                while (nodeIndexStack.Count > 0)
                                {
                                    int curIndex = nodeIndexStack.Pop();
                                    //  visited[curIndex] = true;
                                    curHeight = heightStack.Pop();
                                    //  maxHeight = Mathf.Max(curHeight, maxHeight);
                                    // minHeight = Mathf.Min(curHeight, minHeight);

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
                                        // Debug.Log(graph.edges[edgeIndex].style);
                                        if (graph.edges[edgeIndex].style != "include") continue;

                                        int childNodeIndex;
                                        if (n == 0)
                                        {
                                            childNodeIndex = graph.nodeDict[graph.edges[curNode.edgeIndicesIn[j]].from];
                                            // graph.nodes[childNodeIndex].height -=curHeight;
                                            if (graph.nodes[childNodeIndex].weight < curHeight)
                                                graph.nodes[childNodeIndex].weight = curHeight;// Mathf.Max(curHeight, graph.nodes[childNodeIndex].weight);//+= curHeight;
                                            else
                                            {
                                                continue;

                                            }



                                        }
                                        else
                                        {
                                            childNodeIndex = graph.nodeDict[graph.edges[curNode.edgeIndicesOut[j]].to];
                                            if (graph.nodes[childNodeIndex].height < curHeight)
                                                graph.nodes[childNodeIndex].height = curHeight;//Mathf.Max(curHeight, graph.nodes[childNodeIndex].height);//+= curHeight;
                                            else
                                                continue;
                                        }

                                        nodeIndexStack.Push(childNodeIndex);
                                        if (n == 0) heightStack.Push(curHeight + curNode.inWeights[j]);
                                        else heightStack.Push(curHeight + curNode.outWeights[j]);

                                    }

                                }
                            }
                        }
                    }
                    // sliceWidth = 0.2f;
                    // sliceWidth = Mathf.Max(0.1f, 5.0f * maxHeight / Mathf.Sqrt(graph.nodes.Count));


                    //Debug.Log("maxHeight: " + maxHeight);
                }

                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    var node = graph.nodes[i];

                    //Debug.Log(node.label + " " + node.height);
                    //var y = (node.height - 0.8f * node.weight) * sliceWidth;

                    float y = node.pos.y;
                    if(!graph.RandomInit) y =node.weight - node.height;//-0.1f*node.height; //;+ node.height;* Mathf.Max(1, (graph.nodes.Count / 200.0f)
                                                                        //  Debug.Log(y + " " + node.label);
                                                                        // Debug.Log(i + " weight: " + node.weight + " height: " + node.height+" y " + y * Mathf.Max(1, (graph.nodes.Count / 200.0f)));
                                                                        /*  float x =(maxConnections/10- node.connectedNodes.Count)*20 ;
                                                                          if (x < -vol) x = 0;

                                                                          if (i % 2 == 0) x *= -1;*/
                    else if (graph.FlatInit) y = 0;

                 //   if (node.weight == -1 || node.height == -1) y = -1;
                 //this changes the Layout significantly when initializing!!!!!!!!!!!!!!!!!!!!!but is wrong?

                    // y = Mathf.Sign(y) * (Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(y * graph.nodes.Count / 100))));
                    // Debug.Log(node.weight + " " + node.height + " " + y);
                    Vector3 pos = new Vector3(node.pos.x, y, node.pos.z);// / Mathf.Max(1, (graph.nodes.Count / 200.0f));


                    node.pos = pos;
                }

            }
        

                
    }

        public struct UpdateEnergies : IJob
        {
            NativeArray<float> Energies;
           // float energyBefore;
            //int success;

            public UpdateEnergies(NativeArray<float> Energies)
            {
                this.Energies = Energies;
            }
            
            public void Execute()
            {

                for(int j=0; j<graph.nodes.Count;++j)
                {
                    var n = graph.nodes[j];
                    var dispLength = Mathf.Sqrt(n.disp.x * n.disp.x + n.disp.y * n.disp.y + n.disp.z * n.disp.z) + epsilon;
                    n.pos.x = ((n.pos.x + (n.disp.x / dispLength) * step));
                    if (n.weight == -1 || n.height == -1||graph.UseForces)
                    {
                        n.pos.y = ((n.pos.y + (n.disp.y / dispLength) * step));
                      //  Debug.Log(n.weight + " " + n.height + " " + n.id);
                    }
                       

                    n.pos.z = ((n.pos.z + (n.disp.z / dispLength) * step));

                    Energies[j] = dispLength * dispLength;
                }
           



                graph.fin++;
                energyBefore = energy;
                energy = Energies.Sum();
                // Debug.Log(energy);
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
                energyBefore = energy;
            }

        }
        // Function to calculate forces driven layout
        // ported from Marcel's Javascript versions
        public struct JobTest : IJobParallelFor
        {
            private bool useWeights;
            private int iterations;
            private float spacingValue;
            private float initialStep;
            private float globalWeight;
  
            NativeArray<float> Energies;
            // Color[] vertexColors;
         
            float kSquared;
            float kVal;
           // Color[] vertexColors;

            public JobTest(int iterations, float spacingValue, NativeArray<float> Energies, float initialStep = 3.0f, float globalWeight = 1.0f, bool useWeights = true)
            {
               
                this.iterations = iterations;
                this.spacingValue = spacingValue;
                //Here:middle node
                this.initialStep = initialStep;
                this.globalWeight = globalWeight;
                this.useWeights = useWeights;
                this.Energies = Energies;

               /*  {
                     vertexColors = new Color[10000 * 8];
                     for (int i = 0; i < vertexColors.Length; ++i)
                     {
                         vertexColors[i] = Color.red;
                     }
                 }*/
         
                                    
                kVal =Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 30), 0.1f);
                //kVal = Mathf.Log(kVal);
                kVal = 1f;
                kSquared = kVal * kVal;

                

                for (int i = 0; i < graph.nodes.Count; ++i)
                {
                    Energies[i] = 0;
                }
                Debug.Log(kVal + " k.. " + kSquared);


            }


           public void Execute(int index)
           //public void Execute()
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



                // if (reset)




                // for (var i = 0; i < iterations; i++)
                {
                    /* if (i % 100 == 0)
                     {
                         Debug.Log("Beautify Layout: Iteration " + i + " of " + iterations + "...");
                     }*/


                 //   for (var j = 0; j < graph.nodes.Count; j++)
                    {
                        int j = index;
                        var n = graph.nodes[j];
                        float maxDist = -1;
                        // if (!n.forcesFixed) //&& !n.hidden)
                        {
                            n.disp = Vector3.zero;


                            // calculate local (spring) forces
                            List<int> edgeIndices = n.edgeIndicesIn.Concat<int>(n.edgeIndicesOut).ToList<int>();
                            for (var k = 0; k < n.connectedNodes.Count; k++)
                            {
                                // Debug.Log(vertexColors.Length + " " + edgeIndices[k] * 8 + " " + edgeIndices.Count + " " + n.connectedNodes.Count + " " + k+" "+vertexColors[edgeIndices[k] * 8].a);
                                //    if (graph.edgeObject!=null&&graph.edgeObject.GetComponent<MeshFilter>().mesh.colors[edgeIndices[k] * 8].a == 0) continue;

                                var u = graph.nodes[n.connectedNodes[k]];
                                if (!graph.edges[edgeIndices[k]].active)
                                    continue;
                                var differenceNodes = u.pos - n.pos; ;

                                var lengthDiff = Mathf.Max(0, differenceNodes.magnitude - diameter) + epsilon;
                                maxDist = Mathf.Max(maxDist, lengthDiff);

                                var attractiveForce = (lengthDiff * lengthDiff / kVal);
                                /*  if (useWeights)
                                  {
                                      if (n.weights[k] <= .9f) attractiveForce *= n.weights[k] * globalWeight;
                                      if (n.weights[k] <= .1f) attractiveForce = 0;
                                  }*/
                                n.disp += (differenceNodes / lengthDiff) * attractiveForce;

                                /*
                                if (graph.edges[edgeIndices[k]].style == "include")
                                {
                                   
                                    if (k < n.edgeIndicesIn.Count)
                                    {
                                        n.disp.y = differenceNodes.y/(lengthDiff*lengthDiff);
                                    }
                                    else
                                    {
                                        n.disp.y = + differenceNodes.y / (lengthDiff * lengthDiff);
                                    }
                                }
                                */

                            }
                            n.range = maxDist;


                            var upos = Vector3.zero;
                            var diffVec = upos - n.pos;
                            var lD = Mathf.Max(0, diffVec.magnitude - diameter) + epsilon;
                            var aF = (lD * lD / kVal);
                           // n.disp += (diffVec / lD) * 0.02f * aF;




                            int limit = 0;
                            // calculate global (repulsive) forces
                            for (var k = 0; k < graph.nodes.Count; k++)
                            {
                                var u = graph.nodes[k];
                                if (u.graphNumber == n.graphNumber && n != u && (u.edgeIndicesIn != null || u.edgeIndicesOut != null))
                                {
                                    var differenceNodes = u.pos- n.pos;
                                    var lengthDiff = Mathf.Max(0,differenceNodes.magnitude - diameter) + epsilon;
                                   
                                    var repulsiveForce = -(kSquared / lengthDiff);
                                    if (graph.fin>10&&maxDist>0&&lengthDiff > graph.PushLimit * (maxDist+n.range))
                                    {
                                      //  repulsiveForce = 0;
                                    //    limit++;
                                    }

                                    n.disp += (differenceNodes / lengthDiff) * repulsiveForce;
                                }
                            }
                          //  if (limit > 0) Debug.Log("limit " + limit+ " "+maxDist);

                            //calculate hierarchic repulsive forces
                            //positive
                            float upDist = float.MaxValue;
                            //negative
                            float downDist = float.MinValue;

                            /*
                            if (graph.WaterMode)
                            {

                                //nodes with outgoing edges push downward  

                                var before = n.disp.y;
                                n.disp.y = 0.0f;

                                var hierarchyDisp = float.MinValue;

                                for (var k = 0; k < n.edgeIndicesIn.Count; k++)
                                {
                                    if (graph.edges[edgeIndices[k]].style == "include")
                                    {

                                        var u = graph.nodes[n.connectedNodes[k]];
                                        {
                                            var differenceNodesY = u.pos.y - n.pos.y + diameter;
                                            // upDist = Mathf.Min(upDist, Mathf.Max(epsilon, differenceNodesY));

                                            hierarchyDisp = Mathf.Max(hierarchyDisp, differenceNodesY);

                                        }


                                    }
                                }

                                if (hierarchyDisp != float.MinValue) n.disp.y += kSquared / Mathf.Min(hierarchyDisp, -epsilon);

                                hierarchyDisp = float.MaxValue;
                                //nodes with incoming edges push upward

                                for (var m = 0; m < n.edgeIndicesOut.Count; m++)
                                {
                                    int k = m + n.edgeIndicesIn.Count;
                                    if (graph.edges[edgeIndices[k]].style == "include")
                                    {

                                        var u = graph.nodes[n.connectedNodes[k]];
                                        {
                                            var differenceNodesY = -(u.pos.y - n.pos.y + diameter);

                                            hierarchyDisp = Mathf.Min(hierarchyDisp, differenceNodesY);
                                        }


                                    }
                                }

                                if (hierarchyDisp != float.MaxValue) n.disp.y += kSquared / Mathf.Max(hierarchyDisp, epsilon);*/


                                if (graph.WaterMode)
                            {

                                //nodes with outgoing edges push upward      
                 
                                var before = n.disp.y;
                               // n.disp.y *= 0.4f;

                                var hierarchyDisp = 0.0f;
                        
                                for (var k = 0; k < n.edgeIndicesIn.Count; k++)
                                {
                                    if (graph.edges[edgeIndices[k]].style == "include")
                                    {

                                        var u = graph.nodes[n.connectedNodes[k]];
                                        {
                                            var differenceNodesY = u.pos.y - n.pos.y;

                                            //s
                                            upDist = Mathf.Min(upDist, Mathf.Max(epsilon, differenceNodesY-diameter));
                                      //      if (epsilon>=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id + " " );

                                        }
                                      

                                    }
                                }
                                if (upDist != float.MaxValue)
                                    hierarchyDisp -= kSquared / upDist;


                                //nodes with incoming edges push downward
                  
                                for (var m = 0; m < n.edgeIndicesOut.Count; m++)
                                {
                                    int k = m + n.edgeIndicesIn.Count;
                                    if (graph.edges[edgeIndices[k]].style == "include")
                                    {

                                        var u = graph.nodes[n.connectedNodes[k]];
                                        {
                                            var differenceNodesY = u.pos.y - n.pos.y;
                                            downDist = Mathf.Max(downDist, Mathf.Min(-epsilon, differenceNodesY+diameter));
                                          //  if (-epsilon<=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id+" ");
                                        }


                                    }
                                }
                                if (downDist != float.MinValue)
                                    hierarchyDisp -= kSquared / downDist;

                                    if (Mathf.Sign(n.disp.y) == Mathf.Sign(hierarchyDisp))
                                     {
                                          n.disp.y = 0.5f * n.disp.y + 0.5f*hierarchyDisp;
                                    
                                     }
                                     else
                                     {
                                         n.disp.y = 0.05f * n.disp.y + 0.95f*hierarchyDisp;
                                     }
                                n.disp.y = hierarchyDisp;

                               if (n.id.Contains( "_dec")|| n.id.Contains("_nat"))
                                {
                                   // Debug.Log(n.id +" "+upDist +" "+ downDist);
                                }



                              
                            }

                  

                            if (graph.UseConstraint) n.disp.y = Mathf.Max(downDist, Mathf.Min(n.disp.y, upDist));
                        }

                    }

                    //  yield return null;
                }
               

            }
        }
    }

}
