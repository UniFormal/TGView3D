
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;

namespace TGraph
{


    public class ReadJSON : MonoBehaviour
    {
        public GameObject Percent;
        public MyGraph graph;
        public Material mat;
        public Material lineMat;
        public GameObject grabbable;
        public bool onlyInclude = false;
        public bool buildHierarchy = true;
        public bool normalize = false;
        public GameObject EventSystem;
        // public Material texMat;
        //public Texture2D testTex;
        //private MeshRenderer mr;
        // private GameObject TextPrefab;
        public float globalWeight;
 
        public int iterations = 25;
        public float spaceScale = 1;
        public GameObject UrlSelect;
        int si = 0;
        public int subNode = 400;
        public string url;//http://neuralocean.de/graph/test/nasa.json";
        public int vol = 100;
        public TextAsset[] GraphFiles;


        //TODO: throw out ugly indexing!!!!!
        [System.Serializable]
        public class MyGraph
        {
            public List<MyNode> nodes;
            public NativeArray<Vector3> Disps;
            public NativeArray<Vector3> Positions;
            public Dictionary<string, int> nodeDict;
            public List<MyEdge> edges;
            public List<MyEdge> tmpEdges;
            public List<int> subEdges;
            public int handIndex = 0;

            //use object references instead?
            public List<int> selectedNodes;
            public List<int> movingNodes;
            public int latestSelection = -1;
            public int currentTarget = -1;
            public int subGraphOrign = -1;
            public int fin = 0;

            public GameObject edgeObject;
            public GameObject subObject;
            public List<int> badHack;
            public float lineWidth = 0.003f;
            //public List<int> removeList;
            public Dictionary<string, Color> colorDict;

            public static MyGraph CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<MyGraph>(jsonString);
            }


        }
        [System.Serializable]
        public class MyNode
        {
            public string id;
            public string style;
            public string label;
            public string url;
            public string mathml;
            public string svg;
            public Vector3 pos;
            public Vector3 disp;
            public GameObject nodeObject;
            public bool forcesFixed;

            //use object references instead?
            public List<int> edgeIndicesOut = new List<int>();
            public List<int> edgeIndicesIn = new List<int>();
            public List<int> connectedNodes = new List<int>();


            public List<float> weights = new List<float>();
            public List<float> inWeights = new List<float>();
            public List<float> outWeights = new List<float>();
            public int graphNumber;
            public float height = 0;
            public float weight = 0;
            public GameObject nodeEdgeObject;
            public GameObject labelObject;
            public bool selected = false;
            public int nr;




        }
        [System.Serializable]
        public class MyEdge
        {
            public string id;
            public string style;
            public string from;
            public string to;
            public string label;
            public string url;
            public string clickText;
            public float localIdx = 0;
            public GameObject line;
            public GameObject labelObject;
            public Color col;
            public bool active = true;
            //public List<MyNestedObject> nestedObjects;


        }
        /*
        [System.Serializable]
        private class MyNestedObject
        {
            public string nestedVariable1;
            public string nestedVariable2;
        }
        */


