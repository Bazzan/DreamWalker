using UnityEngine;

namespace GP2_Team7.Objects
{
	public class InteractablePressurePlate : Interactable
	{
		public void OnCollisionEnter(Collision other) => Interact();

		public void OnCollisionExit(Collision other) => Interact();
	}
}