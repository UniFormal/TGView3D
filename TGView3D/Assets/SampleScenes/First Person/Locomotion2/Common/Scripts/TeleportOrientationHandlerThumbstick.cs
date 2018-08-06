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
using System.Collections;

/// <summary>
/// This orientation handler will use the specified thumbstick to adjust the landing orientation of the teleport.
/// </summary>
public class TeleportOrientationHandlerThumbstick : TeleportOrientationHandler
{
	/// <summary>
	/// Used for thumbstick selection.
	/// </summary>
	public enum Thumbsticks
	{
		LeftThumbstick,
		RightThumbstick
	}

	/// <summary>
	/// HeadRelative=Character will orient to match the arrow. ForwardFacing=When user orients to match the arrow, they will be facing the sensors.
	/// </summary>
	[Tooltip("HeadRelative=Character will orient to match the arrow. ForwardFacing=When user orients to match the arrow, they will be facing the sensors.")]
	public OrientationModes OrientationMode;

	/// <summary>
	/// Which thumbstick is to be used for adjusting the teleport orientation.
	/// </summary>
	[Tooltip("Which thumbstick is to be used for adjusting the teleport orientation.")]
	public Thumbsticks Thumbstick;

	/// <summary>
	/// The orientation will only change if the thumbstick magnitude is above this value. This will usually be larger than the TeleportInputHandlerAvatarTouch.ThumbstickTeleportThreshold.
	/// </summary>
	[Tooltip("The orientation will only change if the thumbstick magnitude is above this value. This will usually be larger than the TeleportInputHandlerAvatarTouch.ThumbstickTeleportThreshold.")]
	public float RotateStickThreshold = 0.8f;

	private Quaternion _initialRotation;
	private Quaternion _currentRotation;
	private Vector2 _lastValidDirection;

	protected override void InitializeTeleportDestination()
	{
		_initialRotation = LocomotionTeleport.GetHeadRotationY();
		_currentRotation = _initialRotation;
		_lastValidDirection = new Vector2();
	}

	protected override void UpdateTeleportDestination()
	{
		var direction = OVRInput.Get(Thumbstick == Thumbsticks.LeftThumbstick
			? OVRInput.RawAxis2D.LThumbstick
			: OVRInput.RawAxis2D.RThumbstick);

		if (!AimData.TargetValid)
		{
			_lastValidDirection = new Vector2();
		}

		var length = direction.magnitude;

		if (length < RotateStickThreshold)
		{
			direction = _lastValidDirection;
			length = direction.magnitude;

			if (length < RotateStickThreshold)
			{
				_initialRotation = LocomotionTeleport.GetHeadRotationY();
				direction.x = 0;
				direction.y = 1;
			}
		}
		else
		{
			_lastValidDirection = direction;
		}

		var tracking = LocomotionTeleport.LocomotionController.CameraRig.trackingSpace.rotation;

		if (length > RotateStickThreshold)
		{
			direction /= length; // normalize the vector
			var rot = _initialRotation * Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
			_currentRotation = tracking * rot;
		}
		else
		{
			_currentRotation = tracking * LocomotionTeleport.GetHeadRotationY();
		}

		LocomotionTeleport.OnUpdateTeleportDestination(AimData.TargetValid, AimData.Destination, _currentRotation, GetLandingOrientation(OrientationMode, _currentRotation));
	}
}
