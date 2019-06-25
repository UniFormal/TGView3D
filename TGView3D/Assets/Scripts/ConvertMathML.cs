using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class ConvertMathML
{

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

    static IEnumerator TestRequest(PData pdata, int i)
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
            TGraph.GlobalVariables.Graph.nodes[i].svg = www.downloadHandler.text.Replace("ex\"", "px\"").Replace("Infinity", "0").Replace("currentColor", "white");

            Debug.Log(TGraph.GlobalVariables.Graph.nodes[i].svg);

            TGraph.GraphManager.CreateMathObject(i);

        }
    }

}
