
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
using System.IO;
using System.Text.RegularExpressions;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };


    public static int GetLineNumber(string file)
    {
        TextAsset data = Resources.Load(file) as TextAsset;
        if (data == null) return 0;

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        return lines.Length;

    }

        public static List<List<string>> Read(string file)
    {
      //  Debug.Log("read csv");
        int k = 0;
       // var list = new List<Dictionary<string, object>>();
        var list = new List<List<string>>();
        TextAsset data = Resources.Load(file) as TextAsset;
        if(data== null) return null;


        var lines = Regex.Split(data.text.Replace("|", ","), LINE_SPLIT_RE);
      //  Debug.Log("lines found: "+lines.Length);

        if (lines.Length <= 1) return list;
        string part="";
        string origin = "core";

        string[] header = { "type", "id", "to"};
        //Regex.Split(lines[0], SPLIT_RE);
        Debug.Log("header length: " + header.Length);
        for (var i = 1; i < lines.Length; i++)
        {
            
            var values = Regex.Split(lines[i], SPLIT_RE);
            // Debug.Log("values length: " + values.Length);
            if (values.Length == 0 || values[0] == "") continue;
            var entry = new List<string>();// new Dictionary<string, object>();
                    
            // if (values[0] == "oo") break;
            /*
            if (values[0] == "od")
            {
                var tmp = values[1];
                values[1] = values[2];
                values[2] = tmp;
                Debug.Log(values[1] + " " + values[2]);
            }*/


            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                /*object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;*/
                //  Debug.Log(header[j] + " " + finalvalue);
                entry.Add(value);
            
            }
            //  if (entry[0] == "oo") break;

            /*
            string[] splitString = values[1].Split('/');
        
            if (values[0] == "oo" && splitString[splitString.Length-1] != origin)
            {
                string dir = values[1].Remove(values[1].Length- splitString[splitString.Length - 1].Length-1, splitString[splitString.Length - 1].Length+1);
         
                //not  included yet
                origin = splitString[splitString.Length-1];
                part += "od,"+values[1]+","+dir  + "\n";

            }
            */

            /*
            //new block found
            string[] splitString = values[1].Split('/');
            if (values[0] == "oo" && splitString[splitString.Length-2] != origin)
            {
  
                //old block exists
                if (part != "")
                {
                    //write out old block
                    //origin=origin.Replace('/', '_');
                    //origin = origin.Replace(':', '_');

                
                    File.WriteAllText(Application.dataPath + "/" + origin + ".csv", part);
                    
                }
                //reset variables
                part = "";
                origin = splitString[splitString.Length-2];

            }

            part += lines[i] + "\n";
            */
            list.Add(entry);
            k++;
            // if (k > 5000) break;
           
        }
        // File.WriteAllText(Application.dataPath + "/stdlib_objects.csv", part);
        if (list.Count == 0) Debug.LogError("bad list");
        return list;
    }
}


namespace TGraph
{

    [System.Serializable]
    public class SVGCollection
    {
        public string[] svgs;

