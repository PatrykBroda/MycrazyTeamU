using Mirror;
using TMPro;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [RequireComponent(typeof(PlayerChat))]
    [DisallowMultipleComponent]
    public class PlayerGuild : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private Player player;
        [SerializeField] private PlayerChat chat;
        public GuildData data;

        [Header("Text Meshes")]
        public TextMeshPro overlay;
        public string overlayPrefix = "[";
        public string overlaySuffix = "]";

        // .guild is a copy for easier reading/syncing. Use GuildSystem to manage guilds!
        [SyncVar, HideInInspector] public Guild guild; // TODO SyncToOwner later but need to sync guild name to everyone!

        [SyncVar, HideInInspector] public string inviteFrom = "";
        [SyncVar, HideInInspector] public string inviteGuild = "";
        public float inviteWaitSeconds = 3;

        [SyncVar, HideInInspector] public string guildWaiting = "";

        [HideInInspector] public readonly SyncListGuildInfo allGuilds = new SyncListGuildInfo();
        [HideInInspector] public readonly SyncListNewMasterVoting guildNewMasterVoting = new SyncListNewMasterVoting();

        private void Start()
        {
            // do nothing if not spawned (=for character selection previews)
            if (!isServer && !isClient) return;

            // notify guild members that we are online. this also updates the client's
            // own guild info via targetrpc automatically
            // -> OnStartServer is too early because it's not spawned there yet
            if (isServer)
                SetOnline(true);
        }

        private void Update()
        {
            // update overlays in any case, except on server-only mode
            // (also update for character selection previews etc. then)
            if (!isServerOnly)
            {
                if (overlay != null)
                {
                    if (isLocalPlayer) overlay.text = !string.IsNullOrWhiteSpace(guild.name) && SettingsLoader._showGuildName ? overlayPrefix + guild.name + overlaySuffix : "";
                    else overlay.text = !string.IsNullOrWhiteSpace(guild.name) && SettingsLoader._showPlayersGuildName ? overlayPrefix + guild.name + overlaySuffix : "";
                }
            }
        }

        private void OnDestroy()
        {
            // do nothing if not spawned (=for character selection previews)
            if (!isServer && !isClient) return;

            // notify guild members that we are offline
            if (isServer)
                SetOnline(false);
        }

        // guild ///////////////////////////////////////////////////////////////////
        public bool InGuild() => !string.IsNullOrWhiteSpace(guild.name);

        // ServerCALLBACk to ignore the warning if it's called while server isn't
        // active, which happens if OnDestroy->SetOnline(false) is called while
        // shutting down.
        [ServerCallback]
        public void SetOnline(bool online)
        {
            // validate
            if (InGuild())
                GuildSystem.SetGuildOnline(guild.name, name, online);
        }

        [Command]
        public void CmdInviteTarget()
        {
            Debug.Log("Try Invite to Guild");

            // validate
            if (player.interactionExtended.target != null &&
                player.interactionExtended.target is Player targetPlayer &&
                InGuild() && !targetPlayer.guild.InGuild() &&
                guild.CanInvite(name, targetPlayer.name) &&
                NetworkTime.time >= player.nextRiskyActionTime &&
                uSurvival.Utils.ClosestDistance(player.collider, targetPlayer.collider) <= player.interactionRange)
            {
                // send an invite
                targetPlayer.guild.inviteFrom = name;
                targetPlayer.guild.inviteGuild = guild.name;
                Debug.Log(name + " invited " + player.interactionExtended.target.name + " to guild");
            }

            // reset risky time no matter what. even if invite failed, we don't want
            // players to be able to spam the invite button and mass invite random players.
            player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
        }

        [Command]
        public void CmdInviteAccept()
        {
            Debug.Log("Accept Invite to Guild");

            // valid invitation?
            // note: no distance check because sender might be far away already
            if (!InGuild() && inviteFrom != "" &&
                Player.onlinePlayers.TryGetValue(inviteFrom, out Player sender) &&
                sender.guild.InGuild())
            {
                if (sender.guild.CanAcceptToGuild())
                {
                    // try to add. GuildSystem does all the checks.
                    GuildSystem.AddToGuild(sender.guild.guild.name, sender.name, name, 1);
                }
                else
                {
                    if (GuildSystem.RequestToJoinTheGuild(sender.guild.guild.name, name, player.className))
                    {
                        guildWaiting = sender.guild.guild.name;
                    }
                }
            }

            // reset guild invite in any case
            inviteFrom = "";
            inviteGuild = "";
        }

        [Command]
        public void CmdInviteDecline()
        {
            Debug.Log("Decline Ivite to Guild");

            inviteFrom = "";
            inviteGuild = "";
        }

        [Command]
        public void CmdRequestToJoinTheGuild(string guildName)
        {
            Debug.Log("Send Request To Join The Guild");

            if (NetworkTime.time >= player.nextRiskyActionTime && !InGuild())
            {
                if (GuildSystem.RequestToJoinTheGuild(guildName, name, player.className))
                {
                    guildWaiting = guildName;
                }
            }

            // reset risky time no matter what. even if invite failed, we don't want
            // players to be able to spam the invite button and mass invite random players.
            player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
        }

        [Command]public void CmdKick(string memberName)
        {
            Debug.Log("Try Kick member from Guild");

            // validate
            if (CanKickMember(player.name, memberName))
                GuildSystem.KickFromGuild(guild.name, name, memberName);
        }

        [Command]public void CmdMemberRankUpdate(string memberName, byte newRank)
        {
            Debug.Log("Try Update rank Guild member");

            // validate
            if (CanChangeRank(player.name, memberName))
                GuildSystem.UpdateRankMember(guild.name, name, memberName, newRank);
        }

        [Command] public void CmdSetNotice(string notice)
        {
            Debug.Log("Update notice for guild");

            // validate
            // (only allow changes every few seconds to avoid bandwidth issues)
            if (CanChangeNotice(player.name) && NetworkTime.time >= player.nextRiskyActionTime)
            {
                // try to set notice
                GuildSystem.SetGuildNotice(guild.name, name, notice);
            }

            // reset risky time no matter what. even if set notice failed, we don't
            // want people to spam attempts all the time.
            player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
        }

        // helper function to check if we are near a guild manager npc
        public bool IsGuildManagerNear()
        {
            return player.interactionExtended.target != null &&
                   player.interactionExtended.target is Npc npc &&
                   npc.guildManagement && // only if Npc offers guild management
                   uSurvival.Utils.ClosestDistance(player.collider, player.interactionExtended.target.collider) <= player.interactionRange;
        }

        [Command]
        public void CmdTerminate()
        {
            Debug.Log("Try Terminate Guild");

            // validate
            if (InGuild() && IsGuildManagerNear())
            {
                GuildSystem.TerminateGuild(guild.name, name);
                LoadAllGuilds();
            }
        }

        [Command]
        public void CmdCreate(string guildName)
        {
            Debug.Log("Try Create Guild " + guildName);

            // validate
            if (player.health.current > 0 && player.gold >= data.creationPrice &&
                !InGuild() && IsGuildManagerNear())
            {
                // try to create the guild. pay for it if it worked.
                if (GuildSystem.CreateGuild(name, 1, guildName))
                {
                    player.gold -= data.creationPrice;
                    LoadAllGuilds();
                }
                else
                    chat.TargetMsgInfo("Guild name invalid!");
            }
        }

        [Command]
        public void CmdLeave()
        {
            Debug.Log("Leave from Guild");

            // validate
            if (InGuild()) GuildSystem.LeaveGuild(guild.name, name);
        }

        [Command]
        public void CmdLoadAllGuilds()
        {
            Debug.Log("Load All Guilds to List");
            LoadAllGuilds();
        }

        [Server]
        public void LoadAllGuilds()
        {
            allGuilds.Clear();
            foreach (Guild guild in GuildSystem.guilds.Values)
            {
                bool free = false;
                int wins = 0;

                allGuilds.Add(new GuildInfo(guild.name, guild.master, guild.members.Length, free, wins));
            }
        }

        [Command]
        public void CmdCancellationRequestForJoiningGuild()
        {
            Debug.Log("Cancellation Request For Joining Guild");

            if (string.IsNullOrEmpty(guildWaiting))
            {
                GuildSystem.RemoveApprovalRequest(guildWaiting, player.name);

                guildWaiting = "";
            }

            // reset risky time no matter what. even if set notice failed, we don't
            // want people to spam attempts all the time.
            player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
        }

        [Command]
        public void CmdRequestToJoinGuildAccept(string characterName)
        {
            Debug.Log("Accept Request To Join Guild");

            if (InGuild() && CanAcceptToGuild())
            {
                // try to add. GuildSystem does all the checks.
                if (GuildSystem.AddToGuild(guild.name, name, characterName, 1))
                {
                    if (Player.onlinePlayers.TryGetValue(characterName, out Player other))
                    {
                        other.guild.guildWaiting = "";
                    }
                    //else
                    //{
                    //    Database.singleton.UpdateMember(guild.name, characterName, GuildRank.Novice);
                    //}
                }
            }

            // reset risky time no matter what. even if set notice failed, we don't
            // want people to spam attempts all the time.
            player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
        }

        [Command]
        public void CmdRequestToJoinTheGuildReject(string character)
        {
            Debug.Log("Reject Request To Join Guild");

            if (InGuild() && CanAcceptToGuild())
            {
                GuildSystem.RemoveApprovalRequest(guild.name, character);

                if (Player.onlinePlayers.TryGetValue(character, out Player other))
                {
                    other.guild.guildWaiting = "";
                }
            }

            // reset risky time no matter what. even if set notice failed, we don't
            // want people to spam attempts all the time.
            player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
        }

        [Command]
        public void CmdNewMasterVote(bool state)
        {
            Debug.Log("New Master Vote");

            guildNewMasterVoting.Add(new GuildLeaderChange(player.name, state));
            Database.singleton.GuildNewMasterVote(player.guild.guild.name, player.name, state);

            GuildSystem.UpdateGuild(guild);
        }

        public bool CanAcceptToGuild()
        {
            GuildMember requester = guild.members[guild.GetMemberIndex(player.name)];

            return data.CanAcceptToGuild(requester.rank);
        }

        public bool CanChangeRank(string requesterName, string targetName)
        {
            if (InGuild() && guild.members != null && (targetName != guild.master || guild.master == requesterName))
            {
                int requesterIndex = guild.GetMemberIndex(requesterName);
                int targetIndex = guild.GetMemberIndex(targetName);
                if (requesterIndex != -1 && targetIndex != -1)
                {
                    GuildMember requester = guild.members[requesterIndex];
                    GuildMember target = guild.members[targetIndex];

                    return data.CanChangeRank(requester.rank);
                }
            }

            return false;
        }

        // can 'requester' kick 'target'?
        public bool CanKickMember(string requesterName, string targetName)
        {
            if (InGuild() && guild.members != null && requesterName != targetName)
            {
                int requesterIndex = guild.GetMemberIndex(requesterName);
                int targetIndex = guild.GetMemberIndex(targetName);
                if (requesterIndex != -1 && targetIndex != -1)
                {
                    GuildMember requester = guild.members[requesterIndex];
                    GuildMember target = guild.members[targetIndex];

                    if (target.rank == GuildRank.Master) return false;

                    return data.CanKickMember(requester.rank);
                }
            }
            return false;
        }

        // can 'requester' change the notice?
        public bool CanChangeNotice(string requesterName)
        {
            if (InGuild())
            {
                int requesterIndex = guild.GetMemberIndex(requesterName);
                if (requesterIndex != -1)
                {
                    GuildMember requester = guild.members[requesterIndex];

                    return data.CanChangeNotice(requester.rank);
                }
            }
            return false;
        }

        public void CheckStateVotingNewMaster()
        {
            //find out the Max amount votes
            int amount = 0;
            if (data.voteNewMaster == GuildData.vote.all) amount = guild.members.Length - 1;
            else if (data.voteNewMaster == GuildData.vote.vise)
            {
                for (int i = 0; i < guild.members.Length; i++)
                {
                    if (guild.members[i].rank == GuildRank.Vice) amount = amount + 1;
                }
            }
            else
            {
                for (int i = 0; i < guild.members.Length; i++)
                {
                    if (guild.members[i].rank == GuildRank.Vice || guild.members[i].rank == GuildRank.Member) amount = amount + 1;
                }
            }

            //find out the required number of votes
            int required = 1;
            if (amount > 2) required = (int)(amount / data.requiredAmountVotesInPercent);

            //if the number of voters is equal to the required
            if (guildNewMasterVoting.Count >= required)
            {
                //find out amount players who votes yes and who voted no
                int yes = 0;
                int no = 0;

                for (int i = 0; i < guildNewMasterVoting.Count; i++)
                {
                    if (guildNewMasterVoting[i].state) yes = yes + 1;
                    else no = no + 1;
                }

                if (yes > no) GuildSystem.ChangeMaster(guild.name, guild.newMasterName, true);
                else GuildSystem.ChangeMaster(guild.name, guild.newMasterName, false);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (syncInterval == 0)
            {
                syncInterval = 0.1f;
            }

            player = gameObject.GetComponent<Player>();
            chat = gameObject.GetComponent<PlayerChat>();
            player.guild = this;
        }
#endif
    }
}