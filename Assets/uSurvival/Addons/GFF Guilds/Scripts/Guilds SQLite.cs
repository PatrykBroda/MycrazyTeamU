using GFFAddons;
using SQLite;
using System.Collections.Generic;
using UnityEngine;

namespace uSurvival
{
    public partial class Database
    {
        class guild_info
        {
            // guild master is not in guild_info in case we need more than one later
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string name { get; set; }
            public string notice { get; set; }
            public short storageSize { get; set; }
            public uint goldInStorage { get; set; }
            public string newMasterName { get; set; }
            public string voteDateEnd { get; set; }
        }
        class guild_members
        {
            // guild members are saved in a separate table because instead of in a
            // characters.guild field because:
            // * guilds need to be resaved independently, not just in CharacterSave
            // * kicked members' guilds are cleared automatically because we drop
            //   and then insert all members each time. otherwise we'd have to
            //   update the kicked member's guild field manually each time
            // * it's easier to remove / modify the guild feature if it's not hard-
            //   coded into the characters table
            [PrimaryKey] // important for performance: O(log n) instead of O(n)
            public string character { get; set; }
            // add index on guild to avoid full scans when loading guild members
            [Indexed]
            public string guild { get; set; }
            public int rank { get; set; }
        }
        class guild_awaitingApproval
        {
            public string guild { get; set; }
            public string character { get; set; }
            public string characterClass { get; set; }
        }
        class guild_storage
        {
            public string guild { get; set; }
            public int slot { get; set; }
            public string name { get; set; }
            public ushort amount { get; set; }
            public ushort durability { get; set; }
            public ushort ammo { get; set; }
            public string ammoname { get; set; }
            public bool binding { get; set; }
            public sbyte skin { get; set; }

            public string modules { get; set; }

            public string secondItemName { get; set; }
            public ushort secondItemDurability { get; set; }
            public bool secondItemBinding { get; set; }

            //item enchantment addon
            //public int holes { get; set; }
            //public string upgradeInd { get; set; }

            // PRIMARY KEY (quild, slot) is created manually.
        }
        class guild_newMasterVote
        {
            public string guild { get; set; }
            public string name { get; set; }
            public bool state { get; set; }
        }

        public void Connect_Guilds()
        {
            connection.CreateTable<guild_members>();
            connection.CreateTable<guild_info>();
            connection.CreateTable<guild_awaitingApproval>();
            connection.CreateTable<guild_newMasterVote>();

            connection.CreateTable<guild_storage>();
            connection.CreateIndex(nameof(guild_storage), new[] { "guild", "slot" });

            //load all guulds
            LoadAllGuilds();
        }

        public bool GuildExists(string guild)
        {
            return connection.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guild) != null;
        }

        private void LoadAllGuilds()
        {
            foreach (guild_info row in connection.Query<guild_info>("SELECT * FROM guild_info"))
            {
                if (!GuildSystem.guilds.ContainsKey(row.name))
                {
                    Guild guild = LoadGuild(row.name);
                    GuildSystem.guilds[guild.name] = guild;
                }
            }
        }

        private Guild LoadGuild(string guildName)
        {
            Guild guild = new Guild();

            // set name
            guild.name = guildName;

            // load guild info
            guild_info info = connection.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guildName);
            if (info != null)
            {
                guild.notice = info.notice;
                guild.storageSize = info.storageSize;
                guild.goldInStorage = info.goldInStorage;
                guild.slots = LoadStorageItemsGuild(guild);
            }

            // load members list
            List<guild_members> rows = connection.Query<guild_members>("SELECT * FROM guild_members WHERE guild=?", guildName);
            GuildMember[] members = new GuildMember[rows.Count]; // avoid .ToList(). use array directly.
            for (int i = 0; i < rows.Count; ++i)
            {
                guild_members row = rows[i];

                GuildMember member = new GuildMember();
                member.name = row.character;
                member.rank = (GuildRank)row.rank;

                // is this player online right now? then use runtime data
                if (Player.onlinePlayers.TryGetValue(member.name, out Player player))
                {
                    member.online = true;
                    //member.level = player.level.current;
                }
                else
                {
                    member.online = false;

                    // note: FindWithQuery<characters> is easier than ExecuteScalar<int> because we need the null check
                    //characters character = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", member.name);
                    //member.level = character != null ? character.level : 1;
                }

                members[i] = member;
            }
            guild.members = members;

