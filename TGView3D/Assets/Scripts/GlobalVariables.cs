﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TGraph
{
    public static class GlobalVariables
    {
        public static GraphManager GraphManager;
        public static UIInteracton UIInteractonManager;
        public static ReadJSON JsonManager;
        public static ReadJSON.MyGraph Graph;
        public static UnityEngine.EventSystems.EventSystem EventSystem;
        public static Text Percent;
        public static int UrlMode;
        public static bool Solved = false;
        public static int Vol;
        public static int NodeCount;
        public static bool Init;
       // public static bool Reload = false;
        public static bool JetPack = false;
        public static bool Beam = true;
        public static bool Recalculate = false;
        public static bool TwoD = false;
        public static string JSON = "";
        public static Dictionary<string, Vector3> IdToPosition = new Dictionary<string, Vector3>();
        public static Gestures Gestures;
        public static Vector3 Rotation;
        public static FlyCamera MouseManager;
        /*
public static string Url = "";

public static TextAsset CurrentFile;
public static string URName;
public static string Path="";
public static int SelectionIndex = 0;
*/


    }


    /*Pointer_stringify(data), Pointer_stringify(strFileName), Pointer_stringify(strMimeType)
	//function download() {

		*/
}
