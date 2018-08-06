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

namespace UnitySampleAssets.Utility
{

    public class ActivateTrigger : MonoBehaviour
    {

        // A multi-purpose script which causes an action to occur when
        // a trigger collider is entered.

        public enum Mode
        {
            Trigger = 0, // Just broadcast the action on to the target
            Replace = 1, // replace target with source
            Activate = 2, // Activate the target GameObject
            Enable = 3, // Enable a component
            Animate = 4, // Start animation on target
            Deactivate = 5 // Decativate target GameObject
        }

       
        public Mode action = Mode.Activate; // The action to accomplish

       
        public Object target; // The game object to affect. If none, the trigger work on this game object
        public GameObject source;
        public int triggerCount = 1;
        public bool repeatTrigger = false;

        private void DoActivateTrigger()
        {
            triggerCount--;

            if (triggerCount == 0 || repeatTrigger)
            {
                Object currentTarget = target ?? gameObject;
                Behaviour targetBehaviour = currentTarget as Behaviour;
                GameObject targetGameObject = currentTarget as GameObject;
                if (targetBehaviour != null)
                    targetGameObject = targetBehaviour.gameObject;

                switch (action)
                {
                    case Mode.Trigger:
                        if (targetGameObject != null) targetGameObject.BroadcastMessage("DoActivateTrigger");
                        break;
                    case Mode.Replace:
                        if (source != null)
                        {
                            if (targetGameObject != null)
                            {
                                Instantiate(source, targetGameObject.transform.position,
                                            targetGameObject.transform.rotation);
                                DestroyObject(targetGameObject);
                            }
                        }
                        break;
                    case Mode.Activate:
                        if (targetGameObject != null) targetGameObject.SetActive(true);
                        break;
                    case Mode.Enable:
                        if (targetBehaviour != null)
                            targetBehaviour.enabled = true;
                        break;
                    case Mode.Animate:
                        if (targetGameObject != null) targetGameObject.GetComponent<Animation>().Play();
                        break;
                    case Mode.Deactivate:
                        if (targetGameObject != null) targetGameObject.SetActive(false);
                        break;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            DoActivateTrigger();
        }
    }
}
