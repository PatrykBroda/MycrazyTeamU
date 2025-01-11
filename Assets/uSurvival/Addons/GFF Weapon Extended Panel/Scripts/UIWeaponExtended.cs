using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UIWeaponExtended : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image imageWeapon;
        [SerializeField] private TextMeshProUGUI textWeaponName;
        [SerializeField] private Slider sliderDurability;
        [SerializeField] private GameObject imageAmmo;
        [SerializeField] private TextMeshProUGUI textAmmoAmount;
        [SerializeField] private TextMeshProUGUI textAmmoName;
        [SerializeField] private TextMeshProUGUI textShootingMode;

        [SerializeField] private GameObject panelAmmo;
        [SerializeField] private TextMeshProUGUI textAmmoNName;
        [SerializeField] private TextMeshProUGUI textAmmoBName;
        [SerializeField] private TextMeshProUGUI textAmmoRName;
        [SerializeField] private TextMeshProUGUI textAmmoN;
        [SerializeField] private TextMeshProUGUI textAmmoB;
        [SerializeField] private TextMeshProUGUI textAmmoR;
        [SerializeField] private Color selectedAmmoColor = Color.yellow;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                ItemSlot slot = player.equipment.slots[player.equipment.selection];

                if (slot.amount > 0 && slot.item.data is WeaponItem weapon)
                {
                    panel.SetActive(true);
                    if (weapon.imageHorizontal != null) imageWeapon.sprite = weapon.imageHorizontal;
                    else imageWeapon.sprite = weapon.image;

                    textWeaponName.text = Localization.Translate(weapon.name);
                    sliderDurability.gameObject.SetActive(true);
                    sliderDurability.value = slot.item.DurabilityPercent();

                    if (weapon is RangedWeaponItem rangedWeapon)
                    {
                        imageAmmo.SetActive(true);

                        //original
                        //textAmmoName.text = rangedWeapon.requiredAmmo.name;
                        //textAmmoAmount.text = slot.item.ammo + " / " + player.inventory.Count(new Item (rangedWeapon.requiredAmmo));

                        //if used ammo addon
                        textAmmoName.text = Localization.Translate(rangedWeapon.compatibleAmmo[player.equipment.selectedAmmo].name + " S");
                        textAmmoAmount.text = slot.item.ammo.ToString();

                        textShootingMode.text = Localization.Translate(player.equipment.selectedShootingMode.ToString());

                        panelAmmo.SetActive(true);

                        textAmmoN.text = player.inventory.Count(rangedWeapon.compatibleAmmo[0]).ToString();
                        textAmmoB.text = player.inventory.Count(rangedWeapon.compatibleAmmo[1]).ToString();
                        textAmmoR.text = player.inventory.Count(rangedWeapon.compatibleAmmo[2]).ToString();

                        textAmmoNName.color = player.equipment.selectedAmmo == 0 ? selectedAmmoColor : Color.white;
                        textAmmoBName.color = player.equipment.selectedAmmo == 1 ? selectedAmmoColor : Color.white;
                        textAmmoRName.color = player.equipment.selectedAmmo == 2 ? selectedAmmoColor : Color.white;
                    }
                    else
                    {
                        imageAmmo.SetActive(false);
                        textAmmoName.text = "";
                        textShootingMode.text = "";
                        panelAmmo.SetActive(false);
                    }
                }
                else panel.SetActive(false);
            }
            else panel.SetActive(false);
        }
    }
}