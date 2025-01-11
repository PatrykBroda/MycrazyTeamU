using GFFAddons;
using Mirror;
using System;
using TMPro;
using UnityEngine;

namespace GFFAddons
{
    public enum Clan { none, Military, Churchmen, Anarchists }

    public partial class Npc
    {
        public Clan clan;
        public short minClanRelations = 50;
    }
}

namespace uSurvival
{
    public partial class NetworkManagerSurvival
    {
        [SerializeField] private Transform[] respawnByKlan;

        public Vector3 GetRespawnPosition(int klan)
        {
            return respawnByKlan[klan].position;
        }

        private void OnServerCharacterCreate_NpcGroups(Player player)
        {
            foreach (Clan clan in (Clan[])Enum.GetValues(typeof(Clan)))
            {
                if (clan != Clan.none) player.groups.Add(new NpcClan(clan));
            }
        }
    }

    public partial class Database
    {
        class character_groups
        {
            public string character { get; set; }
            public Clan clan { get; set; }
            public short level { get; set; }
            public uint exp { get; set; }

            // PRIMARY KEY (character, slot) is created manually.
        }

        public void Connect_NpcClans()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_groups>();
        }

        public void CharacterLoad_NpcClans(Player player)
        {
            // fill all slots first
            foreach (character_groups row in connection.Query<character_groups>("SELECT * FROM character_groups WHERE character=?", player.name))
            {
                player.groups.Add(new NpcClan(row.clan, row.level, row.exp));
            }

            if (player.groups.Count == 0)
            {
                foreach (Clan clan in (Clan[])Enum.GetValues(typeof(Clan)))
                {
                    if (clan != Clan.none)
                    {
                        player.groups.Add(new NpcClan(clan, 1, 0));
                    }
                }
            }
        }

        public void CharacterSave_NpcClans(Player player)
        {
            connection.Execute("DELETE FROM character_groups WHERE character=?", player.name);

            for (byte i = 0; i < player.groups.Count; ++i)
            {
                if (player.groups[i].clan != Clan.none)
                {
                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(new character_groups
                    {
                        character = player.name,
                        clan = player.groups[i].clan,
                        level = player.groups[i].level,
                        exp = player.groups[i].exp
                    });
                }
            }
        }
    }

    public partial class Combat
    {
        private void SetClans(Player player, Player victimPlayer)
        {
            if (player.selectedClan == Clan.none)
            {
                if (victimPlayer.selectedClan == Clan.none)
                {
                    player.remainMurdererBuff += 360;
                }
                else if (victimPlayer.selectedClan == Clan.Military)
                {
                    player.remainMurdererBuff += 720;
                }
                else player.remainMurdererBuff -= 60;
            }
            else if (player.selectedClan == Clan.Military)
            {
                if (victimPlayer.selectedClan == Clan.none)
                {
                    player.remainMurdererBuff += 360;
                    player.AddUpdateGroupData(Clan.Military, -60);
                }
                else if (victimPlayer.selectedClan == Clan.Military)
                {
                    player.remainMurdererBuff += 720;
                    player.AddUpdateGroupData(Clan.Military, -60);
                }
                else player.remainMurdererBuff -= 60;
            }
            else if (player.selectedClan == Clan.Anarchists)
            {
                if (victimPlayer.selectedClan == Clan.Anarchists)
                {
                    player.remainMurdererBuff += 720;
                    player.AddUpdateGroupData(Clan.Anarchists, -60);
                }
                else player.remainMurdererBuff -= 60;
            }
            else if (player.selectedClan == Clan.Churchmen)
            {
                if (victimPlayer.selectedClan == Clan.Churchmen)
                {
                    player.remainMurdererBuff += 720;
                    player.AddUpdateGroupData(Clan.Churchmen, -60);
                }
                else player.remainMurdererBuff -= 60;
            }
        }
    }

    public partial class Player
    {
        [Header("Text Groups Meshes")]
        public TextMeshPro overlay;
        public string overlayPrefix = "[";
        public string overlaySuffix = "]";

        [Header("Player info : Groups")]
        [SyncVar] public Clan selectedClan = Clan.none;
        public readonly SyncListNpcClan groups = new SyncListNpcClan() { };
        [HideInInspector] public SafeZoneWithSecurity currentSafeZoneWithSecuritySource;

        [SyncVar] public double remainMurdererBuff = 0;

        public short GetGroupLevel(Clan clan)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].clan == clan) return groups[i].level;
            }

            return 1;
        }

        [Command]
        public void CmdSetKlan(Clan group)
        {
            selectedClan = group;
        }

        public void AddUpdateGroupData(Clan clan, int value)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].clan == clan)
                {
                    groups[i].UpdateExp(value);
                }
            }
        }

        private void Update()
        {
            // update overlays in any case, except on server-only mode
            // (also update for character selection previews etc. then)
            if (!isServerOnly)
            {
                if (overlay != null)
                {
                    if (isLocalPlayer) overlay.text = selectedClan != Clan.none && SettingsLoader._showKlanName ? overlayPrefix + selectedClan.ToString() + overlaySuffix : "";
                    else overlay.text = selectedClan != Clan.none && SettingsLoader._showPlayersKlanName ? overlayPrefix + selectedClan.ToString() + overlaySuffix : "";
                }
            }

            if (isServer)
            {
                if (remainMurdererBuff > 0) remainMurdererBuff -= Time.deltaTime;
            }
        }

        public bool CheckZoneForDropItems()
        {
            if (currentSafeZoneWithSecuritySource != null &&
                (currentSafeZoneWithSecuritySource.clan == selectedClan ||
                (currentSafeZoneWithSecuritySource.clan == Clan.Military && selectedClan == Clan.none))) return false;
            else return true;
        }

        [Server]private void AddMurdererTime()
        {
            remainMurdererBuff += 60;
        }
    }

    public partial class PlayerRespawning
    {
        public Player player;
    }
}
