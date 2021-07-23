using UnityEngine;

// Created by Oliver Lebert 12-01-21
namespace GP2_Team7.Objects.Player
{
	public class InteractableRaycaster : MonoBehaviour
	{
		public float range = 2f;
		public Transform raycastOrigin;

		private Interactable currentInteractableInRange = null;

		private void FixedUpdate()
		{
			if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out RaycastHit hitInfo, range))
			{
				if (hitInfo.collider.TryGetComponent(out currentInteractableInRange))
				{
					currentInteractableInRange.label.SetActive(true);
					return;
				}
			}
		
			if(currentInteractableInRange != null)
				currentInteractableInRange.label.SetActive(false);
		
			currentInteractableInRange = null;
		}

		public void Interact()
		{
			if (currentInteractableInRange != null)
			{
				currentInteractableInRange.eventToTrigger.Invoke();
			}
		}
	}
}
