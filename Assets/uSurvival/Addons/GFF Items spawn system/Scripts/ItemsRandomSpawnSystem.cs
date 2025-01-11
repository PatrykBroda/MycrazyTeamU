using Mirror;
using System;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [Serializable]public class RandomItemDrop
    {
        [HideInInspector]public string name;
        public GameObject item;
        public ushort amount = 1; 
        [Range(0, 1)] public float chance = 1;
    }

    public class ItemsRandomSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private float timeBetweenChecks = 300;
        [SerializeField, Range(0, 1)] public float itemsAmountPercent = 1;
        [SerializeField] private RandomItemDrop[] items;
        [SerializeField] private GameObject[] spawnPoints;
        [SerializeField] private Color colorForGizmo;

        private GameObject[] spawnObjects;

        private void Start()
        {
            if (isServer)
            {
                if (spawnPoints == null || spawnPoints.Length == 0 || itemsAmountPercent == 0) return;
                spawnObjects = new GameObject[spawnPoints.Length];
                InvokeRepeating(nameof(RespawnCheck), 1, timeBetweenChecks);
            }
        }

        private void RespawnCheck()
        {
            int requiredAmount = (int)(spawnPoints.Length * itemsAmountPercent);
            if (SpawnObjectsAmount() < requiredAmount)
            {
                while (SpawnObjectsAmount() < requiredAmount)
                {
                    //find random free place
                    int pointIndex = FindFreePoint();
                    if (pointIndex != -1)
                    {
                        Spawn(pointIndex);
                    }
                    else break;
                }
            }
        }

        private void Spawn(int spawnPointIndex)
        {
            //find random item
            int randomItemIndex = UnityEngine.Random.Range(0, items.Length);

            if (items[randomItemIndex].chance > UnityEngine.Random.Range(0, 1))
            {
                GameObject go = Instantiate(items[randomItemIndex].item, spawnPoints[spawnPointIndex].transform.position, spawnPoints[spawnPointIndex].transform.rotation);
                go.GetComponent<ItemDrop>().amount = items[randomItemIndex].amount;
                NetworkServer.Spawn(go);
                spawnObjects[spawnPointIndex] = go;
            }
        }

        public void FillArray()
        {
            spawnPoints = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnPoints[i] = transform.GetChild(i).gameObject;
            }
        }

        public void Clear()
        {
            spawnPoints = new GameObject[0];
        }

        private int SpawnObjectsAmount()
        {
            int amount = 0;
            for (int i = 0; i < spawnObjects.Length; i++)
            {
                if (spawnObjects[i] != null) amount++;
            }

            return amount;
        }

        private int FindFreePoint()
        {
            for (int i = 0; i < spawnObjects.Length; i++)
            {
                if (spawnObjects[i] == null) return i;
            }

            return -1;
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints.Length == 0) return;

            foreach (GameObject item in spawnPoints)
            {
                Gizmos.color = colorForGizmo;
                Gizmos.DrawSphere(item.transform.position, 0.2f);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].item != null) items[i].name = items[i].item.name;
                else items[i].name = "null";
            }
        }
#endif
    }
}