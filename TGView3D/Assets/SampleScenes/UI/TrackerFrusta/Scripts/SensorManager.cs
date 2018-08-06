using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SensorManager : MonoBehaviour
{
    public GameObject SensorPrefab;
    public bool ShowLines { 
        get { return m_showLines; }
        set
        {
            m_showLines = value;
            for(int i=0; i<Sensors.Count; ++i)
            {
                TrackerFrustum t = Sensors[i].GetComponent<TrackerFrustum>();
                t.ShowLines = value;
            }
        }
    }
    bool m_showLines;
    
    public bool ShowGeo
    {
        get { return m_showGeo; }
        set
        {
            m_showGeo = value;
            for(int i=0; i<Sensors.Count; ++i)
            {
                TrackerFrustum t = Sensors[i].GetComponent<TrackerFrustum>();
                t.ShowGeo = value;
            }
        }
    }
    bool m_showGeo;

    public List<GameObject> Sensors = new List<GameObject>();

    private bool firstFrame = true;
    private bool sensorsSetUp = false;

	void Start()
    {
	    
	}
	
	void LateUpdate()
    {
	    if (firstFrame)
	    {
	        firstFrame = false;
	    }
	    else if (!sensorsSetUp)
	    {
	        SetupSensors();
	    }
    }

    private void SetupSensors()
    {
        GameObject newSensor;
        
        Debug.Log(OVRManager.tracker.count);
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();

        for (int i = 0; i < OVRManager.tracker.count; i++)
        {
            newSensor = (GameObject) Instantiate(SensorPrefab);
            newSensor.transform.parent = rig.transform;
            newSensor.GetComponent<TrackerFrustum>().SensorID = i;
            
            Sensors.Add(newSensor);
        }

        sensorsSetUp = true;
        ShowLines = true;
        ShowGeo = false;
    }
}
