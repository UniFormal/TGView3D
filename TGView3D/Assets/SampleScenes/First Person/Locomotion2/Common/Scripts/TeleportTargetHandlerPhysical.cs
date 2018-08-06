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
using UnityEngine.EventSystems;

/// <summary>
/// This target handler simply returns any location that is detected by the aim collision tests.
/// Essentially, any space the player will fit will be a valid teleport destination.
/// </summary>
public class TeleportTargetHandlerPhysical : TeleportTargetHandler
{
	/// <summary>
	/// This method will be called while the LocmotionTeleport component is in the aiming state, once for each
	/// line segment that the targeting beam requires. 
	/// The function should return true whenever an actual target location has been selected.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	protected override bool ConsiderTeleport(Vector3 start, ref Vector3 end)
	{
		// If the ray hits the world, consider it valid and update the aimRay to the end point.
		if (LocomotionTeleport.AimCollisionTest(start, end, AimCollisionLayerMask, out AimData.TargetHitInfo))
		{
			var d = (end - start).normalized;

			end = start + d * AimData.TargetHitInfo.distance;
			return true;
		}
		return false;
	}
}
