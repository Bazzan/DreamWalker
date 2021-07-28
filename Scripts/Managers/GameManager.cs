using System;
using FMOD.Studio;
using GP2_Team7.Objects.Cameras;
using UnityEngine;

// Created by Oliver Lebert 12-01-21
namespace GP2_Team7.Managers
{
    using Objects.Characters;
    using Objects.Player;
    using Items;

    [DefaultExecutionOrder(-1)]
    public class GameManager : MonoBehaviour
    {
        private static Action<VoidOutType> _onVoidOut;

        /// <summary>
        /// Subscribes to an action that triggers when the player voids out,
        /// e.g. respawning at a checkpoint out of a dangerous position.
        /// </summary>
        /// <param name="action">The action that will trigger on voidout.</param>
        /// <param name="isSubscription">If true, subscribes the action. Unsubscribes if false.</param>
        public static void SubscribeVoidOut(Action<VoidOutType> action, bool isSubscription)
        {
            if (isSubscription)
                _onVoidOut += action;
            else
                _onVoidOut -= action;
        }

        /// <summary>
        /// Causes the game to fade out and respawn the
        /// player at the last specified checkpoint.
        /// </summary>
        public static void VoidOut(VoidOutType voidOutType)
        {
            _onVoidOut?.Invoke(voidOutType);
        }

        public static GameManager Instance { get; private set; }

        public static GameObject Player { get; private set; }

        public static UIManager GameUIManager { get; private set; }

        [SerializeReference, InspectorName("Inventory (The amount of slots corresponds to the size of the inventory!)")]
        private InventoryManager _inventory = InventoryManager.GlobalInventory;

        public static InventoryManager Inventory => Instance._inventory;
        
        public static bool UsingDyslexicFont => Instance._dyslexicFont;

        [SerializeField]
        private UIManager _gameUIPrefab;

        public bool lockMouse = true;
        public Texture2D cursor;

        [NonSerialized] public Vector2 mouseInput;

        public static Action onChangeSettings;

        private IControllable _playerControllable;
        private IControllable _interactableRaycastControllable;
        private KeyAction[] _inputs;
        private bool _dyslexicFont;

        public CutsceneManager CutsceneManagerInst { get; private set; }

        private Vector3 _defaultRespawnPosition;
        private Quaternion _defaultRespawnRotation;

        public Transform respawnPoint;

        public static void GetRespawnPoint(out Vector3 respawnPosition, out Quaternion respawnRotation)
        {
            if (Instance.respawnPoint)
            {
                respawnPosition = Instance.respawnPoint.position;

                respawnRotation = Instance.respawnPoint.rotation;
            }
            else
            {
                respawnPosition = Instance._defaultRespawnPosition;

                respawnRotation = Instance._defaultRespawnRotation;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else if (Instance == null)
            {
                Instance = this;
            }

            if (cursor != null)
                Cursor.SetCursor(cursor, new Vector2(cursor.width * 0.5f, cursor.height * 0.5f), CursorMode.Auto);

            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player != null)
            {
                _playerControllable = Player.GetComponentInChildren<PlayerCharacter>();
                _interactableRaycastControllable = Player.GetComponentInChildren<InteractableRaycaster>();

                _defaultRespawnPosition = Player.transform.position;
                _defaultRespawnRotation = Player.transform.rotation;
            }

            UIManager uiManagerInScene = FindObjectOfType<UIManager>(true);
            GameUIManager = uiManagerInScene ? uiManagerInScene : Instantiate(_gameUIPrefab);

            CutsceneManagerInst = new CutsceneManager();

            _inventory = InventoryManager.GlobalInventory;

            LoadSettings();

            Objects.RespawnPoint.onRespawnPointSet += OnSetRespawnPoint;
        }

        private void OnEnable()
        {
            CutsceneManager.AssignOnCutsceneActiveState(ResetAllInputs);
        }

        private void OnDisable()
        {
            CutsceneManager.UnassignOnCutsceneActiveState(ResetAllInputs);
        }

        public void OnChangeSettings()
        {
            Debug.Log("On change settings");
            LoadSettings();
            onChangeSettings?.Invoke();
        }

        public void OnChangeSettings(SettingsData data)
        {
            Debug.Log("On change settings");
            LoadSettings(data);
            onChangeSettings?.Invoke();
        }

