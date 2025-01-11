using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class UsableItem
    {
        [Header("Customization addon")]
        public EquipmentItemType equipmentItemType;
    }

    public partial struct CharacterCreateMsg
    {
        public CustomizationByType[] customization;
        public float scale;
    }

    public partial class Player
    {
        [Header("Customization")]
        public PlayerCustomization customization;
    }

    public partial class PlayerEquipment
    {
        [Header("Customization")]
        public PlayerCustomization customization;

        /*private int GetSlotIndexByCustomizationType(EquipmentItemType type)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].amount > 0 && ((EquipmentItem)slots[i].item.data).equipmentItemType == type)
                {
                    return i;
                }
            }

            return -1;
        }*/

        private void SwapInventoryEquipForCustomization(int inventoryIndex, int equipmentIndex)
        {
            //необходимо проверить и убрать в инвентарь все предметы которые не совместимы с новым предметом в эквипе
            /*if (slots[equipmentIndex].amount > 0)
            {
                //если предмет является костюмом то переносим предметы которые не должны с ним показываться в инвентарь (штаны и куртки)
                if (inventory.slots[inventoryIndex].item.data is EquipmentItem eqItem && eqItem.equipmentItemType == EquipmentItemType.Suit)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if (slots[i].amount > 0 && slots[i].item.data is EquipmentItem item && item.equipmentItemType != EquipmentItemType.Suit)
                        {
                            customization.RemoveItemFromEquip(item.equipmentItemType);
                            inventory.Add(slots[i].item, slots[i].amount);
                            slots[i] = new ItemSlot();
                        }
                    }
                }
                else if (slots[equipmentIndex].item.data is EquipmentItem eqItem2 && eqItem2.equipmentItemType != EquipmentItemType.Suit)
                {
                    // найти индекс костюма
                    int index = GetSlotIndexByCustomizationType(EquipmentItemType.Suit);
                    if (index != -1)
                    {
                        //перенести костюм в инвентарь
                        customization.RemoveItemFromEquip(eqItem2.equipmentItemType);
                        inventory.Add(slots[index].item, slots[index].amount);
                        slots[index] = new ItemSlot();
                    }
                }
            }*/

            //если перенесли предмет в слот эквипа
            if (slots[equipmentIndex].amount > 0)
            {
                if (slots[equipmentIndex].item.data is EquipmentItem data) customization.UpdateCustomizationValue(data);
            }
            else
            {
                if (inventory.slots[inventoryIndex].item.data is EquipmentItem data) customization.RemoveItemFromEquip(data.equipmentItemType);
            }
        }
    }
}


