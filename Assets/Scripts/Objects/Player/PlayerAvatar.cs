using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Avatars
{
    using Algorithms;

    [System.Serializable]
    public class PlayerAvatar : BehaviourAvatar, IControllable
    {
        public PlayerAvatar(GameCharacter gameCharacter, PlayerState entryState = null) : base(gameCharacter)
        {
            _playerStateMachine = new JStateMachine<PlayerState>(entryState ?? new PS_Idle_Volatile(this), this);
        }

        private JStateMachine<PlayerState> _playerStateMachine;

        public float _foo;

        // The phase actions will be modified by whatever
        // PlayerState is currently running in the state machine.
        internal Dictionary<ButtonAction, PhaseAction> _phaseActions;

        internal override void AvatarUpdate(float deltaTime)
        {
            _playerStateMachine.StateMachineUpdate(deltaTime);
        }

        internal override void AvatarFixedUpdate()
        {
            _playerStateMachine.StateMachineFixedUpdate();
        }

        // Method called by InputManager to trigger input events.
        public PhaseAction GetPhaseAction(ButtonAction buttonAction)
        {
            return _phaseActions[buttonAction];
        }

        public abstract class PlayerState : JStateMachine<PlayerState>.IState
        {
            public PlayerState(PlayerAvatar user)
            {
                _player = user;
            }

            private PlayerAvatar _player;

            public abstract void StateEnter(JStateMachine<PlayerState> stateMachine);
            public abstract void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState);
        }

        /// <summary>
        /// The state in which the player normally moves.
        /// </summary>
        public class PS_Idle_Volatile : PlayerState
        {
            public PS_Idle_Volatile(PlayerAvatar user) : base(user) { }

            public override void StateEnter(JStateMachine<PlayerState> stateMachine)
            {
                throw new System.NotImplementedException();
            }

            public override void StateExit(JStateMachine<PlayerState> stateMachine, JStateMachine<PlayerState>.IState nextState)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public enum PlayerStates
    {
        Idle_Volatile
    }
}

