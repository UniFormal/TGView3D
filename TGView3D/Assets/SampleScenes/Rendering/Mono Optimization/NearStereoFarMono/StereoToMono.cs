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
using VR = UnityEngine.VR;

public class StereoToMono : MonoBehaviour {

    public Camera monoCamera;
    public Camera leftCamera;
    public Camera rightCamera;

    public Material monoDisplayLMaterial;
    public Material monoDisplayRMaterial;

    public float monoDistance = 5.0f;
    public float overlap = 0.2f;

    public bool enableMono = false;
    public bool showMono = true;
    public bool monoAtInfinity = false;

	// Use this for initialization
	void Start () 
    {
        // disable occlusion mesh optimization for now
        OVRPlugin.occlusionMesh = false;

        SetShowMono(showMono);
    }

    public void SetEnableMono(bool b)
    {
        enableMono = b;
    }

    public void SetShowMono(bool b)
    {
        showMono = b;
        if (monoDisplayLMaterial)
        {
            monoDisplayLMaterial.SetColor("_Color", showMono ? new Color(1.0f, 0.5f, 0.5f) : Color.white);
        }

        if (monoDisplayRMaterial)
        {
            monoDisplayRMaterial.SetColor("_Color", showMono ? new Color(1.0f, 0.5f, 0.5f) : Color.white);
        }
    }

    public void SetDistance(float x)
    {
        monoDistance = x;
    }

	// Update is called once per frame
	void Update () {
        // set near/far planes
        if (monoCamera)
        {
            if (enableMono)
            {
                monoCamera.nearClipPlane = monoDistance - overlap;
            }
            else
            {
                monoCamera.nearClipPlane = monoCamera.farClipPlane - 1.0f;
            }
        }

        if (leftCamera && rightCamera)
        {
            if (enableMono)
            {
                monoCamera.enabled = true;
                leftCamera.GetComponent<DisplayMono>().enabled = true;
                rightCamera.GetComponent<DisplayMono>().enabled = true;

                leftCamera.farClipPlane = monoDistance;
                rightCamera.farClipPlane = monoDistance;

                leftCamera.clearFlags = CameraClearFlags.Depth;
                rightCamera.clearFlags = CameraClearFlags.Depth;
            }
            else
            {
                monoCamera.enabled = false;
                leftCamera.GetComponent<DisplayMono>().enabled = false;
                rightCamera.GetComponent<DisplayMono>().enabled = false;

                leftCamera.farClipPlane = monoCamera.farClipPlane;
                rightCamera.farClipPlane = monoCamera.farClipPlane;

                leftCamera.clearFlags = monoCamera.clearFlags;
                rightCamera.clearFlags = monoCamera.clearFlags;
            }

            // calculate correct translation to make mono image appear at switching distance
#if UNITY_2017_2_OR_NEWER
            Vector2 fov = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye).fov;
#else
            Vector2 fov = OVRManager.display.GetEyeRenderDesc(VR.VRNode.LeftEye).fov;
#endif
            float translate = OVRManager.profile.ipd / (Mathf.Tan(fov.x * 0.5f * Mathf.Deg2Rad) * 2.0f * monoDistance);
            if (monoAtInfinity) translate = 0;

            leftCamera.GetComponent<DisplayMono>().SetTranslate(-translate * 0.5f);
            rightCamera.GetComponent<DisplayMono>().SetTranslate(translate * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            OVRManager.display.RecenterPose();
            monoDistance = 5.0f;
        }

		if (Input.GetKeyDown(KeyCode.M) || OVRInput.GetDown(OVRInput.Button.One) && !OVRInspector.instance.IsMenuActive())
        {
            enableMono = !enableMono;
			SetEnableMono(enableMono);
        }

		if (Input.GetKeyDown(KeyCode.S) || OVRInput.GetDown(OVRInput.Button.Two) && !OVRInspector.instance.IsMenuActive())
        {
            showMono = !showMono;
            SetShowMono(showMono);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            monoAtInfinity = !monoAtInfinity;
        }

		if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus) || OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp) && !OVRInspector.instance.IsMenuActive())
        {
            monoDistance += 0.1f;
        }
		if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) || OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown) && !OVRInspector.instance.IsMenuActive())
        {
            monoDistance -= 0.1f;
            if (monoDistance < 1.0f) monoDistance = 1.0f;
			SetDistance(monoDistance);
        }
	}
}