            //load awaiting approval
            List<guild_awaitingApproval> awaitingApproval = connection.Query<guild_awaitingApproval>("SELECT * FROM guild_awaitingApproval WHERE guild=?", guildName);
            PlayersPendingGuildDecision[] playersPendingGuildDecision = new PlayersPendingGuildDecision[awaitingApproval.Count];
            for (int i = 0; i < awaitingApproval.Count; ++i)
            {
                guild_awaitingApproval row = awaitingApproval[i];
                PlayersPendingGuildDecision player = new PlayersPendingGuildDecision();
                player.character = row.character;
                player.characterClass = row.characterClass;
                playersPendingGuildDecision[i] = player;
            }
            guild.approvalRequests = playersPendingGuildDecision;

            return guild;
        }

        // only load guild when their first player logs in
        // => using NetworkManager.Awake to load all guilds.Where would work,
        //    but we would require lots of memory and it might take a long time.
        // => hooking into player loading to load guilds is a really smart solution,
        //    because we don't ever have to load guilds that aren't needed
        private void LoadGuildOnDemand(PlayerGuild playerGuild)
        {
            string guildName = connection.ExecuteScalar<string>("SELECT guild FROM guild_members WHERE character=?", playerGuild.name);
            if (guildName != null)
            {
                // load guild on demand when the first player of that guild logs in
                // (= if it's not in GuildSystem.guilds yet)
                if (!GuildSystem.guilds.ContainsKey(guildName))
                {
                    Guild guild = LoadGuild(guildName);
                    GuildSystem.guilds[guild.name] = guild;
                    playerGuild.guild = guild;
                }
                // assign from already loaded guild
                else playerGuild.guild = GuildSystem.guilds[guildName];
            }
            else
            {
                guild_awaitingApproval info = connection.FindWithQuery<guild_awaitingApproval>("SELECT * FROM guild_awaitingApproval WHERE character=?", playerGuild.name);
                if (info != null) playerGuild.guildWaiting = info.guild;
                else playerGuild.guildWaiting = "";
            }
        }

        private string LoadGuildForPlayer(string player)
        {
            return connection.ExecuteScalar<string>("SELECT guild FROM guild_members WHERE character=?", player);
        }

        public void SaveAllGuilds()
        {
            foreach (Guild guild in GuildSystem.guilds.Values)
            {
                SaveGuild(guild);
            }
        }

        public void SaveGuild(Guild guild, bool useTransaction = true)
        {
            if (useTransaction) connection.BeginTransaction(); // transaction for performance

            // guild info
            connection.InsertOrReplace(new guild_info
            {
                name = guild.name,
                notice = guild.notice,
                storageSize = guild.storageSize,
                goldInStorage = guild.goldInStorage
            });

            // members list
            connection.Execute("DELETE FROM guild_members WHERE guild=?", guild.name);
            foreach (GuildMember member in guild.members)
            {
                connection.InsertOrReplace(new guild_members
                {
                    character = member.name,
                    guild = guild.name,
                    rank = (int)member.rank
                });
            }

            //awaiting Approval
            connection.Execute("DELETE FROM guild_awaitingApproval WHERE guild=?", guild.name);
            foreach (PlayersPendingGuildDecision member in guild.approvalRequests)
            {
                connection.InsertOrReplace(new guild_awaitingApproval
                {
                    guild = guild.name,
                    character = member.character,
                    characterClass = member.characterClass
                });
            }

            SaveStorageItemsGuild(guild);

            if (useTransaction) connection.Commit(); // end transaction
        }

        //public void UpdateMember(string guildname, string character, GuildRank rank)
        //{
        //    connection.Insert(new guild_members
        //    {
        //        guild = guildname,
        //        character = character,
        //        rank = (int)rank
        //    });
        //}

        public void RemoveGuild(string guild)
        {
            connection.BeginTransaction(); // transaction for performance
            connection.Execute("DELETE FROM guild_info WHERE name=?", guild);
            connection.Execute("DELETE FROM guild_members WHERE guild=?", guild);
            connection.Execute("DELETE FROM guild_awaitingApproval WHERE guild=?", guild);
            connection.Execute("DELETE FROM guild_storage WHERE guild=?", guild);
            connection.Execute("DELETE FROM guild_newMasterVote WHERE guild=?", guild);
            connection.Commit(); // end transaction
        }


        //Awaiting Approval from NetworkManager
        public void CharacterLoad_GuildAwaitingApproval(Player player)
        {
            if (!player.guild.InGuild())
            {
                guild_awaitingApproval info = connection.FindWithQuery<guild_awaitingApproval>("SELECT * FROM guild_awaitingApproval WHERE character=?", player.name);
                if (info != null) player.guild.guildWaiting = info.guild;
            }
        }


        //Storage
        public ItemSlot[] LoadStorageItemsGuild(Guild guild)
        {
            guild.slots = new ItemSlot[guild.storageSize];

            foreach (guild_storage row in connection.Query<guild_storage>("SELECT * FROM guild_storage WHERE guild=?", guild.name))
            {
                if (row.slot < guild.storageSize)
                {
                    if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        Item item = new Item(itemData);
                        item.ammo = row.ammo;
                        item.ammoname = row.ammoname;
                        item.durability = (ushort)Mathf.Min(row.durability, item.maxDurability);
                        item.binding = row.binding;
                        item.skin = row.skin;

                        if (string.IsNullOrEmpty(row.secondItemName) == false)
                        {
                            item.secondItemHash = row.secondItemName.GetStableHashCode();
                            item.secondItemDurability = row.secondItemDurability;
                        }

                        item = LoadModulesForSlot(item, row.modules);

                        guild.slots[row.slot] = new ItemSlot(item, row.amount);
                    }
                    else Debug.LogWarning("LoadGuildStorage: skipped item " + row.name + " for " + guild.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
                }
                else Debug.LogWarning("LoadGuildStorage: skipped slot " + row.slot + " for " + guild.name + " because it's bigger than size guild storage size");
            }

            return guild.slots;
        }

        public void SaveStorageItemsGuild(Guild guild)
        {
            // remove old entries first, then add all new ones
            // (we could use UPDATE where slot=... but deleting everything makes
            // sure that there are never any ghosts)
            connection.Execute("DELETE FROM guild_storage WHERE guild=?", guild.name);

            for (int i = 0; i < guild.slots.Length; ++i)
            {
                ItemSlot slot = guild.slots[i];
                if (slot.amount > 0) // only relevant items to save queries/storage/time
                {
                    guild_storage msg = new guild_storage();

                    string secondItem = "";
                    if (ScriptableItem.dict.TryGetValue(slot.item.secondItemHash, out ScriptableItem itemData))
                    {
                        secondItem = itemData.name;
                    }

                    msg.guild = guild.name;
                    msg.slot = i;
                    msg.name = slot.item.name;
                    msg.amount = slot.amount;
                    msg.ammo = slot.item.ammo;
                    msg.ammoname = slot.item.ammoname;
                    msg.durability = slot.item.durability;
                    msg.binding = slot.item.binding;
                    msg.skin = slot.item.skin;
                    msg.modules = SaveModulesFromSlot(slot.item);

                    msg.secondItemName = secondItem;
                    msg.secondItemDurability = slot.item.secondItemDurability;

                    //item enchantment addon
                    //msg.holes = slot.item.holes;
                    //msg.upgradeInd = SaveUpgradeInd(slot.item);

                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(msg);
                }
            }
        }


        //Elections of a new leader
        public void CharacterLoad_GuildLoadMasterVote(Player player)
        {
            if (player.guild.InGuild())
            {
                player.guild.guildNewMasterVoting.Clear();

                foreach (guild_newMasterVote row in connection.Query<guild_newMasterVote>("SELECT * FROM guild_newMasterVote WHERE guild=?", player.guild.name))
                {
                    player.guild.guildNewMasterVoting.Add(new GuildLeaderChange(
                         row.name,
                         row.state
                         ));
                }
            }
        }

        public void GuildNewMasterVote(string guild, string player, bool state)
        {
            connection.Insert(new guild_newMasterVote
            {
                guild = guild,
                name = player,
                state = state
            });
        }
    }
}