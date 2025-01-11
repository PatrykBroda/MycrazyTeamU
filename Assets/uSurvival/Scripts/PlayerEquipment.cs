using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Mirror;
using GFFAddons;

namespace uSurvival
{
    [Serializable]
    public struct EquipmentInfo
    {
        public string requiredCategory;
        public Transform location;
        public ScriptableItemAndAmount defaultItem;
    }

    [RequireComponent(typeof(Animator))]
    public partial class PlayerEquipment : Equipment
    {
        // Used components. Assign in Inspector. Easier than GetComponent caching.
        public Animator animator;

        public EquipmentInfo[] slotInfo =
        {
            new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
            new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
            new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
            new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
        };

        // weapon location is a special case because it's no equipment slot, but
        // still needs a transform to be assigned the model etc.
        // IMPORTANT: use an empty weapon mount object that has no children,
        //            otherwise they will be destroyed by RefreshLocation!
        [FormerlySerializedAs("rightHandLocation")]
        public Transform weaponMount;

        [Header("Avatar")]
        public Camera avatarCamera;

        // cached SkinnedMeshRenderer bones without equipment, by name
        Dictionary<string, Transform> skinBones = new Dictionary<string, Transform>();

        void Awake()
        {
            // cache all default SkinnedMeshRenderer bones without equipment
            // (we might have multiple SkinnedMeshRenderers e.g. on feet, legs, etc.
            //  so we need GetComponentsInChildren)
            foreach (SkinnedMeshRenderer skin in GetComponentsInChildren<SkinnedMeshRenderer>())
                foreach (Transform bone in skin.bones)
                    skinBones[bone.name] = bone;

            // make sure that weaponmount is an empty transform without children.
            // if someone drags in the right hand, then all the fingers would be
            // destroyed by RefreshLocation.
            // => only check in awake once, because at runtime it will have children
            //    if a weapon is equipped (hence we don't check in OnValidate)
            if (weaponMount != null && weaponMount.childCount > 0)
                Debug.LogWarning($"{name} PlayerEquipment.weaponMount should have no children, otherwise they will be destroyed.");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // setup synclist callbacks on client. no need to update and show and
            // animate equipment on server
            slots.Callback += OnEquipmentChanged;

            // refresh all locations once (on synclist changed won't be called for
            // initial lists)
            // -> needs to happen before ProximityChecker's initial SetVis call,
            //    otherwise we get a hidden character with visible equipment
            //    (hence OnStartClient and not Start)
            for (int i = 0; i < slots.Count; ++i)
                RefreshLocation(i);

            RefreshLocations();
        }

        bool CanReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
        {
            // are all equipment SkinnedMeshRenderer bones in the player bones?
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            foreach (Transform bone in equipmentSkin.bones)
                if (!skinBones.ContainsKey(bone.name))
                    return false;
            return true;
        }

        // replace all equipment SkinnedMeshRenderer bones with the original player
        // bones so that the equipment animation works with IK too
        // (make sure to check CanReplaceAllBones before)
        void ReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
        {
            // get equipment bones
            Transform[] bones = equipmentSkin.bones;

            // replace each one
            for (int i = 0; i < bones.Length; ++i)
            {
                string boneName = bones[i].name;
                if (!skinBones.TryGetValue(boneName, out bones[i]))
                    Debug.LogWarning($"{equipmentSkin.name} bone {boneName} not found in original player bones. Make sure to check CanReplaceAllBones before.");
            }

            // reassign bones
            equipmentSkin.bones = bones;
        }

        void RebindAnimators()
        {
            foreach (var anim in GetComponentsInChildren<Animator>())
                anim.Rebind();
        }

