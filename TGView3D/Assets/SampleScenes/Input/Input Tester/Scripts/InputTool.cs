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
using System;
using System.Linq;

public class InputTool : MonoBehaviour {
    
    public Text outputText;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        string s = "";
        var buttonValues = Enum.GetValues(typeof(OVRInput.Button)).Cast<OVRInput.Button>();
        bool firstButton = true;
        foreach (OVRInput.Button b in buttonValues)
        {
            if (OVRInput.Get(b))
            {
                if (!firstButton)
                    s += ", ";
                s += b.ToString();
                firstButton = false;
            }
        }
        var axis1DValues = Enum.GetValues(typeof(OVRInput.Axis1D)).Cast<OVRInput.Axis1D>();
        foreach (OVRInput.Axis1D a in axis1DValues)
        {
            s += string.Format("\n{0}:{1:0.0}", a.ToString(), OVRInput.Get(a));
        }
        var axis2DValues = Enum.GetValues(typeof(OVRInput.Axis2D)).Cast<OVRInput.Axis2D>();
        foreach (OVRInput.Axis2D a in axis2DValues)
        {
            var v = OVRInput.Get(a);
            s += string.Format("\n{0}:{1:0.0},{2:0.0}", a.ToString(), v.x, v.y);
        }
      
        outputText.text = s;
	}
}
