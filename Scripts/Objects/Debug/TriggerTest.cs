using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7
{
    using Objects.Cameras;

    public class TriggerTest : MonoBehaviour
    {
        [SerializeField]
        private bool _isCutscene = false;

        [SerializeField]
        private float _duration;

        [SerializeField]
        private CamFixedViewSettings _viewSettings;

        [Space, SerializeField]
        private CamFixedViewSettings _fixedViewSettings;

        public void OnTriggerEnter(Collider collider)
        {
            if (_isCutscene)
            {
                CameraController.CutscenedCameraEvent(_viewSettings, _duration, OnReach);
            }
            else
            {
                CameraController.SwitchToFixedCameraView(_fixedViewSettings);
            }
        }

        private void OnReach()
        {
            print("the mysterious mr boob");
        }

        public void OnTriggerExit(Collider collider)
        {
            if (!_isCutscene)
            {
                CameraController.SwitchToStandardCameraView();
            }
        }
    }
}
