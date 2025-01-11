using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UINpcGuildManagement : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonRefresh;
        [SerializeField] private Button buttonJoin;

        [Header("Awaiting Approval")]
        [SerializeField] private GameObject panelApproval;
        [SerializeField] private Text textApproval;
        [SerializeField] private Button buttonCancelApproval;

        [Header("Create Guild")]
        [SerializeField] private GameObject panelCreate;
        [SerializeField] private Text createPriceText;
        [SerializeField] private InputField createNameInput;
        [SerializeField] private Button buttonCreate;
        [SerializeField] private Button buttonCreateApply;

        [SerializeField] private UniversalSlot slotPrefab;
        [SerializeField] private Transform storageContent;

        [Header("Terminate Guild")]
        [SerializeField] private GameObject panelTerminate;
        [SerializeField] private Button buttonTerminate;
        [SerializeField] private Button buttonTerminateApply;

        [Header("Panel Info")]
        [SerializeField] private GameObject panelInfo;
        [SerializeField] private Text textInfo;

        private string selectedGuild = "";

        public static UINpcGuildManagement singleton;
        public UINpcGuildManagement()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        private void OnEnable()
        {
            Player player = Player.localPlayer;
            if (player != null) player.guild.CmdLoadAllGuilds();

            InvokeRepeating(nameof(UpdateListAllGuilds), 5, 5);

            buttonClose.onClick.SetListener(() => {
                if (panelTerminate.activeSelf) panelTerminate.SetActive(false);
                else if (panelInfo.activeSelf) panelInfo.SetActive(false);
                else if (panel.activeSelf) panel.SetActive(false);
            });
        }

        private void Update()
        {
            Player player = Player.localPlayer;

            // use collider point(s) to also work with big entities
            if (player != null &&
                player.interactionExtended.target != null &&
                player.interactionExtended.target is Npc &&
                Utils.ClosestDistance(player.collider, player.interactionExtended.target.collider) <= player.interactionRange)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(prefab.gameObject, player.guild.allGuilds.Count, content);

                for (int i = 0; i < player.guild.allGuilds.Count; i++)
                {
                    UIGuildSlot slot = content.GetChild(i).GetComponent<UIGuildSlot>();

                    slot.guildName.text = player.guild.allGuilds[i].name;
                    slot.guildMaster.text = player.guild.allGuilds[i].master;
                    slot.members.text = player.guild.allGuilds[i].members + "/" + GuildSystem.Capacity.ToString();

                    //button
                    slot.button.gameObject.SetActive(!player.guild.InGuild() && string.IsNullOrEmpty(player.guild.guildWaiting));
                    slot.button.onClick.SetListener(() =>
                    {
                        slot.button.transform.GetChild(0).gameObject.SetActive(true);

                        SelectGuild(slot.guildName.text);
                    });
                }

                buttonCreate.gameObject.SetActive(!player.guild.InGuild());
                buttonCreate.onClick.SetListener(() => {
                    if (string.IsNullOrEmpty(player.guild.guildWaiting)) panelCreate.SetActive(true);
                    else panelApproval.SetActive(true);
                });

                buttonRefresh.onClick.SetListener(() =>
                {
                    SelectGuild("");
                    player.guild.CmdLoadAllGuilds();
                });

                //button join
                buttonJoin.gameObject.SetActive(!player.guild.InGuild() && player.guild.allGuilds.Count > 0);
                buttonJoin.onClick.SetListener(() => {
                    if (string.IsNullOrEmpty(player.guild.guildWaiting) == false) panelApproval.SetActive(true);
                    else
                    {
                        if (string.IsNullOrEmpty(selectedGuild) == false)
                        {
                            player.guild.CmdRequestToJoinTheGuild(selectedGuild);
                            SelectGuild("");
                        }
                        else
                        {
                            textInfo.text = "Select guild for Join";
                            panelInfo.SetActive(true);  
                        }
                    }
                });

                //create
                if (panelCreate.activeSelf)
                {
                    createNameInput.characterLimit = GuildSystem.NameMaxLength;
                    createPriceText.text = player.guild.data.creationPrice.ToString();

                    buttonCreateApply.onClick.SetListener(() => {
                        if (player.gold >= player.guild.data.creationPrice)
                        {
                            player.guild.CmdCreate(createNameInput.text);
                            createNameInput.text = ""; // clear the input afterwards
                            panelCreate.SetActive(false);
                        }
                        else
                        {
                            textInfo.text = Localization.Translate("NotEnoughGold");
                            panelInfo.SetActive(true);
                        }
                    });
                }

                buttonTerminate.gameObject.SetActive(player.guild.InGuild() && player.guild.guild.master == player.name);
                if (panelTerminate.activeSelf && !player.guild.InGuild()) panelTerminate.SetActive(false);

                if (panelApproval.activeSelf && !player.guild.InGuild() && !string.IsNullOrEmpty(player.guild.guildWaiting))
                {
                    textApproval.text = Localization.Translate("You are waiting for approval of the guild\n") + "[ " + player.guild.guildWaiting + " ]";
                    buttonCancelApproval.onClick.SetListener(() => {
                        player.guild.CmdCancellationRequestForJoiningGuild();
                    });
                }
                else panelApproval.SetActive(false);
            }
            else panel.SetActive(false);
        }

        public void Show()
        {
            panel.SetActive(true);
        }

        private void SelectGuild(string guildname)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                UIGuildSlot slot = content.GetChild(i).GetComponent<UIGuildSlot>();

                if (slot.guildName.text != guildname) slot.button.transform.GetChild(0).gameObject.SetActive(false);
            }

            selectedGuild = guildname;
        }

        private void UpdateListAllGuilds()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null) player.guild.CmdLoadAllGuilds();
            }
        }
    }
}