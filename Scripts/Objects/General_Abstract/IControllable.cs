using System;

namespace GP2_Team7
{
	/// <summary>
	/// Controllables have dictionaries containing button actions
	/// and ActionDelegates. The InputManager calls GetActionDelegates if
	/// one is assigned to it, and will then execute the delegates
	/// inside it based on what key is pressed or released.
	/// </summary>
	public interface IControllable
	{
		ActionDelegates GetActionDelegates(Actions actions);
	}

	/// <summary>
	/// This class is a wrapper for delegates that contains all methods that is called when the key(s) that are mapped to this action are pressed, held or released (one delegate for each KeyState)
	/// </summary>
	public class ActionDelegates
	{
		public ActionDelegates(Action pressed = null, Action held = null, Action released = null)
		{
			_pressed = pressed;
			_held = held;
			_released = released;
		}

		private Action _pressed;
		private Action _held;
		private Action _released;

		/// <summary>
		/// Removes the input method from all actions in this class
		/// </summary>
		/// <param name="action"></param>
		public void Unsubscribe(params Action[] actions)
		{
            for (int i = 0; i < actions.Length; i++)
            {
                _pressed -= actions[i];
                _held -= actions[i];
                _released -= actions[i];
            }
		}
		
		public void Execute(KeyState phase)
		{
			switch (phase)
			{
				case KeyState.Down:
					_pressed?.Invoke();
					break;

				case KeyState.Held:
					_held?.Invoke();
					break;

				case KeyState.Up:
					_released?.Invoke();
					break;
			}
		}
	}

	public enum Actions
	{
		Forward,
		Left,
		Backward,
		Right,
		Interact
	}


	public enum KeyState
	{
		Inactive,
		Down,
		Held,
		Up
	}
}