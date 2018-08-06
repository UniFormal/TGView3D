using UnityEngine;
using System.Collections;

public class GrabbableCrosshair : MonoBehaviour
{
    public enum CrosshairState { Disabled, Enabled, Targeted }

    CrosshairState m_state = CrosshairState.Disabled;
    Transform m_centerEyeAnchor;

    [SerializeField]
    GameObject m_targetedCrosshair;
    [SerializeField]
    GameObject m_enabledCrosshair;

    private void Start()
    {
        m_centerEyeAnchor = GameObject.Find("CenterEyeAnchor").transform;
    }

    public void SetState(CrosshairState cs)
    {
        m_state = cs;
        if(cs == CrosshairState.Disabled)
        {
            m_targetedCrosshair.SetActive(false);
            m_enabledCrosshair.SetActive(false);
        }
        else if(cs == CrosshairState.Enabled)
        {
            m_targetedCrosshair.SetActive(false);
            m_enabledCrosshair.SetActive(true);
        }
        else if(cs == CrosshairState.Targeted)
        {
            m_targetedCrosshair.SetActive(true);
            m_enabledCrosshair.SetActive(false);
        }
    }

    private void Update()
    {
        if(m_state != CrosshairState.Disabled)
        {
            transform.LookAt(m_centerEyeAnchor);
        }
    }
}
