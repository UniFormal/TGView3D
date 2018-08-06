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

namespace UnitySampleAssets.CrossPlatformInput.PlatformSpecific
{
    public class MobileInput : VirtualInput
    {
        private void AddButton(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            CrossPlatformInputManager.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
        }


        private void AddAxes(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis(name));
        }


        public override float GetAxis(string name, bool raw)
        {
            return virtualAxes.ContainsKey(name) ? virtualAxes[name].GetValue : 0;
        }


        public override void SetButtonDown(string name)
        {
            if (!virtualButtons.ContainsKey(name))
            {
                AddButton(name);
            }
            virtualButtons[name].Pressed();
        }


        public override void SetButtonUp(string name)
        {
            virtualButtons[name].Released();
        }


        public override void SetAxisPositive(string name)
        {
            if (!virtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            virtualAxes[name].Update(1f);
        }


        public override void SetAxisNegative(string name)
        {
            if (!virtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            virtualAxes[name].Update(-1f);
        }


        public override void SetAxisZero(string name)
        {
            if (!virtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            virtualAxes[name].Update(0f);
        }


        public override void SetAxis(string name, float value)
        {
            if (!virtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            virtualAxes[name].Update(value);
        }


        public override bool GetButtonDown(string name)
        {
            if (virtualButtons.ContainsKey(name))
            {
                return virtualButtons[name].GetButtonDown;
            }

            AddButton(name);
            return virtualButtons[name].GetButtonDown;
        }


        public override bool GetButtonUp(string name)
        {
            if (virtualButtons.ContainsKey(name))
            {
                return virtualButtons[name].GetButtonUp;
            }

            AddButton(name);
            return virtualButtons[name].GetButtonUp;
        }


        public override bool GetButton(string name)
        {
            if (virtualButtons.ContainsKey(name))
            {
                return virtualButtons[name].GetButton;
            }

            AddButton(name);
            return virtualButtons[name].GetButton;
        }


        public override Vector3 MousePosition()
        {
            return virtualMousePosition;
        }
    }
}
