
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using TMPro;

namespace TGraph
{


    public class ReadJSON : MonoBehaviour
    {

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


        public string url = "http://neuralocean.de/graph/test/nasa.json";
        public int vol = 100;

     


        [System.Serializable]
        public class MyGraph
        {
            public List<MyNode> nodes;
            public Dictionary<string, int> nodeDict;
            public List<MyEdge> edges;
            public List<MyEdge> tmpEdges;
            public int handIndex = 0;
            public List<int> selectedNodes;
            public List<int> movingNodes;
            public int latestSelection;
            public GameObject edgeObject;
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
            public GameObject line;
            public GameObject labelObject;
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
            Debug.Log("Identify Subgraphs... " + graph.nodes.Count);
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                graph.nodes[i].graphNumber = -1;
            }
            Material curMaterial = mat;
            var nodesToCheck = new Stack<int>();
            var graphNumber = 1;
            Color randColor = Color.blue;
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
            Debug.Log("subgraphs found: "+ (graphNumber - 1));
            foreach (var n in countNodesInGraph) Debug.Log(n);
        
        }



        public static void setTriangles(int i, int[] triangles)
        {
            
            //maybe do loops instead...
            int n = 48;

            triangles[0 + i * n] = 0 + i * 8;
            triangles[1 + i * n] = 1 + i * 8;
            triangles[2 + i * n] = 2 + i * 8;
            triangles[3 + i * n] = 2 + i * 8;
            triangles[4 + i * n] = 1 + i * 8;
            triangles[5 + i * n] = 3 + i * 8;

            triangles[6 + i * n] = 3 + i * 8;
            triangles[7 + i * n] = 7 + i * 8;
            triangles[8 + i * n] = 2 + i * 8;
            triangles[9 + i * n] = 2 + i * 8;
            triangles[10 + i * n] = 7 + i * 8;
            triangles[11 + i * n] = 6 + i * 8;


            triangles[12 + 0 + i * n] = 6 + i * 8;
            triangles[12 + 1 + i * n] = 7 + i * 8;
            triangles[12 + 2 + i * n] = 4 + i * 8;
            triangles[12 + 3 + i * n] = 4 + i * 8;
            triangles[12 + 4 + i * n] = 7 + i * 8;
            triangles[12 + 5 + i * n] = 5 + i * 8;

            triangles[12 + 6 + i * n] = 5 + i * 8;
            triangles[12 + 7 + i * n] = 1 + i * 8;
            triangles[12 + 8 + i * n] = 4 + i * 8;
            triangles[12 + 9 + i * n] = 4 + i * 8;
            triangles[12 + 10 + i * n] = 1 + i * 8;
            triangles[12 + 11 + i * n] = 0 + i * 8;



            triangles[24+ 0 + i * n] = 0 + i * 8;
            triangles[24 + 1 + i * n] = 2 + i * 8;
            triangles[24 + 2 + i * n] = 1 + i * 8;
            triangles[24 + 3 + i * n] = 1 + i * 8;
            triangles[24 + 4 + i * n] = 2 + i * 8;
            triangles[24 + 5 + i * n] = 3 + i * 8;

            triangles[24 + 6 + i * n] = 3 + i * 8;
            triangles[24 + 7 + i * n] = 2 + i * 8;
            triangles[24 + 8 + i * n] = 7 + i * 8;
            triangles[24 + 9 + i * n] = 7 + i * 8;
            triangles[24 + 10 + i * n] = 2 + i * 8;
            triangles[24 + 11 + i * n] = 6 + i * 8;


            triangles[24 + 12 + 0 + i * n] = 6 + i * 8;
            triangles[24 + 12 + 1 + i * n] = 4 + i * 8;
            triangles[24 + 12 + 2 + i * n] = 7 + i * 8;
            triangles[24 + 12 + 3 + i * n] = 7 + i * 8;
            triangles[24 + 12 + 4 + i * n] = 4 + i * 8;
            triangles[24 + 12 + 5 + i * n] = 5 + i * 8;

            triangles[24 + 12 + 6 + i * n] = 5 + i * 8;
            triangles[24 + 12 + 7 + i * n] = 4 + i * 8;
            triangles[24 + 12 + 8 + i * n] = 1 + i * 8;
            triangles[24 + 12 + 9 + i * n] = 1 + i * 8;
            triangles[24 + 12 + 10 + i * n] = 4 + i * 8;
            triangles[24 + 12 + 11 + i * n] = 0 + i * 8;

        }


