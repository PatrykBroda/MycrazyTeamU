using GFFAddons;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace uSurvival
{
    public partial class Player
    {
        [Header("GFF Mail System")]
        public PlayerMail mail;
    }

    public partial class ScriptableItem
    {
        [Header("GFF Can send item by mail ?")]
        public bool canSendOnMail;
    }

    public partial class UIMainPanel
    {
        [Header("GFF Mail Addon")]
        public Button buttonMail;
        public GameObject panelEquipment;
        public AudioClip clip;
        public AudioSource audioSource;
        private int newMessagesAmount = 0;

        public void Update_Mail(Player player)
        {
            if (buttonMail != null)
            {
                buttonMail.transform.GetChild(0).gameObject.SetActive(player.mail.CheckNewMailOnClient());

                //buttonMail.onClick.SetListener(() =>
                //{
                //    UIMail.singleton.panel.SetActive(!UIMail.singleton.panel.activeSelf);
                //});
            }

            if (newMessagesAmount != player.mail.mail.Count)
            {
                audioSource.PlayOneShot(clip);
                newMessagesAmount = player.mail.mail.Count;
            }
        }
    }
}

namespace GFFAddons
{
    //uncomment if using friends addon
    public partial class UIFriends
    {
        public void MailButtonClick_mail(Player player, int icopy)
        {
            UIMail.singleton.panel.SetActive(true);
            UIMail.singleton.panelNewMessage.SetActive(true);
            //UIMail.singleton.mailNewMessage.inputfieldRecipient.text = player.friends.friends[icopy].name;
        }
    }
}