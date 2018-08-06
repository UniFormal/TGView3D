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

#define DEBUG_LOCOMOTION_PANEL
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// The CameraConstraintsPanel provides the user options for enabling various camera-related features of the sample,
/// specifically:
/// * Fade to black when approaching colliding geometry. The intention is to prevent players from seeing hidden areas or outside the world.
/// * Camera Collisions, which will cause the camera to simply not pass into colliding geometry. This is usually uncomfortable and not advised for shipping titles.
/// * Camera Constraint, which is responsible for moving the character capsule to the HMD's position. With this disabled, the character capsule will not
///   be centered on the HMD, which can have any number of undesirable side effects to a shipping title.
/// * Dynamic Height, which will cause the character capsule height to be adjusted to match the HMD's elevation off the ground. This is useful for making the 
///   character capsule better fit the player's physical pose and can be used for making environments that require ducking to access certain areas.
/// </summary>
public class CameraConstraintsPanel : LocomotionSampleSupport
{
	[Tooltip("This object is duplicated for each entry in the panel and needs to have a text component in it's hierarchy.")]
	public GameObject LabelTemplate;

	[Tooltip("This needs to have a Toggle component in it, and a Text component in the hierarchy below it.")]
	public GameObject ToggleTemplate;

	private Toggle _toggleEnableFadeout;
	private Toggle _toggleEnableCameraCollision;
	private Toggle _toggleEnableCameraConstraint;
	private Toggle _toggleEnableDynamicHeight;

	private CharacterCameraConstraint _cameraConstraint;

	/// <summary>
	/// This boolean is necessary for preventing the event handler from being triggered when the checkbox is updated by code to sync
	/// with the current setting of the _cameraConstraint members and other booleans.
	/// </summary>
	private bool _updating;

	// Use this for initialization
	void Start ()
	{
		_cameraConstraint = LocomotionController.PlayerController.GetComponent<CharacterCameraConstraint>();
		AddLabel("Camera Constraint Tuning");
		_toggleEnableFadeout = SetupToggle("Enable Fadeout", ToggleEnableFadeout);
		_toggleEnableCameraCollision = SetupToggle("Enable Camera Collision", ToggleEnableCameraCollision);
		_toggleEnableCameraConstraint = SetupToggle("Enable Camera Constraint", ToggleEnableCameraConstraint);
		_toggleEnableDynamicHeight = SetupToggle("Enable Dynamic Height", ToggleEnableDynamicHeight);

		ToggleTemplate.SetActive(false);
		LabelTemplate.SetActive(false);
	}

	void ToggleEnableFadeout()
	{
		_cameraConstraint.EnableFadeout = !_cameraConstraint.EnableFadeout;
	}

	void ToggleEnableCameraCollision()
	{
		_cameraConstraint.EnableCollision = !_cameraConstraint.EnableCollision;
	}

	void ToggleEnableCameraConstraint()
	{
		_cameraConstraint.enabled = !_cameraConstraint.enabled;
	}

	void ToggleEnableDynamicHeight()
	{
		_cameraConstraint.DynamicHeight = !_cameraConstraint.DynamicHeight;
	}

	Toggle SetupToggle(string label, Action action) 
	{
		var go = AddToggle(label);
		var toggle = go.GetComponent<Toggle>();
		toggle.onValueChanged.AddListener((b) => { if(!_updating) action(); });
		return toggle;
	}

	GameObject AddToggle(string label)
	{
		var go = AddInstance(ToggleTemplate, label);
		var text = go.GetComponentInChildren<Text>();
		text.text = label;
		return go;
	}

	void AddLabel(string label)
	{
		var go = AddInstance(LabelTemplate, label);
		var text = go.GetComponentInChildren<Text>();
		text.text = label;
	}


	void Update ()
	{
		_updating = true;
		UpdateToggle(_toggleEnableFadeout, _cameraConstraint.EnableFadeout);
		UpdateToggle(_toggleEnableCameraCollision, _cameraConstraint.EnableCollision);
		UpdateToggle(_toggleEnableCameraConstraint, _cameraConstraint.enabled);
		UpdateToggle(_toggleEnableDynamicHeight, _cameraConstraint.DynamicHeight);
		_updating = false;
	}
}
