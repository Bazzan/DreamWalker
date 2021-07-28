using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GP2_Team7.Objects
{
    using Managers;
    using Cameras;
    using Scriptables;
    [DefaultExecutionOrder(50)]
    public class JPortal : MonoBehaviour
    {
        private readonly Quaternion _oneEighty = Quaternion.Euler(0, 180, 0);

        public static Action<IPortalTraversable> onPortalTraverse = delegate { };

        public static Action<bool> onAnyPortalExistenceStatus = delegate { };

        private static List<JPortal> _allJPortals = new List<JPortal>();

        public static bool JPortalsExist => _allJPortals?.Count > 0;

        #region Variables

        private int frameDelay = 5;

        private Transform _transform;

        private RenderTexture _renderTex;

        private Camera _portalCamera;

        private RenderTextureDescriptor _descriptor;

        private MeshRenderer _portalRenderer;

        private MaterialPropertyBlock _mpb;

        private bool _isVisible = false;

        public List<IPortalTraversable> _traversablesInPortal = new List<IPortalTraversable>();

        private Scene _crossScene;

        private Coroutine _loadSceneRoutine;

        private Coroutine _unloadSceneRoutine;

        private float _completion = 0;

        #endregion

        #region InspectorValues

        [SerializeField]
        private bool _updateCameraInEditMode = true;

        [SerializeField, Tooltip("The exit portal that you end up through, if it's in the scene.")]
        private JPortal _destinationPortal;

        private JPortal _crossSceneDestinationPortal;

        public JPortal DestinationPortal
        {
            get
            {
                JPortal result;
                if (!_sceneAddress)
                {
                    result = _destinationPortal;
                }
                else
                {
                    result = _crossSceneDestinationPortal;
                }

                if (!result)
                {
                    for (int i = 0; i < _traversablesInPortal.Count; i++)
                    {
                        DisableTraversableClone(_traversablesInPortal[i]);

                        _traversablesInPortal.RemoveAt(i);

                        //if (_traversablesInPortal.Count > 0)
                        i--;
                    }
                }

                return result;
            }

            private set
            {
                if (!_sceneAddress)
                    _destinationPortal = value;
                else
                    _crossSceneDestinationPortal = value;

                Init();
            }
        }

        [SerializeField, Tooltip("Contains destination information if this portal is " +
            "supposed to take you to another scene. If it doesn't, leave blank. Overrides Destination Portal.")]
        private PortalSceneAddress _sceneAddress;

        [SerializeField]
        private float _clipOffset = 0.5f;

        #endregion

        private void Awake()
        {
            _transform = transform;

            _allJPortals.Add(this);

            if (_allJPortals.Count > 0)
                onAnyPortalExistenceStatus(true);

            _portalRenderer = _transform.GetChild(0).GetComponent<MeshRenderer>();

            _portalCamera = _transform.GetComponentInChildren<Camera>();

            _portalCamera.forceIntoRenderTexture = true;

            _portalCamera.enabled = false;

            SetVisible(true);
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (!_isVisible || !DestinationPortal)
                return;

            SetRenderTexture();

            CameraUpdate();

            UpdateTraversables();
        }

        private void LateUpdate()
        {
            if (!DestinationPortal)
                return;


        }

        private void Init()
        {
            print("dio brandredskap");
            if (DestinationPortal)
            {
                print("dio brando");

                DestinationPortal.gameObject.SetActive(true);

                if (DestinationPortal._portalCamera.targetTexture != null)
                    DestinationPortal._portalCamera.targetTexture.Release();

                //SetDestinationPortal(DestinationPortal);

                _portalRenderer.material.SetTexture("_MainTex", _renderTex);
                _portalRenderer.material.mainTexture = _renderTex;
            }
        }

        private void OnEnable()
        {
            if (_sceneAddress && _sceneAddress.SceneName != string.Empty)
            {
                if (DestinationPortal && DestinationPortal._unloadSceneRoutine != null)
                {
                    gameObject.SetActive(false);
                    return;
                }

                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;

                _loadSceneRoutine = StartCoroutine(LoadSceneAndPortal((c) =>
                {
                    _completion = c;
                    print(_completion);
                }));
                
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            if (_crossSceneDestinationPortal && CameraController.Main.gameObject.scene == gameObject.scene)
                _crossSceneDestinationPortal.OnDestinationPortalDeactivate();

            _allJPortals.Remove(this);

            if (_allJPortals.Count <= 0)
                onAnyPortalExistenceStatus(false);

            foreach (IPortalTraversable traversable in _traversablesInPortal)
            {
                if (traversable != null && _traversablesInPortal.Contains(traversable))
                {
                    DisableTraversableClone(traversable);
                }
            }
            
            _traversablesInPortal.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode == LoadSceneMode.Additive && scene.name.CompareTo(_sceneAddress.SceneName) == 0)
            {
                //DestinationPortal = _allJPortals.Find((jp) => jp.name.CompareTo(_sceneAddress.PortalGameObjectName) == 0);

                GameObject[] gObjects = scene.GetRootGameObjects();

                foreach (GameObject go in gObjects)
                {
                    JPortal jp = go.GetComponent<JPortal>();
                    if (jp && jp.name.CompareTo(_sceneAddress.PortalGameObjectName) == 0) //
                    {
                        DestinationPortal = jp;
                        DestinationPortal.gameObject.SetActive(true);
                        break;
                    }
                        
                }

                if (!DestinationPortal)
                    print("theres no fucking portal");

                DestinationPortal._crossScene = gameObject.scene;

                _crossScene = scene;

                DestinationPortal.DestinationPortal = this;
                DestinationPortal._crossSceneDestinationPortal = this;
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene == _crossScene)
            {
                _crossSceneDestinationPortal = null;
            }
        }

        private void UpdateTraversables()
        {
            for (int i = 0; i < _traversablesInPortal.Count; i++)
            {
                Transform travTrans = _traversablesInPortal[i].PortalModel.transform.parent;

                Matrix4x4 destLocal2WorldMatrix = Matrix4x4.TRS
                    (DestinationPortal._transform.localPosition,
                    DestinationPortal._transform.localRotation * _oneEighty,
                    DestinationPortal._transform.localScale);//_destinationPortal._transform.localToWorldMatrix;

                Matrix4x4 m = destLocal2WorldMatrix * _transform.worldToLocalMatrix * travTrans.localToWorldMatrix;

                if (_transform.InverseTransformPoint(travTrans.position).z < 0)
                {
                    Vector3 previousPos = travTrans.position;
                    Quaternion previousRot = travTrans.rotation;

                    if (DestinationPortal == _crossSceneDestinationPortal)
                    {
                        SceneManager.MoveGameObjectToScene(travTrans.gameObject, _crossScene);
                        SceneManager.MoveGameObjectToScene(_traversablesInPortal[i].PortalClone.gameObject, _crossScene);
                        SceneManager.MoveGameObjectToScene(GameManager.Instance.gameObject, _crossScene);
                        SceneManager.MoveGameObjectToScene(GameManager.GameUIManager.gameObject, _crossScene);
                    }
                    else
                    {
                        print(DestinationPortal.name);
                        print(_crossSceneDestinationPortal?.name);
                    }

                    travTrans.SetPositionAndRotation(m.GetColumn(3), m.rotation);

                    if (_traversablesInPortal[i].PortalClone.gameObject.scene != travTrans.gameObject.scene)
                    {
                        
                    }

                    _traversablesInPortal[i].OnPortalTraverse(this, DestinationPortal, m.GetColumn(3), m.rotation);

                    _traversablesInPortal[i].PortalClone.transform.SetPositionAndRotation(previousPos, previousRot);

                    if (!DestinationPortal._traversablesInPortal.Contains(_traversablesInPortal[i]))
                        DestinationPortal._traversablesInPortal.Add(_traversablesInPortal[i]);

                    onPortalTraverse(_traversablesInPortal[i]);

                    _traversablesInPortal.RemoveAt(i);

                    //if (_traversablesInPortal.Count > 0)
                    i--;
                }
                else
                {
                    _traversablesInPortal[i].PortalClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                }
            }
        }

        private void CameraUpdate()
        {
            SetRenderTexture();
            Transform mainCameraTrans = CameraController.MainCamera.transform;
            Transform localCamera = DestinationPortal._portalCamera.transform;

            if (DestinationPortal)
            {
                Vector3 lpos = _transform.InverseTransformPoint(mainCameraTrans.position);
                lpos.z = -lpos.z;
                lpos.x = -lpos.x;

                localCamera.localPosition = lpos;
                Quaternion nq = _transform.rotation;
                localCamera.localRotation = Quaternion.Inverse(Quaternion.Euler(new Vector3(-nq.eulerAngles.x, nq.eulerAngles.y + 180, -nq.eulerAngles.z))) * mainCameraTrans.rotation;

                DestinationPortal._portalCamera.nearClipPlane = Mathf.Clamp(Mathf.Abs(lpos.z) - _clipOffset, _clipOffset, Mathf.Infinity);

                _portalCamera.Render();
            }
        }

        private void SetRenderTexture()
        {
            if (_renderTex == null || _renderTex.width != Screen.width || _renderTex.height != Screen.height)
            {
                if (_renderTex != null)
                {
                    _renderTex.Release();
                }

                _renderTex = new RenderTexture(Screen.width, Screen.height, 0);

                DestinationPortal._portalCamera.targetTexture = _renderTex;

                _portalRenderer.material.SetTexture("_MainTex", _renderTex);
            }
        }

        private void OnDestinationPortalDeactivate()
        {
            if (!CameraController.Main.FocusOnPortalClone && 
                CameraController.Main.gameObject.scene != gameObject.scene)
            {
                _unloadSceneRoutine = StartCoroutine(UnloadSceneAndPortal(gameObject.scene.name));
            }
        }

        //private void OnBecameVisible()
        //{
        //    SetVisible(true);
        //}

        //private void OnBecameInvisible()
        //{
        //    SetVisible(false);
        //}

        private void SetDestinationPortal(JPortal destination)
        {
            if (DestinationPortal)
            {
                DestinationPortal._portalCamera.targetTexture = null;
                DestinationPortal._portalCamera.forceIntoRenderTexture = false;
            }

            if (destination)
            {
                DestinationPortal = destination;
                SetRenderTexture();

                _portalCamera.enabled = false;
                //_portalRenderer.SetPropertyBlock(_mpb);

                //_portalRenderer.material.SetTexture("_MainTex", _renderTex);
            }
            else
            {
                _renderTex = null;
            }
        }

        // In case we need some code here.
        private void SetVisible(bool visible)
        {
            _isVisible = visible;

            //_destinationPortal._portalCamera.enabled = _isVisible;
        }

        private void OnTriggerEnter(Collider other)
        {
            IPortalTraversable traversable = other.GetComponent<IPortalTraversable>();

            if (traversable != null && !_traversablesInPortal.Contains(traversable))
            {
                _traversablesInPortal.Add(traversable);
                EnableTraversableClone(traversable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IPortalTraversable traversable = other.GetComponent<IPortalTraversable>();

            if (traversable != null && _traversablesInPortal.Contains(traversable))
            {
                DisableTraversableClone(traversable);
                _traversablesInPortal.Remove(traversable);
            }
        }

        private void EnableTraversableClone(IPortalTraversable traversable)
        {
            if (!traversable.PortalClone)
            {
                traversable.PortalClone = Instantiate(traversable.PortalModel);
                //traversable.PortalClone.transform.parent = traversable.PortalModel.transform.parent;
                traversable.PortalClone.transform.localScale = traversable.PortalModel.transform.localScale;
            }
            else
            {
                traversable.PortalClone.SetActive(true);
            }
        }

        private void DisableTraversableClone(IPortalTraversable traversable)
        {
            traversable.PortalClone.SetActive(false);
        }

        private IEnumerator LoadSceneAndPortal(Action<float> completionRatio)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_sceneAddress.SceneName, LoadSceneMode.Additive);

            //asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                completionRatio(asyncLoad.progress);
                yield return null;
            }

        }

        private IEnumerator UnloadSceneAndPortal(string sceneName)//(Action<float> completionRatio)
        {
            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneName);

            //asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
