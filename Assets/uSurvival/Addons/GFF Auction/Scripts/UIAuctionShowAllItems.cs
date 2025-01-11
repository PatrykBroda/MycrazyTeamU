using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIAuctionShowAllItems : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonMyItems;
        [SerializeField] private GameObject content;
        [SerializeField] private Dropdown dropdownSort;
        [SerializeField] private InputField inputFieldSearchByName;
        [SerializeField] private Dropdown dropdownType;
        [SerializeField] private Dropdown dropdownSubType;
        [SerializeField] private Text textGoldInInventory;
        [SerializeField] private Button buttonSearch;
        [SerializeField] private Button buttonSearchByName;
        [SerializeField] private Text textInfo;
        [SerializeField] private GameObject prefabAuctionSlot;
        [SerializeField] private GameObject prefabUpgradeImage;
        [SerializeField] private Button buttonFavorites;

        [Header("Components")]
        [SerializeField] private UIAuctionShowMyItems myItems;
        [SerializeField] private UIAuctionFavorites favorites;

        private ScriptableAuctionItems scriptableAuctionItems;
        private double infoTimeEnd;

        //singleton
        public static UIAuctionShowAllItems singleton;
        public UIAuctionShowAllItems() { singleton = this; }

        private void Start()
        {
            buttonMyItems.onClick.SetListener(() =>
            {
                Hide();
                myItems.Show();
            });

                                buttonFavorites.onClick.SetListener(() =>
                    {
                        favorites.Show();
                    });
        }

        public void Show(Player player)
        {
            scriptableAuctionItems = player.auction.itemTypes;

            //load DropdownType
            dropdownType.ClearOptions();
            dropdownType.AddOptions(scriptableAuctionItems.GetTypesToStrings());

            dropdownSubType.ClearOptions();
            dropdownSubType.AddOptions(scriptableAuctionItems.GetSubTypesToStrings(dropdownType.value));

            //all dropdowns change tracking
            dropdownType.onValueChanged.AddListener(ChangeDropdownTypeValue);
            dropdownSubType.onValueChanged.AddListener(ChangeDropdownSubTypeValue);

            SearchItemsByCategory(player);

            myItems.Hide();
            panel.SetActive(true); 
        }
        public void Hide() { panel.SetActive(false); }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0)
                {
                    //button Favorites 
                    buttonFavorites.gameObject.SetActive(player.auction.useFavoritesSearch);

                    buttonSearchByName.onClick.SetListener(() =>
                    {
                        player.auction.searchState = PlayerAuction.SearchState.byName;
                        buttonSearch.interactable = false;
                        buttonSearchByName.interactable = false;
                        player.auction.CmdSearchItemsByName(inputFieldSearchByName.text);

                        StartCoroutine(SetActiveNameSearch());
                    });

                    buttonSearch.interactable = (player.auction.refreshTimeoutEnd < NetworkTime.time);
                    buttonSearch.onClick.SetListener(() =>
                    {
                        SearchItemsByCategory(player);
                    });

                    //show how much gold a player has in inventory
                    textGoldInInventory.text = player.gold == 0 ? "0" : player.gold.ToString("###,###,###,###");

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefabAuctionSlot, player.auction.allItems.Count, content.transform);

                    var sortedList = SortList(player.auction.allItems.ToList());

                    for (int i = 0; i < sortedList.Count; i++)
                    {
                        UIAuctionItemSlot slot = content.transform.GetChild(i).GetComponent<UIAuctionItemSlot>();

                        //item name
                        slot.textItemName.text = sortedList[i].itemslot.item.name;

                        //price
                        slot.textPrice.text = sortedList[i].price == 0 ? "0" : sortedList[i].price.ToString("###,###,###,###");

                        //if item Equipment item
                        if (sortedList[i].itemslot.item.data is UsableItem)
                        {
                            //upgrade
                            //if (player.auction.useUpgradeAddon)
                            //{
                            //    //show How enchanted is the item
                            //    if (sortedList[i].itemslot.item.upgradeInd != null && sortedList[i].itemslot.item.upgradeInd.Length > 0)
                            //        slot.slot.upgradeText.text = "+" + sortedList[i].itemslot.item.upgradeInd.Length;
                            //    else slot.slot.upgradeText.text = "";

                            //    if (player.auction.upgradeAddonPaintRunes)
                            //    {
                            //        if ((!player.auction.upgradeAddonUseHoles && sortedList[i].itemslot.item.upgradeInd != null && sortedList[i].itemslot.item.upgradeInd.Length > 0) || (player.auction.upgradeAddonUseHoles && sortedList[i].itemslot.item.holes > 0))
                            //        {
                            //            slot.textUpgrade.text = "Upgrade : ";

                            //            //instantiate
                            //            if (player.auction.upgradeAddonUseHoles) UIUtils.BalancePrefabs(prefabUpgradeImage, sortedList[i].itemslot.item.holes, slot.runesContent);
                            //            else UIUtils.BalancePrefabs(prefabUpgradeImage, sortedList[i].itemslot.item.upgradeInd.Length, slot.runesContent);

                            //            //fill runes images 
                            //            for (int y = 0; y < sortedList[i].itemslot.item.upgradeInd.Length; y++)
                            //            {
                            //                EnchantmentRuneItem item = EnchantmentRuneItem.GetRuneFromDictByType(sortedList[i].itemslot.item.upgradeInd[y]);
                            //                slot.runesContent.GetChild(y).GetComponent<Image>().sprite = item.image;
                            //                slot.runesContent.GetChild(y).GetComponent<Image>().color = Color.white;
                            //            }
                            //        }
                            //    }
                            //}
                        }

                        //tooltip
                        slot.slot.tooltip.enabled = true;
                        if (slot.slot.tooltip.IsVisible())
                            slot.slot.tooltip.text = sortedList[i].itemslot.ToolTip();

                        slot.slot.dragAndDropable.dragable = false;
                        slot.slot.image.color = Color.white;
                        slot.slot.image.sprite = sortedList[i].itemslot.item.image;
                        slot.slot.amountOverlay.SetActive(sortedList[i].itemslot.amount > 1);
                        slot.slot.amountText.text = sortedList[i].itemslot.amount.ToString();

                        int icopy = i;
                        slot.buttonGetMoney.gameObject.SetActive(false);
                        slot.buttonUnregister.gameObject.SetActive(false);

                        slot.buttonBuy.interactable = (sortedList[icopy].owner != Player.localPlayer.name);
                        slot.buttonBuy.onClick.SetListener(() =>
                        {
                            //check free space in inventory
                            if (player.inventory.CanAdd(sortedList[icopy].itemslot.item, sortedList[icopy].itemslot.amount))
                            {
                                //if the player has enough gold
                                if (player.gold >= sortedList[icopy].price)
                                {
                                    player.auction.CmdBuyItem(sortedList[icopy].id, player.auction.searchState, inputFieldSearchByName.text);
                                }
                                else
                                {
                                    textInfo.text = "Not enough Gold";
                                    infoTimeEnd = NetworkTime.time + player.auction.infoMessageTime;
                                }
                            }
                            else
                            {
                                textInfo.text = "Inventory Full";
                                infoTimeEnd = NetworkTime.time + player.auction.refreshTimeout;
                            }
                        });

                        slot.buttonFavorites.onClick.SetListener(() =>
                        {
                            player.auction.CmdAddToFavorites(sortedList[icopy].itemslot.item.name);
                        });

                        // addon system hooks (Item rarity)
                        UtilsExtended.InvokeMany(typeof(UIAuctionShowAllItems), this, "Update_", slot.slot, sortedList[i].itemslot.item);
                    }

                    if (NetworkTime.time >= infoTimeEnd) textInfo.text = "";
                }
            }
        }

        private List<Auction> SortList(List<Auction> list)
        {
            if (dropdownSort.value == 0) list = list.OrderBy(u => u.price).ToList();
            else if (dropdownSort.value == 1) list = list.OrderByDescending(u => u.price).ToList();
            else if (dropdownSort.value == 2) list = list.OrderBy(u => u.itemslot.item.name).ToList();
            else if (dropdownSort.value == 3) list = list.OrderByDescending(u => u.itemslot.item.name).ToList();

            return list;
        }

        private IEnumerator SetActiveNameSearch()
        {
            yield return new WaitForSeconds(1.5f);
            buttonSearchByName.interactable = true;
        }

        private void SearchItemsByCategory(Player player)
        {
            player.auction.searchState = PlayerAuction.SearchState.byLeftPanel;
            player.auction.refreshTimeoutEnd = NetworkTime.time + player.auction.refreshTimeout;

            player.auction.CmdSearchItemsByCategory(
                scriptableAuctionItems.itemTypes[dropdownType.value].typeName,
                scriptableAuctionItems.itemTypes[dropdownType.value].subTypes[dropdownSubType.value]);

        }

        private void ChangeDropdownTypeValue(int id)
        {
            //updtae dropdoun sub category
            dropdownSubType.ClearOptions();
            dropdownSubType.onValueChanged.RemoveAllListeners();
            dropdownSubType.AddOptions(scriptableAuctionItems.GetSubTypesToStrings(id));
            dropdownSubType.value = 0;
            dropdownSubType.onValueChanged.SetListener(ChangeDropdownSubTypeValue);

            //update dropdown levels
            ChangeDropdownSubTypeValue(0);
        }
        private void ChangeDropdownSubTypeValue(int id)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                SearchItemsByCategory(player);
            }
        }

        //server response functions
        public void ReceiveMsgAlreadySold(Player player)
        {
            textInfo.text = "Item already sold";
            infoTimeEnd = NetworkTime.time + player.auction.infoMessageTime;
        }
        public void ReceiveMsgNotEnoughGold(Player player)
        {
            textInfo.text = "Not enough Gold";
            infoTimeEnd = NetworkTime.time + player.auction.infoMessageTime;
        }
    }
}