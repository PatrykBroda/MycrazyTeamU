// Guilds have to be structs in order to work with SyncLists.
// Note: there are no health>0 checks for guild actions. Dead guild masters
//   should still be able to manage their guild
//   => also keeps code clean, otherwise we'd need Player.onlinePlayers and
//      that's not available on the client (for UI states) etc.
using System;
using uSurvival;

namespace GFFAddons
{
    // guild ranks sorted by value. higher equals more power.
    public enum GuildRank : byte
    {
        Novice = 0,
        Member = 1,
        Vice = 2,
        Master = 3     
    }

    [Serializable]
    public partial struct GuildMember
    {
        // basic info
        public string name;
        public int level;
        public bool online;
        public GuildRank rank;

        public GuildMember(string name, int level, bool online, GuildRank rank)
        {
            this.name = name;
            this.level = level;
            this.online = online;
            this.rank = rank;
        }
    }

    public partial struct Guild
    {
        // Guild.Empty for ease of use
        public static Guild Empty = new Guild();

        // properties
        public string name;
        public string notice;
        public GuildMember[] members;

        //gff start
        public uint goldInStorage;
        public short storageSize;
        public ItemSlot[] slots;
        public PlayersPendingGuildDecision[] approvalRequests;

        public string master => members != null ? Array.Find(members, (m) => m.rank == GuildRank.Master).name : "";

        // if we create a guild then always with a name and a first member
        public Guild(string name, string firstMember, int firstMemberLevel)
        {
            this.name = name;
            notice = "";
            GuildMember member = new GuildMember(firstMember, firstMemberLevel, true, GuildRank.Master);
            members = new GuildMember[] { member };

            storageSize = 0;
            goldInStorage = 0;
            slots = new ItemSlot[0];
            approvalRequests = new PlayersPendingGuildDecision[0];

            newMasterName = "";
            voteDateEnd = "";
        }

        // find member index by name
        // (avoid FindIndex for performance/allocations)
        public int GetMemberIndex(string memberName)
        {
            if (members != null)
            {
                for (int i = 0; i < members.Length; ++i)
                    if (members[i].name == memberName)
                        return i;
            }
            return -1;
        }

        public int GetApprovalRequestsIndex(string memberName)
        {
            if (approvalRequests != null)
            {
                for (int i = 0; i < approvalRequests.Length; ++i)
                    if (approvalRequests[i].character == memberName)
                        return i;
            }
            return -1;
        }

        // can 'requester' leave the guild?
        // => not in GuildSystem because it needs to be available on the client too
        // (avoid FindIndex for performance/allocations)
        public bool CanLeave(string requesterName)
        {
            int index = GetMemberIndex(requesterName);
            return index != -1 &&
                   (members[index].rank != GuildRank.Master || members.Length == 1);
        }

        // can 'requester' terminate the guild?
        // => not in GuildSystem because it needs to be available on the client too
        // (avoid FindIndex for performance/allocations)
        public bool CanTerminate(string requesterName)
        {
            // only 1 person left, which is requester, which is the master?
            return members != null &&
                   members.Length == 1 &&
                   members[0].name == requesterName &&
                   members[0].rank == GuildRank.Master;
        }

        // can 'requester' invite 'target'?
        // => not in GuildSystem because it needs to be available on the client too
        public bool CanInvite(string requesterName, string targetName)
        {
            if (members != null &&
                members.Length < GuildSystem.Capacity &&
                requesterName != targetName)
            {
                // avoid FindIndex for performance/GC
                int requesterIndex = GetMemberIndex(requesterName);
                return requesterIndex != -1 &&
                       members[requesterIndex].rank >= GuildSystem.InviteMinRank;
            }
            return false;
        }

        public string newMasterName;
        public string voteDateEnd;
    }
}