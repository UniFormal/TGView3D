using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour {

    private float val = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var col = this.GetComponent<MeshRenderer>().material.GetColor("_TintColor");
        //for subgraphorigin highlighting
        this.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(col.r, col.g, col.b, Mathf.PingPong(val+=0.001f,.3f)));
        //Debug.Log(col);

    }
}
