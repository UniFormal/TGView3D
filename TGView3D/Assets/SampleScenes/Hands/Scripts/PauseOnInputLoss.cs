using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OVRTouchSample
{
    public class PauseOnInputLoss : MonoBehaviour
    {
        void Start()
        {
            OVRManager.InputFocusAcquired += OnInputFocusAcquired;
            OVRManager.InputFocusLost += OnInputFocusLost;
        }

        private void OnInputFocusLost()
        {
            Time.timeScale = 0.0f;
        }

        private void OnInputFocusAcquired()
        {
            Time.timeScale = 1.0f;
        }
    }
}
