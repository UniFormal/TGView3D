using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Random = UnityEngine.Random;




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

    public class EdgeType
    {
        public string type;
        public bool active = true;

        public EdgeType(string type)
        {
            this.type = type;
        }
    }


    public class ReadJSON : MonoBehaviour
    {



        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void download(string data, string strFileName, string strMimeType);

        public GameObject Percent;
        public static MyGraph Graph;
        public Material mat;
        public Material lineMat;
        public GameObject grabbable;
        public GameObject URLObject;
        public bool onlyInclude = false;
        public GameObject EventSystem;
        public bool recursive = false;
        public float globalWeight;
        public static string CurrentJSON;
        private GameObject GraphObject;
        private bool Reading = true;
        private bool Cors = false;

        public GameObject SemanticSelect;
        public GameObject ArgSolverSelect;
        int si = 0;
        public float time = 0;
        public string url;//http://neuralocean.de/Graph/test/nasa.json";
        public string path;
        public int vol = 100;
        private static string LayoutFile = "";
        public static Dictionary<string, Vector3> nodePosDict;
        SVGCollection svgCol;
        public TextAsset SVGFile;

        public static Color BaseColor = Color.white;
        public static Color SelectedColor = Color.cyan;
        public static Color ConnectedColor = Color.yellow;
        public static Color TargetColor = Color.red;
        public static bool IsCoq = false;
        public static bool IsAG = false;
        public static List<MyNode> FoundNodes;
        public bool Gen = false;
        public bool SwapRoots = true;
        private GameObject Aura;
        public static Dictionary<string, EdgeType> EdgeTypes = new Dictionary<string, EdgeType>();
        public static Dictionary<string, Color> ColorDict;
        public static Dictionary<string, MyChapter> ChapterDict = new Dictionary<string, MyChapter>();

        List<int> countNodesInGraph = new List<int>();
        public Dropdown EdgeTypeSelector;
        public Dropdown EdgeAttributeSelector;
        public static List<string> RootList = new List<string>();

        //TODO: throw out ugly indexing!!!!! + cleanup class variables
        [System.Serializable]
        public class MyGraph
        {

            public List<MyNode> nodes;
            public List<MyEdge> edges;
            public List<MyChapter> chapters;

            [NonSerialized]
            public ReadJSON GraphParser;
            [NonSerialized]
            public NativeArray<Vector3> Disps;
            [NonSerialized]
            public NativeArray<Vector3> Positions;
            [NonSerialized]
            public Dictionary<string, int> nodeDict;
            [NonSerialized]
            public List<MyEdge> tmpEdges;
            [NonSerialized]
            public List<int> subEdges;
            [NonSerialized]
            public int handIndex = 0;

            //use object references instead?
            [NonSerialized]
            public List<int> selectedNodes;
            [NonSerialized]
            public List<int> SelectedEdges = new List<int>();
            [NonSerialized]
            public List<int> movingNodes;
            [NonSerialized]
            public int latestSelection = -1;
            [NonSerialized]
            public int currentTarget = -1;
            [NonSerialized]
            public int subGraphOrign = -1;
            [NonSerialized]
            public int fin = 0;
            [NonSerialized]
            public GameObject edgeObject;
            [NonSerialized]
            public int modus = 0;
            [NonSerialized]
            public GameObject subObject;
            [NonSerialized]
            public float lineWidth = 0.0075f;
            [NonSerialized]
            public bool UseForces = true;
            [NonSerialized]
            public bool WaterMode = true;
            [NonSerialized]
            public bool FlatInit = false;
            [NonSerialized]
            public bool HeightInit = false;
            [NonSerialized]
            public bool UseConstraint = true;
            [NonSerialized]
            public bool RootLeaves = true;
            [NonSerialized]
            public float PushLimit = 1f;



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
         //   [NonSerialized]
            public string color = "";


            [NonSerialized]
            public bool alive = true;

            [NonSerialized]
            public float radius = 0;
            [NonSerialized]
            public string svg;
            [NonSerialized]
            public Vector3 pos;
            [NonSerialized]
            public Vector3 disp;
            [NonSerialized]
            public GameObject nodeObject;
            [NonSerialized]
            public bool forcesFixed;
            [NonSerialized]
            public float range = float.MaxValue;
            [NonSerialized]
            public int ClusterId = -1;
            [NonSerialized]
            public bool generated;
            [NonSerialized]
            public bool visited = false;
 
            //use object references instead?
            [NonSerialized]
            public List<int> edgeIndicesOut = new List<int>();
            [NonSerialized]
            public List<int> edgeIndicesIn = new List<int>();
            [NonSerialized]
            public List<int> connectedNodes = new List<int>();
            [NonSerialized]
            public List<float> weights = new List<float>();
            [NonSerialized]
            public List<float> inWeights = new List<float>();
            [NonSerialized]
            public List<float> outWeights = new List<float>();
            [NonSerialized]
            public int GraphNumber;
            [NonSerialized]
            public float height = 0;
            [NonSerialized]
            public float weight = 0;
            [NonSerialized]
            public GameObject nodeEdgeObject;
            [NonSerialized]
            public GameObject labelObject;
            [NonSerialized]
            public bool selected = false;
            [NonSerialized]
            public int nr;

            public GameObject GetObject()
            {
                return nodeObject;
            }



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

            [NonSerialized]
            public bool alive = true;
            [NonSerialized]
            public float localIdx = 0;
            [NonSerialized]
            public GameObject line;
            [NonSerialized]
            public GameObject labelObject;
            [NonSerialized]
            public string color;
            [NonSerialized]
            public bool active = true;
            [NonSerialized]
            public int targetCount = 0;
            //public List<MyNestedObject> nestedObjects;
            public GameObject GetObject()
            {
                Debug.Log(GlobalVariables.Graph.edgeObject);
                return GlobalVariables.Graph.edgeObject;
            }

        }


        [System.Serializable]
        public class MyChapter
        {
            public string id;
            public List<string> chapters;
            public List<string> nodes;
            public string label;
        }

            void Start()
        {


            Physics.autoSimulation = false;
            GlobalVariables.JsonManager = this;
            ColorDict = new Dictionary<string, Color>();
            ColorDict.Add("include", new Color(0, 255, 0));
            ColorDict.Add("meta", new Color(255, 20, 0));
            ColorDict.Add("alignment", new Color(200, 200, 0));
            ColorDict.Add("view", new Color(0, 0, 255));
            ColorDict.Add("structure", new Color(200, 0, 250));


            ColorDict.Add("attack", new Color(220, 0, 200));
            ColorDict.Add("b", new Color(255, 20, 0));
            ColorDict.Add("c", new Color(200, 200, 0));



            Graph = new MyGraph();
            Graph.nodes = new List<MyNode>();
            Graph.edges = new List<MyEdge>();
            var tmpGraph = MyGraph.CreateFromJSON(JsonUtility.ToJson(Graph));

            var json = JsonUtility.ToJson(tmpGraph);
            CurrentJSON = json;

            UpdateLayout();

#if UNITY_WEBGL
            Debug.Log("#################WEBGLBUILD###############################");

            int pm = Application.absoluteURL.IndexOf("?");
            if (pm != -1)
            {
                //var url = "https://mmt.mathhub.info/:jGraph/json?" + Application.absoluteURL.Split("?"[0])[1];
                var url = Application.absoluteURL.Split("?"[0])[1];

                if (url != "")
                {

                    Debug.Log(url);
                    //   WWW jsonUrl = new WWW("https://cors-anywhere.herokuapp.com/" + url);
                    WWW jsonUrl = new WWW(url);
                    StartCoroutine(ProcessURL(jsonUrl));
                    StartCoroutine(LoadIfReady());
                    // URLObject.GetComponent<InputField>().DeactivateInputField();
                }

            }
#endif


            // AddNode(false);
            //AddEdge(Graph.nodes[0], Graph.nodes[1],false);


            //BuildFromJSON();

        }


        IEnumerator LoadIfReady()
        {
            yield return new WaitUntil(() => !Reading);
            RecalculateLayout();

        }

        public void CleanupScene()
        {
            Camera.main.GetComponent<FlyCamera>().Startid = "";
            GraphObject.transform.parent = null;
            GameObject.Destroy(GraphObject);

        }


        //build Graph depending on json file



        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //   UpdateJson();
                UpdateLayout();
            }

        }

        public void UpdateJSON()
        {
            if (SwapRoots)
            {
            
                foreach (var edge in GlobalVariables.Graph.edges)
                {
                   
                    var tmp = edge.from;
                    edge.from = edge.to;
                    edge.to = tmp;
                

                }

            }
            var json = JsonUtility.ToJson(Graph);
            CurrentJSON = json;
        }

        public void UpdateLayout()
        {

            transform.eulerAngles = Vector3.zero;
            nodePosDict = new Dictionary<string, Vector3>();
            foreach (var node in Graph.nodes)
            {
                nodePosDict.Add(node.id, node.nodeObject.transform.localPosition);
            }

            GlobalVariables.IdToPosition = nodePosDict;

            if (CurrentJSON != null)
            {
                if (!GlobalVariables.Init)
                {
                    Debug.Log("first load");
                    GlobalVariables.JSON = CurrentJSON;
                    LoadGraph(true);
                    GlobalVariables.Init = true;
                    GlobalVariables.UIInteractonManager.Init();


                }
                else
                {
                    Debug.Log(CurrentJSON);
                    GlobalVariables.JSON = CurrentJSON;
                    CleanupScene();
                    LoadGraph(true);
                }



                transform.eulerAngles = GlobalVariables.Rotation;

            }
        }

        public void ResetLayout()
        {
            Debug.Log("new Graph, reload scene");
            GlobalVariables.Init = false;
            GlobalVariables.JSON = CurrentJSON;
            CleanupScene();
            LoadGraph();
            GameObject.Find("Slider").GetComponent<Slider>().value = GameObject.Find("Slider").GetComponent<Slider>().maxValue * .5f;
        }


        public void RecalculateLayout()
        {

            if (MyGraph.CreateFromJSON(CurrentJSON).nodes.Count == 0)
            {
                CurrentJSON = GlobalVariables.UIInteractonManager.GraphFiles[0].text;
            }

            if (CurrentJSON != null)
            {
                transform.eulerAngles = GlobalVariables.Rotation = Vector3.zero;
                if (!GlobalVariables.Init)
                {
                    Debug.Log("first load");
                    GlobalVariables.JSON = CurrentJSON;
                    LoadGraph();
                    GlobalVariables.Init = true;
                    GlobalVariables.UIInteractonManager.Init();
                    Camera.main.transform.parent.localPosition = new Vector3(0, 0, -20);

                    StartCoroutine(Camera.main.GetComponent<FlyCamera>().ZoomIn());

                }

                else if (GlobalVariables.JSON != CurrentJSON)
                {

                    ResetLayout();
                    Camera.main.transform.parent.localPosition = new Vector3(0, 0, -20);

                    StartCoroutine(Camera.main.GetComponent<FlyCamera>().ZoomIn());
                }
                else
                {
                    Debug.Log("update layout");
                    StartCoroutine(AfterSmallUpdate());
              
                }

            }
            else
            {
                Debug.Log("no valid json file");
            }


        }

        public IEnumerator AfterSmallUpdate()
        {
            yield return StartCoroutine(GlobalVariables.GraphManager.SmallUpdate());
            GlobalVariables.Gestures.Init();
            GameObject.Find("Slider").GetComponent<Slider>().value = GameObject.Find("Slider").GetComponent<Slider>().maxValue * .5f;
            transform.GetChild(0).eulerAngles = Vector3.zero;
        }





        public void AddNode(bool build)
        {
            var node = new MyNode();
            node.label = "test";
            node.style = "rejected";
            node.id = Graph.nodes.Count.ToString();
            Graph.nodes.Add(node);
            var json = JsonUtility.ToJson(Graph);
            CurrentJSON = json;
            Debug.Log(node.id);
            int i = 0;

            while ((Graph.nodeDict.ContainsKey(node.id)))
            {
                node.id = i.ToString();
                i++;
            }


            if (build) GlobalVariables.GraphManager.AddNode(node.id);
        }









        public void AddEdge(MyNode from, MyNode to, bool build = true)
        {
            // style;
            // label;
            // url;
            // clickText;
            var edge = new MyEdge();
            if (SwapRoots)
            {

                edge.to = from.id;
                edge.from = to.id;
            }
            else
            {

                edge.from = from.id;
                edge.to = to.id;
            }
            edge.style = "include";
            edge.label = "boom";
            edge.id = "customedge";
            Debug.Log(Graph.edges.Count);
            Graph.edges.Add(edge);
            Debug.Log(Graph.edges.Count);
            var json = JsonUtility.ToJson(Graph);
            CurrentJSON = json;
            // from.edgeIndicesOut.Add(Graph.edges.Count-1);
            // to.edgeIndicesIn.Add(Graph.edges.Count-1);

            if (build) GlobalVariables.GraphManager.AddEdge(from, to);


        }



        IEnumerator ProcessURL(WWW www)
        {

            if (www == null)
            {
                yield return null;
            }

            else
                yield return www;

            Debug.Log(www.url);

            // check for errors
            if (www != null && www.error == null && www.text != null && www.text != "" && MyGraph.CreateFromJSON(www.text) != null && MyGraph.CreateFromJSON(www.text).nodes.Count > 0)
            {
                Debug.Log("WWW Ok!: " + www.text);
                CurrentJSON = www.text;
                var cols = URLObject.GetComponent<InputField>().colors;
                cols.normalColor = cols.disabledColor = Color.green;
                URLObject.GetComponent<InputField>().colors = cols;
                Reading = false;
                Cors = false;
            }
            else if (www != null)
            {
               
#if (UNITY_WEBGL)

                if (!Cors)
                {
                    Debug.Log(www.error + " retry with proxy "+("https://cors-anywhere.herokuapp.com/" + www.url) );
                    Cors = true;
                    StartCoroutine(ProcessURL(new WWW("https://cors-anywhere.herokuapp.com/" + www.url)));
                    yield break;
                }
                else
                {
                    Cors = false;
                    var cols = URLObject.GetComponent<InputField>().colors;
                    cols.normalColor = cols.disabledColor = Color.red;
                    URLObject.GetComponent<InputField>().colors = cols;
                }

#endif

            }

        }

        public void ExportJSON()
        {

            var json = "";
            if (SwapRoots)
            {
                MyGraph tmpGraph = MyGraph.CreateFromJSON(JsonUtility.ToJson(Graph));
                foreach (var edge in tmpGraph.edges)
                {

                    string from = edge.from;
                    edge.from = edge.to;
                    edge.to = from;
                }
                json = JsonUtility.ToJson(tmpGraph);
            }
            else
            {

                json = JsonUtility.ToJson(Graph);
            }



#if UNITY_WEBGL && !UNITY_EDITOR

            download(json, "Graph.json", "text/plain");

#else

            string filePath = Application.dataPath + "/GraphExp.json";
            File.WriteAllText(filePath, json);

#endif

        }

        public void BuildFromJSON(bool keepLayout = false)
        {
            GlobalVariables.Graph = ReadJSON.MyGraph.CreateFromJSON(CurrentJSON);
            GlobalVariables.Graph.GraphParser = this;
            InitGraph(keepLayout);
        }




        public void WebBrowserLoad(string s)
        {
            CurrentJSON = s;
            RecalculateLayout();
        }

        public static void ReadJsonFromPath(string path)
        {
            StreamReader reader = new StreamReader(path);
            CurrentJSON = reader.ReadToEnd();
        }




        public void ReadJsonFromURL()
        {

            if (URLObject.GetComponent<InputField>().text != "")
            {
                var mode = GlobalVariables.UrlMode;

                if (mode == 0)
                {
                    url = "https://mmt.mathhub.info/:jGraph/json?key=archiveGraph&uri=" + URLObject.GetComponent<InputField>().text
                     + "&semantic=" + SemanticSelect.GetComponent<Dropdown>().options[SemanticSelect.GetComponent<Dropdown>().value].text
                     + "&comp=" + ArgSolverSelect.GetComponent<Dropdown>().options[ArgSolverSelect.GetComponent<Dropdown>().value].text;

                }
                else if (mode == 1)
                {

                    url = "localhost:8080/:jGraph/json?key=archiveGraph&uri=" + URLObject.GetComponent<InputField>().text
                     + "&semantic=" + SemanticSelect.GetComponent<Dropdown>().options[SemanticSelect.GetComponent<Dropdown>().value].text
                     + "&comp=" + ArgSolverSelect.GetComponent<Dropdown>().options[ArgSolverSelect.GetComponent<Dropdown>().value].text;
                }
                else
                {
                    url = URLObject.GetComponent<InputField>().text;
                }

                WWW jsonUrl = new WWW(url);

                StartCoroutine(ProcessURL(jsonUrl));

            }



        }





        public void LoadGraph(bool keepLayout = false)
        {

            GraphObject = Instantiate(Resources.Load<GameObject>("Graph"));
            GraphObject.transform.parent = this.transform;
            time = Time.realtimeSinceStartup;

            BuildFromJSON(keepLayout);

        }







        public void ChangeFile(InputField f)
        {
            LayoutFile = f.text;

        }



        void FindRootChapters()
        {

            if (ChapterDict.Count == 0)
            {
                foreach (var chapter in Graph.chapters)
                {
                    RootList.Add(chapter.id);
                }
                Debug.Log("rootchapters");
                foreach (var chapter in Graph.chapters)
                {
                    ChapterDict.Add(chapter.id, chapter);
                }
                /*
                foreach (var chapter in Graph.chapters)
                {
                    foreach (var toChapter in chapter.chapters)
                    {
                        if (RootList.Contains(toChapter))
                            RootList.Remove(toChapter);
                    }
                }*/

            }

        }


        public static void ChaptersToNodes()
        {
            Debug.Log("chapter conversion");
         //   Graph.edges.Clear();
         //   Graph.nodes.Clear();
            //add nodes
            foreach (var chapter in Graph.chapters)
            {
                if (Graph.nodes.FindIndex( node => node.id == chapter.id) <0)
                {
                    Graph.nodes.Add(new MyNode
                    {
                        color = "#800080",
                        label = chapter.label,
                        id = chapter.id
                    });
                }
           
                /*
                foreach (var chapterId in chapter.chapters)
                {
                    var toChapter = ChapterDict[chapterId];
                    Graph.nodes.Add(new MyNode
                    {
                        color = "#800080",
                        label = toChapter.label,
                        id = toChapter.id
                    });
                }*/
            }
            //add edges
            Debug.Log("rootcount "+ RootList.Count);
            foreach (var chapter in Graph.chapters)
            {
                if (!RootList.Contains(chapter.id)) continue;
                foreach (var toChapter in chapter.chapters)
                {
                    if (Graph.edges.FindIndex(edge => edge.id == chapter.id + toChapter) < 0)
                    {
                 //       Debug.Log("add edge to "+toChapter);
                        Graph.edges.Add(new MyEdge
                        {
                            style = "chapter",
                            from = chapter.id,
                            to = toChapter,
                            id = chapter.id + toChapter,
                        });
                    }
                }
/*
                foreach (var toChapter in chapter.nodes)
                {
                    Graph.edges.Add(new MyEdge
                    {
                        style = "tmp",
                        from = chapter.id,
                        to = toChapter,
                        id = chapter.id + toChapter,
                    });
                }*/
                
            }
        }

        void ProcessChapters()
        {

            FindRootChapters();
            ChaptersToNodes();
        }


        public void ClusterChapters()
        {
            List<KeyValuePair<MyNode, List<MyNode>>> chapterNodes = new List<KeyValuePair<MyNode, List<MyNode>>>();

            foreach (var chapter in Graph.chapters)
            {
                
                //if(chapter.chapters.Count == 0)
                {

                    var chapterNode = new MyNode
                    {
                        color = "#800080",
                        label = chapter.label,
                        id = chapter.id
                    };

                    List<MyNode> collectedNodes = new List<MyNode>();
                    foreach (var nodeId in chapter.nodes)
                    {
                        var node = Graph.nodes.Find(n => n.id == nodeId);
                        if (node != null)
                        {
                            collectedNodes.Add(node);
                            chapterNode.radius += .15f * (1 + node.radius);
                        }
       

                    }
                    foreach (var nodeId in chapter.chapters)
                    {
                        var node = Graph.nodes.Find(n => n.id == nodeId);
                        if (node != null)
                        {
                            collectedNodes.Add(node);
                            chapterNode.radius += .15f * (1 + node.radius);
                        }


                    }

                    if (collectedNodes.Count > 0)
                    {
                        Graph.nodes.Add(chapterNode);
                        chapterNodes.Add(new KeyValuePair<MyNode, List<MyNode>>(chapterNode, collectedNodes));
                    }

                }
            }


    

            for (int i = 0; i < Graph.edges.Count; i++)
            {
                foreach (var chapterNode in chapterNodes)
                {
                    foreach (var collectedNode in chapterNode.Value)
                    {
                        var edge = Graph.edges[i];
                        /*  if(edge.from == collectedNode.id)
                          {
                              Graph.edges.Add(new MyEdge
                              {
                                  style = "chapter",
                                  from = chapterNode.Key.id,
                                  to = edge.to,
                                  id = edge.id,
                              });
                          }*/
                        if (edge.from == collectedNode.id)
                        {
                            edge.from = chapterNode.Key.id;
                           // edge.style = "tmp";
                            // Graph.edges.Remove(edge);
                            // i--;
                        }
                        if (edge.to == collectedNode.id)
                        {
                            edge.to = chapterNode.Key.id;
                         //   edge.style = "tmp";
                            // Graph.edges.Remove(edge);
                            // i--;
                        }

                    }
                }
            }

            foreach (var chapterNode in chapterNodes)
            {
                foreach (var collectedNode in chapterNode.Value)
                {
                    Graph.nodes.Remove(collectedNode);
                }
            }



        }

        void KillEdges()
        {
            for (int i = 0; i < Graph.edges.Count; i++)
            {
                var edge = Graph.edges[i];
                if (edge.style != "chapter")
                {
                    Graph.edges.Remove(edge);
                    i--;
                }
            }
        }
        void KillNodes()
        {

            for (int i = 0; i < Graph.nodes.Count; i++)
            {
                var node = Graph.nodes[i];
                if (!ChapterDict.ContainsKey(node.id))
                {
                    Graph.nodes.Remove(node);
                    i--;
                }
            }
        }

        private void InitGraph(bool keepLayout = false)
        {
            Graph = GlobalVariables.Graph;


            // KillEdges();
            // KillNodes();
            Debug.Log(Graph.nodes.Count);
            for(int i = 0; i < 2; i++)
            {
                ClusterChapters();
                Debug.Log(Graph.nodes.Count);

            }


            for (int i = 0; i < Graph.edges.Count; i++)
            {
                var edge = Graph.edges[i];
                if(edge.from == edge.to)
                {
                    Graph.edges.Remove(edge);
                    i--;
                }
            }

             ProcessChapters();
            GlobalVariables.Graph = Graph;
       

            /*  Graph.nodes = Graph.nodes
              .GroupBy(customer => customer.id)
              .Select(group => group.First()).ToList();*/
            GlobalVariables.Vol = vol;

            Debug.Log("Init Graph, nodes edges " + Graph.nodes.Count + " " + Graph.edges.Count +" SwapRoots "+SwapRoots);

            if (SwapRoots)
            {
                foreach (var edge in Graph.edges)
                {
                   // Debug.Log(edge.from + " to " + edge.to);
                    var tmp = edge.from;
                    edge.from = edge.to;
                    edge.to = tmp;// Debug.Log(edge.from + " " + edge.to);
                }

            }

            Graph.movingNodes = new List<int>();
            Graph.selectedNodes = new List<int>();
            //     Graph.selectedNodes.Add(-1);
            //     Graph.selectedNodes.Add(-1);


            EdgeTypes.Clear();
            EdgeTypes.Add("include", new EdgeType("hierarchic"));
            EdgeTypes.Add("chapter", new EdgeType("hierarchic"));
            EdgeTypes.Add("nondir", new EdgeType(""));
            //   EdgeTypes.Add("dontselect", new EdgeType(""));


            if (!ColorDict.ContainsKey("chapter"))
            {

                var col = Color.blue;
                col.a = 0;


                ColorDict.Add("nondir", col * 255);
            }


            if (!ColorDict.ContainsKey("chapter"))
            {

                var col = Color.red;
                col.a = 0;


                ColorDict.Add("chapter", col*255);
            }


            //analyze edge types
            foreach (var edge in Graph.edges)
            {
                if (!EdgeTypes.ContainsKey(edge.style))
                {
                    string type;
                    if (edge.style == "include" || edge.style == "meta" || edge.style == "structure")
                    {
                        type = "hierarchic";
                    }

                    else
                    {
                        type = "";
                    }


                    EdgeTypes.Add(edge.style, new EdgeType(type));

                    if (!ColorDict.ContainsKey(edge.style))
                    {
                        Random.InitState(edge.style.Length + edge.style[0]);
                        var rndcol = Random.ColorHSV(0f, 1f, .9f, 1f) * 255;
                        rndcol.a = 0;
                        ColorDict.Add(edge.style, rndcol);
                    }


                }

            }

            EdgeTypeSelector.ClearOptions();
            EdgeAttributeSelector.ClearOptions();
            EdgeTypeSelector.AddOptions((EdgeTypes.Keys).ToList<string>());

            var types = EdgeTypes.Values;
            var typeStrings = new List<string>();
            foreach (var type in types)
            {
                typeStrings.Add(type.type);
            }

            EdgeAttributeSelector.AddOptions((typeStrings.Distinct<string>().ToList()));
            Graph.nodeDict = new Dictionary<string, int>();
            GlobalVariables.GraphManager.Graph = GlobalVariables.Graph;







            Debug.Log("init time " + (Time.realtimeSinceStartup - time));
            time = Time.realtimeSinceStartup;
            GlobalVariables.GraphManager.ProcessGraph();
            Debug.Log("process time " + (Time.realtimeSinceStartup - time));

            Material mat1 = new Material(mat);
            mat1.color = Color.red;
            Material mat2 = new Material(mat);
            mat2.color = Color.yellow;
            Material mat3 = new Material(mat);
            mat3.color = Color.green;

            Dictionary<string, Material> materialDict = new Dictionary<string, Material>();
             

            //set node materials
            foreach (var node in Graph.nodes)
            {
                //Debug.Log(node.id+" "+node.color + " " + node.style);
                if (node.style == "skeptically_accepted" || node.style == "sceptically_accepted")
                    node.nodeObject.GetComponent<Renderer>().material = mat3;
                else if (node.style == "credulously_accepted")
                    node.nodeObject.GetComponent<Renderer>().material = mat2;
                else if (node.style == "rejected")
                    node.nodeObject.GetComponent<Renderer>().material = mat1;
                else
                {
                    //Debug.Log("no static style "+node.color);  
                    if (ColorUtility.TryParseHtmlString(node.color, out Color color))
                    {
                    //    Debug.Log(color);
                        if (materialDict.ContainsKey(node.color))
                        {
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial = materialDict[node.color];

                        }
                        else
                        {
                           // Debug.LogWarning("weird color? " + color);
                            var genMat = new Material(mat1);
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial = genMat;
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial.color = color;
                            materialDict.Add(node.color, genMat);
                        }

                    }

                }



            }
            /*
         int ec = 0;
         foreach (var edge in Graph.edges)
         {
             if (edge.active) ec++;
         }*/

            //  Debug.Log(Graph.nodes.Count + " " + ec + "-----------------------------------------------------");
            //    GraphManager.Init();


            StartCoroutine(GlobalVariables.GraphManager.FinishInit(keepLayout));



        }







    }


}





