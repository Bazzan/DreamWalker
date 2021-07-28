using UnityEngine;

namespace GP2_Team7.Objects.Characters
{
	using Algorithms;
	using Items;
	using Modules;
	using Cameras;
	using Managers;
	using System;
	using Libraries;

	public partial class PlayerCharacter : GameCharacter, IControllable, IFocable, IGravity, IPossessInventory, IPortalTraversable
	{
		private float _animTransitionTime = 0.2f;

		public abstract class PlayerState : JStateMachine<PlayerState>.IState
		{
			public PlayerState(PlayerCharacter user)
			{
				_player = user;
			}

			protected void Rotate(Quaternion to)
			{
				if (CameraController.Focable == _player as IFocable || CameraController.FocusPoint == _player._transform)
					_player.Rotate(to);
			}

			public abstract bool CanCheckCollision();

			protected PlayerCharacter _player;

			public abstract void StateEnter(JStateMachine<PlayerState> stateMachine);
			public abstract void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState);
		}

		/// <summary>
		/// The state in which the player normally moves.
		/// </summary>
		public class PS_Idle_Volatile : PlayerState, JStateMachine<PlayerState>.IUpdateState, JStateMachine<PlayerState>.IFixedUpdateState
		{
			public PS_Idle_Volatile(PlayerCharacter user) : base(user) { }

			Vector3 _inputDirection;

			public override bool CanCheckCollision() => true;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				_player._speed.y = 0;
				_player._phaseActions[Actions.Forward] = new ActionDelegates(null, OnKeyForward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Backward] = new ActionDelegates(null, OnKeyBackward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Left] = new ActionDelegates(null, OnKeyLeft, OnKeyLeftRightRelease);
				_player._phaseActions[Actions.Right] = new ActionDelegates(null, OnKeyRight, OnKeyLeftRightRelease);

				_player._collisionModule.AssignCollisionExitCallback(OnCollisionExit);

				if (!_player._collisionModule.OnGround)
					OnCollisionExit(ColSide.Ground);

				Debug.Log("idle ouchie");

				if (_player._playerStateMachine.PreviousState is PS_Airborne)
				{

					_player._cloneAnim?.Play("Landing", 0, 0);
					_player._anim.Play("Landing", 0, 0);
				}
				else
				{
					_player._cloneAnim?.CrossFadeInFixedTime("Idle", _player._animTransitionTime, 0);
					_player._anim.CrossFadeInFixedTime("Idle", _player._animTransitionTime, 0);

                }
			}

			public void StateUpdate(JStateMachine<PlayerState> stateMachine, float deltaTime)
			{
				Quaternion rotateTo = CameraController.IsInStandardMode ?
					CameraController.RotationY :
					_player.LocalGravityRotation * Quaternion.Euler(0, CameraController.GetMouseInputWithSensitivity().x, 0);

				float smoothTrans = 15 * deltaTime;

				_player._anim.SetFloat("X", Mathf.MoveTowards(_player._anim.GetFloat("X"), _inputDirection.x, smoothTrans));
				_player._anim.SetFloat("Y", Mathf.MoveTowards(_player._anim.GetFloat("Y"), _inputDirection.z, smoothTrans));

				_player._cloneAnim?.SetFloat("X", Mathf.MoveTowards(_player._cloneAnim.GetFloat("X"), _inputDirection.x, smoothTrans));
				_player._cloneAnim?.SetFloat("Y", Mathf.MoveTowards(_player._cloneAnim.GetFloat("Y"), _inputDirection.z, smoothTrans));

				Rotate(rotateTo);
			}

			public void StateFixedUpdate(JStateMachine<PlayerState> stateMachine)
			{
				Vector3 vector;

				if (_inputDirection == Vector3.zero)
					vector = Vector3.zero;
				else
					vector = _player.VectorToCameraRotation(_inputDirection)
					         * _player._moveData.moveSpeed;

				const string runName = "Running";

				if (_inputDirection == Vector3.zero)
				{
					_player._cloneAnim?.SetBool(runName, false);

                    _player._anim.SetBool(runName, false);
				}
				else
				{
					_player._cloneAnim?.SetBool(runName, true);

                    _player._anim.SetBool(runName, true);
				}

				_player.Move(vector, _player._moveData.acceleration);
			}

			private void OnCollisionExit(ColSide side)
			{
				if (side == ColSide.Ground)
					_player._playerStateMachine.ChangeState(new PS_Airborne(_player));
			}

			private void OnKeyForward()
			{
				_inputDirection.z = 1;
			}

			private void OnKeyForwardBackwardRelease()
			{
				_inputDirection.z = 0;
			}

			private void OnKeyBackward()
			{
				_inputDirection.z = -1;
			}

