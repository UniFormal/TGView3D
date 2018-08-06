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

public enum EUiDisplayType
{
	EUDT_WorldGeoQuad,
	EUDT_OverlayQuad,
	EUDT_WorldGeoCylinder,
	EUDT_OverlayCylinder,
}

/// <summary>
/// Usage: demonstrate how to use overlay layers for a paneled UI system
/// On Mobile, we support both Cylinder layer and Quad layer
/// Press any button: it will cycle  [world geometry Quad]->[overlay layer Quad]->[world geometry cylinder]->[overlay layer cylinder]
/// On PC, only Quad layer is supported
/// Press any button: it will cycle  [world geometry Quad]->[overlay layer Quad]
/// 
/// You should be able to observe sharper and less aliased image when switch from world geometry to overlay layer.
/// 
/// </summary>
public class OverlayUIDemo : MonoBehaviour {

	// the camera is used to render UI panels
	public GameObject uiCamera;

	// the world stereo camera
	public GameObject mainCamera;

	// the parent of grouped UI panels
	public GameObject UIGeoParent;

	// white dot cursor
	public GameObject cursor;

	// display current UI display types
	public GameObject informationText;

	// highlight background panel
	public GameObject backgroundHighlight;

	// world geometries are used for comparison between layers and geo
	public GameObject worldGeoProxy_Quad;
	public GameObject worldGeoProxy_Cylinder;

	// Indicate current ui display types
	private EUiDisplayType desiredUiType = EUiDisplayType.EUDT_OverlayQuad;
	private OVROverlay overOverlay;
	private Vector3 startPos;
	private Transform cylinderSpaceTransform;

	/// <summary>
	/// Usage: Recreate UI render target according overlay type and overlay size
	/// </summary>

	void CameraAndRenderTargetSetup()
	{
		float overlayWidth = transform.localScale.x;
		float overlayHeight = transform.localScale.y;
		float overlayRadius = transform.localScale.z;

#if UNITY_ANDROID
		// Gear VR display panel resolution
		float hmdPanelResWidth = 2560;
		float hmdPanelResHeight = 1440;
#else
		// Rift display panel resolution
		float hmdPanelResWidth = 2160;
		float hmdPanelResHeight = 1200;
#endif

		float singleEyeScreenPhysicalResX = hmdPanelResWidth * 0.5f;
		float singleEyeScreenPhysicalResY = hmdPanelResHeight;

		// Calculate RT Height     
		// screenSizeYInWorld : how much world unity the full screen can cover at overlayQuad's location vertically
		// pixelDensityY: pixels / world unit ( meter )

		float halfFovY = mainCamera.GetComponent<Camera>().fieldOfView / 2;
		float screenSizeYInWorld = 2 * overlayRadius * Mathf.Tan(Mathf.Deg2Rad * halfFovY);
		float pixelDensityYPerWorldUnit = singleEyeScreenPhysicalResY / screenSizeYInWorld;
		float renderTargetHeight = pixelDensityYPerWorldUnit * overlayWidth;

		// Calculate RT width
		float renderTargetWidth = 0.0f;
		if (desiredUiType == EUiDisplayType.EUDT_WorldGeoCylinder || desiredUiType == EUiDisplayType.EUDT_OverlayCylinder)
		{
			// For cylinder the resolution can be distributed uniformly along the angle.
			// So we use the angle coverage to calculate the required resolution
			float pixelDensityXPerRadian = singleEyeScreenPhysicalResX / (mainCamera.GetComponent<Camera>().fieldOfView * mainCamera.GetComponent<Camera>().aspect * Mathf.Deg2Rad);
			float pixelDensityXPerWorldUnit = pixelDensityXPerRadian / overlayRadius;
			renderTargetWidth = pixelDensityXPerWorldUnit * overlayWidth;
		}
		else
		{
			// screenSizeXInWorld : how much world unity the full screen can cover at overlayQuad's location horizontally
			// pixelDensityY: pixels / world unit ( meter )

			float screenSizeXInWorld = screenSizeYInWorld * mainCamera.GetComponent<Camera>().aspect;
			float pixelDensityXPerWorldUnit = singleEyeScreenPhysicalResX / screenSizeXInWorld;
			renderTargetWidth = pixelDensityXPerWorldUnit * overlayWidth;
		}

		// Compute the orthographic size for the camera
		float orthographicSize = overlayHeight / 2.0f;
		float orthoCameraAspect = overlayWidth / overlayHeight;
		uiCamera.GetComponent<Camera>().orthographicSize = orthographicSize;
		uiCamera.GetComponent<Camera>().aspect = orthoCameraAspect;

		RenderTexture overlayRT = new RenderTexture(
				(int)renderTargetWidth,
				(int)renderTargetHeight,
				0,
				RenderTextureFormat.ARGB32,
				RenderTextureReadWrite.sRGB);

		overlayRT.hideFlags = HideFlags.DontSave;
		overlayRT.useMipMap = true;
		overlayRT.filterMode = FilterMode.Trilinear;
		overlayRT.anisoLevel = 4;
#if UNITY_5_5_OR_NEWER
		overlayRT.autoGenerateMips = true;
#else
		overlayRT.generateMips = true;
#endif
		uiCamera.GetComponent<Camera>().targetTexture = overlayRT;

		worldGeoProxy_Cylinder.GetComponent<Renderer>().material.SetTexture("_MainTex", overlayRT);
		worldGeoProxy_Quad.GetComponent<Renderer>().material.SetTexture("_MainTex", overlayRT);
		overOverlay.textures[0] = overlayRT;
	}


