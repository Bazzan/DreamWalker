using UnityEngine;

namespace GP2_Team7.Objects
{
	using Managers;
	
	public class FollowPlayer : MonoBehaviour
	{
		[Tooltip("The local position this object should have when compared to the player")]
		public Vector3 localPosition;

		private Transform _transform;
		
		private void Awake()
		{
			_transform = transform;
		}

		private void Update()
		{
			_transform.position = GameManager.Player.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);
		}
	}
}
