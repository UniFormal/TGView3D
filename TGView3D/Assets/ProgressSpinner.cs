using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressSpinner : MonoBehaviour
{
    private RectTransform rectComponent;
    private float rotateSpeed = 300f;

    private void OnEnable()
    {
        rectComponent = GetComponent<RectTransform>();
        StartCoroutine(Spin());
    }
    private void OnDisable()
    {

    }


    private  IEnumerator Spin()
    {

        while (this.isActiveAndEnabled)
        {
            rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
            transform.parent.position = Input.mousePosition;
            yield return null;
        }

    }
}

