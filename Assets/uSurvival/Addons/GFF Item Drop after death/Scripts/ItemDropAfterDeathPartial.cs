using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("Item drop Chance Addon")]
        public int dropChance = 0;
        public bool dropFromInventory = true;
        public bool dropFromEquipment = true;
        public bool dropFromHotbar = true;
    }

    public partial class ItemDrop
    {
        //gff extended loot
        public bool destroy;
        [SyncVar] public double ownerTime;

        private void Update()
        {
            if (isServer && destroy)
            {
                if (ownerTime < NetworkTime.time) NetworkServer.Destroy(gameObject);
            }
        }
    }

    public partial class PlayerInventory
    {
        public float ownerDurationTime = 300f;
    }
}


namespace uSurvival
{
    public partial class DropRandomItemsOnDeath
    {
        public float ownerDurationTime = 300f;
    }
}