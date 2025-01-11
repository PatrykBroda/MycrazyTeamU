using GFFAddons;
using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Combat
    {
        [TargetRpc]
        public void TargetShowOutgoingDamageInfo(int amount)
        {
            if (SettingsLoader._damageInfo)
                UIDamageInfo.singleton.AddOutgoingDamageInfo("Нанесли урон: " + amount, true);
        }

        [TargetRpc]
        public void TargetShowIncomingDamageInfo(int amount)
        {
            if (SettingsLoader._damageInfo)
                UIDamageInfo.singleton.AddOutgoingDamageInfo("Получили урон: " + amount, false);
        }

        [ClientRpc]
        public void RpcShowPlayerKill(string player, string target)
        {
            UIDamageInfo.singleton.AddInfoPlayerKillPlayer(player + " Убил " + target);
        }

        [TargetRpc]
        public void TargetShowAddItem(string itemname, int amount)
        {
            UIDamageInfo.singleton.AddMessageInfo("Получен предмет: " + Localization.Translate(itemname) + " x " + amount);
        }

        [TargetRpc]
        public void TargetShowErrorInfo(string message)
        {
            UIDamageInfo.singleton.AddMessageErrorInfo(message);
        }
    }
}

namespace GFFAddons
{
    public class UIDamageInfo : MonoBehaviour
    {
        public GameObject panel;
        public Transform content;
        public GameObject prefab;

        public Color colorFrom = Color.red;
        public Color colorGreen = Color.green;
        public Color colorPlayerToPlayer = Color.white;

        public int infoCount = 5;
        public float time = 0.01f;

        //singleton
        public static UIDamageInfo singleton;
        public UIDamageInfo() { singleton = this; }

        public void AddOutgoingDamageInfo(string message, bool color)
        {
            GameObject go = Instantiate(prefab, content, false);
            UIDamageInfoSlot slot = go.GetComponent<UIDamageInfoSlot>();

            slot.text.text = message;
            slot.text.color = color ? colorGreen : colorFrom;
        }

        public void AddInfoPlayerKillPlayer(string message)
        {
            GameObject go = Instantiate(prefab, content, false);
            UIDamageInfoSlot slot = go.GetComponent<UIDamageInfoSlot>();

            slot.text.text = message;
            slot.text.color = colorPlayerToPlayer;
        }

        public void AddMessageInfo(string message)
        {
            GameObject go = Instantiate(prefab, content, false);
            UIDamageInfoSlot slot = go.GetComponent<UIDamageInfoSlot>();

            slot.text.text = message;
            slot.text.color = colorPlayerToPlayer;
        }

        public void AddMessageErrorInfo(string message)
        {
            GameObject go = Instantiate(prefab, content, false);
            UIDamageInfoSlot slot = go.GetComponent<UIDamageInfoSlot>();

            slot.text.text = Localization.Translate(message);
            slot.text.color = colorFrom;
        }

        private void Update()
        {
            if (content.childCount > infoCount)
            {
                Destroy(content.GetChild(0).gameObject);
            }

            for (int i = 0; i < content.childCount; i++)
            {
                UIDamageInfoSlot slot = content.GetChild(i).GetComponent<UIDamageInfoSlot>();

                if (slot.text.color.a > 0.15f) slot.text.color = new Color(slot.text.color.r, slot.text.color.g, slot.text.color.b, slot.text.color.a - (time * Time.deltaTime));
                else Destroy(slot.gameObject);
            }
        }
    }
}