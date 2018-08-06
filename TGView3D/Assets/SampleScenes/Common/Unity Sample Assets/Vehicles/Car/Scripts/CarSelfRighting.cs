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

namespace UnitySampleAssets.Vehicles.Car
{
    public class CarSelfRighting : MonoBehaviour
    {

        // Automatically put the car the right way up, if it has come to rest upside-down.
        [SerializeField] private float waitTime = 3f; // time to wait before self righting
        [SerializeField] private float velocityThreshold = 1f;// the velocity below which the car is considered stationary for self-righting

        private float lastOkTime; // the last time that the car was in an OK state

        private void Update()
        {
            // is the car is the right way up
            if (transform.up.y > 0f || GetComponent<Rigidbody>().velocity.magnitude > velocityThreshold)
            {
                lastOkTime = Time.time;
            }

            if (Time.time > lastOkTime + waitTime)
            {
                RightCar();
            }
        }

        // put the car back the right way up:
        private void RightCar()
        {
            // set the correct orientation for the car, and lift it off the ground a little
            transform.position += Vector3.up;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
    }
}
