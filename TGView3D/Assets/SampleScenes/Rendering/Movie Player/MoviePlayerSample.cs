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
using System.Collections;					// required for Coroutines
using System.Runtime.InteropServices;		// required for DllImport
using System;								// requred for IntPtr
using System.IO;							// required for File

/************************************************************************************
Usage:

	Place a simple textured quad surface with the correct aspect ratio in your scene.

	Add the MoviePlayerSample.cs script to the surface object.

	Supply the name of the media file to play:
	This sample assumes the media file is placed in "Assets/StreamingAssets", ie
	"ProjectName/Assets/StreamingAssets/MovieName.mp4".

	On Desktop, Unity MovieTexture functionality is used. Note: the media file
	is loaded at runtime, and therefore expected to be converted to Ogg Theora
	beforehand.

Implementation:

	In the MoviePlayerSample Awake() call, GetNativeTexturePtr() is called on 
	renderer.material.mainTexture.
	
	When the MediaSurface plugin gets the initialization event on the render thread, 
	it creates a new Android SurfaceTexture and Surface object in preparation 
	for receiving media. 

	When the game wants to start the video playing, it calls the StartVideoPlayerOnTextureId()
	script call, which creates an Android MediaPlayer java object, issues a 
	native plugin call to tell the native code to set up the target texture to
	render the video to and return the Android Surface object to pass to MediaPlayer,
	then sets up the media stream and starts it.
	
	Every frame, the SurfaceTexture object is checked for updates.  If there 
	is one, the target texId is re-created at the correct dimensions and format
	if it is the first frame, then the video image is rendered to it and mipmapped.  
	The following frame, instead of Unity drawing the image that was placed 
	on the surface in the Unity editor, it will draw the current video frame.

************************************************************************************/

public class MoviePlayerSample : MonoBehaviour
{
	public string 	movieName = string.Empty;
    public bool     videoPaused = false;
    private bool    videoPausedBeforeAppPause = false;

    private string	mediaFullPath = string.Empty;
	private bool	startedVideo = false;

	private UnityEngine.Video.VideoPlayer videoPlayer = null;
	private OVROverlay          overlay = null;
	private Renderer 			mediaRenderer = null;

	/// <summary>
	/// Initialization of the movie surface
	/// </summary>
	void Awake()
	{
		Debug.Log("MovieSample Awake");

		mediaRenderer = GetComponent<Renderer>();

		videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
		if (videoPlayer == null)
			videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();

		if (mediaRenderer.material == null || mediaRenderer.material.mainTexture == null)
		{
			Debug.LogError("No material for movie surface");
		}

		if (movieName != string.Empty)
		{
			videoPlayer.url = Application.streamingAssetsPath + "/" + movieName;

			Debug.Log("MovieSample Start");
			videoPlayer.Play();
		}
		else
		{
			Debug.LogError("No media file name provided");
		}
    }

	void Update()
	{
		if (overlay == null)
			overlay = GetComponent<OVROverlay>();
		
		if (overlay == null)
			return;

		overlay.textures[0] = videoPlayer.texture;	
	}

    public void Rewind()
    {
        if (videoPlayer != null)
			videoPlayer.Stop();
    }

    public void SetPaused(bool wasPaused)
    {
        Debug.Log("SetPaused: " + wasPaused);
		if (videoPlayer != null)
        {
            videoPaused = wasPaused;
            if (videoPaused)
				videoPlayer.Pause();
            else
				videoPlayer.Play();
        }
    }

    /// <summary>
    /// Pauses video playback when the app loses or gains focus
    /// </summary>
    void OnApplicationPause(bool appWasPaused)
    {
        Debug.Log("OnApplicationPause: " + appWasPaused);
        if (appWasPaused)
        {
            videoPausedBeforeAppPause = videoPaused;
        }
        
        // Pause/unpause the video only if it had been playing prior to app pause
        if (!videoPausedBeforeAppPause)
        {
            SetPaused(appWasPaused);
        }
    }
}
