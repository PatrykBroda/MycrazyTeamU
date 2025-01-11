using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIFriends : MonoBehaviour
    {
        [SerializeField] private Button buttonOpenFriends;
        [SerializeField] private GameObject imageNewFriends;
        [SerializeField] private GameObject panelFriends;
        [SerializeField] private UIFriendSlot slotPrefab;
        [SerializeField] private Transform memberContent;

        [SerializeField] private InputField inputFieldFindFriend;
        [SerializeField] private Button buttonAddFriend;
        [SerializeField] private Button buttonRequests;
        [SerializeField] private Text textInButtonRequests;
        [SerializeField] private Image imageRequestsInfo;

        [Header("UI Elements : Down Panel info")]
        [SerializeField] private UpdatedDisableAfter panelDownInfo;
        [SerializeField] private Text textDownInfo;

        [Header("UI Elements : Panel info")]
        [SerializeField] private GameObject panelInfo;
        [SerializeField] private Text textInfoValue;
        [SerializeField] private Button buttonInfoAply;

        [Header("Settings : colors")]
        [SerializeField] private Color onlineColor = Color.cyan;
        [SerializeField] private Color offlineColor = Color.gray;

        private enum FriendsOrRequests { friends, requests }
        private FriendsOrRequests showfriendsOrRequests = FriendsOrRequests.friends;

        private List<int> names = new List<int>();

        //singleton
        public static UIFriends singleton;
        public UIFriends() { singleton = this; }

        public void Show()
        {
            panelFriends.SetActive(true);
        }

        public void Hide()
        {
            panelFriends.SetActive(false);
        }

        private void Start()
        {
            buttonOpenFriends.onClick.SetListener(() =>
            {
                if (panelFriends.activeSelf) panelFriends.SetActive(false);
                else panelFriends.SetActive(true);
            });

            buttonRequests.onClick.SetListener(() =>
            {
                if (showfriendsOrRequests == FriendsOrRequests.friends)
                {
                    showfriendsOrRequests = FriendsOrRequests.requests;
                    textInButtonRequests.text = Localization.Translate("Friends");
                }
                else
                {
                    showfriendsOrRequests = FriendsOrRequests.friends;
                    textInButtonRequests.text = Localization.Translate("Requests");
                }
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                imageNewFriends.SetActive(player.friends.friendRequests.Count > 0);

                if (panelFriends.activeSelf)
                {
                    if (showfriendsOrRequests == FriendsOrRequests.requests && player.friends.friendRequests.Count == 0) showfriendsOrRequests = FriendsOrRequests.friends;
                    buttonRequests.gameObject.SetActive(player.friends.friendRequests.Count > 0);

                    if (showfriendsOrRequests == FriendsOrRequests.friends)
                    {
                        imageRequestsInfo.gameObject.SetActive(player.friends.friendRequests.Count > 0);

                        if (string.IsNullOrEmpty(inputFieldFindFriend.text))
                        {
                            //disable button add friend
                            buttonAddFriend.interactable = false;

                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.friends.friends.Count, memberContent);

                            // refresh all friends
                            for (int i = 0; i < player.friends.friends.Count; ++i)
                            {
                                int icopy = i;
                                UIFriendSlot slot = memberContent.GetChild(i).GetComponent<UIFriendSlot>();

                                slot.nameText.text = player.friends.friends[i].name;
                                slot.classNameText.text = player.friends.friends[i].className;
                                slot.levelText.text = player.friends.friends[i].level.ToString();
                                slot.guildText.text = player.friends.friends[i].guild;

                                slot.addButton.gameObject.SetActive(false);

                                if (player.friends.friends[i].online)
                                {
                                    slot.onlineStatusImage.color = onlineColor;

                                    //button send a message to a friend (chat)
                                    slot.chatButton.interactable = true;
                                    slot.chatButton.onClick.SetListener(() =>
                                    {
                                        SendMessageInChat(player.friends.friends[icopy].name);
                                    });

                                    //button invite to party
                                    slot.partyButton.interactable = (!player.party.InParty() || !player.party.party.IsFull()) && NetworkTime.time >= player.nextRiskyActionTime;
                                    slot.partyButton.onClick.SetListener(() =>
                                    {
                                        player.party.CmdInvite(player.friends.friends[icopy].name);
                                    });
                                }
                                else
                                {
                                    slot.onlineStatusImage.color = offlineColor;

                                    slot.chatButton.interactable = false;
                                    slot.partyButton.interactable = false;
                                }

                                //if used Mail addon
                                slot.mailButton.onClick.SetListener(() =>
                                {
                                    panelFriends.SetActive(false);

                                    UIMail.singleton.panel.SetActive(true);
                                    UIMail.singleton.panelNewMessage.SetActive(true);
                                    UIMail.singleton.mailNewMessage.inputfieldRecipient.text = player.friends.friends[icopy].name;

                                    // addon system hooks (Mail addon)
                                    //UtilsExtended.InvokeMany(typeof(UIFriends), this, "MailButtonClick_", player, icopy);
                                });

                                slot.kickButton.onClick.SetListener(() => { RemoveFriend(player, icopy); });
                            }
                        }
                        else
                        {
                            //enable button add friend
                            buttonAddFriend.interactable = NetworkTime.time >= player.nextRiskyActionTime;
                            buttonAddFriend.onClick.SetListener(() =>
                            {
                                if (player.friends.CheckFriendByName(inputFieldFindFriend.text) == false)
                                {
                                    player.friends.CmdSendFriendRequest(inputFieldFindFriend.text);

                                    panelDownInfo.Show();
                                    textDownInfo.text = "You sent a friend invite to a player : " + inputFieldFindFriend.text;
                                }
                                else
                                {
                                    panelDownInfo.Show();
                                    textDownInfo.text = "This player is already friends";
                                }
                            });

                            //We are looking for all friends matching the entered text
                            names.Clear();
                            for (int i = 0; i < player.friends.friends.Count; i++)
                                if (player.friends.friends[i].name.Contains(inputFieldFindFriend.text)) names.Add(i);

                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(slotPrefab.gameObject, names.Count, memberContent);

                            // refresh all friends
                            for (int i = 0; i < names.Count; ++i)
                            {
                                UIFriendSlot slot = memberContent.GetChild(i).GetComponent<UIFriendSlot>();
                                int icopy = i;

                                slot.nameText.text = player.friends.friends[names[icopy]].name;
                                slot.classNameText.text = player.friends.friends[names[icopy]].className;
                                slot.levelText.text = player.friends.friends[names[icopy]].level.ToString();
                                slot.guildText.text = player.friends.friends[names[icopy]].guild;

                                //looking for a friend among all the players who are online
                                if (player.friends.friends[names[icopy]].online)
                                {
                                    //Player friend = Player.onlinePlayers[player.friends[names[icopy]]];
                                    slot.onlineStatusImage.color = onlineColor;

                                    //button send a message to a friend (chat)
                                    slot.chatButton.interactable = true;
                                    slot.chatButton.onClick.SetListener(() =>
                                    {
                                        SendMessageInChat(player.friends.friends[names[icopy]].name);
                                    });

                                    //button invite to party
                                    //slot.partyButton.interactable = (!player.party.InParty() || !player.party.party.IsFull()) && NetworkTime.time >= player.nextRiskyActionTime;
                                    //slot.partyButton.onClick.SetListener(() =>
                                    //{
                                    //    player.party.CmdInvite(player.friends.friends[names[icopy]].name);
                                    //});
                                }
                                else
                                {
                                    slot.onlineStatusImage.color = offlineColor;

                                    slot.chatButton.interactable = false;
                                    slot.partyButton.interactable = false;
                                }

                                //if used Mail addon
                                slot.mailButton.onClick.SetListener(() =>
                                {
                                    panelFriends.SetActive(false);

                                    UIMail.singleton.panel.SetActive(true);
                                    UIMail.singleton.panelNewMessage.SetActive(true);
                                    UIMail.singleton.mailNewMessage.inputfieldRecipient.text = player.friends.friends[names[icopy]].name;

                                    // addon system hooks (Mail addon)
                                    //UtilsExtended.InvokeMany(typeof(UIFriends), this, "MailButtonClick_", player, names[icopy]);
                                });

                                slot.kickButton.onClick.SetListener(() => { RemoveFriend(player, names[icopy]); });
                            }
                        }
                    }
                    else
                    {
                        if (player.friends.friendRequests.Count == 0)
                        {
                            showfriendsOrRequests = FriendsOrRequests.requests;
                            textInButtonRequests.text = "Friends";
                        }

                        imageRequestsInfo.gameObject.SetActive(false);

                        // instantiate/destroy enough slots
                        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.friends.friendRequests.Count, memberContent);

                        // refresh all requests
                        for (int i = 0; i < player.friends.friendRequests.Count; ++i)
                        {
                            int icopy = i;
                            UIFriendSlot slot = memberContent.GetChild(i).GetComponent<UIFriendSlot>();

                            slot.nameText.text = player.friends.friendRequests[i].name;
                            slot.classNameText.text = player.friends.friendRequests[i].className;
                            slot.levelText.text = player.friends.friendRequests[i].level.ToString();
                            slot.guildText.text = player.friends.friendRequests[i].guild;

                            slot.addButton.gameObject.SetActive(true);
                            slot.addButton.onClick.SetListener(() => { player.friends.CmdAcceptFriendRequest(player.friends.friendRequests[icopy].name); });

                            slot.kickButton.onClick.SetListener(() => { player.friends.CmdCancelFriendRequest(player.friends.friendRequests[icopy].name); });

                            slot.mailButton.gameObject.SetActive(false);
                            slot.chatButton.gameObject.SetActive(false);
                            slot.partyButton.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else panelFriends.SetActive(false);
        }

        private void RemoveFriend(Player player, int index)
        {
            textInfoValue.text = "Do you want to remove \n" + player.friends.friends[index].name + " from friends ?";
            panelInfo.SetActive(true);

            buttonInfoAply.onClick.SetListener(() => {
                player.friends.CmdFriendKick(player.friends.friends[index].name);
                panelInfo.SetActive(false);
            });
        }

        private void SendMessageInChat(string friend)
        {
            // set text to reply prefix
            UIChat.singleton.messageInput.text = "/w " + friend + " ";

            // activate
            UIChat.singleton.messageInput.Select();

            // move cursor to end (doesn't work in here, needs small delay)
            UIChat.singleton.messageInput.MoveTextEnd(false);
        }

        public void MessageFromTheServer(string message)
        {
            panelDownInfo.Show();
            textDownInfo.text = message;
        }
    }
}