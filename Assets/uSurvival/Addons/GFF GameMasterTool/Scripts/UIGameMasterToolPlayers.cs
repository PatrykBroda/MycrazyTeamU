using UnityEngine;
using uSurvival;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace GFFAddons
{
    public class UIGameMasterToolPlayers : MonoBehaviour
    {
        [SerializeField] private Color colorPlayer = Color.white;
        [SerializeField] private Color colorGM = Color.magenta;

        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private UIGameMasterToolPlayerSlot prefab;

        [Header("Components")]
        public UIGameMasterTool gameMasterTool;
        public UIGameMasterToolPlayerInfo playerInfo;

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    UIUtils.BalancePrefabs(prefab.gameObject, player.gameMasterTool.players.Count, content);

                    //sort
                    //show first GMs
                    List<GameMasterToolPlayer> sortedList = player.gameMasterTool.players.ToList();
                    sortedList = sortedList.OrderByDescending(p => p.state).ThenBy(p => p.name).ToList();

                    for (int i = 0; i < sortedList.Count; i++)
                    {
                        UIGameMasterToolPlayerSlot slot = content.transform.GetChild(i).GetComponent<UIGameMasterToolPlayerSlot>();
                        slot.textName.text = sortedList[i].name;
                        slot.textName.color = sortedList[i].state == 0 ? colorPlayer : colorGM;

                        int icopy = i;
                        slot.buttonViewInfo.onClick.SetListener(() =>
                        {
                            player.gameMasterTool.CmdFindPlayerByName(sortedList[icopy].name);
                            playerInfo.SetUpPlayerToSearch(sortedList[icopy].name);

                            //switch to character panel
                            gameMasterTool.SetViewMode(UIGameMasterTool.GameMasterToolMode.CharacterInfo);
                        });

                        slot.buttonGMInteract.GetComponentInChildren<TextMeshProUGUI>().text = sortedList[i].state == 0 ? "Add to GMs" : "Remove from GMs";
                        slot.buttonGMInteract.onClick.SetListener(() =>
                        {
                            if (sortedList[icopy].state == 0)
                            {
                                player.gameMasterTool.CmdSetGM(sortedList[icopy].name, true);
                            }
                            else player.gameMasterTool.CmdSetGM(sortedList[icopy].name, false);
                        });
                    }
                }
            }
        }
    }
}