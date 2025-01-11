using UnityEngine;
using Mirror;

namespace uSurvival
{
    public partial class DropRandomItemsOnDeath : NetworkBehaviour
    {
        public ItemDropChance[] dropChances;
        public float radiusMultiplier = 1;
        public int dropSolverAttempts = 3; // attempts to drop without being behind a wall, etc.

        [Server]
        void DropItemAtRandomPosition(GameObject dropPrefab, ushort amount)
        {
            // drop at random point on navmesh that is NOT behind a wall
            // -> dropping behind a wall is just bad gameplay
            // -> on navmesh because that's the easiest way to find the ground
            //    without accidentally raycasting ourselves or something else
            Vector3 position = Utils.ReachableRandomUnitCircleOnNavMesh(transform.position, radiusMultiplier, dropSolverAttempts);

            // drop
            GameObject drop = Instantiate(dropPrefab, position, Quaternion.identity);

            //gff
            drop.GetComponent<ItemDrop>().ownerTime = NetworkTime.time + ownerDurationTime;
            drop.GetComponent<ItemDrop>().destroy = true;
            drop.GetComponent<ItemDrop>().amount = amount;

            NetworkServer.Spawn(drop);
        }

        [Server]
        public void OnDeath()
        {
            foreach (ItemDropChance itemChance in dropChances)
                if (Random.value <= itemChance.probability)
                    DropItemAtRandomPosition(itemChance.drop.gameObject, (ushort)Random.Range(itemChance.minAmount, itemChance.maxAmount));
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            for (int i = 0; i < dropChances.Length; i++)
            {
                if (dropChances[i].drop != null)dropChances[i].name = dropChances[i].drop.name;
            }
        }
    }
}