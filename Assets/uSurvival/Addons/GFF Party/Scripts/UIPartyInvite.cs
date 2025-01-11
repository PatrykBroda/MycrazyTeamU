// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public partial class UIPartyInvite : MonoCache
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text nameText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        public override void OnTick()
        {
            Player player = Player.localPlayer;

            if (player != null && player.party.inviteFrom != "")
            {
                panel.SetActive(true);
                nameText.text = player.party.inviteFrom;
                acceptButton.onClick.SetListener(() =>
                {
                    player.party.CmdAcceptInvite();
                });
                declineButton.onClick.SetListener(() =>
                {
                    player.party.CmdDeclineInvite();
                });
            }
            else panel.SetActive(false); // hide
        }
    }
}