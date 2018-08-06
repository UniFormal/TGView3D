using UnityEngine;
using System.Collections;

// Simple script to keep an object within the user's play area. 
// If the play area is configured, the object attempts to stay within its initial distance,
// but clamped to be within the play area.
// If the play area is not configured, the object is left alone.
// A shipping title might use similar logic to keep critical interactive objects within
// the player's playable area.
public class KeepObjectInBounds : MonoBehaviour
{
    Vector3 m_initialOffset;
    public OVRCameraRig m_playerOrigin;
    public GuardianBoundaryDemoManager m_demo;

    void Start()
    {
        m_demo.TrackingChanged += RefreshDisplay;
        m_initialOffset = gameObject.transform.position - m_playerOrigin.transform.position;
        RefreshDisplay();
    }

	void RefreshDisplay()
    {
		bool configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            float xMin = 10000.0f; float zMin = 10000.0f;
            float xMax = -10000.0f; float zMax = -10000.0f;

            for (int i = 0; i < boundaryPoints.Length; ++i)
            {
                // Transforming the points to deal with the case where GuardianBoundaryDemoSettings.AllowRecenterYaw = false.
                // The boundary points will be returned in the new tracking space, but we want to ignore the new orientation
                // and instead use our nicely axis-aligned original play area.
                // If AllowRecenterYaw = true, trackingSpace will simply be the identity, so this is fine.
                boundaryPoints[i] = m_demo.OrientToOriginalForward * boundaryPoints[i];

                xMin = Mathf.Min(xMin, boundaryPoints[i].x);
                zMin = Mathf.Min(zMin, boundaryPoints[i].z);
                xMax = Mathf.Max(xMax, boundaryPoints[i].x);
                zMax = Mathf.Max(zMax, boundaryPoints[i].z);
            }

            // Now we can easily constrain the object's position to be within the play area.
            Vector3 newPos = m_initialOffset;
            newPos.x = Mathf.Max(Mathf.Min(xMax, m_initialOffset.x), xMin);
            newPos.z = Mathf.Max(Mathf.Min(zMax, m_initialOffset.z), zMin);
            newPos.y = gameObject.transform.position.y;

            if (m_demo.AllowRecenterYaw)
            {
                newPos = Quaternion.Inverse(m_demo.OrientToOriginalForward) * newPos;
            }

            gameObject.transform.position = newPos;
        }
	}
}
