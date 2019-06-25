
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class WMTSettings
    {

       static void InitMTSettings()
        {
        PlayerSettings.WebGL.memorySize = 512 * 2;
        Debug.LogWarning("mt web enabled");
         /*   PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.threadsSupport = true;
           // tweak this value for your project*/
        }


    }


