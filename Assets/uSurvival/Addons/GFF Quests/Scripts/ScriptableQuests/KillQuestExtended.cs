using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

/*[CreateAssetMenu(menuName = "uMMORPG Quest/Kill Quest", order = 999)]
public class GffKillQuest : ScriptableQuest
{
    [Serializable]
    public class Target
    {
        public Monster killTarget;
        public int killAmount;
    }
    public List<Target> Targets = new List<Target>() { };

    // events //////////////////////////////////////////////////////////////////
    public override void OnKilled(Player player, int questIndex, Entity victim)
    {
        // not done yet, and same name as prefab? (hence same monster?)
        Quest quest = player.quests[questIndex];
        if (quest.progress < killAmount && victim.name == killTarget.name)
        {
            // increase int field in quest (up to 'amount')
            ++quest.progress;
            player.quests[questIndex] = quest;
        }
    }

    // fulfillment /////////////////////////////////////////////////////////////
    public override bool IsFulfilled(Player player, Quest quest)
    {
        return quest.progress >= killAmount;
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(Player player, Quest quest)
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));

        for (int i = 0; i < Targets.Count; i++)
        {
            tip.Replace("{KILLTARGET}", Targets[i].killTarget != null ? Targets[i].killTarget.name : "");
            tip.Replace("{KILLAMOUNT}", Targets[i].killAmount.ToString());
        }

        tip.Replace("{KILLED}", quest.progress.ToString());
        return tip.ToString();
    }
}*/
