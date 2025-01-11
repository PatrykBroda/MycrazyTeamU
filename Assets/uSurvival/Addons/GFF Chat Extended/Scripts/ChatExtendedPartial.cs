using GFFAddons;
using Mirror;
using UnityEngine;

namespace GFFAddons
{
    public enum chatChannel { info, gm, all, whisper, trade, battle }
}

namespace uSurvival
{
    public partial class PlayerChat
    {
        [Header("Channels Extended")]
        public ChannelInfo gm = new ChannelInfo("/gm", "", "", null);
        public ChannelInfo all = new ChannelInfo("/all", "", "", null); // синий общий для всей рассы во всех локах
        public ChannelInfo trade = new ChannelInfo("/trade", "", "", null);

        public ChatWordFilter wordsFilter;


        public override void OnStartLocalPlayer()
        {
            UIChatExtended.singleton.AddMessage(new ChatMessage("", gm.identifierIn, "Приветствуем Выживший", "", gm.textPrefab), chatChannel.gm);

            // test messages
            UIChatExtended.singleton.AddMessage(new ChatMessage("", info.identifierIn, "Используйте /w 'Имя' для личного сообщения", "", info.textPrefab), chatChannel.info);
            UIChatExtended.singleton.AddMessage(new ChatMessage("", info.identifierIn, "Или нажмите на сообщение, чтобы ответить", "", info.textPrefab), chatChannel.info);
            //UIChatExtended.singleton.AddMessage(new ChatMessage("Someone", whisper.identifierIn, "Are you there?", "/w Someone ", whisper.textPrefab), chatChannel.whisper);
            //UIChatExtended.singleton.AddMessage(new ChatMessage("Someone", local.identifierIn, "Hello!", "/w Someone ", local.textPrefab), chatChannel.info);
        }

        // submit tries to send the string and then returns the new input text
        [Client]
        public string OnSubmit(string text, chatChannel channel)
        {
            // not empty and not only spaces?
            if (!string.IsNullOrWhiteSpace(text))
            {
                string filteredMessage = wordsFilter.CheckMessage(text);

                // command in the commands list?
                // note: we don't do 'break' so that one message could potentially
                //       be sent to multiple channels (see mmorpg local chat)
                string lastcommand = "";

                //whisper
                if (text.StartsWith(whisper.command) || channel == chatChannel.whisper)
                {
                    // whisper
                    string[] parsed = ParsePM(whisper.command, filteredMessage);
                    string user = parsed[0];
                    string msg = parsed[1];
                    if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                    {
                        if (user != name)
                        {
                            lastcommand = whisper.command + " " + user + " ";
                            CmdMsgWhisper(user, msg);
                        }
                        else Debug.Log("cant whisper to self");
                    }
                    else Debug.Log("invalid whisper format: " + user + "/" + msg);
                }

                //for all race
                else if (channel == chatChannel.all)
                {
                    lastcommand = "";
                    CmdMsgLocal(filteredMessage);
                }

                /*else if (channel == chatChannel.general)
                {
                    if (text.StartsWith(gm.command))
                    {
                        Debug.Log("chat is from gm");
                        // gm
                        string msg = ParseGeneral(gm.command, text);
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            lastcommand = gm.command + " ";
                            CmdMsgGM(msg);
                        }
                        else Debug.Log("msg is null");
                    }

                    //trade
                    else if (text.StartsWith(trade.command))
                    {
                        string msg = ParseGeneral(trade.command, text);
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            lastcommand = trade.command + " ";
                            CmdMsgTrade(msg);
                        }
                    }

                    else
                    {
                        // local chat is special: it has no command
                        lastcommand = "";
                        CmdMsgLocal(text);
                    }
                }*/
                else if (channel == chatChannel.gm)
                {
                    lastcommand = gm.command + " ";
                    CmdMsgGM(text);
                }

                // addon system hooks
                UtilsExtended.InvokeMany(typeof(PlayerChat), this, "OnSubmit_", text);

                // input text should be set to lastcommand
                return lastcommand;
            }

            // input text should be cleared
            return "";
        }

        [Command]
        void CmdMsgGM(string message)
        {
            if (message.Length > maxLength) return;

            //if useed addon "GFF GameMasterTool Extended"
            /*if (player.isGameMaster)
            {
                // it's local chat, so let's send it to all observers via ClientRpc
                RpcMsgGM(name, message);
            }*/
        }

        [ClientRpc]
        public void RpcMsgGM(string sender, string message)
        {
            // add message with identifierIn or Out depending on who sent it
            UIChatExtended.singleton.AddMessage(new ChatMessage("", gm.identifierIn, message, "", gm.textPrefab), chatChannel.gm);
        }

        [Command]
        void CmdMsgTrade(string message)
        {
            if (message.Length > maxLength) return;

            // it's local chat, so let's send it to all observers via ClientRpc
            RpcMsgTrade(name, message);
        }

        [ClientRpc]
        public void RpcMsgTrade(string sender, string message)
        {
            // add message with identifierIn or Out depending on who sent it
            string identifier = sender != name ? trade.identifierIn : trade.identifierOut;
            string reply = whisper.command + " " + sender + " "; // whisper
            UIChatExtended.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, trade.textPrefab), chatChannel.trade);
        }
    }
}




