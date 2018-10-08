using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TGraph
{
    public class Interaction : MonoBehaviour
    {

        // Use this for initialization
        private ReadJSON.MyGraph graph;

        private void Start()
        {
            graph = GlobalVariables.Graph;
        }


    }
    
}
