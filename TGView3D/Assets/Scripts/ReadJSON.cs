
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

namespace TGraph
{


    public class ReadJSON : MonoBehaviour
    {

        public MyGraph graph;
        bool init = false;
        public Material mat;
        public Material lineMat;
        public GameObject grabbable;
        // public Material texMat;
        //public Texture2D testTex;


        //private MeshRenderer mr;



        public string url = "http://neuralocean.de/graph/test/nasa.json";
        public int vol = 100;
        GameObject TextPrefab;
       

        float energy = 1000000f;
        float step = 30f;// initialStep;
        int success = 0;
        bool flat = false;
        float sliceWidth = 0;




        [System.Serializable]
        public class MyGraph
        {
            public List<MyNode> nodes;
            public Dictionary<string, int> nodeDict;
            public List<MyEdge> edges;
            public List<MyEdge> tmpEdges;
            public List<MyNode> selectedNodes;
            public GameObject edgeObject;
            public float lineWidth = 0.002f;
            public List<int> removeList;

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
            public Vector3 pos;
            public Vector3 disp;
            public GameObject nodeObject;
            public bool forcesFixed;
            public List<int> edgeIndicesOut = new List<int>();
            public List<int> edgeIndicesIn = new List<int>();
            public List<int> connectedNodes = new List<int>();
            public int graphNumber;
            public float height = 0;
            public float weight = 0;
            public GameObject nodeEdgeObject;
            public GameObject labelObject;
            public bool selected = false;




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
            public GameObject line;
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
            Debug.Log(graphNumber - 1);
        

        }

        void buildHierarchy()
        {
            float maxHeight = -10;
            float minHeight = 10;

            for (int n = 0; n < 2; n++)
            {
                List<int> rootIndices = new List<int>();
                bool[] visited = new bool[graph.nodes.Count];
                float maxConnections = 0;
                for (int i = 0; i < graph.nodes.Count; i++)
                {
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
                    float curHeight = 0;
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



                        MyNode curNode = graph.nodes[curIndex];

                        int childCount;
                        if (n == 0) childCount = curNode.edgeIndicesIn.Count;
                        else childCount = curNode.edgeIndicesOut.Count;

                        for (int j = 0; j < childCount; j++)
                        {

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
                                if (n == 0) heightStack.Push(curHeight + 1);
                                else heightStack.Push(curHeight + 1);
                            }
                        }

                    }
                }
            }

            sliceWidth = 0.02f;
            sliceWidth = Mathf.Max(0.1f, 5.0f*maxHeight/ Mathf.Sqrt(graph.nodes.Count));

