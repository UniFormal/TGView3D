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

public class Elevator : MonoBehaviour
{
    public float topAltitude;
    public float bottomAltitude;
    public Transform floorObject;
    public ElevatorTrigger floorTrigger;
    public ElevatorDoors doors;
    public bool floorOnly;
    public float speed;

    public bool FloorOnly
    {
        get
        {
            return floorOnly;
        }
        set
        {
            foreach (Transform child in transform)
            {
                if (child != floorObject)
                {
                    child.gameObject.SetActive(!value);
                }
            }
            floorOnly = value;
        }
    }

    public float Speed
    {
        set
        {
            speed = value;
        }
    }
    
    public void GoUp()
    {
        StopAllCoroutines();
        StartCoroutine(GoToFloorAndOpenDoor(true));
    }

    public void GoDown()
    {
        StopAllCoroutines();
        StartCoroutine(GoToFloorAndOpenDoor(false));
    }

    private IEnumerator GoToFloorAndOpenDoor(bool goToTop)
    {
        if (!IsAtDestination(goToTop) && !floorOnly && !doors.isClosed)
        {
            yield return doors.StartCoroutine(doors.MoveDoor(false));
        }
        yield return StartCoroutine(MoveElevator(goToTop));
        if (!floorOnly)
        {
            yield return doors.StartCoroutine(doors.MoveDoor(true));
        }
    }

    private IEnumerator MoveElevator(bool goToTop)
    {
        float progress = GetProgressFromPositions(bottomAltitude, topAltitude, transform.position.y);
        Vector3 topPosition = new Vector3(transform.position.x, topAltitude, transform.position.z);
        Vector3 bottomPosition = new Vector3(transform.position.x, bottomAltitude, transform.position.z);

        int direction = goToTop ? 1 : -1;
        floorTrigger.goToTop = !goToTop;
        while (true)
        {
            progress = Mathf.Clamp01(progress + (speed * Time.deltaTime * direction));
            transform.position = Vector3.Lerp(bottomPosition, topPosition, progress);
            if (progress >= 1 || progress <= 0)
            {
                break;
            }
            yield return null;
        }
    }

    private bool IsAtDestination(bool gotoTop)
    {
        bool result = false;
        if (gotoTop && transform.position.y >= topAltitude)
        {
            result = true;
        } else if (!gotoTop && transform.position.y <= bottomAltitude)
        {
            result = true;
        }
        return result;
    }

    private float GetProgressFromPositions(float near, float far, float current)
    {
        float normalizedFarPosition = far - near;
        float normalizedCurrentPosition = current - near;
        float progress = normalizedCurrentPosition / normalizedFarPosition;
        return progress;
    }
}
