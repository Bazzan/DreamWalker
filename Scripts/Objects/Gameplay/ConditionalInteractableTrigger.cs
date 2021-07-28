using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    public class ConditionalInteractableTrigger : Interactable
    {
        [SerializeField]
        private Conditionizer _conditionizer;

        private void OnTriggerEnter(Collider other)
        {
            if (enabled)
                base.Interact();
        }

        public override bool IsCurrentlyInteractable()
        {
            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Add Condition")]
        public void AddCondition()
        {
            _conditionizer.AddCondition();
        }
#endif
    }
}
