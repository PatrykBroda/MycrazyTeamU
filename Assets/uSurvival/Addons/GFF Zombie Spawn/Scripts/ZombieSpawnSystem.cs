using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class ZombieSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private GameObject[] spawnPonts;
        [SerializeField] private GameObject[] zombies;
        [SerializeField] private ItemDropChance[] dropChances;
        [SerializeField] private Color colorForGizmo = Color.cyan;

        [SerializeField] private short health = 50;
        [SerializeField] private short damage = 50;
        [SerializeField] private short defense = 50;
        [SerializeField] private float attackInterval = 1;
        [SerializeField] private float moveSpeedMin = 0.8f;
        [SerializeField] private float moveSpeedMax = 1.2f;
        [SerializeField] private float respawnTime = 60;

        private void Start()
        {
            if (isServer)
            {
                for (int i = 0; i < spawnPonts.Length; i++)
                {
                    if (spawnPonts[i] != null && spawnPonts[i].transform.childCount == 0)
                    {
                        int random = Random.Range(0, zombies.Length);
                        GameObject go = Instantiate(zombies[random], spawnPonts[i].transform.position, Quaternion.identity);
                        go.GetComponent<Health>().baseHealth = health;
                        go.GetComponent<Combat>().baseDamage = damage;
                        go.GetComponent<Combat>().baseDefense = defense;
                        go.GetComponent<Monster>().attackInterval = attackInterval;
                        go.GetComponent<Monster>().walkSpeed = go.GetComponent<Monster>().walkSpeed * Random.Range(moveSpeedMin, moveSpeedMax);
                        go.GetComponent<Monster>().runSpeed = go.GetComponent<Monster>().runSpeed * Random.Range(moveSpeedMin, moveSpeedMax);

                        go.GetComponent<DropRandomItemsOnDeath>().dropChances = dropChances;
                        go.GetComponent<OnDeathRespawn>().respawnTime = respawnTime;
                        go.name = zombies[random].name + " / " + gameObject.name;
                        NetworkServer.Spawn(go);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (spawnPonts == null && spawnPonts.Length == 0) return;

            foreach (GameObject item in spawnPonts)
            {
                Gizmos.color = colorForGizmo;
                Gizmos.DrawSphere(item.transform.position, 0.2f);
            }
        }

        public void AddChildGameobjectsToSpawnPoints()
        {
            spawnPonts = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnPonts[i] = transform.GetChild(i).gameObject;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            for (int i = 0;i < dropChances.Length; i++)
            {
                if (dropChances[i].drop != null && dropChances[i].name != dropChances[i].drop.name + " / " + dropChances[i].probability)
                {
                    dropChances[i].name = dropChances[i].drop.name + " / " + dropChances[i].probability + " (" + dropChances[i].minAmount + "/" + dropChances[i].maxAmount + ")";
                }
                else dropChances[i].name = "null";
            }
        }
#endif
    }
}