using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    using Cameras;
    using Managers;

    public class ShakeZone : MonoBehaviour
    {
        private Transform _transform;

        [SerializeField]
        public CameraShakeValues shakeValues;

        [SerializeField]
        private AnimationCurve _falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [SerializeField]
        public float multiplier = 1;

        private CameraShake _shake;

        private SphereCollider _sphereCollider;

        private void Awake()
        {
            _shake = new CameraShake(shakeValues, FalloffByDistance);

            _transform = transform;

            _sphereCollider = GetComponent<SphereCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == GameManager.Player)
            {
                _shake.cameraShakeValues = shakeValues;
                CameraController.Shake(_shake);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == GameManager.Player)
                CameraController.RemoveShake(_shake);
        }

        private float FalloffByDistance()
        {
            return Mathf.Clamp(multiplier * _falloffCurve.Evaluate
                ((float)Vector3.Distance(GameManager.Player.transform.position, _transform.position) / _sphereCollider.radius), 0, 1);
        }

        private void OnDestroy()
        {
            CameraController.RemoveShake(_shake);
        }
    }
}

