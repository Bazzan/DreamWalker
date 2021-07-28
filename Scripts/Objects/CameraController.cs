using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace GP2_Team7.Objects.Cameras
{
    using Scriptables;
    using Characters;
    using Managers;
    using Algorithms;

    [DefaultExecutionOrder(-50)]
    public class CameraController : MonoBehaviour
    {
        #region StaticProperties

        public static CameraController Main { get; private set; }

        public static Camera MainCamera => Main?._camera;

        public static IFocable Focable => Main?._focable;

        public static Transform FocusPoint => Main?._focusPoint;

        public static Quaternion Rotation => Main?._pivotTrans.localRotation ?? Quaternion.identity;

        public static Quaternion RotationY
        {
            get
            {
                Quaternion q = Quaternion.Euler(0, Main?._pivotTrans.localEulerAngles.y ?? 0, 0);

                if (Main && Main.FocusOnPortalClone)
                {
                    //if (TransPortalMode)
                    //{
                    //    to = OutPortal.transform.rotation * InPortal.PortalRotation(to)/* * Quaternion.Euler(0, 180, 0)*/;
                    //}

                    q = Quaternion.Inverse(Main._traversable.PortalClone.transform.rotation) * Main._focusPoint.rotation * q;
                }

                return q;
            }
        }

        public static CameraState CurrentState => Main._camStateMachine.CurrentState;

        public static bool IsInStandardMode => Main?._camStateMachine.CurrentState is CS_Standard;
        
        public float MouseSensitivity
        {
            set => _cameraValues.mouseSensitivity = value;
        }

        public static Vector2 GetMouseInputWithSensitivity() => GameManager.GetMouseInput() * 
            (Main && !CutsceneManager.IsInCutscene ? Main._cameraValues.mouseSensitivity : 0);

        public static System.Action<Scene> onActiveSceneChange = delegate { };

        #endregion

        #region StaticMethods

        public static void SwitchToFixedCameraView(CamFixedViewSettings settings)
        {
            if (!(Main._camStateMachine.CurrentState is CS_FixedView))
                Main._camStateMachine.ChangeState(new CS_FixedView(Main, settings));
        }

        /// <summary>
        /// Will change the state of the camera so it will stop listening to input and be completely locked in place
        /// </summary>
        public static void SwitchToStaticCameraView(bool parentToPlayer, Vector3 localPosition, Quaternion rotation)
        {
            if (!(Main._camStateMachine.CurrentState is CS_StaticView))
                Main._camStateMachine.ChangeState(new CS_StaticView(Main, parentToPlayer, localPosition, rotation));
        }

        public static void SwitchToStandardCameraView()
        {
            if (Main._camStateMachine.CurrentState is CS_FixedView)
                CS_FixedView.OnReturnToStandard();
            else
                Main._camStateMachine.ChangeState(new CS_Standard(Main));

            //OnReturnToStandard
        }

        public static void CutscenedCameraEvent(CamFixedViewSettings settings, float duration, params System.Action[] camReachDestinationActions)
        {
            print("uehfuiwhe");
            if (!CutsceneManager.IsInCutscene)
            {
                CS_CutscenedView cutsceneState = new CS_CutscenedView(Main, settings, duration, camReachDestinationActions);
                ICutscene cutscene = cutsceneState as ICutscene;

                Main._camStateMachine.ChangeState(cutsceneState);
                CutsceneManager.PlayCutscene(cutscene, settings);
            }
        }

        public static void Shake(CameraShakeValues cameraShakeValues)
        {
            Main._cameraShakes.Add(new CameraShake(cameraShakeValues, null));
        }

        public static void Shake(float shakeMagnitude, float shakeFrequency, float shakeDuration, ShakeFalloffType shakeFalloffType)
        {
            Main._cameraShakes.Add(new CameraShake(shakeMagnitude, shakeFrequency, shakeDuration, shakeFalloffType, null));
        }

        public static void Shake(CameraShake cameraShakeReference)
        {
            print(Main);
            Main._cameraShakes.Add(cameraShakeReference);
        }

        public static void RemoveShake(CameraShake cameraShakeReference)
        {
            Main._cameraShakes.Remove(cameraShakeReference);
        }

        #endregion

        private Transform _focusPoint;

        // If the object has a specific focus point that it prefers
        // cameras to look at instead of its origin.
        private IFocable _focable;

        // If the object has gravity mechanics applied to it.
        private IGravity _gravity;

        private IPortalTraversable _traversable;

        public bool FocusOnPortalClone { get; private set; } = false;

        private Collider[] _colliders = new Collider[4];

        #region CameraStructure

            // Structure: Base transform, pivot transform, camera.
            //
            // Base transform exists so that the camera can easily
            // adapt to objects being affected by gravity, so all
            // the pivot has to do is to rotate around the focus point.

            private Transform _transform;

            private Transform _pivotTrans;

            private Camera _camera;

        #endregion

        private float _xRotation;

        private Vector3 _velocity;

        private RaycastHit _hit;

        private JStateMachine<CameraState> _camStateMachine = new JStateMachine<CameraState>();

        [ContextMenu("Add Shake")]
        private void AddShake()
        {
            _cameraShakes.Add(new CameraShake(_values, null));
        }

        [SerializeField]
        private CameraShakeValues _values;

        [SerializeField]
        private List<CameraShake> _cameraShakes = new List<CameraShake>();

        // The local camera reference position. For shaking.
        private Vector3 _cameraRefLocalPosition = Vector3.zero;

        // The shake offset.
        private Vector3 _shakeOffset;

        [SerializeField]
        private CameraControllerValues _cameraValues;

        [SerializeField]
        private LayerMask _layerMask;

        private SphereCollider _sphereCol;

        private bool hasHit;

        private void Awake()
        {
            if (Main && Main != this)
            {
                Destroy(gameObject);
                return;
            }

            _transform = transform;

            _pivotTrans = _transform.GetChild(0);

            _camera = _pivotTrans.GetComponentInChildren<Camera>(true);

            _xRotation = _pivotTrans.eulerAngles.x;

            _camStateMachine.ChangeState(new CS_Standard(this));

            _colliders = new Collider[4];

            _sphereCol = _camera.GetComponent<SphereCollider>();
        }

        private void Start()
        {
            Main = this;

            InitializePosition();

            if (!_cameraValues)
                _cameraValues = ScriptableObject.CreateInstance<CameraControllerValues>();
        }

        private void Update()
        {
            _camera.transform.localPosition = TrueCameraLocalPosition();

            _camStateMachine.StateMachineUpdate(Time.deltaTime);

            if (_sphereCol.radius != _cameraValues.cameraColliderRadius)
                _sphereCol.radius = _cameraValues.cameraColliderRadius;
        }

        private void FixedUpdate()
        {
            _camStateMachine.StateMachineFixedUpdate();
        }

        private void OnPortalTraverse(IPortalTraversable traversable)
        {
            if (_traversable != null && traversable == _traversable)
            {
                FocusOnPortalClone = !FocusOnPortalClone;
            }
        }

        private void InitializePosition()
        {
            _focusPoint = GetFocusPoint();

            Vector3 position;
            Quaternion rotation;

            if (_focusPoint)
            {
                position = _focusPoint.position;

                if (_gravity == null)
                {
                    rotation = _focusPoint.rotation;
                }
                else
                {
                    _transform.rotation = _gravity.GravityRotation;

                    rotation = _gravity.LocalGravityRotation;
                }

            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }

            _transform.position = position;
            _pivotTrans.localRotation = rotation;
        }

        // This is here in case we want to change the
        // implementation somewhere in the future.
        private Transform GetFocusPoint()
        {
            PlayerCharacter player;

            if (_traversable != null)
                JPortal.onPortalTraverse -= OnPortalTraverse;

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                player = go.GetComponent<PlayerCharacter>();
                
                if (player)
                {
                    _focable = player as IFocable;
                    _gravity = player as IGravity;
                    _traversable = player as IPortalTraversable;

                    if (_traversable != null)
                        JPortal.onPortalTraverse += OnPortalTraverse;

                    return go.transform;
                }
            }

            return null;
        }

        // Even here, actually.
        public Vector3 GetDestination()
        {
            // If the player has passed through a portal, focus on
            // its clone until the camera has passed through the
            // portal as well.
            if (FocusOnPortalClone)
            {
                if (_focable != null)
                {
                    return _traversable.PortalClone.transform.localToWorldMatrix.
                        MultiplyPoint3x4(_focusPoint.InverseTransformPoint(_focable.FocusPoint));
                }
            }

            return _focable != null ? _focable.FocusPoint : 
                (_focusPoint ?? _transform).position;
        }

        // Perhaps here too.
        private void CheckCollision()
        {
            // Get camera offset in world space.
            Vector3 worldOffset = _pivotTrans.TransformPoint(_cameraValues.cameraOffset);
            Vector3 checkVector = worldOffset - _pivotTrans.position;//_camera.transform.position - _pivotTrans.position;

            // How many positions can we check between the destination check point
            // and the origin?
            int iterations = (int)Mathf.Ceil(checkVector.magnitude / _sphereCol.radius);

            Vector3 checkPos = _pivotTrans.position;

            // Check every position between the origin and the destination,
            // with iterations as the subdivider
            for (int i = 0; i <= iterations; i++)
            {
                checkPos = Vector3.Lerp(_pivotTrans.position, worldOffset, (float)i / (float)iterations);

                if (OverlapCol(_pivotTrans.position, checkPos))
                    return;
            }

            // Here marks the start for things that happen if no collision
            // was detected!

            // hasHit serves the purpose of only keeping the camera close to
            // the player when obstructed if the camera has also touched
            // something.
            if (hasHit)
                hasHit = false;

            if (!Mathf.Approximately(Vector3.Distance(_cameraRefLocalPosition, _cameraValues.cameraOffset), 0))
            {
                if (hasHit)
                    hasHit = false;

                float damp = 10 * Time.deltaTime;

                float x = Mathf.SmoothStep(_cameraRefLocalPosition.x, _cameraValues.cameraOffset.x, damp);
                float y = Mathf.SmoothStep(_cameraRefLocalPosition.y, _cameraValues.cameraOffset.y, damp);
                float z = Mathf.SmoothStep(_cameraRefLocalPosition.z, _cameraValues.cameraOffset.z, damp);

                _cameraRefLocalPosition = new Vector3(x, y, z);
            }

            // Get ready for some overly heavy stuff... Frankly I was
            // desperate to get the collision system to work perfectly
            // smooth, and this was the only way I found that never
            // cause the camera to jitter or clip through narrow gaps, 
            // like, ever.
            bool OverlapCol(Vector3 origin, Vector3 position)
            {
                //int r = Physics.OverlapSphereNonAlloc(position, _sphereCol.radius - _sphereCol.contactOffset, _colliders, _layerMask);

                bool didHit = false;

                // SphereCast is sent between the origin and the subposition
                // for smoother detection as the player rotates the camera.
                if (Physics.SphereCast(origin, _sphereCol.radius - _sphereCol.contactOffset, (position - origin).normalized, out _hit, (position - origin).magnitude, _layerMask))
                {
                    if (Physics.CheckSphere(_camera.transform.position, _cameraValues.cameraColliderRadius - _sphereCol.contactOffset, _layerMask) || hasHit)
                    {
                        _camera.transform.position = _hit.point + _hit.normal * _sphereCol.radius;

                        hasHit = true;

                        didHit = true;
                    }
                }
                else
                {
                    // If Cast fails, check if there's collision at the
                    // subposition, and retrieve the penetration instead.
                    int r = Physics.OverlapSphereNonAlloc(position, _sphereCol.radius - _sphereCol.contactOffset, _colliders, _layerMask);

                    for (int i = 0; i < r; i++)
                    {
                        if (Physics.ComputePenetration(_sphereCol, position, _sphereCol.transform.rotation,
                            _colliders[i], _colliders[i].transform.position, _colliders[i].transform.rotation, out Vector3 dir, out float dist))
                        {
                            if (Physics.CheckSphere(_camera.transform.position, _cameraValues.cameraColliderRadius - _sphereCol.contactOffset, _layerMask) || hasHit)
                            {
                                _camera.transform.position = checkPos + dir * dist;

                                hasHit = true;

                                didHit = true;
                            }

                        }
                    }
                }

                return didHit;
            }
        }

        // This is supposed to occur when the camera is focusing on
        // the player through a portal and it needs to snap to its
        // position. 
        private void CrossPortalSnapPosition()
        {
            Transform cloneT = _traversable.PortalClone.transform;

            // We get our coordinates and rotation in the
            // clone's local space and convert them into
            // the player's, and transform into the world
            // space coords of that. Note to self: Use a
            // Matrix4x4 next time :/
            Vector3 camPosInverted = cloneT.InverseTransformPoint(_transform.position);

            Quaternion camRotInverted = Quaternion.Inverse(cloneT.rotation) * _pivotTrans.rotation;

            _transform.position = _focusPoint.TransformPoint(camPosInverted);

            _pivotTrans.rotation = _focusPoint.rotation * camRotInverted;

            FocusOnPortalClone = false;

            // If the camera ends up in another scene, move to it and
            // announce this to delegate subscribers.
            if (gameObject.scene != _focusPoint.gameObject.scene)
            {
                Scene focusScene = _focusPoint.gameObject.scene;
                SceneManager.MoveGameObjectToScene(gameObject, focusScene);
                SceneManager.SetActiveScene(focusScene);
                onActiveSceneChange(focusScene);

                StartCoroutine(ToggleAuraCamera());
            }
        }

        private IEnumerator ToggleAuraCamera()
        {
            Aura2API.AuraCamera auraCamera = _camera.GetComponent<Aura2API.AuraCamera>();
            print(auraCamera);
            if (!auraCamera)
                yield break;

            auraCamera.enabled = false;
            yield return null;
            auraCamera.enabled = true;
        }

        private Vector3 EvaluateShake()
        {
            Vector3 result = Vector3.zero;

            if (_cameraShakes.Count > 0)
            {
                float deltaTime = Time.deltaTime;

                for (int i = 0; i < _cameraShakes.Count; i++)
                {
                    result += _cameraShakes[i].Evaluate(deltaTime);

                    if (_cameraShakes[i].IsFinished())
                        _cameraShakes.RemoveAt(i);
                }
            }

            return result;
        }

        private Vector3 TrueCameraLocalPosition()
        {
            Vector3 result = _cameraRefLocalPosition + EvaluateShake();

            return result;
        }

        /// <summary>
        /// Converts a vector to be relative to the current camera's y-rotation.
        /// </summary>
        /// <param name="vector">The vector, as if the camera's y-rotation was 0.</param>
        public static Vector3 VectorToCameraRotation(Vector3 vector)
        {
            Quaternion q = Quaternion.Euler(0, Main._pivotTrans.localEulerAngles.y, 0);

            if (Main.FocusOnPortalClone)
            {
                q = Quaternion.Inverse(Main._traversable.PortalClone.transform.rotation) * Main._focusPoint.transform.rotation * q;
            }

            return q * vector;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(transform.GetChild(0).TransformPoint(_cameraValues.cameraOffset), 0.05f);

            Gizmos.color = Color.red;

            Gizmos.DrawSphere(transform.GetChild(0).GetChild(0).position, _cameraValues.cameraColliderRadius);

            Gizmos.DrawLine(transform.position, transform.GetChild(0).position);
        }

        private void OnDestroy()
        {
            JPortal.onPortalTraverse -= OnPortalTraverse;
        }

        #region States

        public abstract class CameraState : JStateMachine<CameraState>.IState
        {
            public CameraState(CameraController camController)
            {
                _camControl = camController;
            }

            protected CameraController _camControl;

            public abstract void StateEnter(JStateMachine<CameraState> stateMachine);

            public abstract void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState);
        }

        /// <summary>
        /// The standard camera state where it follows the player.
        /// </summary>
        public class CS_Standard : CameraState, JStateMachine<CameraState>.IUpdateState
        {
            public CS_Standard(CameraController camController) : base(camController) { }

            public override void StateEnter(JStateMachine<CameraState> stateMachine) { }

            public void StateUpdate(JStateMachine<CameraState> stateMachine, float deltaTime)
            {
                Vector3 destination = _camControl.GetDestination();

                // Lerp smoothly to the destination.
                _camControl._transform.position =
                    Vector3.SmoothDamp(_camControl._transform.position, destination,
                    ref _camControl._velocity, _camControl._cameraValues.moveDamp);

                // If the player is on the other side of the portal,
                // remain on the other side of it until we also cross
                // it, or if we hit something or the portal clone disappears.
                if (_camControl.FocusOnPortalClone)
                {
                    float clipPoint = _camControl._camera.nearClipPlane * 1.15f;

                    if (!_camControl._traversable.PortalClone.activeInHierarchy ||
                        _camControl._traversable.InPortal.transform.InverseTransformPoint(_camControl._camera.transform.position).z < clipPoint ||
                        _camControl.hasHit)
                    {
                        _camControl.CrossPortalSnapPosition();
                    }
                }

                // For simplicity, our parent object adapts to the
                // player's gravity so we can just have the child
                // object act as if gravity was always down.
                if (_camControl._gravity != null)
                    _camControl._transform.rotation = 
                        Quaternion.Lerp(_camControl._transform.rotation, 
                        _camControl._gravity.GravityRotation, 
                        _camControl._cameraValues.rotationSmoothValue * deltaTime);

                float camLerp = _camControl._cameraValues.moveDamp;

                // Making sure the camera object moves back to its
                // pivot position smoothly.
                if (!Mathf.Approximately(_camControl._camera.transform.localEulerAngles.magnitude, 0))
                    _camControl._camera.transform.localRotation =
                        Quaternion.Lerp(_camControl._camera.transform.localRotation, Quaternion.identity, camLerp);

                SetMouseRotation();

                _camControl.CheckCollision();
            }

            private void SetMouseRotation()
            {
                Vector2 mouseRotation = GetMouseInputWithSensitivity();

                // Clamp the pitch
                _camControl._xRotation =
                    Mathf.Clamp(_camControl._xRotation - mouseRotation.y,
                    _camControl._cameraValues.pitchConstraints.x,
                    _camControl._cameraValues.pitchConstraints.y);

                _camControl._pivotTrans.localRotation = Quaternion.Euler(_camControl._xRotation, _camControl._pivotTrans.localEulerAngles.y + mouseRotation.x, 0);
            }

            public override void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState) { }
        }

        /// <summary>
        /// The state in which the camera is placed within a fixed view.
        /// The camera may focus on certain things, or remain absolutely still.
        /// </summary>
        public class CS_FixedView : CameraState, JStateMachine<CameraState>.IUpdateState
        {
            public CS_FixedView(CameraController camController, CamFixedViewSettings fixedViewSettings) : base(camController)
            {
                _viewSettings = fixedViewSettings;
            }

            protected CamFixedViewSettings _viewSettings;

            protected Vector3 _currentVelocity;
            private Vector3 _currentVelPivot;
            private Vector3 _currentVelCamera;

            private float lerpDamp = 0;

            public static void OnReturnToStandard()
            {
                CameraController cc = Main;

                CS_FixedView actualState = Main._camStateMachine.CurrentState as CS_FixedView;

                CameraState changeState;

                if (actualState == null || actualState._viewSettings.camExitType == CamTransitionType.Instant)
                {
                    cc.InitializePosition();
                    changeState = new CS_Standard(cc);
                }
                else
                {
                    CamFixedViewSettings settings = actualState._viewSettings;

                    changeState = new CS_MoveBackToPlayer(cc, settings.translationSpeed, settings.smoothTranslation, settings.rotationLerp);
                }

                cc._camStateMachine.ChangeState(changeState);
            }

            public override void StateEnter(JStateMachine<CameraState> stateMachine)
            {
                if (_viewSettings.camEnterType == CamTransitionType.Instant)
                {
                    _camControl._pivotTrans.localPosition = Vector3.zero;
                    _camControl._pivotTrans.localRotation = Quaternion.identity;
                    _camControl._cameraRefLocalPosition = Vector3.zero;
                    _camControl._camera.transform.localRotation = Quaternion.identity;

                    if (_viewSettings.transformDestination)
                    {
                        _camControl._transform.SetPositionAndRotation
                            (_viewSettings.transformDestination.position, _viewSettings.transformDestination.rotation);
                    }
                    else
                    {
                        if (_viewSettings.focusObject)
                            _camControl._transform.LookAt(_viewSettings.focusObject);
                        else
                            _camControl._transform.rotation = _viewSettings.DestRotation;
                    }
                }
            }

            public virtual void StateUpdate(JStateMachine<CameraState> stateMachine, float deltaTime)
            {
                if (_viewSettings.camEnterType == CamTransitionType.Instant)
                {
                    _camControl._transform.position = CamRootDest();

                    _camControl._pivotTrans.localPosition = Vector3.zero;

                    _camControl._pivotTrans.rotation = PivotRotation();

                    _camControl._cameraRefLocalPosition = CamDest();

                    _camControl._camera.transform.rotation = CameraRot();

                    return;
                }

                lerpDamp = Mathf.Clamp(lerpDamp + 8 * deltaTime, 0, 1);

                float lerp = _viewSettings.rotationLerp * deltaTime * lerpDamp;

                _camControl._transform.position = 
                    Vector3.SmoothDamp(_camControl._transform.position, CamRootDest(),
                    ref _currentVelPivot, _viewSettings.smoothTranslation, _viewSettings.translationSpeed);

                _camControl._pivotTrans.localPosition =
                    Vector3.Lerp(_camControl._pivotTrans.localPosition, Vector3.zero, lerp);
                    //ref _currentVelPivot, _viewSettings.smoothTranslation, 100);

                _camControl._pivotTrans.rotation =
                    Quaternion.Lerp(_camControl._pivotTrans.rotation, PivotRotation(), lerp);

                _camControl._cameraRefLocalPosition = 
                    Vector3.Lerp(_camControl._cameraRefLocalPosition, CamDest(), lerp * 8);

                _camControl._camera.transform.rotation =
                    Quaternion.Lerp(_camControl._camera.transform.rotation, CameraRot(), lerp);
            }

            // Movement for the camera root. Go to transformDestination if defined,
            // destPosition otherwise.
            private Vector3 CamRootDest()
            {
                Vector3 result;

                if (_viewSettings.transformDestination)
                {
                    result = _viewSettings.transformDestination.position;
                }
                else
                {
                    result = _viewSettings.destPosition;
                }

                return result;
            }

            private Vector3 CamDest()
            {
                return new Vector3(0, _viewSettings.cameraOffset.y, _viewSettings.cameraOffset.z);
            }

            // Rotation for the pivot child object.
            private Quaternion PivotRotation()
            {
                Quaternion result;

                if (_viewSettings.focusObject)
                {
                    result = _viewSettings.focusObject.rotation;
                    result = Quaternion.Euler(0, result.eulerAngles.y, 0);
                }
                else
                {
                    result = Quaternion.identity;
                }

                result *= _viewSettings.RotationOffset;

                return result;
            }

            // Rotation for the camera object itself.
            private Quaternion CameraRot()
            {
                Quaternion result;

                if (_viewSettings.focusObject)
                {
                    result = Quaternion.LookRotation(_viewSettings.focusObject.position - _camControl._camera.transform.position);
                }
                else
                {
                    result = _viewSettings.DestRotation;
                }

                return result;
            }

            public override void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState)
            {
                if (_viewSettings.camEnterType == CamTransitionType.Instant && nextState is CS_Standard)
                {
                    _camControl.InitializePosition();
                }
            }
        }

        // Pga att CS_FixedView gör konstiga saker med rotation och kör massa onödig kod om man endast vill låsa kameran i dess nuvarande vy så lade jag till detta state / Oliver
        
        public class CS_StaticView : CameraState, JStateMachine<CameraState>.IUpdateState
        {
            public CS_StaticView(CameraController camController, bool parentToPlayer, Vector3 localPos, Quaternion rotation) : base(camController)
            {
                _parentToPlayer = parentToPlayer;
                _localPos = localPos;
            }

            private bool _parentToPlayer;
            private Transform _player;
            private Vector3 _localPos;
            private Quaternion _rotation;

            public override void StateEnter(JStateMachine<CameraState> stateMachine)
            {
                _player = GameManager.Player.transform;
            }

            public override void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState)
            {
                
            }

            public void StateUpdate(JStateMachine<CameraState> stateMachine, float deltaTime)
            {
                Transform focus;
                if (_camControl.FocusOnPortalClone && _camControl._traversable != null)
                    focus = _camControl._traversable.PortalClone.transform;
                else
                    focus = _player;
                
                if (_parentToPlayer)
                {
                    float t = 25f * Time.deltaTime;
                    Vector3 targetPos = focus.localToWorldMatrix.MultiplyPoint3x4(_localPos);
                    _camControl._pivotTrans.rotation = Quaternion.Lerp(_camControl._pivotTrans.rotation, Quaternion.identity, t);
                    _camControl._transform.position = Vector3.Lerp(_camControl._transform.position, new Vector3(10.4f, targetPos.y, targetPos.z), t);
                }

                if (_camControl.FocusOnPortalClone)
                {
                    float clipPoint = _camControl._camera.nearClipPlane;// * 1.15f;

                    if (/*!_camControl._traversable.PortalClone.activeInHierarchy ||*/
                        _camControl._traversable.InPortal.transform.InverseTransformPoint(_camControl._camera.transform.position).z < clipPoint)
                    {
                        _camControl.CrossPortalSnapPosition();
                    }
                }
            }
        }

        // TODO: Merge CutscenedView and FixedView, since ICutscene does
        // just about what CutscenedView was meant to...?

        /// <summary>
        /// The state in which the camera looks at something when a
        /// puzzle or whatever is solved.
        /// </summary>
        public class CS_CutscenedView : CS_FixedView, ICutscene
        {
            public CS_CutscenedView(CameraController camController, CamFixedViewSettings fixedViewSettings, float duration, params System.Action[] onCameraReachDestination) : 
                base(camController, fixedViewSettings)
            {
                _duration = 0;

                _maxDuration = duration;

                _onCameraReachDestination = delegate { };

                foreach (System.Action action in onCameraReachDestination)
                    _onCameraReachDestination += action;
            }

            private static System.Action _onCameraReachDestination = delegate { };

            private const float _triggerThresholdSpeed = 0.2f;

            private bool _hasTriggeredDelegate = false;

            private float _maxDuration;

            private float _duration;

            #region ICutsceneProperties

            public float? CutsceneTime { get => _duration; set => _duration = value ?? 0; }

            public float? CutsceneLength => _maxDuration;

            public bool IsInterruptible => true;

            public System.Action<System.Action[]> OnCutsceneEnd { get; set; }

            #endregion

            public override void StateEnter(JStateMachine<CameraState> stateMachine)
            {
                base.StateEnter(stateMachine);

                _hasTriggeredDelegate = false;
            }

            public override void StateUpdate(JStateMachine<CameraState> stateMachine, float deltaTime)
            {
                if (_duration < _maxDuration)
                {
                    base.StateUpdate(stateMachine, deltaTime);

                    if (!_hasTriggeredDelegate && _currentVelocity.magnitude < _triggerThresholdSpeed)
                    {
                        _hasTriggeredDelegate = true;
                        _onCameraReachDestination();
                    }
                }
            }

            private void OnDurationOver()
            {
                SwitchToStandardCameraView();
            }

            public override void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState)
            {
                _onCameraReachDestination = null;

                base.StateExit(stateMachine, nextState);
            }

            #region ICutsceneMethods

            public void OnCutsceneStart(float startPosition)
            {
                _duration = startPosition;
            }

            // This will keep running even though the camera
            // is currently in an entirely different state.
            public void OnCutsceneUpdate()
            {
                if (CutsceneTime < CutsceneLength)
                {
                    CutsceneTime += Time.deltaTime;

                    if (_duration >= CutsceneLength)
                    {
                        print("steve its over");
                        OnDurationOver();
                    }
                }
                else if (_camControl._camStateMachine.CurrentState.GetType() != typeof(CS_CutscenedView) && 
                    _camControl._camStateMachine.CurrentState.GetType() != typeof(CS_MoveBackToPlayer))
                {
                    print("stopit");
                    StopCutscene();
                }
            }

            public void StopCutscene()
            {
                if (_camControl._camStateMachine.CurrentState.GetType() != typeof(CS_Standard))
                    OnDurationOver();
                OnCutsceneEnd(null);
            }

            public bool OnSkip(float skipProgress) => false;

            #endregion
        }

        /// <summary>
        /// Moves the camera back to the standard player position.
        /// </summary>
        public class CS_MoveBackToPlayer : CameraState, JStateMachine<CameraState>.IUpdateState
        {
            public CS_MoveBackToPlayer(CameraController camController, float translationSpeed, float smoothTranslation, float rotationLerp) : base(camController)
            {
                _translationSpeed = translationSpeed;

                _smoothTranslation = smoothTranslation;

                _rotationLerp = rotationLerp;
            }

            private float _translationSpeed;

            private float _smoothTranslation;

            private float _rotationLerp;

            private Vector3 _currentVelPivot;

            private Vector3 _currentVelocity;

            private float lerpDamp;

            public override void StateEnter(JStateMachine<CameraState> stateMachine) { }

            public void StateUpdate(JStateMachine<CameraState> stateMachine, float deltaTime)
            {
                lerpDamp = Mathf.Clamp(lerpDamp + 8 * deltaTime, 0, 1);

                float lerp = _rotationLerp * deltaTime * lerpDamp;

                _camControl._transform.position = Vector3.SmoothDamp(_camControl._transform.position, _camControl.GetDestination(),
                    ref _currentVelocity, _smoothTranslation, _translationSpeed);
                //Vector3.Lerp(_camControl._transform.position, _camControl.GetDestination(),
                //_rotationLerp * deltaTime * lerpDamp);

                if (_camControl._gravity != null)
                    _camControl._transform.rotation =
                        Quaternion.Lerp(_camControl._transform.rotation,
                        _camControl._gravity.GravityRotation,
                        lerp);

                _camControl._pivotTrans.localPosition = Vector3.SmoothDamp(_camControl._pivotTrans.localPosition, Vector3.zero,
                    ref _currentVelPivot, _smoothTranslation, _translationSpeed);

                _camControl._pivotTrans.localRotation =
                    Quaternion.Lerp(_camControl._pivotTrans.localRotation, GetPivotRotation(),
                        lerp);

                _camControl._cameraRefLocalPosition = Vector3.Lerp(_camControl._cameraRefLocalPosition,
                    _camControl._cameraValues.cameraOffset, 20 * Time.deltaTime);

                _camControl._camera.transform.localRotation =
                    Quaternion.Lerp(_camControl._camera.transform.localRotation, Quaternion.identity,
                        lerp * 8);

                if ((_camControl._transform.position - _camControl.GetDestination()).sqrMagnitude < 0.1f)
                {
                    _camControl._xRotation = 0;
                    _camControl._camStateMachine.ChangeState(new CS_Standard(_camControl));
                }
            }

            private Quaternion GetPivotRotation()
            {
                Quaternion result;

                if (_camControl._gravity != null)
                    result = _camControl._gravity.LocalGravityRotation;
                else
                    result = _camControl._focusPoint?.rotation ?? Quaternion.identity;

                return result;
            }

            public override void StateExit(JStateMachine<CameraState> stateMachine, JStateMachine<CameraState>.IState nextState) { }
        }

        #endregion
    }

    /// <summary>
    /// Settings for a fixed camera view.
    /// </summary>
    [System.Serializable]
    public struct CamFixedViewSettings
    {
        /// <param name="destPosition">The position the camera should move towards.</param>
        /// <param name="smoothTranslation">The smoothness (acceleration/deceleration) of the translation. The lower, the stiffer.</param>
        /// <param name="translationSpeed">The speed at which the camera moves towards the destination. Recommended to be up in the 80-100's.</param>
        /// <param name="destRotation">The rotation the camera should transition into.</param>
        /// <param name="rotationLerp">The smoothness of the transition. Recommended to be between 5-15.</param>
        /// <param name="camEnterType">How the camera should move towards its destination.</param>
        /// <param name="camExitType">How the camera should later move back to the player.</param>
        /// <param name="cameraOffset">Local offset of the camera from the pivot.</param>
        /// <param name="rotationOffset">Local rotation offset of the pivot point from the destination.</param>
        /// <param name="transformDestination">Allows destination to be determined by a transform, though this isn't mandatory. Overrides destPosition/destRotation.</param>
        /// <param name="focusObject">If the camera should focus on an object from the view. Leave null to remain fixed.</param>
        public CamFixedViewSettings(Vector3 destPosition, float smoothTranslation, float translationSpeed, Vector3 destRotation, float rotationLerp, CamTransitionType camEnterType, CamTransitionType camExitType, Vector3 cameraOffset = default, Vector3 rotationOffset = default, Transform transformDestination = null, Transform focusObject = null, bool showBlackBars = true)
        {
            this.destPosition = destPosition;
            this.smoothTranslation = smoothTranslation;
            this.translationSpeed = translationSpeed;
            this.destRotation = destRotation;
            this.rotationLerp = rotationLerp;
            this.transformDestination = transformDestination;
            this.focusObject = focusObject;
            this.camEnterType = camEnterType;
            this.camExitType = camExitType;
            this.cameraOffset = cameraOffset;
            this.rotationOffset = rotationOffset;
            this.showBlackBars = showBlackBars;
        }

        public Transform transformDestination;

        public Transform focusObject;

        [Space, Header("Destination Values (irrelevant if no focusObject is set)")]
        public Vector3 destPosition;

        public Vector3 destRotation;

        [Space, Header("Offset From Pivot Point")]
        [Tooltip("How many units off of the destination the camera itself will be. If (0, 0, 0), the camera will be inside the position.")]
        public Vector3 cameraOffset;

        [Tooltip("How many degrees off the target rotation the camera will be. If (0, 0, 0), the rotation is aligned with the target.")]
        public Vector3 rotationOffset;

        public Quaternion DestRotation { get => Quaternion.Euler(destRotation); }
        public Quaternion RotationOffset { get => Quaternion.Euler(rotationOffset); }

        [Space]
        public float smoothTranslation;

        public float translationSpeed;

        public float rotationLerp;

        public CamTransitionType camEnterType;

        public CamTransitionType camExitType;

        public bool showBlackBars;
    }

    public enum CamTransitionType
    {
        Instant,
        Smooth
    }

    [System.Serializable]
    public class CameraShake
    {
        public CameraShake(CameraShakeValues shakeValues, System.Func<float> customMultiplier)
        {
            cameraShakeValues = shakeValues;
            duration = 0;
            _customMultiplier = customMultiplier;

            Debug.Log(_customMultiplier == null);
        }

        public CameraShake(float shakeMagnitude, float shakeFrequency, float shakeDuration, ShakeFalloffType shakeFalloffType, System.Func<float> customMultiplier)
        {
            cameraShakeValues = new CameraShakeValues(shakeMagnitude, shakeFrequency, shakeDuration, shakeFalloffType);
            duration = 0;
            _customMultiplier = customMultiplier;
        }

        private Vector3 _localDest;

        private Vector3 _currentShake;

        private Vector3 _currentVel;

        private float _destTimer;

        private float duration;

        private System.Func<float> _customMultiplier;

        public CameraShakeValues cameraShakeValues;

        public Vector3 Evaluate(float deltaTime)
        {
            _destTimer -= deltaTime;

            if (cameraShakeValues.shakeDuration != -1)
                duration += deltaTime;

            if (_destTimer <= 0)
            {
                _localDest = 
                    new Vector3(cameraShakeValues.shakeMagnitude * Random.Range(-1f, 1f), cameraShakeValues.shakeMagnitude * Random.Range(-1f, 1f));

                ReevaluateDestTimer();
            }

            _currentShake = Vector3.SmoothDamp(_currentShake, _localDest, ref _currentVel, 0.01f, cameraShakeValues.shakeFrequency);

            debugFalloff = (1 - cameraShakeValues.GetFalloff(duration));
            debugCustomMultiplier = (_customMultiplier?.Invoke() ?? 1);

            return _currentShake * (1 - cameraShakeValues.GetFalloff(duration)) * (_customMultiplier?.Invoke() ?? 1);
        }

        private float debugFalloff;

        private float debugCustomMultiplier;

        public bool IsFinished() =>  cameraShakeValues.GetDurationRatio(duration) >= 1;

        private void ReevaluateDestTimer()
        {
            _destTimer = Time.fixedDeltaTime;
        }
    }

    [System.Serializable]
    public struct CameraShakeValues
    {
        public CameraShakeValues(float shakeMagnitude, float shakeFrequency, float shakeDuration, ShakeFalloffType shakeFalloffType)
        {
            if (shakeDuration < 0)
                shakeDuration = 0;

            this.shakeMagnitude = shakeMagnitude;
            this.shakeFrequency = shakeFrequency;
            this.shakeDuration = shakeDuration;
            this.shakeFalloffType = shakeFalloffType;
        }

        /// <summary>
        /// How far between the shake turning points are.
        /// </summary>
        public float shakeMagnitude;

        /// <summary>
        /// How frequently the shake moves from one
        /// point to another.
        /// </summary>
        public float shakeFrequency;

        /// <summary>
        /// For how long the shake lasts. Set to -1 to
        /// repeat permanently.
        /// </summary>
        public float shakeDuration;

        /// <summary>
        /// How the shake will reduce over time.
        /// </summary>
        public ShakeFalloffType shakeFalloffType;

        public float GetDurationRatio(float currentDuration) => (float)currentDuration / shakeDuration;

        public float GetFalloff(float currentDuration)
        {
            if (shakeDuration <= 0)
                return 0;

            currentDuration = Mathf.Clamp(currentDuration, 0, shakeDuration);

            float result;

            float durationRatio = GetDurationRatio(currentDuration);

            switch (shakeFalloffType)
            {
                default:
                case ShakeFalloffType.Boolean:
                    result = currentDuration >= shakeDuration ? 1 : 0;
                    break;

                case ShakeFalloffType.Linear:
                    result = durationRatio;
                    break;

                case ShakeFalloffType.Smooth:
                    result = Mathf.SmoothStep(0, 1, durationRatio);
                    break;

                case ShakeFalloffType.Squared:
                    result = durationRatio * durationRatio;
                    break;

                case ShakeFalloffType.SquareRoot:
                    result = Mathf.Sqrt(durationRatio);
                    break;
            }

            return result;
        }
    }

    //[System.Serializable]
    public enum ShakeFalloffType
    {
        /// <summary>
        /// Shake formula: shake * (1 - timePassed / duration)
        /// </summary>
        [Tooltip("Shake formula: shake * (1 - timePassed / duration)")]
        Linear,

        /// <summary>
        /// Shake formula: shake * Mathf.SmoothStep(1, 0, 1 - timePassed / duration))
        /// </summary>
        [Tooltip("Shake formula: shake * Mathf.SmoothStep(1, 0, 1 - timePassed / duration))")]
        Smooth,

        /// <summary>
        /// Shake formula: shake * Sqrt(1 - timePassed / duration)
        /// </summary>
        [Tooltip("Shake formula: shake * Mathf.Sqrt(1 - timePassed / duration))")]
        SquareRoot,

        /// <summary>
        /// Shake formula: shake * (1 - timePassed / duration)^2
        /// </summary>
        [Tooltip("Shake formula: shake * Mathf.Pow(1 - timePassed / duration))")]
        Squared,

        /// <summary>
        /// Keeps shaking until time passes the duration.
        /// </summary>
        [Tooltip("Keeps shaking until time passes the duration.")]
        Boolean
    }


}