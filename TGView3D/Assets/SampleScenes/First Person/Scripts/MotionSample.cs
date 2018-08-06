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
using System.Collections.Generic;

public class MotionSample : MonoBehaviour
{

    
    float walkSpeedScale = 1.0f;
    float initialAcceleration;
    private bool showDirectionIndicator = false;

    public Slider walkSpeedSlider;
    public GameObject directionIndicator;
    public Text rotationSnapSliderLabel;

    public Toggle showDirectionIndicatorToggle;

	// Use this for initialization
	void Start () 
    {
        initialAcceleration = OVRInspector.instance.playerController.Acceleration;
	}
	

    public void SetWalkSpeedScale(float s)
    {
        walkSpeedScale = s;

        OnScaleChanged();
    }
    public void HMDControlsYaw(bool value)
    {
        OVRInspector.instance.playerController.HmdRotatesY = value;
        showDirectionIndicatorToggle.interactable = !value;
        UpdateDirectionIndicatorState();
    }

    public void ShowDirection(bool value)
    {
        showDirectionIndicator = value;
        UpdateDirectionIndicatorState();
    }

    void UpdateDirectionIndicatorState()
    {
        directionIndicator.SetActive(showDirectionIndicator && !OVRInspector.instance.playerController.HmdRotatesY);
    }
    
    public void SetRotationSnap(float value)
    {
        //We choose to multiply this by 5 so that the slider snaps to 5 degree intervals
        value *= 5;
        rotationSnapSliderLabel.text = string.Format("{0}", value);
        OVRInspector.instance.playerController.RotationRatchet = value;
    }
    

    void OnScaleChanged(float prevHeightScale = -1)
    {

        OVRInspector.instance.playerController.Acceleration = initialAcceleration * walkSpeedScale;

        
    }



   
}
