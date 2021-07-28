using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Characters
{
    using Algorithms;
    using Items;
    using Cameras;
    using Managers;

    public partial class PlayerCharacter : GameCharacter, IControllable, IFocable, IGravity, IPossessInventory, IPortalTraversable
    {
        public Vector3 FocusPoint
        {
            get
            {
                Vector3 focusPoint;

                //if (TransPortalMode)
                //{
                //    focusPoint = InPortal.transform.TransformPoint(OutPortal.PortalPoint(_transform.position));
                //}
                //else
                {
                    focusPoint = _transform.position;
                }

                return focusPoint +
                _transform.rotation * _focusPointOffset;
            }
        }

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
            get
            {
                return _localGravityRotation;
            }

            set
            {
                _localGravityRotation = value;

                _transform.rotation = Quaternion.Euler(_gravityRotation) * _localGravityRotation;
            }
        }

        public InventoryManager Inventory => GameManager.Inventory;

        #region IPortalTraversable

        public GameObject PortalModel => _playerModel;

        public GameObject PortalClone { get; set; }

        public JPortal InPortal { get; private set; }

        public JPortal OutPortal { get; private set; }

        #endregion

        private JStateMachine<PlayerState> _playerStateMachine;

        // The phase actions will be modified by whatever
        // PlayerState is currently running in the state machine.
        internal Dictionary<Actions, ActionDelegates> _phaseActions;

        private GameObject _playerModel;

        private Animator _anim;
        private Animator _cloneAnim;

        private SoundEmitter _soundEmitter;

        // Inspector values

        [SerializeField]
        private bool _canCheckCollision = true;

        protected override bool CanCheckCollision() => 
            _canCheckCollision && _playerStateMachine.CurrentState.CanCheckCollision();

        [SerializeField, Tooltip("The euler angles representing the player's gravity.")]
        private Vector3 _gravityRotation;

        [SerializeField, Tooltip("Sets a relative point for where the " +
            "camera should focus on the player.")]
        private Vector3 _focusPointOffset;

        [Space, SerializeField, Tooltip("Make sure that selection is a prefab!")]
        private ParticleSystem _platformDashStartFX;
        private ParticleSystem _platformDashStartFXSpawned;

        [SerializeField, Tooltip("Make sure that selection is a prefab!")]
        private ParticleSystem _platformDashFX;
        private ParticleSystem _platformDashFXSpawned;

        [SerializeField, Tooltip("Make sure that selection is a prefab!")]
        private ParticleSystem _platformDashEndFX;
        private ParticleSystem _platformDashEndFXSpawned;

        [SerializeField]
        private float _voidOutHeight = -100;
        
        [SerializeField]
        private string _interactSoundObjectName = "Interact";

        [SerializeField]
        private Avatar _humanoid;

        // End of Inspector values

        protected override void Awake()
        {
            base.Awake();

            _anim = GetComponent<Animator>();

            _soundEmitter = GetComponentInChildren<SoundEmitter>();

            _playerModel = _transform.Find("Player_Model").gameObject;

            LocalGravityRotation = Quaternion.Inverse(GravityRotation) * _transform.rotation;

            _collisionModule.AssignCollisionStayCallback(OnCollisionSetGravity);

            _phaseActions = new Dictionary<Actions, ActionDelegates>()
            {
                { Actions.Forward, null },
                { Actions.Backward, null },
                { Actions.Left, null },
                { Actions.Right, null },
            };

            print("ive been reborn");

            _playerStateMachine = new JStateMachine<PlayerState>();
            _playerStateMachine.ChangeState(new PS_Idle_Volatile(this));

            InteractableMoveToPlatform.onInteracted = OnPlatformInteracted;
            CircleClickPuzzle.startMovingPlayer = OnStartMovingCircleClickPuzzle;

            if (GameManager.Player != gameObject)
                Destroy(gameObject);
        }

        public void PlayInteractSound()
        {
            if(_soundEmitter != null)
                _soundEmitter.Play(_interactSoundObjectName);
        }
        
        private void OnEnable()
        {
            GameManager.SubscribeVoidOut(OnVoidOut, true);
        }

        private void OnDisable()
        {
            GameManager.SubscribeVoidOut(OnVoidOut, false);
        }

        protected override void Update()
        {
            _playerStateMachine.StateMachineUpdate(CharDeltaTime);

            base.Update();

            if (_transform.position.y < _voidOutHeight && !CutsceneManager.IsInCutscene)
            {
                GameManager.VoidOut(VoidOutType.FallingIntoBottomlessPit);
            }

            if (PortalClone)
            {
                if (!_cloneAnim)
                {
                    GameObject cloneParent = new GameObject("Player (PortalClone)");
                    cloneParent.transform.SetPositionAndRotation(PortalClone.transform.position, PortalClone.transform.rotation);
                    PortalClone.transform.parent = cloneParent.transform;
                    PortalClone.name = PortalModel.name;
                    PortalClone = cloneParent;

                    _cloneAnim = cloneParent.AddComponent<Animator>();
                    _cloneAnim.avatar = _anim.avatar;
                    _cloneAnim.runtimeAnimatorController = _anim.runtimeAnimatorController;
                }
                else if (PortalClone.activeInHierarchy)
                {
                    AnimatorStateInfo cloneState = _cloneAnim.GetCurrentAnimatorStateInfo(0);
                    AnimatorStateInfo thisState = _anim.GetCurrentAnimatorStateInfo(0);
                    if ((cloneState.normalizedTime != thisState.normalizedTime || cloneState.fullPathHash != thisState.fullPathHash))
                    {
                        if (!_anim.IsInTransition(0))
                            _cloneAnim.Play(thisState.fullPathHash, 0, thisState.normalizedTime);
                        //else
                            //_cloneAnim.CrossFadeInFixedTime(thisState.fullPathHash, _animTransitionTime, 0, _anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * thisState.normalizedTime, _anim.GetAnimatorTransitionInfo(0).normalizedTime);
                    }
                        
                }
            }

        }

        void FixedUpdate()
        {
            _playerStateMachine.StateMachineFixedUpdate();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!(Application.isPlaying && _transform))
                return;

            GravityRotation = Quaternion.Euler(_gravityRotation);
        }
