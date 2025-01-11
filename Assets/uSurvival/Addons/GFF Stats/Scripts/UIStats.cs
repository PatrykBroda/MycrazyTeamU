using GFFAddons;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

public class UIStats : MonoBehaviour
{
    public Button buttonOpen;
    public GameObject panel;
    public Transform content;
    public GameObject prefab;

    private void Start()
    {
        buttonOpen.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (panel.activeSelf)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                UIUtils.BalancePrefabs(prefab, player.statistics.players.Count, content);

                for (int i = 0; i < player.statistics.players.Count; ++i)
                {
                    UIStatsSlot slot = content.GetChild(i).GetComponent<UIStatsSlot>();

                    slot.textName.text = player.statistics.players[i].characterName.ToString();
                    slot.textGender.text = player.statistics.players[i].gender.ToString();
                    slot.textGuild.text = player.statistics.players[i].guild != null ? player.statistics.players[i].guild : "";
                    slot.textLifeTime.text = UtilsExtended.PrettySeconds((int)player.statistics.players[i].lifeTime);
                    slot.textMonstersKilled.text = player.statistics.players[i].monstersKilled.ToString();
                    slot.textPlayersKill.text = player.statistics.players[i].playersKill.ToString();
                }
            }
        }
    }
}
