/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1


Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScaleSample : MonoBehaviour {
    
    
    float heightScale = 1.0f;
    float trackingAndIPDScale = 1.0f;
    float walkSpeedScale = 1.0f;
    float newHeightScale = 1.0f;
    float newTrackingAndIPDScale = 1.0f;
    float newWalkSpeedScale = 1.0f;


    bool scaleTogether = true;
    float initialAcceleration;

    public Slider heightScaleSlider;
    public Slider trackingAndIPDSlider;
    public Slider walkSpeedSlider;



	// Use this for initialization
	void Start () 
    {
        initialAcceleration = OVRInspector.instance.playerController.Acceleration;
	}


    public void SetHeightScale(float s)
    {
        if (scaleTogether)
        {
            SetAllScales(s);
            return;
        }
        newHeightScale = s;
    }
    public void SetTrackingAndIPDScale(float s)
    {
        if (scaleTogether)
        {
            SetAllScales(s);
            return;
        }
        newTrackingAndIPDScale = s;
        
    }
    public void SetWalkSpeedScale(float s)
    {
        if (scaleTogether)
        {
            SetAllScales(s);
            return;
        }
        newWalkSpeedScale = s;

    }
    void SetAllScales(float s)
    {
        newHeightScale = newTrackingAndIPDScale = newWalkSpeedScale = s;

        heightScaleSlider.value = s;
        trackingAndIPDSlider.value = s;
        walkSpeedSlider.value = s;

    }
    public void SetScaleTogether(bool v)
    {
        scaleTogether = v;
    }

    

    void OnScaleChanged(float prevHeightScale = -1)
    {
        if (prevHeightScale == -1)
            prevHeightScale = heightScale;

        // Get current capsule height
        var capsuleHeight = OVRInspector.instance.playerController.GetComponent<CharacterController>().height;

        // Set new capsule scale
        OVRInspector.instance.playerController.transform.localScale = new Vector3(heightScale, heightScale, heightScale);
        
        // Scale happens relative to the centre of the capsule, so after scaling it we need to move it to keep the player's feet on the ground
        var newPos = OVRInspector.instance.playerController.transform.localPosition;
        float heightChange;
        if (heightScale > prevHeightScale)
            heightChange = (capsuleHeight * 0.5f * (heightScale - prevHeightScale)) * 1.02f;
        else
            heightChange = (capsuleHeight * 0.5f * (heightScale - prevHeightScale)) * 0.98f;

        newPos.y += heightChange; 
        OVRInspector.instance.playerController.transform.localPosition = newPos;

        // The tracking scale set on the slider is absolute, but the value on the cameraRig is relative to the playercontroller scale.
        // This means we need to scale it first to get the desired value.
        var requiredTrackingScale = trackingAndIPDScale / heightScale;
        OVRInspector.cameraRig.transform.localScale = new Vector3(requiredTrackingScale, requiredTrackingScale, requiredTrackingScale);

        OVRInspector.instance.playerController.Acceleration = initialAcceleration * walkSpeedScale;

    

    }
   
    public void EndDrag()
    {
        float prevHeightScale = heightScale;
        trackingAndIPDScale = newTrackingAndIPDScale;
        heightScale = newHeightScale;
        walkSpeedScale = newWalkSpeedScale;

        OnScaleChanged(prevHeightScale);

        OVRInspector.instance.RescaleInspectorUI();
    }

   
}
