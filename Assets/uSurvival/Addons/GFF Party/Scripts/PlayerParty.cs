using UnityEngine;
using Mirror;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerParty : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        // .party is a copy for easier reading/syncing. Use PartySystem to manage parties!
        [Header("Party")]
        [SyncVar, HideInInspector] public Party party; // TODO SyncToOwner later

        [SyncVar, HideInInspector] public string inviteFrom = "";
        public float inviteWaitSeconds = 3;
        //[Range(0,1)] public float visiblityAlphaRange = 0.5f;

        private void OnDestroy()
        {
            // do nothing if not spawned (=for character selection previews)
            if (!isServer && !isClient) return;

            if (isServer)
            {
                // leave party (if any)
                if (InParty())
                {
                    // dismiss if master, leave otherwise
                    if (party.master == name)
                        Dismiss();
                    else
                        Leave();
                }
            }
        }

        // party ///////////////////////////////////////////////////////////////////
        public bool InParty()
        {
            // 0 means no party, because default party struct's partyId is 0.
            return party.partyId > 0;
        }

        // party invite by name (not by target) so that chat commands are possible
        // if needed
        [Command]public void CmdInvite(string otherName)
        {
            Debug.Log(name + " try invited " + otherName + " to party");
            // validate: is there someone with that name, and not self?
            if (otherName != name &&
                Player.onlinePlayers.TryGetValue(otherName, out Player other) &&
                NetworkTime.time >= player.nextRiskyActionTime)
            {
                // can only send invite if no party yet or party isn't full and
                // have invite rights and other guy isn't in party yet
                if ((!InParty() || !party.IsFull()) && !other.party.InParty())
                {
                    // send an invite
                    other.party.inviteFrom = name;
                    Debug.Log(name + " invited " + other.name + " to party");
                }
            }

            // reset risky time no matter what. even if invite failed, we don't want
            // players to be able to spam the invite button and mass invite random
            // players.
            player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
        }

        [Command]public void CmdAcceptInvite()
        {
            // valid invitation?
            // note: no distance check because sender might be far away already
            if (!InParty() && inviteFrom != "" &&
                Player.onlinePlayers.TryGetValue(inviteFrom, out Player sender))
            {
                // is in party? then try to add
                if (sender.party.InParty())
                {
                    PartySystem.AddToParty(sender.party.party.partyId, name);

                    for (int i = 0; i < sender.party.party.members.Length; i++)
                    {
                        string memberName = sender.party.party.members[i];
                        if (Player.onlinePlayers.ContainsKey(memberName))
                        {
                            Player member = Player.onlinePlayers[memberName];
                            member.party.RpcSendOpenPanel();
                        }
                    }
                }

                // otherwise try to form a new one
                else
                {
                    PartySystem.FormParty(sender.name, name);
                    RpcSendOpenPanel();
                    sender.party.RpcSendOpenPanel();
                }
            }

            // reset party invite in any case
            inviteFrom = "";
        }

        [Command]public void CmdDeclineInvite()
        {
            inviteFrom = "";
        }

        [Command]public void CmdKick(string member)
        {
            // try to kick. party system will do all the validation.
            PartySystem.KickFromParty(party.partyId, name, member);
        }

        // version without cmd because we need to call it from the server too
        public void Leave()
        {
            // try to leave. party system will do all the validation.
            PartySystem.LeaveParty(party.partyId, name);
        }
        [Command] public void CmdLeave() { Leave(); }

        // version without cmd because we need to call it from the server too
        public void Dismiss()
        {
            // try to dismiss. party system will do all the validation.
            PartySystem.DismissParty(party.partyId, name);
        }
        [Command] public void CmdDismiss() { Dismiss(); }

        [Command]public void CmdPartyChangeLeader(string member)
        {
            PartySystem.ChangeLeader(party.partyId, name, member);
        }

        private void ChangePartyMaster()
        {
            // dismiss if master, leave otherwise
            if (party.master == name) Dismiss();
            else Leave();
        }

        [TargetRpc]private void RpcSendOpenPanel()
        {
            UIPartyExtended.singleton.Show();
        }
    }
}


