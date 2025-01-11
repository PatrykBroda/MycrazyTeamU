using GFFAddons;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIMailViewMessage : MonoBehaviour
    {
        public UIMail mail;

        public InputField inputfieldSender;
        public InputField inputfieldSubject;
        public InputField inputfieldMessage;

        public InputField inputfieldGold;
        public InputField inputfieldCoins;

        public UniversalSlot slot;

        public Button buttonTakeGold;
        public Button buttonTakeCoins;

        public Button buttonReply;
        public Button buttonDeleteMessage;

        [HideInInspector] public int selectedMessage;

        void Update()
        {
            Player player = Player.localPlayer;

            Mail message = player.mail.mail[selectedMessage];

            inputfieldSender.text = message.sender;
            inputfieldSubject.text = message.subject;
            inputfieldMessage.text = message.message;

            //gold
            /*buttonTakeGold.interactable = message.goldTake == false && message.gold > 0;
            buttonTakeGold.onClick.SetListener(() =>
            {
                buttonTakeGold.interactable = false;
                player.mail.CmdTakeGold(message.id);
            });
            inputfieldGold.text = message.gold.ToString();*/

            //coins
            /*buttonTakeCoins.interactable = message.coinsTake == false && message.coins > 0;
            buttonTakeCoins.onClick.SetListener(() =>
            {
                buttonTakeCoins.interactable = false;
                player.mail.CmdTakeCoins(message.id);
            });
            inputfieldCoins.text = message.coins.ToString();*/

            //item
            bool state = false; // for item rarity addon

            if (message.itemslot.amount > 0 && !string.IsNullOrEmpty(message.itemslot.item.name))
            {
                state = true;

                //toolTip
                slot.tooltip.enabled = true;
                slot.tooltip.text = message.itemslot.ToolTip();

                slot.dragAndDropable.dragable = true;
                slot.image.color = Color.white;
                slot.image.sprite = message.itemslot.item.image;
                slot.amountOverlay.SetActive(message.itemslot.amount > 1);
                slot.amountText.text = message.itemslot.amount.ToString();

                slot.button.interactable = !message.itemTake;
                slot.button.onClick.SetListener(() => {
                    player.mail.CmdTakeItem(selectedMessage);
                });
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
            UtilsExtended.InvokeMany(typeof(UIMailViewMessage), this, "Update_", player, slot, state ? message.itemslot : new ItemSlot());

            buttonDeleteMessage.onClick.SetListener(() =>
            {
                gameObject.SetActive(false);
                mail.panel.SetActive(true);
                player.mail.CmdDeleteMail(message.id);
            });

            buttonReply.onClick.SetListener(() =>
            {
                gameObject.SetActive(false);
                mail.panelNewMessage.SetActive(true);
                mail.mailNewMessage.inputfieldRecipient.text = inputfieldSender.text;
            });
        }
    }
}