        private void Start()
        {
            if (lockMouse)
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void LoadSettings()
        {
            SettingsData data = SettingsFile.Load();
            LoadSettings(data);
        }
        
        private void LoadSettings(SettingsData data)
        {
            _dyslexicFont = data.dyslexicFont;
            _inputs = data.keybinds;
            float normalizedSensitivity = Array.Find(data.sliderValues, a => a.type == SettingsManager.SliderType.Sensitivity).sliderValue;
            Camera.main.GetComponentInParent<CameraController>().MouseSensitivity = Mathf.Lerp(0f, 4f, normalizedSensitivity);

            Bus masterBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBUS");
            Bus musicBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBUS/MusicBUS");
            Bus sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/MasterBUS/SFXBus");
            
            SliderData[] sliders = data.sliderValues;
            float masterValue = Array.Find(sliders, a => a.type == SettingsManager.SliderType.MasterVolume).sliderValue;
            float musicValue = Array.Find(sliders, a => a.type == SettingsManager.SliderType.MusicVolume).sliderValue;
            float sfxValue = Array.Find(sliders, a => a.type == SettingsManager.SliderType.SfxVolume).sliderValue;

            float dbMaster = Mathf.Lerp(-80, 10, masterValue);
            float dbMusic = Mathf.Lerp(-80, 10, musicValue);
            float dbSfx = Mathf.Lerp(-80, 10, sfxValue);

            // (10^(-80/20f) = 0.0001f)
            float volumeMaster = masterValue < 0.01f ? 0.0001f : Mathf.Pow(10.0f, dbMaster / 60f);
            float volumeMusic = musicValue < 0.01f ? 0.0001f : Mathf.Pow(10.0f, dbMusic / 60f);
            float volumeSfx = sfxValue < 0.01f ? 0.0001f : Mathf.Pow(10.0f, dbSfx / 60f);

            masterBus.setVolume(volumeMaster);
            musicBus.setVolume(volumeMusic);
            sfxBus.setVolume(volumeSfx);
        }

        private void Update()
        {
            ReadPlayerInput();
            mouseInput = GetMouseInput();

            CutsceneManagerInst.CutsceneUpdate();
        }

        private void ReadPlayerInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (PauseManager.IsPaused)
                    PauseManager.Unpause();
                else
                    PauseManager.Pause();
            }

            if (_inputs == null || _playerControllable == null || _interactableRaycastControllable == null || CutsceneManager.IsInCutscene || PauseManager.IsPaused)
                return;

            foreach (KeyAction module in _inputs)
            {
                if (module.action == Actions.Interact)
                {
                    _interactableRaycastControllable.GetActionDelegates(module.action)?.Execute(GetKeyState(module.keycode));
                    continue;
                }

                _playerControllable.GetActionDelegates(module.action).Execute(GetKeyState(module.keycode));
            }
        }

        private void OnSetRespawnPoint(Vector3 newPosition, Quaternion newRotation)
        {
            respawnPoint = null;
            _defaultRespawnPosition = newPosition;
            _defaultRespawnRotation = newRotation;
        }


        private void ResetAllInputs(bool reset, CamFixedViewSettings settings)
        {
            if (reset)
            {
                foreach (KeyAction module in _inputs)
                {
                    if (GetKeyState(module.keycode) == KeyState.Down ||
                        GetKeyState(module.keycode) == KeyState.Held)
                        _playerControllable.GetActionDelegates(module.action)?.Execute(KeyState.Up);
                }
            }
        }

        public KeyState GetKeyState(KeyCode keyCode) => Input.GetKeyDown(keyCode) ? KeyState.Down :
            Input.GetKeyUp(keyCode) ? KeyState.Up :
            Input.GetKey(keyCode) ? KeyState.Held :
            KeyState.Inactive;

        public static Vector2 GetMouseInput()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        private class VoidOutSequence : ICutscene
        {
            public float? CutsceneTime
            {
                get => null;
                set { }
            }

            public float? CutsceneLength => null;

            public Action<object[]> OnCutsceneEnd
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public bool IsInterruptible => throw new NotImplementedException();

            Action<Action[]> ICutscene.OnCutsceneEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void OnCutsceneStart(float startPosition)
            {
                throw new NotImplementedException();
            }

            public void CutsceneEnd()
            {
                throw new NotImplementedException();
            }

            public void OnCutsceneUpdate()
            {
                throw new NotImplementedException();
            }

            public void StopCutscene()
            {
                throw new NotImplementedException();
            }

            public bool OnSkip(float skipProgress) => false;
        }


    }
}

public enum VoidOutType
{
    FallingIntoBottomlessPit,
    EncapsulatedByDarkness
}