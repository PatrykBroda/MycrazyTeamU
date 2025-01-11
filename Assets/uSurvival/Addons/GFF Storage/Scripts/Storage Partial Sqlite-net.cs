using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        [Header("GFF Storage Addon")]
        [SerializeField] private StorageType storageMode;
        private enum StorageType { CommonToAllCharactersOnTheAccount, EachCharacterHasTheirOwnStorage }

        //can not use  : character_inventory
        class character_storage
        {
            public string owner { get; set; }
            public byte slot { get; set; }
            public string name { get; set; }
            public ushort amount { get; set; }

            public ushort ammo { get; set; }
            public string ammoname { get; set; }

            public ushort durability { get; set; }

            public bool binding { get; set; }
            public sbyte skin { get; set; }

            public string modules { get; set; }

            public string secondItemName { get; set; }
            public ushort secondItemDurability { get; set; }

            //item enchantment addon
            //public byte holes { get; set; }
            //public string upgradeInd { get; set; }

            // PRIMARY KEY (account, slot) is created manually.
        }

        public void Connect_Storage()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_storage>();
            connection.CreateIndex(nameof(character_storage), new[] { "account", "slot" });
        }

        public void CharacterLoad_Storage(Player player)
        {
            string owner;

            if (storageMode == StorageType.CommonToAllCharactersOnTheAccount)
            {
                accounts account = connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", player.account);
                player.storage.goldInStorage = account.goldInStorage;
                player.storage.storageSize = account.storageSize;

                owner = player.account;
            }
            else
            {
                characters character = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", player.name);
                player.storage.goldInStorage = character.goldInStorage;
                player.storage.storageSize = character.storageSize;

                owner = player.name;
            }

            // fill all slots first
            for (int i = 0; i < player.storage.storageSize; ++i)
                player.storage.slots.Add(new ItemSlot());

            foreach (character_storage row in connection.Query<character_storage>("SELECT * FROM character_storage WHERE owner=?", owner))
            {
                if (row.slot < player.storage.slots.Count)
                {
                    if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        Item item = new Item(itemData);
                        item.ammo = row.ammo;
                        item.ammoname = row.ammoname;
                        item.durability = (ushort)Mathf.Min(row.durability, item.maxDurability);

                        item.binding = row.binding;
                        item.skin = row.skin;

                        if (string.IsNullOrEmpty(row.secondItemName) == false && ScriptableItem.dict.TryGetValue(row.secondItemName.GetStableHashCode(), out ScriptableItem secondItemData))
                        {
                            item.secondItemHash = secondItemData.name.GetStableHashCode();
                            item.secondItemDurability = row.secondItemDurability;
                        }

                        item = LoadModulesForSlot(item, row.modules);

                        //item enchantment addon
                        //item.holes = row.holes;
                        //item.upgradeInd = LoadUpgradeInd(row.upgradeInd);

                        player.storage.slots[row.slot] = new ItemSlot(item, row.amount);
                    }
                    else Debug.LogWarning("LoadGuildStorage: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
                }
                else Debug.LogWarning("LoadStorage: skipped slot " + row.slot + " for " + player.name + " because it's bigger than size guild storage size");
            }
        }

        public void CharacterSave_Storage(Player player)
        {
            string owner;

            if (storageMode == StorageType.CommonToAllCharactersOnTheAccount)
            {
                connection.Execute("UPDATE accounts SET goldInStorage =?, storageSize =? WHERE name=?", player.storage.goldInStorage, player.storage.storageSize, player.account);

                owner = player.account;
            }
            else
            {
                connection.Execute("UPDATE characters SET goldInStorage =?, storageSize =? WHERE name=?", player.storage.goldInStorage, player.storage.storageSize, player.name);

                owner = player.name;
            }

            // remove old entries first, then add all new ones
            // (we could use UPDATE where slot=... but deleting everything makes
            // sure that there are never any ghosts)
            connection.Execute("DELETE FROM character_storage WHERE owner=?", owner);

            for (byte i = 0; i < player.storage.slots.Count; ++i)
            {
                ItemSlot slot = player.storage.slots[i];
                if (slot.amount > 0) // only relevant items to save queries/storage/time
                {
                    string secondItem = "";
                    if (ScriptableItem.dict.TryGetValue(slot.item.secondItemHash, out ScriptableItem itemData))
                    {
                        secondItem = itemData.name;
                    }

                    character_storage temp = new character_storage();

                    temp.owner = owner;
                    temp.slot = i;
                    temp.name = slot.item.name;
                    temp.amount = slot.amount;
                    temp.ammo = slot.item.ammo;
                    temp.ammoname = slot.item.ammoname;
                    temp.durability = slot.item.durability;

                    temp.binding = slot.item.binding;
                    temp.skin = slot.item.skin;
                    temp.modules = SaveModulesFromSlot(slot.item);

                    temp.secondItemName = secondItem;
                    temp.secondItemDurability = slot.item.secondItemDurability;

                    //item enchantment addon
                    //temp.holes = slot.item.holes;
                    //temp.upgradeInd = SaveUpgradeInd(slot.item);

                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(temp);
                }
            }
        }
    }
}