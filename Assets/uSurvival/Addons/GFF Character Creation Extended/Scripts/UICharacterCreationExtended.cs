using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    public class UICharacterCreationExtended : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private InputField inputFieldName;
        [SerializeField] private Dropdown dropdownClass;
        [SerializeField] private Button buttonMale;
        [SerializeField] private Button buttonWomen;
        [SerializeField] private Button buttonCreate;
        [SerializeField] private Button buttonCancel;
        [SerializeField] private Button buttonRandomize;
        [SerializeField] private Text textBonus;
        [SerializeField] private Text textNameInfo;

        [Header("Components")]
        [SerializeField] private NetworkManagerSurvival manager; // singleton is null until update
        [SerializeField] private AudioSource audioSource;

        [Header("Customization")]
        [SerializeField] private Transform content;
        [SerializeField] private GameObject prefab;
        [SerializeField] private CharcterCreationComponents positions;

        [Header("Rotate Character")]
        [SerializeField] private float strengh = 10;
        private float rotY;
        private bool applyForce;

        private Camera cachedCamera;
        private GameObject playerPreview;
        private List<string> playerClasses;

        public void Show()
        {
            // copy player classes to class selection
            dropdownClass.options = manager.playerClasses.Select(p => new Dropdown.OptionData(Localization.Translate(p.name))).ToList();
            playerClasses = manager.playerClasses.Select(p => new string(p.name)).ToList();

            InstantiatePrefab();

            //move camera
            cachedCamera.transform.position = positions.cameraCustomizationPosition.position;
            cachedCamera.transform.rotation = positions.cameraCustomizationPosition.rotation;

            panel.SetActive(true);
        }

        public bool IsVisible() { return panel.activeSelf; }

        private void Start()
        {
            // setup camera
            cachedCamera = Camera.main;

            // cancel
            buttonCancel.onClick.SetListener(() =>
            {
                audioSource.Play();
                inputFieldName.text = "";
                inputFieldName.characterLimit = manager.characterNameMaxLength;
                panel.SetActive(false);
                Destroy(playerPreview);

                // setup camera
                cachedCamera.transform.position = manager.selectionCameraLocation.position;
                cachedCamera.transform.rotation = manager.selectionCameraLocation.rotation;
            });

            inputFieldName.characterLimit = manager.characterNameMaxLength;
        }

        private void Update()
        {
            // only update while visible (after character selection made it visible)
            if (!panel.activeSelf) return;

            // still in lobby?
            if (manager.state == NetworkState.Lobby)
            {
                if (playerPreview == null){InstantiatePrefab();}
                else
                {
                    PlayerCustomization customization = playerPreview.GetComponent<PlayerCustomization>();
                    List<Customization> types = customization.GetItemsForCharacterCreate();

                    UIUtils.BalancePrefabs(prefab.gameObject, types.Count, content);

                    for (int i = 0; i < types.Count; i++)
                    {
                        UICustomizationSlot slot = content.GetChild(i).GetComponent<UICustomizationSlot>();

                        slot.textTypeName.text = Localization.Translate(types[i].type.ToString());
                        slot.slider.maxValue = types[i].objects.Length - 1;
                        slot.slider.value = customization.FindItemIndexInEditedTypes(types, i);

                        int icopy = i;
                        slot.buttonLeft.onClick.SetListener(() =>
                        {
                            audioSource.Play();
                            if (slot.slider.value > 0)
                            {
                                if (types[icopy].objects[(int)slot.slider.value - 1].parts[0] == null)
                                    customization.SetCustomizationLocalByItem(types[icopy].type, null);
                                else
                                {
                                    customization.SetCustomizationLocalByItem(types[icopy].type, types[icopy].objects[(int)slot.slider.value - 1].parts[0].name);
                                }
                            }
                        });

                        slot.buttonRight.onClick.SetListener(() =>
                        {
                            audioSource.Play();
                            if (slot.slider.value < slot.slider.maxValue)
                            {
                                customization.SetCustomizationLocalByItem(types[icopy].type, types[icopy].objects[(int)slot.slider.value + 1].parts[0].name);
                            }
                        });
                    }

                    //suit color
                    //UICustomizationSlot colorSlot = content.GetChild(types.Count).GetComponent<UICustomizationSlot>();
                    //colorSlot.textTypeName.text = Localization.Translate("Colors");
                    //colorSlot.slider.maxValue = customization.materials.Length - 1;
                    //int typeIndex = customization.FindTypeIndexByType(EquipmentItemType.Suit);
                    //if (typeIndex != -1) colorSlot.slider.value = customization.values[typeIndex].materialValue;

                    //colorSlot.buttonLeft.onClick.SetListener(() =>
                    //{
                    //    audioSource.Play();
                    //    if (colorSlot.slider.value > 0)
                    //    {
                    //        customization.SetColorForSuit(EquipmentItemType.Suit, (byte)(colorSlot.slider.value - 1));
                    //    }
                    //});
                    //colorSlot.buttonRight.onClick.SetListener(() =>
                    //{
                    //    audioSource.Play();
                    //    if (colorSlot.slider.value < 3)
                    //    {
                    //        customization.SetColorForSuit(EquipmentItemType.Suit, (byte)(colorSlot.slider.value + 1));
                    //    }
                    //});

                    // create
                    textNameInfo.gameObject.SetActive(!manager.IsAllowedCharacterName(inputFieldName.text));
                    textNameInfo.text = Localization.Translate("NameLengthRequirements") + "\n" + manager.characterNameMinLength + " " + Localization.Translate("NameLengthRequirementsValue");

                    buttonCreate.interactable = manager.IsAllowedCharacterName(inputFieldName.text);
                    buttonCreate.onClick.SetListener(() =>
                    {
                        audioSource.Play();
                        CustomizationByType[] _defaults = new CustomizationByType[customization.templates.Length];
                        for (int i = 0; i < customization.templates.Length; i++)
                        {
                            CustomizationByType temp = new CustomizationByType();
                            temp.type = customization.values[i].type;
                            temp.defaultValue = customization.values[i].defaultValue;
                            temp.materialValue = customization.values[i].materialValue;
                            _defaults[i] = temp;
                        }

                        CharacterCreateMsg message = new CharacterCreateMsg
                        {
                            name = inputFieldName.text,
                            classIndex = dropdownClass.value,
                            customization = _defaults
                        };
                        NetworkClient.Send(message);

                        panel.SetActive(false);
                        Destroy(playerPreview);
                        inputFieldName.text = "";
                    });

                    //randomize
                    buttonRandomize.onClick.SetListener(() =>
                    {
                        audioSource.Play();
                        customization.Randomize();
                    });
                }

                //character rotate
                if (Input.GetMouseButton(0) && !UIUtils.AnyInputActive())
                {
                    applyForce = true;
                    rotY = Input.GetAxis("Mouse X") * strengh;
                }
                else applyForce = false;
            }
            else panel.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (playerPreview != null && applyForce) playerPreview.transform.Rotate(0, -rotY * Time.deltaTime, 0);
        }

        private void InstantiatePrefab()
        {
            if (playerPreview != null) Destroy(playerPreview);

            // instantiate the prefab
            playerPreview = Instantiate(manager.playerClasses.Find(p => p.name == playerClasses[dropdownClass.value]), positions.characterPosition.position, positions.characterPosition.rotation);
            playerPreview.tag = "Untagged";
            playerPreview.name = "";

            Player player = playerPreview.GetComponent<Player>();
            player.isCreatedCharacter = true;

            player.health.current = 100;
            player.respawning.enabled = false;

            for (int i = 0; i < player.equipment.slotInfo.Length; ++i)
                player.equipment.slots.Add(new ItemSlot());

            //setup customization
            PlayerCustomization customization = playerPreview.GetComponent<PlayerCustomization>();
            customization.SetupForNewCharacter();

            textBonus.text = player.GetDescriptionByLanguage(Localization.languageCurrent);
        }
    }
}