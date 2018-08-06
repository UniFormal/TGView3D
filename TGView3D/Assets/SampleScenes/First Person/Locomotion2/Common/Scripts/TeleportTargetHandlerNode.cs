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

using UnityEngine;

/// <summary>
/// This target handler will only return locations that the aim system detects that contain a TeleportPoint component.
/// </summary>
public class TeleportTargetHandlerNode : TeleportTargetHandler
{
	/// <summary>
	/// When checking line of sight to the destination, add this value to the vertical offset for targeting collision checks.
	/// </summary>
	[Tooltip("When checking line of sight to the destination, add this value to the vertical offset for targeting collision checks.")]
	public float LOSOffset = 1.0f;

	/// <summary>
	/// Teleport logic will only work with TeleportPoint components that exist in the layers specified by this mask.
	/// </summary>
	[Tooltip("Teleport logic will only work with TeleportPoint components that exist in the layers specified by this mask.")]
	public LayerMask TeleportLayerMask;

	/// <summary>
	/// This method will be called while the LocmotionTeleport component is in the aiming state, once for each
	/// line segment that the targeting beam requires. 
	/// The function should return true whenever an actual target location has been selected.
	/// </summary>
	protected override bool ConsiderTeleport(Vector3 start, ref Vector3 end)
	{
		// If the ray hits the world, consider it valid and update the aimRay to the end point.
		if (!LocomotionTeleport.AimCollisionTest(start, end, AimCollisionLayerMask | TeleportLayerMask, out AimData.TargetHitInfo))
		{
			return false;
		}
		TeleportPoint tp = AimData.TargetHitInfo.collider.gameObject.GetComponent<TeleportPoint>();
		if (tp == null)
		{
			return false;
		}

		// The targeting test discovered a valid teleport node. Now test to make sure there is line of sight to the 
		// actual destination. Since the teleport destination is expected to be right on the ground, use the LOSOffset 
		// to bump the collision check up off the ground a bit.
		var dest = tp.destTransform.position;
		var offsetEnd = new Vector3(dest.x, dest.y + LOSOffset, dest.z);
		if (LocomotionTeleport.AimCollisionTest(start, offsetEnd, AimCollisionLayerMask & ~TeleportLayerMask, out AimData.TargetHitInfo))
		{
			return false;
		}

		end = dest;
		return true;
	}
}
