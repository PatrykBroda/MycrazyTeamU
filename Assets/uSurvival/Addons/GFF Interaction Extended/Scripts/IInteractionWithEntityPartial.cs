using GFFAddons;
using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Player : Interactable
    {
        [Header("GFF Interaction Extended Addon")]
        public PlayerInteractionExtended interactionExtended;

        void Awake()
        {
            offers = GetComponents<PlayerOffer>();
        }

        public Color GetInteractionColor()
        {
            return Color.black;
        }

        public Entity GetInteractionEntity()
        {
            return this;
        }

        public string InteractionText()
        {
            if (Player.localPlayer != null && health.current > 0)
            {
                return name;
            }
            return "";
        }

        [Client]
        public void OnInteractClient(Player player)
        {
            //show panel with buttons
            UIInteractionWithEntity.singleton.Show();
        }

        [Server]
        public void OnInteractServer(Player fromPlayer)
        {
            // only allowed while not hidden. someone might try to send this Cmd
            // on an item that is currently hidden for respawning, in which case
            // we should not allow anything here
            if (netIdentity.visible == Visibility.ForceHidden)
                return;

            fromPlayer.interactionExtended.SetTarget(netIdentity);
        }

        // cache all NpcOffers
        [HideInInspector] public PlayerOffer[] offers;
    }

    public partial class Storage
    {
        public Entity GetInteractionEntity()
        {
            return null;
        }
    }

    public partial class Furnace
    {
        public Entity GetInteractionEntity()
        {
            return null;
        }
    }

    public partial class ItemDrop
    {
        public Entity GetInteractionEntity()
        {
            return null;
        }
    }

    public partial class Door
    {
        public Entity GetInteractionEntity()
        {
            return null;
        }
    }
}