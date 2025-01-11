using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GFFAddons
{
    public class UILoginExtendedInfoMessages : MonoBehaviour
    {
        [SerializeField] private bool isUsed = true;
        public float timeInfoMessage = 2;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failedColor = Color.red;

        [SerializeField] private GameObject panel;
        [SerializeField] private Text textInfo;

        public void ShowMessage(string message)
        {
            if (isUsed)
            {
                panel.SetActive(true);
                textInfo.color = successColor;
                textInfo.text = message;

                StartCoroutine(SetDisabledPanelInfo());
            }
        }

        public void ShowErrorMessage(string message)
        {
            if (isUsed)
            {
                panel.SetActive(true);
                textInfo.color = failedColor;
                textInfo.text = message;

                StartCoroutine(SetDisabledPanelInfo());
            }
        }

        public IEnumerator SetDisabledPanelInfo()
        {
            yield return new WaitForSeconds(timeInfoMessage);
            textInfo.text = "";
            panel.SetActive(false);
        }
    }
}