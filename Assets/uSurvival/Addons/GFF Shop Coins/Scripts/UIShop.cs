using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public class DescriptionsForShop
    {
        [HideInInspector]public string name;
        public ShopType shopType;
        public LocalizeText[] languages;
    }

    public class UIShop : MonoBehaviour
    {
        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton is null until update
        public AudioSource sound;

        private ShopType shopType = ShopType.Equipment;
        public DescriptionsForShop[] descriptions;
        private string GetDescriptionByType()
        {
            for (int i = 0; i < descriptions.Length; i++)
            {
                if (descriptions[i].shopType == shopType)
                {
                    for (int x = 0; x < descriptions[i].languages.Length; x++)
                    {
                        if (descriptions[i].languages[x].language == Localization.languageCurrent) return descriptions[i].languages[x].description;
                    }    
                }
            }

            return "";
        }

        public GameObject panel;
        public Image imageBackground;
        public Sprite[] sprites;
        public TextMeshProUGUI textCoinsValue;
        public Color colorSelectedType;
        public Color colorNonSelected;

        public Button buttonOpen;
        public GameObject buttonPrefab;
        public Transform buttonsContent;

        public ScrollRect scrollRect;
        public Transform content;
        public UIShopSlot prefab;
        public Text textDescription;
        public GameObject paneAllTypes;

        [Header("Panel Amount")]
        public GameObject panelAmount;
        public Text textItemName;
        public Image imageSlotInPanelSelectAmount;
        public TMP_InputField inputFieldAmount;
        public Button buttonCompleteThePurchase;

        [Header("Currencies")]
        public Transform coinsContainer;
        public GameObject coinsPrefab;

        [Header("Bundles")]
        public GameObject panelBundles;
        public GameObject bundlePrefab;
        public UniversalSlot itemPrefab;

        [Header("Suits")]
        public GameObject panelViewSuit;
        public GameObject panelSuitPrice;
        public TextMeshProUGUI textSuitPrice;
        public Text textSuitName;
        public TextMeshProUGUI textSuitDefenseBonus;
        public TextMeshProUGUI textSuitWeightBonus;
        public TextMeshProUGUI textSuitMoveSpeedBonus;
        public Button buttonBuySuit;
        public Button buttonCloseSuitPanel;
        public Button buttonPreviousSuit;
        public Button buttonNextSuit;
        public Button buttonWearASuit;
        public UICustomizationSlot changeSuitColor;
        public Button buttonChangeColor;

        [Header("Weapon skins")]
        public GameObject weaponSkinPrefab;
        public GameObject weaponSkinContent;

        [Header("Rotate Character")]
        public float strengh = 10;
        private float rotY;
        private bool applyForce;

        public GameObject prefabPosition;
        private GameObject playerPreview;

        private sbyte selectedIndex = -1;
        private ScriptableItem selectedItem;
        private ScriptableItemAndAmount requestedItem;
        public UIShopConfirmation buyConfirmation;

        //singleton
        public static UIShop singleton;
        public UIShop() { singleton = this; }

        private void Start()
        {
            buttonOpen.onClick.SetListener(() =>
            {
                if (panel.activeSelf) panel.SetActive(false);
                else
                {
                    imageBackground.sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
                    panel.SetActive(true);
                }
            });
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null && player.health.current > 0)
                {
                    textDescription.text = GetDescriptionByType();

                    textCoinsValue.text = player.itemMall.coins.ToString();

                    ScriptableItemMall config = player.itemMall.config;
                    ScriptableItem[] items = player.itemMall.GetShopItemByType(shopType);

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(buttonPrefab, config.categories.Length, buttonsContent);

                    // refresh all items
                    for (int i = 0; i < config.categories.Length; ++i)
                    {
                        int icopy = i;
                        GameObject button = buttonsContent.GetChild(i).gameObject;
                        button.GetComponentInChildren<TextMeshProUGUI>().color = shopType == config.categories[i].type ? colorSelectedType : colorNonSelected;
                        button.GetComponentInChildren<TextMeshProUGUI>().text = Localization.Translate(config.categories[i].type.ToString());
                        button.GetComponent<Button>().interactable = shopType != config.categories[i].type;
                        button.GetComponent<Button>().onClick.SetListener(() =>
                        {
                            sound.Play();
                            shopType = config.categories[icopy].type;
                            //textDescription.text = GetDescriptionByType();

                            if (shopType == ShopType.Suits)
                            {
                                items = player.itemMall.GetShopItemByType(shopType);
                                selectedIndex = player.customization.values[player.customization.FindTypeIndexByType(EquipmentItemType.Suit)].defaultValue;
                                selectedItem = items[selectedIndex];
                                InstantiatePrefab(player);
                            }
                        });
                    }

                    coinsContainer.gameObject.SetActive(shopType == ShopType.Currencies);
                    content.gameObject.SetActive(shopType != ShopType.Currencies && shopType != ShopType.Bundles && shopType != ShopType.WeaponSkins);
                    panelBundles.SetActive(shopType == ShopType.Bundles);
                    panelViewSuit.SetActive(shopType == ShopType.Suits);
                    paneAllTypes.SetActive(shopType != ShopType.Suits);
                    weaponSkinContent.SetActive(shopType == ShopType.WeaponSkins);

                    // refresh all items
                    if (shopType == ShopType.Equipment || shopType == ShopType.Weapons || shopType == ShopType.Ammo || shopType == ShopType.Powers)
                    {
                        scrollRect.content = content.GetComponent<RectTransform>();

                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(prefab.gameObject, items.Length, content);

                        for (sbyte i = 0; i < items.Length; ++i)
                        {
                            UIShopSlot slot = content.GetChild(i).GetComponent<UIShopSlot>();
                            ScriptableItem item = items[i];
                            slot.textDecription.text = Localization.Translate(item.name).ToString();
                            if (item is AmmoItem) slot.textDecription.text += "\n" + item.maxStack;
                            slot.imageLogo.sprite = item.image;
                            slot.textPrice.text = item.itemMallPrice.ToString();
                            if (shopType == ShopType.Ammo) slot.textAmount.text = "x 300";
                            else slot.textAmount.text = "x 1";

                            // refresh valid item
                            slot.tooltip.enabled = shopType != ShopType.Ammo;
                            // only build tooltip while it's actually shown. this
                            // avoids MASSIVE amounts of StringBuilder allocations.
                            if (slot.tooltip.IsVisible())
                                slot.tooltip.text = new ItemSlot(new Item(item)).ToolTip();

                            slot.buttonBuy.gameObject.GetComponentInChildren<Text>().text = Localization.Translate("Buy");

                            sbyte icopy = i;
                            slot.buttonBuy.onClick.SetListener(() =>
                            {
                                sound.Play();

                                selectedIndex = icopy;
                                selectedItem = item;
                                requestedItem.item = item;

                                if (item.maxStack == 1)
                                {
                                    if (player.itemMall.coins >= item.itemMallPrice)
                                    {
                                        requestedItem.item = item;
                                        requestedItem.amount = 1;
                                        player.itemMall.CmdBuyItem(shopType, selectedIndex, 1);
                                    }
                                    else shopType = ShopType.Currencies;
                                }
                                else
                                {
                                    if (player.itemMall.coins >= item.itemMallPrice)
                                        panelAmount.SetActive(true);
                                    else shopType = ShopType.Currencies;
                                }
                            });
                        }
                    }
                    else if (shopType == ShopType.Suits)
                    {
                        textSuitName.text = Localization.Translate(selectedItem.name);
                        textSuitDefenseBonus.text = ((EquipmentItem)selectedItem).defenseBonus.ToString();
                        //textSuitMoveSpeedBonus.text = ((EquipmentItem)selectedItem).moveSpeedBonus.ToString();

                        //int weight = ((EquipmentItem)selectedItem).weightBonus;
                        //textSuitWeightBonus.text = weight > 0 ? (weight / 1000) + "Kg" : "---";
                        textSuitPrice.text = selectedItem.itemMallPrice.ToString();

                        bool isAlreadyBought = player.customization.IsSuitAlreadyBought(selectedItem.name);
                        buttonWearASuit.gameObject.SetActive(isAlreadyBought && selectedIndex != player.customization.values[player.customization.FindTypeIndexByType(EquipmentItemType.Suit)].defaultValue);
                        //panelSuitPrice.SetActive(!isAlreadyBought);

                        buttonBuySuit.gameObject.SetActive(!isAlreadyBought);
                        buttonBuySuit.onClick.SetListener(() =>
                        {
                            sound.Play();

                            if (player.itemMall.coins >= selectedItem.itemMallPrice)
                            {
                                player.itemMall.CmdBuyItem(shopType, selectedIndex, 1);
                                panelViewSuit.SetActive(false);
                                paneAllTypes.SetActive(true);
                            }
                            else
                            {

                            }
                        });

                        buttonCloseSuitPanel.onClick.SetListener(() =>
                        {
                            sound.Play();
                            shopType = ShopType.Equipment;
                            if (playerPreview != null) Destroy(playerPreview);
                        });

                        buttonPreviousSuit.onClick.SetListener(() =>
                        {
                            sound.Play();

                            if (selectedIndex > 0) selectedIndex -= 1;
                            else selectedIndex = (sbyte)(items.Length - 1);

                            int suitIndex = player.itemMall.GetShopTypeIndex(ShopType.Suits);
                            selectedItem = player.itemMall.config.categories[suitIndex].items[selectedIndex];

                            InstantiatePrefab(player);
                        });

                        buttonNextSuit.onClick.SetListener(() =>
                        {
                            sound.Play();

                            if (selectedIndex < items.Length - 1) selectedIndex += 1;
                            else selectedIndex = 0;

                            int suitIndex = player.itemMall.GetShopTypeIndex(ShopType.Suits);
                            selectedItem = player.itemMall.config.categories[suitIndex].items[selectedIndex];

                            InstantiatePrefab(player);
                        });

                        buttonWearASuit.onClick.SetListener(() =>
                        {
                            sound.Play();
                            player.customization.CmdWearSuit(selectedIndex);
                        });

                        Player pl = playerPreview.GetComponent<Player>();
                        changeSuitColor.slider.maxValue = pl.customization.materials.Length - 1;
                        int typeIndex = pl.customization.FindTypeIndexByType(EquipmentItemType.Suit);
                        changeSuitColor.slider.value = pl.customization.localvalues[typeIndex].materialValue;

                        changeSuitColor.buttonLeft.onClick.SetListener(() =>
                        {
                            sound.Play();
                            if (changeSuitColor.slider.value > 0)
                            {
                                pl.customization.SetColorForSuitLocal(EquipmentItemType.Suit, (byte)(changeSuitColor.slider.value - 1));
                            }
                        });
                        changeSuitColor.buttonRight.onClick.SetListener(() =>
                        {
                            sound.Play();
                            if (changeSuitColor.slider.value < 3)
                            {
                                pl.customization.SetColorForSuitLocal(EquipmentItemType.Suit, (byte)(changeSuitColor.slider.value + 1));
                            }
                        });

                        buttonChangeColor.onClick.SetListener(() =>
                        {
                            sound.Play();

                            player.customization.CmdSetColorForSuit(EquipmentItemType.Suit, (byte)(changeSuitColor.slider.value));
                        });

                        if (Input.GetMouseButton(0))
                        {
                            applyForce = true;
                            rotY = Input.GetAxis("Mouse X") * strengh;
                        }
                        else applyForce = false;
                    }
                    else if (shopType == ShopType.Bundles)
                    {
                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(bundlePrefab.gameObject, config.bundles.Length, panelBundles.transform);

                        for (int i = 0; i < config.bundles.Length; ++i)
                        {
                            UIBundleSlot bundleSlot = panelBundles.transform.GetChild(i).GetComponent<UIBundleSlot>();
                            bundleSlot.textBundleName.text = Localization.Translate(config.bundles[i].name);

                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(itemPrefab.gameObject, config.bundles[i].items.Length, bundleSlot.content);

                            for (int x = 0; x < config.bundles[i].items.Length; ++x)
                            {
                                UniversalSlot itemSlot = bundleSlot.content.transform.GetChild(x).GetComponent<UniversalSlot>();

                                // refresh valid item
                                itemSlot.tooltip.enabled = shopType != ShopType.Ammo;
                                // only build tooltip while it's actually shown. this
                                // avoids MASSIVE amounts of StringBuilder allocations.
                                if (itemSlot.tooltip.IsVisible())
                                    itemSlot.tooltip.text = new ItemSlot(new Item(config.bundles[i].items[x].item)).ToolTip();

                                itemSlot.image.sprite = config.bundles[i].items[x].item.image;
                                itemSlot.image.color = Color.white;
                                itemSlot.amountOverlay.SetActive(config.bundles[i].items[x].amount > 1);
                                itemSlot.amountText.text = config.bundles[i].items[x].amount.ToString();
                                itemSlot.sliderDurability.gameObject.SetActive(false);
                                itemSlot.imageBinding.gameObject.SetActive(false);
                            }

                            //bundleSlot.textGold.text = Localization.Translate("Gold") + ":  " + config.bundles[i].gold.ToString();
                            bundleSlot.textPrice.text = config.bundles[i].price.ToString();

                            int icopy = i;
                            bundleSlot.button.onClick.SetListener(() =>
                            {
                                sound.Play();
                                if (config.bundles[icopy].price <= player.itemMall.coins)
                                {
                                    player.itemMall.CmdBuyBundle(icopy);
                                }
                                else shopType = ShopType.Currencies;
                            });
                        }
                    }
                    else if (shopType == ShopType.WeaponSkins)
                    {
                        scrollRect.content = weaponSkinContent.GetComponent<RectTransform>();
                        UIUtils.BalancePrefabs(weaponSkinPrefab.gameObject, items.Length, weaponSkinContent.transform);

                        for (int i = 0; i < items.Length; ++i)
                        {
                            UIShopSlot slot = weaponSkinContent.transform.GetChild(i).GetComponent<UIShopSlot>();
                            ScriptableItem item = items[i];
                            slot.textDecription.text = Localization.Translate(item.name).ToString();
                            slot.imageLogo.sprite = item.image;
                            slot.textPrice.text = item.itemMallPrice.ToString();

                            //if (player.weaponSkins.IsSkinAlreadyPurchased(((ScriptableWeaponSkin)item).weapon.name, item.name) == false)
                            //    slot.buttonBuy.gameObject.GetComponentInChildren<Text>().text = Localization.Translate("Buy");
                            //else
                            //{
                            //    int weaponindex = player.weaponSkins.GetSelectedSkinIndex(((ScriptableWeaponSkin)item).weapon.name);
                            //    if (weaponindex != -1 && player.weaponSkins.selectedskins[weaponindex].skin == item.name)
                            //        slot.buttonBuy.gameObject.GetComponentInChildren<Text>().text = Localization.Translate("Clear");
                            //    else
                            //        slot.buttonBuy.gameObject.GetComponentInChildren<Text>().text = Localization.Translate("Use");
                            //}

                            //int icopy = i;
                            //slot.buttonBuy.onClick.SetListener(() =>
                            //{
                            //    sound.Play();

                            //    if (item is ScriptableWeaponSkin weaponskin)
                            //    {
                            //        if (player.weaponSkins.IsSkinAlreadyPurchased(weaponskin.weapon.name, item.name))
                            //            player.weaponSkins.CmdUseSkin(weaponskin.weapon.name, item.name);
                            //        else
                            //        {
                            //            if (item.itemMallPrice <= player.itemMall.coins)
                            //            {
                            //                requestedItem.item = item;
                            //                requestedItem.amount = 1;
                            //                player.itemMall.CmdBuyWeaponSkin(icopy);
                            //            }
                            //            else shopType = ShopType.Currencies;
                            //        }
                            //    }
                            //});
                        }
                    }
                    else if (shopType == ShopType.Currencies)
                    {
                        for (sbyte i = 0; i < player.itemMall.sellingCoins.Count; ++i)
                        {
                            UIShopSlot slot = coinsContainer.GetChild(i).GetComponent<UIShopSlot>();

                            slot.textPrice.text = player.itemMall.sellingCoins[i].price.ToString();

                            sbyte icopy = i;
                            slot.buttonBuy.onClick.SetListener(() =>
                            {
                                sound.Play();
                                player.itemMall.CmdGetLinkForBuyCoinsFromVk(player.itemMall.sellingCoins[icopy].id);
                            });
                        }
                    }

                    if (panelAmount.activeSelf)
                    {
                        textItemName.text = Localization.Translate(requestedItem.item.name);
                        requestedItem.amount = inputFieldAmount.text.ToUshort();
                        uint coinsRequared = (requestedItem.amount * requestedItem.item.itemMallPrice);

                        if (requestedItem.amount < 1)
                        {
                            requestedItem.amount = 1;
                            inputFieldAmount.text = requestedItem.amount.ToString();
                        }
                        else if (coinsRequared > player.itemMall.coins)
                        {
                            requestedItem.amount = (ushort)(player.itemMall.coins / requestedItem.item.itemMallPrice);
                            inputFieldAmount.text = requestedItem.amount.ToString();
                        }

                        int categoryIndex = player.itemMall.GetShopTypeIndex(shopType);
                        imageSlotInPanelSelectAmount.sprite = player.itemMall.config.categories[categoryIndex].items[selectedIndex].image;

                        buttonCompleteThePurchase.onClick.SetListener(() =>
                        {
                            requestedItem.item = selectedItem;
                            player.itemMall.CmdBuyItem(shopType, selectedIndex, requestedItem.amount);
                            panelAmount.SetActive(false);
                        });
                    }
                }
                else panel.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (panel.activeSelf && panelViewSuit.activeSelf)
            {
                if (playerPreview != null && applyForce) playerPreview.transform.Rotate(0, -rotY, 0);
            }
        }

        private void InstantiatePrefab(Player localplayer)
        {
            if (playerPreview != null) Destroy(playerPreview);

            // instantiate the prefab
            playerPreview = Instantiate(manager.playerClasses.Find(p => p.name == localplayer.className), prefabPosition.transform.position, prefabPosition.transform.rotation);
            if (playerPreview != null)
            {
                playerPreview.tag = "Untagged";
                playerPreview.name = "";

                ExtensionsExtended.SetLayerRecursively(playerPreview, 23);

                Player local = playerPreview.GetComponent<Player>();
                local.health.current = 100;
                local.respawning.enabled = false;
                for (int i = 0; i < localplayer.customization.values.Count; i++)
                {
                    local.customization.localvalues.Add(localplayer.customization.values[i]);
                }

                for (int i = 0; i < local.equipment.slotInfo.Length; ++i)
                    local.equipment.slots.Add(new ItemSlot());

                //setup customization
                local.customization.SetupForCustomizationInGame(local, selectedIndex);

            }
        }

        public void ShowConfirmation(bool result)
        {
            if (result)
            {
                buyConfirmation.panel.SetActive(true);
                buyConfirmation.text.text = Localization.Translate("YouBuy") + ": " + Localization.Translate(requestedItem.item.name);
                buyConfirmation.slot.image.sprite = selectedItem.image;
                buyConfirmation.slot.amountOverlay.SetActive(requestedItem.amount > 1);
                buyConfirmation.slot.amountText.text = requestedItem.amount.ToString();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < descriptions.Length; ++i)
            {
                if (descriptions[i].name != descriptions[i].shopType.ToString()) descriptions[i].name = descriptions[i].shopType.ToString();

                for (int x = 0; x < descriptions[i].languages.Length; x++)
                {
                    descriptions[i].languages[x].text = descriptions[i].languages[x].language.ToString();
                }
            }
        }
#endif
    }
}