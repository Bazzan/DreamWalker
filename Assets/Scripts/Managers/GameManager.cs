using System;
using UnityEngine;

// Created by Oliver Lebert 12-01-21
namespace GP2_Team7.Managers
{
	[Serializable]
	public struct InputModule
	{
		public KeyCode keycode;
		public ButtonAction action;
	}

	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance { get; private set; }
		
		public static GameObject Player { get; private set; }

		public InputModule[] inputs = new InputModule[0];

		[NonSerialized] public Vector2 mouseInput;
		
		private IControllable _playerControllable;

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
			
			Player = GameObject.FindGameObjectWithTag("Player");
			// _playerControllable = Player.GetComponent<GameCharacter>()._avatar; // todo add property in player script that can get the IControllable
		}

		private void Update()
		{
			ReadPlayerInput();
			mouseInput = GetMouseInput();
		}

		private void ReadPlayerInput()
		{
			if (inputs == null || _playerControllable == null)
				return;

			foreach (InputModule module in inputs)
			{
				_playerControllable.GetPhaseAction(module.action).Execute(GetPhase(module.keycode));
			}
		}
		
		public InputPhase GetPhase(KeyCode keyCode) => Input.GetKeyDown(keyCode) ? InputPhase.Down :
			Input.GetKeyUp(keyCode) ? InputPhase.Up :
			Input.GetKey(keyCode) ? InputPhase.Held :
			InputPhase.Inactive;

		public static Vector2 GetMouseInput()
		{
			return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		}
	}
}