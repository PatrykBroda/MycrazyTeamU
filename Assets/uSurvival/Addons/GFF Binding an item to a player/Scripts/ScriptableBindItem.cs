using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uSurvival Item/Bind Item", order = 999)]
    public class ScriptableBindItem : UsableItem
    {
        // [Client] OnUse Rpc callback for effects, sounds, etc.
        // -> can't pass Inventory+slotIndex because .Use might clear it before getting here already
        // -> should always simulate a Use() again to decide which sounds to play etc.,
        //    so that we can simulate it for local player to avoid latency effects.
        //    (passing a 'result' bool wouldn't allow us to call OnUsed without Use() theN)
        public override void OnUsedInventory(Player player)
        {
            if (player.isLocalPlayer) UIItemBinding.singleton.panel.SetActive(true);
        }
    }
}