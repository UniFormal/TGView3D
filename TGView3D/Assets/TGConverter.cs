using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

public class TGConverter : MonoBehaviour
{

    public TextAsset RDFFile;
    public TextAsset CoqFile;
    static string Path; 
    // Start is called before the first frame update
    public string Rdf =  "https://gl.mathhub.info/Coq/mathcomp-bigenough/raw/master/triples.rdf";
    public static List<string> FileContents = new List<string>();
    public static List<string> FileNames= new List<string>();


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
        public List<RDFID> specifiedin;
        public List<RDFID> name;
        public List<RDFID> paratype;
        public List<RDFID> sourceref;


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
        Path = Application.dataPath + "/Graphs/coqConv" + file.name + ".json";

        var text = file.text.Replace("@", "");
        text = text.Replace("https://mathhub.info/ulo#", "");
        text = text.Replace("-", "");
        text = text.Replace("ulo:", "");
        text = text.Replace("tmpuri:/", "");
        //text.Replace("?", "/");
        text = text.Replace("dcterms:", "");
        if (text[0] == '[') text = text.Remove(0, 1);
        if (text[text.Length - 1] == ']') text = text.Remove(text.Length - 1, 1);
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
            /*
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
            }*/
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
                        Debug.Log("chapter " + s + " of " + node.id);
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
                var splitted = node.title.id.Split('/');
                var name = splitted[splitted.Length - 1];
                rdfNodes.Add(new TGraph.ReadJSON.MyNode
                {
                    label = name,
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
            /*
            foreach (var to in node.HasMainHypothesis)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = g.graph.Find(n=>n.id==to.id).Target.id,
                    style = "HasMainHypthesis"
                });
            }*/

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

    public static void Convert(string s, string name)
    {

        var text = s.Replace("@value", "id");
        text = text.Replace("@", "");
        text = text.Replace("specified-in", "specified");
        text = text.Replace("instance-of", "instanceof");
        text = text.Replace("https://mathhub.info/ulo", "");
        text = text.Replace("tmpuri:/", "");
        if (text[0] == '[') text = text.Remove(0, 1);
        if (text[text.Length - 1] == ']') text = text.Remove(text.Length - 1, 1);
        //text.Replace("?", "/");
      //  Debug.Log(text);

       // var gSingle = JsonUtility.FromJson<RDFGraphSingle>(text);
        RDFGraph g = JsonUtility.FromJson<RDFGraph>(text);
        /*
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
    


        }*/

        var rdfEdges = new List<TGraph.ReadJSON.MyEdge>();
        var rdfNodes = new List<TGraph.ReadJSON.MyNode>();
        var rdfChapters = new List<TGraph.ReadJSON.MyChapter>();

        List<string> sourceRefs = new List<string>();

       /* var mainNode = new TGraph.ReadJSON.MyNode
        {
            label = name,
            id = name,
            radius = Mathf.Sqrt(g.graph.Count) * 0.05f,
        };

        rdfNodes.Add(mainNode);
        */
        for (int i = 0; i<g.graph.Count;i++)
        {
            var node = g.graph[i];

           // Debug.Log(node.name.id +" "+node.id);
            if ( node.name.Count == 0 )
            {
                node.name = new List<RDFID>
                {
                    new RDFID(node.id)
                };
            }


            if (node.specifies.Count == 0 && node.uses.Count == 0) continue;

           // Debug.Log(node.specifies.Count);
           
            rdfNodes.Add(new TGraph.ReadJSON.MyNode
            {
                label = node.name[0].id,
                id = node.id,
                radius = Mathf.Sqrt(node.specifies.Count) * 0.05f,
                
                // style = node.paratype[0].id
            });
            
         
            
            foreach (var to in node.specifies)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "specifies"
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


            /*
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
            }*/
            /*
            string origin = node.id;
            if (node.specifiedin.Count > 0)
            {
                origin = node.specifiedin[0].id;
            }*/

            /*
             * //other variant
            string origin = mainNode.id;

            foreach (var to in node.uses)
            {
                
                string target = to.id;
                var toNode = g.graph.Find(n => n.id == target);
                if (toNode != null) target = mainNode.id;// if (toNode != null) target =toNode.specifiedin[0].id;
                else
                {
                    var splitted = target.Split('?');
                    target = splitted[1];
                  //  Debug.Log(target);
                }
           

                if (target!=origin&&rdfEdges.Find(e=>e.id == (origin+target))==null)
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = origin,
                    to = target,
                    style = "uses",
                    id = origin+target

                });
            }*/

            /*
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
              /
              
            }*/
            /*
            foreach (var to in node.justifies)
            {
                rdfEdges.Add(new TGraph.ReadJSON.MyEdge
                {
                    from = node.id,
                    to = to.id,
                    style = "justifies"
                });

            }*/

        }
        var tGraph = new TGraph.ReadJSON.MyGraph
        {
            nodes = rdfNodes,
            edges = rdfEdges,
            chapters = rdfChapters
        };

        string json = JsonUtility.ToJson(tGraph,true);

        if (Path == ""||Path == null) Path = Application.dataPath+"/";

        File.WriteAllText(Path + name + ".json", json);
        Debug.Log("completed " + Path+ name + ".json");
       
    }
    void Start()
    {
        if (RDFFile != null) Convert(RDFFile.text, RDFFile.name);
        else 
            if (CoqFile!= null) ConvertCoq(CoqFile);
        else if( Rdf!="")
        {
            string prefix = "http://rdf-translator.appspot.com/convert/xml/json-ld/";
           
            StartCoroutine(GetRequest(prefix + Rdf));
        }
        else
        {
            string manyPath = "D:/mlibs/AFP/rdf/";//Application.dataPath + " / Graphs/Isabelle/";

            int emergencyCounter = 0;
            foreach ( var file in Directory.GetFiles(manyPath, "*.json"))
            {
                if (emergencyCounter++ <= 3000)
                    continue;
                if (emergencyCounter > 4000) break;
                string content = File.ReadAllText(file);
                
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                FileNames.Add(name);
                FileContents.Add(content);
               
            }
            Path = "D:/mlibs/AFP/json/";//Application.dataPath + "/Graphs/IsabelleConv/"; 
            int k = 0;
            List<JobHandle> handles = new List<JobHandle>();
            foreach(var file in FileNames)
            {
                var j = new MyIsabelleJob{
                    idx = k++
                };
            //    Debug.Log(k);
                handles.Add(j.Schedule());
               
            }
            foreach( var handle in handles)
            {
                handle.Complete();
            }
        }

    }


    public static void MultiConvert(int idx)
    {
        Convert(FileContents[idx], FileNames[idx]);
    }

    // Job adding two floating point values together
    public struct MyIsabelleJob : IJob
    {
        public int idx;
        public void Execute()
        {
            MultiConvert(idx);
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
