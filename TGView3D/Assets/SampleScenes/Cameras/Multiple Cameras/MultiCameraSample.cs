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

public class MultiCameraSample : MonoBehaviour {

	public OVRCameraRig[]	cameraControllers = new OVRCameraRig[0];
	public int				currentController = 0;
    private OVRHarness      harness;

	/// <summary>
	/// Initialize
	/// </summary>
	void Start() {
        GameObject harnessObj = GameObject.Find("Harness");
        if ( harnessObj != null )
        {
            harness = harnessObj.GetComponent<OVRHarness>();
        }
		UpdateCameraControllers();
	}
	
	/// <summary>
	/// Set the current camera controller
	/// </summary>
	void UpdateCameraControllers() {
        for ( int i = 0; i < cameraControllers.Length; i++ ) {
			if ( cameraControllers[i] == null ) {
				continue;
			}
			cameraControllers[i].gameObject.SetActive( i == currentController );
        }
        
        // Re-assign the camera associations of OVRInspector & OVRGazePointer
        OVRGazePointer.instance.Awake();
        OVRInspector.instance.AssignCameraRig();

        // Override the usual fade-from-black
        OVRInspector.instance.AssignFader();
        OVRInspector.instance.fader.fadeOnStart = false;

        OVRInspector.instance.Hide();
    }

	/// <summary>
	/// Check input and switch between camera controllers
	/// These samples use the default Unity Input Mappings with the addition of "Right Shoulder" and "Left Shoulder"
    /// Added Touch compatibility using L/R triggers
	/// </summary>
	void Update() {
        // Don't switch if we're transitioning to a different scene or the menu is open
        if ( OVRInspector.instance.IsMenuActive() ||
            (harness != null && harness.LoadingLevel) )
        {
            return;
        }
        
        if ( OVRInput.GetDown(OVRInput.RawButton.RShoulder) ||
            OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger, OVRInput.Controller.Gamepad | OVRInput.Controller.RTouch) ||
            (Input.GetMouseButtonDown(0)) ) {
            //*************************
            // switch to the next camera
            //*************************
            if ( ++currentController == cameraControllers.Length ) {
				currentController = 0;
			}
			UpdateCameraControllers();
		} else if ( OVRInput.GetDown(OVRInput.RawButton.LShoulder) ||
            OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger, OVRInput.Controller.Gamepad | OVRInput.Controller.LTouch) ) {
            //*************************
            // switch to the previous camera
            //*************************
            if ( --currentController < 0 ) {
				currentController = cameraControllers.Length - 1;
			}
			UpdateCameraControllers();
		}
	}
}
