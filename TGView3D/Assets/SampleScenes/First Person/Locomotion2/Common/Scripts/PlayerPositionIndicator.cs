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
using UnityEngine.Assertions;
using UnityEngine.VR;

/// <summary>
/// The PlayerPositionIndicator is used for visualizing where the player position is relative to the tracking center,
/// along with the orientation. This is primarily intended for diagnosing the position of these objects along with the
/// character capsule to visually confirm everything is moving as expected. 
/// </summary>
public class PlayerPositionIndicator : MonoBehaviour
{
    public float OffsetY;
    public OVRCameraRig CameraRig;
    public Transform CameraCenter;
    public Transform CompassIndicator;

    [Tooltip("This should reference the object that this script will move to match the player's position and heading.")]
    public Transform PlayerIndicator;

    [Tooltip("This should reference the object that this script will move to match the view center and forward direction.")]
    public Transform ViewIndicator;

    void Start()
    {
        Assert.IsNotNull(CameraRig);
        Assert.IsNotNull(CameraCenter);
        Assert.IsNotNull(PlayerIndicator);
        Assert.IsNotNull(ViewIndicator);
    }

    void Update()
    {
        Vector3 viewPos = CameraRig.trackingSpace.position;
        var rot = CameraRig.trackingSpace.rotation;

        viewPos.y += OffsetY;
        ViewIndicator.transform.position = viewPos;
        ViewIndicator.transform.rotation = rot;

        CompassIndicator.transform.position = viewPos;
        CompassIndicator.transform.rotation = Quaternion.identity;

        var playerPos = CameraCenter.transform.position;
        playerPos.y = viewPos.y;
        PlayerIndicator.transform.position = playerPos;
        var forward = CameraCenter.transform.forward;
        forward.y = 0;
        forward.Normalize();
        PlayerIndicator.transform.forward = forward;
    }
}
