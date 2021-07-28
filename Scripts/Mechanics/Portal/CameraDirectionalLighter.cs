using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace GP2_Team7.Objects
{
    using Managers;
    using Cameras;

    public class CameraDirectionalLighter : MonoBehaviour
    {
        [SerializeField]
        private Light _directionalLight;

        [SerializeField]
        private PostProcessVolume _postProcessVolume;

        private bool _priorActiveState;

        private RenderSetter _otherScene;
        private RenderSetter _thisScene;

        private void Awake()
        {
            if (CompareTag("MainCamera"))
            {
                _directionalLight = GameObject.FindGameObjectWithTag("MainSceneLight").GetComponent<Light>();
                CameraController.onActiveSceneChange += OnActiveSceneChange;
            }
        }

        private void OnActiveSceneChange(Scene scene)
        {
            if (_directionalLight)
                _directionalLight.enabled = false;

            GameObject[] sceneObjs = scene.GetRootGameObjects();
            
            foreach (GameObject go in sceneObjs)
            {
                if (go.CompareTag("MainSceneLight"))
                {
                    _directionalLight = go.GetComponent<Light>();
                    break;
                }
            }

            print(_directionalLight.gameObject.scene.name);

            _directionalLight.gameObject.SetActive(true);
        }

        private void Start()
        {
            if (_directionalLight)
            {
                _priorActiveState = _directionalLight.gameObject.activeInHierarchy;
                _directionalLight.gameObject.SetActive(true);
            }
            
            if (_postProcessVolume)
            {
                _postProcessVolume.gameObject.SetActive(true);
            }
        }

        private void OnPreCull()
        {
            if (_directionalLight)
            {
                _directionalLight.enabled = true;
                if (CameraController.Main.gameObject.scene != gameObject.scene)
                {
                    SceneManager.SetActiveScene(gameObject.scene);
                }
            }

            if (_postProcessVolume)
            {
                _postProcessVolume.enabled = true;
            }
        }

        private void OnPreRender()
        {
            if (_directionalLight)
            {
                _directionalLight.enabled = true;
                if (CameraController.Main.gameObject.scene != gameObject.scene)
                {
                    SceneManager.SetActiveScene(gameObject.scene);
                }
            }

            if (_postProcessVolume)
            {
                _postProcessVolume.enabled = true;
            }
        }

        private void OnPostRender()
        {
            if (_directionalLight)
            {
                _directionalLight.enabled = false;

                if (CameraController.Main.gameObject.scene != gameObject.scene)
                {
                    SceneManager.SetActiveScene(CameraController.Main.gameObject.scene);
                }
            }

            if (_postProcessVolume)
            {
                _postProcessVolume.enabled = false;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                if (_directionalLight)
                    _directionalLight.gameObject.SetActive(_priorActiveState);

                if (_postProcessVolume)
                {
                    _postProcessVolume.gameObject.SetActive(_priorActiveState);
                }
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                if (_directionalLight)
                    _directionalLight.gameObject.SetActive(true);

                if (_postProcessVolume)
                {
                    _postProcessVolume.gameObject.SetActive(true);
                }
            }
        }

        private void OnDestroy()
        {
            if (_directionalLight)
                _directionalLight.gameObject.SetActive(_priorActiveState);

            if (_postProcessVolume)
            {
                _postProcessVolume.gameObject.SetActive(_priorActiveState);
            }
        }

        public class RenderSetter
        {
            public RenderSetter()
            {
                ambientEquatorColor = RenderSettings.ambientEquatorColor;
                ambientGroundColor = RenderSettings.ambientGroundColor;
                ambientIntensity = RenderSettings.ambientIntensity;
                ambientLight = RenderSettings.ambientLight;
                ambientMode = RenderSettings.ambientMode;
                ambientProbe = RenderSettings.ambientProbe;
                ambientSkyColor = RenderSettings.ambientSkyColor;
                customReflection = RenderSettings.customReflection;
                defaultReflectionMode = RenderSettings.defaultReflectionMode;
                defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
                flareFadeSpeed = RenderSettings.flareFadeSpeed;
                flareStrength = RenderSettings.flareStrength;
                fog = RenderSettings.fog;
                fogColor = RenderSettings.fogColor;
                fogDensity = RenderSettings.fogDensity;
                fogEndDistance = RenderSettings.fogEndDistance;
                fogMode = RenderSettings.fogMode;
                fogStartDistance = RenderSettings.fogStartDistance;
                haloStrength = RenderSettings.haloStrength;
                reflectionBounces = RenderSettings.reflectionBounces;
                reflectionIntensity = RenderSettings.reflectionIntensity;
                skybox = RenderSettings.skybox;
                subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
                sun = RenderSettings.sun;
            }

            public Color ambientEquatorColor;
            public Color ambientGroundColor;
            public float ambientIntensity;
            public Color ambientLight;
            public AmbientMode ambientMode;
            public SphericalHarmonicsL2 ambientProbe;
            public Color ambientSkyColor;
            public Cubemap customReflection;
            public DefaultReflectionMode defaultReflectionMode;
            public int defaultReflectionResolution;
            public float flareFadeSpeed;
            public float flareStrength;
            public bool fog;
            public Color fogColor;
            public float fogDensity;
            public float fogEndDistance;
            public FogMode fogMode;
            public float fogStartDistance;
            public float haloStrength;
            public int reflectionBounces;
            public float reflectionIntensity;
            public Material skybox;
            public Color subtractiveShadowColor;
            public Light sun;

            public void Set()
            {
                RenderSettings.ambientEquatorColor = ambientEquatorColor;
                RenderSettings.ambientGroundColor = ambientGroundColor;
                RenderSettings.ambientIntensity = ambientIntensity;
                RenderSettings.ambientLight = ambientLight;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientProbe = ambientProbe;
                RenderSettings.ambientSkyColor = ambientSkyColor;
                RenderSettings.customReflection = customReflection;
                RenderSettings.defaultReflectionMode = defaultReflectionMode;
                RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
                RenderSettings.flareFadeSpeed = flareFadeSpeed;
                RenderSettings.flareStrength = flareStrength;
                RenderSettings.fog = fog;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogEndDistance = fogEndDistance;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.haloStrength = haloStrength;
                RenderSettings.reflectionBounces = reflectionBounces;
                RenderSettings.reflectionIntensity = reflectionIntensity;
                RenderSettings.skybox = skybox;
                RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
                RenderSettings.sun = sun;
            }
        }
    }
}
