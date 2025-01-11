using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class MiniMap : NetworkBehaviour
    {
        public Entity entity;

        public Sprite spriteEnemy;
        public Sprite spriteParty;
        public Sprite spriteGuild;

        public SpriteRenderer spriteRenderer;

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (spriteRenderer != null)
                {
                    Player localPlayer = Player.localPlayer;
                    if (localPlayer)
                    {
                        if (entity is Player player)
                        {
                            if (!localPlayer.Equals(player))
                            {
                                if (localPlayer.party.InParty() == false)
                                    spriteRenderer.sprite = null;
                            }
                        }
                        else if (entity is Npc npc)
                        {
                            if (npc.clan != Clan.none)
                            {
                                if (localPlayer.GetGroupLevel(npc.clan) >= npc.minClanRelations)
                                {
                                    spriteRenderer.sprite = spriteParty;
                                }
                                else spriteRenderer.sprite = spriteEnemy;
                            }
                        }
                    }
                }
            }
        }
    }
}


