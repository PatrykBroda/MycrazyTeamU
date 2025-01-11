using System;
using UnityEditor;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "GFF Addons/Guild Extended Data", order = 999)]
    public class GuildData : ScriptableObject
    {
        public uint creationPrice = 100;
        public bool useGuildStorage = false;

        [Header("Who is allowed to accept new guild players")]
        [SerializeField] private GuildRank[] acceptToGuild;

        [Header("Who is allowed to change the rank of a guild member")]
        [SerializeField] private GuildRank[] changeRank;

        [Header("Who is allowed to kick members from guild")]
        [SerializeField] private GuildRank[] kickMembers;

        [Header("Who is allowed change notice")]
        [SerializeField] private GuildRank[] changeNotice;

        [Header("Storage")]
        [SerializeField] private int[] _storagePurchaseCost = new int[5] { 10, 20, 30, 40, 50 };
        [SerializeField] private short _amountPurchasedSlots = 5;
        [SerializeField, Tooltip("if equal to zero, then it will be impossible to store gold")] private uint _maxGoldInStorage = 2000000000;

        [Header("Who is allowed buy slots for storage")]
        [SerializeField] private GuildRank[] buySlotsForStorage;

        [Header("who is allowed to put items in the storage")]
        [SerializeField] private GuildRank[] allowPutItems;

        [Header("who is allowed to put gold in the storage")]
        [SerializeField] private GuildRank[] allowPutGold;

        [Header("who is allowed to take items from storage")]
        [SerializeField] private GuildRank[] allowTakeItems;

        [Header("who is allowed to take gold from storage")]
        [SerializeField] private GuildRank[] allowTakeGold;


        public short amountPurchasedSlots => _amountPurchasedSlots;
        public uint maxGoldInStorage => _maxGoldInStorage;
        public int[] storagePurchaseCost => _storagePurchaseCost;

        [Header("Settings : Change of guild master")]
        public bool guildMasterChangeAllowed;

        public ñondition guildMasterChangeConditions = ñondition.anytime;
        public enum ñondition { anytime, onceAWeek, onceAMonth, masterOfflineWeek, masterOfflineMonth };

        public float guildMasterChangeVotingTime = 60;
        public float requiredAmountVotesInPercent = 50;


        [Header("Settings : who can offer a new guild Master")]
        public offer offerNewMaster = offer.masterAndVise;
        public enum offer { onlyMaster, masterAndVise, all };

        [Header("Settings : who is allowed to vote for the new guild master")]
        public vote voteNewMaster = vote.vise;
        public enum vote { vise, viseAndMembers, all };

        public bool CheckConditionForChoosingNewMaster(Player player)
        {
            if (guildMasterChangeConditions == ñondition.anytime) return true;
            else if (guildMasterChangeConditions == ñondition.onceAWeek)
            {
                if (DateTime.Parse(player.guild.guild.voteDateEnd).AddDays(7) > DateTime.Now) return true;
            }
            else if (guildMasterChangeConditions == ñondition.onceAMonth)
            {
                if (DateTime.Parse(player.guild.guild.voteDateEnd).AddMonths(1) > DateTime.Now) return true;
            }
            else if (guildMasterChangeConditions == ñondition.masterOfflineWeek)
            {

            }
            else if (guildMasterChangeConditions == ñondition.masterOfflineMonth)
            {

            }

            return false;
        }

        public bool CanAcceptToGuild(GuildRank rank)
        {
            if (acceptToGuild.Length == 0 || acceptToGuild == null) return true;

            for (int i = 0; i < acceptToGuild.Length; ++i)
            {
                if (rank == acceptToGuild[i]) return true;
            }

            return false;
        }

        public bool CanChangeRank(GuildRank rank)
        {
            if (changeRank.Length == 0 || changeRank == null) return true;

            for (int i = 0; i < changeRank.Length; ++i)
            {
                if (rank == changeRank[i]) return true;
            }

            return false;
        }

        // can 'requester' kick 'target'?
        public bool CanKickMember(GuildRank rank)
        {
            if (kickMembers.Length == 0 || kickMembers == null) return true;

            for (int i = 0; i < kickMembers.Length; ++i)
                if (rank == kickMembers[i]) return true;

            return false;
        }

        // can 'requester' change the notice?
        public bool CanChangeNotice(GuildRank rank)
        {
            if (changeNotice.Length == 0 || changeNotice == null) return true;

            for (int i = 0; i < changeNotice.Length; ++i)
                if (rank == changeNotice[i]) return true;

            return false;
        }


        //storage
        public int GetStoragePurchaseCost(Guild guild)
        {
            if (guild.storageSize < storagePurchaseCost.Length * amountPurchasedSlots)
                return storagePurchaseCost[guild.storageSize / amountPurchasedSlots];
            else return 0;
        }
        public int GetMaxSlotsAmountForStorage()
        {
            return storagePurchaseCost.Length * amountPurchasedSlots;
        }
        public bool CanBuyMoreSlots(Guild guild)
        {
            return guild.storageSize < storagePurchaseCost.Length * amountPurchasedSlots;
        }
        public uint GetPriceForBuingSlots(Guild guild)
        {
            return (uint)storagePurchaseCost[guild.storageSize / amountPurchasedSlots];
        }

        public bool CanBuySlots(GuildRank rank)
        {
            if (buySlotsForStorage.Length == 0 || buySlotsForStorage == null) return true;

            for (int i = 0; i < buySlotsForStorage.Length; ++i)
            {
                if (rank == buySlotsForStorage[i]) return true;
            }

            return false;
        }
        public bool CanPutItems(GuildRank rank)
        {
            if (allowPutItems.Length == 0 || allowPutItems == null) return true;

            for (int i = 0; i < allowPutItems.Length; ++i)
            {
                if (rank == allowPutItems[i]) return true;
            }

            return false;
        }
        public bool CanPutGold(GuildRank rank)
        {
            if (allowPutGold.Length == 0 || allowPutGold == null) return true;

            for (int i = 0; i < allowPutGold.Length; ++i)
            {
                if (rank == allowPutGold[i]) return true;
            }

            return false;
        }
        public bool CanTakeItems(GuildRank rank)
        {
            if (allowTakeItems.Length == 0 || allowTakeItems == null) return true;

            for (int i = 0; i < allowTakeItems.Length; ++i)
            {
                if (rank == allowTakeItems[i]) return true;
            }

            return false;
        }
        public bool CanTakeGold(GuildRank rank)
        {
            if (allowTakeGold.Length == 0 || allowTakeGold == null) return true;

            for (int i = 0; i < allowTakeGold.Length; ++i)
            {
                if (rank == allowTakeGold[i]) return true;
            }

            return false;
        }
    }
}