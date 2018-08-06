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

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

/// <summary>
/// When this component is enabled, the player will be able to aim and trigger teleport behavior using Avatar Touch controllers.
/// </summary>
public class TeleportInputHandlerAvatarTouch : TeleportInputHandlerHMD
{
	/// <summary>
	/// This needs to be assigned a reference to the OvrAvatar for the local player. The code will search for an avatar if this is null, but it's suggested to assign it in the editor.
	/// </summary>
	[Tooltip("This needs to be assigned a reference to the OvrAvatar for the local player. The code will search for an avatar if this is null, but it's suggested to assign it in the editor.")]
	public OvrAvatar Avatar;

	/// <summary>
	/// The avatar touch input handler supports three different modes for controlling teleports.
	/// </summary>
	public enum InputModes
	{
		/// <summary>
		/// Touching a capacitive button will start the aiming, and pressing that button will trigger the teleport.
		/// </summary>
		CapacitiveButtonForAimAndTeleport,

		/// <summary>
		/// One button will start the aiming, another button will trigger the teleport.
		/// </summary>
		SeparateButtonsForAimAndTeleport,

		/// <summary>
		/// A thumbstick is used for starting the aiming, and releasing the thumbstick will trigger the teleport.
		/// </summary>
		ThumbstickTeleport
	}

	[Tooltip("CapacitiveButtonForAimAndTeleport=Activate aiming via cap touch detection, press the same button to teleport.\nSeparateButtonsForAimAndTeleport=Use one button to begin aiming, and another to trigger the teleport.\nThumbstickTeleport=Push a thumbstick to begin aiming, release to teleport.")]
	public InputModes InputMode;

	/// <summary>
	/// These buttons are used for selecting which capacitive button is used when InputMode==CapacitiveButtonForAimAndTeleport
	/// </summary>
	public enum AimCapTouchButtons
	{
		A,
		B,
		LeftTrigger,
		LeftThumbstick,
		RightTrigger,
		RightThumbstick,
		X,
		Y
	}

	private readonly OVRInput.RawButton[] _rawButtons = {
		OVRInput.RawButton.A,
		OVRInput.RawButton.B,
		OVRInput.RawButton.LIndexTrigger,
		OVRInput.RawButton.LThumbstick,
		OVRInput.RawButton.RIndexTrigger,
		OVRInput.RawButton.RThumbstick,
		OVRInput.RawButton.X,
		OVRInput.RawButton.Y
	};

	private readonly OVRInput.RawTouch[] _rawTouch = {
		OVRInput.RawTouch.A,
		OVRInput.RawTouch.B,
		OVRInput.RawTouch.LIndexTrigger,
		OVRInput.RawTouch.LThumbstick,
		OVRInput.RawTouch.RIndexTrigger,
		OVRInput.RawTouch.RThumbstick,
		OVRInput.RawTouch.X,
		OVRInput.RawTouch.Y
	};

	/// <summary>
	/// This enum is used for controlling which controller is being used for aiming.
	/// </summary>
	public enum AimingControllers
	{
		Left,
		Right
	}

	/// <summary>
	/// Which controller is being used for aiming.
	/// </summary>
	[Tooltip("Select the controller to be used for aiming.")]
	public AimingControllers AimingController;

	/// <summary>
	/// The button to use for triggering aim and teleport when InputMode==CapacitiveButtonForAimAndTeleport
	/// </summary>
	[Tooltip("Select the button to use for triggering aim and teleport when InputMode==CapacitiveButtonForAimAndTeleport")]
	public AimCapTouchButtons CapacitiveAimAndTeleportButton;

	/// <summary>
	/// The thumbstick magnitude required to trigger aiming and teleports when InputMode==InputModes.ThumbstickTeleport
	/// </summary>
	[Tooltip("The thumbstick magnitude required to trigger aiming and teleports when InputMode==InputModes.ThumbstickTeleport")]
	public float ThumbstickTeleportThreshold = 0.5f;

	void Start () {
		if (Avatar == null)
		{
			Debug.LogWarning("Avatar not assigned. Searching hierarchy. Please configure the Avatar before running to improve performance.");
			Avatar = GameObject.FindObjectOfType<OvrAvatar>();
			Assert.IsNotNull(Avatar);
		}
	}

