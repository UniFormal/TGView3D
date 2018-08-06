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

using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// Base class for the panels used in this sample which provides common functionality for setting up various teleport handlers
/// and other minor support functions.
/// </summary>
public class LocomotionSampleSupport : MonoBehaviour
{
	public LocomotionController LocomotionController;

	[Conditional("DEBUG_LOCOMOTION_PANEL")]
	static void Log(string msg)
	{
		Debug.Log(msg);
	}

	/// <summary>
	/// This method will ensure only one specific type TActivate in a given group of components derived from the same TCategory type is enabled.
	/// This is used by the sample support code to select between different targeting, input, aim, and other handlers.
	/// </summary>
	/// <typeparam name="TCategory"></typeparam>
	/// <typeparam name="TActivate"></typeparam>
	/// <param name="target"></param>
	public static TActivate ActivateCategory<TCategory, TActivate>(GameObject target) where TCategory : MonoBehaviour where TActivate : MonoBehaviour
	{
		var components = target.GetComponents<TCategory>();
		Log("Activate " + typeof(TActivate) + " derived from " + typeof(TCategory) + "[" + components.Length + "]");
		TActivate result = null;
		for (int i = 0; i < components.Length; i++)
		{
			var c = (MonoBehaviour)components[i];
			var active = c.GetType() == typeof(TActivate);
			Log(c.GetType() + " is " + typeof(TActivate) + " = " + active);
			if (active)
			{
				result = (TActivate) c;
			}
			if (c.enabled != active)
			{
				c.enabled = active;
			}
		}
		return result;
	}

	/// <summary>
	/// This generic method is used for activating a specific set of components in the LocomotionController. This is just one way 
	/// to achieve the goal of enabling one component of each category (input, aim, target, orientation and transition) that
	/// the teleport system requires.
	/// </summary>
	/// <typeparam name="TInput"></typeparam>
	/// <typeparam name="TAim"></typeparam>
	/// <typeparam name="TTarget"></typeparam>
	/// <typeparam name="TOrientation"></typeparam>
	/// <typeparam name="TTransition"></typeparam>
	protected void ActivateHandlers<TInput, TAim, TTarget, TOrientation, TTransition>()
		where TInput : TeleportInputHandler
		where TAim : TeleportAimHandler
		where TTarget : TeleportTargetHandler
		where TOrientation : TeleportOrientationHandler
		where TTransition : TeleportTransition
	{
		ActivateInput<TInput>();
		ActivateAim<TAim>();
		ActivateTarget<TTarget>();
		ActivateOrientation<TOrientation>();
		ActivateTransition<TTransition>();
	}

	protected void ActivateInput<TActivate>() where TActivate : TeleportInputHandler
	{
		ActivateCategory<TeleportInputHandler, TActivate>();
	}

	protected void ActivateAim<TActivate>() where TActivate : TeleportAimHandler
	{
		ActivateCategory<TeleportAimHandler, TActivate>();
	}

	protected void ActivateTarget<TActivate>() where TActivate : TeleportTargetHandler
	{
		ActivateCategory<TeleportTargetHandler, TActivate>();
	}

	protected void ActivateOrientation<TActivate>() where TActivate : TeleportOrientationHandler
	{
		ActivateCategory<TeleportOrientationHandler, TActivate>();
	}

	protected void ActivateTransition<TActivate>() where TActivate : TeleportTransition
	{
		ActivateCategory<TeleportTransition, TActivate>();
	}

	protected TActivate ActivateCategory<TCategory, TActivate>() where TCategory : MonoBehaviour where TActivate : MonoBehaviour
	{
		return ActivateCategory<TCategory, TActivate>(LocomotionController.gameObject);
	}

	protected void UpdateToggle(Toggle toggle, bool enabled)
	{
		if (enabled != toggle.isOn)
		{
			toggle.isOn = enabled;
		}
	}

	protected GameObject AddInstance(GameObject template, string label)
	{
		var go = Instantiate(template);
		go.transform.SetParent(transform, false);
		go.name = label;
		return go;
	}
}
