using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UICharacterInfo : MonoBehaviour
    {
        public GameObject panel;

        public Text textHealth;
        public Text textHydration;
        public Text textNutrition;
        public Text textEndurance;
        public Text textDamage;
        public Text textDefense;
        public Text textMoveSpeed;
        public Text textLifetime;

        public Text textSelectedKlan;
        public Button buttonKlanExit;

        public Text textGuild;
        public Button buttonGuildReject;

        public Text[] textGroupRelationsName;
        public Text[] textGroupRelationsValue;

        public Text[] combatSkillName;
        public Text[] combatSkillValue;

        public Text[] craftingSkillName;
        public Text[] creftingSkillValue;

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player)
                {
                    textHealth.text = player.health.current + "/" + player.health.max;
                    textHydration.text = player.hydration.current + "/" + player.hydration.max;
                    textNutrition.text = player.nutrition.current + "/" + player.nutrition.max;
                    textEndurance.text = player.endurance.current + "/" + player.endurance.max;
                    textDamage.text = player.combat.damage.ToString();
                    textDefense.text = player.combat.defense.ToString();
                    textMoveSpeed.text = player.movement.runSpeed.ToString();
                    textLifetime.text = UtilsExtended.PrettySeconds((float)player.statistics.lifetime);

                    if (player.combatSkills.skills.Count > 0)
                    {
                        for (int i = 0; i < player.combatSkills.skillTemplates.Length; i++)
                        {
                            combatSkillName[i].text = Localization.Translate(player.combatSkills.skillTemplates[i].name) + " : ";
                            combatSkillValue[i].text = player.combatSkills.skills[i].level + " lv  " + (player.combatSkills.skills[i].GetPercent() * 100).ToString("F2") + "%";
                        }
                    }

                    for (int i = 0; i < player.groups.Count; i++)
                    {
                        //textGroupRelationsName[i].text = Localization.Translate(player.groups[i].clan.ToString()) + " : ";
                        textGroupRelationsValue[i].text = player.groups[i].level + " lv / " + player.groups[i].exp + "%";
                    }

                    //show craft skills
                    for (int i = 0; i < player.craftingExtended.skills.Count; i++)
                    {
                        craftingSkillName[i].text = Localization.Translate(player.craftingExtended.skills[i].name) + " : ";
                        creftingSkillValue[i].text = player.craftingExtended.skills[i].level + " lv  " + (player.craftingExtended.skills[i].GetPercent() * 100).ToString("F2") + "%";
                    }

                    textSelectedKlan.text = Localization.Translate(player.selectedClan.ToString());
                    buttonKlanExit.gameObject.SetActive(player.selectedClan != Clan.none);
                    buttonKlanExit.onClick.SetListener(() =>
                    {
                        player.CmdSetKlan(Clan.none);
                    });

                    textGuild.text = player.guild.InGuild() ? player.guild.guild.name : player.guild.guildWaiting;
                    buttonGuildReject.gameObject.SetActive(player.guild.InGuild());
                    buttonGuildReject.onClick.SetListener(() =>
                    {
                        player.guild.CmdCancellationRequestForJoiningGuild();
                    });
                }
                else panel.SetActive(false);
            }
        }
    }
}


