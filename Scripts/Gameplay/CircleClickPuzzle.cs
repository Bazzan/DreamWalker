using System;
using System.Collections;
using FMODUnity;
using GP2_Team7.Objects.Characters;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GP2_Team7.Objects
{
	using Libraries;
	
	[RequireComponent(typeof(Canvas))]
	public class CircleClickPuzzle : MonoBehaviour
	{
		public static Action<CircleClickPuzzleData> startMovingPlayer;
		public CircleClickTrigger triggerObject;

		public StudioEventEmitter successfulPressEmitter;
		public StudioEventEmitter failPressEmitter;

		public GameObject visual3dObject;
		public Button backgroundButton;
		public Transform startPoint;
		public Transform endPoint;
		[Tooltip("How far ahead the camera the objects will spawn (specified in units)")]
		public float objectDistanceToCam = 1.5f;
    
		[Tooltip("The rectangle's (in which the circles will spawn in) width at the start of the puzzle (will cover the entire screen in the end (canvas is 1920x1080 regardless of screen resolution (will scale accordingly)))")]
		public float startingRectWidth = 100;
		[Range(0f, 1f), Tooltip("Higher values means the circles will spawn on the entire screen very early, lower values will give a more flat curve for the spawning range (this value will be added to a interpolator (that will start at 0) just before a circle is spawned)")]
		public float interpolationStepMaskSize = 0.1f;
		
		public float startMinTimeToWaitBetweenSpawns = 2f;
		public float startMaxTimeToWaitBetweenSpawns = 5f;
		public float finalMinTimeToWaitBetweenSpawns = 1f;
		public float finalMaxTimeToWaitBetweenSpawns = 2f;
		[Range(0f, 1f), Tooltip("Fade speed is percentage units per second 0 = 0 %, 1 = 100 %")]
		public float buttonFadeSpeed = 0.1f;
    
		[Range(0f, 1f), Tooltip("Higher values means the circles will spawn quickly very early, lower values will give a more flat curve for the spawning rate (this value will be added to a interpolator (that will start at 0) just before a circle is spawned)")]
		public float interpolationStepBetweenStartFinalTime = 0.1f;

		[Tooltip("Speed is the value that will be added to interpolator every single frame (when interpolator reaches 1 it means puzzle is cleared)")]
		public float baseSpeedAdd = 0.01f;
		public float speedAddEachSuccessfulPress = 0.01f;
		public float speedSubtractEachFailedPress = 0.01f;
		public float speedSubtractEachMissedObject = 0.01f;
		
		public float minObjectScale = 0.1f;
		public float maxObjectScale = 0.3f;

		[Header("Debug")]
		
		public bool debugRectMask = false;

		private readonly Vector2 _canvasSize = new Vector2(1920, 1080);
		private float _maxMinTimeInterpolator = 0f;
		private float _rectMaskInterpolator = 0f;
		private Vector2 _startingRectSize;
		private Vector2 _cameraFovInDegrees;
		private Vector2 _currentRectMask;
		private Camera _camera;
		private bool _clickedObject = false;
		private CircleClickPuzzleData _puzzleMoveData;

		private void OnValidate()
		{
			if (startMinTimeToWaitBetweenSpawns < 0)
				startMinTimeToWaitBetweenSpawns = Math.Abs(startMinTimeToWaitBetweenSpawns);

			if (startMaxTimeToWaitBetweenSpawns < 0)
				startMaxTimeToWaitBetweenSpawns = Math.Abs(startMaxTimeToWaitBetweenSpawns);
        
			if (finalMinTimeToWaitBetweenSpawns < 0)
				finalMinTimeToWaitBetweenSpawns = Math.Abs(finalMinTimeToWaitBetweenSpawns);
        
			if (finalMaxTimeToWaitBetweenSpawns < 0)
				finalMaxTimeToWaitBetweenSpawns = Math.Abs(finalMaxTimeToWaitBetweenSpawns);
        
			if (startingRectWidth < 0)
				startingRectWidth = Math.Abs(startingRectWidth);
		}

		private void Awake()
		{
			_camera = Camera.main;
			float verticalFov = _camera.fieldOfView;
			_cameraFovInDegrees = new Vector2(Camera.VerticalToHorizontalFieldOfView(verticalFov, _camera.aspect) , verticalFov);
		}

		private void Start()
		{
			_puzzleMoveData  = new CircleClickPuzzleData(baseSpeedAdd, startPoint.position, endPoint.position, OnDisable);
			triggerObject.triggered += TriggerEnter;
			
			startMovingPlayer.Invoke(_puzzleMoveData);
			
			backgroundButton.onClick.AddListener(() => StartCoroutine(ClickBackground()));
			_startingRectSize = new Vector2(startingRectWidth, startingRectWidth / 1.7777f); // 1.7777 for 16x9 aspect ratio which a reference resolution of 1920x1080 would signify
			StartCoroutine(SpawnButton());
		}

		private IEnumerator SpawnButton()
		{
            yield return new WaitUntil(() => !Cameras.CameraController.Main.FocusOnPortalClone);

			float minTime = Mathf.Lerp(startMinTimeToWaitBetweenSpawns, finalMinTimeToWaitBetweenSpawns, _maxMinTimeInterpolator);
			float maxTime = Mathf.Lerp(startMaxTimeToWaitBetweenSpawns, finalMaxTimeToWaitBetweenSpawns, _maxMinTimeInterpolator);

			_currentRectMask = Vector2.Lerp(_startingRectSize, _canvasSize, _rectMaskInterpolator);
        
			_maxMinTimeInterpolator += interpolationStepBetweenStartFinalTime;
			_rectMaskInterpolator += interpolationStepMaskSize;
        
			float secondsToWait = Random.Range(minTime, maxTime);
			yield return new WaitForSeconds(secondsToWait);
        
			Vector2 maxScreenCoords = new Vector2(_currentRectMask.x * 0.5f, _currentRectMask.y * 0.5f);
			Vector2 screenPos = new Vector2(Random.Range(-maxScreenCoords.x, maxScreenCoords.x),Random.Range(-maxScreenCoords.y, maxScreenCoords.y));
			
			GameObject currentVisualObject = Instantiate(visual3dObject);
			float currentObjectScale = Random.Range(minObjectScale, maxObjectScale);
			float maxExtents = GetMaxExtents(currentVisualObject);

			Vector2 maxLocalCoords = new Vector2(Mathf.Tan(_cameraFovInDegrees.x / 2f * Mathf.Deg2Rad) * objectDistanceToCam - maxExtents, Mathf.Tan(_cameraFovInDegrees.y / 2f * Mathf.Deg2Rad) * objectDistanceToCam - maxExtents);
			Vector3 currentLocalCoords = new Vector3
			(
				MathLib.Remap(-maxScreenCoords.x, maxScreenCoords.x, -maxLocalCoords.x, maxLocalCoords.x, screenPos.x), 
				MathLib.Remap(-maxScreenCoords.y, maxScreenCoords.y, -maxLocalCoords.y, maxLocalCoords.y, screenPos.y),
				objectDistanceToCam
			);
			
			currentVisualObject.transform.position = _camera.transform.localToWorldMatrix.MultiplyPoint3x4(currentLocalCoords);
			currentVisualObject.transform.localScale = new Vector3(currentObjectScale, currentObjectScale, currentObjectScale);

			CircleClickObject objectScript = currentVisualObject.TryGetComponent(out objectScript) ? objectScript : currentVisualObject.AddComponent<CircleClickObject>();
			objectScript.onClickEvent = ClickCircle;
			
			StartCoroutine(SpawnButton());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			_puzzleMoveData.reachedDestination -= OnDisable;
			triggerObject.triggered -= TriggerEnter;
		}

		private IEnumerator ClickBackground()
		{
			yield return new WaitForSeconds(0.001f);
			if (_clickedObject)
			{
				_clickedObject = false;
				yield break;
			}
			_puzzleMoveData.additionToInterpolator -= speedSubtractEachFailedPress;
			
			if(failPressEmitter != null)
				failPressEmitter.Play();
		}

		private void TriggerEnter(Collider other)
		{
			CircleClickObject script = other.GetComponent<CircleClickObject>();
			if (script != null)
			{
				script.Disable();
				_puzzleMoveData.additionToInterpolator -= speedSubtractEachMissedObject;
				failPressEmitter.Play();
			}
		}

		private void ClickCircle(CircleClickObject script)
		{
			_clickedObject = true;
			script.Disable();
			_puzzleMoveData.additionToInterpolator += speedAddEachSuccessfulPress;
			
			if(successfulPressEmitter != null)
				successfulPressEmitter.Play();
		}

		private float GetMaxExtents(GameObject obj)
		{
			const float maxDeviationAllowed = 0.001f;
			if (obj.TryGetComponent(out BoxCollider boxCollider))
			{
				float size;
				
				if (boxCollider.size.x - boxCollider.size.y > maxDeviationAllowed ||
				    boxCollider.size.x - boxCollider.size.z > maxDeviationAllowed)
				{
					Debug.LogError("The box collider must have proportional size, using the smallest size axis");
					Vector3 boxSize = boxCollider.size;

					if (boxSize.x < boxSize.y)
						size = boxSize.x < boxSize.z ? boxSize.x : boxSize.z;
					else
						size = boxSize.y < boxSize.x ? boxSize.y : boxSize.x;
				}
				else
				{
					size = boxCollider.size.x;
				}

				size *= 0.5f;
				return Mathf.Sqrt(size * size * 2f);
			}
			
			if (obj.TryGetComponent(out SphereCollider sphereCollider))
			{
				return sphereCollider.radius;
			}
			
			throw new Exception("Please attach either a sphere collider or a box collider (with proportional size) to the root object of the circle click object prefab");
		}
	}

}