using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;
using System.Collections;

namespace GFFAddons
{
    public partial class UIDailyRewards : MonoBehaviour
    {
        [SerializeField] private KeyCode hotKey = KeyCode.P;

        [SerializeField] private Button buttonDailyRewards;
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentDays;
        [SerializeField] private GameObject dayPrefab;
        [SerializeField] private Transform contentRewards;
        [SerializeField] private GameObject prefab;
        [SerializeField] private TextMeshProUGUI textGoldValue;
        [SerializeField] private TextMeshProUGUI textCoinsValue;
        [SerializeField] private Button buttonClaim;
        [SerializeField] private TextMeshProUGUI textInfo;

        [Header("Colors")]
        [SerializeField] private Color colorSelect;
        [SerializeField] private Color colorAvailable;
        [SerializeField] private Color colorNonSelect;

        [Header("Settings : addons")]
        [SerializeField] private bool useMenuAddon;
        [SerializeField] private bool useToolTipsExtended;

        private int selectedDay = -1;

        //singleton
        public static UIDailyRewards singleton;
        public UIDailyRewards()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        public void Show(int day)
        {
            selectedDay = day;
            panel.SetActive(true);
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                buttonDailyRewards.onClick.SetListener(() =>
                {
                    Show(DateTime.Now.Day > player.dailyRewards.rewards.Count ? player.dailyRewards.rewards.Count - 1 : DateTime.Now.Day - 1);
                });

                //hotKey
                if (!useMenuAddon)
                {
                    // hotkey (not while typing in chat, etc.)
                    if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                    {
                        if (panel.activeSelf) panel.SetActive(false);
                        else Show(DateTime.Now.Day > player.dailyRewards.rewards.Count ? player.dailyRewards.rewards.Count - 1 : DateTime.Now.Day - 1);
                    }
                }

                if (panel.activeSelf)
                {
                    int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(dayPrefab.gameObject, player.dailyRewards.rewards.Count, contentDays);

                    //check all days
                    for (int i = 0; i < player.dailyRewards.rewards.Count; i++)
                    {
                        UIDayPrefabSlot daySlot = contentDays.transform.GetChild(i).GetComponent<UIDayPrefabSlot>();
                        daySlot.textDay.text = player.dailyRewards.rewards[i].day.ToString();

                        //paint border color
                        if (selectedDay == i) daySlot.imageBorder.color = colorSelect;
                        else
                        {
                            if (player.dailyRewards.rewards[i].done && player.dailyRewards.rewards[i].get == false)
                                daySlot.imageBorder.color = colorAvailable;
                            else daySlot.imageBorder.color = colorNonSelect;
                        }

                        //if the award has already been received
                        daySlot.imageGet.gameObject.SetActive(player.dailyRewards.rewards[i].get);

                        //button
                        int icopy = i;
                        daySlot.button.onClick.SetListener(() =>
                        {
                            selectedDay = icopy;
                        });
                    }

                    if (selectedDay != -1)
                    {
                        Rewards rewards = player.dailyRewards.GetRewardsByDay(selectedDay);

                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(prefab.gameObject, rewards.items.Length, contentRewards);

                        if (rewards.items.Length > 0)
                        {
                            for (int x = 0; x < rewards.items.Length; ++x)
                            {
                                UniversalSlot slot = contentRewards.GetChild(x).GetComponent<UniversalSlot>();

                                if (rewards.items[x].item != null)
                                {
                                    slot.dragAndDropable.dragable = false;
                                    slot.image.color = Color.white;
                                    slot.button.interactable = true;
                                    slot.image.sprite = rewards.items[x].item.image;

                                    //amount
                                    ushort amount = rewards.items[x].amount != 0 ? rewards.items[x].amount : (ushort)1;
                                    slot.amountOverlay.SetActive(amount > 1);
                                    slot.amountText.text = amount.ToString();

                                    ItemSlot itemslot = new ItemSlot();
                                    itemslot.item = new Item(rewards.items[x].item);
                                    itemslot.amount = amount;

                                    //use GFF ToolTips ?
                                    if (!useToolTipsExtended)
                                    {
                                        slot.tooltip.enabled = true;
                                        slot.tooltip.text = itemslot.ToolTip();
                                    }
                                    /*else
                                    {
                                        slot.tooltipExtended.enabled = true;
                                        slot.tooltipExtended.slot = itemslot;
                                    }*/

                                    // addon system hooks (Item rarity)
                                    UtilsExtended.InvokeMany(typeof(UIDailyRewards), this, "Update_", slot, itemslot.item.data);
                                }
                            }
                        }

                        textGoldValue.text = rewards.gold.ToString();
                        textCoinsValue.text = rewards.coins.ToString();
                    }

                    //button claim rewards
                    buttonClaim.onClick.SetListener(() =>
                    {
                        if (player.dailyRewards.rewards[selectedDay].done)
                        {
                            if (player.dailyRewards.rewards[selectedDay].get == false)
                            {
                                StartCoroutine(UpdateButtonState());
                                player.dailyRewards.CmdAddReward(selectedDay);
                            }
                            else
                            {
                                textInfo.text = Localization.Translate("The reward has already been received");
                                StartCoroutine(UpdateButtonState());
                            }
                        }
                        else
                        {
                            textInfo.text = Localization.Translate("The time of receipt has not yet come");
                            StartCoroutine(UpdateButtonState());
                        }
                    });
                }
            }
            else panel.SetActive(false);
        }

        public void ShowInfoMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                textInfo.text = message;
                UpdateButtonState();
            }
        }

        private IEnumerator UpdateButtonState()
        {
            buttonClaim.interactable = false;
            yield return new WaitForSeconds(2);
            buttonClaim.interactable = true;
            textInfo.text = "";
        }
    }
}