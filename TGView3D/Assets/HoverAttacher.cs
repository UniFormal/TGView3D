using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverAttacher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
        transform.parent.gameObject.AddComponent<HoverText>();
    }

 
}
