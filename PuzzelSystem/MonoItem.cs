using FMODUnity;
using UnityEngine;

namespace GP2_Team7.Items
{
    using Objects;
    using Objects.Cameras;

    public class MonoItem : GameCharacter, IFocable, IGravity
    {
        public static MonoItem SpawnItem(Item itemType, Vector3 position, Quaternion localRotation, Transform parent = null, Vector3 spawnVelocity = default, Quaternion gravity = default)
        {
            MonoItem monoItem = new GameObject(itemType.Name).AddComponent<MonoItem>();
            monoItem._representedItem = itemType;
            monoItem.transform.position = position;
            monoItem.GravityRotation = gravity;
            monoItem.LocalGravityRotation = localRotation;
            monoItem._speed = spawnVelocity;

            monoItem.transform.parent = parent;

            return monoItem;
        }

        private CapsuleCollider _cc;

        public Vector3 FocusPoint => _transform.position +
            _transform.up * _cc.radius;

        public Vector3 Position => _transform.position;

        public Quaternion GravityRotation
        {
            get => Quaternion.Euler(_gravityRotation);
            set
            {
                _gravityRotation = value.eulerAngles;

                _transform.rotation = Quaternion.Euler(_gravityRotation) * _localGravityRotation;
            }
        }

        private Quaternion _localGravityRotation;
        /// <summary>
        /// The object rotation in its gravity space.
        /// </summary>
        public Quaternion LocalGravityRotation
        {
            get => _localGravityRotation;
            set
            {
                _localGravityRotation = value;

                _transform.rotation = Quaternion.Euler(_gravityRotation) * _localGravityRotation;
            }
        }

        private Vector3 _gravityRotation;

        private Transform _modelTrans;

        protected override bool CanCheckCollision() => _representedItem.DetectCollision;

        private bool _isCollected = false;

        private int _bounces = 2;

        private float _bounceSpeedThreshold = -8;

        [SerializeField]
        private Item _representedItem;

        [SerializeField]
        private Vector3 _representRotSpeed = new Vector3(0, 60, 0);

        [SerializeField]
        private StudioEventEmitter _emitter;

        protected override void Awake()
        {
            base.Awake();

            if (_representedItem.HasBeenCollectedOnce && _representedItem.IsCollectableOnlyOnce)
            {
                Destroy(gameObject);
                return;
            }

            _cc = GetComponent<CapsuleCollider>();

            _cc.radius = 0.5f;
            _cc.height = 0;
            _cc.isTrigger = true;

            _collisionModule.Initialize(_cc, 0b0100_0000);

            _collisionModule.AssignCollisionEnterCallback(OnColEnter);

            gameObject.layer = 10;

            for (int i = 0; i < _transform.childCount; i++)
            {
                Destroy(_transform.GetChild(i).gameObject);
            }

            _modelTrans = Instantiate(_representedItem.Model, _transform, false).transform;
        }

        protected override void Update()
        {
            base.Update();

            _modelTrans.Rotate(_representRotSpeed * Time.deltaTime, Space.Self);
        }

        protected override void PositionUpdate()
        {
            if (!Mathf.Approximately(Speed.magnitude, 0))
            {
                _transform.position += GravityRotation * Speed * CharDeltaTime;
            }
        }

        private void OnColEnter(ColSide colSide, Objects.Modules.CollisionModule.ColHit hit)
        {
            if (_speed.y < _bounceSpeedThreshold && _bounces > 0)
            {
                _speed.y *= -0.75f;
                _bounces--;
            }
            else
            {
                _speed = Vector3.zero;
                _bounces = 0;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected)
                return;

            IPossessInventory possessInventory = other.GetComponent<IPossessInventory>();

            if (possessInventory != null)
            {
                _emitter.Play();
                _representedItem.OnCollect(possessInventory.Inventory);
                _isCollected = true;
                Destroy(gameObject);
            }
        }
    }
}