        public void RefreshLocation(Transform location, ItemSlot slot)
        {
            // valid item, not cleared?
            if (slot.amount > 0)
            {
                ScriptableItem itemData = slot.item.data;
                //Debug.Log("RefreshLocation : " + slot.item.name + " / " + location.childCount + " / " + (itemData.modelPrefab == null) + " / " + location.GetChild(0).name != itemData.modelPrefab.name);
                // new model? (don't do anything if it's the same model, which
                // happens after only Item.ammo changed, etc.)
                // note: we compare .name because the current object and prefab
                // will never be equal
                if (location.childCount == 0 || itemData.modelPrefab == null ||
                    location.GetChild(0).name != itemData.modelPrefab.name)
                {
                    // delete old model (if any)
                    if (location.childCount > 0)
                        Destroy(location.GetChild(0).gameObject);

                    // use new model (if any)
                    if (itemData.modelPrefab != null)
                    {
                        GameObject go;
                        if (itemData is RangedWeaponItem weapon)
                        {
                            GameObject model;
                            // instantiate and parent
                            int weaponIndex = player.weaponSkins.GetSelectedSkinIndex(weapon.name);
                            if (weaponIndex == -1) model = itemData.modelPrefab;
                            else model = weapon.GetModel(weaponIndex, player.weaponSkins.selectedskins[weaponIndex].skin);

                            // instantiate and parent
                            go = Instantiate(model, location, false);
                            go.name = itemData.modelPrefab.name; // avoid "(Clone)"

                            WeaponModulesEnable details = model.GetComponentInChildren<WeaponModulesEnable>();
                            if (details != null)
                            {
                                if (details.silencer != null) details.silencer.SetActive(slot.item.modulesHash[0] != 0);

                                //scope
                                if (details.sights != null && details.sights.Length > 0 && slot.item.modulesHash[3] != 0)
                                {
                                    if (ScriptableItem.dict.TryGetValue(slot.item.modulesHash[3], out ScriptableItem _itemData) && _itemData is ScriptableWeaponModule module)
                                    {
                                        for (int x = 0; x < details.sights.Length; x++)
                                        {
                                            if (details.sights[x].name == module.name) details.sights[x].SetActive(true);
                                            else details.sights[x].SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        for (int x = 0; x < details.sights.Length; x++)
                                            details.sights[x].SetActive(false);
                                    }
                                }
                                else
                                {
                                    for (int x = 0; x < details.sights.Length; x++)
                                        details.sights[x].SetActive(false);
                                }
                            }
                        }
                        else
                        {
                            // instantiate and parent
                            go = Instantiate(itemData.modelPrefab, location, false);
                            go.name = itemData.modelPrefab.name; // avoid "(Clone)"
                        }

                        // skinned mesh and all bones can be be replaced?
                        // then replace all. this way the equipment can follow IK
                        // too (if any).
                        // => this is the RECOMMENDED method for animated equipment.
                        //    name all equipment bones the same as player bones and
                        //    everything will work perfectly
                        // => this is the ONLY way for equipment to follow IK, e.g.
                        //    in games where arms aim up/down.
                        SkinnedMeshRenderer equipmentSkin = go.GetComponentInChildren<SkinnedMeshRenderer>();
                        if (equipmentSkin != null && CanReplaceAllBones(equipmentSkin))
                            ReplaceAllBones(equipmentSkin);

                        // animator? then replace controller to follow player's
                        // animations
                        // => this is the ALTERNATIVE method for animated equipment.
                        //    add the Animator and use the player's avatar. works
                        //    for animated pants, etc. but not for IK.
                        // => this is NECESSARY for 'external' equipment like wings,
                        //    staffs, etc. that should be animated but don't contain
                        //    the same bones as the player.
                        Animator anim = go.GetComponent<Animator>();
                        if (anim != null)
                        {
                            // assign main animation controller to it
                            anim.runtimeAnimatorController = animator.runtimeAnimatorController;

                            // restart all animators, so that skinned mesh equipment will be
                            // in sync with the main animation
                            RebindAnimators();
                        }
                    }
                }
            }
            else
            {
                // empty now. delete old model (if any)
                if (location.childCount > 0)
                    Destroy(location.GetChild(0).gameObject);
            }
        }

        void RefreshLocation(int index)
        {
            ItemSlot slot = slots[index];
            EquipmentInfo info = slotInfo[index];

            // valid category and valid location? otherwise don't bother
            if (info.requiredCategory != "" && info.location != null)
                RefreshLocation(info.location, slot);
        }

        void OnEquipmentChanged(SyncListItemSlot.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
        {
            // at first, check if the item model actually changed. we don't need to
            // refresh anything if only the durability changed.
            // => this fixes a bug where attack animations were constantly being
            //    reset for no obvious reason. this happened because with durability
            //    items, any time we get attacked the equipment durability changes.
            //    this causes the OnEquipmentChanged hook to be fired, which would
            //    then refresh the location and rebind the animator, causing the
            //    animator to start at the entry state again (hence restart the
            //    animation).
            //
            // note: checking .data is enough. we don't need to check as deep as
            //       .data.model. this way we avoid the EquipmentItem cast.
            ScriptableItem oldItem = oldSlot.amount > 0 ? oldSlot.item.data : null;
            ScriptableItem newItem = newSlot.amount > 0 ? newSlot.item.data : null;
            if (oldItem != newItem)
            {
                // update the model
                RefreshLocation(index);
            }
        }

        // swap inventory & equipment slots to equip/unequip. used in multiple places
        [Server]
        public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                //gff
                player.InventorySizeOperationsAllowed(equipmentIndex, inventoryIndex) &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count)
            {
                // item slot has to be empty (unequip) or equipable
                ItemSlot slot = inventory.slots[inventoryIndex];
                if (slot.amount == 0 ||
                slot.item.data is UsableItem usableItem && usableItem.CanEquip(this, inventoryIndex, equipmentIndex))
                {
                    // swap them
                    ItemSlot temp = slots[equipmentIndex];
                    slots[equipmentIndex] = slot;
                    inventory.slots[inventoryIndex] = temp;

                    //customization addon
                    SwapInventoryEquipForCustomization(inventoryIndex, equipmentIndex);

                    //increase inventory addon (after customization addon)
                    player.inventory.UpdateInventorySize(inventoryIndex, equipmentIndex);
                }
            }
            else
            {
                //Debug.Log("Swap inventory equip in error");
            }
        }

