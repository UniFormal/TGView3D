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


    public class ReadJSON : MonoBehaviour
    {
        public GameObject Percent;
        public MyGraph graph;
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
        public int iterations = 25;
        public float spaceScale = 1;
    

        public GameObject SemanticSelect;
        public GameObject ArgSolverSelect;
        int si = 0;
        public float time = 0;
        public string url;//http://neuralocean.de/graph/test/nasa.json";
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
        public bool SwapRoots = false;
        private GameObject Aura;
        public static Dictionary<string,string> EdgeTypes = new Dictionary<string, string>() ;
        public static Dictionary<string, Color> ColorDict;

        List<int> countNodesInGraph = new List<int>();
        public Dropdown EdgeTypeSelector;
        public Dropdown EdgeAttributeSelector;


        //TODO: throw out ugly indexing!!!!! + cleanup class variables
        [System.Serializable]
        public class MyGraph
        {
            public List<MyNode> nodes;
            public ReadJSON GraphParser;
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
            public bool UseConstraint = true;
            public bool RootLeaves = true;
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



            public float radius = 0;
            public string svg;
            public Vector3 pos;
            public Vector3 disp;
            public GameObject nodeObject;
            public bool forcesFixed;
            public float range = float.MaxValue;
            public int ClusterId = -1;
            public bool generated;
            public bool visited = false;
            public string color ="";
            //use object references instead?
            public List<int> edgeIndicesOut = new List<int>();
            public List<int> edgeIndicesIn = new List<int>();
            public List<int> connectedNodes = new List<int>();

            public List<float> weights = new List<float>();
            public List<float> inWeights = new List<float>();
            public List<float> outWeights = new List<float>();
            public int GraphNumber;
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
            public string color;
            public bool active = true;
            public int targetCount = 0;
            //public List<MyNestedObject> nestedObjects;


        }

        void Start()
        {




            ColorDict = new Dictionary<string, Color>();
            ColorDict.Add("include", new Color(0, 255, 0));
            ColorDict.Add("meta", new Color(255, 20, 0));
            ColorDict.Add("alignment", new Color(200, 200, 0));
            ColorDict.Add("view", new Color(0, 0, 255));
            ColorDict.Add("structure", new Color(200, 0, 250));


            ColorDict.Add("attack", new Color(220, 0, 200));
            ColorDict.Add("b", new Color(255, 20, 0));
            ColorDict.Add("c", new Color(200, 200, 0));


#if UNITY_WEBGL
            Debug.Log("#################WEBGLBUILD###############################");
            int pm = Application.absoluteURL.IndexOf("?");
            if (pm != -1)
            {
             //var url = "https://mmt.mathhub.info/:jgraph/json?" + Application.absoluteURL.Split("?"[0])[1];
               var url =  Application.absoluteURL.Split("?"[0])[1];

                if (url != "")
                {
                    Debug.Log(url);
                    WWW jsonUrl = new WWW(url);
                    StartCoroutine(ProcessURL(jsonUrl));

                   // URLObject.GetComponent<InputField>().DeactivateInputField();
                }
  
            }
#endif

        }


        public void CleanupScene()
        {
            GraphObject.transform.parent = null;
            GameObject.Destroy(GraphObject);    
        }


        //build graph depending on json file
        public void RecalculateLayout()
        {

            if (CurrentJSON != null)
            {
                if (!GlobalVariables.Init)
                {
                    Debug.Log("first load");
                    GlobalVariables.JSON = CurrentJSON;
                    LoadGraph();
                    GlobalVariables.Init = true;
                    GlobalVariables.UIInteractonManager.Init();
                 
                }

                else if (GlobalVariables.JSON != CurrentJSON)
                {
                    Debug.Log("new Graph, reload scene");
                    GlobalVariables.Init = false;
                    GlobalVariables.JSON = CurrentJSON;
                    CleanupScene();
                    LoadGraph();
                    GameObject.Find("Slider").GetComponent<Slider>().value = GameObject.Find("Slider").GetComponent<Slider>().maxValue * .5f;

                }
                else
                {
                    Debug.Log("update layout");
                    StartCoroutine(GlobalVariables.GraphManager.FinishUpdate());
                    GameObject.Find("Slider").GetComponent<Slider>().value = GameObject.Find("Slider").GetComponent<Slider>().maxValue * .5f;
                }
            }
            else
            {
                Debug.Log("no valid json file");
            }
     

        }





        IEnumerator ProcessURL(WWW www)
        {

            if (www == null)
            {
                yield return null;
            }

            else
                yield return www;

          
         
            // check for errors
            if (www!=null&&www.error == null&& www.text!=null&&www.text!="" && MyGraph.CreateFromJSON(www.text)!=null && MyGraph.CreateFromJSON(www.text).nodes.Count>0)
            {
                Debug.Log("WWW Ok!: " + www.text);
                CurrentJSON= www.text;
                var cols = URLObject.GetComponent<InputField>().colors;
                cols.normalColor = cols.disabledColor = Color.green;
                URLObject.GetComponent<InputField>().colors = cols;
            }
            else if (www!=null)
            {
                Debug.Log(www.error);
                var cols = URLObject.GetComponent<InputField>().colors;
                cols.normalColor = cols.disabledColor = Color.red;
                URLObject.GetComponent<InputField>().colors = cols;
            }
   
        }


        public void BuildFromJSON()
        {
            GlobalVariables.Graph = ReadJSON.MyGraph.CreateFromJSON(CurrentJSON);
            GlobalVariables.Graph.GraphParser = this;
            InitGraph();
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
                    url = "https://mmt.mathhub.info/:jgraph/json?key=archivegraph&uri=" + URLObject.GetComponent<InputField>().text
                     + "&semantic=" + SemanticSelect.GetComponent<Dropdown>().options[SemanticSelect.GetComponent<Dropdown>().value].text
                     + "&comp=" + ArgSolverSelect.GetComponent<Dropdown>().options[ArgSolverSelect.GetComponent<Dropdown>().value].text;

                }
                else if (mode == 1)
                {

                    url = "localhost:8080/:jgraph/json?key=archivegraph&uri=" + URLObject.GetComponent<InputField>().text
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


  


        public void LoadGraph()
        {

            GraphObject =Instantiate(Resources.Load<GameObject>("Graph"));
            GraphObject.transform.parent = this.transform;
            time = Time.realtimeSinceStartup;

            BuildFromJSON();
   
        }

    

       



        public void ChangeFile(InputField f)
        {
            LayoutFile = f.text;

        }


        private void InitGraph()
        {
            graph = GlobalVariables.Graph;

            graph.nodes = graph.nodes
            .GroupBy(customer => customer.id)
            .Select(group => group.First()).ToList();
            GlobalVariables.Vol = vol;

            Debug.Log("nodes edges" + graph.nodes.Count + " " + graph.edges.Count);

            if (SwapRoots)
            {
                foreach (var edge in graph.edges)
                {
                    var tmp = edge.from;
                    edge.from = edge.to;
                    edge.to = tmp;
                }
            }

  

            graph.movingNodes = new List<int>();
            graph.selectedNodes = new List<int>();
            graph.selectedNodes.Add(-1);
            graph.selectedNodes.Add(-1);


            EdgeTypes.Clear();
            EdgeTypes.Add("include", "include");
            EdgeTypes.Add("dontselect", "");

            foreach (var edge in graph.edges)
             {
                if (!EdgeTypes.ContainsKey(edge.style))
                {
                    string type;
                    if (edge.style == "include" || edge.style == "meta" || edge.style == "structure")
                    {
                       type = "include";
                    }

                    else
                    {
                       type = "";
                    }


                    EdgeTypes.Add(edge.style,type);

                    if (!ColorDict.ContainsKey(edge.style)){
                        Random.InitState(edge.style.Length+edge.style[0]);
                        var rndcol = Random.ColorHSV(0f,1f,.9f,1f) * 255;
                        rndcol.a = 0;
                        ColorDict.Add(edge.style,rndcol);
                    }


                }

            }

            EdgeTypeSelector.ClearOptions();
            EdgeAttributeSelector.ClearOptions();
            EdgeTypeSelector.AddOptions((EdgeTypes.Keys).ToList<string>());
            EdgeAttributeSelector.AddOptions((EdgeTypes.Values.Distinct<string>().ToList()));


            graph.nodeDict = new Dictionary<string, int>();
            Debug.Log("setup time " + (Time.realtimeSinceStartup - time));

            
            GlobalVariables.GraphManager.Graph = GlobalVariables.Graph;

            GlobalVariables.GraphManager.ProcessGraph();


            Material mat1 = new Material(mat);
            mat1.color = Color.red;
            Material mat2 = new Material(mat);
            mat2.color = Color.yellow;
            Material mat3 = new Material(mat);
            mat3.color = Color.green;

            Dictionary<string, Material> materialDict = new Dictionary<string, Material>();

            foreach (var node in graph.nodes)
            {

                if (node.style == "skeptically_accepted" || node.style == "sceptically_accepted")
                    node.nodeObject.GetComponent<Renderer>().material = mat3;
                else if (node.style == "credulously_accepted")
                    node.nodeObject.GetComponent<Renderer>().material = mat2;
                else if (node.style == "rejected")
                    node.nodeObject.GetComponent<Renderer>().material = mat1;
                else
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString(node.color, out color))
                    {
                        if (materialDict.ContainsKey(node.color))
                        {
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial = materialDict[node.color];

                        }
                        else
                        {
                            var genMat = new Material(mat1);
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial = genMat;
                            node.nodeObject.GetComponent<Renderer>().sharedMaterial.color = color;
                            materialDict.Add(node.color, genMat);
                        }

                    }

                }
            }

            int ec = 0;
            foreach (var edge in graph.edges)
            {
                if (edge.active) ec++;
            }

            Debug.Log("prep time " + (Time.realtimeSinceStartup - time));
            Debug.Log(graph.nodes.Count + " " + ec + "-----------------------------------------------------");
        //    GraphManager.Init();


            StartCoroutine(GlobalVariables.GraphManager.FinishInit());
     


        }







    }


}





