using GFFAddons;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace uSurvival
{
    public partial class Player
    {
        // npc teleport ////////////////////////////////////////////////////////////
        [Command]
        public void CmdNpcTeleport()
        {
            // validate
            if (health.current > 0 &&
                interactionExtended.target != null && interactionExtended.target is Npc npc &&
                Utils.ClosestDistance(this.collider, npc.collider) <= interactionExtended.range &&
                npc.teleport.destination != null)
            {
                // using Warp is recommended over transform.position
                // (the latter can cause weird bugs when using it with an agent)
                movement.Warp(npc.teleport.destination.position);

                // clear target. no reason to keep targeting the npc after we
                // teleported away from it
                interactionExtended.target = null;
            }
        }
    }
}

namespace GFFAddons
{
    public partial class UIInteractionWithEntity
    {
        public void Update_Npc(Player player, Entity entity)
        {
            if (entity is Npc npc)
            {
                scrollRect.gameObject.SetActive(true);

                // count amount of valid offers
                int validOffers = 0;
                foreach (NpcOffer offer in npc.offers)
                    if (offer.HasOffer(player))
                        ++validOffers;

                if (player.remainMurdererBuff <= 0)
                {
                    //text in scroll rect
                    welcomeText.text = npc.welcome;

                    // instantiate enough buttons
                    UIUtils.BalancePrefabs(offerButtonPrefab, validOffers, offerPanel);

                    // show a button for each valid offer
                    int index = 0;
                    foreach (NpcOffer offer in npc.offers)
                    {
                        if (offer.HasOffer(player))
                        {
                            Button button = offerPanel.GetChild(index).GetComponent<Button>();
                            button.GetComponentInChildren<Text>().text = offer.GetOfferName();
                            button.onClick.SetListener(() => {
                                offer.OnSelect(player);
                            });
                            ++index;
                        }
                    }
                }
                else
                {
                    if (npc.clan == Clan.none) welcomeText.text = "Военные запрещают иметь дела с теми кто не следует их правилам";
                    else if (npc.clan == Clan.Military) welcomeText.text = "Мы не ведем дела с теми кто не следует нашим правилам";

                    // instantiate enough buttons
                    UIUtils.BalancePrefabs(offerButtonPrefab, 0, offerPanel);
                }
            }
        }
    }
}