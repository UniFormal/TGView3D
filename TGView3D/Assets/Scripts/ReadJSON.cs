
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class ReadJSON : MonoBehaviour {

    MyWrapper graph;
    Dictionary<string, int> nodes;
    bool init = false;
    public Material mat;
    public Material texMat;
    public Texture2D testTex;



    //private MeshRenderer mr;



    public string url="http://neuralocean.de/graph/test/nasa.json";
    public int vol =100;
    GameObject TextPrefab;
    GameObject line;

    float energy = 1000000f;
    float step = 30.0f;// initialStep;
    int success = 0;
    bool flat = false;

    float sliceWidth = 0;



[System.Serializable]
    private class MyWrapper
    {
        public List<MyNode> nodes;
        public List<MyEdge> edges;

        public static MyWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<MyWrapper>(jsonString);
        }


    }
    [System.Serializable]
    private class MyNode
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
        public List<int> edgeIndicesOut =new List<int>();
        public List<int> edgeIndicesIn = new List<int>();
        public List<int> connectedNodes = new List<int>();
        public int graphNumber;
        public int height = 0;




    }
    [System.Serializable]
    private class MyEdge
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
                    graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().material = new Material(mat);
                    graph.nodes[currNode].nodeObject.GetComponent<MeshRenderer>().material.color = randColor;

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
                randColor = Random.ColorHSV();
            }
        }
        Debug.Log(graphNumber-1);
 

    }
    // Function to calculate forces driven layout
    // ported from Marcel's Javascript versions
    void SolveUsingForces (int iterations, float spacingValue, bool resetForcesFixed= false, bool usingMinMax = false, float currTemperature = 0.9f, float initialStep = 30.0f)
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
            float kVal = Mathf.Max(Mathf.Min((graph.nodes.Count * 4 + graph.edges.Count / 2.5f) / 2 * 0.5f * spacingValue / 7.0f, 30), 70);
            float kSquared = kVal*kVal ;

            var energyBefore = energy;
            energy = 0;
            for (var j = 0; j < graph.nodes.Count  ; j++)
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
                            var differenceNodesY = u.pos.y - n.pos.y;
                            var differenceNodesZ = u.pos.z - n.pos.z;

                            //var lengthDiff = Mathf.Sqrt(differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY) + 0.001;
                           
                            var lengthDiff =  Mathf.Sqrt( differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY + differenceNodesZ * differenceNodesZ ) + 0.0001f;
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

                        var lengthDiff =  Mathf.Sqrt( differenceNodesX * differenceNodesX + differenceNodesY * differenceNodesY + differenceNodesZ * differenceNodesZ ) + 0.0001f;
                        var attractiveForce = (lengthDiff * lengthDiff / kVal);

                        n.disp.x += (differenceNodesX / lengthDiff) * attractiveForce;
                        n.disp.y += (differenceNodesY / lengthDiff) * attractiveForce;
                        n.disp.z += (differenceNodesZ / lengthDiff) * attractiveForce;
                    }

             


                    // Limit max displacement to temperature currTemperature

                    var dispLength =  Mathf.Sqrt( n.disp.x * n.disp.x + n.disp.y * n.disp.y + n.disp.z * n.disp.z ) + 0.0001f;
                    n.pos.x = ((n.pos.x + (n.disp.x / dispLength) * step));
                    n.pos.y = ((n.pos.y + (n.disp.y / dispLength) * step));
                    n.pos.z =  ((n.pos.z + (n.disp.z / dispLength) * step) );

                    // Prevent from displacement outside of frame
                    if (usingMinMax == true)
                    {
                        n.pos.x = (Mathf.Max(-vol, Mathf.Min(n.pos.x, vol)));
                        n.pos.y = (Mathf.Max(-vol, Mathf.Min(n.pos.y, vol )));
                        n.pos.z =  (Mathf.Max( -vol, Mathf.Min( n.pos.z, vol ) ) );
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


    



        void GenLabel(Transform parent, string label)
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
        text.GetComponent<TextMesh>().text = label;
        
    }


    void RandomSphere(string name)
    {
      //  GameObject node = GameObject.CreatePrimitive(PrimitiveType.Quad);
       //  node.transform.localScale = new Vector3(4, 1, 1);
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
       
       // GameObject node = new GameObject();
       // node.AddComponent<AlignText>();
        Vector3 pos = Random.insideUnitSphere * vol;


        GenLabel(node.transform, graph.nodes[nodes[name]].label);
        

        node.transform.position = pos;
        node.transform.GetComponent<Renderer>().material = mat;
        graph.nodes[nodes[name]].pos = pos;
        graph.nodes[nodes[name]].nodeObject = node;
        node.transform.parent = this.transform;
        node.transform.localScale = new Vector3(20, 20, 20);

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

        


        if (nodes.ContainsKey(name)) return;
 
        
        //dictionary for converting name to true id
        nodes.Add(name, id);

     

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

    private void Awake()
    {
        //Debug.Log(testTex.width + " " + testTex.height);
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



    void Start()
    {
        TextPrefab = (GameObject)Resources.Load("nodeText");


        XRSettings.enabled = false;
        Debug.Log(Application.persistentDataPath);
        //url = "file:///" + Application.persistentDataPath + "/nasa.json";

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
            graph= MyWrapper.CreateFromJSON(www.text);

       
            //Debug.Log(graph.nodes.Count);

            nodes = new Dictionary<string, int>();





            for (int i = 0; i < graph.nodes.Count; i++)
            {
                ProcessNode(graph.nodes[i].id, nodes.Count,false);

            }

  
            for (int i = 0; i < graph.edges.Count; i++)
            {

                if (graph.edges[i].style == "graphmeta")
                {
                    graph.edges.RemoveAt(i);
                    i--;

                } else

                {
                    ProcessNode(graph.edges[i].from, nodes.Count, true);
                    ProcessNode(graph.edges[i].to, nodes.Count, true);

                    if (nodes.ContainsKey(graph.edges[i].from) && nodes.ContainsKey(graph.edges[i].to))
                    {
                        MyNode source = graph.nodes[nodes[graph.edges[i].from]];
                        MyNode target = graph.nodes[nodes[graph.edges[i].to]];
                        source.edgeIndicesOut.Add(i);
                        target.edgeIndicesIn.Add(i);

                        source.connectedNodes.Add(nodes[graph.edges[i].to]);
                        target.connectedNodes.Add(nodes[graph.edges[i].from]);
                    }

                }
            }


            identifySubgraphs();
            SolveUsingForces(30, 40f, true);

            List<int> rootIndices = new List<int>();
            bool[] visited = new bool[graph.nodes.Count];
            float maxConnections = 0;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
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

                //*/
                if (graph.nodes[i].edgeIndicesIn.Count == 0) rootIndices.Add(i);
              
                maxConnections = Mathf.Max(graph.nodes[i].connectedNodes.Count,maxConnections);

            }

            Debug.Log("#root nodes: " + rootIndices.Count);

            float maxHeight = 0;
   

            for (int i = 0; i < rootIndices.Count; i++)
            {
                for (int j = 0; j < graph.nodes.Count; j++)
                {
                    visited[j] = false;
                }
                int curHeight = 1;
                Stack<int> nodeIndexStack = new Stack<int>();
                nodeIndexStack.Push(rootIndices[i]);
                Stack<int> heightStack = new Stack<int>();
                heightStack.Push(curHeight);
                visited[rootIndices[i]] = true;

                while (nodeIndexStack.Count> 0)
                {
                    int curIndex = nodeIndexStack.Pop();
                    curHeight = heightStack.Pop();
                    maxHeight = Mathf.Max(curHeight, maxHeight);
                    MyNode curNode = graph.nodes[curIndex];

                    for(int j=0; j < curNode.edgeIndicesOut.Count; j++)
                    {
                        
                        int childNodeIndex = nodes[graph.edges[curNode.edgeIndicesOut[j]].to];
                        graph.nodes[childNodeIndex].height = Mathf.Max(curHeight, graph.nodes[childNodeIndex].height);
                        if (!visited[childNodeIndex])
                        {
                            visited[childNodeIndex] = true;
                            nodeIndexStack.Push(childNodeIndex);
                            heightStack.Push(curHeight+1);
                        }
                    }

                }
            
            }
            sliceWidth = vol / maxHeight;
            Debug.Log("maxHeight: "+maxHeight);
;

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];

                var y = node.height * sliceWidth;

              /*  float x =(maxConnections/10- node.connectedNodes.Count)*20 ;
                if (x < -vol) x = 0;

                if (i % 2 == 0) x *= -1;*/

                Vector3 pos = new Vector3(node.pos.x,y,node.pos.z);

                node.pos = pos;
                node.nodeObject.transform.position = pos;

 

            }
          


     



            Vector3[] vertices = new Vector3[4*graph.edges.Count];
            int[] triangles = new int[6* graph.edges.Count];
            line = new GameObject();
            MeshRenderer mr = line.AddComponent<MeshRenderer>();
            MeshFilter mf = line.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            for (int i = 0; i < graph.edges.Count; i++)
            {
                if (nodes.ContainsKey(graph.edges[i].from) && nodes.ContainsKey(graph.edges[i].to))
                {
                    MyNode source = graph.nodes[nodes[graph.edges[i].from]];
                    MyNode target = graph.nodes[nodes[graph.edges[i].to]];

                    Vector3 dir = target.pos - source.pos;

                    Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * 0.5f;

                    vertices[0 + i * 4] = source.pos - offset;
                    vertices[2 + i * 4] = source.pos + offset;
                    vertices[1 + i * 4] = target.pos - offset;
                    vertices[3 + i * 4] = target.pos + offset;

                    triangles[0 + i * 6] = 0 + i * 4;
                    triangles[1 + i * 6] = 1 + i * 4;
                    triangles[2 + i * 6] = 2 + i * 4;
                    triangles[3 + i * 6] = 2 + i * 4;
                    triangles[4 + i * 6] = 3 + i * 4;
                    triangles[5 + i * 6] = 1 + i * 4;
                }

            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mr.material = mat;
            mf.mesh = mesh;

            line.transform.parent = this.transform;

            init = true;


        }
        else Debug.Log("WWW Error: " + www.error);


    }

    void Update()
    {
      
        if (Input.GetKeyDown("space"))
        {

            if (!init) return;




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

            Mesh mesh = line.GetComponent<MeshFilter>().mesh;



            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < graph.edges.Count; i++)
            {
                if (nodes.ContainsKey(graph.edges[i].from) && nodes.ContainsKey(graph.edges[i].to))
                {

                    var sourcePos = graph.nodes[nodes[graph.edges[i].from]].nodeObject.transform.position;
                    var targetPos = graph.nodes[nodes[graph.edges[i].to]].nodeObject.transform.position;

                    Vector3 dir = targetPos - sourcePos;
                    Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * 0.5f;


                    vertices[0 + 4 * i] = sourcePos - offset;
                    vertices[2 + 4 * i] = sourcePos + offset;
                    vertices[1 + 4 * i] = targetPos - offset;
                    vertices[3 + 4 * i] = targetPos + offset;

                    mesh.vertices = vertices;


                }


            }
            mesh.RecalculateBounds();




            /* SolveUsingForces(1, 8f, false);



          
             */
        }
    }




}
