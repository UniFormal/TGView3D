using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAway : MonoBehaviour
{
    // Start is called before the first frame update




    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MoveRoutine()
    {
        while (transform.position.x < Screen.width-100)
        {
            transform.parent.position += Vector3.right * 100f;
            yield return new WaitForSeconds(.01f);
        }
        while (transform.position.x < Screen.width-10)
        {
            transform.parent.position += Vector3.right * .1f;
           
        }
       // transform.parent.GetComponent<RectTransform>().set
    }
    IEnumerator MoveBackRoutine()
    {
        while (transform.position.x > 500)
        {
            transform.parent.position -= Vector3.right * 100f;
            yield return new WaitForSeconds(.01f);
        }
        
        while (transform.position.x > Screen.height*.3f)
        {
            transform.parent.position -= Vector3.right * .1f;

        }
    }

    public void MoveRight()
    {
        StartCoroutine(MoveRoutine());
    }

    public void Move()
    {
        if (transform.position.x >= Screen.width - 16)
        {
            StartCoroutine(MoveBackRoutine());
        }
        else
        StartCoroutine(MoveRoutine());
    }


}
