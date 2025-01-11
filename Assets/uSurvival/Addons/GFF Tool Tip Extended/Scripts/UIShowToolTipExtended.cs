using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using uSurvival;

namespace GFFAddons
{
    public class UIShowToolTipExtended : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject tooltipPrefab;
        [HideInInspector] public ItemSlot slot = new ItemSlot();

        // instantiated tooltip
        GameObject current;

        void CreateToolTip()
        {
            // instantiate
            current = Instantiate(tooltipPrefab, transform.position, Quaternion.identity);

            // put to foreground
            current.transform.SetParent(transform.root, true); // canvas
            current.transform.SetAsLastSibling(); // last one means foreground

            // set text immediately. don't wait for next Update(), otherwise there
            // is 1 frame with a small tooltip without a text which is odd.
            Paint();
        }

        void ShowToolTip(float delay)
        {
            Invoke(nameof(CreateToolTip), delay);
        }

        // helper function to check if the tooltip is currently shown
        // -> useful to only calculate item/skill/etc. tooltips when really needed
        public bool IsVisible() => current != null;

        void DestroyToolTip()
        {
            // stop any running attempts to show it
            CancelInvoke(nameof(CreateToolTip));

            // destroy it
            Destroy(current);
        }

        public void OnPointerEnter(PointerEventData d)
        {
            ShowToolTip(0.5f);
        }

        public void OnPointerExit(PointerEventData d)
        {
            DestroyToolTip();
        }

        private void Update()
        {
            // always copy text to tooltip. it might change dynamically when
            // swapping items etc., so setting it once is not enough.
            if (current) Paint();
        }

        void OnDisable()
        {
            DestroyToolTip();
        }

        void OnDestroy()
        {
            DestroyToolTip();
        }

