using UnityEngine;
using System.Collections;

/// <summary>
/// Simple script to animate a cube 
/// 
/// </summary>
/// 
public class MovingCube : MonoBehaviour
{
 	const float movingAmp = 10.0f;
	Transform transformCom;
	float phase = 0;
	// Use this for initialization
	void Start()
	{
		transformCom = GetComponent<Transform>();
		phase = transformCom.localPosition.z / movingAmp * Mathf.PI / 2;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 newPos = transformCom.localPosition;
		newPos.z = Mathf.Sin(Time.time + phase) * movingAmp;
		transformCom.localPosition = newPos;
	}
}
