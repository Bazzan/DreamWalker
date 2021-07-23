using System;

namespace GP2_Team7
{
	/// <summary>
	/// Controllables have dictionaries containing button actions
	/// and PhaseActions. The InputManager calls GetPhaseAction if
	/// one is assigned to it, and will then execute the actions
	/// inside it based on what key is pressed or released.
	/// </summary>
	public interface IControllable
	{
		PhaseAction GetPhaseAction(ButtonAction buttonAction);
	}

	/// <summary>
	/// This class is a wrapper for delegates that contains all methods that is called when the key(s) that are mapped to this action are pressed, held or released (one delegate for each KeyState)
	/// </summary>
	public class PhaseAction
	{
		public PhaseAction(Action pressed = null, Action held = null, Action released = null)
		{
			_pressed = pressed;
			_held = held;
			_released = released;
		}

		private Action _pressed;
		private Action _held;
		private Action _released;

		public void Execute(InputPhase phase)
		{
			switch (phase)
			{
				case InputPhase.Down:
					_pressed?.Invoke();
					break;

				case InputPhase.Held:
					_held?.Invoke();
					break;

				case InputPhase.Up:
					_released?.Invoke();
					break;
			}
		}
	}

	public enum ButtonAction
	{
		Forward,
		Left,
		Backward,
		Right
	}


	public enum InputPhase
	{
		Inactive,
		Down,
		Held,
		Up
	}
}