        List<int> countNodesInGraph = new List<int>();
        void identifySubgraphs()
        {

            Debug.Log("Identify Subgraphs... for node count of " + graph.nodes.Count);
            for (var i = 0; i < graph.nodes.Count; i++)
            {
               /* if (graph.nodes[i].connectedNodes.Count == 0)
                {
                    graph.nodes.RemoveAt(i);
                    Debug.Log("delete " + i);
                    i--;
                }*/
            }
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                graph.nodes[i].graphNumber = -1;
            }
            Material curMaterial = mat;
            var nodesToCheck = new Stack<int>();
            var graphNumber = 1;
            Color randColor = Color.green;
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                var n = graph.nodes[i];
                if (n.graphNumber < 1)
                {

                    nodesToCheck.Push(i);
                    countNodesInGraph.Add(0);

                    while (nodesToCheck.Count > 0)
                    {
                        countNodesInGraph[countNodesInGraph.Count - 1]++;

                        var currNode = nodesToCheck.Pop();
                        if (graph.nodes[currNode].nodeObject == null)
                        {
                            Debug.Log(currNode + " " + graph.nodes[currNode].id + " " + graph.nodes[currNode].label);
                        }
                        graph.nodes[currNode].graphNumber = graphNumber;
                        graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().sharedMaterial = curMaterial;
                        graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().sharedMaterial.color = randColor;

                        for (var j = 0; j < graph.nodes[currNode].connectedNodes.Count; j++)
                        {
                            var u = graph.nodes[currNode].connectedNodes[j];
                            if (graph.nodes[u].graphNumber < 1)
                            {
                                nodesToCheck.Push(u);
                            }
                        }



                    }
                    graphNumber++;
                    curMaterial = new Material(curMaterial);
                    randColor = Random.ColorHSV();

                }
            }
            Debug.Log("subgraphs found: " + (graphNumber - 1));
            foreach (var n in countNodesInGraph) Debug.Log(n+"nodes in Subgraph");

        }

        public void CalculateSubgraph()
        {
            Debug.Log(graph.nodes.Count + " looking for " + graph.latestSelection);
            if (graph.latestSelection >= graph.nodes.Count||graph.latestSelection==-1)
                return;
            var node = graph.nodes[graph.latestSelection];
     
            //deactivate
            if (graph.subGraphOrign == graph.latestSelection)
            {
                GameObject.Destroy(graph.subObject);
                GameObject.Destroy(node.nodeObject.transform.GetChild(1).gameObject);

                graph.subGraphOrign = -1;
                graph.edgeObject.SetActive(true);
            }
            else
            {
                if (graph.subObject != null)
                {
                    GameObject.Destroy(graph.subObject);
                    //TODO: destroy old object
                   // GameObject.Destroy(node.nodeObject.transform.GetChild(1).gameObject);
                }
                   

                graph.edgeObject.SetActive(false);
                graph.subGraphOrign = node.nr;
                Debug.Log(node.label);
                List<int> edgesIn = new List<int>();
                List<int> edgesOut = new List<int>();
                bool[] visited = new bool[graph.nodes.Count];

                for (int n = 0; n < graph.nodes.Count; n++)
                {
                    visited[n] = false;
                }

                foreach (int idx in node.edgeIndicesIn)
                {
                    edgesIn.Add(idx);
                }
                for (int i = 0; i < edgesIn.Count; ++i)
                {
                    int idxIn = edgesIn[i];
                    foreach (int idx in graph.nodes[graph.nodeDict[graph.edges[idxIn].from]].edgeIndicesIn)
                    {
                        
                        if (!visited[graph.nodeDict[graph.edges[idx].from]])
                        {
                            edgesIn.Add(idx);
                            visited[graph.nodeDict[graph.edges[idx].from]] = true;
                        }
                   
                    }


                }

                foreach (int idx in node.edgeIndicesOut)
                {
                    edgesOut.Add(idx);
                }
                for (int i = 0; i < edgesOut.Count; ++i)
                {
                    int idxOut = edgesOut[i];
                    foreach (int idx in graph.nodes[graph.nodeDict[graph.edges[idxOut].to]].edgeIndicesOut)
                    {
                       
                        if (!visited[graph.nodeDict[graph.edges[idx].to]])
                        {
                            edgesOut.Add(idx);
                            visited[graph.nodeDict[graph.edges[idx].to]] = true;
                        }

                    }

                }

                List<int> edgeIndices = (edgesIn.Concat<int>(edgesOut).ToList<int>());
                List<MyEdge> edges = new List<MyEdge>();
                foreach (int eidx in edgeIndices)
                    edges.Add(graph.edges[eidx]);

                graph.subEdges = edgeIndices;
                graph.subObject = TGraph.ReadJSON.BuildEdges(edges, ref graph, graph.edgeObject.GetComponent<MeshRenderer>().sharedMaterial);
  
                graph.subObject.name = "subgraph";
                graph.subObject.transform.parent = this.transform.parent;
                graph.subObject.transform.localPosition = Vector3.zero;
                graph.subObject.transform.localEulerAngles = Vector3.zero;
                GameObject Aura = Instantiate(Resources.Load("Aura")) as GameObject;
                Aura.transform.parent = node.nodeObject.transform;
                Aura.transform.position = node.pos;
                
                Camera.main.transform.LookAt(node.pos);

            }
           
         

        }


        public static void SetTriangles(int i, int[] triangles)
        {
            int sideCount = 4;
            int vertexCount = 8;
            int n = sideCount * 6;
            int tid = n * i;



            for (int k = 0; k < sideCount; ++k)
            {

                triangles[tid++] = (0 + 2 * k) % vertexCount + i * vertexCount;
                triangles[tid++] = (1 + 2 * k) % vertexCount + i * vertexCount;
                triangles[tid++] = (2 + 2 * k) % vertexCount + i * vertexCount;
                triangles[tid++] = (2 + 2 * k) % vertexCount + i * vertexCount;
                triangles[tid++] = (1 + 2 * k) % vertexCount + i * vertexCount;
                triangles[tid++] = (3 + 2 * k) % vertexCount + i * vertexCount;

            }



        }

        public static void createStraightEdge(int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho)
        {

            vertices[0 + i * 8] = sourcePos + offset + offsetOrtho;
            vertices[1 + i * 8] = targetPos + offset + offsetOrtho;

            vertices[2 + i * 8] = sourcePos + offset - offsetOrtho;
            vertices[3 + i * 8] = targetPos + offset - offsetOrtho;


            vertices[4 + i * 8] = sourcePos - offset - offsetOrtho;
            vertices[5 + i * 8] = targetPos - offset - offsetOrtho;

            vertices[6 + i * 8] = sourcePos - offset + offsetOrtho;
            vertices[7 + i * 8] = targetPos - offset + offsetOrtho;
        }

        public static void createEdge(int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho, int type, int controlPoints)
        {
            if (type < 0)
                createStraightEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);
            else
            {
                /*
                for(int k = 0; k < controlPoints; ++k)
                {
                    float alpha = k / controlPoints;
                    vertices[k + 0 * controlPoints + i] = sourcePos * alpha + targetPos * (1 - alpha) + offset + offsetOrtho;
                    vertices[k + 1 * controlPoints + i] = sourcePos * alpha + targetPos * (1 - alpha) + offset - offsetOrtho;
                    vertices[k + 2 * controlPoints + i] = sourcePos * alpha + targetPos * (1 - alpha) - offset - offsetOrtho;
                    vertices[k + 3 * controlPoints + i] = sourcePos * alpha + targetPos * (1 - alpha) - offset + offsetOrtho;
                }*/
                var rand = Random.insideUnitSphere * 0.05f;
                createStraightEdge(i, vertices, sourcePos + rand, targetPos + rand, offset, offsetOrtho);
            }


        }
        public static void createEdge(int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho)
        {

            createStraightEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);

        }


        

    


        public static GameObject BuildEdges(List<MyEdge> edges, ref MyGraph graph, Material lineMat)
        {

            var nodes = graph.nodeDict;
            int tubeCount = edges.Count;
            int controlPoints = 1;
            int polyType = 4;

            /* foreach (var edge in graph.edges)
             {
                 //not straight => more tubes
                 if (edge.localIdx > 0)
                     tubeCount +=controlPoints;

             }
             int[] prefixSumArray = new int[tubeCount];
             prefixSumArray[0] = 0;

             for (int i = 0; i < tubeCount; i++)
             {
                 int edgeTubeCount = 1;
                 if (graph.edges[i].localIdx > 0)
                     edgeTubeCount = 1 + controlPoints;
                 prefixSumArray[i + 1] = prefixSumArray[i] + edgeTubeCount;
             }
             */

            Vector3[] vertices = new Vector3[2 * polyType * tubeCount];
            Color[] vertexColors = new Color[2 * polyType * tubeCount];

            int[] triangles = new int[polyType * 6 * tubeCount];

            GameObject line = new GameObject();

            MeshRenderer mr = line.AddComponent<MeshRenderer>();
            MeshFilter mf = line.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
          //  Debug.Log(triangles.Length);


            for (int i = 0; i < edges.Count; i++)
            {
                if (nodes.ContainsKey(edges[i].from) && nodes.ContainsKey(edges[i].to))
                {
                    MyNode source = graph.nodes[nodes[edges[i].from]];
                    MyNode target = graph.nodes[nodes[edges[i].to]];

                    Vector3 dir = target.pos - source.pos;

                    Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                    Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                    //random
                    /*float a = Random.value < .5 ? 1 : -1;
                    float b = Random.value < .5 ? 1 : -1;
                   
                    float alpha = Random.Range(0f, 1f);
                    Vector3 next = 7*(alpha * (a*offset - b*offsetOrtho) + offsetOrtho);
                    */

                    Vector3 next = 7 * (Quaternion.AngleAxis(360 * edges[i].localIdx, dir) * offset);
                    if (edges[i].localIdx <= 0)next *= 0;
                    //Debug.Log(edges[i].style);
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = graph.colorDict[edges[i].style] / 10;
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = (new Color(255,255,255)+ graph.colorDict[edges[i].style]*3)/4;

                    //creates square tubes by setting vertices manually

                    //  if (edges[i].localIdx > 0)
                    //     Debug.Log(edges[i].localIdx+" "+next * 100+edges[i].from);
                    /*
                    if (edges[i].localIdx > 0)
                        createEdge(i, vertices, source.pos+next, target.pos+next, offset, offsetOrtho);
                    else*/
                        createEdge(i, vertices, source.pos+next, target.pos+next, offset, offsetOrtho);
                    SetTriangles(i, triangles);


                    /*
                    if(edges[i].label!=null&& edges[i].label!= "")
                    {
                        edges[i].labelObject = GenLabel(target.nodeObject.transform.GetChild(0), edges[i].label);
                       
                      //  Debug.Log(edges[i].label);
                    }
                    /*
                    if (edges[i].clickText != null)
                    {
                        edges[i].labelObject = GenLabel(target.nodeObject.transform, edges[i].clickText);
                       // edges[i].labelObject.transform.position -= new Vector3(0, -1, 0);
                        Debug.Log(edges[i].clickText);
                    }*/


                }

            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = vertexColors;

            mr.sharedMaterial = lineMat;
            mf.sharedMesh = mesh;

            mesh.RecalculateBounds();

            return line;

        }




        public static GameObject GenLabel(Transform parent, string label)
        {
            GameObject text = (GameObject)Instantiate(Resources.Load("nodeText"));

            text.transform.parent = parent;
            text.GetComponent<TextMesh>().text = label;
            text.transform.localPosition = Vector3.zero + new Vector3(0, -parent.childCount + 1, 5f);
            text.name = label;

            return text;

            /*
            GameObject text = GameObject.CreatePrimitive(PrimitiveType.Quad);
            text.transform.localScale = new Vector3(4, 1, 1);
            text.transform.parent = parent;


            //SpriteRenderer sr = text.AddComponent<SpriteRenderer>() as SpriteRenderer; sr.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            //MeshRenderer mr = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
            int width = testTex.width;
            int height = testTex.height;


            Texture2D tex = new Texture2D(width, height, TextureFormat.Alpha8, false);

            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureRWrapMode.Clamp;
            tex.anisoLevel= 9;



            byte[] pixelBuffer = new byte[width * height];

            for (int i = 0; i < width * height; i++)
            {
                pixelBuffer[i] =  (byte)(testTex.GetPixel(i % width, i / width).r*255);

            } tex.LoadRawTextureData(pixelBuffer);
            tex.Apply();



            text.transform.GetComponent<MeshRenderer>().material =new Material(texMat);

            text.transform.GetComponent<MeshRenderer>().material.mainTexture = tex;*/

        }


        void RandomSphere(string name)
        {
            //  GameObject node = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //  node.transform.localScale = new Vector3(4, 1, 1);
            // GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject nodeObject = Instantiate(grabbable);
            // GameObject node = new GameObject();
            // node.AddComponent<AlignText>();
            Vector3 pos = Random.insideUnitSphere * vol;

            var node = graph.nodes[graph.nodeDict[name]];
           // Debug.Log(node.label+" "+name);
            node.labelObject = GenLabel(nodeObject.transform, node.label);
            nodeObject.name = node.label;


            nodeObject.transform.localPosition = pos;
            //node.transform.GetComponent<Renderer>().sharedMaterial = mat;
            graph.nodes[graph.nodeDict[name]].pos = pos;
            graph.nodes[graph.nodeDict[name]].nodeObject = nodeObject;
            nodeObject.transform.parent = this.transform;
            //node.transform.localScale = new Vector3(20, 20, 20);


        }

        /* void FixedSphere()
         {

             GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
             GenLabel(node.transform, graph.nodes[nodes[name]].label);
             node.transform.localPosition = pos;
             node.transform.GetComponent<Renderer>().material = mat;
             node.transform.parent = this.transform;

         }*/


        bool ProcessNode(string name, int id, bool fromEdge)
        {

            if (graph.nodeDict.ContainsKey(name)) return false;


            //dictionary for converting name to true id
            graph.nodeDict.Add(name, id);



            if (fromEdge)
            {
                //Add Nodes that are not already present in orginal data
                //TODO: use label of id
                MyNode tmp = new MyNode();
                tmp.id = name;
                tmp.label = name;
                tmp.nr = graph.nodes.Count;
                graph.nodes.Add(tmp);
                Debug.Log(name + "  " + tmp.nr);
            }
            // graph.nodes[id].edgeIndicesIn = new List<int>();

            RandomSphere(name);
            return true;

        }
        void ProcessNodes()
        {
            // List<string> tmpMathMLs = new List<string>();
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].mathml != null)
                {
                    //tmpMathMLs.Add(graph.nodes[i].mathml);
                    PData data = new PData();
                    data.math = graph.nodes[i].mathml;
                   // StartCoroutine(TestRequest(data, i));
                }

            }
            // JsonUtility.ToJson(tmpMathMLs);


            Debug.Log("Start Processing Nodes");

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                //check not required
                if(ProcessNode(graph.nodes[i].id, graph.nodeDict.Count, false))
                    graph.nodes[i].nr = i;
            }
        }


        void ProcessEdges()
        {
            graph.tmpEdges = graph.edges;
            for (int i = 0; i < graph.edges.Count; i++)
            {

                ProcessNode(graph.edges[i].from, graph.nodeDict.Count, true);
                ProcessNode(graph.edges[i].to, graph.nodeDict.Count, true);

                if (graph.nodeDict.ContainsKey(graph.edges[i].from) && graph.nodeDict.ContainsKey(graph.edges[i].to))
                {

                    if (onlyInclude && graph.edges[i].style != "graphinclude" && graph.edges[i].style != "include")
                    {
                        graph.edges.RemoveAt(i);
                        i--;
                        //graph.edges[i].active = false;

                    }
                    else

                    {
                        MyNode source = graph.nodes[graph.nodeDict[graph.edges[i].from]];
                        MyNode target = graph.nodes[graph.nodeDict[graph.edges[i].to]];
                        source.edgeIndicesOut.Add(i);
                        target.edgeIndicesIn.Add(i);

                        float weight = 1;
                        if (graph.edges[i].style != "graphinclude" && graph.edges[i].style != "include")
                        {
                            weight = 0.8f;
                            if (graph.edges[i].style == "graphmeta" || graph.edges[i].style == "meta")
                                weight = .2f;
                        }
                        source.weights.Add(weight);
                        target.weights.Add(weight);
                        source.outWeights.Add(weight);
                        target.inWeights.Add(weight);
                    }

                } 
            }

            foreach (MyNode node in graph.nodes)
            {
                List<int> edgeIndices = (node.edgeIndicesIn.Concat<int>(node.edgeIndicesOut).ToList<int>());

                /* List<Vector2Int> nodeEdgePairs = new List<Vector2Int>(edgeIndices.Count);

                 for(int i = 0; i<edgeIndices.Count;++i)
                 {
                     nodeEdgePairs[i]=new Vector2Int(edgeIndices[i], node.connectedNodes[i]);
                 }*/

         
                foreach(int eidx in node.edgeIndicesIn)
                {
                    node.connectedNodes.Add(graph.nodeDict[graph.edges[eidx].from]);
                }
                foreach (int eidx in node.edgeIndicesOut)
                {
                    node.connectedNodes.Add(graph.nodeDict[graph.edges[eidx].to]);
                }

                //multiple edges between same two nodes
                var duplicates = node.connectedNodes
                .Select((t, i) => new { Index = i, Nid = t })
                .GroupBy(g => g.Nid)
                .Where(g => g.Count() > 1);

                foreach (var duplicateGroup in duplicates)
                {
                   // Debug.Log(node.id+" "+duplicateGroup.Count());
                    int k = 0;
                    foreach (var duplicate in duplicateGroup)
                    {
                        if(node.nr!=duplicate.Nid&& graph.edges[edgeIndices[duplicate.Index]].localIdx==0)
                          //  graph.edges[edgeIndices[duplicate.Index]].localIdx =
                                k++;
                    
                    }
                    float dnum = k;

                    k = 1;
                    foreach (var duplicate in duplicateGroup)
                    {
                        if (node.nr != duplicate.Nid && graph.edges[edgeIndices[duplicate.Index]].localIdx == 0)
                        {
                            graph.edges[edgeIndices[duplicate.Index]].localIdx = k++/dnum;

                           
                        }
                        Debug.Log(node.label +" "+graph.nodes[duplicate.Nid].label + " " + duplicate.Index + " " + graph.edges[edgeIndices[duplicate.Index]].localIdx + " " + graph.edges[edgeIndices[duplicate.Index]].from + " " + graph.edges[edgeIndices[duplicate.Index]].to);


                    }
                }

                //loops
                /*
                var idx = 0;
                int same = -1;
                var count = node.connectedNodes.Count(item => item ==node.nr);
                while (idx != -1)
                {                 
                    idx = node.connectedNodes.IndexOf(node.nr,idx);
                    if (idx != -1)
                    {

                        graph.edges[edgeIndices[idx]].localIdx = (same--/count);
                        idx++;
                    }
                }*/
         
            }



        }



        class PData
        {
            public string format = "MathML";
            public string math = "";
            public bool svg = true;
            public bool mml = false;
            public bool png = false;
            public bool speakText = true;
            public string speakRuleset = "mathspeak";
            public string speakStyle = "default";
            public int ex = 6;
            public int width = 1000000;
            public bool linebreaks = false;
        };



        void Start()
        {


            GlobalVariables.Percent = Percent.GetComponent<Text>();
            GlobalVariables.EventSystem = EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();
            Camera camera = Camera.main;
            float[] distances = new float[32];
            GlobalVariables.Beam = true;
            //camera.farClipPlane = 12;
            distances[18] = 10;
            camera.layerCullDistances = distances;
            camera.layerCullSpherical = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = new Color(0.7f, 0.8f, 0.7f); 





            // Debug.Log(url);
            //float time = Time.realtimeSinceStartup;
            /* url = "file:///" + 
             * th +
                   //"/HOLLight_archive.json"
                  //"/krmt.json"
                    "/nasa.json"
                   //"/smglom_archive.json"
                   ;
        */ // if(UrlSelect.GetComponent<Dropdown>().value != GlobalVariables.SelectionIndex)
             //   UrlSelect.GetComponent<Dropdown>().value = GlobalVariables.SelectionIndex;
            Debug.Log(GlobalVariables.SelectionIndex + " found as index after start");
            //  GlobalVariables.Url = "file:///" + Application.dataPath + "/" + UrlSelect.GetComponent<Dropdown>().captionText.text + ".json";
            if (GlobalVariables.Reload&&!GlobalVariables.Init) LoadGraph();


#if UNITY_WEBGL
            Debug.Log("#################WEBGLBUILD###############################");
            int pm = Application.absoluteURL.IndexOf("?");
            if (pm != -1)
            {
                GlobalVariables.Url = "https://mmt.mathhub.info/:jgraph/json?" + Application.absoluteURL.Split("?"[0])[1];
                Debug.Log("genereted url: "+GlobalVariables.Url);
            }
#endif      

           // GlobalVariables.Url = "mmt.mathhub.info/:jgraph/json" + "?key=archivegraph&uri=MitM/smglom%20MitM/Foundation%20MMT/LFX";

        }




        IEnumerator TestRequest(PData pdata, int i)
        {
            /*List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));*/

            string formData = JsonUtility.ToJson(pdata);

            var data = System.Text.Encoding.UTF8.GetBytes(formData);


            Debug.Log(formData);
            var www = new UnityWebRequest("http://localhost:8003");
            www.method = "POST";
            www.uploadHandler = new UploadHandlerRaw(data);
            www.downloadHandler = new DownloadHandlerBuffer();

            // UnityWebRequest www = UnityWebRequest.Post("http://localhost:8003", (formData));
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Debug.Log("Request upload complete!");
                graph.nodes[i].svg = www.downloadHandler.text.Replace("ex\"", "px\"").Replace("Infinity", "0").Replace("currentColor", "white");

                Debug.Log(graph.nodes[i].svg);
                GameObject mathObject = (GameObject)Instantiate(Resources.Load("mathObject"));

                mathObject.transform.parent = graph.nodes[i].labelObject.transform;
                ImportSVG.ImportAsMesh(graph.nodes[i].svg, ref mathObject);
                mathObject.transform.localPosition = new Vector3(-mathObject.GetComponent<MeshRenderer>().bounds.max.x / 2 * 3000, -150f, 0f);
                mathObject.transform.localEulerAngles = new Vector3(180, 0, 0);
                mathObject.transform.localScale = Vector3.one * 3000;
            }
        }

        /*IEnumerator Lay()
        {

            yield return StartCoroutine(Layouts.SolveUsingForces(25, 0.13f));

            if (GlobalVariables.Solved)
            {

                Layouts.BuildHierarchy();

                graph.badHack = new List<int>();
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    var node = graph.nodes[i];

                    Vector3 pos = new Vector3(node.pos.x, node.pos.y, node.pos.z) / 4f;
                    graph.badHack.Add(i);
                    node.pos = pos;
                    node.nodeObject.transform.localPosition = pos;
                }

                graph.edgeObject = BuildEdges(graph.edges, ref graph, lineMat);

                GlobalVariables.Init = true;
                this.GetComponent<Interaction>().enabled = true;

            }
        }*/

        private IEnumerator FinishUpdate()
        {
            NativeArray<float> Energies = new NativeArray<float>(graph.nodes.Count, Allocator.Persistent);
            var handle = Layouts.BaseLayout(iterations, globalWeight, spaceScale, Energies);

            yield return new WaitUntil(() => handle.IsCompleted);
            handle.Complete();
            Layouts.Normalize(spaceScale);
            //TODO: iterate over edges instead
            foreach (MyNode node in graph.nodes)
            {
                UpdateEdges(node);
            }
            Debug.Log("update fin");
            Energies.Dispose();
        }


        private IEnumerator FinishInit(float time)
        {
            NativeArray<float> Energies = new NativeArray<float>(graph.nodes.Count, Allocator.Persistent);
            var handle = Layouts.BaseLayout(iterations, globalWeight, spaceScale, Energies);
    
           // yield return new WaitUntil(() => handle.IsCompleted);
            while (!handle.IsCompleted)
            {
                //GlobalVariables.Percent.text = ((float)(100.0f * (graph.fin)*2 / iterations)).ToString();
                GlobalVariables.Percent.text = graph.fin.ToString();
              //  if(graph.fin>1) Layouts.Normalize(spaceScale,true);
                yield return  new WaitForSeconds(.1f); 
            }
            GlobalVariables.Percent.text = "";
            Debug.Log("continue");
            handle.Complete();
            Layouts.Normalize(spaceScale);
            graph.edgeObject = BuildEdges(graph.edges, ref graph, lineMat);
            graph.edgeObject.transform.parent = transform.parent;
            graph.edgeObject.name = "EdgeMesh";
            GlobalVariables.Solved = true;
            this.GetComponent<Interaction>().enabled = true;
            GlobalVariables.Init = true;
            this.GetComponent<GlobalAlignText>().childCount = this.transform.childCount;
            //     graph.Positions.Dispose();
            //    graph.Disps.Dispose();
            Energies.Dispose();
            Debug.Log("Finished init " + (Time.realtimeSinceStartup - time));

            this.StartCoroutine(_waitUntilStable(10));


        }




        private static IEnumerator _waitUntilStable(float maxWait)
        {
            float startTime = Time.time;
            //
            // Let's aim for 75% of the target frame rate
            //
            float targetFrameTime = 1 / (Application.targetFrameRate * 0.75f);

            int consecutiveGoodFrames = 0;

            //
            // Wait for enough good consecutive frames or the max wait.
            //
            while ((Time.time - startTime < maxWait) && (consecutiveGoodFrames < 20))
            {
                yield return null;

                if (Time.deltaTime <= targetFrameTime)
                {
                    ++consecutiveGoodFrames;
                }
                else
                {
                    consecutiveGoodFrames = 0;
                }
            }


        }

        private IEnumerator ShowUpdate()
        {
            while (!GlobalVariables.Init)
            {
                if (graph.fin > 1) Layouts.Normalize(spaceScale, true);
                yield return new WaitForSeconds(.1f);
            }
    
        }

        //TODO: change
        IEnumerator ProcessJSON(WWW www)
        {
           
            if (www == null)
            {
                yield return null;
            }
                
            else
                yield return www;

            var time = Time.realtimeSinceStartup;
            Debug.Log(GlobalVariables.SelectionIndex);
            //Debug.Log(www.text);
            string json = GraphFiles[GlobalVariables.SelectionIndex].text;//;
            // check for errors
            if (www!=null&&www.error == null)
            {
                Debug.Log("WWW Ok!: " + www.text);
                json = www.text;

            }
            else if (www!=null)
            {
                Debug.Log(www.error);
            }
           
            GlobalVariables.Graph = MyGraph.CreateFromJSON(json);
            graph = GlobalVariables.Graph;
            graph.nodes = graph.nodes
            .GroupBy(customer => customer.id)
            .Select(group => group.First()).ToList();
            GlobalVariables.Vol = vol;

            Debug.Log(graph.nodes.Count + " " + graph.edges.Count);


            graph.movingNodes = new List<int>();
            graph.selectedNodes = new List<int>();
            graph.selectedNodes.Add(-1);
            graph.selectedNodes.Add(-1);

            graph.colorDict = new Dictionary<string, Color>();
            graph.colorDict.Add("include", new Color(0, 255, 0));
            graph.colorDict.Add("meta", new Color(255, 0, 0));
            graph.colorDict.Add("alignment", new Color(120, 120, 0));
            graph.colorDict.Add("view", new Color(0, 0, 255));
            graph.colorDict.Add("structure", new Color(0, 120, 120));

            /*
            graph.colorDict.Add("graphinclude", new Color(0, 255, 0));
            graph.colorDict.Add("graphmeta", new Color(255, 0, 0));
            graph.colorDict.Add("graphalignment", new Color(120, 120, 0));
            graph.colorDict.Add("graphview", new Color(0, 0, 255));
            graph.colorDict.Add("graphstructure", new Color(0, 120, 120));
            */
            //Debug.Log(graph.nodes.Count);

            graph.nodeDict = new Dictionary<string, int>();
            Debug.Log("setup time " + (Time.realtimeSinceStartup - time));

            ProcessNodes();
            ProcessEdges();

            //graph.Disps = new NativeArray<Vector3>(graph.nodes.Count,Allocator.Persistent);
            //graph.Positions = new NativeArray<Vector3>(graph.nodes.Count, Allocator.Persistent);
            identifySubgraphs();
            Debug.Log("prep time " + (Time.realtimeSinceStartup - time));

            StartCoroutine(FinishInit(time));
            StartCoroutine(ShowUpdate());
             


            }


        public static void UpdateEdgesLite(MyNode node, ReadJSON.MyGraph graph)
        {


            Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;


            Vector3 targetPos = node.nodeObject.transform.localPosition;
            Vector3 sourcePos;

            for (int i = 0; i < node.edgeIndicesIn.Count; i++)
            {
                sourcePos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesIn[i]].from]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                ReadJSON.createEdge(node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {
                targetPos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                ReadJSON.createEdge(node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();
        }



        private void UpdateEdgesFull(MyNode node)
        {

            Mesh mesh = node.nodeEdgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;

   

            Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;


            Vector3 targetPos = node.nodeObject.transform.localPosition;
            Vector3 sourcePos;

            for (int i = 0; i < node.edgeIndicesIn.Count; i++)
            {
                sourcePos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesIn[i]].from]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                ReadJSON.createEdge(node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                ReadJSON.createEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {

                targetPos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                ReadJSON.createEdge(node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                ReadJSON.createEdge(i + node.edgeIndicesIn.Count, vertices, sourcePos, targetPos, offset, offsetOrtho);
            }


            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();

            if (graph.subObject != null)
            {

                Mesh subMesh = graph.subObject.GetComponent<MeshFilter>().sharedMesh;
           
                Vector3[] subVertices = subMesh.vertices;
                int k = 0;
                foreach (int eid in graph.subEdges)
                {
                    for(int v = 0; v < 8; v++)
                    {
                        subVertices[k++] = bigVertices[eid * 8+v];
                    }
                    
                }
                subMesh.vertices = subVertices;
                subMesh.RecalculateBounds();
            }
 

        }

        //if not known if selected EdgeObjects are active
        private void UpdateEdges(MyNode node)
        {

            if (node.nodeEdgeObject != null)
            {
                UpdateEdgesFull(node);
            }
            else
            {
                UpdateEdgesLite(node, graph);
            }
        }

        private void UpdateMoving()
        {
            foreach (int n in graph.selectedNodes)
            {
                if (n == -1) continue; //TODO: change this
                var node = graph.nodes[n];
                GlobalVariables.Graph.nodes[n].pos = node.nodeObject.transform.localPosition;
                //   Debug.Log(node.nodeObject.transform.localPosition);
                UpdateEdgesFull(node);
            }
        }

        private void UpdateSelected()
        {
            foreach (int n in graph.selectedNodes)
            {
                if (n == -1) continue; //TODO: change this
                var node = graph.nodes[n];
                UpdateEdgesFull(node);
            }
        }
        public void LoadGraph()
        {
            Debug.Log("load " + GlobalVariables.Url);

            url = GlobalVariables.Url;
            si = GlobalVariables.SelectionIndex;
            GlobalVariables.CurrentFile = GraphFiles[GlobalVariables.SelectionIndex];
            GameObject.Find("UIDropdown").GetComponent<Dropdown>().value = si;
            WWW jsonUrl = new WWW(url);
            if (url == "") jsonUrl = null;
            StartCoroutine(ProcessJSON(jsonUrl));
  
        }

      
        public void ChangeID(InputField f)
        {
            int result = 0;
            if (System.Int32.TryParse(f.text, out result))
            {
                
                graph.latestSelection = result;
            }
            else
            {
            
                foreach(var p in graph.nodeDict)
                {
                    if (p.Key.IndexOf(f.text) != -1)
                    {
                        graph.latestSelection = graph.nodeDict[p.Key];
                        Debug.Log("found " + graph.latestSelection);
                        break;
                    }

                }
            
               
            }
            
        }


        public void RecalculateLayout()
        {
            Debug.Log(url + " " + GlobalVariables.Url);
            if (!GlobalVariables.Init)
            {
                LoadGraph();
            }
            else if (si != GlobalVariables.SelectionIndex)
            {
                Debug.Log("new graph, reload scene");
             //   graph.Positions.Dispose();
              //  graph.Disps.Dispose();
                GlobalVariables.Init = false;
                GlobalVariables.Reload = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

                Debug.Log("after reload");
               //  LoadGraph();
            }
            else
            {
                //Debug.Log(url + " ................ " + GlobalVariables.Url);

                // StartCoroutine(RLCoroutine());
                Debug.Log("update layout");
               // var handle = Layouts.BaseLayout(iterations, globalWeight, spaceScale);
                StartCoroutine(FinishUpdate());

           
            }

        }

        IEnumerator RLCoroutine()
        {

            /*
            var handle = Layouts.BaseLayout(iterations, globalWeight, spaceScale);

            StartCoroutine(FinishInit(handle, 0));

            //Layouts.BaseLayout(iterations, globalWeight, spaceScale);
            int k = 0;
            foreach (MyNode node in graph.nodes)
            {
                UpdateEdges(node);
                k++;
                if (k == 100)
                {
                    k = 0;
                    yield return null;
                }
            }*/
            Debug.Log("update coroutine fin");
            yield return null;

        }


        /*
                IEnumerator RecalculateLayout()
                {


                    yield return null;
                    var time = Time.realtimeSinceStartup;
                   // Layouts.Spiral();

                    //StartCoroutine(Layouts.SolveUsingForces(25, 0.13f));

                    //Layouts.SolveUsingForces(iterations, 0.13f,useWeights:true, globalWeight:globalWeight);
                    //if(buildHierarchy)Layouts.BuildHierarchy();


                    Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;

                    Vector3[] bigVertices = bigMesh.vertices;

                   // if(normalize) Layouts.Normalize(spaceScale);

                    foreach (MyNode node in graph.nodes)
                    {


                        List<int> edgeIndices = node.edgeIndicesIn.Union<int>(node.edgeIndicesOut).ToList<int>();


                        for (int i = 0; i < edgeIndices.Count; i++)
                        {
                            var sourcePos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].from]].nodeObject.transform.localPosition;
                            var targetPos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].to]].nodeObject.transform.localPosition;

                            //if (sourcePos != graph.nodes[graph.nodeDict[graph.edges[i].from]].pos || targetPos != graph.nodes[graph.nodeDict[graph.edges[i].to]].pos)
                            {
                                // Debug.Log("work");
                                Vector3 dir = targetPos - sourcePos;
                                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                                ReadJSON.createEdge(edgeIndices[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                                //  changed = true;
                            }

                        }


                    }

                    bigMesh.vertices = bigVertices;
                    bigMesh.RecalculateBounds();

                 //   Debug.Log(Time.realtimeSinceStartup - time);
                    GlobalVariables.Solved = true;
                }
        */

        // Update is called once per frame
        void Update()
        {
            /*
                        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two))
                        {
                            if (Camera.main.farClipPlane == 12) Camera.main.farClipPlane = 100;
                            else Camera.main.farClipPlane = 12;
                        }
                        if (OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four))
                        {
                            graph.edgeObject.SetActive(!graph.edgeObject.activeSelf);
                        }*/

            /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(GlobalVariables.Solved);
                if (GlobalVariables.Solved == true)
                {
                    GlobalVariables.Solved = false;
                    StartCoroutine(RecalculateLayout());
                   // globalWeight = globalWeight- 0.1f;
                   // if (globalWeight < 0) globalWeight = 1;
                }
                

            }*/
            if(Input.GetKeyDown(KeyCode.RightShift))
                CalculateSubgraph();

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("enter");
                LoadGraph();
            }
               

            if (GlobalVariables.Recalculate)
            {
                // StopCoroutine(RecalculateLayout());
                //StartCoroutine(RecalculateLayout());

                UpdateSelected();

                GlobalVariables.Recalculate = false;

            }


            if (graph.movingNodes.Count > 0)
            {
                UpdateMoving();

            }

        }
    }


}





