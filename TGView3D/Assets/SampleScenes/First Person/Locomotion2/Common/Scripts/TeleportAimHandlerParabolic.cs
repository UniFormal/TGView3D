﻿/************************************************************************************

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
using System.Collections.Generic;

/// <summary>
/// This aim handler simulates the parabolic curve that a thrown item would follow, a common style of teleport aiming.
/// </summary>
public class TeleportAimHandlerParabolic : TeleportAimHandler
{
	/// <summary>
	/// Maximum range for aiming.
	/// </summary>
	[Tooltip("Maximum range for aiming.")]
	public float Range;

	/// <summary>
	/// The MinimumElevation is relative to the AimPosition.
	/// </summary>
	[Tooltip("The MinimumElevation is relative to the AimPosition.")]
	public float MinimumElevation = -100;

	/// <summary>
	/// The Gravity is used in conjunction with AimVelocity and the aim direction to simulate a projectile.
	/// </summary>
	[Tooltip("The Gravity is used in conjunction with AimVelocity and the aim direction to simulate a projectile.")]
	public float Gravity = -9.8f;

	/// <summary>
	/// The AimVelocity is the initial speed of the faked projectile.
	/// </summary>
	[Tooltip("The AimVelocity is the initial speed of the faked projectile.")]
	[Range(0.001f, 50.0f)]
	public float AimVelocity = 1;

	/// <summary>
	/// The AimStep is the how much to subdivide the iteration.
	/// </summary>
	[Tooltip("The AimStep is the how much to subdivide the iteration.")]
	[Range(0.001f, 1.0f)]
	public float AimStep = 1;

	/// <summary>
	/// Return the set of points that represent the aiming line.
	/// </summary>
	/// <param name="points"></param>
	public override void GetPoints(List<Vector3> points)
	{
		Ray startRay;

		LocomotionTeleport.InputHandler.GetAimData(out startRay);

		var aimPosition = startRay.origin;
		var aimDirection = startRay.direction * AimVelocity;
		var rangeSquared = Range * Range;
		do
		{
			points.Add(aimPosition);

			var aimVector = aimDirection;
			aimVector.y = aimVector.y + Gravity * 0.0111111111f * AimStep;
			aimDirection = aimVector;
			aimPosition += aimVector * AimStep;

		} while ((aimPosition.y - startRay.origin.y > MinimumElevation) && ((startRay.origin - aimPosition).sqrMagnitude <= rangeSquared));
	}
}
