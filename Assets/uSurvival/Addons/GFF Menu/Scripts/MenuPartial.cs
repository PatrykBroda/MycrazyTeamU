using GFFAddons;
using TMPro;
using UnityEngine;

namespace uSurvival
{
    public partial class Player
    {
        void LateUpdate_Options()
        {
            if (NetworkManagerSurvival.singleton.gameObject.GetComponent<NetworkManagerSurvival>().state == NetworkState.World)
            {
                //nameOverlay.gameObject.SetActive(UIMenuOptions._showPlayersNames);
            }
        }
    }

    //public partial class Monster
    //{
    //    [Header("GFF Menu Addon")]
    //    public TextMeshPro nameobject;

    //    void LateUpdate_Options()
    //    {
    //        if (nameobject != null) nameobject.enabled = SettingsLoader._showMonstersNames;
    //    }
    //}
}



namespace GFFAddons
{
    public partial class Npc
    {
        [Header("GFF Menu Addon")]
        public TextMeshPro nameobject;

        void UpdateClient_Options()
        {
            if (nameobject != null) nameobject.enabled = SettingsLoader._showNpcNames;
        }
    }
}
