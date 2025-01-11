using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class IgnoreList
    {
        public string name;
        public bool pm;
        public bool all;
    }

    public class UIIgnoreList : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Button buttonAdd;
        [SerializeField] private Button buttonDelete;

        [SerializeField] private GameObject panelAdd;
        [SerializeField] private InputField addName;
        [SerializeField] private Button buttonAddAplly;
        [SerializeField] private Button buttonAddCancel;

        private int selectedIndex = -1;

        List<IgnoreList> ignoreList = new List<IgnoreList>();

        private void Start()
        {
            LoadIgnoreListFromPlayerPrefs();
        }

        private void Update()
        {
            if (panel.activeSelf)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(prefab, ignoreList.Count, content);

                for (int i = 0; i < ignoreList.Count; i++)
                {
                    UIIgnoreListSlot slot = content.GetChild(i).GetComponent<UIIgnoreListSlot>();

                    slot.textName.text = ignoreList[i].name;
                    slot.togglePM.isOn = ignoreList[i].pm;
                    slot.toggleChat.isOn = ignoreList[i].all;
                    slot.index = i;

                    int icopy = i;
                    slot.button.onClick.SetListener(() =>
                    {
                        selectedIndex = icopy;
                    });

                    slot.togglePM.onValueChanged.SetListener(delegate
                    {
                        ignoreList[icopy].pm = slot.togglePM.isOn;
                        PlayerPrefs.SetInt("ChatExtended_IgnoreListCount_" + icopy + "_pm", ignoreList[icopy].pm.ToInt());
                    });
                    slot.toggleChat.onValueChanged.SetListener(delegate
                    {
                        ignoreList[icopy].all = slot.toggleChat.isOn;
                        PlayerPrefs.SetInt("ChatExtended_IgnoreListCount_" + icopy + "_all", ignoreList[icopy].all.ToInt());
                    });
                }

                buttonAdd.onClick.SetListener(() =>
                {
                    panelAdd.SetActive(true);
                });
                buttonAddAplly.onClick.SetListener(() =>
                {
                    IgnoreList temp = new IgnoreList();
                    temp.name = addName.text;
                    ignoreList.Add(temp);
                    panelAdd.SetActive(false);

                    SaveOnePlayerFromIgnoreList(ignoreList.Count - 1);

                    addName.text = "";
                });
                buttonAddCancel.onClick.SetListener(() =>
                {
                    panelAdd.SetActive(false);
                });

                buttonDelete.interactable = selectedIndex > -1;
                buttonDelete.onClick.SetListener(() =>
                {
                    ignoreList.RemoveAt(selectedIndex);
                    selectedIndex = -1;
                });
            }
        }

        public void Show()
        {
            panel.SetActive(true);
        }

        private void LoadIgnoreListFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey("ChatExtended_IgnoreListCount"))
            {
                int amount = PlayerPrefs.GetInt("ChatExtended_IgnoreListCount");
                if (amount > 0)
                {
                    // fill all slots first
                    for (int i = 0; i < amount; ++i)
                        ignoreList.Add(new IgnoreList());

                    for (int i = 0; i < amount; i++)
                    {
                        ignoreList[i].name = PlayerPrefs.GetString("ChatExtended_IgnoreListCount_" + i + "_name");
                        ignoreList[i].pm = PlayerPrefs.GetInt("ChatExtended_IgnoreListCount_" + i + "_pm").ToBool();
                        ignoreList[i].all = PlayerPrefs.GetInt("ChatExtended_IgnoreListCount_" + i + "_all").ToBool();
                    }
                }
            }
        }

        private void SaveOnePlayerFromIgnoreList(int i)
        {
            PlayerPrefs.SetInt("ChatExtended_IgnoreListCount", ignoreList.Count);

            PlayerPrefs.SetString("ChatExtended_IgnoreListCount_" + i + "_name", ignoreList[i].name);
            PlayerPrefs.SetInt("ChatExtended_IgnoreListCount_" + i + "_pm", ignoreList[i].pm.ToInt());
            PlayerPrefs.SetInt("ChatExtended_IgnoreListCount_" + i + "_all", ignoreList[i].all.ToInt());
        }

        public bool CheckNameForPm(string playername)
        {
            for (int i = 0; i < ignoreList.Count; i++)
            {
                if (ignoreList[i].name == playername)
                {
                    return !ignoreList[i].pm;
                }
            }
            return true;
        }

        public bool CheckNameForAllChat(string playername)
        {
            for (int i = 0; i < ignoreList.Count; i++)
            {
                if (ignoreList[i].name == playername)
                {
                    return !ignoreList[i].all;
                }
            }
            return true;
        }
    }
}