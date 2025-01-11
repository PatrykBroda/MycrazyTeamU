using GFFAddons;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace uSurvival
{
    public partial class Player
    {
        [Header("GFF GameMaster Extended")]
        public PlayerGameMasterTool gameMasterTool;
        public string nameOverlayGameMasterPrefix = "[GM] ";

        // keep the GM flag in here and the controls in PlayerGameMaster.cs:
        // -> we need the flag for NameOverlay prefix anyway
        // -> it might be needed outside of PlayerGameMaster for other GM specific
        //    mechanics/checks later
        // -> this way we can use SyncToObservers for the flag, and SyncToOwner for
        //    everything else in PlayerGameMaster component. this is a LOT easier.
        [SyncVar] public bool isGameMaster = false;
    }

    public partial class Database
    {
        public void BanAccount(string account)
        {
            connection.Execute("UPDATE accounts SET banned = 1 WHERE name=?", account);
        }
    }

    public partial class UICharacterCreation
    {
        public Toggle toggleGameMaster;
    }

    public partial class Energy
    {
        [Header("Components")]
        public Entity entity;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (entity == null) entity = gameObject.GetComponent<Entity>();
        }
#endif
    }
}

namespace GFFAddons
{
    public struct GameMasterMessage
    {
        public string targetName;
        public bool isOnline;

        public string healthStatus;
        public float healthValue;
        public short healthMax;

        public string hydrationStatus;
        public float hydrationValue;
        public short hydrationMax;

        public string nutritionStatus;
        public float nutritionValue;
        public short nutritionMax;

        public float temperatureValue;
        public int temperatureCurrent;
        public short temperatureMax;

        public string enduranceStatus;
        public float enduranceValue;
        public short enduranceMax;

        public List<ItemSlot> inventory;
        public List<ItemSlot> equipment;
        public List<string> equipmentCategory;

        public int gold;
        public int coins;
    }
}