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
using UnityEngine.UI;
using System.Collections;

public class GunManager : MonoBehaviour
{

    /// <summary>
    /// bullets
    /// </summary>
    public Rigidbody bullet_0 = null;  // not affected by gravity
    public Rigidbody bullet_1 = null;  // affected by gravity

    /// <summary>
    /// UI material
    /// </summary>
    public Material uiMaterial = null;

    /// <summary>
    /// To get muzzle position for bullet's starting point
    /// </summary>
    public GameObject muzzle = null;

    /// <summary>
    /// Text update for bullet info
    /// </summary>
    public Text bulletInfo = null;

    /// <summary>
    /// Bullet Speed Accel
    /// </summary>
    private float bulletAccel = 1.0f;

    /// <summary>
    /// Bullet Speed
    /// </summary>
    private float defaultSpeed = 300.0f;

    /// <summary>
    /// Trigger for switching bullets
    /// </summary>
    private bool gravityBullet = false;

    /// <summary>
    /// Trigger for auto fire
    /// </summary>
    private bool autoFire = false;

    /// <summary>
    /// Managing for shooting bullet numbers at a time
    /// </summary>
    private bool fireDepressedLastFrame = false;

    /// <summary>
    /// Use crosshair as target for bullet
    /// </summary>
    public Transform target;

    /// <summary>
    /// Trigger for aiming at crosshair
    /// </summary>
    private bool aimingCrosshair = true;

    /// <summary>
    /// Set gravity
    /// </summary>
    float gravity = -9.8f;

    /// <summary>
    /// The button which fires the weapon
    /// </summary>
    OVRInput.Button fireButton = OVRInput.Button.One;

    /// <summary>
    /// The key which fires the weapon
    /// </summary>
    KeyCode fireKey = KeyCode.Space;

    /// <summary>
    /// Update in MonoBehaviour 
    /// </summary>
    void Update()
    {
        // Fire weapon
        if (OVRInput.Get(fireButton) || Input.GetKey(fireKey) || Input.GetMouseButtonDown(0))
        {
            if (autoFire || !fireDepressedLastFrame)
            {
                FireBullet(gravityBullet);
            }
            fireDepressedLastFrame = true;
        }
        else
        {
            fireDepressedLastFrame = false;
        }
    }

    /// <summary>
    /// Instantiate bullets and manage for physics 
    /// </summary>
    void FireBullet(bool isGravityBullet)
    {
        Rigidbody bullets;
        if (target.gameObject.activeInHierarchy)
        {
            if (isGravityBullet)
            {
                bullets = (Rigidbody)Instantiate(bullet_1, muzzle.transform.position, transform.parent.rotation);
                // Targeting crosshair if you don't want parallel motion for bullet
                if (aimingCrosshair)
                {
                    bullets.transform.LookAt(target);
                }
                // Divide by 3.6f(3,600 secs) to get KM/h
                bullets.velocity = bullets.transform.forward * defaultSpeed * bulletAccel / 3.6f;
                Physics.gravity = new Vector3(0, gravity, 0);
            }
            else
            {
                bullets = (Rigidbody)Instantiate(bullet_0, muzzle.transform.position, transform.parent.rotation);
                // Targeting crosshair if you don't want parallel motion for bullet
                if (aimingCrosshair)
                {
                    bullets.transform.LookAt(target);
                }
                // Divide by 3.6f(3,600 secs) to get KM/h
                bullets.velocity = bullets.transform.forward * defaultSpeed * bulletAccel / 3.6f;
            }
        }
    }

    /// <summary>
    /// Manage bullet speed 
    /// </summary>
    public void AdjustBulletSpeed(float spd)
    {
        bulletAccel = spd;
        bulletInfo.text = System.String.Format("Bullet Speed ({0:F0} km/h)", defaultSpeed * bulletAccel);
    }

    /// <summary>
    /// Set bullet type
    /// </summary>
    public void SetBulletType(bool on)
    {
        gravityBullet = on;
    }

    /// <summary>
    /// Set auto fire for shooting several bullets at a time
    /// </summary>
    public void SetAutoFire(bool on)
    {
        autoFire = on;
    }

    /// <summary>
    /// Set bullet whether to aim at crosshair or move parallel
    /// </summary>
    public void SetAimingCrosshair(bool on)
    {
        aimingCrosshair = on;
    }

}
