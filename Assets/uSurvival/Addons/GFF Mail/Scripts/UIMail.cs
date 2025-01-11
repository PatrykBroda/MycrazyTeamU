using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIMail : MonoBehaviour
    {
        public int maxMessagesInList = 7;

        [Header("Settings : Colors")]
        public Color colorNotRead = Color.green;
        public Color colorRead = Color.gray;

        [Header("Panel inbox messages")]
        public GameObject panel;
        public GameObject messagePrefab;

        public Toggle toggleAll;
        public GameObject panelInbox;
        public GameObject panelDownInbox;
        //panels amount lists
        public Button buttonLeft;
        public Button buttonRight;
        public Text textListsAmount;
        private int amountLists = 0;
        private int selectLists = 1;
        //up buttons in bottom panel
        public Button buttonNewMessage;
        public Button buttonDeleteInboxMessages;
        public Button buttonView;

        [Header("Panel new message")]
        public UIMailNewMessage mailNewMessage;
        public GameObject panelNewMessage;

        [Header("Panel view message")]
        public UIMailViewMessage mailViewMessage;
        public GameObject panelViewMessage;

        //singleton
        public static UIMail singleton;
        public UIMail()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        private void Start()
        {
            toggleAll.onValueChanged.AddListener(delegate { SelectByToggleAll(); });
        }

        void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    int amount = 0;

                    if (player.mail.mail.Count > maxMessagesInList)
                    {
                        amountLists = player.mail.mail.Count / maxMessagesInList;
                        if (amountLists * maxMessagesInList < player.mail.mail.Count) amountLists = amountLists + 1;

                        textListsAmount.text = selectLists + "/" + amountLists;

                        if (selectLists != amountLists) amount = maxMessagesInList;
                        else amount = player.mail.mail.Count - (maxMessagesInList * (selectLists - 1));
                    }
                    else
                    {
                        textListsAmount.text = "1/1";
                        amount = player.mail.mail.Count;
                    }

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(messagePrefab, amount, panelInbox.transform);

                    // refresh all items
                    for (int i = 0; i < amount; ++i)
                    {
                        UIMailSlot slot = panelInbox.transform.GetChild(i).GetComponent<UIMailSlot>();

                        int index = maxMessagesInList * (selectLists - 1);

                        slot.textSender.text = player.mail.mail[i + index].sender;
                        slot.textSubject.text = player.mail.mail[i + index].subject;

                        slot.id = player.mail.mail[i + index].id;
                        slot.read = player.mail.mail[i + index].read;
                        slot.sender = player.mail.mail[i + index].sender;
                        slot.subject = player.mail.mail[i + index].subject;
                        slot.message = player.mail.mail[i + index].message;

                        slot.itemslot = player.mail.mail[i + index].itemslot;
                        slot.itemTake = player.mail.mail[i + index].itemTake;

                        if (slot.read) slot.textSender.color = slot.textSubject.color = colorRead;
                        else slot.textSender.color = slot.textSubject.color = colorNotRead;

                        int messageIndex = (i + index);
                        slot.Button.onClick.SetListener(() =>
                        {
                            mailViewMessage.selectedMessage = messageIndex;
                            panelViewMessage.SetActive(true);

                            if (!slot.read)
                            {
                                slot.read = true;
                                slot.textSender.color = slot.textSubject.color = colorRead;
                                player.mail.CmdSetMessageAsRead(messageIndex);
                            }
                        });
                    }

                    //buttons lists Amount
                    buttonLeft.onClick.SetListener(() =>
                    {
                        if (selectLists != 1) selectLists = selectLists - 1;
                    });
                    buttonRight.onClick.SetListener(() =>
                    {
                        if (selectLists < amountLists) selectLists = selectLists + 1;
                    });

                    //buttons bottom
                    buttonNewMessage.onClick.SetListener(() =>
                    {
                        panelNewMessage.SetActive(true);

                        mailNewMessage.inputfieldRecipient.text = "";
                        mailNewMessage.inputfieldSubject.text = "";
                        mailNewMessage.inputfieldMessage.text = "";
                        mailNewMessage.inputfieldGold.text = "0";
                        mailNewMessage.inputfieldCoins.text = "0";
                    });
                    buttonDeleteInboxMessages.onClick.SetListener(() =>
                    {
                        int[] temp = new int[maxMessagesInList];
                        if (panelInbox.activeSelf)
                        {
                            for (int i = 0; i < panelInbox.transform.childCount; ++i)
                            {
                                if (panelInbox.transform.GetChild(i).GetComponent<UIMailSlot>().Selected.isOn) temp[i] = panelInbox.transform.GetChild(i).GetComponent<UIMailSlot>().id;
                            }
                        }

                        player.mail.CmdDeleteMessages(temp);

                        toggleAll.isOn = false;
                        SelectByToggleAll();
                    });
                    buttonView.onClick.SetListener(() =>
                    {
                        panelViewMessage.SetActive(true);
                    });
                }
                else panel.SetActive(false);
            }
        }

        private void SelectByToggleAll()
        {
            if (panelInbox.activeSelf)
            {
                for (int i = 0; i < panelInbox.transform.childCount; ++i)
                {
                    panelInbox.transform.GetChild(i).GetComponent<UIMailSlot>().Selected.isOn = toggleAll.isOn;
                }
            }
        }
    }
}


