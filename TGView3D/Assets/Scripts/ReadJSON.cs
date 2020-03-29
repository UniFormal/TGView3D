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
        public Slider ScaleSlider;
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
        public static Dictionary<string, int> ChapterDict = new Dictionary<string, int>();

        List<int> countNodesInGraph = new List<int>();
        public Dropdown EdgeTypeSelector;
        public Dropdown EdgeAttributeSelector;
        public static List<string> VisibleList = new List<string>();
        public static List<string> RootList = new List<string>();
        public GameObject Spinner;
        public bool Spinning;

        //TODO: throw out ugly indexing!!!!! + cleanup class variables
        [System.Serializable]
        public class MyGraph
        {

            public List<MyNode> nodes = new List<MyNode>();
            public List<MyEdge> edges = new List<MyEdge>();
            public List<MyChapter> chapters = new List<MyChapter>();

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
            public float lineWidth = 0.005f;
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
            public string style = "none";
            public string label;
            public string url;
            public string mathml;
            //   [NonSerialized]
            public string color = "";


            [NonSerialized]
            public bool alive = true;


            public float radius = 0;
            [NonSerialized]
            public string svg;
            [NonSerialized]
            public MyEdge fastestEdge;
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
            [NonSerialized]
            public bool active = true;
            [NonSerialized]
            public string parentId;


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
            public List<string> chapters = new List<string>();
            public List<string> nodes = new List<string>();
            public List<string> ogNodes = new List<string>();
            public string label;
            public bool highlevel;
            [NonSerialized]
            public string parentId;
            internal bool isLeaf = false;
            // [NonSerialized]
            // public bool isRoot = false;
        }

        void Start()
        {

            Random.InitState(10);
            Physics.autoSimulation = false;
            GlobalVariables.JsonManager = this;
            ColorDict = new Dictionary<string, Color>();
            ColorDict.Add("include", new Color(0, 255, 0));
            ColorDict.Add("meta", new Color(255, 120, 0));
            ColorDict.Add("alignment", new Color(200, 200, 0));
            ColorDict.Add("view", new Color(0, 0, 255));
            ColorDict.Add("structure", new Color(200, 0, 250));
            ColorDict.Add("chapter", new Color(255, 20, 0));
            ColorDict.Add("partof", new Color(255, 0, 20));

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
                var splitted=  Application.absoluteURL.Split("?"[0]);
                string url = "";
                for(int i = 1; i < splitted.Length-1; ++i)
                {
                    url += splitted[i]+"?";
                }
                if (splitted.Length > 1)
                    url += splitted[splitted.Length - 1];
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
            if (Input.GetKeyDown(KeyCode.F4))
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
                else //if (GlobalVariables.JSON != CurrentJSON)
                {
                    Debug.Log(CurrentJSON);
                    GlobalVariables.JSON = CurrentJSON;
                    CleanupScene();
                    LoadGraph(true);
                }




                transform.eulerAngles = GlobalVariables.Rotation;

            }
        }
        public void SoftResetLayout()
        {
            Debug.Log("new Graph, reload scene");
            GlobalVariables.Init = false;
            GlobalVariables.JSON = CurrentJSON;
            ScaleSlider.value = 1;// ScaleSlider.maxValue * .5f;

            CleanupScene();
            LoadGraph();

        }

        public void ResetLayout()
        {
            Debug.Log("new Graph, reload scene");
            GlobalVariables.Init = false;
            GlobalVariables.JSON = CurrentJSON;
            ScaleSlider.value = 1;// ScaleSlider.maxValue * .5f;
            ChapterDict.Clear();
            RootList.Clear();
            OpenList.Clear();
            VisibleList.Clear();
            CleanupScene();
            LoadGraph();

        }

        public void MergeGraphs()
        {
            var mergePath = "D:/mlibs/AFP/json/";//Application.dataPath + "/Graphs/IsabelleConv/";
            MyGraph megaGraph = new MyGraph();
            int k = 0;
            foreach (var file in Directory.GetFiles(mergePath, "*.json"))
            {

                string content = File.ReadAllText(file);
                var tmpGraph = MyGraph.CreateFromJSON(content);
                string col = "#" + ColorUtility.ToHtmlStringRGB(Random.ColorHSV());


                 //if (k++ > 2000) break;

                if (tmpGraph.nodes.Count > 1)
                {
                    foreach (var node in tmpGraph.nodes)
                    {
                        node.color = col;

                        //   Debug.Log(node.radius);
                    }
                }
       



                megaGraph.nodes.AddRange(tmpGraph.nodes);
                megaGraph.edges.AddRange(tmpGraph.edges);

                megaGraph.chapters.AddRange(tmpGraph.chapters);
            }


            foreach (var node in megaGraph.nodes)
            {
                var parts = node.label.Split('.');
                if (parts.Length > 1)
                {
                    node.label = parts[1];

                    var chapterBefore = megaGraph.chapters.Find(c => c.id == parts[0]);
                    if (chapterBefore == null)
                    {
                        var chapter = new MyChapter
                        {
                            id = parts[0],
                            label = parts[0],
                            highlevel = true
                        };
                        chapter.nodes.Add(node.id);
                        megaGraph.chapters.Add(chapter);
                    }
                    else
                    {
                        chapterBefore.nodes.Add(node.id);
                    }

                }
            }


            for (int i = 0; i < megaGraph.chapters.Count; i++)
            {
                if (megaGraph.chapters[i].nodes.Count < 2)
                {
                    megaGraph.chapters.RemoveAt(i);
                    i--;
                }
            }





            CurrentJSON = JsonUtility.ToJson(megaGraph);
        }

        public void RecalculateLayout()
        {

            Spin();

            StartCoroutine(RecalculateRoutine());



        }

        IEnumerator RecalculateRoutine()
        {
            yield return new WaitForSeconds(.05f);
            if (MyGraph.CreateFromJSON(CurrentJSON).nodes.Count == 0 && MyGraph.CreateFromJSON(CurrentJSON).chapters.Count == 0)
            {
                CurrentJSON = GlobalVariables.UIInteractonManager.GraphFiles[0].text;
            }



            if (CurrentJSON != null)
            {

                if (!GlobalVariables.Init)
                {
                    Debug.Log("first load");
                    GlobalVariables.JSON = CurrentJSON;
                    LoadGraph();
                    GlobalVariables.Init = true;
                    GlobalVariables.UIInteractonManager.Init();
                    Camera.main.transform.parent.localPosition = new Vector3(0, 0, -20);

                    //   StartCoroutine(Camera.main.GetComponent<FlyCamera>().ZoomIn());

                }

                else if (GlobalVariables.JSON != CurrentJSON)
                {

                    transform.eulerAngles = GlobalVariables.Rotation = Vector3.zero;
                    ResetLayout();
                    Camera.main.transform.parent.localPosition = new Vector3(0, 0, -20);

                    //    StartCoroutine(Camera.main.GetComponent<FlyCamera>().ZoomIn());
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
            yield return null;
        }




        public IEnumerator AfterSmallUpdate()
        {
            Debug.Log("spin");
            Spin();
            yield return StartCoroutine(GlobalVariables.GraphManager.SmallUpdate(4));
            var scale = ScaleSlider.value;
            Debug.Log(scale);
            GlobalVariables.Gestures.Init(scale);
            UnSpin();


            //  ScaleSlider.value = ScaleSlider.maxValue * .5f;
            //   transform.GetChild(0).eulerAngles = Vector3.zero;
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
                Debug.Log(www.text);
#if (UNITY_WEBGL && !UNITY_EDITOR)

                if (!Cors)
                {
                    Debug.Log(www.error + " retry with proxy " + ("https://cors-anywhere.herokuapp.com/" + www.url));
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
                    url = "https://mmt.mathhub.info/:jgraph/json?key=archivegraph&uri=" + URLObject.GetComponent<InputField>().text;
                //     + "&semantic=" + SemanticSelect.GetComponent<Dropdown>().options[SemanticSelect.GetComponent<Dropdown>().value].text
                 //    + "&comp=" + ArgSolverSelect.GetComponent<Dropdown>().options[ArgSolverSelect.GetComponent<Dropdown>().value].text;

                }
                else if (mode == 1)
                {

                    url = "localhost:8080/:jgraph/json?key=archivegraph&uri=" + URLObject.GetComponent<InputField>().text;
                   //  + "&semantic=" + SemanticSelect.GetComponent<Dropdown>().options[SemanticSelect.GetComponent<Dropdown>().value].text
                    // + "&comp=" + ArgSolverSelect.GetComponent<Dropdown>().options[ArgSolverSelect.GetComponent<Dropdown>().value].text;
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

        public string LastOpened;

        public static List<string> OpenList = new List<string>();
        private bool TReduce = true;

        public void Spin()
        {
            Spinner.SetActive(true);
            Spinning = true;
        }

        public void UnSpin()
        {
            Spinner.SetActive(false);
            Spinning = false;
        }

        public bool OpenChapter(string id)
        {

            LastOpened = id;
            if (!ChapterDict.ContainsKey(id)) return false;


            var chapter = Graph.chapters[ChapterDict[id]];


            int remaining = 0;
            foreach (var cid in chapter.chapters)
            {
                if (Graph.chapters[ChapterDict[cid]].highlevel)
                {
                    remaining++;
                    break;
                }
            }

            if (OpenList.Contains(id)) return false;
            if (remaining == 0 && chapter.nodes.Count == 0) return false;

            OpenList.Add(id);

            foreach (var cid in Graph.chapters[ChapterDict[id]].chapters)
            {
                if (!VisibleList.Contains(cid) && Graph.chapters[ChapterDict[cid]].highlevel)
                {

                    VisibleList.Add(cid);
                }
                else if (VisibleList.Contains(cid))
                {
                    return false;
                }

            }
            UpdateLayout();
           // UIInteracton.SEnableEdgeType("include");
            GlobalVariables.UIInteractonManager.ChainAttribute();

            Debug.Log("update layout");



            RecalculateLayout();
            return true;
        }




        public bool CloseChapter(string id)
        {


            if (OpenList.Contains(id))
            {
                foreach (var cid in Graph.chapters[ChapterDict[id]].chapters)
                {
                    if (OpenList.Contains(cid))
                        return false;
                }
                foreach (var cid in Graph.chapters[ChapterDict[id]].chapters)
                {
                    VisibleList.Remove(cid);
                }
                //  Debug.LogWarning(OpenList.Count);
                OpenList.Remove(id);
                Debug.LogWarning(OpenList.Count);
                LastOpened = null;

                UpdateLayout();
            //    UIInteracton.SEnableEdgeType("include");
                GlobalVariables.UIInteractonManager.ChainAttribute();

                Debug.Log("update layout");



                RecalculateLayout();

                return true;
            }
            return false;
        }


        void FindRootChapters()
        {


            {




                foreach (var chapter in Graph.chapters)
                {
                    RootList.Add(chapter.id);


                }
                foreach (var chapter in Graph.chapters)
                {
                    foreach (var toChapter in chapter.chapters)
                    {
                        //remove only if present
                        if (RootList.Contains(toChapter))
                            RootList.Remove(toChapter);



                    }
                }
                /*
                if (RootList.Count > 1)
                {
                    var rootChapter = new MyChapter
                    {
                        label = "Root",
                        chapters = RootList,
                        highlevel = true,
                        id = "zaRoot",
                        nodes = new List<string>()

                    };
                    ChapterDict.Add(rootChapter.id, Graph.chapters.Count);
                    Graph.chapters.Add(rootChapter);
      
                    Root = rootChapter.id;

                }else if(RootList.Count == 0)
                Root = RootList[0];  
                */
            }


        }


        public static void CrawlChaptersRec()
        {
            foreach (var root in RootList)
                CrawlChapter(root);

        }

        public static void CrawlChapter(string chapterId)
        {

            var chapter = ChapterDict[chapterId];
            int startChapterCount = Graph.chapters[chapter].chapters.Count;
            for (int i = 0; i < startChapterCount; i++)
            {
                var cid = Graph.chapters[chapter].chapters[i];
                CrawlChapter(cid);
                Graph.chapters[ChapterDict[cid]].parentId = chapterId;
                /*
                foreach (var subNid in Graph.chapters[ChapterDict[cid]].nodes)
                {
                    
                }*/
                //   Graph.chapters[chapter].isLeaf = false;
                if (!VisibleList.Contains(cid))
                {
                    //add nodes of child chapters cid to chapter chapter, only if child chapters cid are hidden ( = not in VisibleList) => only to leaves
                    Graph.chapters[chapter].nodes.AddRange(Graph.chapters[ChapterDict[cid]].nodes);
                    //   Graph.chapters[chapter].isLeaf = true;
                    if (!Graph.chapters[ChapterDict[cid]].highlevel)
                        Graph.chapters[chapter].ogNodes.AddRange(Graph.chapters[ChapterDict[cid]].nodes);
                    /*
                    foreach (var subNid in Graph.chapters[ChapterDict[cid]].nodes)
                    {

                            Graph.chapters[chapter].nodes.Add(subNid);
                    }*/
                }
            }
        }






        public static void ChaptersToNodes(bool keepNodeEdges)
        {
            Debug.Log("chapter conversion, total chapters: " + Graph.chapters.Count);
            //   Graph.edges.Clear();
            //   Graph.nodes.Clear();
            //add nodes

            foreach (var cid in VisibleList)
            {
                var chapter = Graph.chapters[ChapterDict[cid]];
                //not yet converted => not needed graph is rebuilt
                // if (Graph.nodes.FindIndex( node => node.id == chapter.id) <0)
                {
                    string col = "#e6ae25";

                    /*
                               if (chapter.highlevel)// && chapter.isRoot)
                               {
                                   col = "#800080";
                               }
                             //  else if (chapter.highlevel) col = "#000000";//#E9DA89";
                               else col = "505000";
                               //continue;// 
                               */

                    Color avgCol = Color.black;
                    avgCol.a = 0;
                    int nn = 0;
                    Color chapterCol = Random.ColorHSV();
                    foreach (var node in chapter.nodes)
                    {

                        var realNode = Graph.nodes.Find(n => n.id == node);
                        realNode.color = "#" + ColorUtility.ToHtmlStringRGB(chapterCol);
                        ColorUtility.TryParseHtmlString(realNode.color, out Color tmpCol);
                        //   Debug.Log((nn++).ToString() + " "+avgCol+ " "+ tmpCol);
                        avgCol += tmpCol;
                    }

                    avgCol /= chapter.nodes.Count;



                    //Debug.Log(avgCol+ " #"+ ColorUtility.ToHtmlStringRGB(avgCol));



                    foreach (var child in chapter.chapters)
                    {
                        //children are still expandable: is highlevel chapter
                        if (Graph.chapters[ChapterDict[child]].highlevel || chapter.nodes.Count > 0)
                        {
                            if (ColorUtility.ToHtmlStringRGB(avgCol) == "FFFFFF")
                                col = "#ff0000";
                            else
                                col = "#" + ColorUtility.ToHtmlStringRGBA(avgCol);//"#800080";
                            if (chapter.nodes.Count == 0 || OpenList.Contains(cid)) col = "#800080";
                            continue;
                        }
                        //no expandable children found
                        else
                        {
                            col = "c54245";
                        }

                    }


                    Graph.nodes.Add(new MyNode
                    {
                        color = col,
                        label = chapter.label,
                        id = chapter.id,
                        radius = Mathf.Sqrt(chapter.nodes.Count+chapter.chapters.Count) * 0.1f,
                        parentId = chapter.parentId,
                        active = true,
                        style = "chapter"
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




                //  if (!chapter.highlevel) continue;
                foreach (var toChapter in chapter.chapters)
                {
                    if (VisibleList.Contains(toChapter))
                    {
                        //if (!Graph.chapters[ChapterDict[toChapter]].highlevel) continue;
                        {
                            //       Debug.Log("add edge to "+toChapter);

                            //    if (chapter.isRoot&&ChapterDict[toChapter].isRoot)
                            {
                                Graph.edges.Add(new MyEdge
                                {
                                    style = "chapter",
                                    from = chapter.id,
                                    to = toChapter,
                                    id = chapter.id + toChapter,
                                    // active = RootList.Contains(chapter.id)
                                });
                            }
                            /*
                            else
                            {
                                Graph.edges.Add(new MyEdge
                                {
                                    style = "dark",
                                    from = chapter.id,
                                    to = toChapter,
                                    id = chapter.id + toChapter,
                                    // active = RootList.Contains(chapter.id)
                                });
                            }*/


                        }

                    }


                }
            }
            //add edges


            /*above
            foreach (var cid in VisibleList)
            {
                var chapter = Graph.chapters[ChapterDict[cid]];

                //  if (!chapter.highlevel) continue;
                foreach (var toChapter in chapter.chapters)
                {
                    if (VisibleList.Contains(toChapter))
                    {
                        //if (!Graph.chapters[ChapterDict[toChapter]].highlevel) continue;
                        {
                            //       Debug.Log("add edge to "+toChapter);

                            //    if (chapter.isRoot&&ChapterDict[toChapter].isRoot)
                            {
                                Graph.edges.Add(new MyEdge
                                {
                                    style = "chapter",
                                    from = chapter.id,
                                    to = toChapter,
                                    id = chapter.id + toChapter,
                                    // active = RootList.Contains(chapter.id)
                                });
                            }
                            /*
                            else
                            {
                                Graph.edges.Add(new MyEdge
                                {
                                    style = "dark",
                                    from = chapter.id,
                                    to = toChapter,
                                    id = chapter.id + toChapter,
                                    // active = RootList.Contains(chapter.id)
                                });
                            }*


                        }

                    }
                }}*/


            foreach (var cid in OpenList)
            {
                var chapter = Graph.chapters[ChapterDict[cid]];
                foreach (var toChapter in chapter.ogNodes)
                {
                    Graph.nodes.Find(n => n.id == toChapter).parentId = chapter.id;
                    Graph.edges.Add(new MyEdge
                    {
                        style = "partof",
                        from = chapter.id,
                        to = toChapter,
                        id = chapter.id + toChapter,
                    });

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

            List<string> chapters = new List<string>();
            if (Graph.chapters.Count == 0 && Graph.nodes.Count<3000)
            {
              /*  for (int i = 0; i < Graph.nodes.Count; i++)
                {
                    var node = Graph.nodes[i];
                    if (!node.id.Contains("zfc"))
                    {
                        i--;
                        Graph.nodes.Remove(node);
                    }
                }*/
                foreach (var node in Graph.nodes)
                {
                    var splitted = node.id.Split('/');

                    var splitted2 = splitted[splitted.Length - 1].Split('?');
                    string chapterName = splitted2[0];
                    string chapterId = node.id.Split('?')[0];
                    if (!chapters.Contains(chapterId))
                    {
                        chapters.Add(chapterId);
                        Graph.chapters.Add(new MyChapter
                        {
                            id = chapterId,
                            label = chapterName,
                            highlevel = true,
                            nodes = new List<string>(),
                            chapters = new List<string>()
                        });
                    }
                   var chapter = Graph.chapters.Find(c => c.id == chapterId);
                    chapter.nodes.Add(node.id);
                }

                for (int i = 0; i<Graph.chapters.Count;i++)
                {
                    var chapter = Graph.chapters[i];
              //      Debug.Log(chapter.id);
                    var splitted = chapter.id.Split('/');
                    if (splitted.Length <= 2) continue;
                    string chapterName = splitted[splitted.Length-2];

                    string chapterId = "";
                    for(int j= 0; j < splitted.Length - 2; j++)
                    {
                        chapterId += splitted[j] + "/";
                    }
                        
                    chapterId+=chapterName;
                    if (!chapters.Contains(chapterId))
                    {
                        chapters.Add(chapterId);
                        Graph.chapters.Add(new MyChapter
                        {
                            id = chapterId,
                            label = chapterName,
                            highlevel = true,
                            nodes = new List<string>(),
                            chapters = new List<string>()
                        });
                    }
                    var chapter2 = Graph.chapters.Find(c => c.id == chapterId);
                    chapter2.chapters.Add(chapter.id);
                }

        

            }

       

            
            if (Graph.nodes.Count > 0||Graph.chapters.Count>0) Spin();




            foreach (var chapter in Graph.chapters)
            {
                chapter.ogNodes = new List<string>(chapter.nodes);
            }
   
                        
            /*

            int num = Graph.chapters.Count;
            for (int i = 0; i < num; i++)
            {
                var chapter = Graph.chapters[i];
                foreach(var node in chapter.nodes)
                {
                    chapter.chapters.Add(node);
                    Graph.chapters.Add(new MyChapter
                    {
                        id = node,
                        label = node,
                        highlevel = false,
                        nodes = new List<string>(),
                        chapters = new List<string>()
                    });
              
                }

            }*/

            if (ChapterDict.Count == 0 && Graph.chapters.Count != 0)
            {
                Debug.Log("only initial load");
                int k = 0;
                foreach (var chapter in Graph.chapters)
                {
                    ChapterDict.Add(chapter.id, k++);
                }

                FindRootChapters();
                foreach(var Root in RootList)
              //  if (Root != null)
                {
                  //  VisibleList.Add(Root);

                  //  OpenList.Add(Root);
                    /*     foreach (var chapter in Graph.chapters[ChapterDict[Root]].chapters)
                         {
                             VisibleList.Add(chapter);
                             foreach (var subChapter in Graph.chapters[ChapterDict[chapter]].chapters)
                             {
                                 VisibleList.Add(subChapter);
                             }
                         }*/
                }

                foreach (var chapter in Graph.chapters)
                {
                   if (chapter.highlevel) { VisibleList.Add(chapter.id); OpenList.Add(chapter.id); }
                    //if (chapter.highlevel && chapter.chapters.Count > 0) { VisibleList.Add(chapter.id); OpenList.Add(chapter.id); } else if (chapter.highlevel) VisibleList.Add(chapter.id);
                }


            }


            //rework graph.chapters

            if (Graph.chapters.Count != 0)
            {
                if (RootList.Count>0) CrawlChaptersRec();




                /*
                for (int i = 0; i < Graph.chapters.Count; i++)
                {
                    var chapter = Graph.chapters[i];
                    if (!VisibleList.Contains(chapter.id))
                    {
                        Graph.chapters.Remove(chapter);
                        i--;
                    }
                }*/




              //    KillEdges();
                  //KillNodes();

                foreach (var node in Graph.nodes)
                {
                    node.active = false;
                }
               // Debug.LogError(OpenList.Count);
                foreach (var node in Graph.nodes)
                {

                    foreach (var chapterId in OpenList)
                    {

                        var chapter = Graph.chapters[ChapterDict[chapterId]];
                        if (chapter.ogNodes.Contains(node.id))
                        {
                            node.active = true;
                            break;
                        }
                    }
                }


                //bend edges
                
                for (int i = 0; i < Graph.edges.Count; i++)
                {
                    var edge = Graph.edges[i];
                //    foreach(var chapter in Graph.chapters)
                    foreach (var chapterId in VisibleList)
                    {

                        var chapter = Graph.chapters[ChapterDict[chapterId]];
                        // if (edge.style == "instanceof") Debug.Log(chapter.id + " "+ edge.from+"->"+edge.to);
                        foreach (var node in chapter.nodes)
                        {
                            //  if (chapter.ogNodes.Contains(node)) continue;

                        //    Debug.Log(chapter.id);

                            if (edge.from == node && !Graph.nodes.Find(n => n.id == node).active)// && !chapter.nodes.Contains(edge.to))
                            {
                                edge.from = chapter.id;
                                // edge.style = "tmp";
                                // Graph.edges.Remove(edge);
                                // i--;
                            }
                            if (edge.to == node && !Graph.nodes.Find(n => n.id == node).active)// && !chapter.nodes.Contains(edge.from))
                            {
                                edge.to = chapter.id;
                                //   edge.style = "tmp";
                                // Graph.edges.Remove(edge);
                                // i--;
                            }
                        }



                    }
                }

                //remove self edges
                for (int i = 0; i < Graph.edges.Count; i++)
                {
                    var edge = Graph.edges[i];
                    if (edge.from == edge.to)
                    {
                        Graph.edges.Remove(edge);
                        i--;
                    }
                }



                //transforms chapters to nodes and optionally include edges to nodes
                ChaptersToNodes(false);

                if (Graph.chapters.Count > 0)
                    for (int i = 0; i < Graph.nodes.Count; i++)
                    {
                
                        var node = Graph.nodes[i];
                   //     if(node.style!="chapter") Debug.Log(node.id);
                        if (!node.active)
                        {
                            i--;
                            Graph.nodes.Remove(node);
                        }
                    }


                GlobalVariables.Graph = Graph;
            }
               
       

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
          
        //    EdgeTypes.Add("chapter", new EdgeType("tree"));
       //     EdgeTypes.Add("uses", new EdgeType("hierarchic"));
        //    EdgeTypes.Add("dontselect", new EdgeType("")); EdgeTypes.Add("dontselect3", new EdgeType("tree")); EdgeTypes.Add("dontselect2", new EdgeType("hierarchic"));

            //  EdgeTypes.Add("dark", new EdgeType("hierarchic"));
            //   EdgeTypes.Add("dontselect", new EdgeType(""));


            if (!ColorDict.ContainsKey("uses"))
            {

                var col = Color.yellow;
                col.a = 0;


                ColorDict.Add("uses", col * 255);
            }


            if (!ColorDict.ContainsKey("chapter"))
            {

                var col = Color.red;
                col.a = 0;


                ColorDict.Add("chapter", col*255);
            }

            /*
            if (!ColorDict.ContainsKey("dark"))
            {

                var col = Color.black;
                col.a = 0;


                ColorDict.Add("dark", col * 255);
            }*/



            //analyze edge types
            foreach (var edge in Graph.edges)
            {
                if (!EdgeTypes.ContainsKey(edge.style))
                {
                    string type;
                    if (edge.style == "include" || edge.style == "meta" || edge.style == "structure" 
                        //|| edge.style == "uses" 
                        || edge.style == "chapter"|| edge.style == "specifies")
                    {
                        type = "hierarchic";
                    }
                    else if(edge.style == "partof")
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
                      //  Random.InitState(edge.style.Length + edge.style[0]);
                        var rndcol = Random.ColorHSV(0f, 1f, .9f, 1f) * 255;
                        rndcol.a = 0;
                        ColorDict.Add(edge.style, rndcol);
                    }


                }

            }


            string style = "transitivelyReduced";

            if (!EdgeTypes.ContainsKey(style))
            {
                EdgeTypes.Add(style, new EdgeType("hierarchic"));

                if (!ColorDict.ContainsKey(style))
                {
                    var rndcol = Color.black;
                    rndcol.a = 0;
                    ColorDict.Add(style, rndcol);
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


            typeStrings.Add("hierarchic");
            typeStrings.Add("");
            typeStrings.Add("tree");
            EdgeAttributeSelector.AddOptions((typeStrings.Distinct<string>().ToList()));
       
            Graph.nodeDict = new Dictionary<string, int>();
            GlobalVariables.GraphManager.Graph = GlobalVariables.Graph;







            Debug.Log("preprocess time " + (Time.realtimeSinceStartup - time));
            time = Time.realtimeSinceStartup;
            GlobalVariables.GraphManager.ProcessGraph();
            Debug.Log("object creation time " + (Time.realtimeSinceStartup - time));
            time = Time.realtimeSinceStartup;



            if (TReduce)
            {
                foreach (var node in Graph.nodes)
                {
                    foreach (var n in Graph.nodes)
                    {
                        n.visited = false;
                    }
                    node.visited = true;
                    List<MyNode> directNodes = new List<MyNode>();
                    foreach (var eid in node.edgeIndicesOut)
                    {

                        var edge = Graph.edges[eid];
                        if (edge.style == "uses" || edge.style == "include" || edge.style == "structure")
                        {
                            int nid = Graph.nodeDict[edge.to];
                            var cNode = Graph.nodes[nid];
                            cNode.fastestEdge = edge;
                            directNodes.Add(cNode);
                        }

                    }
                    int directCount = directNodes.Count;
                    foreach (var dNode in directNodes)
                    {

                        Stack<MyNode> reachableNodes = new Stack<MyNode>();
                        if (!dNode.visited)
                        {
                            dNode.visited = true;
                            reachableNodes.Push(dNode);
                        }

                        while (reachableNodes.Count > 0)
                        {
                            var rNode = reachableNodes.Pop();
                            rNode.visited = true;
                            foreach (var eid in rNode.edgeIndicesOut)
                            {
                                var edge = Graph.edges[eid];
                                if (edge.style == "uses" || edge.style == "include" || edge.style == "structure")
                                {
                                    int nid = Graph.nodeDict[edge.to];
                                    var cNode = Graph.nodes[nid];
                                    if (!cNode.visited)
                                    {
                                        cNode.visited = true;
                                        reachableNodes.Push(cNode);
                                        if (directNodes.Contains(cNode) && cNode.fastestEdge.style != "transitivelyReduced")
                                        {
                                            cNode.fastestEdge.style = "transitivelyReduced";
                                            directCount--;
                                            if (directCount == 0)
                                            {
                                                reachableNodes.Clear();
                                                break;
                                            }

                                        }
                                    }


                                }

                            }
                        }

                    }

                }
            }
           
           

     


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


          //  Debug.Log("last steps time " + (Time.realtimeSinceStartup - time));

            StartCoroutine(StartGraphManager(keepLayout));


        }

        public IEnumerator StartGraphManager(bool keepLayout)
        {


            yield return StartCoroutine(GlobalVariables.GraphManager.FinishInit(keepLayout));

            if (LastOpened != null && Graph.nodeDict.ContainsKey(LastOpened)) 
            GlobalVariables.MouseManager.SelectNode(Graph.nodeDict[LastOpened]);

            if (!keepLayout)
            {
                StartCoroutine(Camera.main.GetComponent<FlyCamera>().ZoomIn());
               // Spinner.SetActive(false);
            }
        }





    }


}





