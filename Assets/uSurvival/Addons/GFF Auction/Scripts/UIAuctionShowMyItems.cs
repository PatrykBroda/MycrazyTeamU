using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIAuctionShowMyItems : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonAllItems;
        [SerializeField] private Text textInfo;
        [SerializeField] private Text textAvailableRegistrationCount;
        [SerializeField] private Text textRegistrationCountValue;
        [SerializeField] private Text textNumberSalesValue;
        [SerializeField] private Text textTaxForRegistration;
        [SerializeField] private Text textTaxForRegistrationValue;
        [SerializeField] private Text textSalesTaxAuction;
        [SerializeField] private Text textSalesTaxAuctionValue;
        [SerializeField] private Button buttonItemRegistration;
        [SerializeField] private GameObject content;
        [SerializeField] private GameObject prefabAuctionSlot;
        [SerializeField] private GameObject prefabUpgradeImage;

        [Header("Components")]
        [SerializeField] private UIAuctionShowAllItems allItems;
        [SerializeField] private UIRegistrationItemOnAuction itemRegistration;

        private double infoTimeEnd;

        public void Show() { panel.SetActive(true); }
        public void Hide() { panel.SetActive(false); }

        private void Start()
        {
            //hide the button if the number of registered is equal to the maximum
            buttonItemRegistration.onClick.SetListener(() =>
            {
                itemRegistration.Show();
                panel.SetActive(false);
            });
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0)
                {
                    buttonAllItems.onClick.SetListener(() =>
                    {
                        allItems.Show(player);
                    });

                    //Show Tax for registration item
                    textTaxForRegistration.gameObject.SetActive(player.auction.GetRegistrationTax() > 0);
                    textTaxForRegistrationValue.gameObject.SetActive(player.auction.GetRegistrationTax() > 0);
                    textTaxForRegistrationValue.text = player.auction.GetRegistrationTax() + " Gold";

                    //Show sales tax
                    textSalesTaxAuction.gameObject.SetActive(player.auction.GetSalesTax() > 0);
                    textSalesTaxAuctionValue.gameObject.SetActive(player.auction.GetSalesTax() > 0);
                    textSalesTaxAuctionValue.text = player.auction.GetSalesTax() + " %";

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefabAuctionSlot, player.auction.myItems.Count, content.transform);

                    player.auction.numberSalesValue = 0;

                    for (int i = 0; i < player.auction.myItems.Count; i++)
                    {
                        UIAuctionItemSlot slot = content.transform.GetChild(i).GetComponent<UIAuctionItemSlot>();
                        ItemSlot itemslot = player.auction.myItems[i].itemslot;

                        slot.slot.dragAndDropable.dragable = false;
                        slot.slot.image.color = Color.white;
                        slot.slot.image.sprite = itemslot.item.image;
                        slot.slot.amountOverlay.SetActive(itemslot.amount > 1);
                        slot.slot.amountText.text = itemslot.amount.ToString();

                        //tooltip
                        slot.slot.tooltip.enabled = true;
                        if (slot.slot.tooltip.IsVisible())
                            slot.slot.tooltip.text = itemslot.ToolTip();

                        slot.textItemName.text = itemslot.item.name;

                        //price
                        slot.textPrice.text = player.auction.myItems[i].price == 0 ? "0" : player.auction.myItems[i].price.ToString("###,###,###,###");

                        //if this Equipment Item
                        if (itemslot.item.data is EquipmentItem)
                        {
                            //if upgrade addon is used
                            if (!player.auction.useUpgradeAddon)
                            {
                                //null upgrade info
                                slot.slot.upgradeText.text = "";
                                slot.textUpgrade.text = "";
                                UIUtils.BalancePrefabs(prefabUpgradeImage, 0, slot.runesContent);
                            }
                            //else
                            //{
                            //    if (itemslot.item.upgradeInd != null && itemslot.item.upgradeInd.Length > 0) slot.slot.upgradeText.text = "+" + itemslot.item.upgradeInd.Length;
                            //    else slot.slot.upgradeText.text = "";

                            //    if (player.auction.upgradeAddonPaintRunes)
                            //    {
                            //        if ((!player.auction.upgradeAddonUseHoles && itemslot.item.upgradeInd != null && itemslot.item.upgradeInd.Length > 0) || (player.auction.upgradeAddonUseHoles && itemslot.item.holes > 0))
                            //        {
                            //            slot.textUpgrade.text = "Upgrade : ";

                            //            //instantiate
                            //            if (player.auction.upgradeAddonUseHoles) UIUtils.BalancePrefabs(prefabUpgradeImage, itemslot.item.holes, slot.runesContent);
                            //            else UIUtils.BalancePrefabs(prefabUpgradeImage, itemslot.item.upgradeInd.Length, slot.runesContent);

                            //            //fill runes images 
                            //            for (int y = 0; y < itemslot.item.upgradeInd.Length; y++)
                            //            {
                            //                EnchantmentRuneItem item = EnchantmentRuneItem.GetRuneFromDictByType(itemslot.item.upgradeInd[y]);

                            //                slot.runesContent.GetChild(y).GetComponent<Image>().sprite = item.image;
                            //                slot.runesContent.GetChild(y).GetComponent<Image>().color = Color.white;
                            //            }
                            //        }
                            //        else
                            //        {
                            //            //null upgrade info
                            //            slot.textUpgrade.text = "";
                            //            UIUtils.BalancePrefabs(prefabUpgradeImage, 0, slot.runesContent);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        //null upgrade info
                            //        slot.textUpgrade.text = "";
                            //        UIUtils.BalancePrefabs(prefabUpgradeImage, 0, slot.runesContent);
                            //    }
                            //}
                        }
                        else
                        {
                            //null upgrade info
                            slot.slot.upgradeText.text = "";
                            slot.textUpgrade.text = "";
                            UIUtils.BalancePrefabs(prefabUpgradeImage, 0, slot.runesContent);
                        }

                        // addon system hooks (Item rarity)
                        UtilsExtended.InvokeMany(typeof(UIAuctionShowMyItems), this, "Update_", slot.slot, itemslot.item);

                        slot.buttonBuy.gameObject.SetActive(false);
                        slot.buttonFavorites.gameObject.SetActive(false);

                        int icopy = i;
                        //if the item has not yet been sold
                        if (string.IsNullOrEmpty(player.auction.myItems[i].buyer))
                        {
                            slot.textOwnerValue.text = "";

                            slot.buttonUnregister.gameObject.SetActive(true);
                            slot.buttonUnregister.onClick.SetListener(() =>
                            {
                                if (player.inventory.CanAdd(itemslot.item, itemslot.amount))
                                {
                                    player.auction.CmdUnregisterItem(player.auction.myItems[icopy].id);
                                }
                                else
                                {
                                    textInfo.text = "Inventory Full";
                                    infoTimeEnd = NetworkTime.time + player.auction.refreshTimeout;
                                }
                            });

                            slot.buttonGetMoney.gameObject.SetActive(false);
                        }
                        //if the item is already sold
                        else
                        {
                            slot.textOwner.text = "Buyer :";
                            slot.buttonBuy.gameObject.SetActive(false);
                            slot.buttonUnregister.gameObject.SetActive(false);

                            slot.buttonGetMoney.gameObject.SetActive(true);
                            slot.buttonGetMoney.onClick.SetListener(() =>
                            {
                                player.auction.CmdGetGoldForSoldGoods(player.auction.myItems[icopy].id);
                            });

                            player.auction.numberSalesValue += 1;
                        }
                    }

                    //Show the number of our sales
                    textRegistrationCountValue.text = player.auction.myItems.Count.ToString();
                    textNumberSalesValue.text = player.auction.numberSalesValue.ToString();

                    if (NetworkTime.time >= infoTimeEnd) textInfo.text = "";
                }
            }
        }
    }
}