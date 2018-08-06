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

//#define IMAGE_EFFECT
//#define DRAW_QUAD
#define COMMAND_BUFFER

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

// copies mono image to left or right camera before other rendering

public class DisplayMono : MonoBehaviour {

    public MonoCamera monoCam;
    public Material displayMaterial;
    public float translate;
    CommandBuffer commandBuf;

    // Use this for initialization
    void Start () {
    }

    // build command buffer to draw mono image before opaque rendering
    void BuildCommandBuffer()
    {
        CommandBuffer commandBuf = new CommandBuffer();
        commandBuf.name = "Mono rendering";
        commandBuf.Blit((Texture)monoCam.renderTex, BuiltinRenderTextureType.CameraTarget, displayMaterial);

        Camera cam = GetComponent<Camera>();
        cam.RemoveAllCommandBuffers();
        if ((cam.renderingPath == RenderingPath.DeferredShading)
            // || (cam.renderingPath == RenderingPath.UsePlayerSettings && UnityEditor.PlayerSettings.renderingPath == RenderingPath.DeferredShading)
            )
        {
            // deferred
            cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, commandBuf);
        }
        else
        {
            // forward
            cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuf);
        }
    }

    void OnEnable()
    {
        /*
        if (commandBuf == null)
            BuildCommandBuffer();
            */
    }

    void OnDisable()
    {
        GetComponent<Camera>().RemoveAllCommandBuffers();
        commandBuf = null;
    }

    public void SetTranslate(float t)
    {
        translate = t;
        displayMaterial.SetFloat("_Translate", translate);
        //BuildCommandBuffer();
    }

    // Update is called once per frame
    void Update () {
#if COMMAND_BUFFER
        if ((commandBuf == null) && monoCam.renderTex)
        {
            BuildCommandBuffer();
        }
#endif
    }

#if DRAW_QUAD
    // draw full screen quad, at depth 'z' in clip space
    void DrawFullScreenQuad(float z = 0.0f)
    {
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.LoadProjectionMatrix(Matrix4x4.identity);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0); GL.Vertex3(-1, -1, z);
        GL.TexCoord2(0, 1); GL.Vertex3(-1, 1, z);
        GL.TexCoord2(1, 1); GL.Vertex3(1, 1, z);
        GL.TexCoord2(1, 0); GL.Vertex3(1, -1, z);
        GL.End();

        GL.PopMatrix();
    }

    // render mono image at far plane
    //void OnPreRender()
    void OnRenderObject()
    {
        //Debug.Log("frame : " + Time.frameCount);
        displayMaterial.SetPass(0);
        displayMaterial.SetTexture("_MainTex", monoCam.renderTex);
        DrawFullScreenQuad(0.999999f);
    }
#endif

#if IMAGE_EFFECT

    // composite stereo on top of mono image
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        displayMaterial.SetTexture("_MonoTex", monoCam.renderTex);
        Graphics.Blit(src, dest, displayMaterial);
    }
#endif

}
