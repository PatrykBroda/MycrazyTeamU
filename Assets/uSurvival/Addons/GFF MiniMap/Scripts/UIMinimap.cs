using UnityEngine;
using UnityEngine.UI;
using uSurvival;
using TMPro;

namespace GFFAddons
{
    public partial class UIMinimap : MonoCache
    {
        //public float zoomMin = 30;
        //public float zoomMax = 60;
        //public float zoomStepSize = 5;
        //public Button plusButton;
        //public Button minusButton;
        //public Camera minimapCamera;

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI textNameOfDistrict;
        [SerializeField] private Transform pockets;
        [SerializeField] private GameObject panelMainMenu;

        [Header("Murderer Debaff")]
        [SerializeField] private GameObject panelMurderer;
        [SerializeField] private TextMeshProUGUI textMurderer;

        [Header("Radiation")]
        [SerializeField] private GameObject imageRadiation;
        [SerializeField] private AudioSource radiationAudio;

        [Header("Safe zone")]
        [SerializeField] private GameObject imageSafeZone;

        //private void Start()
        //{
        //    plusButton.onClick.SetListener(() =>
        //    {
        //        minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        //    });
        //    minusButton.onClick.SetListener(() =>
        //    {
        //        minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        //    });
        //}

        public override void OnTick()
        {
            Player player = Player.localPlayer;
            if (player != null && !panelMainMenu.activeSelf)
            {
                if (SettingsLoader._miniMap == true)
                {
                    panel.SetActive(true);
                }
                else
                {
                    panel.SetActive(false);
                }

                for (int i = 0; i < pockets.childCount; ++i)
                {
                    int icopy = (i + 6);
                    UIEquipmentSlot slot = pockets.GetChild(i).GetComponent<UIEquipmentSlot>();
                    ItemSlot itemSlot = player.equipment.slots[icopy];
                    if (itemSlot.amount > 0)
                    {
                        slot.image.sprite = itemSlot.item.image;
                        slot.image.color = Color.white;
                        slot.amountText.text = itemSlot.amount.ToString();
                    }
                    else
                    {
                        slot.image.sprite = null;
                        slot.image.color = Color.clear;
                        slot.amountText.text = "";
                    }

                    // hotkey pressed and not typing in any input right now?
                    // (and not while reloading, this would be unrealistic, feels weird
                    //  and the reload bar UI needs selected weapon's reloadTime)

                    if (Input.GetKeyDown(player.equipment.pocketHotKeys[icopy - 6]) && !UIUtils.AnyInputActive())
                    {
                        // usable item?
                        if (itemSlot.amount > 0 && itemSlot.item.data is UsableItem usable && usable.CanUseEquipment(player, icopy) == Usability.Usable)
                        {
                            // use it directly or select the slot?
                            player.equipment.CmdUseItem(icopy);
                        }
                    }
                }

                //Murderer DeBuff
                panelMurderer.SetActive(player.remainMurdererBuff > 0);
                if (panelMurderer.activeSelf)
                    textMurderer.text = player.remainMurdererBuff.ToString("F0") + "\n" + "sec";

                //safe zone image
                imageSafeZone.SetActive(player.currentSafeZoneSource != null);

                //radiation zone
                imageRadiation.SetActive(player.radiation.currentRadiationSource != null);
                if (imageRadiation.activeSelf)
                {
                    if (radiationAudio.isPlaying == false) radiationAudio.Play();
                }
                else radiationAudio.Stop();

                if (player.nameOfDistricts.currentDistrictSource) textNameOfDistrict.text = Localization.Translate(player.nameOfDistricts.currentDistrictSource.name);
                else textNameOfDistrict.text = "";
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }
}