	// Use this for initialization
	void Start () {

		cylinderSpaceTransform = new GameObject().transform;
		overOverlay = GetComponent<OVROverlay>();
		startPos = transform.position;

		// Start with Overlay Quad
		desiredUiType = EUiDisplayType.EUDT_OverlayQuad;
		CameraAndRenderTargetSetup();
		overOverlay.enabled = true;
		overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
		worldGeoProxy_Cylinder.SetActive(false);
		worldGeoProxy_Quad.SetActive(false);

		// Keep the cursor on the top by enabling last
		cursor.SetActive(true);
	}



	// Convert ray tracing from cylinder wrapping space to Plane space 

	private static bool CylinderRayTransfer(Vector3 startPosWorld, Vector3 rayDirWorld, Transform cylinderTransform, float radius, ref Vector3 transferedDirWorld, ref Vector3 hitPosition)
	{

		// Reject outside dot here
		if (startPosWorld.magnitude >= radius)
			return false;

		// Step 1: transfer ray to cylinder space
		Vector3 startPosLocal = cylinderTransform.InverseTransformPoint(startPosWorld);
		Vector3 rayDirLocal = cylinderTransform.InverseTransformDirection(rayDirWorld);

		// Step 2: intersect ray with cylinder
		//  1) X^2 + Z^2 = radius^2
		//  2) startPosLocal.x + rayDirLocal.x * rayLength = X
		//  3) startPosLocal.z + rayDirLocal.z * rayLength = Z
		//
		// Then
		//  (startPosLocal.x + rayDirLocal.x * rayLength)^2 + (startPosLocal.z + rayDirLocal.z * rayLength)^2 = radius^2;
		//  rayLength^2 * ( rayDirLocal.x ^ 2 +  rayDirLocal.z ^ 2 ) + rayLength * ( 2 * startPosLocal.x * rayDirLocal.x + 2 * startPosLocal.z * rayDirLocal.z ) + ( rayDirLocal.x ^ 2 +  rayDirLocal.z ^ 2 - radius ^ 2 ) = 0 
		//  a =  ( rayDirLocal.x ^ 2 +  rayDirLocal.z ^ 2 ) 
		//  b =  ( 2 * startPosLocal.x * rayDirLocal.x + 2 * startPosLocal.z * rayDirLocal.z )
		//  c = ( startPosLocal.x ^ 2 +  startPosLocal.z ^ 2 - radius ^ 2 )

		float a = (rayDirLocal.x * rayDirLocal.x + rayDirLocal.z * rayDirLocal.z);
		float b = (2 * startPosLocal.x * rayDirLocal.x) + (2 * startPosLocal.z * rayDirLocal.z);
		float c = (startPosLocal.x * startPosLocal.x + startPosLocal.z * startPosLocal.z - radius * radius);

		// Solve quadratic and find cylinder intersection point
		float rayLength = (-b + Mathf.Sqrt((b * b) - (4 * a * c))) / (2 * a);
		Vector3 cylinderIntersect = startPosLocal + (rayDirLocal * rayLength);

		// Step 3: mapping cylinderIntersect to QuadIntersect
		float intersectArcLength = radius * Mathf.Atan2(cylinderIntersect.x, cylinderIntersect.z);
		Vector3 planeIntersect;
		planeIntersect.x = intersectArcLength;
		planeIntersect.y = cylinderIntersect.y;
		planeIntersect.z = radius;

		// Step 4: get the transformed ray and convert it back to world space
		Vector3 planeIntersectWorld = cylinderTransform.TransformPoint(planeIntersect);
		transferedDirWorld = planeIntersectWorld - startPosWorld;
		transferedDirWorld.Normalize();

		hitPosition = cylinderTransform.TransformPoint(cylinderIntersect);
		return true;
	}

