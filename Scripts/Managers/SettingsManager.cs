using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GP2_Team7.Managers
{
	public class SettingsManager : MonoBehaviour
	{
		[Serializable]
		public struct KeyBindButton
		{
			public GameObject button;
			public Actions action;
		}

		[Serializable]
		public struct SettingSlider
		{
			public Slider slider;
			public SliderType type;
		}

		public enum SliderType
		{
			Sensitivity,
			MasterVolume,
			MusicVolume,
			SfxVolume,
		}

		public Button resetButton;
		public Toggle dyslexicFontToggle;

		public List<SettingSlider> sliders;

		public List<KeyBindButton> keyBindControls;
		public float keyBindHorizontalPadding = 2f;

		[Tooltip("This GameObject should hold text such as \"press any key to bind it to\"")]
		public GameObject textToEnableWhenEditingKeyBinds;

		public static SettingsManager Instance { get; private set; }

		private readonly List<TMP_Text> _keyBindDisplayText = new List<TMP_Text>();
		private readonly List<Button> _keyBindAssignButton = new List<Button>();
		private SettingsData _data;
		private bool _inKeyBindMode = false;
		private Actions _currentActionBeingEdited;
		private TMP_Text _currentKeyBindButtonText;
		private Button _currentKeyBindButton;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}

			for (int i = 0; i < keyBindControls.Count; i++)
			{
				Button currentButton = keyBindControls[i].button.GetComponentInChildren<Button>();
				TMP_Text currentButtonText = keyBindControls[i].button.GetComponentInChildren<TMP_Text>();
				_keyBindDisplayText.Add(currentButtonText);
				_keyBindAssignButton.Add(currentButton);
			}
		}

		private void OnEnable()
		{
			UpdateControls();
		}

		private void OnDisable()
		{
			UnsubscribeEvents();
		}

		private void OnClickResetButton()
		{
			Debug.Log("Clicked reset");
			SettingsFile.Delete();
			_data = new SettingsData();
			Debug.Log("Settings file deleted");
			
			UpdateControls();
			
			if(GameManager.Instance != null)
				GameManager.Instance.OnChangeSettings(_data);
		}

		/// <summary>
		/// Sets the UI Controls value to the values in the settings file
		/// </summary>
		private void UpdateControls()
		{
			UnsubscribeEvents();
			
			_data = SettingsFile.Load(out bool returnedNew);

			if (resetButton != null)
			{
				if (returnedNew)
				{
					resetButton.gameObject.SetActive(false);
					resetButton.onClick.RemoveListener(OnClickResetButton);
				}
				else
				{
					resetButton.gameObject.SetActive(true);
					resetButton.onClick.RemoveListener(OnClickResetButton);
					resetButton.onClick.AddListener(OnClickResetButton);
				}
			}

			dyslexicFontToggle.isOn = _data.dyslexicFont;

			foreach (SettingSlider slider in sliders)
			{
				slider.slider.normalizedValue = Array.Find(_data.sliderValues, a => a.type == slider.type).sliderValue;
			}

			for(int i = 0; i < keyBindControls.Count; i++)
			{
				KeyAction key = Array.Find(_data.keybinds, a => a.action == keyBindControls[i].action);
				UpdateKeyBindButtonText(key.keycode, _keyBindDisplayText[i], _keyBindAssignButton[i]);
			}
			
			SubscribeEvents();
		}

		private void SubscribeEvents()
		{
			foreach (SettingSlider slider in sliders)
			{
				SettingSlider slid = slider;
				slider.slider.onValueChanged.AddListener((x) =>
				{
					OnChangeSlider(slid);
				});
			}

			for (int i = 0; i < keyBindControls.Count; i++)
			{
				var currentButton = _keyBindAssignButton[i];
				var currentButtonText = _keyBindDisplayText[i];
				
				Actions action = keyBindControls[i].action;
				currentButton.onClick.AddListener(() => {OnClickKeyBindButton(currentButtonText, currentButton, action); });
			}

			dyslexicFontToggle.onValueChanged.AddListener(OnClickDyslexicFontButton);
		}

		private void UnsubscribeEvents()
		{
			foreach (SettingSlider slider in sliders)
			{
				SettingSlider slid = slider;
				slider.slider.onValueChanged.RemoveListener((x) =>
				{
					OnChangeSlider(slid);
				});
			}

			for (int i = 0; i < keyBindControls.Count; i++)
			{
				var currentButton = _keyBindAssignButton[i];
				var currentButtonText = _keyBindDisplayText[i];
				
				Actions action = keyBindControls[i].action;
				currentButton.onClick.RemoveListener(() => {OnClickKeyBindButton(currentButtonText, currentButton, action); });
			}

			dyslexicFontToggle.onValueChanged.RemoveListener(OnClickDyslexicFontButton);
		}

		private void OnGUI()
		{
			if (!_inKeyBindMode)
				return;
			
			Event evt = Event.current;
			if (Input.anyKeyDown && (evt.isKey || evt.isMouse))
			{
				Cursor.lockState = CursorLockMode.None;
				KeyCode code = KeyCode.A;
				if (evt.isKey)
				{
					code = evt.keyCode;

					if (code == KeyCode.Escape)
					{
						_inKeyBindMode = false;
						return;
					}
				}
				else if(evt.isMouse)
				{
					if (evt.button == 0)
						code = KeyCode.Mouse0;
					else if (evt.button == 1)
						code = KeyCode.Mouse1;
					else if (evt.button == 2)
						code = KeyCode.Mouse2;
					else if (evt.button == 3)
						code = KeyCode.Mouse3;
					else if (evt.button == 4)
						code = KeyCode.Mouse4;
					else if (evt.button == 5)
						code = KeyCode.Mouse5;
					else if (evt.button == 6)
						code = KeyCode.Mouse6;
				}

				UpdateKeyBindButtonText(code, _currentKeyBindButtonText, _currentKeyBindButton);
				
				SetKeyBind(_currentActionBeingEdited, code);
				
				if (textToEnableWhenEditingKeyBinds != null)
				{
					textToEnableWhenEditingKeyBinds.SetActive(false);
				}
				
				_inKeyBindMode = false;
			}
		}

		private void UpdateKeyBindButtonText(KeyCode code, TMP_Text buttonText, Button keyBindButton)
		{
			buttonText.text = $"{code}";
			buttonText.ForceMeshUpdate();
			
			var tf = keyBindButton.GetComponent<RectTransform>();
			tf.sizeDelta = new Vector2(buttonText.textBounds.size.x + keyBindHorizontalPadding, tf.sizeDelta.y);
		}

		private void OnChangeSlider(SettingSlider slider)
		{
			int index = Array.FindIndex(_data.sliderValues, a => a.type == slider.type);
			_data.sliderValues[index].sliderValue = slider.slider.normalizedValue;
			UpdateSettings();
		}

		private void OnClickDyslexicFontButton(bool value)
		{
			_data.dyslexicFont = value;
			UpdateSettings();
		}

		private void OnClickKeyBindButton(TMP_Text buttonText, Button currentButton, Actions action)
		{
			Cursor.lockState = CursorLockMode.Locked;
			_inKeyBindMode = true;
			
			_currentActionBeingEdited = action;
			_currentKeyBindButtonText = buttonText;
			_currentKeyBindButton = currentButton;
			
			if (textToEnableWhenEditingKeyBinds != null)
			{
				textToEnableWhenEditingKeyBinds.SetActive(true);
			}
		}

		private void UpdateSettings()
		{
			SettingsFile.Save(_data);

			if (!resetButton.gameObject.activeSelf)
			{
				resetButton.gameObject.SetActive(true);
				resetButton.onClick.AddListener(OnClickResetButton);
			}

			if(GameManager.Instance != null)
				GameManager.Instance.OnChangeSettings(_data);
		}

		private void SetKeyBind(Actions action, KeyCode key)
		{
			if (_data.keybinds == null || _data.keybinds.Length == 0)
			{
				_data.keybinds = new KeyAction[Enum.GetNames(typeof(Actions)).Length];
				
				for (int i = 0; i < _data.keybinds.Length; i++)
				{
					_data.keybinds[i].action = (Actions) i;
				}
			}

			int index = Array.FindIndex(_data.keybinds, a => a.action == action);

			if (index < 0)
			{
				index = _data.keybinds.Length;
				KeyAction[] newArray = new KeyAction[index + 1];

				for (int i = 0; i < index; i++)
				{
					newArray[i] = _data.keybinds[i];
				}

				newArray[index].action = action;
				_data.keybinds = newArray;
			}
			else
			{
				_data.keybinds[index].keycode = key;
			}
			
			UpdateSettings();
		}
	}

	public static class SettingsFile
	{
		static readonly string path = Path.Combine(Application.persistentDataPath, "settings.dat");
		
		public static SettingsData Load(out bool returnedNew)
		{
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				returnedNew = false;
				
				return JsonUtility.FromJson<SettingsData>(json);
			}

			Debug.Log("A settings file does not exist, returning a new instance of the settings class (not saved on hard drive)");
			returnedNew = true;
			return new SettingsData();
		}

		public static SettingsData Load() => Load(out bool returnedNew);

		public static void Save(SettingsData settings)
		{
			string json = JsonUtility.ToJson(settings, true);

			File.WriteAllText(path, json);
			Debug.Log($"Saved settings file to hard drive at {path}");
		}

		public static void Delete()
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[Serializable]
	public class KeyAction
	{
		public KeyCode keycode;
		public Actions action;
	}

	[Serializable]
	public class SliderData
	{
		public float sliderValue;
		public SettingsManager.SliderType type;
	}
	
	[Serializable]
	public class SettingsData
	{
		public bool showSubtitles = true;

		public bool dyslexicFont = false;

		public SliderData[] sliderValues;

		public KeyAction[] keybinds;

		public SettingsData()
		{
			sliderValues = new SliderData[Enum.GetNames(typeof(SettingsManager.SliderType)).Length];

			for (int i = 0; i < sliderValues.Length; i++)
			{
				if((SettingsManager.SliderType) i == SettingsManager.SliderType.Sensitivity)
					sliderValues[i] = new SliderData {type = (SettingsManager.SliderType) i, sliderValue = 0.5f};
				
				sliderValues[i] = new SliderData {type = (SettingsManager.SliderType) i, sliderValue = 0.8f};
			}
			
			keybinds = new KeyAction[Enum.GetNames(typeof(Actions)).Length];

			for (int i = 0; i < keybinds.Length; i++)
			{
				Actions action = (Actions) i;
				KeyCode code;
				switch (action)
				{
					case Actions.Forward:
						code = KeyCode.W;
						break;
					case Actions.Left:
						code = KeyCode.A;
						break;
					case Actions.Backward:
						code = KeyCode.S;
						break;
					case Actions.Right:
						code = KeyCode.D;
						break;
					case Actions.Interact:
						code = KeyCode.Mouse0;
						break;
					default:
						code = KeyCode.A;
						Debug.Log($"{action} Does not have a default keycode, reverting to keycode 0 (A)");
						break;
				}

				keybinds[i] = new KeyAction {action = action, keycode = code};
			}
		}
	}
}