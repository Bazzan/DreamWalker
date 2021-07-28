using System;
using UnityEngine;

namespace GP2_Team7.Managers
{
	public class CheatSystem : MonoBehaviour
	{
		[Serializable]
		public class KeyEventsPair
		{
			public KeyCode keyCode;
			public InteractableEventSystem eventsToRun;
		}

		public KeyEventsPair[] events;

		private void Update()
		{
			foreach (KeyEventsPair evt in events)
			{
				if (Input.GetKeyDown(evt.keyCode))
				{
					InteractableEventSystem system = evt.eventsToRun;
					system.enabled = false;
					system.enabled = true;
				}
			}
		}
	}
}
