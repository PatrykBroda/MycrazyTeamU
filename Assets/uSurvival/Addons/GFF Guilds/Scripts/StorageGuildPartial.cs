using UnityEngine;
using UnityEngine.UI;
using System;
using GFFAddons;
using uSurvival;

namespace uSurvival
{
    public partial class ScriptableItem
    {
        [Header("GFF Storage Addon")]
        public bool canPutInTheStorageGuild = true;
    }

    public partial class Player
    {
        [Header("GFF Storage Guild Addon")]
        public PlayerGuildStorage storageGuild;
    }
}

namespace GFFAddons
{
    public static partial class GuildSystem
    {
        public static void IncreaseStorageSize(string guildName, short value)
        {
            // guild exists
            if (guilds.TryGetValue(guildName, out Guild guild))
            {
                guild.storageSize += value;
                Array.Resize(ref guild.slots, guild.storageSize);

                // broadcast and save
                BroadcastChanges(guild);
            }
        }

        public static void UpdateGold(string guildName, uint value)
        {
            // guild exists
            if (guilds.TryGetValue(guildName, out Guild guild))
            {
                guild.goldInStorage = value;

                // broadcast and save
                BroadcastChanges(guild);
            }
        }

        public static void UpdateStorage(string guildName, ItemSlot[] slots)
        {
            // guild exists
            if (guilds.TryGetValue(guildName, out Guild guild))
            {
                guild.slots = slots;

                // broadcast and save
                BroadcastChanges(guild);
            }
        }
    }
}

