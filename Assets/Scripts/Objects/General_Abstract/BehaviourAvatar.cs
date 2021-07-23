using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Avatars
{
    /// <summary>
    /// The abstract base class for all avatars (i.e. objects that possess and control GameCharacter behaviours).
    /// </summary>
    public abstract class BehaviourAvatar
    {
        public BehaviourAvatar(GameCharacter gameCharacter)
        {
            Character = gameCharacter;
        }

        /// <summary>The GameCharacter the BehaviourAvatar possesses.</summary>
        public GameCharacter Character { get; private set; }

        internal abstract void AvatarUpdate(float deltaTime);

        internal abstract void AvatarFixedUpdate();
    }
}