	/// <summary>
	/// Based on the input mode, controller state, and current intention of the teleport controller, return the apparent intention of the user.
	/// </summary>
	/// <returns></returns>
	public override LocomotionTeleport.TeleportIntentions GetIntention()
	{
		if (!isActiveAndEnabled)
		{
			return global::LocomotionTeleport.TeleportIntentions.None;
		}

		// If capacitive touch isn't being used, the base implementation will do the work.
		if (InputMode == InputModes.SeparateButtonsForAimAndTeleport)
		{
			return base.GetIntention();
		}

		// ThumbstickTeleport will begin aiming when the thumbstick is pushed.
		if (InputMode == InputModes.ThumbstickTeleport)
		{
			var direction = OVRInput.Get(AimingController == AimingControllers.Left
				? OVRInput.RawAxis2D.LThumbstick
				: OVRInput.RawAxis2D.RThumbstick);

			bool touching = direction.magnitude > ThumbstickTeleportThreshold
				|| OVRInput.Get(AimingController == AimingControllers.Left ? OVRInput.RawTouch.LThumbstick : OVRInput.RawTouch.RThumbstick);
			if (!touching)
			{
				if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.Aim)
				{
					// If the user has released the thumbstick, enter the PreTeleport state unless FastTeleport is enabled, 
					// in which case enter the Teleport state.
					return FastTeleport ? LocomotionTeleport.TeleportIntentions.Teleport : LocomotionTeleport.TeleportIntentions.PreTeleport;
				}

				// If the user is already in the preteleport state, the intention will be to either remain in this state or switch to Teleport
				if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.PreTeleport)
				{
					return LocomotionTeleport.TeleportIntentions.Teleport;
				}
			}
			else
			{
				if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.Aim)
				{
					return LocomotionTeleport.TeleportIntentions.Aim;
				}
			}

			if (direction.y > ThumbstickTeleportThreshold)
			{
				return LocomotionTeleport.TeleportIntentions.Aim;
			}

			return LocomotionTeleport.TeleportIntentions.None;
		}

		// Capacitive touch logic is essentially the same as the base logic, except the button types are different
		// so different methods need to be used.
		var teleportButton = _rawButtons[(int)CapacitiveAimAndTeleportButton];

		if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.Aim)
		{
			// If the user has actually pressed the teleport button, enter the preteleport state.
			if (OVRInput.GetDown(teleportButton))
			{
				// If the user has released the thumbstick, enter the PreTeleport state unless FastTeleport is enabled, 
				// in which case enter the Teleport state.
				return FastTeleport ? LocomotionTeleport.TeleportIntentions.Teleport : LocomotionTeleport.TeleportIntentions.PreTeleport;
			}
		}

		// If the user is already in the PreTeleport state, the intention will be to either remain in this state or switch to Teleport
		if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.PreTeleport)
		{
			// If they released the button, switch to Teleport.
			if (FastTeleport || OVRInput.GetUp(teleportButton))
			{
				// Button released, enter the Teleport state.
				return LocomotionTeleport.TeleportIntentions.Teleport;
			}
			// Button still down, remain in PreTeleport so they can orient the destination if an orientation handler supports it.
			return LocomotionTeleport.TeleportIntentions.PreTeleport;
		}

		// If it made it this far, then we need to determine if the user intends to be aiming with the capacitive touch.
		// The first check is if cap touch has been triggered. 
		if (OVRInput.GetDown(_rawTouch[(int)CapacitiveAimAndTeleportButton]))
		{
			return LocomotionTeleport.TeleportIntentions.Aim;
		}

		if (LocomotionTeleport.CurrentIntention == LocomotionTeleport.TeleportIntentions.Aim)
		{
			if (!OVRInput.GetUp(_rawTouch[(int)CapacitiveAimAndTeleportButton]))
			{
				return LocomotionTeleport.TeleportIntentions.Aim;
			}
		}

		return LocomotionTeleport.TeleportIntentions.None;
	}

	public override void GetAimData(out Ray aimRay)
	{
		var t = (AimingController == AimingControllers.Left) ? Avatar.ControllerLeft.transform : Avatar.ControllerRight.transform;
		aimRay = new Ray(t.position, t.forward);
	}
}
