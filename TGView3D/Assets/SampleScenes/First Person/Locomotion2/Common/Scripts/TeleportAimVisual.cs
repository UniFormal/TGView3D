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
/// The TeleportAimVisual is responsible for rendering visual effects to indicate where the teleport will occur.
/// In practice this will typically be some kind of beam effect, but the specific appearance is defined by the 
/// component. The Aim Visual monitors the Target Handler to determine the teleport destination, and if the 
/// destination is valid. It uses the AimData provided by the LocomotionTeleport if necessary for rendering
/// an effect between the player and the destination.
/// </summary>
public abstract class TeleportAimVisual : TeleportSupport
{

}
