using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class that handles GameCharacter collision.
/// </summary>
[Serializable]
public class CollisionModule
{
    public CollisionModule(CapsuleCollider capsuleCollider)
    {
        SetCapsuleAndTransform(capsuleCollider);
    }

    public void SetCapsuleAndTransform(CapsuleCollider capsuleCollider)
    {
        _cc = capsuleCollider;
        _transform = _cc.transform;
    }

    private Transform _transform;

    private CapsuleCollider _cc;

    private Collider[] _collisions = new Collider[5];

    // The position on the last frame.
    private Vector3 _previousPosition;

    // gravity maybe? just puttin this here.
    public Vector3 up => Vector3.up;

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

    private Action<ColSide, ColHit> _onCollisionEnter = delegate { };
    private Action<ColSide, ColHit> _onCollisionStay = delegate { };
    private Action<ColSide> _onCollisionExit = delegate { };

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
    public Vector3 PCapCenter => _previousPosition + _transform.rotation * new Vector3(_cc.center.x * _transform.localScale.x, _cc.center.y * _transform.localScale.y, _cc.center.z * _transform.localScale.z);

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
    public Vector3 CapsuleBottomSpherePoint => CapCenter + _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

    /// <summary>
    /// The origin of the bottom sphere in the capsule at the previous position.
    /// </summary>
    public Vector3 PCapsuleBottomSpherePoint => PCapCenter + _transform.up * (ScaledCapsuleHeight * 0.5f - ScaledCapsuleRadius);

    /// <summary>
    /// The vector between the current and last position.
    /// </summary>
    public Vector3 DeltaVector => _transform.position - _previousPosition;

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

    // Update-related stuff and methods.
    // {
    public void CollisionUpdate()
    {
        DetectCollision();

        _previousPosition = _transform.position;
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
            for (int i = 0; i < hits; i++)
            {
                Transform colTrans = _collisions[i].transform;

                if (Physics.ComputePenetration(_cc, detectedPos, _transform.rotation,
                    _collisions[i], colTrans.position, colTrans.rotation, out Vector3 dir, out float dist))
                {
                    float angle = Vector3.Angle(up, dir);

                    ColHit hit = new ColHit(_collisions[i], angle, dir, dist);

                    InvokeCollisionEvents(angle, hit);

                    // Extra offset added to properly ride down walls
                    // when falling. May have bad side effects on ground.
                    if (angle >= SlopeLimit)
                        dist += _cc.contactOffset;

                    _transform.position = detectedPos + dir * dist;
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
        Invoke(ref _onGround, ref _pOnGround, ColSide.Ground);

        Invoke(ref _onWall, ref _pOnWall, ColSide.Wall);

        Invoke(ref _onCeiling, ref _pOnCeiling, ColSide.Ceiling);

        void Invoke(ref bool cHit, ref bool pCHit, ColSide colSide)
        {
            switch (colSide)
            {
                case ColSide.Ground:
                    cHit = angle < SlopeLimit;
                    break;

                case ColSide.Wall:
                    Vector2 wallLimit = WallLimit;
                    cHit = angle >= wallLimit.x && angle <= wallLimit.y;
                    break;

                case ColSide.Ceiling:
                    cHit = angle >= CeilingLimit;
                    break;
            }

            if (cHit && !pCHit)
                _onCollisionEnter(colSide, hit);
            else if (cHit && pCHit)
                _onCollisionStay(colSide, hit);
            else if (!cHit && pCHit)
                _onCollisionExit(colSide);
            
        }
    }

    // If nothing was collided with, reset 
    // all callbacks separately.
    private void ResetCollisionCallbacks()
    {
        Invoke(ref _onGround, ref _pOnGround, ColSide.Ground);

        Invoke(ref _onWall, ref _pOnWall, ColSide.Wall);

        Invoke(ref _onCeiling, ref _pOnCeiling, ColSide.Ceiling);

        void Invoke(ref bool hit, ref bool pHit, ColSide colSide)
        {
            if (hit || pHit)
                _onCollisionExit(colSide);
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

        Vector3 sphereOffset = Vector3.up * ScaledCapsuleRadius;

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

            resultCount = Physics.OverlapCapsuleNonAlloc(currentPoint1, currentPoint2, ScaledCapsuleRadius, results, layerMask);

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

                        if (angle > SlopeLimit)
                        {
                            detectedPosition = currentPoint1 - sphereOffset;

                            float absAngle = Mathf.Abs(angle);

                            detectedPosition.y = absAngle >= 135 + Mathf.Epsilon ? detectedPosition.y : _transform.position.y;

                            return resultCount;
                        }
                    }
                }
            }
        }

        detectedPosition = _transform.position;

        return 0;
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
}

public enum ColSide
{
    Ground,
    Wall,
    Ceiling,
    Nothing
}