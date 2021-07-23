using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GP2_Team7.Objects
{
    using Scriptables;
    using Avatars;

    public class GameCharacter : MonoBehaviour
    {
        private Transform _transform;

        [SerializeField]
        protected internal MovementData _moveData;

        [SerializeReference]
        protected BehaviourAvatar _avatar;

        public BehaviourAvatar Avatar => _avatar;

        [SerializeField]
        protected CollisionModule _collisionModule;

        public float CharTimeScale { get; internal set; } = 1;

        // Might change this later in case we'd want 
        // to implement individual time scales.
        public float CharDeltaTime => Time.deltaTime;

        public float CharFixedDeltaTime => Time.fixedDeltaTime;

        void Awake()
        {
            _transform = transform;
            _collisionModule.SetCapsuleAndTransform(_transform.GetComponent<CapsuleCollider>());
        }

        void Update()
        {
            _avatar?.AvatarUpdate(CharDeltaTime);
        }

        void FixedUpdate()
        {
            _avatar?.AvatarFixedUpdate();
        }

#if UNITY_EDITOR
        [ContextMenu("Add PlayerAvatar as BehaviourAvatar")]
        void AddPlayerAvatar()
        {
            _avatar = new PlayerAvatar(this);
        }
#endif
    }
}