        private void Paint()
        {
            if (slot.amount > 0)
            {
                ToolTipItemExtended go = current.GetComponent<ToolTipItemExtended>();

                //image and name
                go.image.sprite = slot.item.image;
                go.textName.text = Localization.Translate(slot.item.name);

                //name color
                //if (player.itemRarityConfig == null) go.nameText.color = Color.white;
                //else go.nameText.color = player.itemRarityConfig.GetColor(slot.item);

                //info
                int index = 0;

                if (slot.item.data is WeaponItem weapon)
                {
                    go.strings[0].text = Localization.Translate("Type") + " :";
                    if (weapon is MeleeWeaponItem) go.stringsValue[0].text = Localization.Translate("Melee");
                    else go.stringsValue[0].text = Localization.Translate("Range");
                    index += 1;

                    //combat skills
                    //go.panels[index].SetActive(true);
                    //go.strings[index].text = "Required Skill :";
                    /*if (slot.item.data.subType == itemSubType.Sword) go.stringsValue[index].text = "Melee : " + ((EquipmentItem)item).minMelee;
                    else if (slot.item.data.subType == itemSubType.Shield) go.stringsValue[index].text = "Shield : " + ((EquipmentItem)item).minShield;
                    else if (slot.item.data.subType == itemSubType.Bow) go.stringsValue[index].text = "Range : " + ((EquipmentItem)item).minRange;*/
                    //index += 1;

                    //hand(s) Used
                    //go.panels[index].SetActive(true);
                    //go.strings[index].text = "Hand(s) Used :";
                    //if (weapon.category == "WeaponTwohand") go.stringsValue[index].text = "Two-handed weapon";
                    //else go.stringsValue[index].text = "One-handed";
                    //index += 1;

                    if (weapon is RangedWeaponItem rangeWeapon)
                    {
                        //ammo
                        if (rangeWeapon.compatibleAmmo.Length > 0)
                        {
                            go.panels[index].SetActive(true);
                            go.strings[index].text = Localization.Translate("Ammo") + " : ";
                            go.stringsValue[index].text = rangeWeapon.compatibleAmmo[0].name;
                            index += 1;
                        }

                        go.panels[index].SetActive(true);
                        go.strings[index].text = Localization.Translate("Cooldown") + " : ";
                        go.stringsValue[index].text = rangeWeapon.cooldown.ToString();
                        index += 1;
                    }

                    //damage
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Damage") + " :";

                    //original
                    go.stringsValue[index].text = weapon.damage.ToString();

                    //if use Min And Max damage Addon
                    //int min = ((EquipmentItem)item).damageMinBonus + (int)(((float)((EquipmentItem)item).damageMinBonus / 100) * slot.item.ItemUpgradeBonus(runeType.damage));
                    //int max = ((EquipmentItem)item).damageMaxBonus + (int)(((float)((EquipmentItem)item).damageMaxBonus / 100) * slot.item.ItemUpgradeBonus(runeType.damage));
                    //go.stringsValue[index].text = min + "-" + max;

                    index += 1;

                    //defense
                    if (weapon.defenseBonus > 0)
                    {
                        go.panels[index].SetActive(true);
                        go.strings[index].text = Localization.Translate("Defense") + " :";
                        go.stringsValue[index].text = weapon.defenseBonus.ToString();
                        index += 1;
                    }

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    go.stringsValue[index].text = weapon.GetItemWeight(1);
                    index += 1;

                    //durability
                    index = DurabilityInfo(go, index);

                    //tradable
                    index = TradableInfo(go, weapon, index);

                    //price
                    index = PriceInfo(go, weapon, index);

                    //special Effects (damage, gefense, move speed, crit, vampirism)
                    index = SpecialEffectsInfo(go, index);
                }
                else if (slot.item.data is EquipmentItem eItem)
                {
                    go.strings[0].text = Localization.Translate("Type") + " :";
                    go.stringsValue[0].text = Localization.Translate(eItem.category);
                    index += 1;

                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Defense") + " :";
                    go.stringsValue[index].text = eItem.defenseBonus.ToString();
                    index += 1;

                    if (eItem.addSlots > 0)
                    {
                        go.panels[index].SetActive(true);
                        go.strings[index].text = Localization.Translate("AddSlots") + " :";
                        go.stringsValue[index].text = "+ " + eItem.addSlots.ToString();
                        index += 1;
                    }

                    if (eItem.weightBonus > 0)
                    {
                        go.panels[index].SetActive(true);
                        go.strings[index].text = Localization.Translate("AddKg") + ":";
                        go.stringsValue[index].text = "+ " + eItem.GetItemWeightBonusInKg();
                        index += 1;
                    }

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    go.stringsValue[index].text = eItem.GetItemWeight(1);
                    index += 1;

                    //durability
                    index = DurabilityInfo(go, index);

                    //tradable
                    index = TradableInfo(go, eItem, index);

                    index = PriceInfo(go, eItem, index);

                    //special Effects (damage, gefense, move speed, crit, vampirism)
                    index = SpecialEffectsInfo(go, index);
                }
                else if (slot.item.data is ScriptableWeaponModule module)
                {
                    go.strings[0].text = Localization.Translate("Weapon Module") + " :";
                    go.stringsValue[0].text = Localization.Translate(module.category);
                    index += 1;

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    go.stringsValue[index].text = module.GetItemWeight(1);
                    index += 1;

                    //tradable
                    index = TradableInfo(go, module, index);

                    index = PriceInfo(go, module, index);

                    //special Effects (damage, gefense, move speed, crit, vampirism)
                    index = SpecialEffectsInfo(go, index);
                }
                else if (slot.item.data is PotionItem potion)
                {
                    go.strings[0].text = Localization.Translate("Type") + " :";
                    go.stringsValue[0].text = Localization.Translate(potion.potionType.ToString());
                    index += 1;

                    go.strings[index].text = Localization.Translate("Amount") + " :";
                    go.stringsValue[index].text = slot.amount.ToString();
                    index += 1;

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    if (slot.amount == 1) go.stringsValue[index].text = potion.GetItemWeight(1);
                    else go.stringsValue[index].text = potion.GetItemWeight(1) + " / " + potion.GetItemWeight(slot.amount);
                    index += 1;

                    //tradable
                    TradableInfo(go, slot.item.data, index);

                    index = PriceInfo(go, slot.item.data, index);
                }
                else if (slot.item.data is AmmoItem ammo)
                {
                    go.strings[0].text = Localization.Translate("Type") + " :";
                    go.stringsValue[0].text = Localization.Translate("Ammo");
                    index += 1;

                    //amount
                    go.strings[index].text = Localization.Translate("Amount") + " :";
                    go.stringsValue[index].text = slot.amount.ToString();
                    index += 1;

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    if (slot.amount == 1) go.stringsValue[index].text = ammo.GetItemWeight(1);
                    else go.stringsValue[index].text = ammo.GetItemWeight(1) + " / " + ammo.GetItemWeight(slot.amount);
                    index += 1;

                    //tradable
                    index = TradableInfo(go, ammo, index);

                    index = PriceInfo(go, slot.item.data, index);

                    //special Effects (damage, crit, vampirism)
                    string bonus = "";
                    //damage %
                    //if (ammo.ammoDamageBonus > 0) bonus = bonus + "Damage +" + ammo.ammoDamageBonus + "% ";
                    if (bonus.Length > 0)
                    {
                        go.panels[3].SetActive(true);
                        go.strings[3].text = "Special Effects :";
                        go.stringsValue[3].text = bonus;
                        index += 1;
                    }
                }
                else
                {
                    go.strings[0].text = Localization.Translate("Type") + " :";
                    go.stringsValue[0].text = Localization.Translate("Resource");
                    index += 1;

                    //amount
                    go.strings[index].text = Localization.Translate("Amount") + " :";
                    go.stringsValue[index].text = slot.amount.ToString();
                    index += 1;

                    //weight
                    go.panels[index].SetActive(true);
                    go.strings[index].text = Localization.Translate("Weight") + " :";
                    if (slot.amount == 1) go.stringsValue[index].text = slot.item.data.GetItemWeight(1);
                    else go.stringsValue[index].text = slot.item.data.GetItemWeight(1) + " / " + slot.item.data.GetItemWeight(slot.amount);
                    index += 1;

                    //tradable
                    TradableInfo(go, slot.item.data, index);

                    index = PriceInfo(go, slot.item.data, index);
                }

                //description
                go.itemDescription.text = slot.item.data.GetDescriptionByLanguage(Localization.languageCurrent);
            }
        }

