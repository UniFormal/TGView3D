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
using System;
using System.Collections;
using System.Collections.Generic;

public class CrosshairManager : MonoBehaviour
{

    /// <summary>
    /// Get CenterEyeAnchor game object
    /// </summary>
    GameObject cameraAnchor;

    /// <summary>
    /// Crosshair auto depth
    /// </summary>
    bool autoDepth = true;

    /// <summary>
    /// Turn on/off for Crosshair
    /// </summary>
    bool activeCrosshair = true;

    /// <summary>
    /// Default crosshair scale when not using autoscale
    /// </summary>
    float crosshairDefaultScale = 0.5f;

    /// <summary>
    /// distance from camera to crosshair in metres
    /// </summary>
    float crosshairDepth;

    /// <summary>
    /// crosshair default depth
    /// </summary>
    public float crosshairDefaultDepth;

    /// <summary>
    /// When autoscaling, the angular size in degrees of crosshair
    /// </summary>
    public float crosshairTargetAnglularSize;

    /// <summary>
    /// For autoscaling to work correctly this must be the world size of the crosshair object when its scale is 1
    /// </summary>
    public float unscaledCrosshairDiameter;

    /// <summary>
    /// Slider controller angular size.
    /// </summary>
    public Slider sizeSlider;

    /// <summary>
    /// Is crosshair scaled so it always appears the same angular size
    /// </summary>
    bool autoScale = true;

    /// <summary>
    /// Get crosshair game object for scaling
    /// </summary>
    public GameObject image = null;

    /// <summary>
    /// Start in MonoBehaviour:Initialize crosshair components
    /// </summary>
	void Start () 
    {
        cameraAnchor = GameObject.Find("CenterEyeAnchor");      
        image.SetActive(activeCrosshair);
        crosshairDepth = crosshairDefaultDepth;
	}

    // Enable/Disable crosshair depending on UI activation
    public void OnUIShow()
    {
        activeCrosshair = false;
        image.SetActive(activeCrosshair);
    }

    public void OnUIHide()
    {
        activeCrosshair = true;
        image.SetActive(activeCrosshair);
    }

    /// <summary>
    /// Update in MonoBehaviour
    /// </summary>	
    void Update()
    {     
        float newCrossHairDepth = crosshairDefaultDepth;
        RaycastHit hit;
        if (autoDepth && Physics.Raycast(cameraAnchor.transform.position, cameraAnchor.transform.forward, out hit))
        {
            switch (hit.transform.tag)
            {
                case "wall":
                    newCrossHairDepth = hit.distance;
                    break;
                case "bullet":
                    //in case we hit a bullet, just keep the old distance, as we don't want to flicker between the wall and the bullet distance
                    newCrossHairDepth = crosshairDepth;
                    break;
            }
        }
        crosshairDepth = newCrossHairDepth;
        image.transform.localPosition = new Vector3(0.0f, 0.0f, crosshairDepth);
        
        if (autoScale)
        {
            // Calculate size that would be required in order have the target angular size
            float desiredSize = Mathf.Tan(crosshairTargetAnglularSize * Mathf.Deg2Rad * 0.5f) * 2 * crosshairDepth;
            // Find and set required scale
            float requiredScale = desiredSize / unscaledCrosshairDiameter;
            image.transform.localScale = new Vector3(requiredScale, requiredScale, requiredScale);
        }
        
    }


    /// <summary>
    /// Turn on/off for crosshair auto depth
    /// </summary>
    public void SetAutoDepth(bool on)
    {
        autoDepth = on;

    }

    /// <summary>
    /// Adjusting crosshair depth
    /// </summary>
    public void SetCrosshairDepth(float depth)
    {
        crosshairDefaultDepth = depth;
    }

    /// <summary>
    /// Adjusting crosshair apparent angular size in degrees
    /// </summary>
    public void SetCrosshairApparentSize(float degrees)
    {
        crosshairTargetAnglularSize = degrees;
    }

    /// <summary>
    /// Set whether auto scaling is on
    /// </summary>
    public void SetAutoScale(bool on)
    {
        autoScale = on;
        sizeSlider.interactable = autoScale;
        if (!autoScale)
        {
            image.transform.localScale = new Vector3(crosshairDefaultScale, crosshairDefaultScale, crosshairDefaultScale);
        }
    }

   

}
