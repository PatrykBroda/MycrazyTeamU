using Mirror;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UIInteractionWithEntity : MonoBehaviour
    {
        [Header("Original")]
        [SerializeField] private GameObject panelOriginal;
        [SerializeField] private TextMeshProUGUI hotkeyText;
        [SerializeField] private TextMeshProUGUI actionText;

        [Header("Extended")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonCancel;
        [SerializeField] private GameObject[] windowsThatHideInteraction; //Main UiPanel

        [Header("For Npc")]
        [SerializeField] private GameObject scrollRect;
        [SerializeField] private Text welcomeText;
        [SerializeField] private GameObject offerButtonPrefab;
        [SerializeField] private Transform offerPanel;

        public static UIInteractionWithEntity singleton;
        public UIInteractionWithEntity()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            //if (singleton == null)
            singleton = this;
        }

        public void Show()
        {
            panel.SetActive(true);
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null && player.interactionExtended.current != null && !AnyLockWindowActive())
            {
                Entity entity = player.interactionExtended.current.GetInteractionEntity();

                if (entity == null || player.interactionExtended.target == null)
                {
                    hotkeyText.text = player.interactionExtended.key.ToString();
                    actionText.text = player.interactionExtended.current.InteractionText();

                    //rarity addon
                    //panel.GetComponent<Image>().color = player.interaction.current.GetInteractionColor();

                    panelOriginal.SetActive(true);
                    panel.SetActive(false);
                }
                else
                {
                    buttonCancel.onClick.SetListener(() =>
                    {
                        player.interactionExtended.CmdSetTargetNull();
                        panel.SetActive(false);
                    });

                    if (entity is Player targetPlayer)
                    {
                        scrollRect.gameObject.SetActive(false);

                        // count amount of valid offers
                        int validOffers = 0;
                        foreach (PlayerOffer offer in targetPlayer.offers)
                            if (offer.HasOffer(player))
                                ++validOffers;

                        // instantiate enough buttons
                        UIUtils.BalancePrefabs(offerButtonPrefab, validOffers, offerPanel);

                        // show a button for each valid offer
                        int index = 0;
                        foreach (PlayerOffer offer in targetPlayer.offers)
                        {
                            if (offer.HasOffer(player))
                            {
                                Button button = offerPanel.GetChild(index).GetComponent<Button>();
                                button.GetComponentInChildren<Text>().text = offer.GetOfferName(player);
                                button.interactable = NetworkTime.time >= player.nextRiskyActionTime;
                                button.onClick.SetListener(() => {
                                    offer.OnSelect(player, targetPlayer);
                                });
                                ++index;
                            }
                        }
                    }

                    UtilsExtended.InvokeMany(typeof(UIInteractionWithEntity), this, "Update_", player, entity);

                    panelOriginal.SetActive(false);
                    panel.SetActive(true);
                }
            }
            else
            {
                panelOriginal.SetActive(false);
                panel.SetActive(false);
            }
        }

        private bool AnyLockWindowActive()
        {
            // check manually. Linq.Any() is HEAVY(!) on GC and performance
            foreach (GameObject go in windowsThatHideInteraction)
                if (go.activeSelf)
                    return true;
            return false;
        }
    }
}