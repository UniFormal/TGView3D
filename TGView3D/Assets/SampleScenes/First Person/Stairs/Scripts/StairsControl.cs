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

public class StairsControl : MonoBehaviour
{
    public Staircase[] staircaseObjects;
    public GameObject[] wallObjects;
    public Elevator elevator;
    public Vector3 defaultStepScale;
    public LightContrastFader[] contrastLights;
    public Slider stepHeightSlider;
    public Slider stepDepthSlider;
    public Slider stepAngleSlider;
    protected bool suppressAngleHandler;

    public void SetStepWidth(float stepWidth)
    {
        for (int i = 0; i < staircaseObjects.Length; i++)
        {
            staircaseObjects [i].SetStepWidth(stepWidth);
        }
    }

    public void SetStepHeight(float stepHeight)
    {
        for (int i = 0; i < staircaseObjects.Length; i++)
        {
            staircaseObjects [i].SetStepHeight(stepHeight);
        }
        SetCalculatedAngle(stepHeight, stepDepthSlider.value);
    }

    public void SetStepDepth(float stepDepth)
    {
        for (int i = 0; i < staircaseObjects.Length; i++)
        {
            staircaseObjects [i].SetStepDepth(stepDepth);
        }
        SetCalculatedAngle(stepHeightSlider.value, stepDepth);
    }

    public void SetStepAngle(float stepAngle)
    {
        if (suppressAngleHandler)
        {
            suppressAngleHandler = false;
            return;
        }
        SetCalculatedDepth(stepHeightSlider.value, stepAngle);
    }

    // Set the current stair angle (and possible angle range) from the height and depth
    protected void SetCalculatedAngle(float stepHeight, float stepDepth)
    {
        stepAngleSlider.maxValue = Mathf.Atan2(stepHeight, stepDepthSlider.minValue) * Mathf.Rad2Deg;
        stepAngleSlider.minValue = Mathf.Atan2(stepHeight, stepDepthSlider.maxValue) * Mathf.Rad2Deg;
        suppressAngleHandler = true;
        stepAngleSlider.value = Mathf.Atan2(stepHeight, stepDepth) * Mathf.Rad2Deg;
    }

    // Set the current stair depth from the height and angle
    protected void SetCalculatedDepth(float stepHeight, float stepAngle)
    {
        stepDepthSlider.value = Mathf.Tan((90f - stepAngle) * Mathf.Deg2Rad) * stepHeight;
    }

    public void SetStepContrast(float contrast)
    {
        for (int i = 0; i < contrastLights.Length; i++)
        {
            contrastLights [i].SetContrast(contrast);
        }
    }

    public void SetStepSmoothCollider(bool isSmooth)
    {     
        for (int i = 0; i < staircaseObjects.Length; i++)
        {
            staircaseObjects [i].SetStepSmoothCollider(isSmooth);
        }
    }

    public void SetElevatorSpeed(float speed)
    {
        elevator.Speed = speed;
    }

    // Switches elevator between a realistic style w/ walls and ceilings,
    // and a DOOM style floor-only look.
    public void SetElevatorIsFloorOnly(bool isFloorOnly)
    {
        elevator.FloorOnly = isFloorOnly;
    }

    public void SetWallAroundStairsLeft(bool useWall)
    {
        wallObjects [0].SetActive(useWall);
    }

    public void SetWallAroundStairsRight(bool useWall)
    {
        wallObjects [1].SetActive(useWall);
    }

    void Start()
    {
        suppressAngleHandler = true;
        SetStepWidth(defaultStepScale.x);
        SetStepDepth(defaultStepScale.y);
        SetStepHeight(defaultStepScale.z);
        SetWallAroundStairsLeft(true);
        SetWallAroundStairsRight(true);
        SetElevatorIsFloorOnly(false);
        SetStepSmoothCollider(true);
        SetStepContrast(0f);
    }

}
