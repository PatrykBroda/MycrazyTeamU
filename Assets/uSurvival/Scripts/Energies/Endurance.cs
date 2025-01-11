using System;
using System.Collections.Generic;
using UnityEngine;

namespace uSurvival
{
    // inventory, attributes etc. can influence max
    // (no recovery bonus because it makes no sense (physically/biologically)
    public interface IEnduranceBonus
    {
        short EnduranceBonus(short baseEndurance);
    }

    [Serializable]
    public class DrainState
    {
        public MoveState state;
        public int drain;
    }

    [DisallowMultipleComponent]
    public class Endurance : Energy
    {
        [Header("Components")]
        public PlayerMovement movement;

        [Header("Configuration")]
        public short _recoveryPerTick = 1;
        public short baseEndurance = 10;

        public List<DrainState> drainStates = new List<DrainState>{
            new DrainState{state = MoveState.RUNNING, drain = -1},
            new DrainState{state = MoveState.AIRBORNE, drain = -1}
        };

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        IEnduranceBonus[] _bonusComponents;
        IEnduranceBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<IEnduranceBonus>();

        public override short max
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (IEnduranceBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.EnduranceBonus(baseEndurance);

                return (short)(baseEndurance + bonus);
            }
        }

        public bool IsInDrainState(out DrainState drainState)
        {
            // search manually. Linq.Find is HEAVY(!) on GC and performance
            foreach (DrainState drain in drainStates)
            {
                if (drain.state == movement.state)
                {
                    drainState = drain;
                    return true;
                }
            }
            drainState = null;
            return false;
        }

        // in a state that drains it? otherwise recover
        public override short recoveryPerTick =>
            IsInDrainState(out DrainState drainState)
                ? (short)drainState.drain
                : _recoveryPerTick;
    }
}