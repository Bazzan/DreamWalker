using System;
using UnityEngine;

namespace GP2_Team7.Objects
{
	public class CircleClickObject : MonoBehaviour
	{
		public GameObject optionalParticleSystemToEnableOnClick;
		public Action<CircleClickObject> onClickEvent;
		
		private void Start()
		{
			if(optionalParticleSystemToEnableOnClick != null)
				optionalParticleSystemToEnableOnClick.SetActive(false);
		}

		private void OnMouseDown()
		{
			onClickEvent.Invoke(this);
		}

		public void Disable()
		{
			Destroy(gameObject);
		}
	}
}
