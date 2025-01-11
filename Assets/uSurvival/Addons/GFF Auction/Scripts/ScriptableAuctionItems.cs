using System;
using System.Collections.Generic;
using UnityEngine;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uMMORPG Auction Items By Types", order = 999)]
    public class ScriptableAuctionItems : ScriptableObject
    {
        [Serializable]
        public class AuctionItemType
        {
            public string typeName;
            public ItemType type;
            public string[] subTypes;
        }

        public List<AuctionItemType> itemTypes = new List<AuctionItemType>() { };

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < itemTypes.Count; i++)
            {
                itemTypes[i].typeName = ((ItemType)i).ToString();
            }
        }
#endif

        public List<string> GetTypesToStrings()
        {
            List<string> strings = new List<string>() { };
            for (int i = 0; i < itemTypes.Count; i++)
            {
                strings.Add(itemTypes[i].typeName);
            }

            return strings;
        }

        public List<string> GetSubTypesToStrings(int typeIndex)
        {
            List<string> strings = new List<string>() { };
            for (int i = 0; i < itemTypes[typeIndex].subTypes.Length; i++)
            {
                strings.Add(itemTypes[typeIndex].subTypes[i]);
            }

            return strings;
        }
    }
}


