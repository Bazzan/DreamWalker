using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    [RequireComponent(typeof(SphereCollider), typeof(Light))]
    [InteractOn(Interaction.KeyDown)]
    public class VoidLight : Interactable
    {
        public static System.Action<VoidLight> onPlayerVoidLightEnter = delegate { };

        public static System.Action<VoidLight> onPlayerVoidLightExit = delegate { };

        const float oneNinetieth = 1f / 90f;

        public bool PlayerIsInLight { get; private set; }

        private Transform _transform;

        private RaycastHit _hit;

        // The light bounding box.
        private SphereCollider _sphereCollider;

        private Light _spotLight;

        private Coroutine _weakLightSourcePeriod;

        private Coroutine _waypointTravel;
        
        [SerializeField]
        private bool _volatile;

        [SerializeField]
        public Vector3 _waypoint1;

        [SerializeField]
        public Vector3 _waypoint2;

        [SerializeField]
        private float _waypointSpeed = 3;

        [SerializeField]
        private float _waypointSpeedReturn = 3;

        [SerializeField]
        private float _waitUntilMoveBack = 2;

        [SerializeField]
        private float _waypointSmoothness;

        [SerializeField]
        private Color _gizmoColor;

        [SerializeField]
        private LayerMask _layerMask;

        [SerializeField]
        private bool _weak;
        public bool Weak
        {
            get => _weak;
            set
            {
                if (_weak != value)
                {
                    _weak = value;

                    if (value)
                    {
                        _weakLightSourcePeriod = StartCoroutine(WeakLightSourceOperation());

                        _spotLight.intensity = _referenceIntensity;
                    }
                    else
                    {
                        if (_weakLightSourcePeriod != null)
                        {
                            StopCoroutine(_weakLightSourcePeriod);
                            _weakLightSourcePeriod = null;

                            _spotLight.intensity = _referenceIntensity;
                        }
                    }
                }
            }
        }

        private float _surge;

        [SerializeField]
        private float _onPeriod = 10;

        [SerializeField]
        private float _offPeriod = 5;

        private float _referenceIntensity;

        private Collider[] _colliders;

        [SerializeField]
        private LayerMask _playerMask = 0b0000_0100_0000;

        protected override void Awake()
        {
            base.Awake();

            _colliders = new Collider[4];

            _transform = transform;

            _sphereCollider = GetComponent<SphereCollider>();

            _spotLight = GetComponent<Light>();

            _referenceIntensity = _spotLight.intensity;

            if (_weak)
            {
                _weakLightSourcePeriod = StartCoroutine(WeakLightSourceOperation());

                _spotLight.intensity = _referenceIntensity;
            }
            else
            {
                if (_weakLightSourcePeriod != null)
                {
                    StopCoroutine(_weakLightSourcePeriod);
                    _weakLightSourcePeriod = null;

                    _spotLight.intensity = _referenceIntensity;
                }
            }

            if (_volatile)
                _transform.position = _waypoint1;

            //_waypointTravel = StartCoroutine(TravelWayPoints());
        }

        private void Update()
        {
            CastSphereOnSurface();

            if (!_spotLight.enabled || _spotLight.intensity <= 0)
            {
                _sphereCollider.enabled = false;
                PlayerIsInLight = false;
                onPlayerVoidLightExit(this);
            }
            else if (!_sphereCollider.enabled)
            {
                _sphereCollider.enabled = true;
                if (SafeCheck())
                    onPlayerVoidLightEnter(this);
            }
        }

        private void FixedUpdate()
        {
            if (_weak)
                _surge = Random.Range(0f, 1f);
        }

        public override void Interact()
        {
            if ( _waypointTravel == null)
                _waypointTravel = StartCoroutine(TravelWayPoints());
        }

        public bool SafeCheck()
        {
            int results = Physics.OverlapSphereNonAlloc(_transform.position + _transform.rotation * _sphereCollider.center, _sphereCollider.radius - _sphereCollider.contactOffset * 3, _colliders, _playerMask);

            if (results > 0)
            {
                for (int i = 0; i < results; i++)
                {
                    if (_colliders[i].gameObject == Managers.GameManager.Player)
                    {
                        return true;
                    }
                        
                }
            }

            return false;
        }

        private void CastSphereOnSurface()
        {
            Ray ray = new Ray(_transform.position, _transform.forward);

            float distance;

            // Cast a ray onto the surface, set the center
            // offset of the collider onto the hit point.
            if (Physics.Raycast(ray, out _hit, _spotLight.range, _layerMask))
            {
                distance = _hit.distance;
            }
            else
            {
                distance = _spotLight.range;
            }

            _sphereCollider.center = Vector3.forward * distance;

            _sphereCollider.radius = distance * _spotLight.spotAngle * oneNinetieth * 0.9f;
        }

        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Gizmos.color = _gizmoColor;

            Gizmos.DrawSphere(_waypoint1, 0.2f);

            Gizmos.DrawSphere(_waypoint2, 0.2f);

            Gizmos.DrawLine(_waypoint1, _waypoint2);
        }
        #endif

        // SecondVoidPuzzleManager keeps track of
        // PlayerIsInLight.

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Managers.GameManager.Player)
            {
                onPlayerVoidLightEnter(this);
                PlayerIsInLight = true;
            }
                
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Managers.GameManager.Player)
            {
                onPlayerVoidLightExit(this);
                PlayerIsInLight = false;
            }
        }

        private void OnDisable()
        {
            PlayerIsInLight = false;
        }

        private IEnumerator WeakLightSourceOperation()
        {
            float intensityCache = _spotLight.intensity;

            while (true)
            {
                intensityCache = _referenceIntensity;

                yield return new WaitUntil(() => PlayerIsInLight);

                yield return new WaitForSeconds(_onPeriod);

                if (!PlayerIsInLight)
                    continue;

                yield return StartCoroutine(LightBulbSurgeSequence(true, intensityCache * 0.75f, 1, 10, new Vector2(0.5f, 3)));

                yield return new WaitForSeconds(_offPeriod);

                yield return StartCoroutine(LightBulbSurgeSequence(false, intensityCache * 0.75f, 1, 10, new Vector2(0.5f, 3)));

                _spotLight.intensity = intensityCache;
            }
        }

        private IEnumerator LightBulbSurgeSequence(bool dieAtEnd, float intensity, float duration, float modulationRate, Vector2 surgeVariance)
        {
            float mult = dieAtEnd ? 1 : 0;

            while (duration > 0)
            {
                yield return new WaitForEndOfFrame();

                duration -= Time.deltaTime;

                if (mult != 1)
                    mult = Mathf.MoveTowards(mult, 1, 10 * Time.deltaTime);

                _spotLight.intensity = mult * intensity * 
                    Mathf.Sin(Time.time * Mathf.Lerp(surgeVariance.x, surgeVariance.y, _surge) * modulationRate);
            }

            float target = dieAtEnd ? 0 : intensity;

            while (_spotLight.intensity != target)
            {
                yield return new WaitForEndOfFrame();

                _spotLight.intensity = Mathf.MoveTowards(_spotLight.intensity, target, 25 * Time.deltaTime);
            }
        }

        private IEnumerator TravelWayPoints()
        {
            //do
            //{
                yield return new WaitForSeconds(0.5f);

                yield return new WaitUntil(() => _volatile);
                print("price " + gameObject.name);
                yield return StartCoroutine(Travel(_waypoint2, _waypointSpeed));

                yield return new WaitForSeconds(_waitUntilMoveBack);

                yield return new WaitUntil(() => _volatile);

                yield return StartCoroutine(Travel(_waypoint1, _waypointSpeedReturn));

            _waypointTravel = null;
            //}
            //while (_playerIsInLight);
        }

        IEnumerator Travel(Vector3 waypoint, float speed)
        {
            Vector3 velocity = Vector3.zero;

            while (Vector3.SqrMagnitude(waypoint - _transform.position) > 0.001f)
            {
                if (!Managers.CutsceneManager.IsInCutscene)
                    _transform.position = Vector3.SmoothDamp
                            (_transform.position, waypoint, ref velocity, _waypointSmoothness, speed);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
