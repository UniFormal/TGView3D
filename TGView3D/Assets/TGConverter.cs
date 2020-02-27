using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class TGConverter : MonoBehaviour
{

    public TextAsset RDFFile;
    public TextAsset CoqFile;
    static string Path; 
    // Start is called before the first frame update
    public string Rdf =  "https://gl.mathhub.info/Coq/mathcomp-bigenough/raw/master/triples.rdf";




    [System.Serializable]
    public class CoqNode
    {
        public string id;
        public List<RDFID> uses;
        public List<RDFID> declares;
        public List<RDFID> HasInHypothesis;
        public List<RDFID> HasMainHypothesis;
        public List<RDFID> HasMainConclusion;
        public RDFID title;
        public RDFID Target;
    }


    [System.Serializable]
    public class RDFID
    {
        public string id;

        public RDFID(string id)
        {
            this.id = id;
        }
    }


    [System.Serializable]
    public class CoqGraph
    {
        public List<CoqNode> graph;
    }
    [System.Serializable]
    public class CoqGraphSingle
    {
        public List<CoqNodeSingle> graph;
    }

    [System.Serializable]
    public class CoqNodeSingle
    {
        public RDFID uses;
        public RDFID declares;
        public RDFID HasInHypothesis;
        public RDFID HasMainHypothesis;
        public RDFID HasMainConclusion;
    }
   

    [System.Serializable]
    public class RDFNodeSingle
    {
        public RDFID uses;
        public RDFID specifies;
        public RDFID instanceof;
        public RDFID justifies;

    }
    [System.Serializable]
    public class RDFNode
    {
        public string id;
        public List<RDFID> specifies;
        public List<RDFID> uses;
        public List<RDFID> instanceof;
        public List<RDFID> justifies;
        //string specifiedin;
        public string name;
        public string paratype;
        public string sourceref;


        public RDFNode(string id, List<string> specifies, List<string> uses, List<string> instanceof, string name, string justifies, string paratype, string sourceref)
        {

        }


    }

    [System.Serializable]
    public class RDFGraph
    {
        public List<RDFNode> graph;
    }
    [System.Serializable]
    public class RDFGraphSingle
    {
        public List<RDFNodeSingle> graph;
    }

    public static void ConvertCoq(TextAsset file)
    {
        Path = Application.dataPath + "/Graphs/conv" + file.name + ".json";
        var text = file.text.Replace("@", "");
        text = text.Replace("-", "");
        text = text.Replace("ulo:", "");
        text = text.Replace("tmpuri:/", "");
        //text.Replace("?", "/");
        text = text.Replace("dcterms:", "");
        var gSingle = JsonUtility.FromJson<CoqGraphSingle>(text);
       // var gDeclares = JsonUtility.FromJson<CoqGraphDeclares>(text);


        CoqGraph g = JsonUtility.FromJson<CoqGraph>(text);


        for (int i = 0; i < g.graph.Count; ++i)
        {
            var node = g.graph[i];

            if (node.title.id == "" || node.title.id == null)
            {
                node.title.id = node.id;
            }

            if (node.uses == null)
            {
                var at= new List<RDFID>();
                at.Add(gSingle.graph[i].uses);
                node.uses = at;
            }
            if (node.declares == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].declares);
                node.declares = at;
            }
            if (node.HasMainHypothesis == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].HasMainHypothesis);
                node.HasMainHypothesis = at;
            }
            if (node.HasInHypothesis == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].HasInHypothesis);
                node.HasInHypothesis = at;
            }

            if (node.HasMainConclusion == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].HasMainConclusion);
                node.HasMainConclusion = at;
            }
        }

        var rdfEdges = new List<TGraph.ReadJSON.MyEdge>();
        var rdfNodes = new List<TGraph.ReadJSON.MyNode>();
        var rdfChapters = new List<TGraph.ReadJSON.MyChapter>();



  

        foreach (var node in g.graph)
        {
            /*
            string pid = "";

            var splitted = node.id.id.Split('/');

            for(int i= 0; i < splitted.Length - 2;i++)
            {
                pid += splitted[i];
            }


            rdfNodes.Add(new TGraph.ReadJSON.MyNode
            {
                label = node.title.id,
                id = node.id.id,
            });
            


            var chapter = rdfChapters.Find(c => c.id == pid);
            if (chapter == null)
            {

                rdfChapters.Add(new TGraph.ReadJSON.MyChapter
                {
                    label = splitted[splitted.Length-2],
                    id = pid,
                    highlevel = true,
                    chapters = ,
                    nodes = specifiedNodes
                });
            }
            */


            if (node.declares.Count > 0)
            {
                var specifiedNodes = new List<string>();
                var specifiedChapters = new List<string>();
                foreach (var d in node.declares)
                {
                    var s = d.id;
            
                    if (g.graph.Find(n => n.id == s).declares.Count == 0) // rdf node has no other chapters = > isNode
                         specifiedNodes.Add(s);
                    else
                    {
                        Debug.Log("chapter "+s+" of "+node.id);
                        specifiedChapters.Add(s);

                    }
                        
                }
                var splitted = node.id.Split('/');
                var name = splitted[splitted.Length - 1];

                rdfChapters.Add(new TGraph.ReadJSON.MyChapter
                {
                    label = name,
                    id = node.id,
                    highlevel = true,
                    chapters = specifiedChapters,
                    nodes = specifiedNodes
                });
            }

            else
            {
                rdfNodes.Add(new TGraph.ReadJSON.MyNode
                {
                    label = node.title.id,
                    id = node.id,
                });
            }

            
            foreach (var to in node.uses)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "uses"
                });
            }
            foreach (var to in node.HasMainHypothesis)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = g.graph.Find(n=>n.id==to.id).Target.id,
                    style = "HasMainHypthesis"
                });
            }


            var tGraph = new TGraph.ReadJSON.MyGraph
            {
                nodes = rdfNodes,
                edges = rdfEdges,
                chapters = rdfChapters
            };

            string json = JsonUtility.ToJson(tGraph, true);

            File.WriteAllText(Path, json);
        }
    }

    public static void Convert(TextAsset file)
    {
        Path = Application.dataPath + "/Graphs/conv" + file.name + ".json";
        var text = file.text.Replace("@", "");
        text = text.Replace("-", "");
        text = text.Replace("https://mathhub.info/ulo", "");
        text = text.Replace("tmpuri:/", "");
        //text.Replace("?", "/");
        var gSingle = JsonUtility.FromJson<RDFGraphSingle>(text);
        RDFGraph g = JsonUtility.FromJson<RDFGraph>(text);

        for (int i = 0; i < g.graph.Count; ++i)
        {
            var node = g.graph[i];

            if(node.name ==""||node.name == null)
            {
                node.name = node.id;
            }

            if (node.uses == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].uses);
                node.uses = at;
            }
            if (node.justifies == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].justifies);
                node.justifies = at;
            }
                 
            if (node.specifies == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].specifies);
                node.specifies = at;
            }
            if (node.instanceof == null)
            {
                var at = new List<RDFID>();
                at.Add(gSingle.graph[i].instanceof);
                node.instanceof = at;
            }
    


        }

        var rdfEdges = new List<TGraph.ReadJSON.MyEdge>();
        var rdfNodes = new List<TGraph.ReadJSON.MyNode>();
        var rdfChapters = new List<TGraph.ReadJSON.MyChapter>();



        List<string> sourceRefs = new List<string>();

        foreach (var node in g.graph)
        {
            /*
foreach (var to in node.specifies)
{
    rdfEdges.Add(new TGraph.ReadJSON.MyEdge
    {
        from = node.id,
        to = to,
        style = "specifies"
    });
}*/

            if (node.specifies.Count > 0)
            {
                var specifiedNodes = new List<string>();
                var specifiedChapters = new List<string>();
                foreach (var s in node.specifies)
                {
                    if (g.graph.Find(n => n.id == s.id).specifies.Count == 0) //rdf node has no other chapters = > isNode
                        specifiedNodes.Add(s.id);
                    else
                        specifiedChapters.Add(s.id);
                }
                rdfChapters.Add(new TGraph.ReadJSON.MyChapter
                {
                    label = node.name,
                    id = node.id,
                    highlevel = true,
                    chapters = specifiedChapters,
                    nodes = specifiedNodes
                });
            }
            
            else
            {
                rdfNodes.Add(new TGraph.ReadJSON.MyNode
                {
                    label = node.name,
                    id = node.id,
                    style = node.paratype
                });
            }

 
            
            foreach (var to in node.uses)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "uses"
                });
            }

            foreach (var to in node.instanceof)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "instanceof"
                });
            }

            if (node.sourceref != null && node.sourceref != "")
            {

                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    to = node.id,
                    from = node.sourceref,
                    style = "sourceref"
                });
                /*
                if (!sourceRefs.Contains(node.sourceref))
                {
                    sourceRefs.Add(node.sourceref);
                    rdfChapters.Add(new TGraph.ReadJSON.MyChapter
                    {
                        label = node.sourceref,
                        id = node.sourceref,
                        highlevel = true,
                        nodes = new List<string>()
                    });
                }
                rdfChapters.Find(c => c.id == node.sourceref).nodes.Add(node.id);
              */


            }

            foreach (var to in node.instanceof)
            {

                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "justifies"
                });

            }

            var tGraph = new TGraph.ReadJSON.MyGraph
            {
                nodes = rdfNodes,
                edges = rdfEdges,
                chapters = rdfChapters
            };

            string json = JsonUtility.ToJson(tGraph,true);

            File.WriteAllText(Path, json);
        }
    }
    void Start()
    {
        if(RDFFile!=null) Convert(RDFFile);
        else 
            if (CoqFile!= null) ConvertCoq(CoqFile);
        else
        {
            string prefix = "http://rdf-translator.appspot.com/convert/xml/json-ld/";
           
            StartCoroutine(GetRequest(prefix + Rdf));
        }

    }


    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                ConvertCoq(new TextAsset(webRequest.downloadHandler.text));
            }
        }
       
    }


}
