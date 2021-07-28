using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Items
{
    using Objects.Scriptables;

    //[CreateAssetMenu("")]
    public abstract class Item : ScriptableObject
    {
        public abstract bool HasBeenCollectedOnce { get; }
        public abstract bool IsUsableFromMenu { get; }
        public abstract bool ConsumeOnUse { get; }
        public virtual bool IsCollectableOnlyOnce => false;
        public virtual bool DetectCollision => true;

        [SerializeField]
        protected MovementData _movementData;
        public MovementData GetMovementData => _movementData;

        [SerializeField]
        protected string _defaultName;
        public string DefaultName => _defaultName;
        public abstract string Name { get; }

        [Space]

        [SerializeField]
        protected ItemType _itemType;
        public ItemType ItemType => _itemType;

        [SerializeField]
        protected GameObject _defaultModel;
        public GameObject DefaultModel => _defaultModel;
        public abstract GameObject Model { get; }

        [SerializeField]
        protected Texture2D _defaultIcon;
        public Texture2D DefaultIcon => _defaultIcon;
        public abstract Texture2D Icon { get; }

        /// <summary>
        /// Executed by the parent MonoBehaviour.
        /// </summary>
        protected internal abstract void OnCollect(InventoryManager inventory);

        protected internal abstract void OnUse();
    }

}

public enum ItemType
{
    Item,
    KeyItem
}