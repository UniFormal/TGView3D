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

// creates render texture for mono camera, copies other camera parameters from source camera

public class MonoCamera : MonoBehaviour {

    public Camera srcCam;
    public RenderTextureFormat textureFormat = RenderTextureFormat.Default;
    public int depthBits = 24;
    public int antiAliasingSamples = 1;
    public bool createDepthTexture = false;
    public RenderTexture renderTex;

	// Use this for initialization
	void Start () {
        if (srcCam == null)
        {
            Debug.Log("No src camera!");
            return;
        }
#if UNITY_2017_2_OR_NEWER
        OVRDisplay.EyeRenderDesc eyeDesc = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye);
#else
        OVRDisplay.EyeRenderDesc eyeDesc = OVRManager.display.GetEyeRenderDesc(UnityEngine.VR.VRNode.LeftEye);
#endif

        renderTex = new RenderTexture((int)eyeDesc.resolution.x, (int)eyeDesc.resolution.y, depthBits, textureFormat, RenderTextureReadWrite.Linear);
        renderTex.antiAliasing = antiAliasingSamples;

        Camera cam = (Camera)GetComponent<Camera>();
        //cam.targetTexture = renderTex;
        cam.depth = -1;  // ensure this camera is rendered before others

        // copy other parameters from source camera
        cam.nearClipPlane = srcCam.nearClipPlane;
        cam.farClipPlane = srcCam.farClipPlane;

        cam.fieldOfView = eyeDesc.fov.y;
        cam.aspect = eyeDesc.resolution.x / eyeDesc.resolution.y;

        if (createDepthTexture)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }
    }

    // Update is called once per frame
    void Update() {
        // offset left camera to center eye
#if UNITY_2017_2_OR_NEWER
        Vector3 offset = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye) - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye);
#else
        Vector3 offset = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye) - UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.LeftEye);
#endif
        //Debug.Log("Offset = " + offset.ToString("F4"));
        if (transform.parent)
        {
            transform.parent.localPosition = offset;
        }
        else
        {
            Debug.Log("Mono camera must be parented to empty game object!");
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // copy from eye texture to our render texture
        Graphics.Blit(src, renderTex);
    }

}
