
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

namespace TGraph { 

    public class ServerRequest : MonoBehaviour
    {
        private void CreateMathObject(int i, string svg)
        {
            //instantiate default MathML Object for node i
            GameObject mathObject = (GameObject)Instantiate(Resources.Load("mathObject"));

            //convert svg to Unity mesh
            ImportSVG.ImportAsMesh(svg, ref mathObject);

            //correctly set parent and position the text mesh
            //...

        }

        //requests conversion of MathML to SVG from local MathJax Server
        private IEnumerator TestRequest(PData pdata, int i)
        {
            string formData = JsonUtility.ToJson(pdata);
            var data = System.Text.Encoding.UTF8.GetBytes(formData);

            var www = new UnityWebRequest("http://localhost:8003");
            www.method = "POST";
            www.uploadHandler = new UploadHandlerRaw(data);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //replace certain symbols that are not supported by the Unity SVG Importer
                string svg = www.downloadHandler.text.Replace("ex\"", "px\"").Replace("Infinity", "0").Replace("currentColor", "white");
                CreateMathObject(i, svg);
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
    }
}