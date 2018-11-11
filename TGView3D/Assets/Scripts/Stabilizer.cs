using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizer : MonoBehaviour {

    Vector3 oldPos;
    Vector3 oldRot;
	// Use this for initialization
	void Start () {
        oldPos = transform.localPosition;
        oldRot = transform.localEulerAngles;
    }
	
	// Update is called once per frame
	void Update () {

        if (Vector3.Magnitude(oldPos - transform.localPosition) < 0.00001f)
        {
            transform.localPosition = oldPos;
        }
      
            oldPos = transform.localPosition;
       

        if (Vector3.Angle(oldRot, transform.localEulerAngles) < .01f)
        {
            transform.localEulerAngles = oldRot;
        }
      
            oldRot = transform.localEulerAngles;
        

    }
}