        public static void createEdge(int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho)
        {
            vertices[0 + i * 8] = sourcePos + offset + offsetOrtho;
            vertices[1 + i * 8] = targetPos + offset + offsetOrtho;

            vertices[2 + i * 8] = sourcePos + offset - offsetOrtho;
            vertices[3 + i * 8] = targetPos + offset - offsetOrtho;


            vertices[4 + i * 8] = sourcePos - offset + offsetOrtho;
            vertices[5 + i * 8] = targetPos - offset + offsetOrtho;

            vertices[6 + i * 8] = sourcePos - offset - offsetOrtho;
            vertices[7 + i * 8] = targetPos - offset - offsetOrtho;




        }

        public static GameObject BuildEdges(List<MyEdge> edges, ref MyGraph graph, Material lineMat)
        {

            var nodes = graph.nodeDict;
            Vector3[] vertices = new Vector3[2*4 * edges.Count];
            int[] triangles = new int[2*2*2 * 6 * edges.Count];
            Color[] vertexColors = new Color[2*4 * edges.Count];
            GameObject line = new GameObject();
            MeshRenderer mr = line.AddComponent<MeshRenderer>();
            MeshFilter mf = line.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

        
            for (int i = 0; i < edges.Count; i++)
            {
                 if (nodes.ContainsKey(edges[i].from) && nodes.ContainsKey(edges[i].to))
                {
                    MyNode source = graph.nodes[nodes[edges[i].from]];
                    MyNode target = graph.nodes[nodes[edges[i].to]];

                    Vector3 dir = target.pos - source.pos;

                    Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                    Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                    //Debug.Log(edges[i].style);
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = graph.colorDict[edges[i].style];
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = graph.colorDict[edges[i].style]+ new Color(0,0,255);

                    //creates square tubes by setting vertices manually
                    createEdge(i,vertices, source.pos,target.pos, offset, offsetOrtho);
                    setTriangles(i, triangles);

            

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
            node.labelObject=GenLabel(nodeObject.transform, node.label);
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


        void ProcessNode(string name, int id, bool fromEdge)
        {

            if (graph.nodeDict.ContainsKey(name)) return;


            //dictionary for converting name to true id
            graph.nodeDict.Add(name, id);



            if (fromEdge)
            {
                //Add Nodes that are not already present in orginal data

                MyNode tmp = new MyNode();
                tmp.id = name;
                tmp.label = name;
                graph.nodes.Add(tmp);
            }
            // graph.nodes[id].edgeIndicesIn = new List<int>();

            RandomSphere(name);

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
                    StartCoroutine(TestRequest(data,i));
                }

            }
            // JsonUtility.ToJson(tmpMathMLs);


            Debug.Log("Start Processing Nodes");

            for (int i = 0; i < graph.nodes.Count; i++)
            {
             
                graph.nodes[i].nr = i;
                ProcessNode(graph.nodes[i].id, graph.nodeDict.Count, false);

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

                    if (onlyInclude&&graph.edges[i].style != "graphinclude" && graph.edges[i].style != "include")
                    {
                         graph.edges.RemoveAt(i);
                         i--;

                    }
                    else
                    
                    {
                        MyNode source = graph.nodes[graph.nodeDict[graph.edges[i].from]];
                        MyNode target = graph.nodes[graph.nodeDict[graph.edges[i].to]];
                        source.edgeIndicesOut.Add(i);
                        target.edgeIndicesIn.Add(i);

                        source.connectedNodes.Add(graph.nodeDict[graph.edges[i].to]);
                        target.connectedNodes.Add(graph.nodeDict[graph.edges[i].from]);
                        float weight = 1;
                        if (graph.edges[i].style != "graphinclude" && graph.edges[i].style != "include")
                        {
                            weight = 0.8f;
                            if (graph.edges[i].style == "graphmeta" || graph.edges[i].style =="meta")
                                weight = 0.2f;
                        }
                        source.weights.Add(weight);
                        target.weights.Add(weight);
                        source.outWeights.Add(weight);
                        target.inWeights.Add(weight);
                    }

                }
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


            GlobalVariables.EventSystem = EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();
            Camera camera = Camera.main;
            float[] distances = new float[32];

            //camera.farClipPlane = 12;
            distances[18] = 10;
            camera.layerCullDistances = distances;
            camera.layerCullSpherical = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = new Color(0.7f, 0.8f, 0.7f); 





            // Debug.Log(url);
            //float time = Time.realtimeSinceStartup;
              url = "file:///" + Application.dataPath +
                   //"/HOLLight_archive.json"
                  "/krmt.json"
                    //"/nasa.json"
                   //"/smglom_archive.json"
                   ;
                   

            WWW www =  new WWW(url);



            StartCoroutine(ProcessJSON(www));


  


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
                graph.nodes[i].svg = www.downloadHandler.text.Replace("ex\"", "px\"").Replace("Infinity","0").Replace("currentColor","white");

                Debug.Log(graph.nodes[i].svg);
                GameObject mathObject =(GameObject) Instantiate(Resources.Load("mathObject"));

                mathObject.transform.parent = graph.nodes[i].labelObject.transform;
                ImportSVG.ImportAsMesh(graph.nodes[i].svg, ref mathObject);
                mathObject.transform.localPosition = new Vector3(- mathObject.GetComponent<MeshRenderer>().bounds.max.x/2*3000,-150f,0f);
                mathObject.transform.localEulerAngles = new Vector3(180,0,0);
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

        IEnumerator ProcessJSON(WWW www)
        {
            yield return www;


            //Debug.Log(www.text);


            // check for errors
           if (www.error == null)
            {
            //    Debug.Log("WWW Ok!: " + www.text);
                graph = MyGraph.CreateFromJSON(www.text);
                GlobalVariables.Graph = graph;
                GlobalVariables.Vol = vol;

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

                graph.colorDict.Add("graphinclude", new Color(0, 255, 0));
                graph.colorDict.Add("graphmeta", new Color(255, 0, 0));
                graph.colorDict.Add("graphalignment", new Color(120, 120, 0));
                graph.colorDict.Add("graphview", new Color(0, 0, 255));
                graph.colorDict.Add("graphstructure", new Color(0, 120, 120));

                //Debug.Log(graph.nodes.Count);

                graph.nodeDict = new Dictionary<string, int>();

                ProcessNodes();
             

                ProcessEdges();
                identifySubgraphs();
                Layouts.Spiral();
                //StartCoroutine(Lay());
                Layouts.BuildHierarchy();
                Layouts.SolveUsingForces(iterations, 0.13f, useWeights: true, globalWeight: globalWeight);

                {

                   

                    
                    Layouts.Normalize(spaceScale);

                    graph.edgeObject = BuildEdges(graph.edges, ref graph, lineMat);

                    GlobalVariables.Init = true;
                    this.GetComponent<Interaction>().enabled = true;

                }


                GlobalVariables.Solved = true;

            }

            this.GetComponent<GlobalAlignText>().childCount = this.transform.childCount;

        }

        private void UpdatePosition(List<int> nodes)
        {
           
                foreach (int n in nodes)
                {

                    if (n == -1) continue;
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

                        //if (sourcePos != graph.nodes[graph.nodeDict[graph.edges[i].from]].pos || targetPos != graph.nodes[graph.nodeDict[graph.edges[i].to]].pos)
                        {
                            // Debug.Log("work");
                            Vector3 dir = targetPos - sourcePos;
                            Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                            Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                            ReadJSON.createEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);
                            ReadJSON.createEdge(edgeIndices[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                            //  changed = true;
                        }

                    }
                    //  if (changed)
                    {
                        mesh.vertices = vertices;
                        bigMesh.vertices = bigVertices;
                        mesh.RecalculateBounds();
                        bigMesh.RecalculateBounds();
                    }
                }
            
      
        }

        IEnumerator RecalculateLayout()
        {


            yield return null;
            var time = Time.realtimeSinceStartup;
           // Layouts.Spiral();

            //StartCoroutine(Layouts.SolveUsingForces(25, 0.13f));

            Layouts.SolveUsingForces(iterations, 0.13f,useWeights:true, globalWeight:globalWeight);
            if(buildHierarchy)Layouts.BuildHierarchy();

          
            Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;

            Vector3[] bigVertices = bigMesh.vertices;

            if(normalize) Layouts.Normalize(spaceScale);

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

            Debug.Log(Time.realtimeSinceStartup - time);
            GlobalVariables.Solved = true;
        }


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
                

            }
   

            if (graph.movingNodes.Count > 0)
            {
                UpdatePosition(graph.selectedNodes);
            }

        }
    }


}





