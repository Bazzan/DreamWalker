using System.Collections.Generic;
using GP2_Team7.Objects.Cameras;
using UnityEngine;

namespace GP2_Team7.Managers
{
	public class PauseManager : MonoBehaviour
	{
		public Canvas pauseMenu;
		[Tooltip("This child index will be enabled when pausing the game (resets the state of the menu if you pressed escape)")]
		public int mainMenuChildIndex;
		[Tooltip("These child indices will be ignored (good for background, event system etc that should never be enabled or disabled by themselves)")]
		public List<int> ignoreTheseChildIndices;
		
		public static PauseManager Instance { get; private set; }

		public static CursorLockMode previousLockMode;

		public static bool IsPaused
		{
			get => Instance != null && Instance.isPaused;
		}

		private bool isPaused = false;
		private bool switchedCameraState = false;
		
		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else if(Instance != this)
				Destroy(this);
			
			pauseMenu.gameObject.SetActive(false);
		}

		public static void Pause()
		{
			if (Instance == null)
				return;

			if (CameraController.IsInStandardMode)
			{
				CameraController.SwitchToStaticCameraView(false, Vector3.zero, Quaternion.identity);
				Instance.switchedCameraState = true;
			}

			Instance.isPaused = true;
			Instance.pauseMenu.gameObject.SetActive(true);
			Transform tf = Instance.pauseMenu.transform;
			for (int i = 0; i < tf.childCount; i++)
			{
				if (Instance.ignoreTheseChildIndices.Contains(i))
					continue;
				
				bool active = i == Instance.mainMenuChildIndex;
				tf.GetChild(i).gameObject.SetActive(active);
			}
			
			Time.timeScale = 0f;
			previousLockMode = Cursor.lockState;
			Cursor.lockState = CursorLockMode.None;
		}

		public static void Unpause()
		{
			if (Instance == null || !Instance.isPaused )
				return;

			if (Instance.switchedCameraState)
			{
				Instance.switchedCameraState = false;
				CameraController.SwitchToStandardCameraView();
			}

			Instance.isPaused = false;
			Instance.pauseMenu.gameObject.SetActive(false);
			Time.timeScale = 1f;
			Cursor.lockState = previousLockMode;
		}
	}
}