#endif

        public Vector3 VectorToCameraRotation(Vector3 vector)
        {
            Vector3 result = CameraController.VectorToCameraRotation(vector);

            //if (TransPortalMode)
            //{
            //    result = Quaternion.Inverse(InPortal.transform.rotation) * Quaternion.Euler(0, 180, 0) * OutPortal.transform.rotation * result;
            //}

            return result;
        }

        // Added these three methods to lock/unlock player movement, didn't see a better solution / Oliver
        public void LockPlayerMovement()
        {
            if (_playerStateMachine.CurrentState is PS_Idle_Volatile || _playerStateMachine.CurrentState is PS_Airborne)
                _playerStateMachine.ChangeState(new PS_LockedMovement(this));
        }

        public void UnlockPlayerMovement()
        {
            if (_playerStateMachine.CurrentState is PS_LockedMovement)
                _playerStateMachine.ChangeState(new PS_Idle_Volatile(this));
        }

        public void TogglePlayerMovement()
        {
            LockPlayerMovement();
            UnlockPlayerMovement();
        }
        
        private void OnPlatformInteracted(InteractableMoveToPlatform platform, MoveToPlatformData moveToPlatformData)
        {
            moveToPlatformData.platformTransform = platform.transform;

            if (_playerStateMachine.CurrentState is PS_Idle_Volatile || _playerStateMachine.CurrentState is PS_Airborne)
                _playerStateMachine.ChangeState(new PS_PlatformTraversal(this, moveToPlatformData));
        }

        private void OnStartMovingCircleClickPuzzle(CircleClickPuzzleData data)
        {
            if (_playerStateMachine.CurrentState is PS_Idle_Volatile || _playerStateMachine.CurrentState is PS_Airborne || _playerStateMachine.CurrentState is PS_LockedMovement)
                _playerStateMachine.ChangeState(new PS_BlackRoom(this, data));
        }

        internal override void Rotate(Quaternion to, float? t = null)
        {
            if (t == null)
                t = _moveData.rotationValue;

            //if (TransPortalMode)
            //{
            //    to = OutPortal.transform.rotation * InPortal.PortalRotation(to)/* * Quaternion.Euler(0, 180, 0)*/;
            //}

            LocalGravityRotation = Quaternion.Lerp(LocalGravityRotation, to, (t ?? _moveData.rotationValue) * CharDeltaTime);
        }

        public void PlayFootstep()
        {
            _soundEmitter.Play("Footsteps");
        }

        protected override void PositionUpdate()
        {
            if (!Mathf.Approximately(Speed.magnitude, 0))
            {
                _transform.position += GravityRotation * Speed * CharDeltaTime;
            }
        }

        private void OnCollisionSetGravity(ColSide colSide, Modules.CollisionModule.ColHit hit)
        {
            if (colSide == ColSide.Ground && hit.collider.CompareTag("Gravity"))
            {
                //print("grav");
                Gravitation.SetGravityFromNormal(GravityRotation, hit.normal, out Quaternion newGravityRotation);
                GravityRotation = newGravityRotation;
            }
        }

        private void SetGravity(Quaternion gravityRotation)
        {
            _gravityRotation = gravityRotation.eulerAngles;
        }

        // Method called by InputManager to trigger input events.
        public ActionDelegates GetActionDelegates(Actions actions)
        {
            if (_phaseActions.ContainsKey(actions))
                return _phaseActions[actions];

            return null;
        }

        private void OnVoidOut(VoidOutType voidOutType)
        {
            if (_playerStateMachine.CurrentState.GetType() != typeof(PS_VoidOut))
                _playerStateMachine.ChangeState(new PS_VoidOut(this));
        }

        public void OnPortalTraverse(JPortal inPortal, JPortal outPortal, Vector3 newPosition, Quaternion newRotation)
        {
            InPortal = inPortal;
            OutPortal = outPortal;
            LocalGravityRotation = Quaternion.Inverse(InPortal.transform.rotation) * Quaternion.Euler(0, 180, 0) * OutPortal.transform.rotation * LocalGravityRotation;
            //_transform.rotation = Quaternion.Inverse(_inPortal.transform.rotation) * Quaternion.Euler(0, 180, 0) * _outPortal.transform.rotation * _transform.rotation;
            //_transform.position = newPosition;

            Vector3 otherPortalSpeed = inPortal.transform.InverseTransformVector(_speed);

            otherPortalSpeed.x = -otherPortalSpeed.x;
            otherPortalSpeed.z = -otherPortalSpeed.z;

            _speed = outPortal.transform.TransformVector(otherPortalSpeed);

            // Shitty day-before-release workaround for when you leave
            // the portal for the main hub... I'm sorry, whoever is
            // code-reviewing this. :(
            if (_playerStateMachine.CurrentState is PS_BlackRoom)
            {
                _playerStateMachine.ChangeState(new PS_Idle_Volatile(this));
                CameraController.SwitchToStandardCameraView();

                Cursor.lockState = CursorLockMode.Locked;

                _speed = _transform.forward * 8;
            }

            _collisionModule.Teleport(_transform.position);

            if (PortalClone)
            {
                if (!_cloneAnim)
                {
                    GameObject cloneParent = new GameObject("Player (PortalClone)");
                    cloneParent.transform.SetPositionAndRotation(PortalClone.transform.position, PortalClone.transform.rotation);
                    PortalClone.transform.parent = cloneParent.transform;
                    PortalClone.name = name;
                    PortalClone = cloneParent;

                    _cloneAnim = cloneParent.AddComponent<Animator>();
                    _cloneAnim.avatar = _anim.avatar;
                    _cloneAnim.runtimeAnimatorController = _anim.runtimeAnimatorController;
                }
            }

            //PortalClone.GetComponent<Animator>().Play(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash, 0, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).);
            //TransPortalMode = !TransPortalMode;
        }
    }
}
