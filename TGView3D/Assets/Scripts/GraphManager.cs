using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using static TGraph.ReadJSON;



namespace TGraph
{

  
    public class GraphManager : MonoBehaviour
    {
        // Start is called before the first frame update


        public  ReadJSON.MyGraph Graph;
        public GameObject Percent;
        public Material mat;
        public Material lineMat;
        public GameObject grabbable;
        public GameObject URLObject;
        public bool onlyInclude = false;
        public GameObject EventSystem;
        public bool recursive = false;
        public float globalWeight;

        public int Iterations = 25;
        public float spaceScale = 1;

        public GameObject SemanticSelect;
        public GameObject ArgSolverSelect;
        int si = 0;
        public float time = 0;
        public string url;//http://neuralocean.de/graph/test/nasa.json";
        public string path;
        public int vol = 100;
        public static Dictionary<string, Vector3> nodePosDict;
        SVGCollection svgCol;
        public TextAsset SVGFile;
        static bool IsMPD = false;
        public static Color BaseColor = Color.white;
        public static Color SelectedColor = Color.cyan;
        public static Color ConnectedColor = Color.yellow;
        public static Color TargetColor = Color.red;
        public static bool IsCoq = false;
        public static bool IsAG = false;
        public static List<MyNode> FoundNodes;
        public bool Gen = false;
        private GameObject Aura;
        public float[] GlobalEnergies;
        List<int> countNodesInGraph = new List<int>();
        public GameObject NodeText;

        void Awake()
        {


            GlobalVariables.Percent = Percent.GetComponent<Text>();
            GlobalVariables.EventSystem = EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();
            Camera camera = Camera.main;
            float[] distances = new float[32];
            GlobalVariables.Beam = true;
            camera.farClipPlane = 100;
            distances[18] = 4;
            camera.layerCullDistances = distances;
            if (XRSettings.enabled) camera.layerCullSpherical = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = new Color(0.7f, 0.8f, 0.7f); 


            if (FoundNodes == null) FoundNodes = new List<MyNode>();

            GlobalVariables.GraphManager = this;
            Graph = GlobalVariables.Graph;
            Debug.Log(GlobalVariables.GraphManager);
    
        }

   
        



        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                ScreenCapture.CaptureScreenshot("Screenshot__" + System.DateTime.Now.ToString("MM-dd_HH-mm-ss") + ".png");
                Debug.Log("pic");
            }

