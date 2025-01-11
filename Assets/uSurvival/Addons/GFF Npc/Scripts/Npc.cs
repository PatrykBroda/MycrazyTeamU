using Mirror;
using UnityEngine;
using UnityEngine.AI;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public partial class Npc : Entity, Interactable
    {
        // Used components. Assign in Inspector. Easier than GetComponent caching.
        [Header("Components")]
        public NavMeshAgent agent;
        public AudioSource audioSource;
        public NpcTeleport teleport;

        [Header("Welcome Text")]
        [TextArea(1, 30)] public string welcome;

        // cache all NpcOffers
        [HideInInspector] public NpcOffer[] offers;

        [Header("Movement")]
        public float walkSpeed = 1;
        public float runSpeed = 5;
        [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
        public float moveDistance = 10;

        [Header("Running")]
        [Range(0f, 1f)] public float runStepLength = 0.7f;
        public double runStepInterval = 3;
        public float runCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        float stepCycle;
        double nextStep;

        [Header("Attack")]
        public float followDistance = 20;
        [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.5f; // move as close as 0.5 * attackRange to a target
        public float attackRange = 2;
        public float attackInterval = 0.5f; // how long one attack takes (seconds): ideally the attack animation time
        double attackEndTime;  // double for long term precision
        public AudioClip attackSound;
        public MuzzleFlash muzzleFlash;

        [Header("Difficulty")]
        [Tooltip("If a player runs out of attack range while an attack is in progress and kiting is enabled, then the attack will be canceled. If kiting is disabled, then the attack will always finish once it started, even if the player ran out of range.")]
        public bool allowKiting = false;

        [Header("Debug")]
        [Tooltip("Debug GUI visibility curve. X axis = distance, Y axis = alpha. Nothing will be displayed if Y = 0.")]
        public AnimationCurve debugVisibilityCurve = new AnimationCurve(new Keyframe(0, 0.3f), new Keyframe(15, 0.3f), new Keyframe(20, 0f));

        [Header("GFF Idle Sounds Addon")]
        public AudioSource idleAudio;
        public float idleTimeWaitingMin = 30;
        public float idleTimeWaitingMax = 90;
        public AudioClip[] idleSounds;
        private double nextIdleSound;

        [Header("Sounds")]
        public AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
        public AudioSource feetAudio;

        // state
        [SyncVar] public string state = "IDLE";

//#pragma warning disable CS0109 // member does not hide accessible member
//        new Camera camera;
//#pragma warning restore CS0109 // member does not hide accessible member

        // 'Entity' can't be SyncVar and NetworkIdentity causes errors when null,
        // so we use [SyncVar] GameObject and wrap it for simplicity
        [Header("Target")]
        [SyncVar] GameObject _target;
        public Entity target
        {
            get { return _target != null ? _target.GetComponent<Entity>() : null; }
            set { _target = value != null ? value.gameObject : null; }
        }

        [SerializeField] private Animator animator;

        // save the start position for random movement distance and respawning
        [HideInInspector] public Vector3 startPosition;

        void Awake()
        {
            offers = GetComponents<NpcOffer>();
            //camera = Camera.main;
        }

        void Start()
        {
            // remember start position in case we need to respawn later
            startPosition = transform.position;
        }

        public override void OnStartServer()
        {
            // change to dead if we spawned with 0 health
            if (health.current == 0) state = "DEAD";
        }
        public override void OnStartClient()
        {
            base.OnStartClient();

            nextIdleSound = NetworkTime.time + Random.Range(idleTimeWaitingMin, idleTimeWaitingMax);
        }

        // visibility //////////////////////////////////////////////////////////////
        // is the entity currently hidden?
        // note: usually the server is the only one who uses forceHidden, the
        //       client usually doesn't know about it and simply doesn't see the
        //       GameObject.
        public bool IsHidden() => netIdentity.visible == Visibility.ForceHidden;

        // monsters, npcs etc. don't have to be updated if no player is around
        // checking observers is enough, because lonely players have at least
        // themselves as observers, so players will always be updated
        // and dead monsters will respawn immediately in the first update call
        // even if we didn't update them in a long time (because of the 'end'
        // times)
        // -> update only if:
        //    - observers are null (they are null in clients)
        //    - if they are not null, then only if at least one (on server)
        //    - if the entity is hidden, otherwise it would never be updated again
        //      because it would never get new observers
        public bool IsWorthUpdating() =>
            netIdentity.observers == null ||
            netIdentity.observers.Count > 0 ||
            IsHidden();

        // entity logic will be implemented with a finite state machine
        // -> we should react to every state and to every event for correctness
        // -> we keep it functional for simplicity
        // note: can still use LateUpdate for Updates that should happen in any case
        private void Update()
        {
            // only update if it's worth updating (see IsWorthUpdating comments)
            // -> we also clear the target if it's hidden, so that players don't
            //    keep hidden (respawning) monsters as target, hence don't show them
            //    as target again when they are shown again
            if (IsWorthUpdating())
            {
                if (isClient) UpdateClient();
                if (isServer)
                {
                    if (target != null && target.netIdentity.visible == Visibility.ForceHidden) target = null;
                    state = UpdateServer();
                }
            }
        }

        // animations //////////////////////////////////////////////////////////////
        [ClientCallback] // no need for animations on the server
        void LateUpdate()
        {
            // pass parameters to animation state machine
            // => passing the states directly is the most reliable way to avoid all
            //    kinds of glitches like movement sliding, attack twitching, etc.
            // => make sure to import all looping animations like idle/run/attack
            //    with 'loop time' enabled, otherwise the client might only play it
            //    once
            // => only play moving animation while the agent is actually moving. the
            //    MOVING state might be delayed to due latency or we might be in
            //    MOVING while a path is still pending, etc.
            // now pass parameters after any possible rebinds
            if (animator != null)
            {
                animator.SetBool("MOVING", state == "MOVING" && agent.velocity != Vector3.zero);
                animator.SetFloat("Speed", agent.speed);
                animator.SetBool("ATTACKING", state == "ATTACKING");
                animator.SetBool("DEAD", state == "DEAD");
            }
            else
            {
                foreach (Animator anim in GetComponentsInChildren<Animator>())
                {
                    anim.SetBool("MOVING", state == "MOVING" && agent.velocity != Vector3.zero);
                    anim.SetFloat("Speed", agent.speed);
                    anim.SetBool("ATTACKING", state == "ATTACKING");
                    anim.SetBool("DEAD", state == "DEAD");
                }
            }
        }

        // note: client can find out if moving by simply checking the state!
        // -> agent.hasPath will be true if stopping distance > 0, so we can't
        //    really rely on that.
        // -> pathPending is true while calculating the path, which is good
        // -> remainingDistance is the distance to the last path point, so it
        //    also works when clicking somewhere onto a obstacle that isn'
        //    directly reachable.
        public bool IsMoving() =>
            agent.pathPending ||
            agent.remainingDistance > agent.stoppingDistance ||
            agent.velocity != Vector3.zero;

        // finite state machine events /////////////////////////////////////////////
        bool EventTargetDisappeared() =>
            target == null;

        bool EventTargetDied() =>
            target != null && target.health.current == 0;

        bool EventTargetTooFarToAttack() =>
            target != null &&
            uSurvival.Utils.ClosestDistance(collider, target.collider) > attackRange;

        bool EventTargetTooFarToFollow() =>
            target != null &&
            Vector3.Distance(startPosition, target.collider.ClosestPoint(transform.position)) > followDistance;

        bool EventAggro() =>
            target != null && target.health.current > 0;

        bool EventAttackFinished() =>
            NetworkTime.time >= attackEndTime;

        bool EventMoveEnd() =>
            state == "MOVING" && !IsMoving();

        bool EventMoveRandomly() =>
            Random.value <= moveProbability * Time.deltaTime;

        // finite state machine - server ///////////////////////////////////////////
        [Server]
        string UpdateServer_IDLE()
        {
            // calculating a path before going to MOVING? then just wait.
            if (agent.pathPending) return "IDLE";
            // calculated a path? then go to MOVING now!
            if (agent.hasPath) return "MOVING";

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventTargetDied())
            {
                // we had a target before, but it died now. clear it.
                target = null;
                return "IDLE";
            }
            if (EventTargetTooFarToFollow())
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to start. don't stay here.
                target = null;
                agent.speed = walkSpeed;
                agent.stoppingDistance = 0;
                agent.destination = startPosition;
                // wait while path is pending! if we go to MOVING directly and end
                // up not finding a path then we could constantly switch between
                // IDLE->MOVING->IDLE->... if the target is behind a wall with no
                // entry, causing strange behaviour and constant resync!
                return "IDLE";
            }
            if (EventTargetTooFarToAttack() || IsTargetInRangeButNotReachable())
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                agent.speed = runSpeed;
                // 0 stopping distance! try to run exactly to target position and
                // only stop if close enough AND reachable. we don't want to stop
                // only if close enough, otherwise we might stand e.g. in front of
                // a fence but don't reach the target behind the fence. it's smarter
                // to always try to finish the path (e.g. to behind the fence) until
                // reachable.
                agent.stoppingDistance = 0;
                agent.destination = target.collider.ClosestPoint(transform.position);
                // wait while path is pending! if we go to MOVING directly and end
                // up not finding a path then we could constantly switch between
                // IDLE->MOVING->IDLE->... if the target is behind a wall with no
                // entry, causing strange behaviour and constant resync!
                return "IDLE";
            }
            if (EventAggro())
            {
                // target in attack range. try to attack it
                // -> start attack timer and go to casting
                attackEndTime = NetworkTime.time + attackInterval;
                RpcOnAttackStarted();
                return "ATTACKING";
            }
            if (EventMoveRandomly())
            {
                // walk to a random position in movement radius (from 'start')
                // note: circle y is 0 because we add it to start.y
                Vector2 circle2D = Random.insideUnitCircle * moveDistance;
                agent.speed = walkSpeed;
                agent.stoppingDistance = 0;
                agent.destination = startPosition + new Vector3(circle2D.x, 0, circle2D.y);
                return "MOVING";
            }
            if (EventMoveEnd()) { } // don't care
            if (EventTargetDisappeared()) { } // don't care

            return "IDLE"; // nothing interesting happened
        }

        [Server]
        string UpdateServer_MOVING()
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventMoveEnd())
            {
                // we reached our destination.
                return "IDLE";
            }
            if (EventTargetDied())
            {
                // we had a target before, but it died now. clear it.
                target = null;
                agent.ResetMovement();
                return "IDLE";
            }
            if (EventTargetTooFarToFollow())
            {
                // we had a target before, but it's out of follow range now.
                // clear it and go back to start. don't stay here.
                target = null;
                agent.speed = walkSpeed;
                agent.stoppingDistance = 0;
                agent.destination = startPosition;
                return "MOVING";
            }
            if (EventTargetTooFarToAttack() || IsTargetInRangeButNotReachable())
            {
                // we had a target before, but it's out of attack range now.
                // follow it. (use collider point(s) to also work with big entities)
                agent.speed = runSpeed;
                // 0 stopping distance! try to run exactly to target position and
                // only stop if close enough AND reachable. we don't want to stop
                // only if close enough, otherwise we might stand e.g. in front of
                // a fence but don't reach the target behind the fence. it's smarter
                // to always try to finish the path (e.g. to behind the fence) until
                // reachable.
                agent.stoppingDistance = 0;
                agent.destination = target.collider.ClosestPoint(transform.position);
                return "MOVING";
            }
            if (EventAggro())
            {
                // the target is close, but we are probably moving towards it already
                // so let's just move a little bit closer into attack range so that
                // we can keep attacking it if it makes one step backwards
                //
                // .. AND ..
                //
                // dont stop moving until it's actually reachable. e.g. if the
                // target is behind a fence, it's not enough to stop in attackrange
                // in front of the fence. we need to move behind the fence until we
                // are in range and reachable.
                Collider targetCollider = target.collider;
                if (Vector3.Distance(transform.position, targetCollider.ClosestPoint(transform.position)) <= attackRange * attackToMoveRangeRatio &&
                    uSurvival.Utils.IsReachableVertically(collider, targetCollider, attackRange))
                {
                    // target in attack range. try to attack it
                    // -> start attack timer and go to casting
                    // (we may get a target while randomly wandering around)
                    attackEndTime = NetworkTime.time + attackInterval;
                    agent.ResetMovement();
                    RpcOnAttackStarted();
                    return "ATTACKING";
                }
            }
            if (EventAttackFinished()) { } // don't care
            if (EventTargetDisappeared()) { } // don't care
            if (EventMoveRandomly()) { } // don't care

            return "MOVING"; // nothing interesting happened
        }

        [Server]
        string UpdateServer_ATTACKING()
        {
            // calculating a path before going to MOVING? then just wait.
            if (agent.pathPending) return "ATTACKING";
            // calculated a path? then go to MOVING now!
            if (agent.hasPath) return "MOVING";

            // keep looking at the target for server & clients (only Y rotation)
            if (target) LookAtY(target.transform.position);

            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventTargetDisappeared())
            {
                // target disappeared, stop attacking
                target = null;
                return "IDLE";
            }
            if (EventTargetDied())
            {
                // target died, stop attacking
                target = null;
                return "IDLE";
            }
            if (EventAttackFinished())
            {
                // finished attacking. apply the damage on the target
                combat.DealDamageAt(target, combat.damage, target.transform.position, -transform.forward, target.collider);

                // did the target die? then clear it so that the monster doesn't
                // run towards it if the target respawned
                if (target.health.current == 0)
                    target = null;

                // go back to IDLE
                return "IDLE";
            }
            if (EventMoveEnd()) { } // don't care
            if (EventTargetTooFarToAttack())
            {
                if (allowKiting)
                {
                    // allow players to kite/dodge attacks by running far away. most
                    // people want this feature in survival games (unlike MMOs where
                    // kiting is protected against)
                    // => run closer to target if out of range now
                    agent.speed = runSpeed;
                    agent.stoppingDistance = attackRange * attackToMoveRangeRatio;
                    agent.destination = target.collider.ClosestPoint(transform.position);
                    // wait while path is pending! if we go to MOVING directly and end
                    // up not finding a path then we could constantly switch between
                    // IDLE->MOVING->IDLE->... if the target is behind a wall with no
                    // entry, causing strange behaviour and constant resync!
                    return "ATTACKING";
                }
            }
            if (EventTargetTooFarToFollow())
            {
                if (allowKiting)
                {
                    // allow players to kite/dodge attacks by running far away. most
                    // people want this feature in survival games (unlike MMOs where
                    // kiting is protected against)
                    // => way too far to even run there, so let's cancel the attack and
                    //    go back to start. don't stay here.
                    target = null;
                    agent.speed = walkSpeed;
                    agent.stoppingDistance = 0;
                    agent.destination = startPosition;
                    // wait while path is pending! if we go to MOVING directly and end
                    // up not finding a path then we could constantly switch between
                    // IDLE->MOVING->IDLE->... if the target is behind a wall with no
                    // entry, causing strange behaviour and constant resync!
                    return "ATTACKING";
                }
            }
            if (EventAggro()) { } // don't care, always have aggro while attacking
            if (EventMoveRandomly()) { } // don't care

            return "ATTACKING"; // nothing interesting happened
        }

        [Server]
        string UpdateServer_DEAD()
        {
            // events sorted by priority (e.g. target doesn't matter if we died)
            if (EventAttackFinished()) { } // don't care
            if (EventMoveEnd()) { } // don't care
            if (EventTargetDisappeared()) { } // don't care
            if (EventTargetDied()) { } // don't care
            if (EventTargetTooFarToFollow()) { } // don't care
            if (EventTargetTooFarToAttack()) { } // don't care
            if (EventAggro()) { } // don't care
            if (EventMoveRandomly()) { } // don't care

            return "DEAD"; // nothing interesting happened
        }

        [Server]
        string UpdateServer()
        {
            if (state == "IDLE") return UpdateServer_IDLE();
            if (state == "MOVING") return UpdateServer_MOVING();
            if (state == "ATTACKING") return UpdateServer_ATTACKING();
            if (state == "DEAD") return UpdateServer_DEAD();
            Debug.LogError("invalid state: " + state);
            return "IDLE";
        }

        //interaction
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

        [Client]public void OnInteractClient(Player player)
        {
            //show panel with buttons
            UIInteractionWithEntity.singleton.Show();
        }

        [Server]public void OnInteractServer(Player fromPlayer)
        {
            // only allowed while not hidden. someone might try to send this Cmd
            // on an item that is currently hidden for respawning, in which case
            // we should not allow anything here
            if (netIdentity.visible == Visibility.ForceHidden)
                return;

            fromPlayer.interactionExtended.SetTarget(netIdentity);
        }

        // events //////////////////////////////////////////////////////////////////
        [Server]public void OnDeath()
        {
            state = "DEAD";

            // stop any movement
            agent.ResetMovement();

            // clear target
            target = null;
        }

        [Server]public void OnRespawn()
        {
            // respawn at start position
            // (always use Warp instead of transform.position for NavMeshAgents)
            agent.Warp(startPosition);

            state = "IDLE";
        }

        // finite state machine - client ///////////////////////////////////////////
        [Client]
        void UpdateClient()
        {
            if (state == "ATTACKING")
            {
                // keep looking at the target for server & clients (only Y rotation)
                if (target) LookAtY(target.transform.position);
            }
            else if (state == "MOVING")
            {
                ProgressStepCycle();
            }
            else if (state == "IDLE")
            {
                if (idleAudio && idleSounds.Length > 0) IdleSounds();
            }
        }

        // helper functions ////////////////////////////////////////////////////////
        // look at a transform while only rotating on the Y axis (to avoid weird
        // tilts)
        private void LookAtY(Vector3 position)
        {
            transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
        }

        // helper function to check if the target is in attack range, but not reachable
        // -> we only care about being reachable while in attack range, since it
        //    means nothing if we are 10m away and there is a tree inbetween.
        private bool IsTargetInRangeButNotReachable()
        {
            if (target != null)
            {
                Collider targetCollider = target.collider;
                return uSurvival.Utils.ClosestDistance(collider, targetCollider) <= attackRange &&
                       !uSurvival.Utils.IsReachableVertically(collider, targetCollider, attackRange);
            }
            return false;
        }

        // this function is called by people who attack us. simply forwarded to
        // OnAggro.
        [Server]public void OnReceivedDamage(Entity attacker, int damage)
        {
            OnAggro(attacker);
        }

        // aggro ///////////////////////////////////////////////////////////////////
        // this function is called by AggroArea
        [ServerCallback]
        public void OnAggro(Entity entity)
        {
            // can we attack that type?
            if (entity != null && CanAttack(entity))
            {
                // no target yet(==self), or closer than current target?
                // => has to be at least 20% closer to be worth it, otherwise we
                //    may end up nervously switching between two targets
                // => we also switch if current target is unreachable and we found
                //    a new target that is reachable, even if it's further away
                // => we do NOT use Utils.ClosestDistance, because then we often
                //    also end up nervously switching between two animated targets,
                //    since their collides moves with the animation.
                //    => we don't even need closestdistance here because they are in
                //       the aggro area anyway. transform.position is perfectly fine
                if (target == null)
                {
                    target = entity;
                }
                else if (target != entity) // different one? evaluate if we should target it
                {
                    // select closest target, but also always select the reachable
                    // one if the current one is unreachable.
                    float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                    float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                    if ((newDistance < oldDistance * 0.8) ||
                        (!uSurvival.Utils.IsReachableVertically(collider, target.collider, attackRange) &&
                         uSurvival.Utils.IsReachableVertically(collider, entity.collider, attackRange)))
                    {
                        target = entity;
                    }
                }
            }
        }

        // attack rpc //////////////////////////////////////////////////////////////
        [ClientRpc]
        public void RpcOnAttackStarted()
        {
            // play sound (if any)
            if (attackSound) audioSource.PlayOneShot(attackSound);
            ShowMuzzleFlash();
        }

        // check if we can attack someone else
        private bool CanAttack(Entity entity)
        {
            return (entity is Monster || (entity is Player player && player.selectedClan != Clan.none && player.selectedClan != clan)) &&
                   entity.health.current > 0 &&
                   health.current > 0;
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Fire();
            }
        }

        //sounds 
        private void IdleSounds()
        {
            if (NetworkTime.time > nextIdleSound && idleAudio.isPlaying == false)
            {
                nextIdleSound = NetworkTime.time + Random.Range(idleTimeWaitingMin, idleTimeWaitingMax);
                PlayIdleAudio();
            }
        }

        private void PlayIdleAudio()
        {
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(0, idleSounds.Length);
            idleAudio.clip = idleSounds[n];
            if (idleAudio.clip != null) idleAudio.PlayOneShot(idleAudio.clip);
        }

        private void PlayFootStepAudio()
        {
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footstepSounds.Length);
            feetAudio.clip = footstepSounds[n];
            if (feetAudio.clip != null) feetAudio.PlayOneShot(feetAudio.clip);

            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = feetAudio.clip;
        }

        private void ProgressStepCycle()
        {
            /*if (controller.velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
            {
                stepCycle += (controller.velocity.magnitude + (speed * (state == "MOVING" ? 1 : runStepLength))) *
                             Time.fixedDeltaTime;
            }*/

            if (NetworkTime.time > nextStep)
            {
                nextStep = NetworkTime.time + runStepInterval;
                if (footstepSounds != null && footstepSounds.Length > 0) PlayFootStepAudio();
            }
        }

        // OnDrawGizmos only happens while the Script is not collapsed
        void OnDrawGizmos()
        {
            // draw the movement area (around 'start' if game running,
            // or around current position if still editing)
            Vector3 startHelp = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startHelp, moveDistance);

            // draw the follow dist
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(startHelp, followDistance);
        }
    }
}