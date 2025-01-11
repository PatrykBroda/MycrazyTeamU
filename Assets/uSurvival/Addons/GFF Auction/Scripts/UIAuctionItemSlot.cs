using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public class UIAuctionItemSlot : MonoBehaviour
    {
        [Header("Item")]
        public UniversalSlot slot;
        public Text textItemName;

        [Header("Item Price")]
        public Text textPrice;

        [Header("Owner")]
        public Text textOwner;
        public Text textOwnerValue;

        [Header("Buttons")]
        public Button buttonBuy;
        public Button buttonUnregister;
        public Button buttonGetMoney;
        public Button buttonFavorites;

        [Header("Upgrade Addon")]
        public Transform runesContent;
        public Text textUpgrade;
    }
}