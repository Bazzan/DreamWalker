using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    using Characters;

    public class InteractableGravityPlatform : MonoBehaviour
    {
        private BoxCollider _boxCollider;

        bool hasPlayer = false;
        int platformState = 0;

        Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        private void OnTransformChildrenChanged()
        {
            if (transform.GetComponentInChildren<PlayerCharacter>())
            {
                if (!hasPlayer)
                {
                    hasPlayer = true;

                    anim.Play("PlatformMove" + platformState);

                    platformState = platformState == 0 ? 1 : 0;
                }
                else hasPlayer = false;
            }
            else hasPlayer = false;
        }
    }
}
