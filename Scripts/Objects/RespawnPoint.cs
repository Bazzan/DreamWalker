using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    [RequireComponent(typeof(SphereCollider))]
    public class RespawnPoint : MonoBehaviour
    {
        public static System.Action<Vector3, Quaternion> onRespawnPointSet = delegate { };

        [SerializeField]
        private Transform _respawnPoint;

        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                onRespawnPointSet(_respawnPoint.position, _respawnPoint.rotation);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_respawnPoint.position, 0.5f);
        }
    }
}