using GFFAddons;
using Mirror;
using SQLite;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace uSurvival
{
    public partial class PlayerEquipment
    {
        public KeyCode ammoSelectKey = KeyCode.X;
        public KeyCode shootingModeSelectKey = KeyCode.B;

        [Serializable]
        public struct HotbarModelLocation
        {
            public string requiredCategory;
            public Transform location;
        }

        // Used components. Assign in Inspector. Easier than GetComponent caching.
        [Header("Components")]
        public Player player;
        //public PlayerMovement movement;
        public PlayerReloading reloading;
        public PlayerLook look;
        public AudioSource audioSource;

        public KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
        [SyncVar(hook = nameof(OnSelectionChanged))] public byte selection = 0; // always between 0 and slots.count, no checks needed

        // punching: reusing 'melee weapon' makes sense because it's the same code anyway
        public MeleeWeaponItem hands;

        // items will automatically be displayed on the body if a free
        // EquipmentInfo is found
        public HotbarModelLocation[] modelLocations;

        public void RefreshLocations()
        {
            //на клиенте срабатывает 3 раза
            Debug.Log("Refresh location in equipment");
            // run a sweep to assign / replace / delete locations
            // note: destroying all of them and then instantiating all of them is
            //       way easier, but also way too expensive. instead we scan through
            //       them and only delete/add prefabs as needed.

            // keep track of the locations that were assigned
            HashSet<int> assignedLocations = new HashSet<int>();

            // try to place each prefab into a free location with the matching category
            for (int i = 0; i < slots.Count; ++i)
            {
                ItemSlot slot = slots[i];
                if (slot.amount > 0 && slot.item.data.modelPrefab != null)
                {
                    UsableItem itemData = (UsableItem)(slot.item.data);

                    // find a location for this one
                    for (int locationIndex = 0; locationIndex < modelLocations.Length; ++locationIndex)
                    {
                        HotbarModelLocation modelLocation = modelLocations[locationIndex];

                        // matching category and not assigned yet?
                        if (itemData.category.StartsWith(modelLocation.requiredCategory) && !assignedLocations.Contains(locationIndex))
                        {
                            GameObject model;

                            // if the location has no model yet, simply add one
                            if (modelLocation.location.childCount == 0)
                            {
                                if (itemData is RangedWeaponItem weapon)
                                {
                                    GameObject go;
                                    // instantiate and parent
                                    int weaponIndex = player.weaponSkins.GetSelectedSkinIndex(weapon.name);
                                    if (weaponIndex == -1) go = weapon.modelPrefab;
                                    else go = weapon.GetModel(weaponIndex, player.weaponSkins.selectedskins[weaponIndex].skin);

                                    model = Instantiate(go, modelLocation.location, false);
                                    model.name = go.name; // avoid "(Clone)"

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
                                    model = Instantiate(itemData.modelPrefab, modelLocation.location, false);
                                    model.name = itemData.modelPrefab.name; // avoid "(Clone)"
                                }
                            }

                            // it has a model. is it the right one?
                            //else if (modelLocation.location.GetChild(0).name == itemData.modelPrefab.name)
                            //{
                            //    model = modelLocation.location.GetChild(0).gameObject;
                            //}
                            // otherwise replace it
                            else
                            {
                                // destroy and unparent so childCount is 0 in next
                                // check (destroy doesn't destroy immediately)
                                GameObject oldModel = modelLocation.location.GetChild(0).gameObject;
                                oldModel.transform.parent = null;
                                Destroy(oldModel);

                                if (itemData is RangedWeaponItem weapon)
                                {
                                    GameObject go;
                                    // instantiate
                                    int weaponIndex = player.weaponSkins.GetSelectedSkinIndex(weapon.name);
                                    if (weaponIndex == -1) go = weapon.modelPrefab;
                                    else go = weapon.GetModel(weaponIndex, player.weaponSkins.selectedskins[weaponIndex].skin);
                                    model = Instantiate(go);
                                    model.name = go.name; // avoid "(Clone)"
                                    model.transform.SetParent(modelLocation.location, false);

                                    WeaponModulesEnable details = model.GetComponentInChildren<WeaponModulesEnable>();
                                    if (details != null)
                                    {
                                        if (details.silencer != null) details.silencer.SetActive(slot.item.modulesHash[0] != 0);
                                        //if (details.sights != null) details.sights.SetActive(slot.item.modulesHash[3] != 0);

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
                                    // instantiate
                                    model = Instantiate(itemData.modelPrefab);
                                    model.name = itemData.modelPrefab.name; // avoid "(Clone)"
                                    model.transform.SetParent(modelLocation.location, false);
                                }
                            }

                            // hide if selected, show if not selected anymore
                            model.SetActive(i != selection);
                            if (i != selection)
                            {
                                for (int x = 0; x < model.transform.childCount; x++)
                                {
                                    ExtensionsExtended.SetLayerRecursively(model.transform.GetChild(x).gameObject, LayerMask.NameToLayer("HideForLocalPlaer"));
                                }
                                //model.layer = LayerMask.NameToLayer("HideForLocalPlaer");
                            }

                                // remember that we assigned this one
                                assignedLocations.Add(locationIndex);

                            // done searching a location for this model
                            break;
                        }
                    }
                }
            }

            // now clear all locations that were not assigned. those might still
            // have old models
            for (int locationIndex = 0; locationIndex < modelLocations.Length; ++locationIndex)
            {
                // not assigned? then destroy the old model (if any)
                HotbarModelLocation modelLocation = modelLocations[locationIndex];
                if (!assignedLocations.Contains(locationIndex) &&
                    modelLocation.location.childCount > 0)
                {
                    // destroy and unparent so childCount is 0 in next
                    // check (destroy doesn't destroy immediately)
                    GameObject oldModel = modelLocation.location.GetChild(0).gameObject;
                    oldModel.transform.parent = null;
                    Destroy(oldModel);
                }
            }
        }

        void OnSelectionChanged(byte oldValue, byte newValue)
        {
            // refresh locations to hide the selected model
            RefreshLocations();
        }

        // update //////////////////////////////////////////////////////////////////
        [Client]
        void TryUseItem(UsableItem itemData)
        {
            // note: no .amount > 0 check because it's either an item or hands

            // repeated or one time use while holding mouse down?
            if (itemData.keepUsingWhileButtonDown || Input.GetMouseButtonDown(0))
            {
                if ((itemData is RangedWeaponItem && (selectedShootingMode == ShootingModes.auto || Input.GetMouseButtonDown(0))) ||
                     (itemData is RangedWeaponItem weapon && selectedShootingMode == ShootingModes.burst && burstAmmoAmount < weapon.burstAmmoAmount) ||
                     itemData is RangedWeaponItem == false)
                {
                    // check durability
                    if (IsHandsOrItemWithValidDurability(selection))
                    {
                        // get the exact look position on whatever object we aim at
                        Vector3 lookAt = look.lookPositionRaycasted;

                        // use it
                        Usability usability = itemData.CanUseEquipment(player, selection, lookAt);
                        if (usability == Usability.Usable)
                        {
                            // attack by using the weapon item
                            //Debug.DrawLine(Camera.main.transform.position, lookAt, Color.gray, 1);
                            CmdUseItem(selection, lookAt);

                            // simulate OnUsed locally without waiting for the Rpc to avoid
                            // latency effects:
                            // - usedEndTime would be synced too slowly, hence fire interval
                            //   would be too slow on clients
                            // - TryUseItem would be called immediately again afterwards
                            //   because useEndTime wouldn't be reset yet due to latency
                            // - decals/muzzle flash would be delayed by latency and feel
                            //   bad
                            // => only IF NOT SERVER. we don't need to simulate anything
                            //    for the host. only for connected clients that are
                            //    affected by latency.
                            if (player.isNonHostLocalPlayer)
                                OnUsedItem(itemData, lookAt);

                            if (selectedShootingMode == ShootingModes.burst) burstAmmoAmount++;
                        }
                        else if (usability == Usability.Empty)
                        {
                            // play empty sound locally (if any)
                            // -> feels best to only play it when clicking the mouse button once, not while holding
                            if (Input.GetMouseButtonDown(0))
                            {
                                if (itemData.emptySound)
                                {
                                    if (itemData is RangedWeaponItem data)
                                    {
                                        WeaponDetails details = data.GetWeaponDetails(player.equipment);
                                        if (details != null && details.audioReloadSource != null) details.audioReloadSource.PlayOneShot(itemData.emptySound);
                                    }
                                    else audioSource.PlayOneShot(itemData.emptySound);
                                }
                            }
                        }
                        // do nothing if on cooldown (just wait) or if not usable at all
                    }
                }
            }
        }

        void Update()
        {
            // current selection model needs to be refreshed on the client AND on
            // the server, because the server needs the muzzle location when firing.
            // -> slots.Callback is only called on clients as it seems, so we need
            //    to do it in Update. RefreshLocation only refresh if necessary
            //    anyway.
            RefreshLocation(weaponMount, slots[selection]);

            // localplayer selected item usage
            if (isLocalPlayer)
            {
                // mouse down and can we use items right now?
                if (Input.GetMouseButton(0) &&
                    Cursor.lockState == CursorLockMode.Locked &&
                    health.current > 0 &&
                    movement.state != MoveState.CLIMBING &&
                    reloading.ReloadTimeRemaining() == 0 &&
                    !look.IsFreeLooking() &&
                    !Utils.IsCursorOverUserInterface() &&
                    Input.touchCount <= 1)
                {
                    // use current item or hands
                    TryUseItem(GetCurrentUsableItemOrHands());
                }

                if (Input.GetMouseButton(0) == false && selectedShootingMode == ShootingModes.burst) burstAmmoAmount = 0;

                if (Cursor.lockState == CursorLockMode.Locked && !Utils.IsCursorOverUserInterface())
                {
                    if (Input.mouseScrollDelta.y > 0 && selection < keys.Length - 1) CmdSelect((byte)(selection + 1));
                    else if (Input.mouseScrollDelta.y < 0 && selection > 0) CmdSelect((byte)(selection - 1));

                    for (byte i = 0; i < keys.Length; ++i)
                    {
                        // hotkey pressed and not typing in any input right now?
                        // (and not while reloading, this would be unrealistic, feels weird
                        //  and the reload bar UI needs selected weapon's reloadTime)
                        if (Input.GetKeyDown(keys[i]) && !UIUtils.AnyInputActive())
                        {
                            ItemSlot itemSlot = slots[i];

                            // empty? then select (for hand combat)
                            if (itemSlot.amount == 0)
                            {
                                CmdSelect(i);
                            }
                            // usable item?
                            else if (itemSlot.item.data is WeaponItem usable || itemSlot.item.data is StructureItem)
                            {
                                // use it directly or select the slot?
                                //if (usable.useDirectly)
                                //player.equipment.CmdUseItem(i, player.look.lookPositionRaycasted);
                                //else
                                CmdSelect(i);
                            }
                        }
                    }
                }

                ItemSlot slot = slots[selection];
                if (slot.amount > 0 && slot.item.data is RangedWeaponItem itemData)
                {
                    if (itemData.compatibleAmmo != null && itemData.compatibleAmmo.Length > 0)
                    {
                        if (Input.GetKeyDown(ammoSelectKey) && !UIUtils.AnyInputActive())
                        {
                            CmdSetAmmo();

                            // ammo type in inventory?
                            int inventoryIndex = inventory.GetItemIndexByName(itemData.compatibleAmmo[selectedAmmo].name);
                            if (inventoryIndex != -1)
                            {
                                // ask server to reload
                                reloading.CmdReloadWeaponOnEquipment(inventoryIndex, selection);

                                // play audio locally to avoid server delay and to save bandwidth
                                if (itemData.reloadSound) player.audioSource.PlayOneShot(itemData.reloadSound, 0.6f);
                            }
                        }

                        // weapon Shooting Mode Addon
                        if (Input.GetKeyDown(shootingModeSelectKey) && !UIUtils.AnyInputActive())
                        {
                            SetShootingMode();

                            // play audio locally to avoid server delay and to save bandwidth
                            if (itemData.changeShootingModeSound) player.audioSource.PlayOneShot(itemData.changeShootingModeSound, 0.6f);
                        }
                    }
                }
            }
        }

        // helpers /////////////////////////////////////////////////////////////////
        // returns tool or hands
        public UsableItem GetUsableItemOrHands(int index)
        {
            ItemSlot slot = slots[index];
            return slot.amount > 0 ? (UsableItem)slot.item.data : hands;
        }

        // returns current tool or hands
        public UsableItem GetCurrentUsableItemOrHands()
        {
            return GetUsableItemOrHands(selection);
        }

        // check slot's durability (ignore hands and ignore maxDurability=0)
        public bool IsHandsOrItemWithValidDurability(int slotIndex)
        {
            return slots[slotIndex].amount == 0 ||
                   slots[slotIndex].item.CheckDurability();
        }

        ////////////////////////////////////////////////////////////////////////////
        [Command]
        public void CmdSelect(byte index)
        {
            // validate: valid index and not reloading? (switching while reloading
            // is very unrealistic, feels weird and UI reload bar uses selected
            // weapon's reloadTime)

            if (0 <= index && index < slots.Count &&
                reloading.ReloadTimeRemaining() == 0)
                selection = index;
        }

        // note: lookAt is available in PlayerLook, but we still pass the exact
        //       uncompressed Vector3 here, because it needs to be PRECISE when
        //       shooting, building structures, etc.
        //
        // note: we pass the item index even though selection is already known on
        //       the server because SOME items are used directly without selecting!
        [Command]
        public void CmdUseItem(int index, Vector3 lookAt)
        {
            // validate
            if (0 <= index && index < slots.Count &&
                health.current > 0 &&
                IsHandsOrItemWithValidDurability(index))
            {
                // use item at index, or hands
                // note: we don't decrease amount / destroy in all cases because
                // some items may swap to other slots in .Use()
                UsableItem itemData = GetUsableItemOrHands(index);
                if (itemData.CanUseEquipment(player, index, lookAt) == Usability.Usable)
                {
                    // use it
                    itemData.UseEquipment(player, index, lookAt);

                    // RpcUsedItem needs itemData, but we can't send that as Rpc
                    // -> we could send the Item at slots[index], but .data is null
                    //    for hands because hands actually live in '.hands' variable
                    // -> we could create a new Item(itemData) and send, but it's
                    //    kinda odd that it's different from slot
                    // => only sending hash saves A LOT of bandwidth over time since
                    //    this rpc is called very frequently (each weapon shot etc.)
                    //    (we reuse Item's hash generation for simplicity)
                    RpcUsedItem(new Item(itemData).hash, lookAt);
                }
                else
                {
                    // CanUse is checked locally before calling this Cmd, so if we
                    // get here then either our prediction is off (in which case we
                    // really should show a message for easier debugging), or someone
                    // tried to cheat, or there's some networking issue, etc.
                    Debug.Log("CmdUseItem rejected for: " + name + " item=" + itemData.name + "@" + NetworkTime.time);
                }
            }
        }

        [Command]
        public void CmdUseItem(int index)
        {
            if (player.isGameMaster)
            {
                player.combat.TargetShowErrorInfo("CmdUseItem " + index);
            }

            // validate
            // note: checks durability only if it should be used (if max > 0)
            if (player.health.current > 0 &&
                0 <= index && index < slots.Count &&
                slots[index].amount > 0 &&
                slots[index].item.CheckDurability() &&
                slots[index].item.data is UsableItem usable)
            {
                // use item
                // note: we don't decrease amount / destroy in all cases because
                // some items may swap to other slots in .Use()
                if (usable.CanUseEquipment(player, index) == Usability.Usable)
                {
                    // .Use might clear the slot, so we backup the Item first for the Rpc
                    Item item = slots[index].item;
                    usable.UseEquipment(player, index);
                    RpcUsedItem(item);
                }
            }
        }

        [ClientRpc]
        public void RpcUsedItem(Item item)
        {
            // validate
            if (item.data is UsableItem usable)
            {
                usable.OnUsedEquipment(player);
            }
        }

        // used by local simulation and Rpc, so we might as well put it in a function
        void OnUsedItem(UsableItem itemData, Vector3 lookAt)
        {
            // reset cooldown for local player to avoid waiting for sync result
            if (player.isNonHostLocalPlayer)
                player.SetItemCooldown(itemData.cooldownCategory, itemData.cooldown);

            // call OnUsed
            itemData.OnUsedEquipment(player, lookAt);

            // trigger upperbody usage animation in all animators, so it works for
            // skinned meshes too.
            // (trigger works best for usage, especially for repeated usage to)
            // (only for weapons, not for potions until we can hold potions in hand
            //  later on)
            if (itemData is WeaponItem)
                foreach (Animator animator in GetComponentsInChildren<Animator>())
                    animator.SetTrigger("UPPERBODY_USED");
        }

        [ClientRpc]
        public void RpcUsedItem(int itemNameHash, Vector3 lookAt)
        {
            // local player simulates OnUsed immediately after using, so only do it
            // for other players
            // => always call OnUsed on server
            // => only call OnUsed on client if not local player, because local
            //    player simulates it
            if (!player.isNonHostLocalPlayer)
            {
                // use Item finds ScriptableItem based on hash, no need to do it manually
                Item item = new Item { hash = itemNameHash };

                // OnUsed logic
                OnUsedItem((UsableItem)item.data, lookAt);
            }
        }

        // drag & drop /////////////////////////////////////////////////////////////
        void OnDragAndDrop_EquipmentSlot_EquipmentSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            CmdSwapEquipmentEquipment(slotIndices[0], slotIndices[1]);
        }

        [Command]
        public void CmdSwapEquipmentEquipment(int fromIndex, int toIndex)
        {
            // validate: make sure that the slots actually exist in the inventory
            // and that they are not equal
            if (player.health.current > 0 &&
                0 <= fromIndex && fromIndex < slots.Count &&
                0 <= toIndex && toIndex < slots.Count &&
                fromIndex != toIndex)
            {
                // item slot has to be empty (unequip) or equipable
                if (slots[fromIndex].item.data is UsableItem usableItem && usableItem.CanEquip(this, -1, toIndex))
                {
                    // swap them
                    ItemSlot temp = slots[fromIndex];
                    slots[fromIndex] = slots[toIndex];
                    slots[toIndex] = temp;

                    //RefreshLocations();
                }
            }
        }
    }

    public partial class PlayerReloading
    {
        [Header("Components")]
        public PlayerEquipment equipment;

        [Command]
        public void CmdReloadWeaponOnEquipment(int inventoryAmmoIndex, int equipmentWeaponIndex)
        {
            // validate
            if (health.current > 0 &&
                0 <= inventoryAmmoIndex && inventoryAmmoIndex < inventory.slots.Count &&
                0 <= equipmentWeaponIndex && equipmentWeaponIndex < equipment.slots.Count &&
                inventory.slots[inventoryAmmoIndex].amount > 0 &&
                equipment.slots[equipmentWeaponIndex].amount > 0)
            {
                ItemSlot ammoSlot = inventory.slots[inventoryAmmoIndex];
                ItemSlot weaponSlot = equipment.slots[equipmentWeaponIndex];
                if (CanLoadAmmoIntoWeaponExtended(ammoSlot, weaponSlot.item))
                {
                    RangedWeaponItem weaponData = (RangedWeaponItem)weaponSlot.item.data;

                    ushort magazinBonus = 0;
                    if (weaponSlot.item.modulesHash[4] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[4], out ScriptableItem moduleData))
                        if (moduleData is ScriptableWeaponModule module) magazinBonus = module.addMagazinAmmo;

                    // add as many as possible
                    ushort limit = (ushort)Mathf.Clamp(ammoSlot.amount, 0, weaponData.magazineSize + magazinBonus - weaponSlot.item.ammo);
                    ammoSlot.amount -= limit;
                    weaponSlot.item.ammo += limit;
                    weaponSlot.item.ammoname = ammoSlot.item.name;

                    // put back into the lists
                    inventory.slots[inventoryAmmoIndex] = ammoSlot;
                    equipment.slots[equipmentWeaponIndex] = weaponSlot;

                    // start reload timer
                    reloadTimeEnd = NetworkTime.time + weaponData.reloadTime;
                }
            }
        }
    }

    public partial class ShouldersLookAt
    {
        [Header("Components")]
        public PlayerEquipment equipment;
    }

    public partial class PlayerConstruction
    {
        [Header("Components")]
        public PlayerEquipment equipment;
    }

    public partial class Zoom
    {
        [Header("Components")]
        public PlayerEquipment equipment;
    }

    public partial class UsableItem
    {
        public virtual Usability CanUseEquipment(Player player, int equipmentIndex, Vector3 lookAt)
        {
            // base cooldown check
            return player.GetItemCooldown(cooldownCategory) > 0
                   ? Usability.Cooldown
                   : Usability.Usable;
        }

        public virtual Usability CanUseEquipment(Player player, int equipmentIndex)
        {
            // base cooldown check
            return player.GetItemCooldown(cooldownCategory) > 0
                   ? Usability.Cooldown
                   : Usability.Usable;
        }

        public virtual void UseEquipment(Player player, int equipmentIndex, Vector3 lookAt)
        {
            // start cooldown (if any)
            // -> no need to set sync dict dirty if we have no cooldown
            if (cooldown > 0)
                player.SetItemCooldown(cooldownCategory, cooldown);
        }

        public virtual void UseEquipment(Player player, int equipmentIndex)
        {
            // start cooldown (if any)
            // -> no need to set sync dict dirty if we have no cooldown
            if (cooldown > 0)
                player.SetItemCooldown(cooldownCategory, cooldown);

            //player.weight.weightCurrent -= itemWeight;
        }

        public virtual void OnUsedEquipment(Player player, Vector3 lookAt) { }

        public virtual void OnUsedEquipment(Player player) { }
    }

    public partial class EquipmentItem
    {
        int FindEquipableSlotForExtended(PlayerEquipment equipment, int inventoryIndex)
        {
            int index = -1;
            for (int i = 0; i < equipment.slots.Count; ++i)
                if (CanEquip(equipment, inventoryIndex, i))
                {
                    if (equipment.slots[i].amount == 0) return i;
                    else index = i;
                }
            return index;
        }

        public override void UseEquipment(Player player, int equipmentIndex, Vector3 lookAt)
        {
            base.UseEquipment(player, equipmentIndex, lookAt);
        }

        // can we equip this item into this specific equipment slot?
        public bool CanEquipFromEquipment(PlayerEquipment equipment, int toIndex)
        {
            string requiredCategory = equipment.slotInfo[toIndex].requiredCategory;
            return requiredCategory != "" &&
                   category.StartsWith(requiredCategory);
        }
    }

    public partial class RangedWeaponItem
    {
        public AudioClip successfulSilentUseSound; // swinging axe at enemy etc.

        public override Usability CanUseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // check base usability first (cooldown etc.)
            Usability baseUsable = base.CanUseEquipment(player, hotbarIndex, lookAt);
            if (baseUsable != Usability.Usable)
                return baseUsable;

            // reloading?
            if (player.reloading.ReloadTimeRemaining() > 0)
                return Usability.Cooldown;

            // not enough ammo?
            //if (requiredAmmo != null && player.equipment.slots[hotbarIndex].item.ammo == 0)
            //return Usability.Empty;

            //if used Ammo Extended Addon
            if ((compatibleAmmo != null && compatibleAmmo.Length > 0) && player.equipment.slots[hotbarIndex].item.ammo == 0)
                return Usability.Empty;

            // otherwise we can use it
            return Usability.Usable;
        }

        public override void UseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // call base function to start cooldown
            base.UseEquipment(player, hotbarIndex, lookAt);

            ItemSlot slot = player.equipment.slots[hotbarIndex];

            // decrease ammo (if any is required)
            //if (requiredAmmo != null)
            //if used Ammo Extended Addon
            if (compatibleAmmo != null && compatibleAmmo.Length > 0)
            {
                --slot.item.ammo;
                player.equipment.slots[hotbarIndex] = slot;
            }

            // reduce durability in any case. rifles always get worse each shot.
            slot.item.durability = (ushort)Mathf.Max(slot.item.durability - 1, 0);
            player.equipment.slots[hotbarIndex] = slot;
        }

        public override void OnUsedEquipment(Player player, Vector3 lookAt)
        {
            Debug.Log("OnUsedEquipment");

            WeaponDetails details = GetWeaponDetails(player.equipment);
            if (details != null)
            {
                ItemSlot slot = player.equipment.slots[player.equipment.selection];

                if (slot.item.modulesHash[0] != 0 && ScriptableItem.dict.TryGetValue(slot.item.modulesHash[0], out ScriptableItem moduleData))
                {
                    //silencer
                    if (successfulSilentUseSound && ((ScriptableWeaponModule)moduleData).muffle)
                    {
                        details.audioShotSource.PlayOneShot(successfulSilentUseSound);
                        ShowMuzzleFlash(player.equipment);
                    }
                    else if (((ScriptableWeaponModule)moduleData).flashHider)
                    {
                        details.audioShotSource.PlayOneShot(successfulUseSound);
                    }
                }
                else
                {
                    details.audioShotSource.PlayOneShot(successfulUseSound);

                    // show muzzle flash in any case
                    ShowMuzzleFlash(player.equipment);
                }
            }

            // show decal if we didn't hit anything living
            if (
                RaycastToLookAt(player, lookAt, out RaycastHit hit) &&
                !hit.transform.GetComponent<Health>())
            {
                // instantiate
                //GameObject go = Instantiate(decalPrefab, hit.point + hit.normal * decalOffset, Quaternion.LookRotation(-hit.normal));

                // parent to hit collider so that decals don't hang in air if we
                // hit a moving object like a door.
                // (.collider.transform instead of
                // -> parent to .collider.transform instead of .transform because
                //    for our doors, .transform would be the door parent, while
                //    .collider is the part that actually moves. so this is safer.
                //go.transform.parent = hit.collider.transform;

                InstanceHitDecalAndParticle(hit);
            }

            // recoil (only for local player)
            if (player.isLocalPlayer)
            {
                // horizontal from - to +
                // vertical from 0 to + (recoil never goes downwards)
                float horizontal = UnityEngine.Random.Range(-recoilHorizontal / 2, recoilHorizontal / 2);
                float vertical = UnityEngine.Random.Range(0, recoilVertical);

                // rotate player horizontally, rotate camera vertically
                player.transform.Rotate(new Vector3(0, horizontal, 0));
                Camera.main.transform.Rotate(new Vector3(-vertical, 0, 0));
            }
        }
    }

    public partial class RangedRaycastWeaponItem
    {
        public override void UseEquipment(Player player, int selectedIndex, Vector3 lookAt)
        {
            // raycast to find out what we hit
            if (RaycastToLookAt(player, lookAt, out RaycastHit hit))
            {
                // hit an entity? then deal damage
                Entity victim = hit.transform.GetComponent<Entity>();
                if (victim)
                {
                    int _damage = damage;
                    //bonus from Ammo Addon
                    _damage += (int)(damage * compatibleAmmo[player.equipment.selectedAmmo].damageBonusFromAmmo);
                    player.combat.DealDamageAt(victim, _damage, hit.point, hit.normal, hit.collider);
                }
            }

            // base logic (decrease ammo and durability)
            base.UseEquipment(player, selectedIndex, lookAt);
        }
    }

    public partial class RangedProjectileWeaponItem
    {
        public override void UseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // raycast to find out what we hit
            // spawn the projectile.
            // -> we need to call an RPC anyway, it doesn't make much of a
            //    difference if we use NetworkServer.Spawn for everything.
            // -> we try to spawn it at the weapon's projectile mount
            if (projectile != null)
            {
                // spawn at muzzle location
                WeaponDetails details = GetWeaponDetails(player.equipment);
                if (details != null && details.muzzleLocation != null)
                {
                    Vector3 spawnPosition = details.muzzleLocation.position;
                    Quaternion spawnRotation = details.muzzleLocation.rotation;

                    GameObject go = Instantiate(projectile.gameObject, spawnPosition, spawnRotation);
                    Projectile proj = go.GetComponent<Projectile>();
                    proj.owner = player.gameObject;
                    proj.damage = damage;
                    proj.direction = lookAt - spawnPosition;
                    NetworkServer.Spawn(go);
                }
                else Debug.LogWarning("weapon details or muzzle location not found for player: " + player.name);
            }
            else Debug.LogWarning(name + ": missing projectile");

            // base logic (decrease ammo and durability)
            base.UseEquipment(player, hotbarIndex, lookAt);
        }
    }

    public partial class MeleeWeaponItem
    {
        public override Usability CanUseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // check base usability first (cooldown etc.)
            Usability baseUsable = base.CanUseEquipment(player, hotbarIndex, lookAt);
            if (baseUsable != Usability.Usable)
                return baseUsable;

            // not reloading?
            return player.reloading.ReloadTimeRemaining() > 0
                   ? Usability.Cooldown
                   : Usability.Usable;
        }

        public override void UseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // call base function to start cooldown
            base.UseEquipment(player, hotbarIndex, lookAt);

            // can hit an entity?
            Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
            if (victim != null)
            {
                // deal damage
                player.combat.DealDamageAt(victim, player.combat.damage + damage, hit.point, hit.normal, hit.collider);

                // reduce durability only if we hit something
                // (an axe doesn't lose durability if we swing it in the air)
                // (slot might be invalid in case of hands)
                ItemSlot slot = player.equipment.slots[hotbarIndex];
                if (slot.amount > 0)
                {
                    slot.item.durability = (ushort)Mathf.Max(slot.item.durability - 1, 0);
                    player.equipment.slots[hotbarIndex] = slot;
                }
            }
        }

        public override void OnUsedEquipment(Player player, Vector3 lookAt)
        {
            //Debug.Log("OnUsedEquipment");
            // find out what we hit by simulating it again to decide which sound to play
            Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
            if (victim != null)
            {
                if (successfulUseSound)
                    player.equipment.audioSource.PlayOneShot(successfulUseSound);
            }
            else
            {
                if (failedUseSound)
                    player.equipment.audioSource.PlayOneShot(failedUseSound);
            }
        }
    }

    public partial class StructureItem
    {
        public override Usability CanUseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // check base usability first (cooldown etc.)
            Usability baseUsable = base.CanUseEquipment(player, hotbarIndex, lookAt);
            if (baseUsable != Usability.Usable)
                return baseUsable;

            // calculate look direction in a way that works on clients and server
            // (via lookAt)
            Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;

            // calculate bounds based on structurePrefab + position + rotation
            // (server doesn't have construction.preview GameObject)
            // THIS POSITION IS DIFFERENT
            Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
            Quaternion rotation = player.construction.CalculatePreviewRotation(this);

            // we need the structure prefab's bounds, but rotated and positioned to
            // where we want to build.
            //
            // this doesn't work yet:
            /*
                Bounds bounds = new Bounds();
                Bounds originalBounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
                Vector3 p0 = new Vector3(originalBounds.center.x - bounds.size.x,
                                         originalBounds.center.y - bounds.size.y,
                                         originalBounds.center.z - bounds.size.z);

                Vector3 p1 = new Vector3(originalBounds.center.x + bounds.size.x,
                                         originalBounds.center.y - bounds.size.y,
                                         originalBounds.center.z - bounds.size.z);

                Vector3 p2 = new Vector3(originalBounds.center.x - bounds.size.x,
                                         originalBounds.center.y + bounds.size.y,
                                         originalBounds.center.z - bounds.size.z);

                Vector3 p3 = new Vector3(originalBounds.center.x - bounds.size.x,
                                         originalBounds.center.y - bounds.size.y,
                                         originalBounds.center.z + bounds.size.z);

                Vector3 p4 = new Vector3(originalBounds.center.x + bounds.size.x,
                                         originalBounds.center.y + bounds.size.y,
                                         originalBounds.center.z - bounds.size.z);

                Vector3 p5 = new Vector3(originalBounds.center.x + bounds.size.x,
                                         originalBounds.center.y - bounds.size.y,
                                         originalBounds.center.z + bounds.size.z);

                Vector3 p6 = new Vector3(originalBounds.center.x - bounds.size.x,
                                         originalBounds.center.y + bounds.size.y,
                                         originalBounds.center.z + bounds.size.z);

                Vector3 p7 = new Vector3(originalBounds.center.x + bounds.size.x,
                                         originalBounds.center.y + bounds.size.y,
                                         originalBounds.center.z + bounds.size.z);

                bounds.Encapsulate(position + rotation * p0);
                bounds.Encapsulate(position + rotation * p1);
                bounds.Encapsulate(position + rotation * p2);
                bounds.Encapsulate(position + rotation * p3);
                bounds.Encapsulate(position + rotation * p4);
                bounds.Encapsulate(position + rotation * p5);
                bounds.Encapsulate(position + rotation * p6);
                bounds.Encapsulate(position + rotation * p7);

            */
            // so for now, let's set the prefab's position/rotation, get the bounds
            // and then reset it
            Vector3 prefabPosition = structurePrefab.transform.position;
            Quaternion prefabRotation = structurePrefab.transform.rotation;
            structurePrefab.transform.position = position;
            structurePrefab.transform.rotation = rotation;
            Bounds bounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
            structurePrefab.transform.position = prefabPosition;
            structurePrefab.transform.rotation = prefabRotation;

            return CanBuildThere(player.look.headPosition, bounds, player.look.raycastLayers)
                   ? Usability.Usable
                   : Usability.Empty; // for empty sound. better than 'Never'.
        }

        public override void UseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // call base function to start cooldown
            base.UseEquipment(player, hotbarIndex, lookAt);

            // get position and rotation from Construction component
            // calculate look direction in a way that works on clients and server
            // (via lookAt)
            Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;
            Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
            Quaternion rotation = player.construction.CalculatePreviewRotation(this);

            // spawn it into the world
            GameObject go = Instantiate(structurePrefab, position, rotation);
            go.name = structurePrefab.name; // avoid "(Clone)". important for saving..
            NetworkServer.Spawn(go);

            // decrease amount
            ItemSlot slot = player.equipment.slots[hotbarIndex];
            slot.DecreaseAmount(1);
            player.equipment.slots[hotbarIndex] = slot;
        }
    }

    public partial class PotionItem
    {
        public override void UseEquipment(Player player, int hotbarIndex, Vector3 lookAt)
        {
            // call base function to start cooldown
            base.UseEquipment(player, hotbarIndex, lookAt);

            ApplyEffects(player);

            // decrease amount
            ItemSlot slot = player.equipment.slots[hotbarIndex];
            slot.DecreaseAmount(1);
            player.equipment.slots[hotbarIndex] = slot;
        }
    }

    public partial class Database
    {
        class character_weapon_selection
        {
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string character { get; set; }
            public byte selection { get; set; }
        }

        class character_equipment_selection
        {
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string character { get; set; }
            public byte selection { get; set; }
        }

        void LoadEquipmentSelection(PlayerEquipment equipment)
        {
            // load selection
            character_equipment_selection row = connection.FindWithQuery<character_equipment_selection>("SELECT * FROM character_equipment_selection WHERE character=?", equipment.name);
            if (row != null)
            {
                equipment.selection = row.selection;
            }
        }

        void SaveEquipmentSelection(PlayerEquipment equipment)
        {
            connection.InsertOrReplace(new character_equipment_selection { character = equipment.name, selection = equipment.selection });
        }

        void LoadWeaponSelection(PlayerEquipment equipment)
        {
            // load selection
            character_weapon_selection row = connection.FindWithQuery<character_weapon_selection>("SELECT * FROM character_weapon_selection WHERE character=?", equipment.name);
            if (row != null)
            {
                equipment.selection = row.selection;
            }
        }

        void SaveWeaponSelection(PlayerEquipment equipment)
        {
            connection.InsertOrReplace(new character_weapon_selection { character = equipment.name, selection = equipment.selection });
        }
    }
}