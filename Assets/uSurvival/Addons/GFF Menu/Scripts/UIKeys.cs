using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UIKeys : MonoBehaviour
    {
        [Header("Components")]
        public UIMenuHelper menuHelperScript;

        [Header("UI Elements")]
        public GameObject prefab;
        public Transform content;

        [Header("EnableByGetKeyDown")]
        public GameObject panelReplace;
        public Text textReplace;

        Event keyEvent;
        KeyCode newKey;
        bool waitingForKey = false;

        void Update()
        {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(prefab.gameObject, menuHelperScript.menuList.Count, content);

            // refresh all buttons
            for (int i = 0; i < menuHelperScript.menuList.Count; ++i)
            {
                UIButtonKeySlot slot = content.GetChild(i).GetComponent<UIButtonKeySlot>();
                slot.gameObject.name = menuHelperScript.menuList[i].name;
                slot.textName.text = menuHelperScript.menuList[i].name;
                slot.button.GetComponentInChildren<Text>().text = menuHelperScript.menuList[i].currentKey.ToString();

                //button
                int icopy = i; // needed for lambdas, otherwise i is Count
                slot.button.onClick.SetListener(() => {
                    if (!waitingForKey)
                        StartCoroutine(AssignKey(icopy));
                });
            }
        }

        void OnGUI()
        {
            //keyEvent dictates what key our user presses bt using Event.current to detect the current event
            keyEvent = Event.current;

            //Executes if a button gets pressed and the user presses a key
            if (keyEvent.isKey && waitingForKey)
            {
                newKey = keyEvent.keyCode; //Assigns newKey to the key user presses
                waitingForKey = false;
                panelReplace.SetActive(false);
            }
        }

        private IEnumerator AssignKey(int i)
        {
            panelReplace.SetActive(true);
            textReplace.text = "Press a KEY for replace [" + menuHelperScript.menuList[i].name + "]";
            waitingForKey = true;
            yield return WaitForKey(); //Executes endlessly until user presses a key

            menuHelperScript.menuList[i].currentKey = newKey;
            PlayerPrefs.SetString(menuHelperScript.menuList[i].name, menuHelperScript.menuList[i].currentKey.ToString());

            yield return null;
        }

        //Used for controlling the flow of our below Coroutine
        private IEnumerator WaitForKey()
        {
            while (!keyEvent.isKey)
                yield return null;
        }
    }
}


