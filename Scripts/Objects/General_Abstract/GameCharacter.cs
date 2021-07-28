using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GP2_Team7.Objects
{
    using Scriptables;
    using Characters;
    using Modules;

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public abstract class GameCharacter : MonoBehaviour
    {
        private Rigidbody _rb;

        internal Vector3 _speed;
        public virtual Vector3 Speed => _speed;

        protected Transform _transform;

        [SerializeField]
        protected internal MovementData _moveData;

        // [SerializeReference]
        // protected BehaviourAvatar _avatar;

        // public BehaviourAvatar Avatar => _avatar;

        [SerializeField]
        protected CollisionModule _collisionModule;

        protected abstract bool CanCheckCollision();

        public float CharTimeScale { get; internal set; } = 1;

        // Might change this later in case we'd want 
        // to implement individual time scales.
        public float CharDeltaTime => Time.deltaTime;

        public float CharFixedDeltaTime => Time.fixedDeltaTime;

        private float _remainInPlaceTimer;
        private Vector3 _remainPlace;

        protected virtual void Awake()
        {
            _transform = transform;

            _collisionModule.Initialize(_transform.GetComponent<CapsuleCollider>());

            _remainInPlaceTimer = 0.25f;

            _remainPlace = _transform.position;

            _rb = GetComponent<Rigidbody>();

            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        protected virtual void Update()
        {
            // _avatar?.AvatarUpdate(CharDeltaTime);
            if (_remainInPlaceTimer > 0)
            {
                _remainInPlaceTimer -= Time.deltaTime;
                _transform.position = _remainPlace;
                _speed = Vector3.zero;
            }

            PositionUpdate();

            _collisionModule.CollisionUpdate(CanCheckCollision());
        }

        /// <summary>
        /// Automatically applies gravity to this object.
        /// Preferrably, you should be putting this in a
        /// fixed timestep function, such as FixedUpdate.
        /// </summary>
        internal virtual void Gravity()
        {
            _speed.y -= _moveData.gravity * CharDeltaTime;
        }

        /// <summary>
        /// Automatically moves the character in a direction.
        /// Preferrably, you should be putting this in a
        /// fixed timestep function, such as FixedUpdate.
        /// </summary>
        /// <param name="vector"></param>
        internal virtual void Move(Vector3 vector, float accelerationValue)
        {
            Vector3 targetVector = vector;

            Vector3 moveVector = Vector3.MoveTowards(new Vector3(_speed.x, 0, _speed.z), vector, accelerationValue * CharDeltaTime);

            _speed.x = moveVector.x;
            _speed.z = moveVector.z;
        }

        internal virtual void Rotate(Quaternion to, float? t = null)
        {
            _transform.rotation = Quaternion.Lerp(_transform.rotation, to, t ?? _moveData.rotationValue * CharDeltaTime);
        }

        /// <summary>
        /// The method that updates transform.position for this object.
        /// May be overridden if there are unique movement mechanics,
        /// such as gravity, etc.
        /// </summary>
        protected virtual void PositionUpdate()
        {
            if (!Mathf.Approximately(Speed.magnitude, 0))
            {
                _transform.position += Speed * CharDeltaTime;
            }
        }

#if UNITY_EDITOR
        // Adding Avatars is sooooo awkward why did i doooo thiss
        [ContextMenu("Add PlayerAvatar as BehaviourAvatar")]
        void AddPlayerAvatar()
        {
            // _avatar = new PlayerAvatar(this);
        }
#endif
    }
}
