// player game master stats / actions / controls.
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public partial class PlayerGameMasterTool : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        [Header("Settings")]
        public float superSpeedBonus = 4;

        // note: isGameMaster flag is in Player.cs

        // server data via SyncVar and SyncToOwner is the easiest solution
        [HideInInspector, SyncVar] public int connections;
        [HideInInspector, SyncVar] public int maxConnections;
        [HideInInspector, SyncVar] public float uptime;
        [HideInInspector, SyncVar] public int tickRate;

        [HideInInspector, SyncVar] public bool immortality = false;
        [HideInInspector, SyncVar(hook = nameof(InvisibilityChanged))] public bool invisibility = false;
        [HideInInspector, SyncVar] public bool killingWithOneHit = false;
        [HideInInspector, SyncVar] public bool superSpeed = false;
        [HideInInspector, SyncVar] public bool weightIgnore = false;

        [HideInInspector] public readonly SyncListGameMasterToolPlayer players = new SyncListGameMasterToolPlayer();

        [HideInInspector, SyncVar] public GameMasterMessage playerInfoMessage = new GameMasterMessage();
        [HideInInspector, SyncVar] public int characterFound = -1;
        private Player findPlayer;
        private double characterInfoStart;

        // tick rate helpers
        private int tickRateCounter;
        private double tickRateStart;

        public override void OnStartServer()
        {
            base.OnStartServer();

            // validate: only for GMs
            if (!player.isGameMaster) return;

            StartUpdateDate();
        }

        [Server]private void StartUpdateDate(){InvokeRepeating(nameof(RefreshData), 1, 1);}
        [Server]private void StopUpdateDate(){CancelInvoke();}

        [ServerCallback]
        private void Update()
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // measure tick rate to get an idea of server load
            ++tickRateCounter;
            if (NetworkTime.time >= tickRateStart + 1)
            {
                // save tick rate. will be synced to client automatically.
                tickRate = tickRateCounter;

                // start counting again
                tickRateCounter = 0;
                tickRateStart = NetworkTime.time;
            }

            if (findPlayer != null && NetworkTime.time >= characterInfoStart + 1)
            {
                if (Player.onlinePlayers.ContainsKey(name))
                {
                    playerInfoMessage = CreateGameMasterMessage(findPlayer);
                }
                else
                {
                    findPlayer = null;
                    playerInfoMessage = new GameMasterMessage();
                }

                characterInfoStart = NetworkTime.time;
            }
        }

        [Server]private void RefreshData()
        {
            // refresh sync vars. will be synced to client automatically.
            connections = NetworkServer.connections.Count;
            maxConnections = NetworkManager.singleton.maxConnections;
            uptime = Time.realtimeSinceStartup;

            players.Clear();
            foreach (var player in Player.onlinePlayers)
                players.Add(new GameMasterToolPlayer(player.Value));
        }

        // server data /////////////////////////////////////////////////////////////
        [Command]public void CmdShutdown()
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            NetworkManagerSurvival.Quit();
        }
        [Command]public void CmdRestart()
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            //NetworkClient.Disconnect();
            //NetworkClient.Shutdown();

            NetworkManagerSurvival.singleton.StopHost();
            NetworkManagerSurvival.singleton.StartHost();
        }

        //chat && Info Messages
        [Command]public void CmdSendGlobalChatMessage(string message)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            player.chat.SendGlobalMessage(message);
        }
        [Command]public void CmdSendGlobalInfoMessage(string message)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            SendGlobalInfoMessage(message);
        }
        [Server]public void SendGlobalInfoMessage(string message)
        {
            foreach (Player onlinePlayer in Player.onlinePlayers.Values)
                onlinePlayer.gameMasterTool.TargetGlobalInfoMessage(message);
        }
        [TargetRpc]public void TargetGlobalInfoMessage(string message)
        {
            UIInfoPanel.singleton.Show(message);
        }

        //find Player
        [Command]public void CmdFindCharacterInDatabase(string otherPlayer)
        {
            // validate: only for GMs
            if (player.isGameMaster && string.IsNullOrEmpty(otherPlayer) == false)
            {
                if (Database.singleton.CharacterExists(otherPlayer))
                {
                    characterFound = 1;
                    if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                        findPlayer = other;
                }
                else characterFound = 0;
            }
        }
        [Command]public void CmdFindPlayerByName(string otherPlayer)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                findPlayer = other;
            else findPlayer = null;
        }

        // GM ///////////////////////////////////////////////////////////////
        [Command]public void CmdSetCharacterImmortality(bool value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            immortality = value;
        }
        [Command]public void CmdSetCharacterInvisibility(bool value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            invisibility = value;

            // hide or show GM
            netIdentity.visible = value ? Visibility.ForceHidden : Visibility.Default;
        }
        [Command]public void CmdSetCharacterKillingWithOneHit(bool value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            killingWithOneHit = value;
        }
        [Command]public void CmdSetCharacterSuperSpeed(bool value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;
            //do not take away or add several times the Super Speed Bonus
            if (superSpeed == value) return;

            superSpeed = value;
        }

        private void InvisibilityChanged(bool oldValue, bool newValue)
        {
            // hide or show GM
            netIdentity.visible = newValue ? Visibility.ForceHidden : Visibility.Default;
        }

        // Player Info ///////////////////////////////////////////////////////////////
        [Command]
        public void CmdSetHealthForPlayer(string otherPlayer, short value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                SetCharacterHealth(other, value);
                playerInfoMessage = CreateGameMasterMessage(other);
            }
        }
        [Command]
        public void CmdSetHydrationForPlayer(string otherPlayer, short value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                SetCharacterHydration(other, value);
                playerInfoMessage = CreateGameMasterMessage(other);
            }
        }
        [Command]
        public void CmdSetNutritionForPlayer(string otherPlayer, short value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                SetCharacterNutrition(other, value);
                playerInfoMessage = CreateGameMasterMessage(other);
            }
        }
        [Command]
        public void CmdSetTemperatureForPlayer(string otherPlayer, short value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                SetCharacterTemperature(other, value);
                playerInfoMessage = CreateGameMasterMessage(other);
            }
        }
        [Command]
        public void CmdSetEnduranceForPlayer(string otherPlayer, short value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                SetCharacterEndurance(other, value);
                playerInfoMessage = CreateGameMasterMessage(other);
            }
        }
        [Server]
        private void SetCharacterHealth(Player player, short value)
        {
            player.health.current = value;
        }
        [Server]
        private void SetCharacterHydration(Player player, short value)
        {
            player.hydration.current = value;
        }
        [Server]
        private void SetCharacterNutrition(Player player, short value)
        {
            player.nutrition.current = value;
        }
        [Server]
        private void SetCharacterTemperature(Player player, short value)
        {
            //player.temperature.current = value;
        }
        [Server]
        private void SetCharacterEndurance(Player player, short value)
        {
            player.endurance.current = value;
        }

        //gold
        [Command]
        public void CmdSetCharacterGold(uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (value > 0)
                player.gold = value;
        }
        [Command]
        public void CmdAddCharacterGoldForPlayer(string otherPlayer, uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                SetCharacterGold(other, other.gold + value);
        }
        [Command]
        public void CmdSetCharacterGoldForPlayer(string otherPlayer, uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                SetCharacterGold(other, value);
        }
        [Server]
        public void SetCharacterGold(Player player, uint value)
        {
            player.gold = value;
        }

        //coins
        [Command]
        public void CmdSetCharacterCoins(uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (value > 0)
                player.itemMall.coins = value;
        }
        [Command]
        public void CmdAddCharacterCoins(string otherPlayer, uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                other.itemMall.coins += value;
        }
        [Command]
        public void CmdSetCharacterCoins(string otherPlayer, uint value)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                other.itemMall.coins = value;
        }


        // player actions //////////////////////////////////////////////////////////
        [Command]
        public void CmdWarp(string otherPlayer)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // warp self to other
            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
                player.movement.Warp(other.transform.position);
        }
        [Command]
        public void CmdSummon(string otherPlayer)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // summon other to self and add chat message so the player knows why
            // it happened
            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                other.movement.Warp(player.transform.position);
                other.chat.TargetMsgInfo("A GM summoned you.");
            }
        }
        [Command]
        public void CmdKill(string otherPlayer)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // kill other and add chat message so the player knows why it happened
            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                other.health.current = 0;
                other.chat.TargetMsgInfo("A GM killed you.");
            }
        }
        [Command]
        public void CmdKick(string otherPlayer)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // kick other
            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                // TODO add a reason for kick so people don't think they were disconnected
                other.gameMasterTool.TargetGlobalInfoMessage("You have been disconnected from the server by an GM");

                StartCoroutine(Disconect(other));
            }
        }
        [Command]
        public void CmdBan(string otherPlayer, string reason)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            // ban other
            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                // TODO add a reason for kick so people don't think they were disconnected
                other.gameMasterTool.TargetGlobalInfoMessage("You have been Banned by GM " + reason);

                Database.singleton.BanAccount(other.account);

                StartCoroutine(Disconect(other));
            }
        }
        [Command]
        public void CmdSetGM(string otherPlayer, bool state)
        {
            // validate: only for GMs
            if (!player.isGameMaster) return;

            if (Player.onlinePlayers.TryGetValue(otherPlayer, out Player other))
            {
                if (state)
                {
                    if (!other.isGameMaster)
                    {
                        other.isGameMaster = true;
                        other.gameMasterTool.StartUpdateDate();

                        other.gameMasterTool.TargetGlobalInfoMessage("Now you GM");
                    }
                }
                else
                {
                    if (other.isGameMaster)
                    {
                        other.isGameMaster = false;
                        other.gameMasterTool.StopUpdateDate();

                        other.gameMasterTool.TargetGlobalInfoMessage("Now you're not a GM");
                    }
                }
            }
        }

        [Server]public void CmdSetGMFromChat()
        {
            // validate: only for GMs
            if (player.isGameMaster) return;

            player.isGameMaster = true;
            StartUpdateDate();

            TargetGlobalInfoMessage("Now you GM");
        }

        //messages
        [Server]private GameMasterMessage CreateGameMasterMessage(Player targetPlayer)
        {
            GameMasterMessage message = new GameMasterMessage
            {
                targetName = targetPlayer.name,
                isOnline = true,
                healthStatus = targetPlayer.health.current + " / " + targetPlayer.health.max,
                healthValue = targetPlayer.health.Percent(),
                healthMax = targetPlayer.health.max,
                hydrationStatus = targetPlayer.hydration.current + " / " + targetPlayer.hydration.max,
                hydrationValue = targetPlayer.hydration.Percent(),
                hydrationMax = targetPlayer.hydration.max,
                nutritionStatus = targetPlayer.nutrition.current + " / " + targetPlayer.nutrition.max,
                nutritionValue = targetPlayer.nutrition.Percent(),
                nutritionMax = targetPlayer.nutrition.max,
                //temperatureValue = targetPlayer.temperature.Percent(),
                //temperatureCurrent = targetPlayer.temperature.current,
                //temperatureMax = targetPlayer.temperature.max,
                enduranceStatus = targetPlayer.endurance.current + " / " + targetPlayer.endurance.max,
                enduranceValue = targetPlayer.endurance.Percent(),
                enduranceMax = targetPlayer.endurance.max,
                inventory = new List<ItemSlot>(),
                equipment = new List<ItemSlot>(),
                equipmentCategory = new List<string>()
            };

            foreach (ItemSlot itemSlot in targetPlayer.inventory.slots)
            {
                message.inventory.Add(itemSlot);
            }

            foreach (ItemSlot itemSlot in targetPlayer.equipment.slots)
            {
                message.equipment.Add(itemSlot);
            }

            for (int i = 0; i < targetPlayer.equipment.slotInfo.Length; i++)
            {
                string overlay = uSurvival.Utils.ParseLastNoun(targetPlayer.equipment.slotInfo[i].requiredCategory.ToString());
                message.equipmentCategory.Add(overlay);
            }

            return message;
        }

        private IEnumerator Disconect(Player other)
        {
            yield return new WaitForSeconds(1);
            other.connectionToClient.Disconnect();
        }

#if UNITY_EDITOR
        // validation //////////////////////////////////////////////////////////////
        protected override void OnValidate()
        {
            base.OnValidate();

            if (player == null) player = gameObject.GetComponent<Player>();
            if (player != null && player.gameMasterTool == null) player.gameMasterTool = this;

            // gm tool data should only ever be synced to owner!
            // observers should not know about it!
            if (syncMode != SyncMode.Owner)
            {
                syncMode = SyncMode.Owner;

                Undo.RecordObject(this, name + " " + GetType() + " component syncMode changed to Owner.");
            }
        }
#endif
    }
}