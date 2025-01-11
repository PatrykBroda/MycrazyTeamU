using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerMail : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        [Header("Settings")]
        public bool disableSendMessagesToMyself;
        public bool allowSendItems = true;

        [Header("Settings : message sending fee")]
        public uint costOfSendingMessageGold = 0;
        public uint costOfSendingMessageCoins = 0;
        public uint costOfSendingItemGold = 0;
        public uint costOfSendingItemCoins = 0;

        [SerializeField] private float timeBetweenChecks = 15;

        public readonly SyncListMail mail = new SyncListMail();
        [HideInInspector] public int mailIndex = -1;

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(CheckNewMailOnServer), 1, timeBetweenChecks);
        }

        private void CheckNewMailOnServer()
        {
            Database.singleton.CharacterLoadMail(player);
        }

        public bool CheckNewMailOnClient()
        {
            for (int i = 0; i < mail.Count; i++)
            {
                if (mail[i].read == false) return true;
            }

            return false;
        }

        [Command]
        public void CmdSendNewMail(string recipient, string subject, string message, int inventoryIndex, ushort amount)
        {
            if (string.IsNullOrEmpty(recipient) == false && (disableSendMessagesToMyself == false || recipient != player.name))
            {
                uint goldCost = costOfSendingMessageGold;
                uint coinsCost = costOfSendingMessageCoins;
                if (inventoryIndex != -1)
                {
                    goldCost += costOfSendingItemGold;
                    coinsCost += costOfSendingItemCoins;
                }

                //check costs Of Sending
                if (player.gold >= goldCost && player.itemMall.coins >= coinsCost)
                {
                    player.gold -= goldCost;
                    player.itemMall.coins -= coinsCost;

                    //item
                    ItemSlot sendingSlot = new ItemSlot();
                    if (allowSendItems && inventoryIndex != -1 && player.inventory.slots[inventoryIndex].amount >= amount && player.inventory.slots[inventoryIndex].item.data.canSendOnMail)
                    {
                        sendingSlot = player.inventory.slots[inventoryIndex];
                        sendingSlot.amount = (ushort)(player.inventory.slots[inventoryIndex].amount - amount);
                        player.inventory.slots[inventoryIndex] = sendingSlot;
                        sendingSlot.amount = amount;
                    }

                    //check if there is a character who sent the letter
                    if (Database.singleton.SendNewMessage(recipient, name, subject, message, false, sendingSlot) == false)
                    {
                        //return the letter
                        Database.singleton.SendNewMessage(name, "System", "Message not delivered", message, false, sendingSlot);
                    }
                }
            }
        }

        [Command]
        public void CmdSetMessageAsRead(int messageIndex)
        {
            Database.singleton.SetMessageRead(mail[messageIndex].id);

            Database.singleton.CharacterLoadMail(player);
        }

        [Command]
        public void CmdTakeItem(int messageIndex)
        {
            if (!mail[messageIndex].itemTake)
            {
                ItemSlot itemslot = mail[messageIndex].itemslot;
                if (player.inventory.CanAdd(itemslot.item, itemslot.amount))
                {
                    player.inventory.Add(itemslot.item, itemslot.amount, false);
                    Database.singleton.SetMessageItemTaken(mail[messageIndex].id);
                    Database.singleton.CharacterLoadMail(player);
                }
                //else player.TargetSendMessageToChat("Inventory Full");
            }
        }

        [Command]
        public void CmdDeleteMail(int messageIndex)
        {
            Database.singleton.DeleteMessage(messageIndex);
            Database.singleton.CharacterLoadMail(player);
        }
        [Command]
        public void CmdDeleteMessages(int[] delete)
        {
            for (int i = 0; i < delete.Length; ++i)
                if (delete[i] > 0) Database.singleton.DeleteMessage(delete[i]);

            Database.singleton.CharacterLoadMail(player);
        }

        void OnDragAndDrop_InventorySlot_MailSlot(int[] slotIndices)
        {
            ItemSlot slot = player.inventory.slots[slotIndices[0]];
            int toIndex = slotIndices[1];

            if (slot.amount > 0 && toIndex == 0 && slot.item.data.canSendOnMail == true)
            {
                if (slot.amount > 1 && slot.item.maxStack > 1)
                {
                    // swap them
                    mailIndex = slotIndices[0];
                    UIMail.singleton.mailNewMessage.panelAmount.SetActive(true);
                    UIMail.singleton.mailNewMessage.inputfieldItemAmount.ActivateInputField();
                }
                else
                {
                    mailIndex = slotIndices[0];
                }
            }
        }
        void OnDragAndDrop_InventoryExtendedSlot_MailSlot(int[] slotIndices)
        {
            ItemSlot slot = player.inventory.slots[slotIndices[0]];
            int toIndex = slotIndices[1];

            if (slot.amount > 0 && toIndex == 0 && slot.item.data.canSendOnMail == true)
            {
                if (slot.amount > 1 && slot.item.maxStack > 1)
                {
                    // swap them
                    mailIndex = slotIndices[0];
                    UIMail.singleton.mailNewMessage.panelAmount.SetActive(true);
                    UIMail.singleton.mailNewMessage.inputfieldItemAmount.ActivateInputField();
                }
                else
                {
                    mailIndex = slotIndices[0];
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            player = gameObject.GetComponent<Player>();
            player.mail = this;
        }
#endif
    }
}