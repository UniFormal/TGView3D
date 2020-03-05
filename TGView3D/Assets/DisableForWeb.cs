using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableForWeb : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        this.gameObject.SetActive(false);
#endif
    }
}
