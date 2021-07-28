using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Items
{
    [CreateAssetMenu(menuName = "Items/Pyramid Puzzle Artifact")]
    public class Item_PyramidPuzzleArtifact : Item
    {
        public static bool HasBeenCollected { get; private set; }

        public override bool HasBeenCollectedOnce => HasBeenCollected;

        public override bool IsUsableFromMenu => false;

        public override bool ConsumeOnUse => true;

        public override bool DetectCollision => false;

        public override string Name => _defaultName;

        public override GameObject Model => _defaultModel;

        public override Texture2D Icon => _defaultIcon;

        public override bool IsCollectableOnlyOnce => true;


        protected internal override void OnCollect(InventoryManager inventory)
        {
            if (!HasBeenCollected)
            {
                HasBeenCollected = true;
                inventory.CollectItem(this);
            }
        }

        protected internal override void OnUse()
        {
            
        }
    }

}