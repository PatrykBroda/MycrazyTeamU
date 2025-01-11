using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System;
using uSurvival;
using TMPro;
using UnityEditor;

namespace GFFAddons
{
    public partial class UIGuild : MonoBehaviour
    {
        //public KeyCode hotKey = KeyCode.G;

        [Header("Settings : Colors")]
        [SerializeField] private Color onlineColor = Color.cyan;
        [SerializeField] private Color offlineColor = Color.gray;
        [SerializeField] private bool useColorsForMembers;
        [SerializeField] private Color masterColor = Color.magenta;
        [SerializeField] private Color officersColor = Color.yellow;
        [SerializeField] private Color memberColor = Color.green;
        [SerializeField] private Color noviceColor = Color.gray;

        [Header("Settings : Addons")]
        [SerializeField] private bool useMenuAddon;

        [Header("UI Elements")]
        [SerializeField] private Button buttonOpen;
        [SerializeField] private Button buttonClose;
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private InputField noticeInput;
        [SerializeField] private Button noticeEditButton;
        [SerializeField] private Button noticeSetButton;
        [SerializeField] private UIGuildMemberSlot slotPrefab;
        [SerializeField] private Transform memberContent;
        [SerializeField] private Dropdown management;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject panelRanksInfo;

        [Header("UI Elements : Gold")]
        [SerializeField] private GameObject panelGold;
        [SerializeField] private TextMeshProUGUI goldValue;

        [Header("UI Elements : Sort")]
        [SerializeField] private Button buttonSortOnline;
        [SerializeField] private Button buttonSortName;
        [SerializeField] private Button buttonSortLevel;
        [SerializeField] private Button buttonSortRank;
        private sort sortOption = sort.byNameAscending;
        public enum sort
        {
            byOnlineAscending,
            byOnlineDescending,
            byNameAscending,
            byNameDescending,
            byLevelAscending,
            byLevelDescending,
            byRankAscending,
            byRankDescending
        };

        [Header("UI Elements : Management")]
        [SerializeField] private GameObject panelManagement;
        [SerializeField] private GameObject panelGuildWars;

        [Header("UI Elements : Change Guild Lider")]
        [SerializeField] private GameObject panelChangeMaster;
        [SerializeField] private Text textPanelName;
        [SerializeField] private Dropdown dropdownChangeMaster;
        [SerializeField] private Text textVotingTime;
        [SerializeField] private Text textVotingProgress;
        [SerializeField] private Button buttonChangeMasterConfirm;
        [SerializeField] private Button buttonChangeMasterRefuse;

        private List<GuildMember> sortedUsers = new List<GuildMember>();
        private List<string> names = new List<string>();
        private List<string> namesNotLocalize = Enum.GetNames(typeof(GuildRank)).ToList();