	bool wasMenuActive = false;
	void OverlaySwitch()
	{
		float cylinderRadius = transform.localScale.z;
		bool isMenuActive = OVRInspector.instance.IsMenuActive();
		bool triggerDisplay = wasMenuActive && !isMenuActive;
		wasMenuActive = OVRInspector.instance.IsMenuActive();

		// Hide the cursor by set the size to 0, we don't want to disable it since it will cause overlay layers reordering
		// Want to keep it topmost.
		cursor.transform.localScale = isMenuActive ? new Vector3(0, 0, 0) : new Vector3(0.3f, 0.3f, 0.3f);

		if (isMenuActive)
		{
			worldGeoProxy_Quad.SetActive(false);
			worldGeoProxy_Cylinder.SetActive(false);
			overOverlay.enabled = false;
			return;
		}

		// Switch between World Geo, Plane Overlay and Cylinder Overlay
		if (triggerDisplay || Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )
		{
#if UNITY_ANDROID
			if ( desiredUiType == EUiDisplayType.EUDT_WorldGeoQuad)
			{
				// Switch to EUDT_OverlayQuad
				desiredUiType = EUiDisplayType.EUDT_OverlayQuad;
				informationText.GetComponent<TextMesh>().text = "Overlay Layer Quad, Press any key to World Geo Cylinder";

				overOverlay.enabled = true;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
				worldGeoProxy_Quad.SetActive(false);
				worldGeoProxy_Cylinder.SetActive(false);
			}
			else if (desiredUiType == EUiDisplayType.EUDT_OverlayQuad)
			{
				// Switch to EUDT_WorldGeoCylinder
				desiredUiType = EUiDisplayType.EUDT_WorldGeoCylinder;
				informationText.GetComponent<TextMesh>().text = "World Geo Cylinder, Press any key to Overlay Layer Cylinder";

				// Resize renderTarget
				CameraAndRenderTargetSetup();
				overOverlay.enabled = false;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Cylinder;
				worldGeoProxy_Quad.SetActive(false);
				worldGeoProxy_Cylinder.SetActive(true);
			}
			else if (desiredUiType == EUiDisplayType.EUDT_WorldGeoCylinder)
			{
				// Switch to EUDT_OverlayCylinder
				desiredUiType = EUiDisplayType.EUDT_OverlayCylinder;
				informationText.GetComponent<TextMesh>().text = "Overlay Layer Cylinder, Press any key to World Geo Quad";

				overOverlay.enabled = true;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Cylinder;
				worldGeoProxy_Quad.SetActive(false);
				worldGeoProxy_Cylinder.SetActive(false);
			}
			else if (desiredUiType == EUiDisplayType.EUDT_OverlayCylinder)
			{
				// Switch to EUDT_WorldGeoQuad
				desiredUiType = EUiDisplayType.EUDT_WorldGeoQuad;
				informationText.GetComponent<TextMesh>().text = "World Geo Quad, Press any key to Overlay Layer Quad";

				// Resize renderTarget
				CameraAndRenderTargetSetup();

				overOverlay.enabled = false;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
				worldGeoProxy_Quad.SetActive(true);
				worldGeoProxy_Cylinder.SetActive(false);
			}
#else
			if ( desiredUiType == EUiDisplayType.EUDT_WorldGeoQuad)
			{
				// Switch to EUDT_OverlayQuad
				desiredUiType = EUiDisplayType.EUDT_OverlayQuad;
				informationText.GetComponent<TextMesh>().text = "Overlay Layer Quad, Press any key to World Geo Quad";
				overOverlay.enabled = true;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
				worldGeoProxy_Quad.SetActive(false);
				worldGeoProxy_Cylinder.SetActive(false);
			}
			else if (desiredUiType == EUiDisplayType.EUDT_OverlayQuad)
			{
				// Switch to EUDT_WorldGeoQuad
				desiredUiType = EUiDisplayType.EUDT_WorldGeoQuad;
				informationText.GetComponent<TextMesh>().text = "World Geo Quad, Press any key to Overlay Layer Quad";
				overOverlay.enabled = false;
				overOverlay.currentOverlayShape = OVROverlay.OverlayShape.Quad;
				worldGeoProxy_Quad.SetActive(true);
				worldGeoProxy_Cylinder.SetActive(false);
			}
#endif
		}

		// Cylinder's original is at cylinder center, Quad overlay is at Quad center
		// So offset cylinder with cylinderRadius for making cylinder inscribing the Quad 
		if (desiredUiType == EUiDisplayType.EUDT_WorldGeoCylinder || desiredUiType == EUiDisplayType.EUDT_OverlayCylinder)
			transform.position = startPos - Vector3.forward * (cylinderRadius);
		else
			transform.position = startPos;
	}

