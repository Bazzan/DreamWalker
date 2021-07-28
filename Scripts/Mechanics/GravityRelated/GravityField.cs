using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Managers
{
    using Objects;
    using Objects.Characters;
    using Objects.Cameras;

    public class GravityField : MonoBehaviour
    {
        private Transform _transform;

        private List<IGravity> _gravityObjects = new List<IGravity>();

        private Collider _collider;

        private System.Func<IGravity, Quaternion> _gravityMethod;

        [SerializeField]
        private bool _inverted;

        // Start is called before the first frame update
        void Awake()
        {
            _transform = transform;

            _collider = GetComponent<Collider>();

            if (_collider is SphereCollider)
                _gravityMethod = SphereGravity;
            else if (_collider is CapsuleCollider)
                _gravityMethod = CapsuleGravity;
        }

        // Update is called once per frame
        void Update()
        {
            if (_gravityMethod == null)
                throw new System.Exception("Force field collider type must be a sphere collider or a capsule collider!");

            foreach (IGravity gravity in _gravityObjects)
            {
                gravity.GravityRotation = _gravityMethod(gravity);
            }
        }

        private Quaternion SphereGravity(IGravity gravityObject)
        {
            Vector3 delta = _inverted ? _transform.position - gravityObject.Position : gravityObject.Position - _transform.position;

            Quaternion rot = Quaternion.FromToRotation(gravityObject.GravityRotation * Vector3.up, delta) * 
                gravityObject.GravityRotation;// * Quaternion.LookRotation(_transform.position - gravityObject.Position);

            return rot;
        }

        private Quaternion CapsuleGravity(IGravity gravityObject)
        {
            CapsuleCollider cc = _collider as CapsuleCollider;

            Vector3 relativeCenter = _transform.InverseTransformPoint(gravityObject.Position);

            switch (cc.direction)
            {
                case 0:
                    relativeCenter.x = Clamp(relativeCenter.x);
                    relativeCenter.y = 0;
                    relativeCenter.z = 0;
                    break;

                case 1:
                    relativeCenter.x = 0;
                    relativeCenter.y = Clamp(relativeCenter.y);
                    relativeCenter.z = 0;
                    break;

                case 2:
                    relativeCenter.x = 0;
                    relativeCenter.y = 0;
                    relativeCenter.z = Clamp(relativeCenter.z);
                    break;
            }

            relativeCenter = _transform.TransformPoint(relativeCenter);

            Vector3 delta = _inverted ? relativeCenter - gravityObject.Position : gravityObject.Position - relativeCenter;

            Quaternion rot = Quaternion.FromToRotation(gravityObject.GravityRotation * Vector3.up, delta) *
                gravityObject.GravityRotation;// * Quaternion.LookRotation(_transform.position - gravityObject.Position);

            return rot;

            float Clamp(float value)
            {
                float limit = cc.height * 0.5f - cc.radius;
                return Mathf.Clamp(value, -limit, limit);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            IGravity gravity = other.GetComponent<IGravity>();

            if (gravity != null && !_gravityObjects.Contains(gravity))
            {
                _gravityObjects.Add(gravity);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IGravity gravity = other.GetComponent<IGravity>();

            if (gravity != null && _gravityObjects.Contains(gravity))
            {
                _gravityObjects.Remove(gravity);
            }
        }
    }
}