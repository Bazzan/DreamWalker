using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    public class BuildingLightSwitch : Interactable
    {
        private Coroutine[] _lightRoutines;

        [SerializeField]
        private MeshRenderer[] _lights;

        private bool _wasEnabled;

        [Space]

        [SerializeField]
        private Vector2Int _flickerVariance = new Vector2Int(3, 15);

        [SerializeField]
        private Vector2 _delayVariance = new Vector2(0.1f, 1);

        [SerializeField]
        private Vector2 _frequencyVariance = new Vector2(0.01f, 0.15f);

        protected override void Awake()
        {
            //base.Awake();

            _lightRoutines = new Coroutine[_lights.Length];

            _wasEnabled = enabled;

            foreach (MeshRenderer light in _lights)
            {
                light.enabled = enabled;
            }
        }

        public bool PowerOn
        {
            set
            {
                if (!gameObject.activeInHierarchy)
                    return;

                if (_lightRoutines == null || _lightRoutines.Length < _lights.Length)
                {
                    _lightRoutines = new Coroutine[_lights.Length];
                }

                for (int i = 0; i < _lights.Length; i++)
                {
                    if (_lightRoutines[i] != null)
                        StopCoroutine(_lightRoutines[i]);

                    _lightRoutines[i] = StartCoroutine(LightSwitchProcess(_lights[i], value, Random.Range(_delayVariance.x, _delayVariance.y), _frequencyVariance, Random.Range(_flickerVariance.x, _flickerVariance.y + 1)));
                }
            }
        }

        private void OnValidate()
        {
            if (enabled != _wasEnabled)
            {
                _wasEnabled = enabled;
                PowerOn = enabled;
            }
        }

        private void OnEnable()
        {
            if (_lightRoutines == null || _lightRoutines.Length < _lights.Length)
            {
                _lightRoutines = new Coroutine[_lights.Length];

                _wasEnabled = enabled;

                foreach (MeshRenderer light in _lights)
                {
                    light.enabled = enabled;
                }
            }
        }

        private void OnDisable()
        {
            _wasEnabled = true;
            PowerOn = false;
        }

        private IEnumerator LightSwitchProcess(MeshRenderer meshRenderer, bool switchOn, float delay, Vector2 frequencyVariance, int flickerCount)
        {
            yield return new WaitForSeconds(delay);

            while (flickerCount > 0)
            {
                meshRenderer.enabled = !meshRenderer.enabled;

                flickerCount--;

                yield return new WaitForSeconds(Random.Range(frequencyVariance.x, frequencyVariance.y));
            }

            meshRenderer.enabled = switchOn;
        }
    }
}
