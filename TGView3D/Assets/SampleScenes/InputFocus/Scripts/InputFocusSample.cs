using UnityEngine;
using System.Collections;

public class InputFocusSample : MonoBehaviour
{

    public GameObject nearObjectOwner;

    private void OnInputFocusAcquired()
    {
        Debug.Log("Input Focus Acquired, the application is the foreground application and should receive input, resume the game");

        Transform parentTransformObj = nearObjectOwner.transform;

        // Show hidden near field objects
        for (int i = 0; i < parentTransformObj.childCount; i++)
        {
            Transform child = parentTransformObj.GetChild(i);
            child.gameObject.GetComponent<Renderer>().enabled = true;
        }

        // Resume animations
        Time.timeScale = 1.0f;

        // Resume the sound/music
        GetComponent<AudioSource>().enabled = true;
    }

    private void OnInputFocusLost()
    {
        Debug.Log("Input Focus Lost,  the application is in the background, should hide any input representations such as hands, pause the game");

        // Hide near field Objects
        float hideObjectsInRange = 3.0f;
        Transform parentTransformObj = nearObjectOwner.transform;

        for (int i = 0; i < parentTransformObj.childCount; i++)
        {
            Transform child = parentTransformObj.GetChild(i);

            if ((child.position - Camera.main.transform.position).magnitude < hideObjectsInRange)
                child.gameObject.GetComponent<Renderer>().enabled = false;
        }

        // Pause animations
        Time.timeScale = 0.0f;

        // Pause the sound/music
        GetComponent<AudioSource>().enabled = false;
    }

    // Use this for initialization
    void Start()
    {
        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        OVRManager.InputFocusLost += OnInputFocusLost;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