        public static JSONDict CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<JSONDict>(jsonString);
        }

    }



    [System.Serializable]
    public class JSONDict
    {
        public KeyPosition[] keysAndPositions;

        public static JSONDict CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<JSONDict>(jsonString);
        }

    }

    [System.Serializable]
    public class KeyPosition
    {
        public string id;
        public Vector3 pos;

    }

    [System.Serializable]
    public class SimpleGraph
    {
        public List<SimpleNode> nodes;
        public List<SimpleEdge> edges;
        public static SimpleGraph CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<SimpleGraph>(jsonString);
        }

    }
    [System.Serializable]
    public class SimpleNode
    {

        public string id;
        public string label;
        public string style;
        public float radius;

    }
    [System.Serializable]
    public class SimpleEdge
    {
        public string type;
        public string style;
        public string from;
        public string to;
    }

    public class ReadJSON : MonoBehaviour
    {
        public GameObject Percent;
        public MyGraph graph;
        public Material mat;
        public Material lineMat;
        public GameObject grabbable;
        public bool onlyInclude = false;
        public GameObject EventSystem;
        public bool recursive = false;
        // public Material texMat;
        //public Texture2D testTex;
        //private MeshRenderer mr;
        // private GameObject TextPrefab;
        public float globalWeight;

        public int iterations = 25;
        public float spaceScale = 1;
        public GameObject UrlSelect;
        int si = 0;
        public float time = 0;
        public string url;//http://neuralocean.de/graph/test/nasa.json";
        public int vol = 100;
        public TextAsset[] GraphFiles;
        private static string LayoutFile = "";
        public static Dictionary<string, Vector3> nodePosDict;
        SVGCollection svgCol;
        public TextAsset SVGFile;
        static bool IsMPD = false;
        public static Color BaseColor = Color.white;
        public static Color SelectedColor = Color.cyan;
        public static Color ConnectedColor = Color.yellow;
        public static Color TargetColor = Color.red;
        public static bool IsCoq = false;
        public static List<MyNode> FoundNodes;
        public bool Gen = false;


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
            public int modus = 0;
            public GameObject subObject;
            public float lineWidth = 0.003f;
            public bool UseForces = true;
            public bool WaterMode = true;
            public bool FlatInit = false;
            public bool HeightInit = false;
            public bool UseConstraint = false;
            public bool RandomInit = false;
            public float PushLimit = 2f;


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
            public float radius = 0;
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
            public float range = float.MaxValue;
            public int ClusterId = -1;
            public bool generated;
            public bool visited = false;
            //use object references instead?
            public List<int> edgeIndicesOut = new List<int>();
            public List<int> edgeIndicesIn = new List<int>();
            public List<int> connectedNodes = new List<int>();
            public List<NE> targets = new List<NE>();

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
            public string type = "";
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
            public int targetCount = 0;
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
        public class NE
        {
            public MyNode node;
            public MyEdge edge;

        }


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
            Color randColor = mat.color;
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
            //  foreach (var n in countNodesInGraph) Debug.Log(n+"nodes in Subgraph");

        }




        public void CalculateSubgraph()
        {



            Debug.Log(graph.nodes.Count + " looking for " + graph.latestSelection);
            if (graph.latestSelection >= graph.nodes.Count || graph.latestSelection == -1)
                return;

            //remove this for coq
            FoundNodes.Clear();
            if (FoundNodes.Count == 0) FoundNodes.Add(graph.nodes[graph.latestSelection]);

            foreach (var node in FoundNodes)
            {



                //var node = graph.nodes[graph.latestSelection];

                  //deactivate
                   if (graph.subGraphOrign == graph.latestSelection)
                   {
                       GameObject.Destroy(graph.subObject);
                       GameObject.Destroy(node.nodeObject.transform.GetChild(1).gameObject);

                       graph.subGraphOrign = -1;
                       graph.edgeObject.SetActive(true);
                   }
                   //build
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

                    //if (GameObject.Find("VR") == null)
                    //    Camera.main.transform.LookAt(node.pos);

                }

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
        /*
        public static void createEdge(List<ReadJSON.MyEdge> edges,int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho, int type, int controlPoints)
        {
            if (true)//(type < 0)
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
                }
                var rand = Random.insideUnitSphere * 0.05f;
                createStraightEdge(i, vertices, sourcePos + rand, targetPos + rand, offset, offsetOrtho);
            }


        }*/
        public static void createEdge(List<ReadJSON.MyEdge> edges, int i, Vector3[] vertices, Vector3 sourcePos, Vector3 targetPos, Vector3 offset, Vector3 offsetOrtho)
        {
            Vector3 next = 7 * (Quaternion.AngleAxis(360 * edges[i].localIdx, targetPos - sourcePos) * offset);
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


                    //Debug.Log(edges[i].style);
                    vertexColors[0 + i * 8] = vertexColors[2 + i * 8] = vertexColors[4 + i * 8] = vertexColors[6 + i * 8] = GenerateOriginColor(graph.colorDict[edges[i].style]);
                    vertexColors[1 + i * 8] = vertexColors[3 + i * 8] = vertexColors[5 + i * 8] = vertexColors[7 + i * 8] = GenerateTargetColor(graph.colorDict[edges[i].style]);

                    //creates square tubes by setting vertices manually

                    //  if (edges[i].localIdx > 0)
                    //     Debug.Log(edges[i].localIdx+" "+next * 100+edges[i].from);
                    /*
                    if (edges[i].localIdx > 0)
                        createEdge(graph.edges,i, vertices, source.pos+next, target.pos+next, offset, offsetOrtho);
                    else*/
                    createEdge(graph.edges, i, vertices, source.pos, target.pos, offset, offsetOrtho);
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




        public static GameObject GenLabel(Transform parent, string label, string type)
        {
            GameObject text = (GameObject)Instantiate(Resources.Load("nodeText"));

            text.transform.parent = parent;
            text.GetComponent<TextMesh>().text = label;
            if (type == "o") text.GetComponent<TextMesh>().color = Color.red;
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
            GameObject nodeObject = Instantiate(grabbable);
            nodeObject.transform.localScale *= 1.1f;
            // GameObject node = new GameObject();
            // node.AddComponent<AlignText>();
            Vector3 pos = Random.insideUnitSphere * vol;

            var node = graph.nodes[graph.nodeDict[name]];
            // Debug.Log(node.label+" "+name);
     
            node.labelObject = GenLabel(nodeObject.transform, node.label, node.style);
  
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


        bool ProcessNode(string name, int id, MyEdge edge)
        {

            if (graph.nodeDict.ContainsKey(name)) return false;


            //dictionary for converting name to true id
            graph.nodeDict.Add(name, id);



            if (edge != null)
            {
                //Add Nodes that are not already present in orginal data
                //TODO: use label of id
                MyNode tmp = new MyNode();
                tmp.id = name;
                tmp.label = name;
                tmp.nr = graph.nodes.Count;
                tmp.generated = true;
                tmp.radius = edge.targetCount * 10;
                //      Debug.Log(tmp.radius);
                graph.nodes.Add(tmp);


                // Debug.Log(name + "  " + tmp.nr);
            }
            // graph.nodes[id].edgeIndicesIn = new List<int>();

            RandomSphere(name);
            return true;

        }
        void ProcessNodes()
        {

            for (int i = 0; i < graph.nodes.Count; i++)
            {
                //check not required
                if (ProcessNode(graph.nodes[i].id, graph.nodeDict.Count, null))
                    graph.nodes[i].nr = i;
            }


            string json = SVGFile.text;//;
            string[] svgs = JsonUtility.FromJson<SVGCollection>(json).svgs;

            // List<string> tmpMathMLs = new List<string>();
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].mathml != null)
                {
                    //tmpMathMLs.Add(graph.nodes[i].mathml);
                    // PData data = new PData();
                    //   data.math = graph.nodes[i].mathml;
                    // StartCoroutine(TestRequest(data, i));
                    graph.nodes[i].svg = svgs[i];
                    CreateMathObject(i);
                }

            }
            // JsonUtility.ToJson(tmpMathMLs);





        }


        void ProcessEdges()
        {
            graph.tmpEdges = graph.edges;
            for (int i = 0; i < graph.edges.Count; i++)
            {

                ProcessNode(graph.edges[i].from, graph.nodeDict.Count, graph.edges[i]);
                ProcessNode(graph.edges[i].to, graph.nodeDict.Count, graph.edges[i]);

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
                        graph.edges[i].type = "include";
                        if (graph.edges[i].style != "graphinclude" && graph.edges[i].style != "include")
                        {
                            weight = 0.8f;
                            if (graph.edges[i].style == "graphmeta" || graph.edges[i].style == "meta")
                                weight = .2f;
                            graph.edges[i].type = "include";
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


                foreach (int eidx in node.edgeIndicesIn)
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
                        if (node.nr != duplicate.Nid && graph.edges[edgeIndices[duplicate.Index]].localIdx == 0)
                            //  graph.edges[edgeIndices[duplicate.Index]].localIdx =
                            k++;

                    }
                    float dnum = k;

                    k = 1;
                    foreach (var duplicate in duplicateGroup)
                    {
                        if (node.nr != duplicate.Nid && graph.edges[edgeIndices[duplicate.Index]].localIdx == 0)
                        {
                            graph.edges[edgeIndices[duplicate.Index]].localIdx = k++ / dnum;


                        }
                        // Debug.Log(node.label +" "+graph.nodes[duplicate.Nid].label + " " + duplicate.Index + " " + graph.edges[edgeIndices[duplicate.Index]].localIdx + " " + graph.edges[edgeIndices[duplicate.Index]].from + " " + graph.edges[edgeIndices[duplicate.Index]].to);
                        //TODO up here

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


            if (LayoutFile != "")
                GameObject.Find("FileInputField").GetComponent<InputField>().text = LayoutFile;


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


            if (FoundNodes == null) FoundNodes = new List<MyNode>();


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


            if (IsCoq) LoadCoqGraph();

            else if (GlobalVariables.SelectionIndex == GraphFiles.Length - 1)
            {
                LoadMPDGraph();
                IsMPD = true;
            }
            else
            {
                IsMPD = false;
            }

            if (GlobalVariables.Reload && !GlobalVariables.Init) LoadGraph();


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


        public void LoadCoqDirectory() {

            int n = 0;
            List<string> tmpOrigins = new List<string>();

            // FoundNodes.Add(graph.nodes[761]);

            foreach (var node in FoundNodes)
            {
                var label = node.label;
                string[] splitStringDir = label.Split('/');
                string dirFile = splitStringDir[splitStringDir.Length - 1];
                Debug.Log("load" + dirFile);
                var data = CSVReader.Read(dirFile);
                if (data == null) continue;


                string last = "";



                int cCount = 0;
                foreach (var val in data)
                {


                    // redunddant od eges?
                    if (last != val[1])
                    {
                        if (cCount != 0)
                            graph.edges.Last().targetCount = cCount;

                        cCount = 0;
                        var diredge = new MyEdge();
                        diredge.style = "include";// val[0];
                        diredge.to = val[1];
                        string[] splitString = val[1].Split('/');
                        string dir = val[1].Remove(val[1].Length - splitString[splitString.Length - 1].Length - 1, splitString[splitString.Length - 1].Length + 1);
                        diredge.from = dir;
                        graph.edges.Add(diredge);
                        last = val[1];

                    }
                    cCount++;
                    //oo edges
                    /*
                    var edge = new MyEdge();
                    edge.style = "include";// val[0];
                    edge.from = val[1];
                    edge.to = val[2];

                    if (!graph.edges.Contains(edge))
                    {
                        graph.edges.Add(edge);
                        n++;

                    }
                    if (n > 2000) return;
                    */



                }

                /*
                            //od edges of targets
                            foreach (var val in data)
                            {


                                val[1] = val[2];


                                var diredge = new MyEdge();
                                diredge.style = "include";// val[0];
                                diredge.to = val[1];
                                string[] splitString = val[1].Split('/');
                                string dir = val[1].Remove(val[1].Length - splitString[splitString.Length - 1].Length - 1, splitString[splitString.Length - 1].Length + 1);
                                diredge.from = dir;

                                if (!tmpOrigins.Contains(val[1]))
                                {
                                    graph.edges.Add(diredge);
                                    tmpOrigins.Add(val[1]);
                                }

                            }
                        }*/
            }

        }

        public void SpawnNodesEarly(List<string> val, MyEdge edge)
        {

            
            MyNode origin  = graph.nodes.Find(x => x.id == val[1]);
            if (origin == null)
            {
                origin = new MyNode();
                origin.style = "o";
                origin.generated = true;
                origin.id = val[2];
                string[] parts = val[2].Split('/');
                origin.label = parts[parts.Length - 1];
                // origin.label = val[1];
                graph.nodes.Add(origin);
            }

            MyNode node = graph.nodes.Find(x => x.id == val[2]);
            if (node == null)
            {
                node = new MyNode();
                node.style = "o";
                if (val[0] == "ii") node.style = "d";
                node.generated = true;
                node.id = val[2];
                string[] parts = val[2].Split('/');
                node.label = parts[parts.Length - 1];
                // node.label = val[1];
                graph.nodes.Add(node);
            }
            var ne = new NE()
            {
                node = node,
                edge = edge

            };

            origin.targets.Add(ne);
            
        }

        public void LoadCoq()
        {
            Debug.Log("num nodes found: " + FoundNodes.Count);

            if (GlobalVariables.Init) FoundNodes.Add(graph.nodes[graph.latestSelection]);
            Debug.Log(FoundNodes.Count);
            if (!IsCoq||FoundNodes.Count>0) { 

                IsCoq = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void LoadCSV(string file)
        {
            List<List<string>> data = CSVReader.Read(file);
            // Debug.Log(data[0]["id"]);
            int n = 0;
            foreach (var val in data)
            {
                if (val[0].Length == 1)
                {
                    if(val [0] == "s")
                    {
                        graph.nodes.Find(f => f.id == val[1]).radius = System.Int32.Parse(val[2]);
                    }
                    else
                    {
                        var node = new MyNode();

                        node.style = val[0];
                        node.id = val[1];
                        string[] parts = val[1].Split('/');
                        node.label = parts[parts.Length - 1];
                        // node.label = val[1];
                        graph.nodes.Add(node);
                        /*        if (val[0] == "o")
                                {
                                    var edge = new MyEdge();
                                    edge.style = "meta";// val[0];
                                    edge.from = val[1];
                                    string target = "";
                                    for(int i =0; i < parts.Length - 1;++i)
                                    {
                                        target += parts[i];
                                    }
                                    edge.to = target;
                                    graph.edges.Add(edge);
                                }*/
                    }

                }
                else
                {
                    if (val[0]=="ii"&&val[2] == "cic:") continue;
                    var edge = new MyEdge();
                    edge.style = "include";// val[0];
                    edge.from = val[1];
                    edge.to = val[2];
                    edge.label = val[0];
                    Debug.Log("found " + val[0]);


                    if (!edge.from.Contains(edge.to)&&edge.from != edge.to && !graph.edges.Any(x => x.from == edge.from && x.to == edge.to))
                    {
                        graph.edges.Add(edge);
                        SpawnNodesEarly(val, edge);
                        n++;
                    }

                    // if (n > 300) break;
                }
        
 
              

                /*   else if (val[0] == "od")
                   {
                       var edge = new MyEdge();
                       edge.style = "meta";// val[0];
                       edge.from = val[1];
                       edge.to = val[2];
                       graph.edges.Add(edge);
                   }*/
            }
        }

        public void LoadCoqGraph()
        {
            Debug.Log("load coq graph");
            graph = new MyGraph();
            graph.nodes = new List<MyNode>();
            graph.edges = new List<MyEdge>();

            float st = Time.realtimeSinceStartup;
            var curstring =
              //  "graph_algebra";
            //"exportSG";
            //"fingroup";
            "tlc";
           // "extlib";


            if (Gen)
            {
                LoadCSV(curstring);

                Debug.Log("searchcrap " + (Time.realtimeSinceStartup - st));
                // LoadCSV("core");
                // LoadCSV("cut");

                // LoadCoqDirectory();
                //LoadCoqDirectory("N");
                //LoadCoqDirectory("BinNat");
                //
                GlobalVariables.Graph = graph;
                TransitiveReduction();
                SimpleExport(curstring);
            }
            else

            {
                SimpleImport(curstring);
                InitGraph();

            }
        }

        public void TransitiveReduction()
        {
            int r = 0;
            int l = 0;
            foreach (var node in graph.nodes)
            {
                for (int n = 0; n < graph.nodes.Count; n++)
                {
                    graph.nodes[n].visited = false;
                }



                var targets = new List<NE>();
                var subs = new List<NE>();
                node.visited = true;
                foreach (var ne in node.targets)
                {
                    if (ne.edge.style == "od") continue;
                    targets.Add(ne);
                    subs.Add(ne);

                }
                for (int i = 0; i < subs.Count; ++i)
                {
                    l++;
                    //  if (l > 10000) break;
                    if (targets.Count == 0) break;

                    for (int o = 0; o < subs[i].node.targets.Count; ++o)
                    {
                        var subTarget = subs[i].node.targets[o];
                        if (subTarget.edge.style != "include") continue;

                        if (!subTarget.node.visited)
                        {
                            subTarget.node.visited = true;
                            for (int j = 0; j < targets.Count; ++j)
                            {
                                var t = targets[j];
                                if (subTarget.node == t.node)
                                {
                                    graph.edges.Remove(t.edge);
                                    targets.Remove(t);
                                  //  Debug.Log(t.edge.from+" "+t.edge.to+" "+subTarget.edge.from+" "+subTarget.edge.to);
                                    r++;
                                }
                            }
                            subs.Add(subTarget);
                        }

                    }

                }
            }
            Debug.Log(l + ", #removed: " + r);
        }

        public void SimpleImport(string s)
        {
            StreamReader reader = new StreamReader(Application.dataPath + "/"+s+".json");
            var text = reader.ReadToEnd();
            SimpleGraph sG = SimpleGraph.CreateFromJSON(text);
            Debug.Log(FoundNodes.Count+" fffffffffffffffffffffffffffffffffffffffffffff");
            Debug.Log(sG.edges.Count);
            var MyNodes = new List<MyNode>();
            var MyEdges = new List<MyEdge>();
            foreach (var node in sG.nodes)
            {
                var MyNode = new MyNode
                {
                    id = node.id,
                    label = node.label,
                    style = node.style,
                    radius = node.radius
                
                };
                //only show objects when found
             
                if (MyNode.style == "o")// && FoundNodes.Exists(f =>  MyNode.id.Contains(f.id)))
                    MyNodes.Add(MyNode);
                else if (MyNode.style == "d")
                    MyNodes.Add(MyNode);
         
            }
            int e = 0;
            foreach (var edge in sG.edges)
            {
           
                var MyEdge = new MyEdge
                {
                    from = edge.from,
                    to = edge.to,
                    style = "include",
                    label = edge.type
                };
                Debug.Log(MyEdge.label);
                //add oo edges of found nodes
                if (MyEdge.label == "oo")// && (FoundNodes.Exists(f => MyEdge.from.Contains(f.id))|| FoundNodes.Exists(f => MyEdge.from.Contains(f.id))))
                {
                    MyEdges.Add(MyEdge); e++;
                    if (e > 8000)
                    {
                        Debug.LogError("size");
                        break;
                    }
                }
                else if (MyEdge.label == "ii")// && !(FoundNodes.Exists(f => f.id == MyEdge.from) || FoundNodes.Exists(f => f.id == MyEdge.to)))
                {
                    MyEdge.style = "include";
                    MyEdges.Add(MyEdge); e++;
                  
                }
                else if (MyEdge.label == "od" && ( FoundNodes.Exists(f => f.id == MyEdge.from)))
                {

                    MyEdges.Add(MyEdge);

                }

                else if(MyEdge.label=="dd")
                {
                    MyEdges.Add(MyEdge);
               
                   // if (e > 5000) break;
                }


            }

            var MyGraph = new MyGraph
            {
                nodes = MyNodes,
                edges = MyEdges

            };
            GlobalVariables.Graph = MyGraph;


        }

        public void SimpleExport(string s)
        {
            var simpleNodes = new List<SimpleNode>();
            var simpleEdges = new List<SimpleEdge>();
            foreach (var node in graph.nodes)
            {
                var simpleNode = new SimpleNode
                {
                    id = node.id,
                    label = node.label,
                    radius = node.radius,
                    style = node.style
                };
                simpleNodes.Add(simpleNode);
            }
            foreach (var edge in graph.edges)
            {
                var simpleEdge = new SimpleEdge
                {
                    from = edge.from,
                    to = edge.to,
                    style = edge.style,
                    type = edge.label
                    

                };
                simpleEdges.Add(simpleEdge);
            }

            var simpleGraph = new SimpleGraph
            {
                nodes = simpleNodes,
                edges = simpleEdges

            };


            string json = JsonUtility.ToJson(simpleGraph);
            File.WriteAllText(Application.dataPath + "/"+s+".json", json);
        }


        public void CreateMathObject(int i)
        {
            GameObject mathObject = (GameObject)Instantiate(Resources.Load("mathObject"));

            var parentTransform = graph.nodes[i].labelObject.transform;

       
            ImportSVG.ImportAsMesh(graph.nodes[i].svg, ref mathObject);
            var y = 0f;
            if (graph.nodes[i].label != "")
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

                CreateMathObject(i);

            }
        }


        private void UpdateAllEdges()
        {
            Mesh bigMesh = graph.edgeObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] bigVertices = bigMesh.vertices;
            for (int i = 0; i < graph.edges.Count; i++)
            {
                var sourcePos = graph.nodes[graph.nodeDict[graph.edges[i].from]].nodeObject.transform.localPosition;
                var targetPos = graph.nodes[graph.nodeDict[graph.edges[i].to]].nodeObject.transform.localPosition;
                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                ReadJSON.createEdge(graph.edges, i, bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }
            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();
        }

        private IEnumerator FinishUpdate()
        {

            if (nodePosDict == null)
            {
                NativeArray<float> Energies = new NativeArray<float>(graph.nodes.Count, Allocator.Persistent);
                var handle = Layouts.BaseLayout(iterations, globalWeight, spaceScale, Energies);
               

                Debug.Log("Begin Layout " + ((Time.realtimeSinceStartup - time)));

                // yield return new WaitUntil(() => handle.IsCompleted);
                while (!handle.IsCompleted)


                {
                    //GlobalVariables.Percent.text = ((float)(100.0f * (graph.fin)*2 / iterations)).ToString();
                    GlobalVariables.Percent.text = graph.fin.ToString();

                    if (graph.fin > 1)
                    {
                         Layouts.Normalize(spaceScale, true);
                        //Debug.Log((Time.realtimeSinceStartup-time));
                         UpdateAllEdges();
                    }


                    yield return new WaitForSeconds(.1f);
                }
                graph.fin = 0;
                GlobalVariables.Percent.text = "";
                handle.Complete();
                Layouts.Normalize(spaceScale);
                Energies.Dispose();
            }
            else
            {
                Debug.LogWarning("loading layout from file");
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    var node = graph.nodes[i];
                    if (nodePosDict.ContainsKey(node.id)){
                        var pos = nodePosDict[node.id];
                        node.pos = pos;
                        node.nodeObject.transform.localPosition = pos;
                    }
                    else
                    {
                        node.pos = Vector3.zero;
                        node.nodeObject.transform.localPosition = Vector3.zero;
                    }

                }
                if (IsCoq)
                {
                    UpdateAllEdges();
                    NativeArray<float> Energies = new NativeArray<float>(graph.nodes.Count, Allocator.Persistent);
                    var handle = Layouts.UpdateLayout(iterations, globalWeight, spaceScale, Energies);

                    Debug.Log("Begin Layout " + ((Time.realtimeSinceStartup - time)));

                    // yield return new WaitUntil(() => handle.IsCompleted);
                    while (!handle.IsCompleted)


                    {
                        //GlobalVariables.Percent.text = ((float)(100.0f * (graph.fin)*2 / iterations)).ToString();
                        GlobalVariables.Percent.text = graph.fin.ToString();

                        if (graph.fin > 1)
                        {
                             Layouts.Normalize(spaceScale, true);
                            //Debug.Log((Time.realtimeSinceStartup-time));
                             UpdateAllEdges();
                        }


                        yield return new WaitForSeconds(.1f);
                    }
                    graph.fin = 0;
                    GlobalVariables.Percent.text = "";
                    handle.Complete();
                    Layouts.Normalize(spaceScale);
                    Energies.Dispose();
                }
            


            }


            //TODO: iterate over edges instead
            foreach (MyNode node in graph.nodes)
            {
                UpdateEdges(node);
            }

           // Clustering.DBScan();

        }


        private IEnumerator FinishInit()
        {
            Layouts.Init();
            Layouts.Spiral();
          
            graph.edgeObject = BuildEdges(graph.edges, ref graph, lineMat);
            graph.edgeObject.transform.parent = transform.parent;
            graph.edgeObject.name = "EdgeMesh";
            UIInteracton.SEnableEdgeType("meta");
          //  graph.edgeObject.SetActive(false);

            yield return FinishUpdate();


            GlobalVariables.Solved = true;
            this.GetComponent<Interaction>().enabled = true;
            GlobalVariables.Init = true;
            this.GetComponent<GlobalAlignText>().childCount = this.transform.childCount;
            //     graph.Positions.Dispose();
            //    graph.Disps.Dispose();

            /*    nodePosDict = new Dictionary<string, Vector3>();
          foreach(var node in graph.nodes)
             {
                 nodePosDict.Add(node.id, node.pos);
             }*/


            Debug.Log("Finished init " + ((Time.realtimeSinceStartup - time)));
            //graph.edgeObject.SetActive(true);





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

        private void InitGraph()
        {
            graph = GlobalVariables.Graph;

            graph.nodes = graph.nodes
            .GroupBy(customer => customer.id)
            .Select(group => group.First()).ToList();
            GlobalVariables.Vol = vol;

            Debug.Log("nodes edges" + graph.nodes.Count + " " + graph.edges.Count);

            foreach(var edge in graph.edges)
            {
                var tmp = edge.from;
                edge.from = edge.to;
                edge.to = tmp;
            }

            graph.movingNodes = new List<int>();
            graph.selectedNodes = new List<int>();
            graph.selectedNodes.Add(-1);
            graph.selectedNodes.Add(-1);

            graph.colorDict = new Dictionary<string, Color>();
            graph.colorDict.Add("include", new Color(0, 255, 0));
            graph.colorDict.Add("meta", new Color(255, 0, 0));
            graph.colorDict.Add("alignment", new Color(200, 200, 0));
            graph.colorDict.Add("view", new Color(0, 0, 255));
            graph.colorDict.Add("structure", new Color(0, 120, 120));


            graph.nodeDict = new Dictionary<string, int>();
            Debug.Log("setup time " + (Time.realtimeSinceStartup - time));

            ProcessNodes();
            ProcessEdges();

            identifySubgraphs();

            if (IsCoq)
            {
                /*
                if (recursive)
                {
                    List<List<string>> sizes = CSVReader.Read("sizes");

                    foreach (var size in sizes)
                    {
                        if (!graph.nodeDict.ContainsKey(size[1])) break;
                        graph.nodes[graph.nodeDict[size[1]]].radius = System.Int32.Parse(size[2]);
                    }
                }
                else
                {
                    foreach (var node in graph.nodes)
                    {

                        node.radius += CSVReader.GetLineNumber(node.label);
                    }
                }
          
                */
                float maxSize = -1f;
                foreach (var node in graph.nodes)
                {
                    maxSize = Mathf.Max(maxSize, node.radius);
                }

                Material loadedMat = new Material(mat);
                loadedMat.color = Color.red;


            
                foreach (var node in graph.nodes)
                {
                    node.radius = 10*
                    //Mathf.Sqrt
                        (node.radius / (maxSize+1));
                    node.nodeObject.transform.localScale *= 2*(1+node.radius);
                    if(node.generated)
                        node.nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;

                }
            }

            //graph.Disps = new NativeArray<Vector3>(graph.nodes.Count,Allocator.Persistent);
            //graph.Positions = new NativeArray<Vector3>(graph.nodes.Count, Allocator.Persistent);





            Debug.Log("prep time " + (Time.realtimeSinceStartup - time));

            StartCoroutine(FinishInit());
        }

    
        IEnumerator ProcessJSON(WWW www, int type = 0)
        {
           
            if (www == null)
            {
                yield return null;
            }

                
            else
                yield return www;


            Debug.Log("index used for processing"+GlobalVariables.SelectionIndex);
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
           
            GlobalVariables.Graph = ReadJSON.MyGraph.CreateFromJSON(json);
            Debug.Log(GlobalVariables.Graph.nodes.Count);

            if (type == 1)
            {
                graph.WaterMode = false;
                graph.HeightInit = false;
                graph.FlatInit = true;
            }

            InitGraph();

            
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
                ReadJSON.createEdge(graph.edges,node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {
                targetPos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;
                ReadJSON.createEdge(graph.edges,node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            bigMesh.vertices = bigVertices;
            bigMesh.RecalculateBounds();
            if (graph.subObject != null)
            {

                Mesh subMesh = graph.subObject.GetComponent<MeshFilter>().sharedMesh;

                Vector3[] subVertices = subMesh.vertices;
                int k = 0;
                foreach (int eid in graph.subEdges)
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

                ReadJSON.createEdge(graph.edges,node.edgeIndicesIn[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                ReadJSON.createEdge(graph.edges,i, vertices, sourcePos, targetPos, offset, offsetOrtho);
            }

            sourcePos = targetPos;
            for (int i = 0; i < node.edgeIndicesOut.Count; i++)
            {

                targetPos = graph.nodes[graph.nodeDict[graph.edges[node.edgeIndicesOut[i]].to]].nodeObject.transform.localPosition;

                Vector3 dir = targetPos - sourcePos;
                Vector3 offset = Vector3.Cross(dir, Vector3.up).normalized * graph.lineWidth;
                Vector3 offsetOrtho = Vector3.Cross(dir, offset).normalized * graph.lineWidth;

                ReadJSON.createEdge(graph.edges,node.edgeIndicesOut[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
                ReadJSON.createEdge(graph.edges,i + node.edgeIndicesIn.Count, vertices, sourcePos, targetPos, offset, offsetOrtho);
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
                UpdateEdges(node);
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

            time = Time.realtimeSinceStartup;
            StartCoroutine(ProcessJSON(jsonUrl));
  
        }

      
        public void ChangeID(InputField f)
        {
            if (!GlobalVariables.Init) return;
            int result = 0;
            if (System.Int32.TryParse(f.text, out result))
            {
                
                graph.latestSelection = result;
            }
            else
            {
                FoundNodes.Clear();
                foreach (var pair in graph.nodeDict)
                {
                    var p = graph.nodes[pair.Value].label;

                    int startIdx = p.ToLower().IndexOf(f.text.ToLower());
                    if (startIdx != -1)
                    {
                        if(p[startIdx] == p.ToUpper()[startIdx]|| (startIdx>0 &&p[startIdx - 1] == '_')){
                            graph.latestSelection = graph.nodeDict[pair.Key];
                            Debug.Log("found " + graph.latestSelection);
                            if(!FoundNodes.Contains(graph.nodes[graph.nodeDict[pair.Key]]))
                                FoundNodes.Add(graph.nodes[graph.nodeDict[pair.Key]]);
                        }
           
                        //break;
                    }

                }
            
               
            }
            
        }

        public void ChangeFile(InputField f)
        {
            LayoutFile = f.text;

        }

        public void StoreLayout()
        {
          //  svgCol = new SVGCollection();
           // svgCol.svgs = new string[graph.nodes.Count];


            var positions = new JSONDict();
            positions.keysAndPositions = new KeyPosition[graph.nodes.Count];
            for(int i = 0; i < graph.nodes.Count; ++i)
            {
                positions.keysAndPositions[i] = new KeyPosition();
                positions.keysAndPositions[i].id = graph.nodes[i].id;
                positions.keysAndPositions[i].pos = graph.nodes[i].nodeObject.transform.localPosition;
             //   svgCol.svgs[i] = graph.nodes[i].svg;
            }

            string json = JsonUtility.ToJson(positions);
           // Debug.Log(json);
            string filePath =Path.Combine(Application.dataPath, LayoutFile);
           // Debug.Log(filePath);
           if(LayoutFile!="")
            File.WriteAllText(filePath, json);
          //  json = JsonUtility.ToJson(svgCol);
           // File.WriteAllText(Application.dataPath+"/svgs.json", json);


        }

        public void LoadMPDGraph()
        {
         
            WWW jsonUrl = null;
            time = Time.realtimeSinceStartup;
            WWW layoutUrl = new WWW(Path.Combine(Application.dataPath, LayoutFile));
            if (LayoutFile != "")
            {
                StartCoroutine(LoadLayout(layoutUrl));
            }
            StartCoroutine(ProcessJSON(jsonUrl, 1));
        }


        public void LoadMPD()
        {
            var mpdi = GraphFiles.Length - 1;
            GlobalVariables.CurrentFile = GraphFiles[mpdi];
            GlobalVariables.SelectionIndex = si = mpdi;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }


        IEnumerator LoadLayout(WWW www)
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
                foreach(var idAndPos in nodePosArray)
                {
                    nodePosDict.Add(idAndPos.id, idAndPos.pos);
                }

            }
            else if (www != null)
            {
                Debug.Log(www.error);
            }

        
        }


        public void RecalculateLayout()
        {
            Debug.Log("urls: "+url + " " + GlobalVariables.Url);
            if (!GlobalVariables.Init)
            {
                LoadGraph();
            }
            else if (si != GlobalVariables.SelectionIndex)
            {
                Debug.Log("new graph, reload scene");
                GlobalVariables.Init = false;
                GlobalVariables.Reload = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                Debug.Log("after reload");
            }
            else
            {
                Debug.Log("update layout");
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

                                ReadJSON.createEdge(graph.edges,edgeIndices[i], bigVertices, sourcePos, targetPos, offset, offsetOrtho);
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

            
          //Debug.Log(IsCoq + " " + (graph.latestSelection!=-1) );
     
            //for gestures
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





