using UnityEngine;
using System.Collections;

public class TeleportDemo : MonoBehaviour {

    
    TeleportController teleportController 
    {
        get
        {
            return OVRInspector.cameraRig.GetComponent<TeleportController>();
        }

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetAllowRotation(bool allow)
    {
        teleportController.allowRotation = allow;
    }

    public void SetRotationSpeed(float speed)
    {
        teleportController.rotationSpeed = speed;
    }

    public void SetCorrectForHeadPosition(float amount)
    {
        teleportController.realHeadMovementCompensation = amount;
    }
    public void SetFadeSpeed(float speed)
    {
        teleportController.fadeSpeed = speed;
    }

    public void SetFadeDuration(float length)
    {
        teleportController.fadeLength = length;
    }

    public void OnInspectorShow()
    {
        teleportController.teleportEnabled = false;
    }
    public void OnInspectorHide()
    {
        teleportController.teleportEnabled = true;
    }
}
