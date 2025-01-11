using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIMailNewMessage : MonoBehaviour
    {
        public UIMail mail;

        [Header("UI Elements : new message")]
        public InputField inputfieldRecipient;
        public InputField inputfieldSubject;
        public InputField inputfieldMessage;

        public InputField inputfieldGold;
        public InputField inputfieldCoins;
        public UniversalSlot slot;
        public Button buttonSend;
        public Button buttonCancel;

        public GameObject panelAmount;
        public InputField inputfieldItemAmount;
        public Button buttonAmountCancel;
        public Button buttonAmountOk;
        private ushort itemAmount = 1;

        [Header("Panel Error")]
        public GameObject panelError;
        public Text textError;

        void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                slot.gameObject.SetActive(player.mail.allowSendItems);

                bool state = false; // for item rarity addon

                //refresh item
                if (player.mail.mailIndex != -1 && player.mail.mailIndex < player.inventory.slots.Count && player.inventory.slots[player.mail.mailIndex].amount > 0)
                {
                    state = true;

                    slot.button.onClick.SetListener(() => { player.mail.mailIndex = -1; });

                    // refresh valid item
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = player.inventory.slots[player.mail.mailIndex].ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = player.inventory.slots[player.mail.mailIndex].item.image;
                    slot.amountOverlay.SetActive(itemAmount > 1);
                    slot.amountText.text = itemAmount.ToString();
                }
                else
                {
                    // refresh invalid item
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }

                // addon system hooks (Item rarity)
                UtilsExtended.InvokeMany(typeof(UIMailNewMessage), this, "Update_", player, slot, state ? player.inventory.slots[player.mail.mailIndex] : new ItemSlot());

                uint goldCost = player.mail.costOfSendingMessageGold;
                uint coinsCost = player.mail.costOfSendingMessageCoins;
                if (player.mail.mailIndex != -1)
                {
                    goldCost += player.mail.costOfSendingItemGold;
                    coinsCost += player.mail.costOfSendingItemCoins;
                }

                inputfieldGold.text = goldCost.ToString();
                inputfieldCoins.text = coinsCost.ToString();

                //buttons
                buttonSend.interactable = inputfieldRecipient.text.Length > 0;
                buttonSend.onClick.SetListener(() => {

                    if (player.mail.disableSendMessagesToMyself == false || inputfieldRecipient.text != player.name)
                    {
                        //check costs Of Sending
                        if (player.gold >= goldCost)
                        {
                            if (player.itemMall.coins >= coinsCost)
                            {
                                //send a letter to the database
                                player.mail.CmdSendNewMail(inputfieldRecipient.text,
                                inputfieldSubject.text,
                                inputfieldMessage.text,
                                player.mail.mailIndex,
                                itemAmount);

                                player.mail.mailIndex = -1;
                                inputfieldGold.text = "";
                                inputfieldCoins.text = "";

                                mail.panel.SetActive(true);
                                gameObject.SetActive(false);
                            }
                            else
                            {
                                textError.text = "Not Enough Coins";
                                panelError.SetActive(true);
                            }
                        }
                        else
                        {
                            textError.text = "Not Enough Money";
                            panelError.SetActive(true);
                        }
                    }
                    else
                    {
                        textError.text = "You can't send mail to yourself";
                        panelError.SetActive(true);
                    }
                });

                buttonCancel.onClick.SetListener(() => {
                    inputfieldRecipient.text = "";
                    inputfieldSubject.text = "";
                    inputfieldMessage.text = "";
                    inputfieldGold.text = "";
                    inputfieldCoins.text = "";
                    player.mail.mailIndex = -1;

                    gameObject.SetActive(false);
                });

                if (panelAmount.activeSelf)
                {
                    if (inputfieldItemAmount.text.ToInt() > player.inventory.slots[player.mail.mailIndex].amount)
                        inputfieldItemAmount.text = player.inventory.slots[player.mail.mailIndex].amount.ToString();

                    buttonAmountCancel.onClick.SetListener(() =>
                    {
                        player.mail.mailIndex = -1;
                        panelAmount.SetActive(false);
                        inputfieldItemAmount.text = "99";
                    });
                    buttonAmountOk.onClick.SetListener(() =>
                    {
                        itemAmount = inputfieldItemAmount.text.ToUshort();
                        panelAmount.SetActive(false);
                        inputfieldItemAmount.text = "99";
                    });
                }
            }
        }
    }
}


