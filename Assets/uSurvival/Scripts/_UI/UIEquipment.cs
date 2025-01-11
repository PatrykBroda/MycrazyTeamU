using UnityEngine;
using UnityEngine.UI;

namespace uSurvival
{
    public class UIEquipment : MonoBehaviour
    {
        public GameObject panel;
        public Transform content;
        public Text textPlayerName;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                textPlayerName.text = player.name;

                // refresh all
                for (short i = 0; i < player.equipment.slots.Count; ++i)
                {
                    UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
                    slot.mainSlot.dragAndDropable.name = i.ToString(); // drag and drop slot
                    ItemSlot itemSlot = player.equipment.slots[i];

                    // set category overlay in any case. we use the last noun in the
                    // category string, for example EquipmentWeaponBow => Bow
                    // (disabled if no category, e.g. for archer shield slot)
                    //slot.categoryOverlay.SetActive(player.equipment.slotInfo[i].requiredCategory != "");
                    string overlay = Localization.Translate(player.equipment.slotInfo[i].requiredCategory);
                    slot.mainSlot.categoryText.text = overlay != "" ? overlay : "?";

                    if (itemSlot.amount > 0)
                    {
                        // refresh valid item
                        //slot.mainSlot.tooltip.enabled = true;
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        //if (slot.mainSlot.tooltip.IsVisible())
                        //slot.mainSlot.tooltip.text = itemSlot.ToolTip();

                        slot.mainSlot.dragAndDropable.dragable = true;
                        slot.mainSlot.image.color = Color.white; // reset for no-durability items

                        slot.mainSlot.tooltipExtended.enabled = true;
                        if (slot.mainSlot.tooltipExtended.IsVisible())
                            slot.mainSlot.tooltipExtended.slot = itemSlot;

                        //binding
                        slot.mainSlot.imageBinding.SetActive(itemSlot.item.binding);

                        if (itemSlot.item.data is PotionItem)
                        {
                            slot.mainSlot.image.sprite = itemSlot.item.image;
                            slot.mainSlot.sliderDurability.gameObject.SetActive(false);
                            slot.mainSlot.amountOverlay.SetActive(true);
                            slot.mainSlot.amountText.text = itemSlot.amount.ToString();

                            short icopy = i; // needed for lambdas, otherwise i is Count
                            if (slot.button != null) slot.button.onClick.SetListener(() =>
                            {
                                //remove from inventory buying item
                                player.equipment.CmdUseItem(icopy);
                            });
                        }
                        else if (itemSlot.item.data is RangedWeaponItem weapon)
                        {
                            int weaponIndex = player.weaponSkins.GetSelectedSkinIndex(weapon.name);

                            if (weaponIndex == -1)
                            {
                                if (weapon.imageHorizontal != null) slot.mainSlot.image.sprite = weapon.imageHorizontal;
                                else slot.mainSlot.image.sprite = itemSlot.item.image;
                            }
                            else slot.mainSlot.image.sprite = weapon.GetWeaponSkinSprite(weaponIndex, player.weaponSkins.selectedskins[weaponIndex].skin);

                            //durability
                            slot.mainSlot.sliderDurability.gameObject.SetActive(true);
                            slot.mainSlot.sliderDurability.value = itemSlot.item.DurabilityPercent();

                            //ammo
                            if (slot.ammoIcon != null)
                            {
                                slot.ammoIcon.SetActive(true);
                                slot.textAmmoAmount.text = itemSlot.item.ammo + " / " + player.inventory.Count(weapon.compatibleAmmo[player.equipment.selectedAmmo]);
                            }

                            //modules
                            slot.mainSlot.modules.GetChild(0).gameObject.SetActive(weapon.barrelModules.Length > 0);
                            slot.mainSlot.modules.GetChild(1).gameObject.SetActive(weapon.handguardModules.Length > 0);
                            slot.mainSlot.modules.GetChild(2).gameObject.SetActive(weapon.buttModules.Length > 0);
                            slot.mainSlot.modules.GetChild(3).gameObject.SetActive(weapon.sightsModules.Length > 0);
                            slot.mainSlot.modules.GetChild(4).gameObject.SetActive(weapon.magazineModules.Length > 0);

                            for (int x = 0; x < itemSlot.item.modulesHash.Length; x++)
                            {
                                UIEquipmentSlot moduleSlot = slot.mainSlot.modules.GetChild(x).GetComponent<UIEquipmentSlot>();

                                if (itemSlot.item.modulesHash[x] != 0)
                                {
                                    ScriptableItem module = ScriptableItem.dict[itemSlot.item.modulesHash[x]];
                                    moduleSlot.dragAndDropable.name = i + "" + x.ToString(); // drag and drop slot
                                    moduleSlot.image.sprite = module.image;
                                    moduleSlot.image.color = Color.white;

                                    //moduleSlot.tooltip.enabled = true;
                                    // only build tooltip while it's actually shown. this
                                    // avoids MASSIVE amounts of StringBuilder allocations.
                                    //if (moduleSlot.tooltip.IsVisible())
                                    //    moduleSlot.tooltip.text = module.ToolTip();

                                    moduleSlot.tooltipExtended.enabled = true;
                                    if (moduleSlot.tooltipExtended.IsVisible())
                                        moduleSlot.tooltipExtended.slot = new ItemSlot(new Item(module));

                                }
                                else
                                {
                                    moduleSlot.image.sprite = null;
                                    moduleSlot.image.color = Color.clear;
                                    moduleSlot.tooltip.enabled = false;
                                    moduleSlot.tooltipExtended.enabled = false;
                                }
                            }
                        }
                        else
                        {
                            slot.mainSlot.image.sprite = itemSlot.item.image;

                            //durability
                            slot.mainSlot.sliderDurability.gameObject.SetActive(true);
                            slot.mainSlot.sliderDurability.value = itemSlot.item.DurabilityPercent();

                            //ammo
                            if (slot.ammoIcon != null) slot.ammoIcon.SetActive(false);

                            //hide modules
                            if (slot.mainSlot.modules != null)
                                for (int x = 0; x < slot.mainSlot.modules.childCount; x++)
                                    slot.mainSlot.modules.GetChild(x).gameObject.SetActive(false);
                        }

                        if (slot.secondSlot != null)
                        {
                            slot.secondSlot.dragAndDropable.name = i.ToString();  // drag and drop slot

                            if (itemSlot.item.data is EquipmentItem eItem && eItem.secondItems != null && eItem.secondItems.Length > 0)
                            {
                                slot.secondSlot.gameObject.SetActive(true);
                                slot.secondSlot.categoryText.text = Localization.Translate(eItem.secondCategory);

                                if (ScriptableItem.dict.TryGetValue(itemSlot.item.secondItemHash, out ScriptableItem secondItemData))
                                {
                                    slot.secondSlot.dragAndDropable.dragable = true;
                                    ItemSlot secondSlot = new ItemSlot(new Item(secondItemData));

                                    slot.secondSlot.tooltipExtended.enabled = true;
                                    if (slot.secondSlot.tooltipExtended.IsVisible())
                                        slot.secondSlot.tooltipExtended.slot = secondSlot;

                                    slot.secondSlot.image.color = Color.white; // reset for no-durability items
                                    slot.secondSlot.image.sprite = secondItemData.image;

                                    slot.secondSlot.sliderDurability.gameObject.SetActive(true);
                                    slot.secondSlot.sliderDurability.value = (itemSlot.item.secondItemDurability != 0 && secondItemData.maxDurability != 0) ? (float)itemSlot.item.secondItemDurability / (float)secondItemData.maxDurability : 0;
                                    slot.secondSlot.imageBinding.SetActive(itemSlot.item.secondItemBinding);
                                }
                                else
                                {
                                    // refresh invalid item
                                    slot.secondSlot.tooltip.enabled = false;
                                    slot.secondSlot.tooltipExtended.enabled = false;
                                    slot.secondSlot.dragAndDropable.dragable = false;
                                    slot.secondSlot.image.color = Color.clear;
                                    slot.secondSlot.image.sprite = null;

                                    slot.secondSlot.sliderDurability.gameObject.SetActive(false);
                                    slot.secondSlot.imageBinding.SetActive(false);
                                }
                            }
                            else
                            {
                                slot.secondSlot.gameObject.SetActive(false);
                                slot.secondSlot.tooltip.enabled = false;
                                slot.secondSlot.tooltipExtended.enabled = false;
                            }
                        }
                    }
                    else
                    {
                        // refresh invalid item
                        slot.mainSlot.tooltip.enabled = false;
                        slot.mainSlot.tooltipExtended.enabled = false;
                        slot.mainSlot.dragAndDropable.dragable = false;
                        slot.mainSlot.image.color = Color.clear;
                        slot.mainSlot.image.sprite = null;

                        slot.mainSlot.sliderDurability.gameObject.SetActive(false);
                        slot.mainSlot.imageBinding.SetActive(false);

                        if (slot.mainSlot.amountOverlay) slot.mainSlot.amountOverlay.SetActive(false);

                        if (slot.ammoIcon != null) slot.ammoIcon.SetActive(false);

                        //modules
                        if (slot.mainSlot.modules != null)
                            for (int x = 0; x < slot.mainSlot.modules.childCount; x++)
                                slot.mainSlot.modules.GetChild(x).gameObject.SetActive(false);

                        if (slot.secondSlot != null)
                        {
                            slot.secondSlot.gameObject.SetActive(false);
                            slot.secondSlot.tooltip.enabled = false;
                            slot.secondSlot.tooltipExtended.enabled = false;
                        }
                    }
                }
            }
        }

        // only enable avatar camera while panel is active.
        // no need to render while the window is hidden!
        // (can't put into UIEquipment because that itself is disabled
        //  while the UI is disabled)
        private void OnEnable()
        {
            if (Player.localPlayer != null)
                Player.localPlayer.equipment.avatarCamera.enabled = true;
        }

        private void OnDisable()
        {
            if (Player.localPlayer != null)
                Player.localPlayer.equipment.avatarCamera.enabled = false;
        }
    }
}