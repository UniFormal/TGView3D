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

public class LightControl : MonoBehaviour {
    private float counterFlash = 0;
    private float counterMove = 0;
    private Light lightObject;
    public float r, g, b;

    public bool flashEnabled;
    public bool moveEnabled;

    public void SetRed(float x)
    {
        r = x;
    }
    public void SetGreen(float x)
    {
        g = x;
    }
    public void SetBlue(float x)
    {
        b = x;
    }
    public void SetFlashEnabled(bool x)
    {
        flashEnabled = x;
    }
    public void SetMoveEnabled(bool x)
    {
        moveEnabled = x;
    }


	// Use this for initialization
	void Start () {
        lightObject = GetComponent<Light>();

        r = g = b = 1;
	}
	
	// Update is called once per frame
	void Update () {
        lightObject.color = new Color(r, g, b);

        if (flashEnabled)
        {
            counterFlash += Time.deltaTime;
            lightObject.intensity = (Mathf.Sin(counterFlash * 20f) + 1f) * 3;
        }
        if (moveEnabled)
        {
            counterMove += Time.deltaTime;
            lightObject.transform.localRotation = Quaternion.AngleAxis(counterMove * 360, Vector3.forward) * Quaternion.AngleAxis(15, Vector3.left);
                
        }
	}
}
