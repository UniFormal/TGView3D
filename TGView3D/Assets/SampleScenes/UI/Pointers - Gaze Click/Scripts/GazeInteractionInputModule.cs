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
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    public class GazeInteractionInputModule : OVRInputModule
    {
        private GameObject currentlyGazedObject;
        private float lastTimeGazedObjectChanged;
        [Tooltip("How long to wait until a gazed control is activated")]
        public float activationDwellTime = 1.0f;
        private bool isDragging;

        override public void Process()
        {
            base.Process();

            // Handle selection
            if (!currentlyGazedObject || lastTimeGazedObjectChanged > Time.time)
            {
                OVRGazePointer.instance.SelectionProgress = 0f;
            }
            else
            {
                float currentDwellTime = Time.time - lastTimeGazedObjectChanged;
                OVRGazePointer.instance.SelectionProgress = currentDwellTime / activationDwellTime;
            }
        }

        /// <summary>
        /// overwritten from the base version so we can use our custom 
        /// </summary>
        /// <returns></returns>
        override protected MouseState GetGazePointerData()
        {
            MouseState state = base.GetGazePointerData();

            var raycast = state.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData.pointerCurrentRaycast;
            
            //get custom press state
            PointerEventData.FramePressState pressState = GetGazeButtonState(raycast.gameObject);
            //set it
            state.SetButtonState(PointerEventData.InputButton.Left, pressState, state.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData);

            return state;
        }

        /// <summary>
        /// Modfied version of the base class that takes into account when which object got hit
        /// </summary>
        /// <returns></returns>
        protected PointerEventData.FramePressState GetGazeButtonState(GameObject rayCastHit)
        {
            var pressed = Input.GetKeyDown(gazeClickKey) || OVRInput.GetDown(joyPadClickButton);
            var released = Input.GetKeyUp(gazeClickKey) || OVRInput.GetUp(joyPadClickButton);

            bool shouldImmediatelyRelease;
            GameObject newGazedObject = GetCurrentlyGazedGameObject(rayCastHit, out shouldImmediatelyRelease);
            if (currentlyGazedObject != newGazedObject)
            {
                released |= true;
                currentlyGazedObject = newGazedObject;
                lastTimeGazedObjectChanged = Time.time;
            }

            float currentDwellTime = Time.time - lastTimeGazedObjectChanged;
            if (currentlyGazedObject && currentDwellTime >= activationDwellTime)
            {
                pressed |= true;
                if (shouldImmediatelyRelease)
                {
                    //reset the time so this doesn't get activated again
                    lastTimeGazedObjectChanged = float.MaxValue;
                    released |= true; //simulate click
                }
            }

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        private GameObject GetCurrentlyGazedGameObject(GameObject go, out bool shouldImmediatelyRelease)
        {
            shouldImmediatelyRelease = true;
            if (!go)
            {
                return null;
            }

            Slider slider = go.GetComponentInParent<Slider>();
            if (slider) 
            {
                shouldImmediatelyRelease = false;
                return slider.gameObject;
            }
            Button button = go.GetComponentInParent<Button>();
            if (button)
            {
                return button.gameObject;
            }
            Toggle toggle = go.GetComponentInParent<Toggle>();
            if (toggle)
            {
                return toggle.gameObject;
            }

            //special thing to make everything actionable with gaze controls
            GazeInteractionReceiver gazeInteractionReceiver = go.GetComponentInParent<GazeInteractionReceiver>();
            if (gazeInteractionReceiver)
            {
                return gazeInteractionReceiver.gameObject;
            }

            return null;
        }

        public void SetActivationDwellTime(float v)
        {
            activationDwellTime = v;
        }
    }
}
