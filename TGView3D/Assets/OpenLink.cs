using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void openWindow(string url);

    public string URL;


    public void OpenUrl()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
                     
                openWindow(URL);
#else
        Application.OpenURL(URL);
#endif
    }


}