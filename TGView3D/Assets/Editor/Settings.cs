
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class WMTSettings
    {

        static WMTSettings()
        {
            PlayerSettings.WebGL.memorySize = 512 * 2;
            Debug.LogWarning("mt web enabled");
       //     PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.threadsSupport = false;//true;
           // tweak this value for your project
        }




}


