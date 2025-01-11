using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public partial class PlayerInteractionExtended : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;
        public Health health;

        [Header("Interaction")]
        public float range = 3;
        public KeyCode key = KeyCode.F;

        [SyncVar] public Entity target;

        // interactable that we currently look at. save here so we don't have to
        // raycast again if we need it in UI etc.
        [HideInInspector] public Interactable current;

        // raycast into 'direction', from eyes, with max distance, to check if we
        // look at an interactable. can be used on client to find out what we look
        // at, and on server to find out if the door etc. is actually reachable.
        // (from eyes, not from camera, so that we can't pick items behind a wall
        //  even if a camera is above the player can see the item/door/etc.)
        private Interactable RaycastFindInteractable(Vector3 direction)
        {
            // raycast against ALL layers so that we don't hit an interactable if
            // anything is in front of it, e.g. a wall. otherwise we could pick
            // items through walls, etc.
            // (ignore self in raycasts, otherwise animation might cause cast hits)
            //
            // IMPORTANT: interaction range checks in Cmds need to check
            //            look.headPosition instead of transform.position because
            //            that's what we do here too. otherwise interacting with
            //            objects above the player would be difficult.
            //            => that is why we have an additional Vector3.Dist check
            //               here, so that Cmds get 100% the same results. this is
            //               needed because the raycast stops at the collider, which
            //               is closer than collider.transform.position!
            if (uSurvival.Utils.RaycastWithout(player.look.headPosition, direction, out RaycastHit hit, range, gameObject, player.look.raycastLayers) &&
                Vector3.Distance(player.look.headPosition, hit.transform.position) <= range)
                return hit.transform.GetComponent<Interactable>();
            else
            {
                return null;
            }
        }

        [Server]public void SetTarget(NetworkIdentity ni)
        {
            // validate
            if (ni != null)
            {
                // can directly change it, or change it after casting?
                if (health.current > 0)
                    target = ni.GetComponent<Entity>();
            }
            else target = null;
        }

        [Command]public void CmdSetTargetNull()
        {
            target = null;
        }

        public void SetTargetNull()
        {
            target = null;
        }

        // we pass lookAt position so that the server can do it's own raycast to
        // prevent exploits/cheating.
        // -> PlayerLook does have look directions, but they are highly compressed
        //    and we need the detailed one here. passing the detailed one only once
        //    saves a whole lot of bandwidth.
        // -> we pass the look position, not direction, so that it'll work perfectly
        //    fine even if the player is running and client & server positions were
        //    not 100% the same. (look direction might raycast something completely
        //    different while running)
        [Command]public void CmdInteract(Vector3 lookAt)
        {
            // validate: alive?
            if (health.current > 0)
            {
                // direction := look at position - eyes
                Vector3 direction = (lookAt - player.look.headPosition).normalized;

                // raycast to make sure that we can't pick up items through walls,
                // etc.
                Interactable interactable = RaycastFindInteractable(direction);
                if (interactable != null)
                {
                    if (interactable.GetInteractionEntity() != null)
                    {
                        SetTarget(interactable.GetInteractionEntity().netIdentity);
                    }
                    else interactable.OnInteractServer(player);
                }
                else
                {
                    if (player.isGameMaster)
                    {
                        player.combat.TargetShowErrorInfo("(InterEx)OnServer interactable == null");
                    }
                }
            }
        }

        [ClientCallback]
        private void Update()
        {
            if (!isLocalPlayer) return;

            // raycast into the scene to check if we are looking at interactables
            // only if not over a UI element & not pinching on mobile
            // note: this only works if the UI's CanvasGroup blocks Raycasts
            if (
                //!uSurvival.Utils.IsCursorOverUserInterface() &&
                Input.touchCount <= 1)
            {
                // always save in 'current' for interactable UI etc.
                Vector3 direction = player.look.lookDirectionRaycasted;
                current = RaycastFindInteractable(direction);

                // interactable and pressing the interact key?
                if (current != null && Input.GetKeyDown(key))
                {
                    // call OnInteract on client and server
                    // (some effects like doors are server sided, some effects like
                    //  'open storage UI' are client sided)
                    current.OnInteractClient(player);
                    CmdInteract(player.look.lookPositionRaycasted);
                }
            }

            if (current == null && target != null) CmdSetTargetNull();
        }

        // death ///////////////////////////////////////////////////////////////////
        // universal OnDeath function that takes care of all the Entity stuff.
        // should be called by inheriting classes' finite state machine on death.
        [Server]public virtual void OnDeath()
        {
            // clear target
            target = null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (player == null) player = gameObject.GetComponent<Player>();
            if (health == null) health = gameObject.GetComponent<Health>();
            player.interactionExtended = this;
        }
#endif
    }
}