        private int DurabilityInfo(ToolTipItemExtended go, int index)
        {
            go.panels[index].SetActive(true);
            go.strings[index].text = Localization.Translate("Durability") + " :";
            go.stringsValue[index].text = slot.item.durability + "/" + slot.item.data.maxDurability;
            if (slot.item.durability < 1) go.stringsValue[index].color = Color.red;
            return index += 1;
        }
        private int TradableInfo(ToolTipItemExtended go, ScriptableItem item, int index)
        {
            go.panels[index].SetActive(true);
            go.strings[index].text = Localization.Translate("Tradable") + " :";
            go.stringsValue[index].text = item.tradable ? Localization.Translate("TradableYes") : Localization.Translate("TradableNo");
            return index += 1;
        }
        private int PriceInfo(ToolTipItemExtended go, ScriptableItem item, int index)
        {
            go.panels[index].SetActive(true);
            go.strings[index].text = Localization.Translate("Price") + " :";
            go.stringsValue[index].text = item.buyPrice.ToString();
            return index += 1;
        }

        private int SpecialEffectsInfo(ToolTipItemExtended go, int index)
        {
            string temp = SpecialEffects(slot.item.data);
            if (temp.Length > 0)
            {
                go.panels[index].SetActive(true);
                go.strings[index].text = "Special Effects :";
                go.stringsValue[index].text = temp;
                index += 1;
            }
            return index += 1;
        }
        private string SpecialEffects(ScriptableItem item)
        {
            string info = "";
            if (item is EquipmentItem go)
            {
                //health
                if (go.healthBonus > 0) info = info + "Health +" + go.healthBonus + " ";
                //mana
                /*if (go.manaBonus > 0) info = info + "Mana +" + go.manaBonus + " ";
                //stamina
                if (go.staminaBonus > 0) info = info + "Stamina +" + go.staminaBonus + " ";

                //health %
                if (go.healthPercent > 0) info = info + "Health +" + go.healthPercent + "% ";
                //mana %
                if (go.manaPercent > 0) info = info + "Mana +" + go.manaPercent + "% ";
                //stamina %
                if (go.staminaPercent > 0) info = info + "Stamina +" + go.staminaPercent + "% ";

                //damage %
                if (go.damagePercent > 0) info = info + "Damage +" + go.damagePercent + "% ";
                //defense %
                if (go.defensePercent > 0) info = info + "Defense +" + go.defensePercent + "% ";

                //speed
                if (go.moveSpeedBonus > 0) info = info + "Move speed +" + go.moveSpeedBonus + " ";*/

                //accuracy
                //if (go.accuracyBonus > 0) info = info + "Accuracy +" + go.accuracyBonus + " ";
                //dodge
                //if (go.dodgeBonus > 0) info = info + "Dodge +" + go.dodgeBonus + " ";
            }
            /*else if (item is PotionsExtendedItem potion)
            {
                //health
                if (potion.buff.healthMaxBonus.baseValue > 0) info = info + "Health +" + potion.buff.healthMaxBonus.baseValue + " ";
                //mana
                if (potion.buff.manaMaxBonus.baseValue > 0) info = info + "Mana +" + potion.buff.manaMaxBonus.baseValue + " ";
                //stamina
                if (potion.buff.staminaMaxBonus.baseValue > 0) info = info + "Stamina +" + potion.buff.staminaMaxBonus.baseValue + " ";

                //health
                if (potion.buff.healthBonusPercent.baseValue > 0) info = info + "Health +" + potion.buff.healthBonusPercent.baseValue + "% ";
                //mana
                if (potion.buff.manaBonusPercent.baseValue > 0) info = info + "Mana +" + potion.buff.manaBonusPercent.baseValue + "% ";
                //stamina
                if (potion.buff.staminaPercentPerSecondBonus.baseValue > 0) info = info + "Stamina +" + potion.buff.staminaPercentPerSecondBonus.baseValue + "% ";

                //damage %
                if (potion.buff.damageBonusPercent.baseValue > 0) info = info + "Damage +" + potion.buff.damageBonusPercent.baseValue + "% ";
                //defense %
                if (potion.buff.defenseBonusPercent.baseValue > 0) info = info + "Defense +" + potion.buff.defenseBonusPercent.baseValue + "% ";
            }*/

            return info;
        }
    }
}


