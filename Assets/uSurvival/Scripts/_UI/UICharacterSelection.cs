// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using GFFAddons;

namespace uSurvival
{
    public class UICharacterSelection : MonoBehaviour
    {
        public GameObject panel;
        public Text textSelectCharacter;

        // available characters (set after receiving the message from the server)
        public Button startButton;
        public Button deleteButton;
        public Button createButton;
        public Button quitButton;

        [Header("Components")]
        public NetworkManagerSurvival manager; // singleton is null until update
        public UICharacterCreationExtended uiCharacterCreation;
        public UIConfirmation uiConfirmation;
        public AudioSource sound;

        private void Start()
        {
            startButton.onClick.SetListener(() => {
                // set client "ready". we will receive world messages from
                // monsters etc. then.
                NetworkClient.Ready();

                // send CharacterSelect message (need to be ready first!)
                NetworkClient.Send(new CharacterSelectMsg { value = manager.selection });

                // clear character selection previews
                manager.ClearPreviews();

                // make sure we can't select twice and call AddPlayer twice
                panel.SetActive(false);
            });

            createButton.onClick.SetListener(() => {
                sound.Play();
                panel.SetActive(false);
                uiCharacterCreation.Show();
            });
        }

        private void Update()
        {
            // show while in lobby and while not creating a character
            if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
            {
                panel.SetActive(true);

                // characters available message received already?
                if (manager.charactersAvailableMsg.characters != null)
                {
                    // instantiate/destroy enough slots
                    CharactersAvailableMsg.CharacterPreview[] characters = manager.charactersAvailableMsg.characters;

                    // start button: calls AddPLayer which calls OnServerAddPlayer
                    // -> button sends a request to the server
                    // -> if we press button again while request hasn't finished
                    //    then we will get the error:
                    //    'ClientScene::AddPlayer: playerControllerId of 0 already in use.'
                    //    which will happen sometimes at low-fps or high-latency
                    // -> internally ClientScene.AddPlayer adds to localPlayers
                    //    immediately, so let's check that first
                    startButton.gameObject.SetActive(manager.selection != -1);

                    // delete button
                    deleteButton.gameObject.SetActive(manager.selection != -1);
                    deleteButton.onClick.SetListener(() => {
                        sbyte icopy = manager.selection;
                        sound.Play();
                        uiConfirmation.Show(
                           Localization.Translate("Do you really want to delete") + "\n <b>" + characters[icopy].name + "</b>?",
                            () => {
                                Debug.Log("Delete send index " + icopy);
                                NetworkClient.Send(new CharacterDeleteMsg { value = icopy });
                            }
                        );
                    });

                    createButton.interactable = characters.Length < manager.characterLimit;

                    quitButton.onClick.SetListener(() => { NetworkManagerSurvival.Quit(); });

                    textSelectCharacter.gameObject.SetActive(manager.selection == -1);

                    //show create panel if characters count = 0
                    if (characters.Length == 0)
                    {
                        manager.charactersAvailableMsg.characters = null;
                        panel.SetActive(false);
                        uiCharacterCreation.Show();
                    }

                    else if (characters.Length == 1) manager.selection = 0;
                }
            }
            else panel.SetActive(false);
        }
    }
}