using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LegacyFucntions : MonoBehaviour
{
  
}

/*
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
        if (data == null) return null;


        var lines = Regex.Split(data.text.Replace("|", ","), LINE_SPLIT_RE);
        //  Debug.Log("lines found: "+lines.Length);

        if (lines.Length <= 1) return list;
        string part = "";
        string origin = "core";

        string[] header = { "type", "id", "to" };
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
            }*s/


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
                entry[header[j]] = finalvalue;*s/
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
*s/
list.Add(entry);
k++;
// if (k > 5000) break;

}
// File.WriteAllText(Application.dataPath + "/stdlib_objects.csv", part);
if (list.Count == 0) Debug.LogError("bad list");
return list;
}
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


public class NE
{
public TGraph.ReadJSON.MyNode node;
public TGraph.ReadJSON.MyEdge edge;

}

public void LoadCoqDirectory()
{

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




} */

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
        }*s/
}

}

public void SpawnNodesEarly(List<string> val, MyEdge edge)
{


MyNode origin = graph.nodes.Find(x => x.id == val[1]);
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
if (!IsCoq || FoundNodes.Count > 0)
{

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
    if (val[0] == "s")
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
                }*s/
    }

}
else
{
    if (val[0] == "ii" && val[2] == "cic:") continue;
    var edge = new MyEdge();
    edge.style = "include";// val[0];
    edge.from = val[1];
    edge.to = val[2];
    edge.label = val[0];
    Debug.Log("found " + val[0]);


    if (!edge.from.Contains(edge.to) && edge.from != edge.to && !graph.edges.Any(x => x.from == edge.from && x.to == edge.to))
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
   }*s/
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
"graph_algebra";
//"exportSG";
//"fingroup";
//"tlc";
//"extlib";


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
// StreamReader reader = new StreamReader(Application.dataPath + "/"+s+".json");
// var text = reader.ReadToEnd();
TextAsset data = Resources.Load(s) as TextAsset;
var text = data.text;
SimpleGraph sG = SimpleGraph.CreateFromJSON(text);
Debug.Log(FoundNodes.Count);
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

if (MyNode.style == "o" && FoundNodes.Exists(f => MyNode.id.Contains(f.id)))
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
if (MyEdge.label == "oo" && (FoundNodes.Exists(f => MyEdge.from.Contains(f.id)) || FoundNodes.Exists(f => MyEdge.from.Contains(f.id))))
{
    MyEdge.style = "meta";
    MyEdges.Add(MyEdge); e++;
    if (e > 8000)
    {
        Debug.LogError("size");
        break;
    }
}
else if (MyEdge.label == "ii")// && !(FoundNodes.Exists(f => f.id == MyEdge.from) || FoundNodes.Exists(f => f.id == MyEdge.to)))
{
    MyEdge.style = "structure";
    MyEdges.Add(MyEdge); e++;

}
else if (MyEdge.label == "od" && (FoundNodes.Exists(f => f.id == MyEdge.from)))
{
    MyEdge.style = "meta";
    MyEdges.Add(MyEdge);

}

else if (MyEdge.label == "dd")
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
File.WriteAllText(Application.dataPath + "/" + s + ".json", json);
}

if (IsCoq)
        {
            UpdateAllEdges();
            NativeArray<float> Energies = new NativeArray<float>(graph.nodes.Count, Allocator.Persistent);
            Layouts.Energies = Energies;
            var handle = Layouts.UpdateLayout(iterations, globalWeight, spaceScale);

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

*/

/*
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
     }*s/
  Debug.Log("update coroutine fin");
      yield return null;

  }



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
          
                *s/
float maxSize = -1f;
                foreach (var node in graph.nodes)
                {
                    maxSize = Mathf.Max(maxSize, node.radius);
                }

                Material loadedMat = new Material(mat);
loadedMat.color = Color.gray;
                Material loadedMat2 = new Material(mat);
loadedMat2.color = Color.yellow;


                foreach (var node in graph.nodes)
                {
                    node.radius = 2*
                        //Mathf.Sqrt
                        (node.radius / (maxSize+1));
                    node.nodeObject.transform.localScale *= 2* (1+node.radius);
                    if(node.generated)
                        node.nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat;
                    else if(node.style=="o") node.nodeObject.GetComponent<MeshRenderer>().sharedMaterial = loadedMat2;
                }
            }
  */