        [Command]
        public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
        {
            SwapInventoryEquip(inventoryIndex, equipmentIndex);
        }

        [Command]
        public void CmdMergeInventoryEquip(int inventoryIndex, int equipmentIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = inventory.slots[inventoryIndex];
                ItemSlot slotTo = slots[equipmentIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        inventory.slots[inventoryIndex] = slotFrom;
                        slots[equipmentIndex] = slotTo;
                    }
                }
            }
        }

        [Command]
        public void CmdMergeEquipInventory(int equipmentIndex, int inventoryIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and in the equipment
            if (health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = slots[equipmentIndex];
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        slots[equipmentIndex] = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }

        [Command]
        public void CmdDropItem(int index)
        {
            // validate
            if (health.current > 0 && 0 <= index && index < slots.Count && slots[index].amount > 0 && CanDrop(index))
            {
                DropItemAndClearSlot(index);
            }
        }

        // helpers for easier slot access //////////////////////////////////////////
        // GetEquipmentTypeIndex("Chest") etc.
        public int GetEquipmentTypeIndex(string category)
        {
            // avoid Linq because it is HEAVY(!) on GC and performance
            for (int i = 0; i < slotInfo.Length; ++i)
                if (slotInfo[i].requiredCategory == category)
                    return i;
            return -1;
        }

        // death & respawn /////////////////////////////////////////////////////////
        [Server]
        public void DropItemAndClearSlot(int index)
        {
            UpdateInventorySizeAfterDeath(index);

            // drop and remove from inventory
            ItemSlot slot = slots[index];
            ((PlayerInventory)inventory).DropItem(slot.item, slot.amount);
            slot.amount = 0;
            slots[index] = slot;

            customization.RemoveItemFromEquip(((UsableItem)slot.item.data).equipmentItemType);
        }

        // drop all equipment on death, so others can loot us
        [Server]
        public void OnDeath()
        {
            for (int i = 0; i < slots.Count; ++i)
                if (slots[i].amount > 0 && !slots[i].item.binding && slots[i].item.data.dropFromEquipment && player.CheckZoneForDropItems())
                    DropItemAndClearSlot(i);
        }

        // we don't clear items on death so that others can still loot us. we clear
        // them on respawn.
        [Server]
        public void OnRespawn()
        {
            // for each slot: make empty slot or default item if any
            for (int i = 0; i < slotInfo.Length; ++i)
            {
                if (slotInfo[i].defaultItem.item != null)
                {
                    slots[i] = new ItemSlot(new Item(slotInfo[i].defaultItem.item), slotInfo[i].defaultItem.amount);
                }
                else
                {
                    slots[i] = new ItemSlot();
                }
            }
        }

        // durability //////////////////////////////////////////////////////////////
        public void OnReceivedDamage(Entity attacker, int damage)
        {
            // reduce durability in each item
            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].amount > 0 && slots[i].item.data is WeaponItem == false)
                {
                    ItemSlot slot = slots[i];
                    slot.item.durability = (ushort)Mathf.Clamp(slot.item.durability - damage, 0, slot.item.maxDurability);
                    slots[i] = slot;
                }
            }
        }

        // drag & drop /////////////////////////////////////////////////////////////
        void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                inventory.slots[slotIndices[0]].item.data.Equals(slots[slotIndices[1]].item.data))
            {
                CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                //CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
                CmdSwapFromInventoryToEquip(slotIndices[0], slotIndices[1]);
            }
        }

        void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

            // merge? (just check equality, rest is done server sided)
            if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                slots[slotIndices[0]].item.data.Equals(inventory.slots[slotIndices[1]].item.data))
            {
                CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
            }
            // swap?
            else
            {
                //CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]);
                CmdSwapFromEquipToInventory(slotIndices[1], slotIndices[0]);
            }
        }

        void OnDragAndClear_EquipmentSlot(int slotIndex)
        {
            CmdDropItem(slotIndex);
        }

        // validation
        protected override void OnValidate()
        {
            base.OnValidate();

            // it's easy to set a default item and forget to set amount from 0 to 1
            // -> let's do this automatically.
            for (int i = 0; i < slotInfo.Length; ++i)
                if (slotInfo[i].defaultItem.item != null && slotInfo[i].defaultItem.amount == 0)
                {
                    slotInfo[i].defaultItem.amount = 1;
                    if (movement == null) movement = gameObject.GetComponent<PlayerMovement>();
                }
        }
    }
}