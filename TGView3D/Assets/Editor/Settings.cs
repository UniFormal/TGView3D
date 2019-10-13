
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class WMTSettings
    {

        static WMTSettings()
        {
            PlayerSettings.WebGL.memorySize = 512 * 2;

            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = false;//true;

        if (PlayerSettings.WebGL.threadsSupport)
            Debug.LogWarning("web mt enabled");
           // tweak this value for your project
        }




}


