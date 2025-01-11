// a simple gather quest example
using System.Text;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "uMMORPG Quest/Gather Quest", order = 999)]
    public partial class GatherQuest : ScriptableQuest
    {
        [Header("Fulfillment")]
        //public ScriptableItem gatherItem;
        //public int gatherAmount;

        public ScriptableItemAndAmount[] gatherItems;

        // fulfillment /////////////////////////////////////////////////////////////
        public override bool IsFulfilled(Player player, Quest quest)
        {
            bool fulfilled = true;
            for (int i = 0; i < gatherItems.Length; i++)
            {
                if (gatherItems[i].item != null && player.inventory.Count(new Item(gatherItems[i].item)) < gatherItems[i].amount)
                {
                    fulfilled = false;
                    break;
                }
            }
            return fulfilled;
        }

        public override void OnCompleted(Player player, Quest quest)
        {
            // remove gathered items from player's inventory
            for (int i = 0; i < gatherItems.Length; i++)
            {
                if (gatherItems[i].item != null)
                {
                    player.inventory.Remove(new Item(gatherItems[i].item), gatherItems[i].amount);
                }
            }
        }

        // tooltip /////////////////////////////////////////////////////////////////
        public override string ToolTip(Player player, Quest quest)
        {
            // we use a StringBuilder so that addons can modify tooltips later too
            // ('string' itself can't be passed as a mutable object)
            StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));
            /*tip.Replace("{GATHERAMOUNT}", gatherAmount.ToString());
            if (gatherItem != null)
            {
                int gathered = player.inventory.Count(new Item(gatherItem));
                tip.Replace("{GATHERITEM}", gatherItem.name);
                tip.Replace("{GATHERED}", Mathf.Min(gathered, gatherAmount).ToString());
            }*/
            return tip.ToString();
        }
    }
}


