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
using JetBrains.Annotations;
using UnityEngine.Assertions;

/// <summary>
/// This currently just provides accessors for the current locomotion component to retrieve shared settings,
/// but the longer term plan is for this to be the master object for a game object that contains all the 
/// components necessary for defining a specific configuration for locomotion. The intention is to make it possible 
/// for the developer to make multiple LocomotionController objects, each with specific tunings to allow the user to choose 
/// between configurations (such as teleport vs linear motion) by simply activating the appropriate LocomotionController
/// and deactivating the rest.
/// </summary>
public class LocomotionController : MonoBehaviour
{
    public OVRCameraRig CameraRig;
    public CharacterController CharacterController;
    public OVRPlayerController PlayerController;

    void Start()
    {
        if (CharacterController == null)
        {
            CharacterController = GetComponentInParent<CharacterController>();
        }
        Assert.IsNotNull(CharacterController);
        if (PlayerController == null)
        {
            PlayerController = GetComponentInParent<OVRPlayerController>();
        }
        Assert.IsNotNull(PlayerController);
    }
}
