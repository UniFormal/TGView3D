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
/// This panel provides in-game access to a variety of tuning parameters for the teleport system.
/// Note that the controls here are just a subset of the full set of options visible in the editor,
/// and is a finer level of control than is provided by the Locomotion Presets panel. If you change
/// values in this panel, you may find the locomotion presets panel no longer configures everything
/// as expected so keep that in mind when tweaking these values at runtime.
/// 
/// It is unlikely a shipping title will need this kind of control visible to the end user; this
/// is provided here mainly as a way to experiment and adjust various features without fully reconfiguring
/// the system like the Locomotion Presets panel does.
/// </summary>
public class TeleportTuningPanel : LocomotionSampleSupport
{
	public GameObject LabelTemplate;
	public GameObject ToggleTemplate;

	private Toggle _toggleAvatarTouch;
	private Toggle _toggleGenericHMD;
	private Toggle _toggleLaser;
	private Toggle _toggleParabolic;
	private Toggle _toggleNavMesh;
	private Toggle _toggleTeleportNode;
	private Toggle _toggleGeometry;
	private Toggle _toggle360;
	private Toggle _toggleHMD;
	private Toggle _toggleThumbstickRelative;
	private Toggle _toggleThumbstickForward;
	private Toggle _toggleInstant;
	private Toggle _toggleBlink;
	private Toggle _toggleWarp;
	private Toggle _toggleLinearMotionReady;
	private Toggle _toggleLinearMotionAim;
	private Toggle _toggleLinearMotionPreTeleport;
	private Toggle _toggleLinearMotionPostTeleport;
	private Toggle _toggleRotationReady;
	private Toggle _toggleRotationAim;
	private Toggle _toggleRotationPreTeleport;
	private Toggle _toggleRotationPostTeleport;

	private MonoBehaviour[] _locomotionBehaviors;
	private LocomotionTeleport _teleportController;
	private bool _updating;

	// Use this for initialization
	void Start ()
	{
		_locomotionBehaviors = LocomotionController.GetComponents<MonoBehaviour>();

		AddLabel("Input Handler");
		_toggleAvatarTouch = SetupToggle<TeleportInputHandler, TeleportInputHandlerAvatarTouch>("Avatar Touch");
		_toggleGenericHMD = SetupToggle<TeleportInputHandler, TeleportInputHandlerHMD>("Generic HMD");

		AddLabel("Aim Handler");
		_toggleLaser = SetupToggle<TeleportAimHandler, TeleportAimHandlerLaser>("Laser");
		_toggleParabolic = SetupToggle<TeleportAimHandler, TeleportAimHandlerParabolic>("Parabolic");
		
		AddLabel("Target Handler");
		_toggleNavMesh = SetupToggle<TeleportTargetHandler, TeleportTargetHandlerNavMesh>("Nav Mesh");
		_toggleTeleportNode = SetupToggle<TeleportTargetHandler, TeleportTargetHandlerNode>("Teleport Node");
		_toggleGeometry = SetupToggle<TeleportTargetHandler, TeleportTargetHandlerPhysical>("Geometry");

		AddLabel("Orientation Handler");
		_toggle360 = SetupToggle<TeleportOrientationHandler, TeleportOrientationHandler360>("360");
		_toggleHMD = SetupToggle<TeleportOrientationHandler, TeleportOrientationHandlerHMD>("HMD");
		_toggleThumbstickRelative = SetupToggle<TeleportOrientationHandler, TeleportOrientationHandlerThumbstick>("Thumbstick Head Relative", TeleportOrientationHandler.OrientationModes.HeadRelative);
		_toggleThumbstickForward = SetupToggle<TeleportOrientationHandler, TeleportOrientationHandlerThumbstick>("Thumbstick Forward Facing", TeleportOrientationHandler.OrientationModes.ForwardFacing);

		AddLabel("Transition");
		_toggleInstant = SetupToggle<TeleportTransition, TeleportTransitionInstant>("Instant");
		_toggleBlink = SetupToggle<TeleportTransition, TeleportTransitionBlink>("Blink");
		_toggleWarp = SetupToggle<TeleportTransition, TeleportTransitionWarp>("Warp (no orientation)");

		AddLabel("Enable Linear Motion During Teleport States");
		_teleportController = LocomotionController.GetComponent<LocomotionTeleport>();
		_toggleLinearMotionReady = SetupToggle("Ready State Motion", ToggleReadyStateMotion);
		_toggleLinearMotionAim = SetupToggle("Aim State Motion", ToggleAimStateMotion);
		_toggleLinearMotionPreTeleport = SetupToggle("PreTeleport State Motion", TogglePreTeleportStateMotion);
		_toggleLinearMotionPostTeleport = SetupToggle("PostTeleport State Motion", TogglePostTeleportStateMotion);

		AddLabel("Enable Rotation During Teleport States");
		_teleportController = LocomotionController.GetComponent<LocomotionTeleport>();
		_toggleRotationReady = SetupToggle("Ready State Rotation", ToggleReadyStateRotation);
		_toggleRotationAim = SetupToggle("Aim State Rotation", ToggleAimStateRotation);
		_toggleRotationPreTeleport = SetupToggle("PreTeleport State Rotation", TogglePreTeleportStateRotation);
		_toggleRotationPostTeleport = SetupToggle("PostTeleport State Rotation", TogglePostTeleportStateRotation);

		ToggleTemplate.SetActive(false);
		LabelTemplate.SetActive(false);
	}

	private void ToggleReadyStateMotion()
	{
		_teleportController.EnableMovementDuringReady = _toggleLinearMotionReady.isOn;
	}

	void ToggleAimStateMotion()
	{
		_teleportController.EnableMovementDuringAim = _toggleLinearMotionAim.isOn;
	}

