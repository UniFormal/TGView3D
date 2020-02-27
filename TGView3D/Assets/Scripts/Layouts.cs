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
        public static float step;// initialStep;
        static float currTemperature;
        //   static float maxHeight = -10;
        //   static float minHeight = 10;

        //this does not really do much, we cancle it out later
        public static float Scaler = 1;//00f;

        static float epsilon = 0.00001f;
        static float diameter = 1*0.05f;//2.5f;
        public static float VolumeWidth = diameter * 2 * Scaler;
        static bool TwoD = false;
        public static NativeArray<float> Energies;

        public static void Init()
        {
            if (GlobalVariables.Graph != null)
            {
                graph = GlobalVariables.Graph;


                VolumeWidth = (diameter) * 2 * Scaler * Mathf.Sqrt(graph.nodes.Count) / 20;
            }

            
         
        }

        public static float Stretch(int i)
        {




            //roots
            if (graph.nodes[i].edgeIndicesOut.Where(idx => TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count == 0
                && graph.nodes[i].edgeIndicesIn.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count > 0)
            {

                return -VolumeWidth;

            }


            else if (graph.nodes[i].edgeIndicesIn.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count == 0
                && graph.nodes[i].edgeIndicesOut.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count > 0)
            {
                return VolumeWidth;

            }
            //for only view graphs
            else if (graph.nodes[i].edgeIndicesIn.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count == 0
                 && graph.nodes[i].edgeIndicesOut.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "hierarchic" && graph.edges[idx].active).ToList().Count == 0
                 && (graph.nodes[i].edgeIndicesIn.Where(idx => TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "" ).ToList().Count > 0
                 || graph.nodes[i].edgeIndicesOut.Where(idx => TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == "" ).ToList().Count > 0))

                //    maxConnections = Mathf.Max(graph.nodes[i].connectedNodes.Count, maxConnections);

                return Random.Range(-VolumeWidth, VolumeWidth);
              
            return 0;
        }

        
        public static void ToTwoD(bool twoD)
        {
            if (twoD && twoD != TwoD)
            {

                for (var i = 0; i < graph.nodes.Count; i++)
                {

                    var pos = graph.nodes[i].pos;
                    pos.z = 0;
                    graph.nodes[i].pos = pos;
                    graph.nodes[i].nodeObject.transform.localPosition = pos;


                }
            }
        }



        public static void Init(bool twoD)
        {
            Init();

       

            TwoD = twoD;


        }
        public static void Spiral()
        {
            Random.InitState(10);
           // if (!graph.WaterMode) graph.FlatInit = false;
            for (var i = 0; i < graph.nodes.Count; i++)
            {

                /*var y = (1-2f*(i%2))* (float)(10 * i) % graph.nodes.Count() * 10.0f / graph.nodes.Count * 100;

              //  var y= graph.nodes[i].nodeObject.transform.localPosition.y;
    
                float angle = (float)(10*i)%graph.nodes.Count() * 10.0f / graph.nodes.Count * 2 * Mathf.PI;
                // Debug.Log(angle);

             
                Vector3 pos = graph.nodes.Count * (float)i / (float)graph.nodes.Count / 10 * new Vector3(Mathf.Sin(angle), y, Mathf.Cos(angle));
                */
                Vector3 pos = Random.insideUnitSphere * VolumeWidth * Mathf.Pow(graph.nodes.Count, 1 / 3f) / 2;
                if (graph.FlatInit) pos.y = 0;
                if (TwoD) pos.z = 0;// Random.Range(-.01f,.01f);
                if (graph.RootLeaves) pos.y = Stretch(i);
                graph.nodes[i].pos = pos;
                graph.nodes[i].nodeObject.transform.localPosition = pos;
            }

        }

        public static JobHandle ActivateForces(float globalWeight, int iterations, NativeArray<float> Energies, JobHandle hHandle)
        {
   
            //50*0.0213f
            var jt = new JobTest(iterations, 1, Energies, useWeights: true, globalWeight: globalWeight);
            var updateEnergies = new UpdateEnergies(Energies);
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(iterations * 2, Allocator.Persistent);

            //var handle = jt.Schedule();

            int batchCount = 12;// Mathf.Max(iterations * graph.nodes.Count / (100 * 100), 1);
            var handle = jt.Schedule(
                graph.nodes.Count, batchCount,
                hHandle);
            for (int i = 1; i < iterations * 2; i += 2)
            {
                //handle.Complete();


                handles[i - 1] = handle;
                handle = jt.Schedule(
                    graph.nodes.Count, batchCount,
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


        public static void InitEnergies(NativeArray<float> energies)
        {
           
            //step=1f;
            Layouts.Energies = energies;
            Debug.Log(currTemperature + " " + step + " " + energy + " " + energyBefore);
        }


        public static void EnTree()
        {
            var handle = new JobHandle();
            //if (graph.HeightInit)
            {
                var bH = new BuildHierarchy();
                handle = bH.Schedule();
            }
            handle.Complete();
        }

        public static JobHandle BaseLayout(int iterations, float globalWeight, float spaceScale, NativeArray<float> energies)
        {
            currTemperature = 0.95f;
            energyBefore = 0;
            energy = 1000000f;
            step = .3f;// initialStep;
            success = 0;
            Init();
            Spiral();
            InitEnergies(energies);
            var handle = new JobHandle();
            //if (graph.HeightInit)
            {
                var bH = new BuildHierarchy();
                handle = bH.Schedule();
            }
            handle.Complete();
   
            return UpdateLayout(iterations, globalWeight, spaceScale);

        }


        public static JobHandle UpdateLayout(int iterations = 1, float globalWeight = 1, float spaceScale = 1)
        {

          
            if (graph.UseForces)
                return ActivateForces(globalWeight, iterations, Energies, new JobHandle());

            return new JobHandle();

        }

        public static void Normalize(float spaceScale, bool temp = false)
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

            foreach (var node in graph.nodes)
            {
                node.nodeObject.transform.localPosition = node.pos* spaceScale / (Mathf.Pow(Scaler, 1 / 3f));

                //if (!temp) node.pos = node.nodeObject.transform.localPosition;
            }
            
            Vector3 avgPos = Vector3.zero;
            Vector3 desiredgPos = Vector3.zero;
            int rn = 0;
            foreach (ReadJSON.MyNode node in graph.nodes)
            {

                if (node.active)
                {
                   // desiredgPos += node.nodeObject.transform.localPosition;
                    rn++;
                }

                avgPos +=   node.nodeObject.transform.localPosition;
            }
            avgPos /= graph.nodes.Count;

     
            foreach (ReadJSON.MyNode node in graph.nodes)
            {
                node.nodeObject.transform.localPosition -= avgPos-desiredgPos;
            }


            if (temp)
            {

            }


            else
            {

                /*
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
                   foreach (var node in graph.nodes)
                {
                    node.nodeObject.transform.localPosition = node.pos * spaceScale / (Mathf.Pow(Scaler, 1 / 3f));
                }*/

                float d = 0;
                int hv = 0;
                Material loadedMat = new Material(graph.nodes[0].nodeObject.GetComponent<MeshRenderer>().material);

                loadedMat.color = Color.yellow;
                Vector3 maxVec = Vector3.one * float.MinValue;
                Vector3 minVec = Vector3.one * float.MaxValue;
                foreach (ReadJSON.MyNode node in graph.nodes)
                {
                    var nodepos = node.nodeObject.transform.localPosition;
                    node.pos = nodepos;
                }
                    foreach (ReadJSON.MyNode node in graph.nodes)
                {
                    var nodepos = node.pos;
                    maxVec = Vector3.Max(nodepos, maxVec);
                    minVec = Vector3.Min(nodepos, minVec);

                   // if (graph.WaterMode)
                    {
                        foreach (int edge in node.edgeIndicesIn)
                        {

                            if (TGraph.ReadJSON.EdgeTypes[graph.edges[edge].style].type == "hierarchic"&&graph.edges[edge].active)
                            {

                                d += (nodepos - graph.nodes[graph.nodeDict[graph.edges[edge].from]].nodeObject.transform.position).magnitude;
                                if (graph.nodes[graph.nodeDict[graph.edges[edge].from]].pos.y < node.pos.y)
                                {
                                    //   node.nodeObject.transform.localScale = Vector3.one*.2f;

                                   //   node.nodeObject.GetComponent<MeshRenderer>().material.color = Color.red;

                                    //    Debug.Log(node.id +", height "+node.weight+ " from " + graph.edges[edge].from+ ", height " + graph.nodes[graph.nodeDict[graph.edges[edge].from]].weight);
                                   // graph.nodes[graph.nodeDict[graph.edges[edge].from]].nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;
                                    //node.nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;
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
                            if (TGraph.ReadJSON.EdgeTypes[graph.edges[edge].style].type == "hierarchic")
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
      
                }
                var width = (maxVec - minVec).magnitude;
                Debug.Log(hv + " Height Violations, " + d / graph.nodes.Count + " average edgelength, " + width + " diameter, " + d / graph.nodes.Count / width + " normalized edgeLength" +" energy "+energy);
            }


            /*
            Vector3 maxVec = Vector3.one * float.MinValue;
            Vector3 minVec = Vector3.one * float.MaxValue;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                
                var node = graph.nodes[i];
                maxVec = Vector3.Max(node.pos, maxVec);
                minVec = Vector3.Min(node.pos, minVec);
            }

            Vector3 scaleVec = (maxVec - minVec);
           

            //if (scaleVec.y == 0) scaleVec.y++;
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
            }*/


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

                string hType = "tree";
                int start = graph.modus / 2; //0,1-> 0 ,2->1
                int end = (graph.modus + 1) / 2 + 1;//0->1 , 1-> 2, 2 ->2
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    graph.nodes[i].weight = -1;
                    graph.nodes[i].height = -1;
                }

                start = 0; end = 1;
                //  Debug.Log("modus: " + graph.modus + " -> " + start + " " + end);
                for (int n = start; n < end; n++)
                {
                    //if (graph.HeightInit || (!graph.FlatInit && graph.WaterMode))
                    {
                        List<int> rootIndices = new List<int>();
                        //   float maxConnections = 0;
                        for (int i = 0; i < graph.nodes.Count; i++)
                        {

                            if (n == 0)
                            {


                                //not included -> root --> lowest
                                // if (graph.nodes[i].edgeIndicesOut.Count == 1&&graph.nodes[i].edgeIndicesOut[0]==graph.nodes[i].nr)
                                if (graph.nodes[i].edgeIndicesOut.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == hType && graph.edges[idx].active).ToList().Count == 0
                                    && graph.nodes[i].edgeIndicesIn.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == hType && graph.edges[idx].active).ToList().Count > 0)
                                {

                                    rootIndices.Add(i);
               
                                }
                               

                            }
                            else
                            {

                                // if (graph.nodes[i].edgeIndicesIn.Count == 0) rootIndices.Add(i);
                                if (graph.nodes[i].edgeIndicesIn.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == hType && graph.edges[idx].active).ToList().Count == 0
                                 && graph.nodes[i].edgeIndicesOut.Where(idx =>TGraph.ReadJSON.EdgeTypes[graph.edges[idx].style].type == hType && graph.edges[idx].active).ToList().Count > 0)
                                {
                                    rootIndices.Add(i);

                                }
                                
                            }
                            //    maxConnections = Mathf.Max(graph.nodes[i].connectedNodes.Count, maxConnections);

                        }

           

                        //   Debug.Log(rootIndices.Count + " root nodes");
                        //  rootIndices = rootIndices.GetRange(0, Mathf.Min(rootIndices.Count,200));
                        // if (rootIndices.Count > 200) continue;

                        {

                            for (int i = 0; i < rootIndices.Count; i++)
                            {

                                float curHeight = 0;
                                Stack<int> nodeIndexStack = new Stack<int>();
                                nodeIndexStack.Push(rootIndices[i]);
                                Stack<float> heightStack = new Stack<float>();
                                heightStack.Push(curHeight);
                                graph.nodes[rootIndices[i]].weight = 0;
                                graph.nodes[rootIndices[i]].height = 0;

                                // visited[rootIndices[i]] = true;

                                while (nodeIndexStack.Count > 0)
                                {
                                    int curIndex = nodeIndexStack.Pop();
                                    //  visited[curIndex] = true;
                                    curHeight = heightStack.Pop();
                    //                Debug.Log(curHeight);
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
                                       // if (graph.edges[edgeIndex].style != "include") continue;
                                        if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndex].style].type != hType ) continue;
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
             
                    if(node.height>=0 || node.weight >= 0)
                    {
                        //Debug.Log(node.label + " " + node.height);
                        //var y = (node.height - 0.8f * node.weight) * sliceWidth;
             
                        float y = node.weight - node.height;//-0.1f*node.height; //;+ node.height;* Mathf.Max(1, (graph.nodes.Count / 200.0f)
                                                            //  Debug.Log(y + " " + node.label);
                                                            // Debug.Log(i + " weight: " + node.weight + " height: " + node.height+" y " + y * Mathf.Max(1, (graph.nodes.Count / 200.0f)));
                                                            /*  float x =(maxConnections/10- node.connectedNodes.Count)*20 ;
                                                              if (x < -vol) x = 0;

                                                              if (i % 2 == 0) x *= -1;*/
                                                            // else if (graph.FlatInit) y = 0;

                        //   if (node.weight == -1 || node.height == -1) y = -1;
                        //this changes the Layout significantly when initializing!!!!!!!!!!!!!!!!!!!!!but is wrong?
                        Debug.Log(y);
                        // y = Mathf.Sign(y) * (Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(y * graph.nodes.Count / 100))));
                        // Debug.Log(node.weight + " " + node.height + " " + y);
                        node.style = "chapter";

                        Vector3 pos = new Vector3(node.pos.x, y, node.pos.z);// / Mathf.Max(1, (graph.nodes.Count / 200.0f));


                        node.pos = pos;
                    }
                    else
                    {
                  //      Debug.Log(node.id);
                        node.style = "";
                    }
             
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
                this.Energies = Layouts.Energies;
            }

            public void Execute()
            {

                for (int j = 0; j < graph.nodes.Count; ++j)
                {
                    var n = graph.nodes[j];
               

                    var dispLength = n.disp.magnitude + epsilon;
                    var disp = n.disp / dispLength * step;
                    if (graph.UseConstraint)
                    {
                     
                        var neo = ApplyConstraints(n, disp.y);
                        disp.y = neo;
                    }
                    //if(n.active)
                        n.pos += disp;
                    Energies[j] = dispLength * dispLength;
                    /*  if (n.weight == -1 || n.height == -1||graph.UseForces)
   {
       n.pos.y = ((n.pos.y + (n.disp.y / dispLength) * step));
     //  Debug.Log(n.weight + " " + n.height + " " + n.id);
   }*/
                }

                Layouts.Energies = this.Energies;
                graph.fin++;
                energyBefore = energy;
               // Debug.Log(energy);
                energy = Energies.Sum();
               // Debug.Log(energy + " " + step);
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
            //    Debug.Log(energy);
            }

        }
        public static void ApplyWaterForceMagnetic(ReadJSON.MyNode n, List<int> edgeIndices, float kSquared)
        {
         
            var upDist = Vector3.zero;
            var downDist = Vector3.zero;

            for (var k = 0; k < n.edgeIndicesIn.Count; k++)
            {
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodes = u.pos - n.pos;
                        //s
                        var ortho1 = Vector3.Cross(differenceNodes, Vector3.up).normalized;
                        var ortho2 = Vector3.Cross(differenceNodes, ortho1).normalized;
                        var angle = Vector3.Angle(differenceNodes, Vector3.up);
                        upDist += Vector3.Magnitude(differenceNodes) * (ortho1+ortho2) * angle/360;

                    }


                }
            }

            //nodes with incoming edges push downward

            for (var m = 0; m < n.edgeIndicesOut.Count; m++)
            {
                int k = m + n.edgeIndicesIn.Count;
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodes = u.pos - n.pos;
                        var ortho1 = Vector3.Cross(differenceNodes, Vector3.up).normalized;
                        var ortho2 = Vector3.Cross(differenceNodes, ortho1).normalized;
                        var angle = Vector3.Angle(differenceNodes, Vector3.up);
                        downDist += Vector3.Magnitude(differenceNodes) * (ortho1 + ortho2) * angle / 360;
                    }


                }
            }
        
            n.disp += (-downDist - upDist);
            /*
        if (Mathf.Sign(n.disp.y) == Mathf.Sign(hierarchyDisp))
        {
            n.disp.y = 0.9f * n.disp.y + .1f * hierarchyDisp;

        }
        else
        {
            n.disp.y = 0.1f * n.disp.y + 0.9f * hierarchyDisp;
        }*/




            //  if (graph.UseConstraint) n.disp.y = Mathf.Max(downDist, Mathf.Min(n.disp.y, upDist));
        }


        public static void ApplyWaterForceSum(ReadJSON.MyNode n, List<int> edgeIndices, float kSquared)
        {
            //calculate hierarchic repulsive forces
            //positive
            float upDist = 0;
            //negative
            float downDist = 0;



            //var plane = new Plane(u.pos - n.pos, u.pos);
            //  differenceNodesY = plane.GetDistanceToPoint(n.pos);
            //nodes with outgoing edges push upward      


            // n.disp.y *= 0.4f;
            //downDist = upDist = 0.0f;

            var hierarchyDisp = 0f;
            for (var k = 0; k < n.edgeIndicesIn.Count; k++)
            {
              //  Debug.Log(TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type + " " + graph.edges[edgeIndices[k]].style);
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {
                    
                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodesY = u.pos.y - n.pos.y;
                        //s
                        upDist += kSquared/ Mathf.Max(epsilon, differenceNodesY - (diameter+u.radius+n.radius));
                        //      if (epsilon>=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id + " " );

                    }


                }
            }

            //nodes with incoming edges push downward

            for (var m = 0; m < n.edgeIndicesOut.Count; m++)
            {
                int k = m + n.edgeIndicesIn.Count;

               // Debug.Log(TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type + " " + graph.edges[edgeIndices[k]].style);
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodesY = u.pos.y - n.pos.y;
                        downDist += kSquared/Mathf.Min(-epsilon, differenceNodesY + (diameter+u.radius+n.radius));
                        //  if (-epsilon<=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id+" ");
                    }


                }
            }
            hierarchyDisp = (-downDist - upDist);


            if (Mathf.Sign(n.disp.y) == Mathf.Sign(hierarchyDisp))
            {
              //  n.disp.y = 0.9f * n.disp.y + .1f * hierarchyDisp;


            }
            else
            {
                n.disp.y = 0.1f * n.disp.y + 0.9f * hierarchyDisp;
            }




            //  if (graph.UseConstraint) n.disp.y = Mathf.Max(downDist, Mathf.Min(n.disp.y, upDist));
        }


        public static void ApplyWaterForce(ReadJSON.MyNode n, List<int> edgeIndices, float kSquared)
        {
            //calculate hierarchic repulsive forces
            //positive
            float upDist = float.MaxValue;
            //negative
            float downDist = float.MinValue;



            //var plane = new Plane(u.pos - n.pos, u.pos);
            //  differenceNodesY = plane.GetDistanceToPoint(n.pos);
            //nodes with outgoing edges push upward      

            var before = n.disp.y;
            // n.disp.y *= 0.4f;
            //downDist = upDist = 0.0f;
            var hierarchyDisp = 0.0f;

            for (var k = 0; k < n.edgeIndicesIn.Count; k++)
            {
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodesY = u.pos.y - n.pos.y;
                        differenceNodesY = Mathf.Pow(Mathf.Abs(differenceNodesY), 1);
                        //s
                        upDist = Mathf.Min
                            (upDist, Mathf.Max(epsilon, differenceNodesY - diameter));
                        //      if (epsilon>=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id + " " );

                    }


                }
            }
            if (upDist != float.MaxValue) hierarchyDisp -= kSquared / upDist;


            //nodes with incoming edges push downward

            for (var m = 0; m < n.edgeIndicesOut.Count; m++)
            {
                int k = m + n.edgeIndicesIn.Count;
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        var differenceNodesY = u.pos.y - n.pos.y;
                        differenceNodesY = Mathf.Pow((differenceNodesY), 1);
                        downDist = Mathf.Max
                            (downDist, Mathf.Min(-epsilon, differenceNodesY + diameter));
                        //  if (-epsilon<=differenceNodesY) if (n.id.Contains("_dec") || n.id.Contains("_nat")) Debug.Log(n.id + " " + u.id+" ");
                    }


                }
            }
            if (downDist != float.MinValue) hierarchyDisp -= kSquared / downDist;
            //  hierarchyDisp = downDist + upDist;

            if (hierarchyDisp != 0)
            {

                if (Mathf.Sign(n.disp.y) == Mathf.Sign(hierarchyDisp))
                {
                    n.disp.y = 0.5f * n.disp.y + .1f * hierarchyDisp;

                }
                else
                {
                    n.disp.y = 0.1f * n.disp.y + 0.5f * hierarchyDisp;
                }
            }
        }


        public static float ApplyConstraints(ReadJSON.MyNode n, float disp)
        {
            //limit disp by largest valid up and downDist
            List<int> edgeIndices = n.edgeIndicesIn.Concat<int>(n.edgeIndicesOut).ToList<int>();
            float downDist = float.MinValue;
            float upDist = float.MaxValue;
            float better = 0;
            for (var k = 0; k < n.edgeIndicesIn.Count; k++)
            {
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic" && graph.edges[edgeIndices[k]].active)
                {
                    //nodes u that have edges that go to n are above n
                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        // u - n is positive
                        var differenceNodesY =  ( u.pos.y -n.pos.y)/2;
                        upDist = Mathf.Min (upDist, differenceNodesY);
                        //the smallest distance upwards

                    }

                }
            }
      
     
      

            //analogue
            for (var m = 0; m < n.edgeIndicesOut.Count; m++)
            {
                int k = m + n.edgeIndicesIn.Count;
                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic"&& graph.edges[edgeIndices[k]].active)
                {

                    var u = graph.nodes[n.connectedNodes[k]];
                    {
                        //u - n is negative
                        var differenceNodesY = (u.pos.y - n.pos.y) / 2;
                        downDist =  Mathf.Max(downDist, differenceNodesY);
                        //the smallest (negative) distance downward
                    }


                }
            }


            return Mathf.Clamp(disp, downDist, upDist);


       

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
            // Color[] vertexColors;https://isabelle.sketis.net/repos/isabelle/file/tip/src/Pure/General/graph.ML

            public JobTest(int iterations, float spacingValue, NativeArray<float> Energies, float initialStep = 3.0f, float globalWeight = 1.0f, bool useWeights = true)
            {

                this.iterations = iterations;
                this.spacingValue = spacingValue;

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
                // kVal = Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 30), 0.1f);
                //  kVal =Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 300), 70);


                float s = 3;
              //  if (TwoD) s = 1;

                kVal = Mathf.Pow(VolumeWidth, 1f / s);
                //kVal = Mathf.Log(kVal);
                //kVal = 1f;
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

                                var lengthDiff = Mathf.Max(0, differenceNodes.magnitude - (diameter+n.radius+u.radius)) + epsilon;
                                maxDist = Mathf.Max(maxDist, lengthDiff);

                                var attractiveForce = (lengthDiff * lengthDiff / kVal);
                          /*
                              //    if (useWeights)
                                  {
                                      if (n.weights[k] <= .9f) attractiveForce *= n.weights[k] * globalWeight;
                                      if (n.weights[k] <= .1f) attractiveForce = 0;
                                  }*/
                                n.disp += (differenceNodes / lengthDiff) *attractiveForce;
                                
                                /*
                                if (TGraph.ReadJSON.EdgeTypes[graph.edges[edgeIndices[k]].style].type == "hierarchic")
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
                            /*
                            var upos = Vector3.right*n.GraphNumber*3;
                            var diffVec = upos - n.pos;
                            var lD = Mathf.Max(0, diffVec.magnitude - diameter) + epsilon;
                            var aF = (lD * lD / kVal);

                            //if (graph.fin % 5 == 0)
                            n.disp += (diffVec / lD) * 0.003f*  aF;*/

                            /*
                            if (TwoD)
                            {
                                upos = new Vector3(n.pos.x, n.pos.y, 0);
                                diffVec = upos - n.pos;
                                lD = Mathf.Max(0, diffVec.magnitude - diameter) + epsilon;
                                aF = (lD * lD / kVal);

                                //if (graph.fin % 5 == 0)
                                n.disp += (diffVec / lD) * 1000*aF;
                            }*/

                            /*
                            if (n.weight!=-1)
                            {
                                upos = new Vector3(n.pos.x, n.weight, n.pos.z);
                                diffVec = upos - n.pos;
                                lD = Mathf.Max(0, diffVec.magnitude - diameter) + epsilon;
                                aF = (lD * lD / kVal);

                                //if (graph.fin % 5 == 0)
                                n.disp += (diffVec / lD) * 10 * aF;
                            }
                            if (n.height != -1)
                            {
                                upos = new Vector3(n.pos.x,- n.height, n.pos.z);
                                diffVec = upos - n.pos;
                                lD = Mathf.Max(0, diffVec.magnitude - diameter) + epsilon;
                                aF = (lD * lD / kVal);

                                //if (graph.fin % 5 == 0)
                                n.disp += (diffVec / lD) * 10 * aF;
                            }*/



                            var repulsion = Vector3.zero;

                            int limit = 0;
                            // calculate global (repulsive) forces
                            for (var k = 0; k < graph.nodes.Count; k++)
                            {
                                var u = graph.nodes[k];
                              //  if (u.GraphNumber == n.GraphNumber && n != u && (u.edgeIndicesIn != null || u.edgeIndicesOut != null))
                                {
                                    var differenceNodes = u.pos - n.pos;
                                    var lengthDiff = Mathf.Max(0, differenceNodes.magnitude - (diameter+n.radius+u.radius)) + epsilon;

                                    var repulsiveForce = -(kSquared / lengthDiff);
                                    if (graph.PushLimit > 0.01f)
                                    {
                                        if (//n.GraphNumber == u.GraphNumber&&
                                            graph.fin > iterations*.2f && maxDist > 0 && lengthDiff > graph.PushLimit * (maxDist + n.range))
                                        {
                                            repulsiveForce = 0;
                                            //    limit++;
                                        }
                                        else //increase it to even out zeroing
                                        {
                                            repulsiveForce *= 1.2f;
                                        }

                                       // if (n.parentId != u.parentId || n.parentId == "" || u.parentId == "") repulsiveForce *= 2f;
                                       // else repulsiveForce /= 2f;

                                    }

                                    if (n.GraphNumber != u.GraphNumber) repulsiveForce *= 4;

                                    repulsion += (differenceNodes / lengthDiff) * repulsiveForce;
                                }
                            }
                            //  if (limit > 0) Debug.Log("limit " + limit+ " "+maxDist);
                            if (repulsion == Vector3.zero) return;
                            n.disp += repulsion;

                       



                            if (graph.WaterMode)
                            {
                                ApplyWaterForceSum(n, edgeIndices, kSquared);
                            }

                            if (TwoD) n.disp.z = 0;//*= 0.001f;
                            if (graph.HeightInit&&!graph.UseConstraint) n.disp.y = 0;

                            if (n.style == "chapter") n.disp.y = 0;
                        

                        }

                    }

                    //  yield return null;
                }


            }
        }
    }

}