			private void OnKeyRight()
			{
				_inputDirection.x = 1;
			}

			private void OnKeyLeftRightRelease()
			{
				_inputDirection.x = 0;
			}

			private void OnKeyLeft()
			{
				_inputDirection.x = -1;
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
				_player._phaseActions[Actions.Forward].Unsubscribe(OnKeyForward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Backward].Unsubscribe(OnKeyBackward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Left].Unsubscribe(OnKeyLeft, OnKeyLeftRightRelease);
				_player._phaseActions[Actions.Right].Unsubscribe(OnKeyRight, OnKeyLeftRightRelease);

				_player._collisionModule.UnassignCollisionExitCallback(OnCollisionExit);
			}
		}

		/// <summary>
		/// The state in which the player normally moves.
		/// </summary>
		public class PS_Airborne : PlayerState, JStateMachine<PlayerState>.IUpdateState, JStateMachine<PlayerState>.IFixedUpdateState
		{
			public PS_Airborne(PlayerCharacter user) : base(user) { }

			Vector3 _inputDirection;

			public override bool CanCheckCollision() => true;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				_player._phaseActions[Actions.Forward] = new ActionDelegates(null, OnKeyForward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Backward] = new ActionDelegates(null, OnKeyBackward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Left] = new ActionDelegates(null, OnKeyLeft, OnKeyLeftRightRelease);
				_player._phaseActions[Actions.Right] = new ActionDelegates(null, OnKeyRight, OnKeyLeftRightRelease);

				_player._collisionModule.AssignCollisionEnterCallback(OnCollisionEnter);
				_player._collisionModule.AssignCollisionStayCallback(OnCollisionEnter);

				Debug.Log("hey look im flying");

				_player._cloneAnim?.CrossFadeInFixedTime("Fall", _player._animTransitionTime, 0);
				_player._anim.CrossFadeInFixedTime("Fall", _player._animTransitionTime, 0);
			}

			public void StateUpdate(JStateMachine<PlayerState> stateMachine, float deltaTime)
			{
				Rotate(CameraController.RotationY);
			}

			public void StateFixedUpdate(JStateMachine<PlayerState> stateMachine)
			{
				Vector3 vector;

				if (_inputDirection == Vector3.zero)
					vector = Vector3.zero;
				else
					vector = _player.VectorToCameraRotation(_inputDirection) * _player._moveData.moveSpeed;

				//print(vector);

				_player.Move(vector, _player._moveData.acceleration * 0.5f);

				_player.Gravity();
			}

			private void OnKeyForward()
			{
				_inputDirection.z = 1;
			}

			private void OnKeyForwardBackwardRelease()
			{
				_inputDirection.z = 0;
			}

			private void OnKeyBackward()
			{
				_inputDirection.z = -1;
			}

			private void OnKeyRight()
			{
				_inputDirection.x = 1;
			}

			private void OnKeyLeftRightRelease()
			{
				_inputDirection.x = 0;
			}

			private void OnKeyLeft()
			{
				_inputDirection.x = -1;
			}

			private void OnCollisionEnter(ColSide side, CollisionModule.ColHit colHit)
			{
				if (side == ColSide.Ground)
					_player._playerStateMachine.ChangeState(new PS_Idle_Volatile(_player));
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
				_player._phaseActions[Actions.Forward].Unsubscribe(OnKeyForward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Backward].Unsubscribe(OnKeyBackward, OnKeyForwardBackwardRelease);
				_player._phaseActions[Actions.Left].Unsubscribe(OnKeyLeft, OnKeyLeftRightRelease);
				_player._phaseActions[Actions.Right].Unsubscribe(OnKeyRight, OnKeyLeftRightRelease);

				_player._collisionModule.UnassignCollisionEnterCallback(OnCollisionEnter);
				_player._collisionModule.UnassignCollisionStayCallback(OnCollisionEnter);
			}
		}

		/// <summary>
		/// A state where the players movement is locked (used in InteractableEventSystem to lock movement and camera rotation when for example opening a UI Diary)
		/// </summary>
		public class PS_LockedMovement : PlayerState
		{
			public PS_LockedMovement(PlayerCharacter user) : base(user) {}

			public override bool CanCheckCollision() => false;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				_player._transform.parent = null;
				_player._speed = Vector3.zero;
				
				_player._cloneAnim?.CrossFadeInFixedTime("Idle", _player._animTransitionTime, 0);
				_player._anim.CrossFadeInFixedTime("Idle", _player._animTransitionTime, 0);
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
				
			}
		}

		public class PS_PlatformTraversal : PlayerState, JStateMachine<PlayerState>.IUpdateState
		{
			public PS_PlatformTraversal(PlayerCharacter user, MoveToPlatformData moveToPlatformData) : base(user)
			{
				_moveToPlatformData = moveToPlatformData;
			}

			public override bool CanCheckCollision() => false;

			private Vector3 _from;

			private float _moveProgress = 0f;

			private float timeSinceStart = 0f;

			private MoveToPlatformData _moveToPlatformData;

			private CameraShake _cameraShake;
			
			private bool _playedSound = false;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				_player._transform.parent = null;

				if (_player._playerModel)
					_player._playerModel.SetActive(false);

				_player._speed = Vector3.zero;

				ActivateParticle(_player._platformDashStartFX, ref _player._platformDashStartFXSpawned);
				ActivateParticle(_player._platformDashFX, ref _player._platformDashFXSpawned);

				_player._platformDashFXSpawned.transform.parent = _player._transform;
				_player._platformDashFXSpawned.transform.localPosition += Vector3.forward * 0.5f;

				_player._platformDashFXSpawned.Play(true);
				_player._platformDashStartFXSpawned.transform.parent = null;

				_cameraShake = new CameraShake(new CameraShakeValues(0.25f, 65, _moveToPlatformData.speedCurve[Math.Max(0, _moveToPlatformData.speedCurve.length - 1)].time, ShakeFalloffType.Boolean), null);

				CameraController.Shake(_cameraShake);

				_player._soundEmitter.Play("Jump");

				_from = _player._transform.position;
                
				_moveProgress = 0;
			}

			public void StateUpdate(JStateMachine<PlayerState> stateMachine, float deltaTime)
			{
				if (_moveProgress > 0.8f && !_playedSound)
				{
					_playedSound = true;
					_player._soundEmitter.TriggerCue("Jump");
				}
                
				if (_moveProgress < 1f)
				{
					timeSinceStart += deltaTime;
					_moveProgress = _moveToPlatformData.speedCurve.Evaluate(timeSinceStart);
					_player._transform.position = MathLib.BezierLerp(_from, _moveToPlatformData.Destination, _moveToPlatformData.playerBezierTangent, _moveToPlatformData.platformBezierTangent, _moveProgress);

					if (_player._collisionModule.DeltaVector != Vector3.zero)
						_player._platformDashFXSpawned.transform.rotation = Quaternion.LookRotation(-_player._collisionModule.DeltaVector);
				}
				else
				{
					_player._transform.rotation = Quaternion.Euler(_player._gravityRotation) * _player._localGravityRotation;
					_player._playerStateMachine.ChangeState(new PS_Idle_Volatile(_player));
					_moveToPlatformData.finishedMoving();
				}
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
				if (_player._playerModel)
					_player._playerModel.SetActive(true);

				_player._platformDashFXSpawned.Stop(true);

				_player._platformDashFXSpawned.transform.parent = null;

				ActivateParticle(_player._platformDashEndFX, ref _player._platformDashEndFXSpawned);

				_player._platformDashEndFXSpawned.transform.parent = null;

				CameraController.RemoveShake(_cameraShake);

				_player._soundEmitter.TriggerCue("Jump");
			}

			private void ActivateParticle(ParticleSystem original, ref ParticleSystem pooledReference)
			{
				if (!pooledReference && original)
					pooledReference = Instantiate(original, _player._transform).GetComponent<ParticleSystem>();
				else
					pooledReference.gameObject.SetActive(true);

				pooledReference.gameObject.SetActive(true);

				pooledReference.transform.position = _player.FocusPoint;
				pooledReference.transform.localRotation = Quaternion.Euler(0, 180, 0);
			}
		}

		public class PS_BlackRoom : PlayerState, JStateMachine<PlayerState>.IUpdateState
		{
			public PS_BlackRoom(PlayerCharacter user, CircleClickPuzzleData data) : base(user)
			{
				_canCheckCollision = true;
				_data = data;
			}

			private bool _canCheckCollision = true;
			private CircleClickPuzzleData _data;
			private float interpolator = 0f;

			private Vector3 _enterOffset;

			public override bool CanCheckCollision() => _canCheckCollision;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				_player._anim.CrossFade("StruggleForward", 0.2f, 0);
				_player._cloneAnim.CrossFade("StruggleForward", 0.2f, 0);
				_player._speed = Vector3.zero;

				_enterOffset = Vector3.right * (_player._transform.position.x - _data.originPosition.x);

				_player._transform.position = _data.originPosition + _enterOffset;
				_player._transform.rotation = Quaternion.identity;
			}

			public void StateUpdate(JStateMachine<PlayerState> stateMachine, float deltaTime)
			{
				if (CameraController.FocusPoint == _player._transform && 
					CameraController.Main.FocusOnPortalClone || (_player._transform.position.z - _data.originPosition.z) < Mathf.Abs(CameraController.MainCamera.transform.localPosition.z))
				{
					_data.additionToInterpolator = 0.03f;
				}
				else if (interpolator < 0f)
				{
					interpolator = 0f;
					_data.additionToInterpolator = 0f;
				}
                
				if (interpolator < 1f)
				{
					_player._anim.SetFloat("Y", Mathf.Clamp01(_data.additionToInterpolator * 10f));
					_player._cloneAnim.SetFloat("Y", Mathf.Clamp01(_data.additionToInterpolator * 10f));
					_player._transform.position = Vector3.Lerp(_data.originPosition, _data.destinationPosition, interpolator) + _enterOffset;
					interpolator += _data.additionToInterpolator * deltaTime;
				}
				else
				{
					_player._playerStateMachine.ChangeState(new PS_Idle_Volatile(_player));
				}
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
                _data.reachedDestination.Invoke();
            }
		}

		public class PS_VoidOut : PlayerState, JStateMachine<PlayerState>.IUpdateState, JStateMachine<PlayerState>.IFixedUpdateState, ICutscene
		{
			public PS_VoidOut(PlayerCharacter user) : base(user)
			{
				_canCheckCollision = true;
			}

			public override bool CanCheckCollision() => _canCheckCollision;

			public float? CutsceneTime { get => null; set { } }

			public float? CutsceneLength => null;

			public bool IsInterruptible => false;

			public Action<Action[]> OnCutsceneEnd { get; set; }

			private bool _canCheckCollision = true;

			public override void StateEnter(JStateMachine<PlayerState> stateMachine)
			{
				if (_player._collisionModule.OnGround)
					_player._speed = Vector3.zero;

				CutsceneManager.PlayCutscene(this, new CamFixedViewSettings());
			}

			public void StateFixedUpdate(JStateMachine<PlayerState> stateMachine)
			{
				_player.Move(Vector3.zero, _player._moveData.acceleration * 0.5f);

				//_player.Gravity();
			}

			public void StateUpdate(JStateMachine<PlayerState> stateMachine, float deltaTime)
			{
				//throw new System.NotImplementedException();
			}

			public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
			{
				//throw new System.NotImplementedException();
			}

			public void OnCutsceneStart(float startPosition)
			{
				UIManager.SubscribeOnHasFaded(OnHasFaded, true);

				UIManager.FadeScreen(true, 1, 2f);
			}

			public void OnCutsceneUpdate() { }

			public bool OnSkip(float skipProgress) => false;

			public void StopCutscene()
			{
				UIManager.SubscribeOnHasFaded(OnHasFaded, false);
			}

			private void OnHasFaded(bool wasFadeOut)
			{
				if (wasFadeOut)
				{
					GameManager.GetRespawnPoint(out Vector3 respawnPosition, out Quaternion respawnRotation);

					_player._collisionModule.Teleport(respawnPosition);

					_player.LocalGravityRotation = respawnRotation;

					CameraController.SwitchToStandardCameraView();

					_player._playerStateMachine.ChangeState(new PS_Idle_Volatile(_player));

					UIManager.FadeScreen(false, 1, 1f);
				}
				else
				{
					StopCutscene();
					OnCutsceneEnd(null);
				}
			}
		}
	}

	[Serializable]
	public struct MoveToPlatformData
	{
		public MoveToPlatformData(Transform platformTransform, Vector3 platformOffset, AnimationCurve speedCurve, Vector3 playerBezierTangent, Vector3 platformBezierTangent, Action finishedMoving)
		{
			this.platformTransform = platformTransform;
			this.platformOffset = platformOffset;
			this.speedCurve = speedCurve;
			this.playerBezierTangent = playerBezierTangent;
			this.platformBezierTangent = platformBezierTangent;
			this.finishedMoving = finishedMoving;
		}

		public Transform platformTransform;

		public Vector3 platformOffset;

		public AnimationCurve speedCurve;

		public Vector3 playerBezierTangent;

		public Vector3 platformBezierTangent;
        
		public Action finishedMoving;

		public Vector3 Destination => platformTransform.position + platformOffset;
	}

	[Serializable]
	public class CircleClickPuzzleData
	{
		public CircleClickPuzzleData(float additionToInterpolator, Vector3 originPosition, Vector3 destinationPosition, Action reachedDestination)
		{
			this.additionToInterpolator = additionToInterpolator;
			this.originPosition = originPosition;
			this.destinationPosition = destinationPosition;
			this.reachedDestination = reachedDestination;
		}

		public float additionToInterpolator;
		public Vector3 originPosition;
		public Vector3 destinationPosition;
		public Action reachedDestination;
	}

	public enum PlayerStates
	{
		Idle_Volatile
	}
}