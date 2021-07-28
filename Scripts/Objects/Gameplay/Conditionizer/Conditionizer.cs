using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    using Items;

    [Serializable]
    public class Conditionizer
    {
        [SerializeReference]
        public List<Condition> conditions;

        public bool AllTrue()
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            return conditions.All((cond) => cond.IsTrue());
        }

        [Serializable]
        public abstract class Condition
        {
            public abstract bool IsTrue();
        }

        [Serializable]
        public class Condition_HasItem : Condition
        {
            [Header("If...")]
            public Item itemToCheck;
            [Header("...exists in the inventory of..."), Tooltip("The MonoBehaviour must inherit from IPossessInventory! (Player Character does.)")]
            public MonoBehaviour inventoryOwner;

            public bool isTrue;

            public override bool IsTrue()
            {
                if (inventoryOwner && itemToCheck && inventoryOwner is IPossessInventory possessor)
                {
                    bool result;
                    switch (itemToCheck.ItemType)
                    {
                        default:
                            result = false;
                            break;

                        case ItemType.Item:
                            result = possessor.Inventory.ContainsItem(itemToCheck);
                            break;

                        case ItemType.KeyItem:
                            result = possessor.Inventory.ContainsKeyItem(itemToCheck);
                            break;
                    }

                    if (!result && !isTrue)
                        result = !result;

                    return result;
                }

                return false;
            }
        }

#if UNITY_EDITOR
        [Header("Conditions are added in the context menu! (Click the icon in top right corner with the three dots)")]
        public Conditions condition;

        public void AddCondition()
        {
            Condition cond;
            switch (condition)
            {
                default:
                    return;

                case Conditions.HasItem:
                    cond = new Condition_HasItem();
                    break;
            }

            conditions.Add(cond);
        }
#endif
    }
}

public enum Conditions
{
    HasItem //Condition_HasItem
}