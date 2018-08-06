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
using System.Collections.Generic;

public class TeleportController : MonoBehaviour {

    public float maxTeleportRange;
    public OVRInput.Button teleportButton;
    public KeyCode teleportKey;
    public Transform pointerTransform; // Could be a tracked controller
    public bool allowRotation;
    public bool allowForRealHeadRotation;
    public float realHeadMovementCompensation;
    public float rotationSpeed = 1;

    public float fadeSpeed = 1f;
    public float fadeLength = 0.5f;

    public float rotateStickThreshold = 0.5f;

    [HideInInspector()]
    public bool teleportEnabled = true;

    public GameObject positionIndicatorPrefab;

    

    public LayerMask teleportLayerMask;


    private GameObject positionIndicator;
    private TeleportPoint currentTeleportPoint;
    private float rotationAmount;
    private Quaternion initialRotation;
    private bool teleporting = false;

	
	// Update is called once per frame
	void Update () {

        RaycastHit hit;
        if (positionIndicator)
        {
            if (allowRotation)
            {
                float leftStickRotation = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
                if (Mathf.Abs(leftStickRotation) > rotateStickThreshold)
                {
                    rotationAmount += Time.deltaTime * leftStickRotation * rotationSpeed;
                    positionIndicator.transform.rotation = Quaternion.Euler(new Vector3(0, rotationAmount, 0)) * initialRotation;
                }
            }

            if (OVRInput.GetUp(teleportButton) || Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))
            {
                DoTeleport(positionIndicator.transform);
                

            }
        }
        else if (Physics.Raycast(pointerTransform.position, pointerTransform.forward, out hit, maxTeleportRange, teleportLayerMask))
        {
            TeleportPoint tp = hit.collider.gameObject.GetComponent<TeleportPoint>();
            tp.OnLookAt();

            if (teleportEnabled && !teleporting && (OVRInput.GetDown(teleportButton) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
            {
                StartTeleport(tp);
            }
            
        }
        

	}

    void StartTeleport(TeleportPoint tp)
    {
        teleporting = true;
        
        if (currentTeleportPoint)
        {
            currentTeleportPoint.GetComponent<MeshRenderer>().enabled = true;
        }
        currentTeleportPoint = tp;
        currentTeleportPoint.GetComponent<MeshRenderer>().enabled = false;
        
        positionIndicator = GameObject.Instantiate<GameObject>(positionIndicatorPrefab);
        positionIndicator.transform.position = tp.GetDestTransform().position;
        initialRotation = positionIndicator.transform.rotation = tp.GetDestTransform().rotation;
            
        rotationAmount = 0;
    }

    void DoTeleport(Transform destTransform)
    {
        StartCoroutine(TeleportCoroutine(destTransform));
    }


    IEnumerator TeleportCoroutine(Transform destTransform)
    {
        Vector3 destPosition = destTransform.position;
        Quaternion destRotation = destTransform.rotation;

        float fadeLevel = 0;

        while (fadeLevel < 1)
        {
            yield return null;
            fadeLevel += fadeSpeed * Time.deltaTime;
            fadeLevel = Mathf.Clamp01(fadeLevel);
            OVRInspector.instance.fader.SetFadeLevel(fadeLevel);
        }


        transform.position = destPosition;
        GameObject.DestroyObject(positionIndicator);
        positionIndicator = null;

       
        if (allowForRealHeadRotation)
        {
#if UNITY_2017_2_OR_NEWER
            Quaternion headRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
#else
            Quaternion headRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head);
#endif
            Vector3 euler = headRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            headRotation = Quaternion.Euler(euler);
            transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(headRotation), realHeadMovementCompensation) * destRotation;
        }
        else
        {
            transform.rotation = destRotation;
        }

        yield return new WaitForSeconds(fadeLength);

        teleporting = false;

        while (fadeLevel > 0)
        {
            yield return null;
            fadeLevel -= fadeSpeed * Time.deltaTime;
            fadeLevel = Mathf.Clamp01(fadeLevel);
            OVRInspector.instance.fader.SetFadeLevel(fadeLevel);
        }



        yield return null;
    }
}
