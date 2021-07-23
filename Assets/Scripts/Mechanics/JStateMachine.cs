using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Algorithms
{
    /// <summary>
    /// A generic state machine with abstract structure for use in any situation.
    /// Can both be declared as an instance as-is, or be inherited by another class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JStateMachine<T> where T : JStateMachine<T>.IState
    {
        public JStateMachine(T initialState, object owner = null)
        {
            _currentState = initialState;
            Owner = owner;
        }

        public object Owner { get; private set; }

        public T PreviousState { get; private set; }

        private T _currentState;
        public T CurrentState { get => _currentState; private set => ChangeState(value); }

        private IUpdateState _updateState;
        private IFixedUpdateState _fixedUpdateState;

        /// <summary>
        /// Changes what state the state machine processes. It is recommended that
        /// you provide a reference rather than declare a new state directly in
        /// the field.
        /// </summary>
        /// <param name="newState">The state to change to.</param>
        public void ChangeState(T newState)
        {
            _currentState?.StateExit(this, newState);

            PreviousState = _currentState;

            _currentState = newState;

            _updateState = newState as IUpdateState;
            _fixedUpdateState = newState as IFixedUpdateState;

            _currentState?.StateEnter(this);
        }

        /// <summary>
        /// Implement this to execute IUpdateStates.
        /// </summary>
        public virtual void StateMachineUpdate(float deltaTime)
        {
            _updateState?.StateUpdate(this, deltaTime);
        }

        /// <summary>
        /// Implement this to execute IFixedUpdateStates.
        /// </summary>
        public virtual void StateMachineFixedUpdate()
        {
            _fixedUpdateState?.StateFixedUpdate(this);
        }

        /// <summary>
        /// Marks the class as a generic state machine state. The fact that
        /// this is an interface, means that you can actually declare
        /// ScriptableObjects as passable states if you want (the benefit in
        /// this case is the fact that you can edit SOs at runtime).
        /// </summary>
        public interface IState
        {
            void StateEnter(JStateMachine<T> stateMachine);
            void StateExit(JStateMachine<T> stateMachine, IState nextState);
        }

        /// <summary>
        /// Implement this if the IState is supposed to update once every frame.
        /// </summary>
        public interface IUpdateState
        {
            void StateUpdate(JStateMachine<T> stateMachine, float deltaTime);
        }

        /// <summary>
        /// Implement this if the IState is supposed to update every physics step.
        /// </summary>
        public interface IFixedUpdateState
        {
            void StateFixedUpdate(JStateMachine<T> stateMachine);
        }
    }
}
