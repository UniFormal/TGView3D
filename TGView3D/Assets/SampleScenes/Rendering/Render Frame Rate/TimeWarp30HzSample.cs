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
using System.Collections;					// required for Coroutines
using System.Runtime.InteropServices;		// required for DllImport

/// <summary>
/// Sets the minimum vsync count in VRAPI to allow TimeWarp sub-stepping, where the same frame is
/// warped multiple times during slow synthesis.
/// \warning OVR_TW_SetMinimumVsyncs is deprecated and will be removed in the near future.
/// </summary>
public class TimeWarp30HzSample : MonoBehaviour
{
    public enum VsyncMode
    {
        VSYNC_60FPS = 1,
        VSYNC_30FPS = 2,
        VSYNC_20FPS = 3
    }

    public VsyncMode targetFrameRate = VsyncMode.VSYNC_30FPS;
    private Renderer targetFpsRenderer = null;

    public Material fps20Material;
    public Material fps30Material;
    public Material fps60Material;

    public Text sliderFrameRateLabel;
    public Toggle chromaticToggle;
    public Toggle monoscopicToggle;

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        targetFpsRenderer = GetComponentInChildren<Renderer>();
        Debug.Log("vsyncCount is currently " + OVRManager.instance.vsyncCount);

        // Make Chromatic & Monoscopic checkboxes reflect current state
        if (chromaticToggle != null)
        {
            chromaticToggle.isOn = OVRManager.instance.chromatic;
        }
        if (monoscopicToggle != null)
        {
            monoscopicToggle.isOn = OVRManager.instance.monoscopic;
        }

#if (UNITY_ANDROID && !UNITY_EDITOR)
		// delay one frame because OVRCameraController initializes TimeWarp in Start()
		Invoke("UpdateVSyncMode", 0.01666f);
#else
        UpdateVSyncMode();
#endif
    }

    /// <summary>
    /// Set the Time Warp vsync rate
    /// </summary>
    void UpdateVSyncMode()
    {
        // This is initialized in OVRCameraController.Start() to VSYNC_60FPS, however, you can override the setting
        // as desired to support Time Warp VSync at 60/30/20 FPS frame rates for consistency or power savings.
        // Time Warp at 30 FPS will also help app that can't consistently perform at 60 FPS
        OVRManager.instance.vsyncCount = (int)targetFrameRate;
        Debug.Log("vsyncCount set to " + OVRManager.instance.vsyncCount);
        string sliderFrameRateLabelText = "";

        // swap the material on the fps quad renderer
        switch (targetFrameRate)
        {
            case VsyncMode.VSYNC_20FPS:
                targetFpsRenderer.sharedMaterial = fps20Material;
                sliderFrameRateLabelText = "20";
                break;
            case VsyncMode.VSYNC_30FPS:
                targetFpsRenderer.sharedMaterial = fps30Material;
                sliderFrameRateLabelText = "30";
                break;
            case VsyncMode.VSYNC_60FPS:
                targetFpsRenderer.sharedMaterial = fps60Material;
                sliderFrameRateLabelText = "60";
                break;
        }
        if (sliderFrameRateLabel)
        {
            sliderFrameRateLabel.text = sliderFrameRateLabelText;
        }
    }

    public void SetVSyncMode(float modeValueOffset)
    {
        float modeValue = modeValueOffset;
        if (modeValue < 1 || modeValue > System.Enum.GetNames(typeof(VsyncMode)).Length)
        {
            Debug.LogWarning("Invalid VSync Mode");
            return;
        }

        targetFrameRate = (VsyncMode)modeValue;
        UpdateVSyncMode();
    }

    // Revert to default settings on scene exit
    void OnDestroy()
    {
        SetVSyncMode((float)VsyncMode.VSYNC_60FPS);
        OVRManager.instance.monoscopic = false;
        OVRManager.instance.chromatic = true;
    }
}
