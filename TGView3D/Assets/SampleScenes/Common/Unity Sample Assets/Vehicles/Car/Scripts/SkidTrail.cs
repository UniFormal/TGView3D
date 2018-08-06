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

namespace UnitySampleAssets.Vehicles.Car
{
    public class SkidTrail : MonoBehaviour
    {

        // this script works in conjunction with a trail renderer to leave a skid trail behind wheels.
        // The Wheel script instantiates a new skid trail for each new skid, which begins parented to the wheel,
        // and is detached (by setting the transform.parent to null) when the skid stops.

        [SerializeField] private float persistTime; // the amount of time for the skid trail to persist
        [SerializeField] private float fadeDuration; // the amount of time before the skid trail will start to fade
        private float startAlpha;

        // Use this for initialization
        private IEnumerator Start()
        {

            while (true)
            {
                yield return new WaitForSeconds(1);

                // check whether this skid trail has finished
                // (the Wheel script sets the parent to null when the skid finishes)
                if (transform.parent == null)
                {

                    // set the start colour
                    Color startCol = GetComponent<Renderer>().material.color;

                    // wait for the persist time
                    yield return new WaitForSeconds(persistTime);

                    float t = Time.time;

                    // fade out the skid mark
                    while (Time.time < t + fadeDuration)
                    {
                        float i = Mathf.InverseLerp(t, t + fadeDuration, Time.time);
                        GetComponent<Renderer>().material.color = startCol*new Color(1, 1, 1, 1 - i);
                        yield return null;
                    }

                    // the object has faded and is now done so destroy it
                    Destroy(gameObject);
                }
            }
        }
    }
}
