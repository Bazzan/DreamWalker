using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace GP2_Team7.Managers
{
    using Objects;
    using Objects.Characters;
    using Objects.Cameras;

    public class SecondVoidPuzzleManager : PuzzleHandler
    {
        public StudioEventEmitter emitter;
        public string musicParameterName = "darkRoomSeq";
        
        private PlayerCharacter _player;

        [SerializeField, Tooltip("The bounds in which darkness can get you.")]
        private Bounds _darknessBounds;

        [SerializeField]
        private VoidLight[] _voidLights;

        private float _consumedByDarknessLevel = 0;
        public float ConsumedByDarknessLevel
        {
            get => _consumedByDarknessLevel;

            private set
            {
                if (gameObject.activeInHierarchy && _consumedByDarknessLevel < _maxDarknessTimer && value >= _maxDarknessTimer)
                {
                    GameManager.VoidOut(VoidOutType.EncapsulatedByDarkness);
                    UIManager.SubscribeOnHasFaded(OnHasFaded, true);
                    ResetPuzzle();
                }

                _consumedByDarknessLevel = value;
            }
        }

        [System.NonSerialized]
        private CameraShake _cameraShake;

        private PostProcessVolume _volume;

        [SerializeField]
        private CameraShakeValues _shakeValues = new CameraShakeValues(0.5f, 40, 2, ShakeFalloffType.Smooth);

        [SerializeField]
        private float _maxDarknessTimer = 1;

        [SerializeField]
        private float _darknessRecoveryRate = 1.5f;

        private VoidLight _currentPlayerVoidLight;

        protected override void Awake()
        {
            base.Awake();
            print("yes yes yes omg");
            VoidLight.onPlayerVoidLightEnter = OnPlayerEnterLight;
            VoidLight.onPlayerVoidLightExit = OnPlayerExitLight;

            

            _volume = GetComponentInChildren<PostProcessVolume>();
            _volume.weight = 0;

            gameObject.SetActive(false);
        }

        private void Update()
        {
            float musicValue = Mathf.Lerp(100, 0, DarknessRatio());
            Debug.Log(musicValue);
            emitter.SetParameter(musicParameterName, musicValue);
            
            if (!PlayerIsInLight())
            {
                if (ConsumedByDarknessLevel == 0)
                {
                    if (_cameraShake == null)
                        _cameraShake = new CameraShake(_shakeValues, DarknessRatio);
                    else
                        _cameraShake.cameraShakeValues = _shakeValues;

                    CameraController.Shake(_cameraShake);
                }

                //print(_consumedByDarknessLevel / _maxDarknessTimer);

                ConsumedByDarknessLevel = ConsumedByDarknessLevel + 
                    (ConsumedByDarknessLevel < _maxDarknessTimer ? Time.deltaTime : 0);
            }
            else if (ConsumedByDarknessLevel > 0)
            {
                ConsumedByDarknessLevel = Mathf.MoveTowards(ConsumedByDarknessLevel, 0, Time.deltaTime * _darknessRecoveryRate);

                if (ConsumedByDarknessLevel <= 0)
                    CameraController.RemoveShake(_cameraShake);
            }

            _volume.weight = ConsumedByDarknessLevel / _maxDarknessTimer;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_darknessBounds.center, _darknessBounds.size);
        }

        public void ResetPuzzle()
        {
            for (int i = 0; i < puzzleConditions.Length; i++)
            {
                // Unset puzzle condition if true.
                if (puzzleConditions[i])
                    Interacted(i);
            }

            outputAction(PuzzleHandlerTriggerMode.OnReset);
        }

        private bool PlayerIsInLight()
        {
            return _currentPlayerVoidLight || !_darknessBounds.Contains(GameManager.Player.transform.position);
        }

        private void OnPlayerEnterLight(VoidLight voidLight)
        {
            _currentPlayerVoidLight = voidLight;
        }

        private void OnPlayerExitLight(VoidLight voidLight)
        {
            if (_currentPlayerVoidLight == voidLight)
            {
                _currentPlayerVoidLight = null;

                foreach (VoidLight vl in _voidLights)
                {
                    if (vl && vl != voidLight && vl.gameObject.activeInHierarchy && vl.SafeCheck())
                    {
                        _currentPlayerVoidLight = vl;
                        break;
                    }
                }
            }
        }

        private float DarknessRatio()
        {
            return (float)ConsumedByDarknessLevel / _maxDarknessTimer;
        }

        private void OnHasFaded(bool wasFadeOut)
        {
            if (wasFadeOut)
            {
                _consumedByDarknessLevel = 0.1f;

            }
            else
            {
                UIManager.SubscribeOnHasFaded(OnHasFaded, false);
            }
        }

        private void OnDisable()
        {
            UIManager.SubscribeOnHasFaded(OnHasFaded, false);
        }
    }
}
