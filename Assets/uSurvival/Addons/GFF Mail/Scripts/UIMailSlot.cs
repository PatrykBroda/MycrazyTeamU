using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIMailSlot : MonoBehaviour
    {
        public Text textSender;
        public Text textSubject;
        public Toggle Selected;
        public Button Button;

        public int id;
        public bool read;
        public string sender;
        public string subject;
        public string message;

        public long gold = 0;
        public bool goldTake;
        public int coins = 0;
        public bool coinsTake;

        public ItemSlot itemslot;
        public bool itemTake;
    }
}


