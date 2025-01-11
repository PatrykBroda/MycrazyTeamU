using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [Serializable]public class CustomizationObject
    {
        public string name;
        public bool showWhenCharacterCreate = false;
        public bool disableHairInThisSuit = false;
        public bool disableHatInThisSuit = false;
        public GameObject[] parts;
        public EquipmentItem item;
    }

    [Serializable]public class Customization
    {
        public string name;
        public EquipmentItemType type;
        public CustomizationObject[] objects;
        //public bool disableForSuit = false;
        public bool disableForHat = false;
        public bool colorChange = false;
    }

    public struct CustomizationByType
    {
        public EquipmentItemType type;
        public sbyte defaultValue;
        //public int customValue;
        public byte materialValue;
    }

    public struct BoughtSuits
    {
        public string classname;
        public string suitname;
    }

    [DisallowMultipleComponent]
    public class PlayerCustomization : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        [Header("Components")]
        public Material[] materials;

        public Customization[] templates;
        [HideInInspector] public readonly SyncList<CustomizationByType> values = new SyncList<CustomizationByType>();
        public readonly SyncList<BoughtSuits> boughtSuits = new SyncList<BoughtSuits>();
        [HideInInspector] public List<CustomizationByType> localvalues = new List<CustomizationByType>();

        public override void OnStartClient()
        {
            base.OnStartClient();

            SetCustomization();

            // setup synclist callbacks on client. no need to update and show and
            // animate equipment on server
            values.Callback += OnEquipmentChanged;
        }

        private void OnEquipmentChanged(SyncList<CustomizationByType>.Operation op, int index, CustomizationByType oldSlot, CustomizationByType newSlot)
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

            // update the model
            SetCustomization();
        }

        //for Character creation
        public void SetupForNewCharacter()
        {
            for (int i = 0; i < templates.Length; i++)
            {
                CustomizationByType typeValues = new CustomizationByType();
                typeValues.type = templates[i].type;
                typeValues.defaultValue = ItemListByType(templates[i].objects) ? (sbyte)0 : (sbyte)-1;
                //typeValues.customValue = -1;
                values.Add(typeValues);
            }

            Randomize();
        }

        private bool ItemListByType(CustomizationObject[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].showWhenCharacterCreate) return true;
            }
            return false;
        }

        public List<Customization> GetItemsForCharacterCreate()
        {
            List<Customization> list = new List<Customization>();

            for (int t = 0; t < templates.Length; t++)
            {
                List<CustomizationObject> types = new List<CustomizationObject>();
                for (int x = 0; x < templates[t].objects.Length; x++)
                {
                    if (templates[t].objects[x].showWhenCharacterCreate) types.Add(templates[t].objects[x]);
                }

                if (types.Count > 0)
                {
                    Customization temp = new Customization();
                    temp.type = templates[t].type;
                    temp.objects = types.ToArray();
                    list.Add(temp);
                }
            }

            return list;
        }

        private sbyte FindItemIndexInTemplatesByName(int typeindexInTemplates, string itemname)
        {
            for (sbyte i = 0; i < templates[typeindexInTemplates].objects.Length; i++)
            {
                if (templates[typeindexInTemplates].objects[i].parts[0] != null && templates[typeindexInTemplates].objects[i].parts[0].name == itemname) return i;
            }

            return -1;
        }

        public int FindItemIndexInEditedTypes(List<Customization> editedTypes, int index)
        {
            int typeIndexInTemplates = FindTypeIndexByType(editedTypes[index].type);
            if (typeIndexInTemplates != -1)
            {
                GameObject go = templates[typeIndexInTemplates].objects[values[typeIndexInTemplates].defaultValue].parts[0];
                for (int i = 0; i < editedTypes[index].objects.Length; i++)
                {
                    //if (editedTypes[index].objects[i].parts[0].Equals(go)) return i;
                    if (editedTypes[index].objects[i].parts[0] == null)
                    {
                        if (go == null) return i;
                    }
                    else
                    {
                        if (editedTypes[index].objects[i].parts[0].name == go.name) return i;
                    }
                }
            }

            return 0;
        }

        public int FindTypeIndexByType(EquipmentItemType type)
        {
            for (int i = 0; i < templates.Length; i++)
            {
                if (templates[i].type == type) return i;
            }
            return -1;
        }

        public int FindTypeIndexByName(string type)
        {
            for (int i = 0; i < templates.Length; i++)
            {
                if (templates[i].type.ToString() == type) return i;
            }
            return -1;
        }

        public void Randomize()
        {
            List<Customization> editedTypes = GetItemsForCharacterCreate();
            for (int i = 0; i < editedTypes.Count; i++)
            {
                int typeIndexInTemplates = FindTypeIndexByType(editedTypes[i].type);
                if (typeIndexInTemplates != -1)
                {
                    int rnd = UnityEngine.Random.Range(0, editedTypes[i].objects.Length);
                    CustomizationByType temp = values[typeIndexInTemplates];
                    temp.defaultValue = editedTypes[i].objects[rnd].parts[0] == null ? (sbyte)0 : FindItemIndexInTemplatesByName(typeIndexInTemplates, editedTypes[i].objects[rnd].parts[0].name);
                    if (temp.type == EquipmentItemType.Suit) temp.materialValue = (byte)UnityEngine.Random.Range(0, materials.Length);
                    values[typeIndexInTemplates] = temp;   
                }
            }


            //templates[indexSuit].objects[values[indexSuit].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[values[indexSuit].materialValue];

            SetCustomization();
        }

        public void SetCustomization()
        {
            //Debug.Log("SetCustomization for player " + player.name);
            int indexSuit = FindTypeIndexByType(EquipmentItemType.Suit);
            for (int t = 0; t < templates.Length; t++)
            {
                //disable hats for suit
                if (templates[t].type == EquipmentItemType.Hats && templates[indexSuit].objects[values[indexSuit].defaultValue].disableHatInThisSuit)
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                }
                //disable hair for suit
                else if (templates[t].type == EquipmentItemType.Hair && templates[indexSuit].objects[values[indexSuit].defaultValue].disableHairInThisSuit)
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                }
                else
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                    {
                        bool active = false;
                        //if (values[t].customValue == -1)
                            active = values[t].defaultValue == x;
                        //else active = values[t].customValue == x;

                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(active);
                    }
                }
            }

            //set color
            if (indexSuit != -1)templates[indexSuit].objects[values[indexSuit].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[values[indexSuit].materialValue];
        }

        private void SetLocalCustomization()
        {
            int indexSuit = FindTypeIndexByType(EquipmentItemType.Suit);
            for (int t = 0; t < templates.Length; t++)
            {
                //disable hats for suit
                if (templates[t].type == EquipmentItemType.Hats && templates[indexSuit].objects[localvalues[indexSuit].defaultValue].disableHatInThisSuit)
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                }
                //disable hair for suit
                else if (templates[t].type == EquipmentItemType.Hair && templates[indexSuit].objects[localvalues[indexSuit].defaultValue].disableHairInThisSuit)
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                }
                else
                {
                    for (int x = 0; x < templates[t].objects.Length; x++)
                    {
                        bool active = false;
                        //if (values[t].customValue == -1)
                        active = localvalues[t].defaultValue == x;
                        //else active = values[t].customValue == x;

                        for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                            if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(active);
                    }
                }
            }

            templates[indexSuit].objects[localvalues[indexSuit].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[localvalues[indexSuit].materialValue];
        }

        public void SetCustomizationLocalByItem(EquipmentItemType type, string itemname)
        {
            //проверяем тип предмета который хотим поменять - доступно ли ?
            int typeIndexInTamplates = FindTypeIndexByType(type);
            if (typeIndexInTamplates != -1)
            {
                sbyte newValue = string.IsNullOrEmpty(itemname) ? (sbyte)0 : FindItemIndexInTemplatesByName(typeIndexInTamplates, itemname);

                //set new value
                CustomizationByType temp = values[typeIndexInTamplates];
                temp.defaultValue = newValue;
                values[typeIndexInTamplates] = temp;

                int indexSuit = FindTypeIndexByType(EquipmentItemType.Suit);
                for (int t = 0; t < templates.Length; t++)
                {
                    //disable hats for suit
                    if (templates[t].type == EquipmentItemType.Hats && templates[indexSuit].objects[values[indexSuit].defaultValue].disableHatInThisSuit)
                    {
                        for (int x = 0; x < templates[t].objects.Length; x++)
                            for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                                if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                    }
                    //disable hair for suit
                    else if (templates[t].type == EquipmentItemType.Hair && templates[indexSuit].objects[values[indexSuit].defaultValue].disableHairInThisSuit)
                    {
                        for (int x = 0; x < templates[t].objects.Length; x++)
                            for (int p = 0; p < templates[t].objects[x].parts.Length; p++)
                                if (templates[t].objects[x].parts[p] != null) templates[t].objects[x].parts[p].SetActive(false);
                    }
                    else
                    {
                        for (int i = 0; i < templates[t].objects.Length; i++)
                            for (int p = 0; p < templates[t].objects[i].parts.Length; p++)
                                if (templates[t].objects[i].parts[p] != null) templates[t].objects[i].parts[p].SetActive(values[t].defaultValue == i);

                        if (templates[t].type == EquipmentItemType.Suit)
                        {
                            templates[t].objects[values[t].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[values[t].materialValue];
                        }
                    }
                }
            }
        }

        [Command]public void CmdSetColorForSuit(EquipmentItemType type, byte matIndex)
        {
            int typeIndex = FindTypeIndexByType(type);
            if (typeIndex != -1)
            {
                //set new value
                CustomizationByType temp = values[typeIndex];
                temp.materialValue = matIndex;
                values[typeIndex] = temp;

                templates[typeIndex].objects[values[typeIndex].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[matIndex];
            }
        }

        public void SetColorForSuit(EquipmentItemType type, byte matIndex)
        {
            int typeIndex = FindTypeIndexByType(type);
            if (typeIndex != -1)
            {
                //set new value
                CustomizationByType temp = values[typeIndex];
                temp.materialValue = matIndex;
                values[typeIndex] = temp;

                templates[typeIndex].objects[values[typeIndex].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[matIndex];
            }
        }

        public void SetColorForSuitLocal(EquipmentItemType type, byte matIndex)
        {
            int typeIndex = FindTypeIndexByType(type);
            if (typeIndex != -1)
            {
                //set new value
                CustomizationByType temp = localvalues[typeIndex];
                temp.materialValue = matIndex;
                localvalues[typeIndex] = temp;

                templates[typeIndex].objects[localvalues[typeIndex].defaultValue].parts[0].GetComponent<SkinnedMeshRenderer>().material = materials[matIndex];
            }
        }

        //for Player Equipment Work
        [Server]public void UpdateCustomizationValue(EquipmentItem data)
        {
            int index = FindTypeIndexByType(data.equipmentItemType);
            if (index != -1)
            {
                sbyte itemPosition = ItemPosition(data, index);

                CustomizationByType temp = values[index];
                temp.defaultValue = itemPosition;
                values[index] = temp;
            }
        }

        [Server]public void RemoveItemFromEquip(EquipmentItemType type)
        {
            int index = FindTypeIndexByType(type);
            if (index != -1)
            {
                CustomizationByType temp = values[index];
                temp.defaultValue = -1;
                values[index] = temp;
            }
        }

        private sbyte ItemPosition(EquipmentItem data, int typeIndex)
        {
            for (sbyte i = 0; i < templates[typeIndex].objects.Length; i++)
            {
                if (templates[typeIndex].objects[i].item != null && templates[typeIndex].objects[i].item.Equals(data)) return i;
            }

            return -1;
        }

        //Customization in Game
        public void SetupForCustomizationInGame(Player player, sbyte suitIndex)
        {
            for (int i = 0; i < player.customization.values.Count; i++)
            {
                //values.Add(player.customization.values[i]);
                localvalues.Add(player.customization.values[i]);
            }

            int index = FindTypeIndexByType(EquipmentItemType.Suit);
            CustomizationByType _values = localvalues[index];
            _values.defaultValue = suitIndex;
            localvalues[index] = _values;

            SetLocalCustomization();
        }

        public bool SetNewItem(EquipmentItemType type, EquipmentItem newItemData)
        {
            int typeIndex = FindTypeIndexByType(type);
            sbyte newItemIndex = ItemPosition(newItemData, typeIndex);
            if (newItemIndex != -1)
            {
                CustomizationByType _values = values[typeIndex];
                _values.defaultValue = newItemIndex;
                values[typeIndex] = _values;

                return true;
            }

            return false;
        }

        public bool IsSuitAlreadyBought(string name)
        {
            for (int i = 0; i < boughtSuits.Count; i++)
            {
                if (boughtSuits[i].suitname == name && boughtSuits[i].classname == player.className) return true;
            }

            return false;
        }

        [Command]public void CmdWearSuit(sbyte itemIndex)
        {
            int typeIndex = FindTypeIndexByType(EquipmentItemType.Suit);
            CustomizationByType _values = values[typeIndex];
            _values.defaultValue = itemIndex;
            values[typeIndex] = _values;
        }

        public EquipmentItem GetSuitData()
        {
            int typeIndex = FindTypeIndexByType(EquipmentItemType.Suit);

            return templates[typeIndex].objects[values[typeIndex].defaultValue].item;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            player = gameObject.GetComponent<Player>();
            player.customization = this;

            for (int i = 0; i < templates.Length; i++)
            {
                templates[i].name = templates[i].type.ToString();
            }
        }
#endif
    }
}