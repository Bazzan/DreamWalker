using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Items
{
    using Objects.Characters;
    using Objects.Player;

    [System.Serializable]
    public class InventoryManager
    {
        public InventoryManager() { }

        public InventoryManager(int inventorySize)
        {
            items = new Item[inventorySize];
        }

        public static InventoryManager GlobalInventory { get; private set; } = new InventoryManager(10);

        private System.Action<InventoryManager, Item, bool> _onCollectItem = delegate { };

        private System.Action<InventoryManager, Item> _onRemoveItem = delegate { };

        public void OnCollectItemSubscribe(System.Action<InventoryManager, Item, bool> action) => _onCollectItem += action;

        public void OnCollectItemUnsubscribe(System.Action<InventoryManager, Item, bool> action) => _onCollectItem -= action;

        public void OnRemoveItemSubscribe(System.Action<InventoryManager, Item> action) => _onRemoveItem += action;

        public void OnRemoveItemUnsubscribe(System.Action<InventoryManager, Item> action) => _onRemoveItem -= action;

        [SerializeField]
        internal Item[] items = new Item[10];

        [SerializeField]
        internal List<Item> keyItems = new List<Item>();

        public Item ReturnItem(int index) => items[Mathf.Clamp(index, 0, items.Length - 1)];

        public Item[] ReturnItems => items;

        public Item ReturnKeyItem(int index) => keyItems[Mathf.Clamp(index, 0, keyItems.Count - 1)];

        public Item[] ReturnKeyItems => keyItems.ToArray();

        /// <summary>
        /// Adds an item to the inventory. Returns whether
        /// or not the item was successfully added.
        /// </summary>
        /// <param name="item"></param>
        public bool CollectItem(Item item)
        {
            switch (item.ItemType)
            {
                default:
                    return false;

                case ItemType.Item:
                    return CollectItemStandard();

                case ItemType.KeyItem:
                    return CollectItemKey();
            }

            bool CollectItemStandard()
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null)
                    {
                        items[i] = item;
                        _onCollectItem(this, item, item.HasBeenCollectedOnce);
                        return true;
                    }
                }

                return false;
            }

            bool CollectItemKey()
            {
                if (keyItems.Contains(item))
                    return false;

                keyItems.Add(item);
                _onCollectItem(this, item, item.HasBeenCollectedOnce);
                return true;
            }
        }

        public void RemoveItem(Item item)
        {
            switch (item.ItemType)
            {
                case ItemType.Item:
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == item)
                        {
                            items[i] = null;
                            _onRemoveItem(this, item);
                            return;
                        }
                    }
                    break;

                case ItemType.KeyItem:
                    for (int i = 0; i < keyItems.Count; i++)
                    {
                        if (items[i] == item)
                        {
                            items[i] = null;
                            _onRemoveItem(this, item);
                            return;
                        }
                    }
                    break;
            }
        }

        public void ClearInventory()
        {
            items = new Item[0];
            keyItems.Clear();
        }

        /// <summary>
        /// Checks if a specific item object is stored in
        /// the inventory.
        /// </summary>
        /// <param name="item">Reference to the item.</param>
        public bool ContainsItem(Item item)
        {
            foreach (Item i in items)
            {
                if (i == item)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if any item of a certain type is stored
        /// in the inventory.
        /// </summary>
        /// <param name="itemType">The item type.</param>
        public bool ContainsItem(System.Type itemType)
        {
            if (itemType == typeof(Item))
            {
                foreach (Item i in items)
                {
                    if (i.GetType() == itemType)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if any item of a certain type is stored
        /// in the inventory, and returns the first instance.
        /// </summary>
        /// <param name="itemType">The item type.</param>
        /// <param name="firstItemOfType">The item type.</param>
        public bool ContainsItem(System.Type itemType, out Item firstItemOfType)
        {
            if (itemType == typeof(Item))
            {
                foreach (Item i in items)
                {
                    if (i.GetType() == itemType)
                    {
                        firstItemOfType = i;
                        return true;
                    }
                }
            }

            firstItemOfType = null;
            return false;
        }

        public bool ContainsKeyItem(Item item) => keyItems.Contains(item);

        public bool ContainsKeyItem(System.Type itemType)
        {
            if (itemType == typeof(Item))
            {
                foreach (Item i in keyItems)
                {
                    if (i.GetType() == itemType)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Uses an item in the inventory.
        /// </summary>
        /// <param name="inventoryItem"></param>
        public void UseItem(Item inventoryItem)
        {
            inventoryItem.OnUse();

            if (inventoryItem.ConsumeOnUse)
            {
                RemoveItem(inventoryItem);
            }
        }
    }


}