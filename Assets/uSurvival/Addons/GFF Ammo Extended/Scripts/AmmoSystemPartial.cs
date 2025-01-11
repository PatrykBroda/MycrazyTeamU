using Mirror;
using System.Text;
using UnityEngine;

namespace uSurvival
{
    public partial class AmmoItem
    {
        [Header("GFF Ammo Extended")]
        [SerializeField, Range(0, 1)] private float _ammoDamageBonus;

        public float damageBonusFromAmmo => _ammoDamageBonus;

        public override string ToolTip()
        {
            // we use a StringBuilder so that addons can modify tooltips later too
            // ('string' itself can't be passed as a mutable object)
            StringBuilder tip = new StringBuilder(base.ToolTip());
            tip.Replace("{AMMODAMAGE}", (_ammoDamageBonus > 0 ? (_ammoDamageBonus * 100) + "%" : "No"));
            return tip.ToString();
        }
    }

    public partial class RangedWeaponItem
    {
        [Header("GFF Ammo Extended")]
        [SerializeField] private AmmoItem[] _compatibleAmmo; // null if no ammo is required

        public AmmoItem[] compatibleAmmo => _compatibleAmmo;

        public byte GetAmmoIndex(string ammoname)
        {
            for (int i = 0; i < _compatibleAmmo.Length; i++)
            {
                if (_compatibleAmmo[i] != null && _compatibleAmmo[i].name == ammoname) return (byte)i;
            }
            return 0;
        }

        public string GetCompatibleAmmoForToolTip()
        {
            if (_compatibleAmmo == null || _compatibleAmmo.Length == 0) return "";
            else
            {
                string values = "";
                for (int i = 0; i < _compatibleAmmo.Length; i++)
                {
                    if (_compatibleAmmo[i] != null && values == "") values += _compatibleAmmo[i].name;
                    else values += ", " + _compatibleAmmo[i].name;
                }
                return values;
            }
        }
    }

    public partial class PlayerEquipment
    {
        [SyncVar] public byte selectedAmmo = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (slots[selection].amount > 0 && slots[selection].item.data is RangedWeaponItem weapon)
            {
                selectedAmmo = weapon.GetAmmoIndex(slots[selection].item.ammoname);
            }
        }

        [Command]public void CmdSetAmmo()
        {
            if (slots[selection].amount > 0 && slots[selection].item.data is RangedWeaponItem weapon)
            {
                if (weapon.compatibleAmmo.Length > selectedAmmo + 1) selectedAmmo += 1;
                else selectedAmmo = 0;
            }
        }

        [Command]public void CmdSetAmmo(byte index)
        {
            if (slots[selection].amount > 0 && slots[selection].item.data is RangedWeaponItem weapon && weapon.compatibleAmmo.Length >= index)
            {
                selectedAmmo = index;
            }
        }
    }

    public partial class PlayerReloading
    {
        public bool CanLoadAmmoIntoWeaponExtended(ItemSlot ammoSlot, Item weapon)
        {
            // valid slots?
            if (ammoSlot.amount > 0 &&
                ammoSlot.item.data is AmmoItem ammoData &&
                weapon.data is RangedWeaponItem weaponData)
            {
                // correct ammo type?
                if (weaponData.compatibleAmmo[equipment.selectedAmmo] == ammoData) return true;
            }
            return false;
        }

        public void CheckAllAmmoForReload(RangedWeaponItem weapon)
        {
            int inventoryIndex = -1;
            inventoryIndex = inventory.GetItemIndexByName(weapon.compatibleAmmo[equipment.selectedAmmo].name);
            if (inventoryIndex != -1)
            {
                // ask server to reload
                CmdReloadWeaponOnHotbar(inventoryIndex, equipment.selection);
                //CmdReloadWeaponInInventory(inventoryIndex, equipment.selection);

                // play audio locally to avoid server delay and to save bandwidth
                if (weapon.reloadSound) audioSource.PlayOneShot(weapon.reloadSound, 0.5f);
            }
            else
            {
                for (byte i = 0; i < weapon.compatibleAmmo.Length; i++)
                {
                    inventoryIndex = inventory.GetItemIndexByName(weapon.compatibleAmmo[i].name);
                    if (inventoryIndex != -1)
                    {
                        // ask server to reload
                        equipment.CmdSetAmmo(i);
                        CmdReloadWeaponOnHotbar(inventoryIndex, equipment.selection);

                        // play audio locally to avoid server delay and to save bandwidth
                        if (weapon.reloadSound) audioSource.PlayOneShot(weapon.reloadSound, 0.5f);

                        break;
                    }
                }
            }
        }
    }
}