        private void Start()
        {
            buttonOpen.onClick.SetListener(() => {
                panel.SetActive(!panel.activeSelf);
            });

            buttonClose.onClick.SetListener(() => {
                if (panelRanksInfo.activeSelf) panelRanksInfo.SetActive(false);
                else panel.SetActive(false); 
            });

            //sort
            buttonSortOnline.onClick.SetListener(() =>
            {
                if (sortOption == sort.byOnlineAscending) sortOption = sort.byOnlineDescending;
                else sortOption = sort.byOnlineAscending;
            });
            buttonSortName.onClick.SetListener(() =>
            {
                if (sortOption == sort.byNameAscending) sortOption = sort.byNameDescending;
                else sortOption = sort.byNameAscending;
            });
            buttonSortRank.onClick.SetListener(() =>
            {
                if (sortOption == sort.byRankAscending) sortOption = sort.byRankDescending;
                else sortOption = sort.byRankAscending;
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player)
            {
                buttonOpen.gameObject.SetActive(player.guild.InGuild());

                // hotkey (not while typing in chat, etc.)
                //if (!useMenuAddon && Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                //    panel.SetActive(!panel.activeSelf);

                // only update the panel if it's active
                if (panel.activeSelf)
                {
                    if (player.guild.InGuild())
                    {
                        Guild guild = player.guild.guild;

                        //members
                        int memberCount = guild.members != null ? guild.members.Length : 0;

                        // guild properties
                        nameText.text = guild.name;
                        capacityText.text = memberCount.ToString() + " / " + GuildSystem.Capacity.ToString();

                        noticeInput.interactable = player.guild.CanChangeNotice(player.name) && NetworkTime.time >= player.nextRiskyActionTime;
                        noticeInput.onEndEdit.SetListener(delegate { UpdateNotice(player, noticeInput.text); });
                        if (!noticeInput.isFocused) noticeInput.text = guild.notice ?? "";

                        // notice input: copies notice while not editing it
                        noticeInput.characterLimit = GuildSystem.NoticeMaxLength;

                        management.gameObject.SetActive(player.guild.CanAcceptToGuild());

                        //show members
                        if (management.value == 0)
                        {
                            //sort
                            sortedUsers.Clear();
                            if (sortOption == sort.byOnlineAscending) sortedUsers = guild.members.OrderBy(u => u.online).ToList();
                            else if (sortOption == sort.byOnlineDescending) sortedUsers = guild.members.OrderByDescending(u => u.online).ToList();
                            else if (sortOption == sort.byNameAscending) sortedUsers = guild.members.OrderBy(u => u.name).ToList();
                            else if (sortOption == sort.byNameDescending) sortedUsers = guild.members.OrderByDescending(u => u.name).ToList();
                            else if (sortOption == sort.byLevelAscending) sortedUsers = guild.members.OrderBy(u => u.level).ToList();
                            else if (sortOption == sort.byLevelDescending) sortedUsers = guild.members.OrderByDescending(u => u.level).ToList();
                            else if (sortOption == sort.byRankAscending) sortedUsers = guild.members.OrderBy(u => u.rank).ToList();
                            else sortedUsers = guild.members.OrderByDescending(u => u.rank).ToList();

                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(slotPrefab.gameObject, memberCount, memberContent);                       

                            // refresh all members
                            for (int i = 0; i < memberCount; ++i)
                            {
                                UIGuildMemberSlot slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();
                                GuildMember member = sortedUsers[i];

                                slot.onlineStatusImage.color = member.online ? onlineColor : offlineColor;

                                slot.nameText.text = member.name;
                                if (useColorsForMembers)
                                {
                                    if (member.rank == GuildRank.Master) slot.nameText.color = masterColor;
                                    else if (member.rank == GuildRank.Vice) slot.nameText.color = officersColor;
                                    else if (member.rank == GuildRank.Member) slot.nameText.color = memberColor;
                                    else slot.nameText.color = noviceColor;
                                }

                                //dropdown
                                names.Clear();
                                slot.dropdown.onValueChanged.RemoveAllListeners();
                                for (int x = 0; x < namesNotLocalize.Count; x++)
                                    names.Add(Localization.Translate(namesNotLocalize[x]));

                                if (member.name != guild.master) names.Remove(Localization.Translate(GuildRank.Master.ToString()));

                                slot.dropdown.ClearOptions();
                                slot.dropdown.AddOptions(names);
                                slot.dropdown.value = (int)member.rank;

                                slot.dropdown.interactable = player.guild.CanChangeRank(player.name, member.name);
                                slot.dropdown.onValueChanged.SetListener(delegate
                                {
                                    player.guild.CmdMemberRankUpdate(slot.nameText.text, (byte)slot.dropdown.value);
                                });

                                slot.promoteButton.gameObject.SetActive(false);
                                slot.demoteButton.gameObject.SetActive(false);

                                slot.kickButton.gameObject.SetActive(player.guild.CanKickMember(player.name, member.name));
                                slot.kickButton.onClick.SetListener(() =>
                                {
                                    player.guild.CmdKick(member.name);
                                });

                                slot.partyButton.gameObject.SetActive(member.name != player.name && member.online && NetworkTime.time >= player.nextRiskyActionTime && (!player.party.InParty() || !player.party.party.IsFull()));
                                slot.partyButton.onClick.SetListener(() =>
                                {
                                    player.party.CmdInvite(member.name);
                                });
                            }

                            panelGuildWars.SetActive(false);
                            panelManagement.SetActive(false);
                            panelChangeMaster.SetActive(false);
                        }
                        //show waiting approval
                        else if (management.value == 1)
                        {
                            // instantiate/destroy enough slots
                            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.guild.guild.approvalRequests.Length, memberContent);

                            // refresh all members
                            for (int i = 0; i < player.guild.guild.approvalRequests.Length; ++i)
                            {
                                UIGuildMemberSlot slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();

                                slot.onlineStatusImage.color = Player.onlinePlayers.ContainsKey(player.guild.guild.approvalRequests[i].character) ? onlineColor : offlineColor;

                                slot.nameText.text = player.guild.guild.approvalRequests[i].character;
                                slot.nameText.color = memberColor;

                                //slot.rankText.text = "--";

                                slot.dropdown.gameObject.SetActive(false);

                                slot.promoteButton.gameObject.SetActive(true);
                                slot.promoteButton.interactable = guild.members.Length < GuildSystem.Capacity && player.guild.CanAcceptToGuild();
                                slot.promoteButton.onClick.SetListener(() =>
                                {
                                    player.guild.CmdRequestToJoinGuildAccept(slot.nameText.text);
                                });

                                slot.demoteButton.gameObject.SetActive(true);
                                slot.demoteButton.interactable = player.guild.CanAcceptToGuild();
                                slot.demoteButton.onClick.SetListener(() =>
                                {
                                    player.guild.CmdRequestToJoinTheGuildReject(slot.nameText.text);
                                });

                                slot.kickButton.gameObject.SetActive(false);
                            }

                            panelGuildWars.SetActive(false);
                            panelManagement.SetActive(false);
                            panelChangeMaster.SetActive(false);
                        }
                        //change guild master
                        else if (management.value == 2)
                        {
                            if (player.guild.data.guildMasterChangeAllowed)
                            {
                                //if no one has already started changing the master
                                if (player.guild.data.CheckConditionForChoosingNewMaster(player) && (guild.newMasterName == "" || guild.newMasterName == null))
                                {
                                    textPanelName.text = "Submit a request to change a Guild Master";
                                    textVotingTime.text = "";
                                    textVotingProgress.text = "";
                                    dropdownChangeMaster.gameObject.SetActive(true);
                                    dropdownChangeMaster.ClearOptions();

                                    names.Clear();
                                    for (int i = 0; i < guild.members.Length; i++)
                                    {
                                        if (guild.members[i].rank != GuildRank.Master) names.Add(guild.members[i].name);
                                    }
                                    dropdownChangeMaster.AddOptions(names);

                                    buttonChangeMasterRefuse.interactable = false;
                                    buttonChangeMasterConfirm.onClick.SetListener(() =>
                                    {
                                        buttonChangeMasterConfirm.gameObject.SetActive(false);
                                        GuildSystem.RequestChangeMaster(guild.name, dropdownChangeMaster.captionText.text);
                                    });
                                }
                                else
                                {
                                    dropdownChangeMaster.gameObject.SetActive(false);

                                    textPanelName.text = "Do you agree that the player  " + guild.newMasterName + "\n become the new Guild Master?";
                                    textVotingTime.text = "Voting End Time : " + DateTime.Parse(guild.voteDateEnd).AddMinutes(player.guild.data.guildMasterChangeVotingTime);

                                    //Have we voted already?
                                    int yes = 0;
                                    int no = 0;
                                    for (int i = 0; i < player.guild.guildNewMasterVoting.Count; i++)
                                    {
                                        if (player.guild.guildNewMasterVoting[i].name == player.name)
                                        {
                                            buttonChangeMasterConfirm.interactable = false;
                                            buttonChangeMasterRefuse.interactable = false;
                                        }

                                        if (player.guild.guildNewMasterVoting[i].state == true) yes++;
                                        else no++;
                                    }
                                    textVotingProgress.text = "Current voting result : Yes = " + yes + "/ No = " + no;

                                    //buttons
                                    buttonChangeMasterConfirm.gameObject.SetActive(true);
                                    buttonChangeMasterConfirm.onClick.SetListener(() =>
                                    {
                                        buttonChangeMasterConfirm.interactable = false;
                                        buttonChangeMasterRefuse.interactable = false;

                                        player.guild.CmdNewMasterVote(true);
                                        player.guild.CheckStateVotingNewMaster();
                                    });

                                    buttonChangeMasterRefuse.gameObject.SetActive(true);
                                    buttonChangeMasterRefuse.onClick.SetListener(() =>
                                    {
                                        buttonChangeMasterConfirm.interactable = false;
                                        buttonChangeMasterRefuse.interactable = false;

                                        player.guild.CmdNewMasterVote(false);
                                        player.guild.CheckStateVotingNewMaster();
                                    });
                                }
                            }
                            else
                            {
                                textPanelName.text = "Changing the guild master is prohibited";
                                dropdownChangeMaster.gameObject.SetActive(false);
                                textVotingTime.text = "";
                                textVotingProgress.text = "";
                                buttonChangeMasterConfirm.gameObject.SetActive(false);
                                buttonChangeMasterRefuse.gameObject.SetActive(false);
                            }

                            panelManagement.SetActive(true);
                            panelChangeMaster.SetActive(true);
                        }
                        //show guilds war
                        else if (management.value == 3)
                        {
                            panelGuildWars.SetActive(true);
                            panelManagement.SetActive(false);
                        }

                        //guild storage addon
                        panelGold.SetActive(player.guild.data.useGuildStorage);
                        goldValue.text = player.guild.guild.goldInStorage.ToString();

                        // leave
                        leaveButton.gameObject.SetActive(guild.CanLeave(player.name));
                        leaveButton.onClick.SetListener(() => {
                            player.guild.CmdLeave(); 
                        });
                    }
                    else panel.SetActive(false);
                }
            }
        }

        private void UpdateNotice(Player player, string notice)
        {
            if (noticeInput.text != player.guild.guild.notice)
            {
                player.guild.CmdSetNotice(noticeInput.text);
            }
        }
    }
}