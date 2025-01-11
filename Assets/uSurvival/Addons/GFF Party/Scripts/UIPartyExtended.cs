using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIPartyExtended : MonoCache
    {
        [Header("Settings")]
        [SerializeField] private bool allowChangeLeader;
        [SerializeField] private bool allowKickFromParty;

        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button button;
        [SerializeField] private UIPartyExtendedMemberSlot prefab;
        [SerializeField] private Text textPartyAmount;
        [SerializeField] private Transform memberContent;
        [SerializeField] private AnimationCurve alphaCurve;

        [Header("Components")]
        [SerializeField] private AudioSource audioSource;

        //singleton
        public static UIPartyExtended singleton;
        public UIPartyExtended()
        {
            singleton = this;
        }

        public void Show(){panel.SetActive(true);}

        public void Hide(){panel.SetActive(false);}

        private void Start()
        {
            button.onClick.SetListener(() =>
            {
                audioSource.Play();
                panel.SetActive(!panel.activeSelf);
            });
        }

        public override void OnTick()
        {
            Player player = Player.localPlayer;

            // only show and update while there are party members
            if (player != null)
            {
                button.gameObject.SetActive(player.party.InParty()); 

                if (player.party.InParty())
                {
                    Party party = player.party.party;

                    //party amount
                    textPartyAmount.text = Localization.Translate("Party") + ": " + party.members.Length + " / " + Party.Capacity;

                    // get party members without self. no need to show self in HUD too.
                    List<string> members = party.members.Where(m => m != player.name).ToList();

                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefab.gameObject, members.Count, memberContent);

                    // refresh all members
                    for (int i = 0; i < members.Count; ++i)
                    {
                        UIPartyExtendedMemberSlot slot = memberContent.GetChild(i).GetComponent<UIPartyExtendedMemberSlot>();
                        string memberName = members[i];
                        float distance = Mathf.Infinity;
                        float visRange = player.VisRange();

                        slot.masterIndicatorText.gameObject.SetActive(party.master == memberName);
                        slot.nameText.text = memberName;

                        //buttons
                        slot.buttonChangeLeader.gameObject.SetActive(party.master == player.name);
                        slot.buttonChangeLeader.onClick.SetListener(() => {
                            player.party.CmdPartyChangeLeader(memberName);
                        });

                        slot.buttonKickFromParty.onClick.SetListener(() => {
                            if (party.master == player.name) player.party.CmdKick(memberName);
                            else
                            {
                                player.party.CmdLeave();
                                panel.SetActive(false);
                            }
                        });

                        // pull health, mana, etc. from observers so that party struct
                        // doesn't have to send all that data around. people will only
                        // see health of party members that are near them, which is the
                        // only time that it's important anyway.
                        if (Player.onlinePlayers.ContainsKey(memberName))
                        {
                            Player member = Player.onlinePlayers[memberName];

                            //sliders
                            slot.healthSlider.value = member.health.Percent();

                            // distance color based on visRange ratio
                            distance = Vector3.Distance(player.transform.position, member.transform.position);
                            visRange = member.VisRange(); // visRange is always based on the other guy
                        }

                        // distance overlay alpha based on visRange ratio
                        // (because values are only up to date for members in observer range)
                        float ratio = visRange > 0 ? distance / visRange : 1f;
                        float alpha = alphaCurve.Evaluate(ratio);

                        // icon alpha
                        //Color iconColor = slot.icon.color;
                        //iconColor.a = alpha;
                        //slot.icon.color = iconColor;

                        // health bar alpha
                        foreach (Image image in slot.healthSlider.GetComponentsInChildren<Image>())
                        {
                            Color color = image.color;
                            color.a = alpha;
                            image.color = color;
                        }
                    }
                }
                else
                {
                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefab.gameObject, 0, memberContent);

                    textPartyAmount.text = Localization.Translate("Party") + ": 0" + " / " + Party.Capacity;
                }
            }
            else panel.SetActive(false);
        }
    }
}