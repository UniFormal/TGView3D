using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignText : MonoBehaviour {
    Vector3 basePosition;
	// Use this for initialization
	void Start () {
        basePosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
       transform.rotation=Quaternion.LookRotation(transform.position - Camera.main.transform.position);
      // transform.rotation= Quaternion.LookRotation(Camera.main.transform.forward);
       //transform.position = basePosition - Camera.main.transform.forward.normalized;
    }
}
