using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ExportDot 
{

    public static void Export()
    {
        var graph = TGraph.GlobalVariables.Graph;
        string dot = "digraph D {\n";
        foreach (var node in graph.nodes)
        {
            string targets = "{";
            foreach (var edge in node.edgeIndicesOut)
            {
                if (graph.edges[edge].style == "include")
                {
                    string target = "\"" + graph.nodes[graph.nodeDict[graph.edges[edge].to]].label
                        + graph.nodes[graph.nodeDict[graph.edges[edge].to]].nr
                        + "\"";
                    targets += target + ", ";
                }

            }



            if (targets != "{")
            {
                targets = targets.Remove(targets.Length - 2, 2);
                targets += "}";
                dot += "\"" + node.label
                    + node.nr
                    + "\"" + " -> " + targets + "[arrowhead=none]\n";
            }
        }
        dot += "\n}";

        File.WriteAllText(Application.dataPath + "/dot.gv", dot);
    }
}
