using UnityEngine;

namespace OVRTouchSample
{
    /**
     * This code is only to assist some demo features, such as switching between grab volumes and spherecasts.
     * None of this code is needed in a shipping app.
     */
    public class DistanceGrabDemo : MonoBehaviour
    {
        bool m_useSpherecast = false;

        [SerializeField]
        GameObject m_GrabThroughWalls;

        public bool UseSpherecast
        {
            get { return m_useSpherecast; }
            set
            {
                m_useSpherecast = value;
                for(int i=0; i<m_grabbers.Length; ++i)
                {
                    m_grabbers[i].UseSpherecast = m_useSpherecast;
                }
            }
        }

        bool m_allowGrabThroughWalls = false;
        public bool AllowGrabThroughWalls
        {
            get { return m_allowGrabThroughWalls; }
            set
            {
                m_allowGrabThroughWalls = value;
                for(int i=0; i<m_grabbers.Length; ++i)
                {
                    m_grabbers[i].m_preventGrabThroughWalls = !m_allowGrabThroughWalls;
                }
            }
        }

        [SerializeField]
        DistanceGrabber[] m_grabbers;

    }
}
