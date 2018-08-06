using System;
using UnityEngine;
using System.Collections;


public class OvrPlayerControllerConstraint : MonoBehaviour {

    [Tooltip("This game object will copy the target Player's transform position and rotation when the OVRPlayerController updates the transform.")]
	public OVRPlayerController Player;

	// Use this for initialization
	void Start () {
		Player.TransformUpdated += PlayerOnTransformUpdated;
	}

	private void PlayerOnTransformUpdated(Transform playerTransform)
	{
		transform.position = playerTransform.position;
		transform.rotation = playerTransform.rotation;
	}
}
