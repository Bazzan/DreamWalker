using UnityEngine;
using UnityEngine.Events;

namespace GP2_Team7.Objects
{
	[RequireComponent(typeof(Collider))]
	public class Interactable : MonoBehaviour
	{
		[Tooltip("The floating text/icon on this interactable object that gives feedback that this object is interactable and is currently in range")]
		public GameObject label;
		[Tooltip("The event that will be called when pressing the interact button")]
		public UnityEvent eventToTrigger;
	}
}
