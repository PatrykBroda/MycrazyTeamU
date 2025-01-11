using GFFAddons;
using UnityEngine;

namespace uSurvival
{
    public partial class NetworkManagerSurvival
    {
        [Header("Character Selection")]
        public sbyte selection = -1;
        public Transform[] selectionLocations;
        public Transform selectionCameraLocation;

        // handshake: character selection //////////////////////////////////////////
        private void LoadPreview(GameObject prefab, Transform location, sbyte selectionIndex, CharactersAvailableMsg.CharacterPreview character)
        {
            // instantiate the prefab
            GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
            preview.tag = "Untagged";
            preview.transform.parent = location;
            Player player = preview.GetComponent<Player>();

            player.health.current = 100;
            //player.hotbar.enabled = false;
            player.respawning.enabled = false;

            // assign basic preview values like name and equipment
            player.name = character.name;

            //customization addon
            for (int i = 0; i < character.customizationValues.Length; i++)
            {
                CustomizationByType temp = new CustomizationByType();
                temp.type = character.customizationValues[i].type;
                temp.defaultValue = character.customizationValues[i].defaultValue;
                temp.materialValue = character.customizationValues[i].materialValue;
                player.customization.values.Add(temp);
            }

            player.customization.SetCustomization();

            // add selection script
            preview.AddComponent<SelectableCharacter>();
            preview.GetComponent<SelectableCharacter>().index = selectionIndex;

            for (int i = 0; i < preview.GetComponent<Player>().equipment.slotInfo.Length; ++i)
                preview.GetComponent<Player>().equipment.slots.Add(new ItemSlot());
        }

        public void ClearPreviews()
        {
            selection = -1;
            foreach (Transform location in selectionLocations)
                if (location.childCount > 0)
                    Destroy(location.GetChild(0).gameObject);
        }

        private void OnServerCharacterCreate_Customization(CharacterCreateMsg message, Player player)
        {
            for (int i = 0; i < player.customization.templates.Length; i++)
            {
                CustomizationByType temp = new CustomizationByType();
                temp.type = message.customization[i].type;
                temp.defaultValue = message.customization[i].defaultValue;
                temp.materialValue = message.customization[i].materialValue;
                player.customization.values.Add(temp);
            }

            //add suit to bought suits
            BoughtSuits suit = new BoughtSuits();
            suit.classname = player.className;
            int suitsIndex = player.customization.FindTypeIndexByType(EquipmentItemType.Suit);
            if (suitsIndex != -1) suit.suitname = player.customization.templates[suitsIndex].objects[player.customization.values[suitsIndex].defaultValue].name;
            player.customization.boughtSuits.Add(suit);
        }

        private void OnClientCharactersAvailable(CharactersAvailableMsg message)
        {
            charactersAvailableMsg = message;
            //Debug.Log("characters available:" + charactersAvailableMsg.characters.Length);

            // set state
            state = NetworkState.Lobby;

            // clear previous previews in any case
            ClearPreviews();

            // load previews for 3D character selection
            for (sbyte i = 0; i < charactersAvailableMsg.characters.Length; ++i)
            {
                CharactersAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];

                // find the prefab for that class
                Player prefab = playerClasses.Find(p => p.name == character.className).GetComponent<Player>();
                if (prefab != null)
                {
                    LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
                }
                else
                    Debug.LogWarning("Character Selection: no prefab found for class " + character.className);
            }

            // setup camera
            Camera.main.transform.position = selectionCameraLocation.position;
            Camera.main.transform.rotation = selectionCameraLocation.rotation;
        }
    }

    public partial class Player
    {
        [Header("Character Create Extended")]
        [SerializeField] private LocalizeText[] bonusDescription;
        [HideInInspector] public bool isCreatedCharacter = false;
        public string GetDescriptionByLanguage(SystemLanguage lang)
        {
            for (int i = 0; i < bonusDescription.Length; i++)
            {
                if (bonusDescription[i].language == lang) return bonusDescription[i].description;
            }
            return null;
        }
    }
}