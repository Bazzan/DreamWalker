using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Modules
{
    /// <summary>
    /// The class that handles GameCharacter collision.
    /// </summary>
    [Serializable]
    public class CollisionModule
    {
        public CollisionModule(CapsuleCollider capsuleCollider)
        {
            Initialize(capsuleCollider);
        }

        public void Initialize(CapsuleCollider capsuleCollider)
        {
            _cc = capsuleCollider;

            _character = _cc.GetComponent<GameCharacter>();

            _transform = _cc.transform;

            _previousPosition = _transform.position;

            _collisions = new Collider[6];

            _onCollisionEnter = delegate { };
            _onCollisionStay = delegate { };
            _onCollisionExit = delegate { };
        }

        public void Initialize(CapsuleCollider capsuleCollider, LayerMask layerMask)
        {
            _cc = capsuleCollider;

            _character = _cc.GetComponent<GameCharacter>();

            _transform = _cc.transform;

            _previousPosition = _transform.position;

            _collisions = new Collider[6];

            _layerMask = layerMask;

            _onCollisionEnter = delegate { };
            _onCollisionStay = delegate { };
            _onCollisionExit = delegate { };
        }

        #region debug
        [SerializeField]
        private bool _detectGroundCollision = true;

        [SerializeField]
        private bool _detectPenetrationCollision = true;
        #endregion

        private Transform _transform;

        private CapsuleCollider _cc;

        private Collider[] _collisions = new Collider[5];

        private Collider _groundCollision;

        private RaycastHit _hit;

        private GameCharacter _character;

        // The position at the last frame.
        private Vector3 _previousPosition;

        // The position at the last frame in platform
        // space. Updates only when standing on one.
        private Vector3 _platformPreviousPosition;

        /// <summary>
        /// Returns _platformPreviousPosition when on a platform, otherwise
        /// _previousPosition. When set, assigns InverseTransformPoint of
        /// the value to _platformPreviousPosition if on a platform, or the
        /// value as-is to _previousPosition otherwise.
        /// </summary>
        private Vector3 PreviousPosition
        {
            get => _transform.parent ? _transform.parent.TransformPoint(_platformPreviousPosition) : _previousPosition;

            set
            {
                if (_transform.parent)
                    _platformPreviousPosition =
                        _transform.parent.InverseTransformPoint(value);
                else
                    _previousPosition = value;
            }
        }

        // gravity maybe? just puttin this here.
        public Vector3 up => _transform.up; //Vector3.up;

        /// <summary>
        /// The height of the capsule, scaled with Transform.localScale.
        /// </summary>
        public float ScaledCapsuleHeight => _cc.height * _transform.localScale.y;

        /// <summary>
        /// The height of the capsule, scaled with Transform.localScale.
        /// </summary>
        public float ScaledCapsuleRadius
        {
            get
            {
                float scale;

                float localScaleX = _transform.localScale.x;
                float localScaleZ = _transform.localScale.z;

                // For some reason, if one coordinate is smaller than the other,
                // the radius will stop scaling, so it has to sync with the larger
                // coordinate.
                if (localScaleX >= localScaleZ)
                    scale = localScaleX;
                else
                    scale = localScaleZ;

                return _cc.radius * scale;
            }
        }

        private Action<ColSide, ColHit> _onCollisionEnter = (colSide, colHit) => { };
        private Action<ColSide, ColHit> _onCollisionStay = (colSide, colHit) => { };
        private Action<ColSide> _onCollisionExit = (colSide) => { };

        private bool _onGround;
        private bool _pOnGround;
        public bool OnGround => _onGround;
        public bool POnGround => _pOnGround;

        private bool _onWall;
        private bool _pOnWall;
        public bool OnWall => _onWall;
        public bool POnWall => _pOnWall;

        private bool _onCeiling;
        private bool _pOnCeiling;
        public bool OnCeiling => _onCeiling;
        public bool POnCeiling => _pOnCeiling;

        /// <summary>
        /// Returns the center of the capsule.
        /// </summary>
        public Vector3 CapCenter => _transform.position + _transform.rotation * new Vector3(_cc.center.x * _transform.localScale.x, _cc.center.y * _transform.localScale.y, _cc.center.z * _transform.localScale.z);

        /// <summary>
        /// Returns the center of the capsule at the previous position.
        /// </summary>
        public Vector3 PCapCenter => PreviousPosition + _transform.rotation * new Vector3(_cc.center.x * _transform.localScale.x, _cc.center.y * _transform.localScale.y, _cc.center.z * _transform.localScale.z);

        /// <summary>
        /// The tip top of the capsule. For CapsuleCasts,
        /// please use CapsuleTopSpherePoint instead.
        /// </summary>
        public Vector3 CapsuleTopPoint => CapCenter + _transform.up * ScaledCapsuleHeight * 0.5f;

        /// <summary>
        /// The tip top of the capsule at the previous position. For CapsuleCasts,
        /// please use PCapsuleTopSpherePoint instead.
        /// </summary>
        public Vector3 PCapsuleTopPoint => PCapCenter + _transform.up * ScaledCapsuleHeight * 0.5f;

        /// <summary>
        /// The origin of the top sphere in the capsule.
        /// </summary>
        public Vector3 CapsuleTopSpherePoint => CapCenter + _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

        /// <summary>
        /// The origin of the top sphere in the capsule at the previous position.
        /// </summary>
        public Vector3 PCapsuleTopSpherePoint => PCapCenter + _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

        /// <summary>
        /// The tip bottom of the capsule. For CapsuleCasts,
        /// please use CapsuleBottomSpherePoint instead.
        /// </summary>
        public Vector3 CapsuleBottomPoint => CapCenter - _transform.up * ScaledCapsuleHeight * 0.5f;

        /// <summary>
        /// The tip bottom of the capsule at the previous position. For CapsuleCasts,
        /// please use PCapsuleBottomSpherePoint instead.
        /// </summary>
        public Vector3 PCapsuleBottomPoint => PCapCenter - _transform.up * ScaledCapsuleHeight * 0.5f;

        /// <summary>
        /// The origin of the bottom sphere in the capsule.
        /// </summary>
        public Vector3 CapsuleBottomSpherePoint => CapCenter - _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

        /// <summary>
        /// The origin of the bottom sphere in the capsule at the previous position.
        /// </summary>
        public Vector3 PCapsuleBottomSpherePoint => PCapCenter - _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

        /// <summary>
        /// The vector between the current and last position.
        /// </summary>
        public Vector3 DeltaVector
        {
            get
            {
                return _transform.position - PreviousPosition;
            }
        }
            
        public Vector3 InverseDeltaVector => -_transform.InverseTransformPoint(/*_previousPosition*/PreviousPosition);

        public Vector3 Normal { get; private set; }

        public float NormalAngle => Vector3.Angle(Normal, up);

        // Inspector Properties:

        [SerializeField, Tooltip("How steep may a slope be before ground collision " +
            "callbacks stop evoking?")]
        private float _slopeLimit = 60;

        public float SlopeLimit => _slopeLimit;

        [SerializeField, Tooltip("At this angle value, collisions start invoking ceiling " +
            "hit callbacks.")]
        private float _ceilingLimit = 150;

        public float CeilingLimit => _ceilingLimit;

        public Vector2 WallLimit => new Vector2(SlopeLimit, CeilingLimit);

        [SerializeField]
        private LayerMask _layerMask;

        [SerializeField, Tooltip("The layer mask for when colliding with GameObjects with the ITriggerXXX interfaces.")]
        private LayerMask _triggerMask;

        /// <summary>
        /// The update function for the collision module. Put this in your Update
        /// method.
        /// </summary>
        public void CollisionUpdate(bool checkCollisionThisFrame)
        {
            if (checkCollisionThisFrame)
            {
                if (_detectPenetrationCollision)
                    DetectCollision();

                if (_detectGroundCollision)
                    GroundCheck(0, out bool didHit);
            }

            _pOnGround = _onGround;

            _pOnWall = _onWall;

            _pOnCeiling = _onCeiling;

            PreviousPosition = _transform.position;
        }

        public void Teleport(Vector3 newPosition)
        {
            _transform.position = newPosition;
            PreviousPosition = newPosition;
        }

        /// <summary>
        /// The collision detection itself. Performs the recursive overlap check,
        /// snaps the transform back to the detected position, and lets Physics.ComputePenetration
        /// do the rest of the work from there.
        /// </summary>
        private void DetectCollision()
        {
            int hits = DeltaCheckOverlapCapsule(_collisions, out Vector3 detectedPos, _layerMask, true);

            if (hits > 0)
            {
                //Debug.Log(hits);

                bool hasPushedToDetectedPos = false;

                for (int i = 0; i < hits; i++)
                {
                    Transform colTrans = _collisions[i].transform;

                    if (Physics.ComputePenetration(_cc, detectedPos, _transform.rotation,
                        _collisions[i], colTrans.position, colTrans.rotation, out Vector3 dir, out float dist))
                    {
                        float angle = Vector3.Angle(up, dir);

                        ColHit hit = new ColHit(_collisions[i], angle, dir, dist);

                        // For context: Since a linecast is already taking
                        // care of ground collision, we don't want this 
                        // check to interact with groundable surfaces.
                        if ((OnGround && (angle > SlopeLimit) || !OnGround && !POnGround) && 
                            (angle > SlopeLimit || !OnGround || dist >= ScaledCapsuleRadius * 0.375f))
                        {
                            //Not particularly sure if this does anything,
                            //but I'm just trying to prevent characters from 
                            //occassionally shaking upon touching a wall...
                            if (Mathf.Approximately(dir.x, 0))
                                dir.x = 0;

                            if (Mathf.Approximately(dir.y, 0))
                                dir.y = 0;

                            if (Mathf.Approximately(dir.z, 0))
                                dir.z = 0;

                            // Adding slight contact offset so we don't get
                            // stuck in surfaces we collide with.
                            Vector3 pushoutVector = dir * (dist + _cc.contactOffset * 0.01f);

                            if (angle > 90 && (angle < -90 || angle < 270) && (OnGround || POnGround))
                            {
                                // Rotating the push vector to prevent slanted
                                // walls/ceilings from pushing us through the
                                // ground when running against them.

                                Vector3 pushoutYLessVector = _transform.InverseTransformVector(pushoutVector);
                                pushoutYLessVector = Quaternion.FromToRotation(pushoutYLessVector.normalized, new Vector3(pushoutYLessVector.x, 0, pushoutYLessVector.z)) * pushoutYLessVector;

                                pushoutVector = _transform.TransformVector(pushoutYLessVector);
                            }

                            InvokeCollisionEvents(angle, hit);

                            if (hasPushedToDetectedPos)
                            {
                                _transform.position = detectedPos;
                                hasPushedToDetectedPos = false;
                            }

                            _transform.position += pushoutVector;
                        }
                    }
                }
            }
            else
            {
                ResetCollisionCallbacks();
            }
        }

        private void InvokeCollisionEvents(float angle, ColHit hit)
        {
            //Invoke(ref _onGround, ref _pOnGround, ColSide.Ground);

            Invoke(ref _onWall, ref _pOnWall, ColSide.Wall);

            Invoke(ref _onCeiling, ref _pOnCeiling, ColSide.Ceiling);

            // We're just comparing the collision states of
            // the current and previous frames, and executing
            // events based on the resulting pattern.
            void Invoke(ref bool cHit, ref bool pCHit, ColSide colSide)
            {
                switch (colSide)
                {
                    //case ColSide.Ground:
                    //    cHit = angle < SlopeLimit;
                    //    break;

                    case ColSide.Wall:
                        Vector2 wallLimit = WallLimit;
                        cHit = angle >= wallLimit.x && angle <= wallLimit.y;
                        break;

                    case ColSide.Ceiling:
                        cHit = angle >= CeilingLimit;
                        break;
                }

                if (cHit && !pCHit)
                    _onCollisionEnter?.Invoke(colSide, hit);
                else if (cHit && pCHit)
                    _onCollisionStay?.Invoke(colSide, hit);
                else if (!cHit && pCHit)
                    _onCollisionExit?.Invoke(colSide);

            }
        }

        // If nothing was collided with, reset 
        // all callbacks separately.
        private void ResetCollisionCallbacks()
        {
            //Invoke(ref _onGround, ref _pOnGround, ColSide.Ground);

            Invoke(ref _onWall, ref _pOnWall, ColSide.Wall);

            Invoke(ref _onCeiling, ref _pOnCeiling, ColSide.Ceiling);

            void Invoke(ref bool hit, ref bool pHit, ColSide colSide)
            {
                if (hit)
                {
                    hit = false;
                    _onCollisionExit(colSide);
                }
            }
        }
        // }

        /// <summary>
        /// Returns the amount of capsule radii that fit between the
        /// character's current and last position. Primarily used for testing
        /// sweep collision (like quarter steps in Mario 64, except it's every coverable step).
        /// </summary>
        private int GetDeltaCheckIterations()
        {
            float radius = ScaledCapsuleRadius;
            return radius != 0 ? (int)Mathf.Ceil(DeltaVector.magnitude / radius) : 0;
        }

        /// <summary>
        /// Recursively performs OverlapCapsule between the current and last
        /// positions, and returns the first position where collision is overlapped,
        /// as well as with what colliders.
        /// </summary>
        // Also, this was imported from another project of mine, so please excuse the shitty commenting,
        // or even coding in the worst case scenario!
        private int DeltaCheckOverlapCapsule(Collider[] results, out Vector3 detectedPosition, int layerMask, bool pushoutSlopes)
        {
            // How many substeps can be carried out between the current
            // and last position of the object?
            int iterations = GetDeltaCheckIterations();

            int resultCount = 0;

            Vector3 sphereOffset = up * ScaledCapsuleRadius;

            Vector3 pPoint1 = PCapsuleBottomSpherePoint;
            Vector3 pPoint2 = PCapsuleTopSpherePoint;

            Vector3 point1 = CapsuleBottomSpherePoint;
            Vector3 point2 = CapsuleTopSpherePoint;

            Vector3 currentPoint1 = point1;
            Vector3 currentPoint2 = point2;

            

            // Checks for collision between the current and last position.
            // Done by carrying out OverlapCapsules at every substep 
            // (1 substep = 1 radius length).
            for (int i = 0; i <= iterations; i++)
            {
                float t = iterations > 0 ? (float)i / iterations : 0;
                currentPoint1 = Vector3.Lerp(pPoint1, point1, t);
                currentPoint2 = Vector3.Lerp(pPoint2, point2, t);

                resultCount = Physics.OverlapCapsuleNonAlloc(currentPoint1, currentPoint2, ScaledCapsuleRadius /*- _cc.contactOffset * 0.01f*/, results, layerMask);

                if (resultCount > 0)
                {
                    for (int j = 0; j < resultCount; j++)
                    {
                        Transform colTrans = results[j].transform;//_ccTrans.position

                        // ComputePenetration is only here to help determine
                        // the angle of the surface hit (since slope surfaces
                        // aren't allowed to be returned if grounded).
                        if (Physics.ComputePenetration(_cc, currentPoint1 - sphereOffset, _transform.rotation,
                            results[j], colTrans.position, colTrans.rotation, out Vector3 dir, out float dist))
                        {
                            float angle = Vector3.Angle(up, dir);

                            if (angle > SlopeLimit || pushoutSlopes || (OnGround && _collisions[i] != _groundCollision))
                            {
                                float absAngle = Mathf.Abs(angle);

                                detectedPosition = _transform.InverseTransformPoint(currentPoint1 - sphereOffset);

                                detectedPosition.y = absAngle >= 135 + Mathf.Epsilon ? detectedPosition.y : 0;

                                detectedPosition = _transform.TransformPoint(detectedPosition);

                                return resultCount;
                            }
                        }
                    }
                }
            }

            detectedPosition = _transform.position;

            return 0;
        }

        private void GroundCheck(float checkOffset, out bool didHitGround)
        {
            Vector3 start;
            Vector3 end;

            // The main raycast function.
            void GroundCheckCast(out bool didHit)
            {
                // Set the points between which the linecast runs.
                start = new Vector3(CapsuleBottomPoint.x, CapsuleBottomSpherePoint.y, CapsuleBottomPoint.z);
                end = new Vector3(CapsuleBottomPoint.x, CapsuleBottomPoint.y, CapsuleBottomPoint.z);

                // Extending ground check when on ground 
                // for better slope detection
                if (!OnGround)
                {
                    end += up * (Mathf.Clamp(InverseDeltaVector.y, -Mathf.Infinity, -0.2f));//1f
                }
                else
                {
                    end += -up * 0.15f;
                }

                Debug.DrawLine(start, end, Color.red);

                bool linecastDidHit = Physics.Linecast(start, end, out _hit, _layerMask);

                float slopeAngle = Vector3.Angle(_hit.normal, up);

                bool isSlope = Vector3.Angle(_hit.normal, up) < SlopeLimit;

                bool isStillOrFalling = _character.Speed.y <= 0;

                // Over-complicating it a bit here because 90-degree walls
                // can fuck up raycasts pretty bad, especially on stairs.
                if (linecastDidHit && isStillOrFalling)
                {
                    if (isSlope)
                        didHit = true;
                    else
                        didHit = slopeAngle > 89 ? OnGround : false;
                }
                else
                    didHit = false;
            }

            GroundCheckCast(out didHitGround);

            // If ground collision was successful, then snap to its surface.
            // If the object isn't static, treat it as a moving platform by
            // becoming its child object.
            if (didHitGround)
            {
                if (OnGround || PCapsuleBottomPoint.y >= CapsuleBottomPoint.y)
                {
                    Vector3 invertedHitPoint = _transform.InverseTransformPoint(_hit.point);

                    _transform.position = _transform.TransformPoint(0, invertedHitPoint.y, 0);

                    if (!_pOnGround || _groundCollision != _hit.collider)
                    {
                        _groundCollision = _hit.collider;

                        // If a collider isn't static, it's treated as a moving
                        // platform. Moving platforms must have a special hierarchy
                        // structure to work properly.
                        if (!_groundCollision.gameObject.isStatic)
                        {
                            Transform parent = _hit.collider.transform.parent;

                            //PreviousPosition = Vector3.zero;

                            // Characters must be parented to the root object of
                            // moving platforms, since collision and mesh renderers
                            // change in scale, which will affect the characters
                            // and is not allowed.
                            if (parent && parent.CompareTag("Platform"))
                            {
                                _transform.SetParent(_hit.collider.transform.parent, true);
                            }
                            else
                            {
#if UNITY_EDITOR
                                string warning = "The collider landed on is non-static, but ";

                                if (parent)
                                    warning += "its parent object does not have a \"Platform\" tag. " +
                                        "Please make sure that all groundable platform are tagged \"Platform\".";
                                else
                                    warning += "does not have a parent object. Please make sure " +
                                        "that all groundable platforms have a parent object, that " +
                                        "are also tagged \"Platform\". Otherwise, make this collider static.";

                                Debug.LogWarning(warning);
#endif
                            }
                        }
                    }
                }
            }
            else if (_transform.parent)
            {
                _transform.parent = null;
            }

            Normal = _hit.normal;

            ColHit colHit = new ColHit(_hit.collider, Vector3.Angle(up, _hit.normal), _hit.normal, 0);

            // We do ground collision separately from wall/ceiling
            // collision because we want the bottom point to always
            // be on the ground - which it can't if we use push the
            // entire capsule out.
            _onGround = didHitGround;
            if (_onGround && !_pOnGround)
            {
                _onCollisionEnter(ColSide.Ground, colHit);
            }
            else if (_onGround && _pOnGround)
            {
                _onCollisionStay(ColSide.Ground, colHit);
            }
            else if (!_onGround && _pOnGround)
            {
                _onCollisionExit(ColSide.Ground);
            }
        }

        /// <summary>
        /// Struct that contains information about collision hits.
        /// </summary>
        public struct ColHit
        {
            public ColHit(Collider collider, float angle, Vector3 normal, float penetration)
            {
                this.collider = collider;
                this.angle = angle;
                this.normal = normal;
                this.penetration = penetration;
            }

            public Collider collider;
            public float angle;
            public Vector3 normal;
            public float penetration;
        }

        public void AssignCollisionEnterCallback(params Action<ColSide, ColHit>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionEnter += actions[i];
        }

        public void UnassignCollisionEnterCallback(params Action<ColSide, ColHit>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionEnter -= actions[i];
        }

        public void AssignCollisionStayCallback(params Action<ColSide, ColHit>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionStay += actions[i];
        }

        public void UnassignCollisionStayCallback(params Action<ColSide, ColHit>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionStay -= actions[i];
        }

        public void AssignCollisionExitCallback(params Action<ColSide>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionExit += actions[i];
        }

        public void UnassignCollisionExitCallback(params Action<ColSide>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
                _onCollisionExit -= actions[i];
        }
    }

    
}


public enum ColSide
{
    Ground,
    Wall,
    Ceiling,
    Nothing
}