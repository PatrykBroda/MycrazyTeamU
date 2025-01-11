using UnityEngine;
using Mirror;
using GFFAddons;

namespace uSurvival
{
    public partial class PlayerReloading : NetworkBehaviour
    {
        public Health health;
        //public PlayerHotbar hotbar;
        public Inventory inventory;
        public PlayerMovement movement;
        public AudioSource audioSource;

        public KeyCode reloadKey = KeyCode.R;
        [SyncVar] double reloadTimeEnd; // server time, synced to client for UI. double for long term precision.

        private void Update()
        {
            if (!isLocalPlayer) return;

            // reload key pressed?
            // and cursor locked? (not in UI), alive, not climbing, not typing in an input?
            if (Input.GetKeyDown(reloadKey) &&
                Cursor.lockState == CursorLockMode.Locked &&
                health.current > 0 && movement.state != MoveState.CLIMBING &&
                ReloadTimeRemaining() == 0 &&
                !UIUtils.AnyInputActive())
            {
                // usable item in selected hotbar slot?
                ItemSlot slot = equipment.slots[equipment.selection];
                if (slot.amount > 0 && slot.item.data is RangedWeaponItem weapon)
                {
                    //gff
                    int magazinBonus = 0;
                    if (slot.item.modulesHash[4] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[4], out ScriptableItem moduleData))
                        if (moduleData is ScriptableWeaponModule module) magazinBonus = module.addMagazinAmmo;

                    // requires ammo and not fully loaded yet?
                    if (slot.item.ammo > 0)
                    {
                        if (slot.item.ammo < weapon.magazineSize + magazinBonus)
                        {
                            // ammo type in inventory?
                            int inventoryIndex = inventory.GetItemIndexByName(weapon.compatibleAmmo[equipment.selectedAmmo].name);
                            if (inventoryIndex != -1)
                            {
                                // ask server to reload
                                CmdReloadWeaponOnHotbar(inventoryIndex, equipment.selection);
                                //CmdReloadWeaponInInventory(inventoryIndex, equipment.selection);

                                // play audio locally to avoid server delay and to save bandwidth
                                if (weapon.reloadSound) audioSource.PlayOneShot(weapon.reloadSound, 0.5f);
                            }
                        }
                    }
                    else CheckAllAmmoForReload(weapon);
                }
            }
        }

        // how much time remaining until the casttime ends? (using server time)
        public float ReloadTimeRemaining() =>
            NetworkTime.time >= reloadTimeEnd ? 0 : (float)(reloadTimeEnd - NetworkTime.time);

        public bool CanLoadAmmoIntoWeapon(ItemSlot ammoSlot, Item weapon)
        {
            // valid slots?
            if (ammoSlot.amount > 0 &&
                ammoSlot.item.data is AmmoItem ammoData &&
                weapon.data is RangedWeaponItem weaponData)
            {
                // correct ammo type?
                if (weaponData.compatibleAmmo[equipment.selectedAmmo] == ammoData)
                {
                    int magazinBonus = 0;
                    if (weapon.modulesHash[4] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[4], out ScriptableItem moduleData))
                        if (moduleData is ScriptableWeaponModule module) magazinBonus = module.addMagazinAmmo;

                    // weapon not fully loaded yet?
                    return weapon.ammo < weaponData.magazineSize + magazinBonus;
                }
            }
            return false;
        }

        [Command]
        public void CmdReloadWeaponOnHotbar(int inventoryAmmoIndex, int hotbarWeaponIndex)
        {
            // validate
            if (health.current > 0 &&
                0 <= inventoryAmmoIndex && inventoryAmmoIndex < inventory.slots.Count &&
                0 <= hotbarWeaponIndex && hotbarWeaponIndex < equipment.slots.Count &&
                inventory.slots[inventoryAmmoIndex].amount > 0 &&
                equipment.slots[hotbarWeaponIndex].amount > 0)
            {
                ItemSlot ammoSlot = inventory.slots[inventoryAmmoIndex];
                ItemSlot weaponSlot = equipment.slots[hotbarWeaponIndex];
                if (CanLoadAmmoIntoWeapon(ammoSlot, weaponSlot.item))
                {
                    RangedWeaponItem weaponData = (RangedWeaponItem)weaponSlot.item.data;

                    ushort magazinBonus = 0;
                    float reloadBonus = 0;
                    if (weaponSlot.item.modulesHash[4] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[4], out ScriptableItem moduleData))
                        if (moduleData is ScriptableWeaponModule module)
                        {
                            magazinBonus = module.addMagazinAmmo;
                            reloadBonus = module.reducesReloadTime;
                        }

                    // add as many as possible
                    ushort limit = (ushort)Mathf.Clamp(ammoSlot.amount, 0, weaponData.magazineSize + magazinBonus - weaponSlot.item.ammo);
                    ammoSlot.amount -= limit;
                    weaponSlot.item.ammo += limit;

                    // put back into the lists
                    inventory.slots[inventoryAmmoIndex] = ammoSlot;
                    equipment.slots[hotbarWeaponIndex] = weaponSlot;

                    // start reload timer
                    reloadTimeEnd = NetworkTime.time + weaponData.reloadTime - (weaponData.reloadTime * reloadBonus);
                }
            }
        }

        [Command]
        public void CmdReloadWeaponInInventory(int ammoIndex, int weaponIndex)
        {
            // validate
            if (health.current > 0 &&
                0 <= ammoIndex && ammoIndex < inventory.slots.Count &&
                0 <= weaponIndex && weaponIndex < inventory.slots.Count &&
                inventory.slots[ammoIndex].amount > 0 &&
                inventory.slots[weaponIndex].amount > 0)
            {
                ItemSlot ammoSlot = inventory.slots[ammoIndex];
                ItemSlot weaponSlot = inventory.slots[weaponIndex];
                if (CanLoadAmmoIntoWeapon(ammoSlot, weaponSlot.item))
                {
                    RangedWeaponItem weaponData = (RangedWeaponItem)weaponSlot.item.data;

                    ushort magazinBonus = 0;
                    float reloadBonus = 0;
                    if (weaponSlot.item.modulesHash[4] != 0 && ScriptableItem.dict.TryGetValue(equipment.slots[equipment.selection].item.modulesHash[4], out ScriptableItem moduleData))
                        if (moduleData is ScriptableWeaponModule module)
                        {
                            magazinBonus = module.addMagazinAmmo;
                            reloadBonus = module.reducesReloadTime;
                        }

                    // add as many as possible
                    ushort limit = (ushort)Mathf.Clamp(ammoSlot.amount, 0, weaponData.magazineSize + magazinBonus - weaponSlot.item.ammo);
                    ammoSlot.amount -= limit;
                    weaponSlot.item.ammo += limit;

                    // put back into the lists
                    inventory.slots[ammoIndex] = ammoSlot;
                    inventory.slots[weaponIndex] = weaponSlot;

                    // start reload timer
                    reloadTimeEnd = NetworkTime.time + weaponData.reloadTime - (weaponData.reloadTime * reloadBonus);
                }
            }
        }
    }
}