            //Debug.Log("maxHeight: " + maxHeight);
            ;

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];

                //Debug.Log(node.label + " " + node.height);
                var y = (node.height - 0.5f * node.weight) * sliceWidth;

                /*  float x =(maxConnections/10- node.connectedNodes.Count)*20 ;
                  if (x < -vol) x = 0;

                  if (i % 2 == 0) x *= -1;*/

                // y = Mathf.Sign(y) * (Mathf.Sqrt(Mathf.Sqrt(Mathf.Abs(y * graph.nodes.Count / 100))));

                Vector3 pos = new Vector3(node.pos.x, y * Mathf.Max(1, (graph.nodes.Count / 200.0f)), node.pos.z) / Mathf.Max(1, (graph.nodes.Count / 200.0f));



                node.pos = pos;
                node.nodeObject.transform.position = pos;



            }
        }


        // Function to calculate forces driven layout
        // ported from Marcel's Javascript versions
        void SolveUsingForces(int iterations, float spacingValue, bool resetForcesFixed = false, bool usingMinMax = false, float currTemperature = 0.9f, float initialStep = 3.0f)
        {
            if (resetForcesFixed == true)
            {
                for (var j = 0; j < graph.nodes.Count; j++)
                {
                    var n = graph.nodes[j];
                    n.forcesFixed = false;
                }
            }




            for (var i = 0; i < iterations; i++)
            {
                if (i % 100 == 0)
                {
                    Debug.Log("Beautify Layout: Iteration " + i + " of " + iterations + "...");
                }

                float area = vol * vol;
                float kVal = Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 30), 0.1f);
                float kSquared = kVal * kVal;

                var energyBefore = energy;
                energy = 0;
                for (var j = 0; j < graph.nodes.Count; j++)
                {
                    var n = graph.nodes[j];
                    if (!n.forcesFixed) //&& !n.hidden)
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
                                var differenceNodesY = 0;// u.pos.y - n.pos.y;
                                var differenceNodesZ = u.pos.z - n.pos.z;

                                //var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY) + 0.001;

                                var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY + differenceNodesZ * differenceNodesZ) + 0.0001f;
                                var repulsiveForce = -(kSquared / lengthDiff);

                                n.disp.x += (differenceNodesX / lengthDiff) * repulsiveForce;
                                n.disp.y += (differenceNodesY / lengthDiff) * repulsiveForce;
                                n.disp.z += (differenceNodesZ / lengthDiff) * repulsiveForce;
                            }
                        }

                        // calculate local (spring) forces
                        for (var k = 0; k < n.connectedNodes.Count; k++)
                        {
                            var u = graph.nodes[n.connectedNodes[k]];
                            var differenceNodesX = u.pos.x - n.pos.x;
                            var differenceNodesY = u.pos.y - n.pos.y;
                            var differenceNodesZ = u.pos.z - n.pos.z;

                            var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY + differenceNodesZ * differenceNodesZ) + 0.0001f;
                            var attractiveForce = (lengthDiff * lengthDiff / kVal);

                            n.disp.x += (differenceNodesX / lengthDiff) * attractiveForce;
                            n.disp.y += (differenceNodesY / lengthDiff) * attractiveForce;
                            n.disp.z += (differenceNodesZ / lengthDiff) * attractiveForce;
                        }




                        // Limit max displacement to temperature currTemperature

                        var dispLength = Mathf.Sqrt(n.disp.x * n.disp.x + n.disp.y * n.disp.y + n.disp.z * n.disp.z) + 0.0001f;
                        n.pos.x = ((n.pos.x + (n.disp.x / dispLength) * step));
                        n.pos.y = ((n.pos.y + (n.disp.y / dispLength) * step));
                        n.pos.z = ((n.pos.z + (n.disp.z / dispLength) * step));

                        // Prevent from displacement outside of frame
                        if (usingMinMax == true)
                        {
                            n.pos.x = (Mathf.Max(-vol, Mathf.Min(n.pos.x, vol)));
                            n.pos.y = (Mathf.Max(-vol, Mathf.Min(n.pos.y, vol)));
                            n.pos.z = (Mathf.Max(-vol, Mathf.Min(n.pos.z, vol)));
                        }
                        energy += dispLength * dispLength;
                    }
                    n.nodeObject.transform.position = n.pos;

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
            }

        }

        public static void setTriangles(int i, int[] triangles)
        {
            triangles[0 + i * 24] = 0 + i * 8;
            triangles[1 + i * 24] = 1 + i * 8;
            triangles[2 + i * 24] = 2 + i * 8;
            triangles[3 + i * 24] = 2 + i * 8;
            triangles[4 + i * 24] = 1 + i * 8;
            triangles[5 + i * 24] = 3 + i * 8;

            triangles[6 + i * 24] = 3 + i * 8;
            triangles[7 + i * 24] = 7 + i * 8;
            triangles[8 + i * 24] = 2 + i * 8;
            triangles[9 + i * 24] = 2 + i * 8;
            triangles[10 + i * 24] = 7 + i * 8;
            triangles[11 + i * 24] = 6 + i * 8;


            triangles[12 + 0 + i * 24] = 6 + i * 8;
            triangles[12 + 1 + i * 24] = 7 + i * 8;
            triangles[12 + 2 + i * 24] = 4 + i * 8;
            triangles[12 + 3 + i * 24] = 4 + i * 8;
            triangles[12 + 4 + i * 24] = 7 + i * 8;
            triangles[12 + 5 + i * 24] = 5 + i * 8;

            triangles[12 + 6 + i * 24] = 5 + i * 8;
            triangles[12 + 7 + i * 24] = 1 + i * 8;
            triangles[12 + 8 + i * 24] = 4 + i * 8;
            triangles[12 + 9 + i * 24] = 4 + i * 8;
            triangles[12 + 10 + i * 24] = 1 + i * 8;
            triangles[12 + 11 + i * 24] = 0 + i * 8;
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

        public static GameObject BuildEdges(List <MyEdge> edges, MyGraph graph, Dictionary<string, int> nodes, Material lineMat)
        {

            Vector3[] vertices = new Vector3[2*4 * edges.Count];
            int[] triangles = new int[2*2 * 6 * edges.Count];
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
                    Vector3 offsetOrtho = Vector3.Cross(dir, Vector3.right).normalized * graph.lineWidth;

                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = new Color(0, 255, 0);
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = new Color(0, 255, 20);

                    createEdge(i,vertices, source.pos,target.pos, offset, offsetOrtho);
                    setTriangles(i, triangles);
                    



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




        void GenLabel(Transform parent, MyNode node)
        {

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



            GameObject text = GameObject.Instantiate(TextPrefab);
            text.transform.parent = parent;
            text.GetComponent<TextMesh>().text = node.label;
            node.labelObject = text;
            


        }


        void RandomSphere(string name)
        {
            //  GameObject node = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //  node.transform.localScale = new Vector3(4, 1, 1);
            // GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject node = Instantiate(grabbable);
            // GameObject node = new GameObject();
            // node.AddComponent<AlignText>();
            Vector3 pos = Random.insideUnitSphere * vol;


            GenLabel(node.transform, graph.nodes[graph.nodeDict[name]]);


            node.transform.position = pos;
            //node.transform.GetComponent<Renderer>().sharedMaterial = mat;
            graph.nodes[graph.nodeDict[name]].pos = pos;
            graph.nodes[graph.nodeDict[name]].nodeObject = node;
            node.transform.parent = this.transform;
            //node.transform.localScale = new Vector3(20, 20, 20);


        }

        /* void FixedSphere()
         {

             GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
             GenLabel(node.transform, graph.nodes[nodes[name]].label);
             node.transform.position = pos;
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

   

        class PData
        {
            public string format = "TeX";
            public string math = "b + y = \\sqrt{f} = \\sum_n^5 {x}";
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



        void Awake()
        {
            TextPrefab = (GameObject)Resources.Load("nodeText");

            Camera camera = Camera.main;
            float[] distances = new float[32];

            camera.farClipPlane = 18;
            distances[18] = 6;
            camera.layerCullDistances = distances;
            camera.layerCullSpherical = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = new Color(0.7f, 0.8f, 0.7f); 

           // XRSettings.enabled=false;

            Debug.Log(Application.persistentDataPath);
            url = "file:///" + Application.persistentDataPath + "/nasa.json";

            Debug.Log(url);

            float time = Time.realtimeSinceStartup;

            WWW www = new WWW(url);

            StartCoroutine(WaitForRequest(www));

            PData data = new PData();

            //StartCoroutine(TestRequest(data));



        }



        IEnumerator TestRequest(PData pdata)
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
            }
        }



        IEnumerator WaitForRequest(WWW www)
        {
            yield return www;


            //Debug.Log(www.text);


            // check for errors
            if (www.error == null)
            {
                Debug.Log("WWW Ok!: " + www.text);
                graph = MyGraph.CreateFromJSON(www.text);


                //Debug.Log(graph.nodes.Count);

                graph.nodeDict = new Dictionary<string, int>();





                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    ProcessNode(graph.nodes[i].id, graph.nodeDict.Count, false);

                }

                graph.tmpEdges = graph.edges;
                for (int i = 0; i < graph.edges.Count; i++)
                {

                    if (graph.edges[i].style == "graphmeta")
                    {
                        graph.edges.RemoveAt(i);
                        i--;

                    }
                    else

                    {
                        ProcessNode(graph.edges[i].from, graph.nodeDict.Count, true);
                        ProcessNode(graph.edges[i].to, graph.nodeDict.Count, true);

                        if (graph.nodeDict.ContainsKey(graph.edges[i].from) && graph.nodeDict.ContainsKey(graph.edges[i].to))
                        {
                            MyNode source = graph.nodes[graph.nodeDict[graph.edges[i].from]];
                            MyNode target = graph.nodes[graph.nodeDict[graph.edges[i].to]];
                            source.edgeIndicesOut.Add(i);
                            target.edgeIndicesIn.Add(i);

                            source.connectedNodes.Add(graph.nodeDict[graph.edges[i].to]);
                            target.connectedNodes.Add(graph.nodeDict[graph.edges[i].from]);
                        }

                    }
                }


                identifySubgraphs();

                SolveUsingForces(100, 0.13f); 
                buildHierarchy();
                //SolveUsingForces(100, 0.13f);
               // buildHierarchy();

                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    var node = graph.nodes[i];
                    Vector3 pos = new Vector3(node.pos.x, node.pos.y, node.pos.z) /6f;

                    node.pos = pos;
                    node.nodeObject.transform.position = pos;
                }

                graph.edgeObject =BuildEdges(graph.edges, graph, graph.nodeDict, lineMat);

                init = true;


            }
            else Debug.Log("WWW Error: " + www.error);


        }

        void Update()
        {

            

            if (!init) return;

            if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two) )
            {
                if (Camera.main.farClipPlane==18) Camera.main.farClipPlane = 100;
                else Camera.main.farClipPlane = 18;

            }
            if (OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four) )
            {

                /* if(graph==null) graph = GameObject.Find("Nodes").GetComponent<TGraph.ReadJSON>().graph;
                 if (graph.edgeObject.activeInHierarchy)
                 {
                     graph.edgeObject.SetActive(false);
                 }
                 else
                 {
                     graph.edgeObject.SetActive(true);
                 }*/
                graph.edgeObject.SetActive(!graph.edgeObject.activeSelf);
            }


            // bool changed = false;

            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("space");
                SolveUsingForces(2, 0.13f);

            }
            if (Input.GetKeyDown(KeyCode.Return))
            {

                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    var node = graph.nodes[i];



                    Vector3 pos = new Vector3(node.pos.x / 4.0f, node.pos.y, node.pos.z / 4.0f);



                    node.pos = pos;
                    node.nodeObject.transform.position = pos;
                }
            }*/
            // if (OVRInput.Get(OVRInput.Button.Two)|| Input.GetKey(KeyCode.Return))

            {


          

          
                foreach (MyNode node in graph.selectedNodes)
                {
                    List<int> edgeIndices = node.edgeIndicesIn.Union<int>(node.edgeIndicesOut).ToList<int>();
           
                    Mesh mesh = node.nodeEdgeObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
                    Vector3[] vertices = mesh.vertices;
                    Vector3[] bigVertices = bigMesh.vertices;

                    for (int i = 0; i < edgeIndices.Count; i++)
                    {
                        var sourcePos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].from]].nodeObject.transform.position;
                        var targetPos = graph.nodes[graph.nodeDict[graph.edges[edgeIndices[i]].to]].nodeObject.transform.position;

                        //if (sourcePos != graph.nodes[graph.nodeDict[edges[i].from]].pos || targetPos != graph.nodes[graph.nodeDict[edges[i].to]].pos)
                        {
                            // Debug.Log("work");
                            Vector3 dir = targetPos - sourcePos;
                            Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                            Vector3 offsetOrtho = Vector3.Cross(dir, Vector3.right).normalized * graph.lineWidth;

                            createEdge(i, vertices, sourcePos, targetPos, offset, offsetOrtho);
                            createEdge(edgeIndices[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);


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



            /*
            if (Input.GetKeyDown("p"))
            {

                /*
                            if (flat)
                            {
                                for (int i = 0; i < graph.nodes.Count; i++)
                                {
                                    var node = graph.nodes[i];
                                    node.nodeObject.transform.position = new Vector3(node.pos.x, node.pos.y, node.pos.z);
                                }
                                flat = false;
                            }
                            else
                            {
                                for (int i = 0; i < graph.nodes.Count; i++)
                                {
                                    var node = graph.nodes[i];
                                    node.nodeObject.transform.position = new Vector3(node.pos.x, node.pos.y, 0);
                                }
                                flat = true;
                            }



                for (int i = 0; i < graph.edges.Count; i++)
                {
                    if (nodes.ContainsKey(graph.edges[i].from) && nodes.ContainsKey(graph.edges[i].to))
                    {

                        var sourcePos = graph.nodes[nodes[graph.edges[i].from]].pos;
                        var targetPos = graph.nodes[nodes[graph.edges[i].to]].pos;

                        graph.nodes[nodes[graph.edges[i].from]].nodeObject.transform.position = sourcePos;
                        graph.nodes[nodes[graph.edges[i].to]].nodeObject.transform.position = targetPos;

                        Vector3 dir = targetPos - sourcePos;
                        Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * lineWidth;


                        vertices[0 + 4 * i] = sourcePos - offset;
                        vertices[2 + 4 * i] = sourcePos + offset;
                        vertices[1 + 4 * i] = targetPos - offset;
                        vertices[3 + 4 * i] = targetPos + offset;



                    }


                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();



            } */
        }


    }

}

/*
              string to = "";
              for(int j=0; j< graph.nodes[i].edgeIndicesIn.Count; j++)
              {
                  to += graph.nodes[nodes[graph.edges[graph.nodes[i].edgeIndicesIn[j]].from]].label+", ";
              }
              Debug.Log(graph.nodes[i].label + "<-" + to);

              string from = "";
              for (int j = 0; j < graph.nodes[i].edgeIndicesOut.Count; j++)
              {
                  from += graph.nodes[nodes[graph.edges[graph.nodes[i].edgeIndicesOut[j]].to]].label + ", ";
              }
              Debug.Log(graph.nodes[i].label + "->" + from);
*/