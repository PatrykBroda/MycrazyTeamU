using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIGuildInvite : MonoCache
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text textName;
        [SerializeField] private Text textGuildName;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        public override void OnTick()
        {
            Player player = Player.localPlayer;

            if (player != null && player.guild.InGuild() == false && player.guild.inviteFrom != "")
            {
                panel.SetActive(true);
                textName.text = player.guild.inviteFrom;
                textGuildName.text = player.guild.inviteGuild;
                acceptButton.onClick.SetListener(() =>
                {
                    player.guild.CmdInviteAccept();
                });
                declineButton.onClick.SetListener(() =>
                {
                    player.guild.CmdInviteDecline();
                });
            }
            else panel.SetActive(false); // hide
        }
    }
}