            if (GlobalVariables.Init)
            {
                //for gestures
                /*
                if (GlobalVariables.Recalculate)
                {
                    UpdateSelected();
                    GlobalVariables.Recalculate = false;
                }*/


                if (Graph.movingNodes.Count > 0)
                {
                    UpdateMoving();
                }
            }
     

        }

  


        void identifySubGraphs()
        {

            Debug.Log("Identify SubGraphs... for node count of " + Graph.nodes.Count);
            for (var i = 0; i < Graph.nodes.Count; i++)
            {
                /* if (Graph.nodes[i].connectedNodes.Count == 0)
                 {
                     Graph.nodes.RemoveAt(i);
                     Debug.Log("delete " + i);
                     i--;
                 }*/
            }
            for (var i = 0; i < Graph.nodes.Count; i++)
            {
                Graph.nodes[i].GraphNumber = -1;
            }
            Material curMaterial = mat;
            var nodesToCheck = new Stack<int>();
            var GraphNumber = 1;
            Color randColor = mat.color;
            for (var i = 0; i < Graph.nodes.Count; i++)
            {
                var n = Graph.nodes[i];
                if (n.GraphNumber < 1)
                {

                    nodesToCheck.Push(i);
                    countNodesInGraph.Add(0);

                    while (nodesToCheck.Count > 0)
                    {
                        countNodesInGraph[countNodesInGraph.Count - 1]++;

                        var currNode = nodesToCheck.Pop();
                        if (Graph.nodes[currNode].nodeObject == null)
                        {
                            Debug.Log(currNode + " " + Graph.nodes[currNode].id + " " + Graph.nodes[currNode].label);
                        }
                        Graph.nodes[currNode].GraphNumber = GraphNumber;
                        Graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().sharedMaterial = curMaterial;
                        Graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().sharedMaterial.color = randColor;

                        for (var j = 0; j < Graph.nodes[currNode].connectedNodes.Count; j++)
                        {
                            var u = Graph.nodes[currNode].connectedNodes[j];
                            if (Graph.nodes[u].GraphNumber < 1)
                            {
                                nodesToCheck.Push(u);
                            }
                        }



                    }
                    GraphNumber++;
                    curMaterial = new Material(curMaterial);
                    randColor = Random.ColorHSV();

                }
            }
            Debug.Log("subGraphs found: " + (GraphNumber - 1));
            //  foreach (var n in countNodesInGraph) Debug.Log(n+"nodes in SubGraph");

        }

        //selects node based on ID or finds its label
        public void ChangeID(InputField f)
        {
            if (GlobalVariables.Graph.nodes.Count==0) return;
            int result = 0;
            if (System.Int32.TryParse(f.text, out result))
            {

                Graph.latestSelection = result;
            }
            else
            {
                FoundNodes.Clear();


                //exact hit
                foreach (var pair in Graph.nodeDict)
                {
                    var p = Graph.nodes[pair.Value].label;

                    if (p.ToLower() == f.text.ToLower())


                    {
                        Graph.latestSelection = Graph.nodeDict[pair.Key];
                        Debug.Log("found " + Graph.latestSelection);
                        return;
                    }

                    //break;


                }


                //contains part of search
                foreach (var pair in Graph.nodeDict)
                {
                    var p = Graph.nodes[pair.Value].label;

                    int startIdx = p.ToLower().IndexOf(f.text.ToLower());
                    if (startIdx != -1)
                    {
                        if (p[startIdx] == p.ToUpper()[startIdx] || (startIdx > 0 && p[startIdx - 1] == '_'))
                        {
                            Graph.latestSelection = Graph.nodeDict[pair.Key];
                            Debug.Log("found " + Graph.latestSelection);
                            if (!FoundNodes.Contains(Graph.nodes[Graph.nodeDict[pair.Key]]))
                                FoundNodes.Add(Graph.nodes[Graph.nodeDict[pair.Key]]);
                            return;
                        }

                        //break;
                    }

                }





            }

        }


        public void CalculateSubGraph()
        {
            for (int i = 0; i < Graph.nodes.Count; ++i)
            {
                Graph.nodes[i].pos = Graph.nodes[i].nodeObject.transform.localPosition;
            }

            if (Aura != null) GameObject.Destroy(Aura);
            Debug.Log(Graph.nodes.Count + " looking for " + Graph.latestSelection);
            if (Graph.latestSelection >= Graph.nodes.Count || Graph.latestSelection == -1)
                return;

            //remove this for coq
            if (!IsCoq) FoundNodes.Clear();
            if (FoundNodes.Count == 0) FoundNodes.Add(Graph.nodes[Graph.latestSelection]);

            foreach (var node in FoundNodes)
            {


                Debug.Log(Graph.subGraphOrign);
                //var node = Graph.nodes[Graph.latestSelection];

                //deactivate
                if (Graph.subGraphOrign == Graph.latestSelection)
                {
                    GameObject.Destroy(Graph.subObject);
                    //for aura?
                    //GameObject.Destroy(node.nodeObject.transform.GetChild(1).gameObject);

                    Graph.subGraphOrign = -1;
                    Graph.edgeObject.SetActive(true);
                }

                else
                {

                    if (Graph.subObject != null)
                    {
                        GameObject.Destroy(Graph.subObject);
                        //TODO: destroy old aura
                        // GameObject.Destroy(node.nodeObject.transform.GetChild(1).gameObject);
                    }


                    Graph.edgeObject.SetActive(false);
                    Graph.subGraphOrign = node.nr;
                    Debug.Log(node.label);
                    List<int> edgesIn = new List<int>();
                    List<int> edgesOut = new List<int>();
                    bool[] visited = new bool[Graph.nodes.Count];

                    for (int n = 0; n < Graph.nodes.Count; n++)
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
                        foreach (int idx in Graph.nodes[Graph.nodeDict[Graph.edges[idxIn].from]].edgeIndicesIn)
                        {

                            if (!visited[Graph.nodeDict[Graph.edges[idx].from]])
                            {
                                edgesIn.Add(idx);
                                visited[Graph.nodeDict[Graph.edges[idx].from]] = true;
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
                        foreach (int idx in Graph.nodes[Graph.nodeDict[Graph.edges[idxOut].to]].edgeIndicesOut)
                        {

                            if (!visited[Graph.nodeDict[Graph.edges[idx].to]])
                            {
                                edgesOut.Add(idx);
                                visited[Graph.nodeDict[Graph.edges[idx].to]] = true;
                            }

                        }

                    }

                    List<int> edgeIndices = (edgesIn.Concat<int>(edgesOut).ToList<int>());
                    List<MyEdge> edges = new List<MyEdge>();
                    foreach (int eidx in edgeIndices)
                        edges.Add(Graph.edges[eidx]);

                    Graph.subEdges = edgeIndices;
                    Graph.subObject = GraphManager.BuildEdges(edges, ref Graph, Graph.edgeObject.GetComponent<MeshRenderer>().sharedMaterial);
                    Graph.subObject.transform.parent = this.transform.GetChild(0).transform;

                    Graph.subObject.name = "subGraph";
                    Graph.subObject.transform.localPosition = Vector3.zero;
                    Graph.subObject.transform.localEulerAngles = Vector3.zero;
                    Aura = Instantiate(Resources.Load("Aura")) as GameObject;
                    Debug.Log(Aura);
                    Aura.transform.parent = node.nodeObject.transform;
                    Aura.transform.position = node.pos;

                    //if (GameObject.Find("VR") == null)
                    //    Camera.main.transform.LookAt(node.pos);


                }
                if (!IsCoq) break;
            }

        }



        //Graphbuilding stuff



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

        public static void createEdge(List<ReadJSON.MyEdge> edges, int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho)
        {

            Vector3 next = 2 * (Quaternion.AngleAxis((360 * edges[i].localIdx), targetPos - sourcePos) * offset);
            if (edges[i].localIdx <= 0) next *= 0;
            createStraightEdge(i, vertices, sourcePos + next, targetPos + next, offset, offsetOrtho);

        }



        public static Color GenerateTargetColor(Color color)
        {

            if (IsMPD) return new Color(0, 100, 0);
            return color / 40 / 3;
        }

        public static Color GenerateOriginColor(Color color)
        {

            if (IsMPD) return new Color(0, 100, 0);
            return (new Color(255, 255, 255) + color * 3) / 4 / 4;
        }

        public static GameObject BuildEdges(List<MyEdge> edges, ref MyGraph Graph, Material lineMat)
        {

            var nodes = Graph.nodeDict;
            int tubeCount = edges.Count;
            int controlPoints = 1;
            int polyType = 4;

            /* foreach (var edge in Graph.edges)
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
                 if (Graph.edges[i].localIdx > 0)
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
                    MyNode source = Graph.nodes[nodes[edges[i].from]];
                    MyNode target = Graph.nodes[nodes[edges[i].to]];

                    Vector3 dir = target.pos - source.pos;

                    Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                    Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;

                    //random
                    /*float a = Random.value < .5 ? 1 : -1;
                    float b = Random.value < .5 ? 1 : -1;

                    float alpha = Random.Range(0f, 1f);
                    Vector3 next = 7*(alpha * (a*offset - b*offsetOrtho) + offsetOrtho);
                    */


                    //Debug.Log(edges[i].style);
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = GenerateOriginColor(TGraph.ReadJSON.ColorDict[edges[i].style]);
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = GenerateTargetColor(TGraph.ReadJSON.ColorDict[edges[i].style]);

                    //creates square tubes by setting vertices manually

                    //  if (edges[i].localIdx > 0)
                    //     Debug.Log(edges[i].localIdx+" "+next * 100+edges[i].from);
                    /*
                    if (edges[i].localIdx > 0)
                        createEdge(Graph.edges,i, vertices, source.pos+next, target.pos+next, offset, offsetOrtho);
                    else*/
                    createEdge(Graph.edges, i, vertices, source.pos, target.pos, offset, offsetOrtho);
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
            mf.sharedMesh = mf.sharedMesh;

            mesh.RecalculateBounds();

            return line;

        }




        public GameObject GenLabel(Transform parent, string label, string type)
        {
            GameObject text = (GameObject)Instantiate(NodeText);

            text.transform.parent = parent;
            text.GetComponent<TextMesh>().text = label;
            if (type == "o") text.GetComponent<TextMesh>().color = Color.gray;
            text.transform.localPosition = Vector3.zero + new Vector3(0, 0, 1f);
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


            var node = Graph.nodes[Graph.nodeDict[name]];
            // Debug.Log(node.label+" "+name);
            GameObject nodeObject;
            if (node.style == "model")
            {
                nodeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nodeObject.transform.localScale *= 0.1f;// 0.070f;
            }
            else nodeObject = Instantiate(grabbable);
        

            Vector3 pos = Random.insideUnitSphere * vol;

            if (GlobalVariables.IdToPosition.ContainsKey(name) && GlobalVariables.IdToPosition[name] != null) pos = GlobalVariables.IdToPosition[name];
            node.labelObject = GenLabel(nodeObject.transform, node.label, node.style);
            nodeObject.name = node.label;


            nodeObject.transform.localPosition = pos;
            //node.transform.GetComponent<Renderer>().sharedMaterial = mat;
            Graph.nodes[Graph.nodeDict[name]].pos = pos;
            Graph.nodes[Graph.nodeDict[name]].nodeObject = nodeObject;
            nodeObject.transform.parent = transform.GetChild(0).GetChild(0);
      
            //node.transform.localScale = new Vector3(20, 20, 20);


        }


        bool ProcessNode(string name, int id, MyEdge edge)
        {

            if (Graph.nodeDict.ContainsKey(name)) return false;


            //dictionary for converting name to true id
            Graph.nodeDict.Add(name, id);



            if (edge != null)
            {
                //Add Nodes that are not already present in orginal data
                //TODO: use label of id
                MyNode tmp = new MyNode();
                tmp.id = name;
                tmp.label = name;
                tmp.nr = Graph.nodes.Count;
                tmp.generated = true;
                tmp.radius = edge.targetCount * 10;
                //      Debug.Log(tmp.radius);
                Graph.nodes.Add(tmp);


                // Debug.Log(name + "  " + tmp.nr);
            }
            // Graph.nodes[id].edgeIndicesIn = new List<int>();

            RandomSphere(name);
            return true;

        }

        void ProcessNodes()
        {

            for (int i = 0; i < Graph.nodes.Count; i++)
            {
                //check not required

                if (ProcessNode(Graph.nodes[i].id, Graph.nodeDict.Count, null))
                {
                    Graph.nodes[i].nr = i;
                    // Debug.Log(Graph.nodes[i].label + " " + i);
                }


            }

            /*
            string json = SVGFile.text;//;
            string[] svgs = JsonUtility.FromJson<SVGCollection>(json).svgs;

            // List<string> tmpMathMLs = new List<string>();
            for (int i = 0; i < Graph.nodes.Count; i++)
            {
                if (Graph.nodes[i].mathml != null && Graph.nodes[i].mathml != "")
                {
                    //tmpMathMLs.Add(Graph.nodes[i].mathml);
                    // PData data = new PData();
                    //   data.math = Graph.nodes[i].mathml;
                    // StartCoroutine(TestRequest(data, i));
                    Debug.Log(Graph.nodes[i].mathml);
                    Graph.nodes[i].svg = svgs[i];
                    CreateMathObject(i);
                }

            }*/
            // JsonUtility.ToJson(tmpMathMLs);





        }


        public void AddEdge(MyNode start, MyNode end)
        {
            GameObject myLine = new GameObject();
            myLine.transform.parent =start.nodeObject.transform.GetChild(0);
           // myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
      
            lr.material = lineMat;
            lr.material.color = Color.white;
             var LineUpdater = myLine.AddComponent<SingleLine>();
            LineUpdater.Target = end.nodeObject.transform;
            LineUpdater.Origin = start.nodeObject.transform;
            //  lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.SetColors(Color.red, Color.red);
            lr.SetWidth(0.1f, 0.01f);
            lr.SetPosition(0, start.nodeObject.transform.position);
            lr.SetPosition(1, end.nodeObject.transform.position);

        }


        public void BlendEdge(MyNode start, MyNode end)
        {
            GameObject myLine = new GameObject();
            myLine.transform.parent = this.transform.GetChild(0);
            // myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = lineMat;
            lr.material.color = Color.white;
            var LineUpdater = myLine.AddComponent<SingleLine>();
            LineUpdater.Target = end.nodeObject.transform;
            LineUpdater.Origin = start.nodeObject.transform;
            //  lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.SetColors(Color.cyan, Color.cyan);
            lr.SetWidth(0.1f, 0.1f);
            lr.SetPosition(0, start.nodeObject.transform.position);
            lr.SetPosition(1, end.nodeObject.transform.position);

        }




        /*
        void AddEdge()
        {
            var edge = new MyEdge();
            Graph.edges.Add(edge);
            ProcessEdge(Graph.edges.Count - 1);
            PlaceEdge(Graph.nodes[Graph.nodeDict[edge.from]]);
            PlaceEdge(Graph.nodes[Graph.nodeDict[edge.to]]);

        }


        void ProcessEdge(int i)
        {

        }*/

        public void PlaceEdge( MyNode node)
        {
            List<int> edgeIndices = (node.edgeIndicesIn.Concat<int>(node.edgeIndicesOut).ToList<int>());

            /* List<Vector2Int> nodeEdgePairs = new List<Vector2Int>(edgeIndices.Count);

             for(int i = 0; i<edgeIndices.Count;++i)
             {
                 nodeEdgePairs[i]=new Vector2Int(edgeIndices[i], node.connectedNodes[i]);
             }*/


            foreach (int eidx in node.edgeIndicesIn)
            {
                node.connectedNodes.Add(Graph.nodeDict[Graph.edges[eidx].from]);
            }
            foreach (int eidx in node.edgeIndicesOut)
            {
                node.connectedNodes.Add(Graph.nodeDict[Graph.edges[eidx].to]);
            }

            //multiple edges between same two nodes
            var duplicates = node.connectedNodes
            .Select((t, i) => new { Index = i, Nid = t })
            .GroupBy(g => g.Nid)
            .Where(g => g.Count() > 1);

            foreach (var duplicateGroup in duplicates)
            {

                int k = 0;
                foreach (var duplicate in duplicateGroup)  //count duplicates
                {
                    if (node.nr != duplicate.Nid )//&& Graph.edges[edgeIndices[duplicate.Index]].localIdx == 0)
                        //  Graph.edges[edgeIndices[duplicate.Index]].localIdx =
                        k++;

                }
                float dnum = k;


                k = 1;
                foreach (var duplicate in duplicateGroup)
                {
                    if (node.nr != duplicate.Nid)// && Graph.edges[edgeIndices[duplicate.Index]].localIdx == 0)
                    {


                        if (duplicate.Index < node.edgeIndicesIn.Count)
                            Graph.edges[edgeIndices[duplicate.Index]].localIdx = k / dnum - 0.25f;
                        else
                            Graph.edges[edgeIndices[duplicate.Index]].localIdx = Mathf.Repeat((k / dnum) + 0.5f + 0.25f, 1f);



                        k++;

                       // Debug.LogWarning(k / dnum + "----" + node.id + " " + duplicateGroup.Count() + " " + duplicate.Index + " " + node.edgeIndicesIn.Count);


                        Debug.Log(Graph.edges[edgeIndices[duplicate.Index]].localIdx);

                    }
                    // Debug.Log(node.label +" "+Graph.nodes[duplicate.Nid].label + " " + duplicate.Index + " " + Graph.edges[edgeIndices[duplicate.Index]].localIdx + " " + Graph.edges[edgeIndices[duplicate.Index]].from + " " + Graph.edges[edgeIndices[duplicate.Index]].to);
                    //TODO up here

                }
            }

            //loops

            var idx = 0;
            float same = -1;
            float count = node.connectedNodes.Count(item => item == node.nr);
            // while (idx != -1)
            {
                idx = node.connectedNodes.IndexOf(node.nr, idx);
                if (idx != -1)
                {

                    Debug.Log("SAME");
                    GameObject torus = GameObject.Instantiate(Resources.Load("Torus")) as GameObject;
                    node.nodeObject.transform.Rotate(Vector3.up, 360 * -1f * same / count);
                    torus.transform.parent = node.nodeObject.transform;
                    torus.transform.localPosition = new Vector3(1, 0, 0);
                    torus.transform.localScale = new Vector3(100, 100, 100);
                    torus.GetComponent<Renderer>().material = new Material(mat);
                    torus.GetComponent<Renderer>().material.color =
                    // new Color(20/255f,20/255f,140/255f);
                   TGraph.ReadJSON.ColorDict[Graph.edges[edgeIndices[idx]].style] / 255f;


                    Graph.edges[edgeIndices[idx]].localIdx = (same-- / count);

                    idx++;
                }
            }
    }


        void ProcessEdges()
        {
            Graph.tmpEdges = Graph.edges;
            for (int i = 0; i < Graph.edges.Count; i++)
            {
                //add nodes if not already present
                ProcessNode(Graph.edges[i].from, Graph.nodeDict.Count, Graph.edges[i]);
                ProcessNode(Graph.edges[i].to, Graph.nodeDict.Count, Graph.edges[i]);


                //useless?
                if (Graph.nodeDict.ContainsKey(Graph.edges[i].from) && Graph.nodeDict.ContainsKey(Graph.edges[i].to))
                {

                    if (onlyInclude && Graph.edges[i].style != "Graphinclude" && Graph.edges[i].style != "include")
                    {
                        Graph.edges.RemoveAt(i);
                        i--;
                        //Graph.edges[i].active = false;

                    }
                    else //set weights... TODO change this
                    {
                        MyNode source = Graph.nodes[Graph.nodeDict[Graph.edges[i].from]];
                        MyNode target = Graph.nodes[Graph.nodeDict[Graph.edges[i].to]];
                        source.edgeIndicesOut.Add(i);
                        target.edgeIndicesIn.Add(i);

                        float weight = 1;
                   
                        if (Graph.edges[i].style != "Graphinclude" && Graph.edges[i].style != "include")
                        {
                            weight = 0.8f;
                            if (Graph.edges[i].style == "Graphmeta" || Graph.edges[i].style == "meta")
                            {
                                weight = .2f;
                         
                            }
                        }
                        source.weights.Add(weight);
                        target.weights.Add(weight);
                        source.outWeights.Add(weight);
                        target.inWeights.Add(weight);

                      //  PlaceEdge(source);
                      //  PlaceEdge(target)

                    }

                }
            }

            foreach (MyNode node in Graph.nodes)
            {
                PlaceEdge(node);
            }



        }


        public static void CreateMathObject(int i)
        {

            //BUGDANGER
            var Graph = GlobalVariables.Graph;
            GameObject mathObject = (GameObject)Instantiate(Resources.Load("mathObject"));

            var parentTransform = Graph.nodes[i].labelObject.transform;


            ImportSVG.ImportAsMesh(Graph.nodes[i].svg, ref mathObject);
            var y = 0f;
          
            if (Graph.nodes[i].label != "")
            {
                mathObject.transform.parent = parentTransform;
                y -= 50f;
                mathObject.transform.localPosition = new Vector3(-mathObject.GetComponent<MeshRenderer>().bounds.max.x / 2 * 3000, y, 0f);
                mathObject.transform.localScale = Vector3.one * 3000;
            }
            else
            {


                parentTransform.localScale = Vector3.one;
                mathObject.transform.parent = parentTransform;
                mathObject.transform.localPosition = Vector3.zero;
                parentTransform.localPosition = new Vector3(0, 0, 3);
                mathObject.transform.localScale = Vector3.one * 60;
            }

            mathObject.transform.localEulerAngles = new Vector3(180, 0, 0);

        }


        public IEnumerator UpdateLoop(int iterations, NativeArray<float> Energies)
        {
            var stime = Time.realtimeSinceStartup;
            int k = iterations;

            JobHandle handle = new JobHandle();

#if UNITY_WEBGL && !UNITY_EDITOR
                k = iterations;
                
#endif
            for (int p = 0; p < iterations; p += k)
            {
                handle = Layouts.UpdateLayout(k, globalWeight, spaceScale);


                Debug.Log("Begin Layout " + ((Time.realtimeSinceStartup - time)));


                //   while (!handle.IsCompleted)

                GlobalVariables.Percent.text = Graph.fin.ToString();
#if !UNITY_WEBGL || UNITY_EDITOR

                do
                {
                    //GlobalVariables.Percent.text = ((float)(100.0f * (Graph.fin)*2 / iterations)).ToString();
                    GlobalVariables.Percent.text = Graph.fin.ToString();

                    if (Graph.fin > 1)
                    {
                        Layouts.Normalize(spaceScale, true);
                        //Debug.Log((Time.realtimeSinceStartup-time));
                        UpdateAllEdges();
                    }


                    yield return null;
                } while (!handle.IsCompleted);

#endif

                handle.Complete();
                yield return null;
            }

           // Graph.fin = 0;
            GlobalVariables.Percent.text = "";
            handle.Complete();
            Layouts.Normalize(spaceScale);

            GlobalEnergies = new float[Energies.Length];
            Energies.CopyTo(GlobalEnergies);
            Energies.Dispose();

            UpdateAllEdges();
            Debug.Log(Time.realtimeSinceStartup - stime);
           
            
            
        }



        public IEnumerator FinishUpdate()
        {
           
            NativeArray<float> Energies = new NativeArray<float>(Graph.nodes.Count, Allocator.Persistent);
            for(int i = 0; i< Energies.Length;++i)
            {
                Energies[i] = 0;
            }
            var handle = Layouts.BaseLayout(0, globalWeight, spaceScale, Energies);
            handle.Complete();

            yield return StartCoroutine(UpdateLoop(Iterations,Energies));

 

        }



        public IEnumerator SmallUpdate()
        {
            Layouts.VolumeWidth /= Layouts.Scaler;
            Layouts.ToTwoD(GlobalVariables.TwoD);
            Layouts.Scaler = 1;
            Layouts.step = .1f; Mathf.Max(Layouts.step,(.5f+Layouts.step/2));
            NativeArray<float> Energies = new NativeArray<float>(Graph.nodes.Count, Allocator.Persistent);
            Layouts.InitEnergies(Energies);


            yield return StartCoroutine(UpdateLoop(4,Energies));


        }


        public IEnumerator FinishInit(bool keepLayout = false)
        {
            time = Time.realtimeSinceStartup;
            Layouts.Init(GlobalVariables.TwoD);
            if (!keepLayout)
            {
                
                Layouts.Spiral();
              
            }
          

            Graph.edgeObject = BuildEdges(Graph.edges, ref Graph, lineMat);
            Graph.edgeObject.tag = "Edge";
           
            
            Graph.edgeObject.transform.parent = transform.GetChild(0);
            Graph.edgeObject.name = "EdgeMesh";
            UIInteracton.SEnableEdgeType("meta");
       
            if(!keepLayout) yield return FinishUpdate();


            GlobalVariables.Solved = true;
            this.GetComponent<Interaction>().enabled = true;
            GlobalVariables.Init = true;
            GlobalVariables.NodeCount = this.transform.childCount;
            if (!XRSettings.enabled) GameObject.Find("CameraMain").GetComponent<Gestures>().Init();

            Debug.Log("Finished init " + ((Time.realtimeSinceStartup - time)));
 
        }


        public void ProcessGraph()
        {
            ProcessNodes();
            ProcessEdges();

            /*
         
            */
            identifySubGraphs();
        
        }

        public void AddNode(string uri)
        {

            if(ProcessNode(uri, Graph.nodeDict.Count, null))
            {
                var node = Graph.nodes.Last();

                var mousePos = Input.mousePosition;
              

       
                var spawnPos = Camera.main.ScreenToWorldPoint(mousePos) ;

                spawnPos += Camera.main.transform.forward * Camera.main.nearClipPlane * 2;
                

                node.nodeObject.transform.position = spawnPos;

            }
            else
            {
                Debug.LogError("redundant id");
            }
        }



        public IEnumerator LoadLayout(WWW www)
        {
            if (www == null)
            {
                yield return null;
            }

            else
                yield return www;

            if (www != null && www.error == null)
            {
                Debug.Log("WWW Ok!: " + www.text);
                var json = www.text;
                var nodePosArray = JSONDict.CreateFromJSON(json).keysAndPositions;
                nodePosDict = new Dictionary<string, Vector3>();
                foreach (var idAndPos in nodePosArray)
                {
                    nodePosDict.Add(idAndPos.id, idAndPos.pos);
                }

            }
            else if (www != null)
            {
                Debug.Log(www.error);
            }


        }


        //Graph updating stuff


        //efficient
        private void UpdateAllEdges()
        {
            Mesh bigMesh = Graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;
            for (int i = 0; i < Graph.edges.Count; i++)
            {
                var sourcePos = Graph.nodes[Graph.nodeDict[Graph.edges[i].from]].nodeObject.transform.localPosition;
                var targetPos = Graph.nodes[Graph.nodeDict[Graph.edges[i].to]].nodeObject.transform.localPosition;
                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;
                GraphManager.createEdge(Graph.edges, i, bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }
            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();
           
            

        }



        public static void UpdateEdgesLite(MyNode node, ReadJSON.MyGraph Graph)
        {


            Mesh bigMesh = Graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;


            Vector3 targetPos = node.nodeObject.transform.localPosition;
            Vector3 sourcePos;

            for (int i = 0; i < node.edgeIndicesIn.Count; i++)
            {
                sourcePos = Graph.nodes[Graph.nodeDict[Graph.edges[node.edgeIndicesIn[i]].from]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;
                GraphManager.createEdge(Graph.edges, node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {
                targetPos = Graph.nodes[Graph.nodeDict[Graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;
                GraphManager.createEdge(Graph.edges, node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();

            
            if (Graph.subObject != null)
            {

                Mesh subMesh = Graph.subObject.GetComponent<MeshFilter>().sharedMesh;

                Vector3[] subVertices = subMesh.vertices;
                int k = 0;
                foreach (int eid in Graph.subEdges)
                {
                    for (int v = 0; v < 8; v++)
                    {
                        subVertices[k++] = bigVertices[eid * 8 + v];
                    }

                }
                subMesh.vertices = subVertices;
                subMesh.RecalculateBounds();
            }

        }


        private void UpdateEdgesFull(MyNode node)
        {

            Mesh mesh = node.nodeEdgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;



            Mesh bigMesh = Graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;


            Vector3 targetPos = node.nodeObject.transform.localPosition;
            Vector3 sourcePos;

            for (int i = 0; i < node.edgeIndicesIn.Count; i++)
            {
                sourcePos = Graph.nodes[Graph.nodeDict[Graph.edges[node.edgeIndicesIn[i]].from]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;

                GraphManager.createEdge(Graph.edges, node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                GraphManager.createEdge(Graph.edges, i, vertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {

                targetPos = Graph.nodes[Graph.nodeDict[Graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * Graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * Graph.lineWidth;

                GraphManager.createEdge(Graph.edges, node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                GraphManager.createEdge(Graph.edges, i + node.edgeIndicesIn.Count, vertices, sourcePos, targetPos, offset, offsetOrtho);
            }


            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();
   
            if (Graph.subObject != null)
            {

                Mesh subMesh = Graph.subObject.GetComponent<MeshFilter>().sharedMesh;

                Vector3[] subVertices = subMesh.vertices;
                int k = 0;
                foreach (int eid in Graph.subEdges)
                {
                    for (int v = 0; v < 8; v++)
                    {
                        subVertices[k++] = bigVertices[eid * 8 + v];
                    }

                }
                subMesh.vertices = subVertices;
                subMesh.RecalculateBounds();


            }
           // Debug.Log(node.id + " full update");

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
                UpdateEdgesLite(node, Graph);
            }
        }

        private void UpdateMoving()
        {
            foreach (int n in Graph.selectedNodes)
            {
                if (n == -1) continue; //TODO: change this
                var node = Graph.nodes[n];
           //    if(node.nodeObject!=null)
                    GlobalVariables.Graph.nodes[n].pos = node.nodeObject.transform.localPosition;
                //   Debug.Log(node.nodeObject.transform.localPosition);
                UpdateEdges(node);
            }
        }

        private void UpdateSelected()
        {
            foreach (int n in Graph.selectedNodes)
            {
                if (n == -1) continue; //TODO: change this
                var node = Graph.nodes[n];
                UpdateEdges(node);
            }
        }


    }
}