	void TogglePreTeleportStateMotion()
	{
		_teleportController.EnableMovementDuringPreTeleport = _toggleLinearMotionPreTeleport.isOn;
	}

	void TogglePostTeleportStateMotion()
	{
		_teleportController.EnableMovementDuringPostTeleport = _toggleLinearMotionPostTeleport.isOn;
	}

	private void ToggleReadyStateRotation()
	{
		_teleportController.EnableRotationDuringReady = _toggleRotationReady.isOn;
	}

	void ToggleAimStateRotation()
	{
		_teleportController.EnableRotationDuringAim = _toggleRotationAim.isOn;
	}

	void TogglePreTeleportStateRotation()
	{
		_teleportController.EnableRotationDuringPreTeleport = _toggleRotationPreTeleport.isOn;
	}

	void TogglePostTeleportStateRotation()
	{
		_teleportController.EnableRotationDuringPostTeleport = _toggleRotationPostTeleport.isOn;
	}

	Toggle SetupToggle<TCategory, TActivate>(string label, TeleportOrientationHandler.OrientationModes mode) where TCategory : MonoBehaviour where TActivate : TeleportOrientationHandlerThumbstick
	{
		var toggle = AddToggle(label);
		toggle.onValueChanged.AddListener((b) =>
		{
			if (!_updating)
			{
				var orientationComponent = ActivateCategory<TCategory, TActivate>();
				orientationComponent.OrientationMode = mode;
			} 
			
		});
		return toggle;
	}



	Toggle SetupToggle<TCategory, TActivate>(string label) where TCategory : MonoBehaviour where TActivate : MonoBehaviour
	{
		var toggle = AddToggle(label);
		toggle.onValueChanged.AddListener((b) => { if(!_updating) ActivateCategory<TCategory, TActivate>(); });
		return toggle;
	}

	Toggle SetupToggle(string label, Action action) 
	{
		var toggle = AddToggle(label);
		toggle.onValueChanged.AddListener((b) => { if (!_updating) action(); });
		return toggle;
	}

	Toggle AddToggle(string label)
	{
		var go = AddInstance(ToggleTemplate, label);
		var toggle = go.GetComponent<Toggle>();
		var text = go.GetComponentInChildren<Text>();
		text.text = label;
		return toggle;
	}

	void AddLabel(string label)
	{
		var go = AddInstance(LabelTemplate, label);
		var text = go.GetComponentInChildren<Text>();
		text.text = label;
	}

	T GetExactComponent<T>() where T : MonoBehaviour
	{
		for (int i = 0; i < _locomotionBehaviors.Length; i++)
		{
			var b = _locomotionBehaviors[i];
			if (b.GetType() == typeof(T))
			{
				return (T)b;
			}
		}
		return null;
	}

	void UpdateToggle<TBehavior>(Toggle toggle) where TBehavior : MonoBehaviour
	{
		var isEnabled = GetExactComponent<TBehavior>().enabled;

		UpdateToggle(toggle, isEnabled);
	}

	void UpdateToggle<TBehavior>(Toggle toggle, TeleportOrientationHandler.OrientationModes mode) where TBehavior : TeleportOrientationHandlerThumbstick
	{
		var component = GetExactComponent<TBehavior>();
		var isEnabled = component.enabled && component.OrientationMode == mode;

		UpdateToggle(toggle, isEnabled);
	}

	void Update ()
	{
		_updating = true;

		UpdateToggle<TeleportInputHandlerAvatarTouch>(_toggleAvatarTouch);
		UpdateToggle<TeleportInputHandlerHMD>(_toggleGenericHMD);
		UpdateToggle<TeleportAimHandlerLaser>(_toggleLaser);
		UpdateToggle<TeleportAimHandlerParabolic>(_toggleParabolic);
		UpdateToggle<TeleportTargetHandlerNavMesh>(_toggleNavMesh);
		UpdateToggle<TeleportTargetHandlerNode>(_toggleTeleportNode);
		UpdateToggle<TeleportTargetHandlerPhysical>(_toggleGeometry);
		UpdateToggle<TeleportOrientationHandler360>(_toggle360);
		UpdateToggle<TeleportOrientationHandlerHMD>(_toggleHMD);
		UpdateToggle<TeleportOrientationHandlerThumbstick>(_toggleThumbstickRelative, TeleportOrientationHandler.OrientationModes.HeadRelative);
		UpdateToggle<TeleportOrientationHandlerThumbstick>(_toggleThumbstickForward, TeleportOrientationHandler.OrientationModes.ForwardFacing);
		UpdateToggle<TeleportTransitionInstant>(_toggleInstant);
		UpdateToggle<TeleportTransitionBlink>(_toggleBlink);
		UpdateToggle<TeleportTransitionWarp>(_toggleWarp);

		UpdateToggle(_toggleLinearMotionReady, _teleportController.EnableMovementDuringReady);
		UpdateToggle(_toggleLinearMotionAim, _teleportController.EnableMovementDuringAim);
		UpdateToggle(_toggleLinearMotionPreTeleport, _teleportController.EnableMovementDuringPreTeleport);
		UpdateToggle(_toggleLinearMotionPostTeleport, _teleportController.EnableMovementDuringPostTeleport);

		UpdateToggle(_toggleRotationReady, _teleportController.EnableRotationDuringReady);
		UpdateToggle(_toggleRotationAim, _teleportController.EnableRotationDuringAim);
		UpdateToggle(_toggleRotationPreTeleport, _teleportController.EnableRotationDuringPreTeleport);
		UpdateToggle(_toggleRotationPostTeleport, _teleportController.EnableRotationDuringPostTeleport);

		_updating = false;
	}
}
