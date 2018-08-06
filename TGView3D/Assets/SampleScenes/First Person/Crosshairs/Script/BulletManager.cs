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

public class BulletManager : MonoBehaviour
{
    /// <summary>
    /// Define layer for collision detect 
    /// </summary>
    public LayerMask layerMask;

    /// <summary>
    /// Skin width
    /// </summary>
    float skinWidth = 0.1f;

    /// <summary>
    /// Get minium collider bound
    /// </summary>
    float minimumExtent;

    /// <summary>
    /// Get partial extent for collier
    /// </summary>
    float partialExtent;

    /// <summary>
    /// Sqrt value of minimum extent
    /// </summary>
    float sqrMinimumExtent;
    
    /// <summary>
    /// Previous position of bullet
    /// </summary>
    Vector3 prevPosition;

    /// <summary>
    /// Regidbody of bullet
    /// </summary>
    Rigidbody bulletRigidbody;


    /// <summary>
    /// Awake in MonoBehaviour
    /// </summary>
    void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        minimumExtent = Mathf.Min(Mathf.Min(GetComponent<Collider>().bounds.extents.x, GetComponent<Collider>().bounds.extents.y), GetComponent<Collider>().bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }

    /// <summary>
    /// Start in MonoBehaviour
    /// </summary>
    void Start()
    {
        // Bullet will be destroyed in 3 secs
        Destroy(gameObject, 3.0f);
    }

    /// <summary>
    /// Fixed in MonoBehaviour
    /// </summary>
    void FixedUpdate()
    {
		//Initialize prevPosition here because RigidBody.position may not be valid in Awake.
		if (prevPosition.magnitude < 1e-3f)
			prevPosition = bulletRigidbody.position;
		
        //have we moved more than our minimum extent
        Vector3 movementThisStep = bulletRigidbody.position - prevPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);

            RaycastHit hit;

            //check for walls we might have missed 
            if (Physics.Raycast(prevPosition, movementThisStep, out hit, movementMagnitude, layerMask.value))
            {
                bulletRigidbody.position = hit.point - (movementThisStep / movementMagnitude) * partialExtent;
            }
        }     

        prevPosition = bulletRigidbody.position;
    }

    /// <summary>
    /// Collision detection for destroying bullets
    /// </summary>
    void OnCollisionEnter(Collision hit)
    {
        if (hit.gameObject.GetComponent<WallManager>())
        {
            // Flashing object
            hit.transform.GetComponent<WallManager>().FlashObject(true);

            // Collision effect
            ContactPoint contact = hit.contacts[0];
            hit.gameObject.GetComponent<WallManager>().Damage(contact.point, 0.2f);
            Destroy(gameObject);
        }

        if (hit.transform.tag == "bullet" || hit.transform.tag == "floor")
        {
            Destroy(gameObject);
        }
    }
}
