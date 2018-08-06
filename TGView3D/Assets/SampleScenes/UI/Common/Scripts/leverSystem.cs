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
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Serialization;
using System;

public class leverSystem : MonoBehaviour {

    bool previousState;
    bool currentState;
    bool moving;
    float movement = -40;
    public float speed;

    [Serializable]
    public class LeverClickedEvent : UnityEvent<bool> { }

    [SerializeField]
    private LeverClickedEvent onClick = new LeverClickedEvent();
	// Use this for initialization
	void Start () {
        transform.localRotation = Quaternion.Euler(movement, 0, 0);
	
	}
	
	// Update is called once per frame
	void Update () {
        if (moving)
        {
            float initialSign = Mathf.Sign(movement);
            movement += Time.deltaTime * (previousState ? -1 : 1) * speed;

            transform.rotation = Quaternion.Euler(new Vector3(movement, 0, 0));
            if ((movement > 40f && !previousState) || (movement < -40 && previousState))
            {
                currentState = !previousState;
                moving = false;
            }
            if (Mathf.Sign(movement) != initialSign)
            {
                onClick.Invoke(!previousState);
            }

            transform.localRotation = Quaternion.Euler(movement, 0, 0);
        }
	
	}

    public void OnPointerClick()
    {
        if (!moving)
        {
            previousState = currentState;
            moving = true;
        }
    }
}
