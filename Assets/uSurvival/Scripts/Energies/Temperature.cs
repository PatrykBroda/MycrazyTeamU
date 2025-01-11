using UnityEngine;

namespace uSurvival
{
    // inventory, attributes etc. can influence max
    public interface ITemperatureBonus
    {
        short TemperatureRecoveryBonus();
    }

    [DisallowMultipleComponent]
    public class Temperature : Energy
    {
        public short baseRecoveryPerTick = -1;

        // degree celsius * 100 so we can lose 0.01°C every second instead of 1°C
        // every second, which should drain it in 36s then
        // (increasing tick rate causes too slow feedback when warming up on a fire)
        // => 3650 equals 36.50°C, which is the normal body temperature
        [SerializeField] short _max = 3650;
        public override short max { get { return _max; } }

        // current heat source: no [SyncVar] because OnTriggerEnter/Exit is called
        // on client and server anyway
        HeatSource currentHeatSource;

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        ITemperatureBonus[] _bonusComponents;
        ITemperatureBonus[] bonusComponents =>
            _bonusComponents ??= GetComponents<ITemperatureBonus>();

        public override short recoveryPerTick
        {
            get
            {
                // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
                short bonus = 0;
                foreach (ITemperatureBonus bonusComponent in bonusComponents)
                    bonus += bonusComponent.TemperatureRecoveryBonus();

                return (short)(baseRecoveryPerTick + bonus + (currentHeatSource ? currentHeatSource.recoveryBonus : 0));
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        void OnTriggerEnter(Collider co)
        {
            // heat source?
            HeatSource heatSource = co.GetComponent<HeatSource>();
            if (heatSource != null)
            {
                // none yet?
                if (currentHeatSource == null)
                {
                    currentHeatSource = heatSource;
                }
                // otherwise keep closest one
                else if (currentHeatSource != heatSource) // different one? otherwise don't bother with calculations
                {
                    float oldDistance = Vector3.Distance(transform.position, currentHeatSource.transform.position);
                    float newDistance = Vector3.Distance(transform.position, heatSource.transform.position);
                    if (newDistance < oldDistance)
                        currentHeatSource = heatSource;
                }
            }
        }

        // [Client] & [Server] so we don't need a SyncVar
        void OnTriggerExit(Collider co)
        {
            // clear current heat source (if any)
            if (currentHeatSource != null && currentHeatSource.transform == co.transform)
                currentHeatSource = null;
        }
    }
}