using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UICraftingExtended : MonoBehaviour
    {
        [Serializable]
        public class RecipesListType
        {
            public string name;
            public CraftingRecipeExtended[] recipes;
        }

        public KeyCode hotKey = KeyCode.C;

        [Header("Add all the recipes that you use in your project")]
        public RecipesListType[] ListRecipes = new RecipesListType[] { };

        [Header("Settings : colors")]
        public Color colorSelectedRecipe = Color.yellow;
        public Color colorLearnedRecipe = Color.gray;
        public Color colorNotLearnedRecipe = Color.red;
        public Color colorMissingItem = Color.red;
        public Color colorError = Color.red;
        public Color colorSucces = Color.green;

        [Header("Settings : sounds")]
        public SoundsSystem soundSystem;
        public AudioSource audioSource;
        public AudioClip soundSuccessfully;
        public AudioClip soundFailed;
        public AudioClip soundCrafting;
        public AudioClip soundOpenRecipes;

        [Header("Settings : Text Info")]
        public float infoTime = 3;
        private double infoTimeEnd;

        [Header("UI Elements")]
        public GameObject panel;
        public Button buttonOpen;
        public Transform ingredientContent;
        public InputField[] InputFieldsAmount;
        public Button buttonCraft;
        public Button buttonStopCraft;
        public Image imageCraftProgress;
        public Toggle toggleAuto;
        public Toggle toggleGold;
        public Toggle toggleRecycle;

        [Header("Panel Gold")]
        public GameObject panelGold;
        public Text textGold;
        public Button buttonCraftOk;
        public Button buttonCraftCancel;

        [Header("Panel recipes")]
        public Button buttonRecipes;
        public GameObject panelRecipes;
        public Transform content;
        public GameObject recipePrefab;
        public Dropdown dropdownRecipes;

        [Header("Panel Info")]
        public GameObject panelInfo;
        public TextMeshProUGUI textInfo;

        [Header("Panel Skills")]
        public GameObject panelSkills;
        public Text[] textSkillLv;
        public Text[] textSkillExp;

        private CraftingRecipeExtended selectedRecipe;
        private List<CraftingRecipeExtended> availableRecipes = new List<CraftingRecipeExtended>();

        public static UICraftingExtended singleton;
        public UICraftingExtended()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        private void Start()
        {
            //fill dropdown all available recipes
            dropdownRecipes.ClearOptions();
            List<string> names = new List<string>();

            for (int i = 0; i < ListRecipes.Length; i++)
                if (ListRecipes[i] != null) names.Add(Localization.Translate(ListRecipes[i].name));

            dropdownRecipes.AddOptions(names);

            buttonOpen.onClick.SetListener(() =>
            {
                if (panel.activeSelf) panel.SetActive(false);
                else panel.SetActive(true);
            });

            //button open all recipes
            buttonRecipes.onClick.AddListener(() =>
            {
                panelRecipes.SetActive(!panelRecipes.activeSelf);
                audioSource.PlayOneShot(soundOpenRecipes);
            });
        }

        public void Open(Player player)
        {
            //restore all panel and button values to default
            player.craftingExtended.craftingState = CraftingState.None;
            for (int i = 0; i < InputFieldsAmount.Length; i++) InputFieldsAmount[i].text = "1";

            panelRecipes.SetActive(false);
            toggleAuto.isOn = false;
            toggleGold.isOn = false;
            toggleRecycle.isOn = false;
            selectedRecipe = null;
            panel.SetActive(true);
        }

        public void Close()
        {
            Player player = Player.localPlayer;
            if (player) player.craftingExtended.CmdClearAllIndices();
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                // hotkey (not while typing in chat, etc.)
                if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                {
                    if (panel.activeSelf) Close();
                    else Open(player);
                }

                // only update if the panel active
                if (panel.activeSelf)
                {
                    //buttonStopCraft.gameObject.SetActive(player.state == "CRAFTING" && player.craftingState == CraftingState.InProgress && toggleAuto.isOn);
                    //buttonStopCraft.onClick.SetListener(() =>
                    //{
                    //    toggleAuto.isOn = false;
                    //});

                    // refresh all Ingredients items
                    for (int i = 0; i < player.craftingExtended.craftingIndices.Count; ++i)
                    {
                        UniversalSlot slot = ingredientContent.GetChild(i).GetChild(0).GetComponent<UniversalSlot>();
                        ItemSlot itemSlot = new ItemSlot();

                        slot.button.interactable = player.craftingExtended.craftingState != CraftingState.InProgress;

                        //if recipe selected from book
                        if (selectedRecipe != null && selectedRecipe._ingredients.Length > i)
                        {
                            if (!selectedRecipe.disassembleToResources)
                            {
                                int icopy = i;
                                slot.button.interactable = true;
                                slot.button.onClick.SetListener(() =>
                                {
                                    player.craftingExtended.CmdClearСraftingIndex(icopy);
                                    selectedRecipe = null;
                                });

                                int inventoryIndex = -1;
                                for (int x = 0; x < selectedRecipe._ingredients[i].items.Length; x++)
                                {
                                    inventoryIndex = player.inventory.GetItemIndexByName(selectedRecipe._ingredients[i].items[x].item.name);
                                    if (inventoryIndex != -1)
                                    {
                                        break;
                                    }
                                }

                                if (inventoryIndex != -1 && player.inventory.slots[inventoryIndex].amount >= selectedRecipe._ingredients[i].items[0].amount)
                                {
                                    itemSlot = player.inventory.slots[inventoryIndex];

                                    // refresh valid item
                                    //slot.tooltip.enabled = true;
                                    //slot.tooltip.text = itemSlot.ToolTip();

                                    slot.tooltipExtended.enabled = true;
                                    if (slot.tooltipExtended.IsVisible())
                                        slot.tooltipExtended.slot = itemSlot;

                                    slot.dragAndDropable.dragable = true;
                                    slot.image.color = Color.white;
                                    slot.image.sprite = itemSlot.item.image;

                                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                                    slot.amountText.text = itemSlot.amount.ToString();
                                    InputFieldsAmount[i].gameObject.SetActive(itemSlot.item.maxStack > 1 && player.craftingExtended.craftingState != CraftingState.InProgress);
                                    InputFieldsAmount[i].text = selectedRecipe._ingredients[i].items[0].amount.ToString();
                                }
                                else
                                {
                                    //slot.tooltip.enabled = true;
                                    //slot.tooltip.text = new ItemSlot(new Item(selectedRecipe._ingredients[i].items[0].item)).ToolTip();

                                    slot.tooltipExtended.enabled = true;
                                    slot.tooltipExtended.slot = new ItemSlot(new Item(selectedRecipe._ingredients[i].items[0].item));

                                    slot.dragAndDropable.dragable = false;
                                    slot.image.color = colorMissingItem;
                                    slot.image.sprite = selectedRecipe._ingredients[i].items[0].item.image;

                                    slot.amountOverlay.SetActive(true);
                                    slot.amountText.text = inventoryIndex == -1 ? "0" : player.inventory.slots[inventoryIndex].amount.ToString();
                                    InputFieldsAmount[i].gameObject.SetActive(true);
                                    InputFieldsAmount[i].text = selectedRecipe._ingredients[i].items[0].amount.ToString();

                                    slot.upgradeText.text = "";
                                }
                            }
                            else
                            {
                                slot.button.interactable = false;

                                slot.tooltip.enabled = true;
                                slot.tooltip.text = new ItemSlot(new Item(selectedRecipe._ingredients[i].items[0].item)).ToolTip();

                                slot.dragAndDropable.dragable = false;
                                slot.image.color = Color.white;
                                slot.image.sprite = selectedRecipe._ingredients[i].items[0].item.image;

                                slot.amountOverlay.SetActive(true);
                                slot.amountText.text = "";
                                InputFieldsAmount[i].gameObject.SetActive(true);
                                InputFieldsAmount[i].text = selectedRecipe._ingredients[i].items[0].amount.ToString();

                                slot.upgradeText.text = "";
                            }
                        }

                        else if (player.craftingExtended.craftingIndices[i] != -1 &&
                            player.craftingExtended.craftingIndices[i] < player.inventory.slots.Count &&
                            player.inventory.slots[player.craftingExtended.craftingIndices[i]].amount > 0)
                        {
                            itemSlot = player.inventory.slots[player.craftingExtended.craftingIndices[i]];

                            int icopy = i;
                            slot.button.onClick.SetListener(() =>
                            {
                                player.craftingExtended.CmdClearСraftingIndex(icopy);
                                selectedRecipe = null;
                            });

                            // refresh valid item
                            slot.tooltip.enabled = true;
                            slot.tooltip.text = itemSlot.ToolTip();

                            slot.dragAndDropable.dragable = true;
                            slot.image.color = Color.white;
                            slot.image.sprite = itemSlot.item.image;

                            slot.amountOverlay.SetActive(itemSlot.amount > 1);
                            slot.amountText.text = itemSlot.amount.ToString();
                            InputFieldsAmount[i].gameObject.SetActive(itemSlot.item.maxStack > 1 && player.craftingExtended.craftingState != CraftingState.InProgress);
                        }
                        else
                        {
                            // refresh invalid item
                            slot.button.onClick.RemoveAllListeners();
                            slot.tooltip.enabled = false;
                            slot.tooltipExtended.enabled = false;
                            slot.dragAndDropable.dragable = false;
                            slot.image.color = Color.clear;
                            slot.image.sprite = null;
                            slot.amountOverlay.SetActive(false);
                            slot.upgradeText.text = "";

                            InputFieldsAmount[i].gameObject.SetActive(false);
                        }

                        // addon system hooks (Item rarity, Upgrade)
                        //UtilsExtended.InvokeMany(typeof(UICraftingExtended), this, "UpdateItemSlot_", player, slot, itemSlot);
                    }

                    //button crafting start
                    buttonCraft.interactable = player.craftingExtended.craftingState != CraftingState.InProgress;
                    buttonCraft.onClick.SetListener(() =>
                    {
                        StartCraft(player);
                    });

                    //toggles
                    toggleGold.interactable = player.craftingExtended.craftingState != CraftingState.InProgress;
                    toggleRecycle.interactable = player.craftingExtended.craftingState != CraftingState.InProgress;

                    //buttons in panelGold
                    buttonCraftOk.onClick.SetListener(() =>
                    {
                        StartCraft(player);

                        panelGold.SetActive(false);
                    });
                    buttonCraftCancel.onClick.SetListener(() =>
                    {
                        player.craftingExtended.craftingState = CraftingState.None;
                        panelGold.SetActive(false);
                    });

                    // show progress bar while crafting
                    // (show 100% if craft time = 0 because it's just better feedback)
                    if (player.craftingExtended.craftingState == CraftingState.InProgress)
                    {
                        imageCraftProgress.gameObject.SetActive(true);
                        imageCraftProgress.fillAmount = player.craftingExtended.CraftingTimeRemaining() > 0 ? ((float)(player.craftingExtended.craftingTime - player.craftingExtended.CraftingTimeRemaining()) / player.craftingExtended.craftingTime) : 0;

                        PlaySoundCrafting();
                    }
                    else
                    {
                        imageCraftProgress.gameObject.SetActive(false);
                    }

                    //clear info text
                    if (NetworkTime.time >= infoTimeEnd)
                    {
                        panelInfo.SetActive(false);
                        textInfo.text = "";
                    }

                    //if open book with recipes
                    if (panelRecipes.activeSelf)
                    {
                        availableRecipes.Clear();

                        for (int i = 0; i < ListRecipes[dropdownRecipes.value].recipes.Length; i++)
                        {
                            CraftingRecipeExtended recipe = ListRecipes[dropdownRecipes.value].recipes[i];
                            if (recipe != null)
                            {
                                if (!player.craftingExtended.useLearnedRecipes || player.craftingExtended.CheckRecipes(recipe))
                                    availableRecipes.Add(recipe);
                            }
                        }

                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(recipePrefab.gameObject, availableRecipes.Count, content);

                        // refresh valid recipes
                        for (int i = 0; i < availableRecipes.Count; ++i)
                        {
                            UIRecipePrefab slot = content.GetChild(i).GetComponent<UIRecipePrefab>();

                            slot.recipeName.text = Localization.Translate(availableRecipes[i].name);
                            slot.slot.image.sprite = availableRecipes[i].resultItems[0].image;
                            slot.slot.image.color = Color.white;

                            //slot.toolTip.enabled = true;
                            //if (slot.toolTip.IsVisible())
                            //    slot.toolTip.text = availableRecipes[i].resultItems[0].ToolTip();

                            slot.slot.button.interactable = false;
                            slot.slot.tooltipExtended.enabled = true;
                            if (slot.slot.tooltipExtended.IsVisible())
                                slot.slot.tooltipExtended.slot = new ItemSlot(new Item(availableRecipes[i].resultItems[0]));

                            //change colors to recipes
                            if (selectedRecipe != null && selectedRecipe.name == availableRecipes[i].name)
                                content.GetChild(i).transform.GetComponent<UIRecipePrefab>().recipeName.color = colorSelectedRecipe;
                            else content.GetChild(i).transform.GetComponent<UIRecipePrefab>().recipeName.color = colorLearnedRecipe;

                            if (!player.craftingExtended.useLearnedRecipes || player.craftingExtended.CheckRecipes(availableRecipes[i]))
                            {
                                int icopy = i; // needed for lambdas, otherwise i is Count
                                slot.button.interactable = player.craftingExtended.craftingState != CraftingState.InProgress;
                                slot.button.onClick.SetListener(() =>
                                {
                                    audioSource.PlayOneShot(soundOpenRecipes);
                                    selectedRecipe = availableRecipes[icopy];
                                    player.craftingExtended.CmdSetRecipeFromBook(availableRecipes[icopy].name);
                                    toggleRecycle.isOn = availableRecipes[icopy].disassembleToResources;

                                    //check skill
                                    int skillIndex = player.craftingExtended.FindSkillIndex(availableRecipes[icopy].skillAndExp.skill.name);
                                    if (skillIndex != -1 && player.craftingExtended.skills[skillIndex].level < availableRecipes[icopy].skillAndExp.requiredLevel)
                                    {
                                        textInfo.color = colorError;

                                        textInfo.text = Localization.Translate("Skill Level Required") + ": " + availableRecipes[icopy].skillAndExp.requiredLevel;
                                        infoTimeEnd = NetworkTime.time + 10;
                                        panelInfo.SetActive(true);
                                    }
                                    else
                                    {
                                        panelInfo.SetActive(false);
                                    }
                                });

                                // addon system hooks (Item rarity)
                                //UtilsExtended.InvokeMany(typeof(UICraftingExtended), this, "UpdateItem_", player, slot.slot, availableRecipes[i].r);
                            }
                            else
                            {
                                slot.GetComponent<Image>().color = colorNotLearnedRecipe;
                            }
                        }
                    }

                    //show craft skills
                    for (int i = 0; i < player.craftingExtended.skills.Count; i++)
                    {
                        textSkillLv[i].text = Localization.Translate("lv") + ": " + player.craftingExtended.skills[i].level;
                        textSkillExp[i].text = Localization.Translate("epx")+ ": " + player.craftingExtended.skills[i].GetPercent().ToString("F2") + "%";
                    }
                }
            }
            else panel.SetActive(false);
        }

        private void StartCraft(Player player)
        {
            ushort[] amounts = new ushort[] { 0, 0, 0, 0, 0 };
            for (int i = 0; i < InputFieldsAmount.Length; i++)
            {
                if (player.craftingExtended.craftingIndices[i] != -1)
                    amounts[i] = ushort.Parse(InputFieldsAmount[i].text);
            }

            player.craftingExtended.StartCraft(amounts, toggleRecycle.isOn, toggleGold.isOn);
        }

        public void ShowInfo(Player player, bool failed, bool continueCrafting, CraftingErrors message)
        {
            //play sound
            if (failed)
            {
                PlaySoundFailed();
                textInfo.color = colorError;
            }
            else
            {
                PlaySoundSuccess();
                textInfo.color = colorSucces;
            }

            textInfo.text = Localization.Translate(message.ToString());
            infoTimeEnd = NetworkTime.time + infoTime;
            panelInfo.SetActive(true);

            //auto continuation ?
            if (toggleAuto.isOn && continueCrafting)
            {
                StartCraft(player);
            }
            else toggleAuto.isOn = false;
        }

        public void OpenPanelGold(long gold)
        {
            panelGold.SetActive(true);
            textGold.text = gold + " :" + Localization.Translate("Gold");
        }

        //sounds
        private void PlaySoundSuccess()
        {
            if (soundSystem == SoundsSystem.viaIndividualSounds) audioSource.PlayOneShot(soundSuccessfully);
            else if (soundSystem == SoundsSystem.viaAddonMenu)
            {
                // addon system hooks (Menu Sounds)
                UtilsExtended.InvokeMany(typeof(UICraftingExtended), this, "PlaySoundUI_", soundSuccessfully);
            }
        }
        private void PlaySoundFailed()
        {
            if (soundSystem == SoundsSystem.viaIndividualSounds) audioSource.PlayOneShot(soundFailed);
            else if (soundSystem == SoundsSystem.viaAddonMenu)
            {
                // addon system hooks (Menu Sounds)
                UtilsExtended.InvokeMany(typeof(UICraftingExtended), this, "PlaySoundUI_", soundFailed);
            }
        }
        private void PlaySoundCrafting()
        {
            if (soundSystem == SoundsSystem.viaIndividualSounds && !audioSource.isPlaying) audioSource.PlayOneShot(soundCrafting);
            else if (soundSystem == SoundsSystem.viaAddonMenu)
            {
                // addon system hooks (Menu Sounds)
                UtilsExtended.InvokeMany(typeof(UICraftingExtended), this, "PlaySoundUI_", soundCrafting);
            }
        }
    }
}