	// Update is called once per frame
	void Update () {

		// Switch ui display types 
		OverlaySwitch();

		// UI trace
		RaycastHit hitInfo;
		int uiLayerMask = LayerMask.GetMask("DemoUI");

		Vector3 rayTracingDirection = mainCamera.transform.forward;
		OVROverlay currentOverlay = GetComponent<OVROverlay>();
		Vector3 cylinderIntersectPos = mainCamera.transform.position + mainCamera.transform.forward * 200;

		// Need transfer ray back to UI camera space for correct ray casting
		if ( desiredUiType == EUiDisplayType.EUDT_OverlayCylinder || desiredUiType == EUiDisplayType.EUDT_WorldGeoCylinder )
		{
			cylinderSpaceTransform.position = currentOverlay.transform.position;
			cylinderSpaceTransform.rotation = currentOverlay.transform.rotation;
			cylinderSpaceTransform.localScale = Vector3.one;

			float cylinderRadius = currentOverlay.transform.localScale.z;
			CylinderRayTransfer(mainCamera.transform.position, mainCamera.transform.forward, cylinderSpaceTransform, cylinderRadius, ref rayTracingDirection, ref cylinderIntersectPos);

		}

		Physics.Raycast(mainCamera.transform.position, rayTracingDirection, out hitInfo, 500, uiLayerMask);

		// Highlight the selected tile
		if (hitInfo.collider != null)
		{
			Vector3 highLightEdge = new Vector3(0.1f, 0.1f, 0.00f);
			backgroundHighlight.SetActive(true);
			backgroundHighlight.transform.position = hitInfo.collider.gameObject.transform.position;
			backgroundHighlight.transform.localScale = hitInfo.collider.gameObject.transform.localScale + highLightEdge;
		}
		else
		{
			backgroundHighlight.SetActive(false);
		}

		if ( currentOverlay.currentOverlayShape == OVROverlay.OverlayShape.Cylinder)
		{
			cursor.transform.position = cylinderIntersectPos;
		}
		else
		{
			Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hitInfo, 500);

			if (hitInfo.collider != null)
				cursor.transform.position = hitInfo.point;
			else
				cursor.transform.position = mainCamera.transform.forward * 26 + mainCamera.transform.position;
		}

		cursor.transform.LookAt(mainCamera.transform.position);
	}
}

