using GFFAddons;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

public class UIChatExtended : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject activeComponents;
    [SerializeField] private Button showButton;
    [SerializeField] private Button hideButton;
    [SerializeField] private Button ignoreButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject resizeButton;
    [SerializeField] private InputField messageInput;
    [SerializeField] private Button sendButton;

    [SerializeField] private Button allButton;
    [SerializeField] private Button whisperButton;

    [SerializeField] private Transform contentAll;
    [SerializeField] private ScrollRect scrollRectAll;

    [SerializeField] private Transform contentWhisper;
    [SerializeField] private ScrollRect scrollRectWhisper;

    [SerializeField] private UIIgnoreList ignoreListScript;
    [SerializeField] private GameObject panelOptions;

    [SerializeField] private Toggle toggleAll;
    [SerializeField] private Toggle toggleWhisper;
    [SerializeField] private Toggle toggleInfo;
    [SerializeField] private Toggle toggleTrade;
    [SerializeField] private Toggle toggleBattle;

    [SerializeField] private GameObject panelMainMenu;

    public KeyCode[] activationKeys = { KeyCode.Return, KeyCode.KeypadEnter };
    public int keepHistory = 100; // only keep 'n' messages

    private chatChannel channel = chatChannel.all;
    private enum chatState { show, hide }
    private chatState state = chatState.hide;

    private bool eatActivation;

    public static UIChatExtended singleton;
    public UIChatExtended()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        //if (singleton == null)
        singleton = this;
    }

    private void Start()
    {
        LoadDataFromPlayerPrefs();

        toggleAll.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetInt("ChatExtended_All_ShowAll", toggleAll.isOn.ToInt());
        });
        toggleWhisper.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetInt("ChatExtended_All_ShowWhisper", toggleWhisper.isOn.ToInt());
        });
        toggleInfo.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetInt("ChatExtended_All_ShowInfo", toggleInfo.isOn.ToInt());
        });
        toggleTrade.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetInt("ChatExtended_All_ShowTrade", toggleTrade.isOn.ToInt());
        });

        ignoreButton.onClick.SetListener(() =>
        {
            ignoreListScript.Show();
        });

        optionsButton.onClick.SetListener(() =>
        {
            panelOptions.SetActive(true);
        });

        // show and hide button
        showButton.onClick.SetListener(() =>
        {
            state = chatState.show;
        });
        hideButton.onClick.SetListener(() =>
        {
            state = chatState.hide;

            // unfocus the whole chat in any case. otherwise we would scroll or
            // activate the chat window when doing wsad movement afterwards
            UIUtils.DeselectCarefully();
        });

        whisperButton.onClick.SetListener(() =>
        {
            channel = chatChannel.whisper;
        });

        allButton.onClick.SetListener(() =>
        {
            channel = chatChannel.all;
        });
    }

    private void Update()
    {
        Player player = Player.localPlayer;
        if (player && !panelMainMenu.activeSelf && SettingsLoader._chat == true)
        {
            panel.SetActive(true);

            activeComponents.gameObject.SetActive(state == chatState.show);
            panel.GetComponent<Image>().enabled = state == chatState.show;

            // character limit
            PlayerChat chat = player.GetComponent<PlayerChat>();
            messageInput.characterLimit = chat.maxLength;

            if (Utils.AnyKeyDown(activationKeys) && !UIUtils.AnyInputActive())
            {
                if (state != chatState.show)
                {
                    state = chatState.show;
                    activeComponents.SetActive(true);
                    messageInput.ActivateInputField();
                    messageInput.Select();
                }
                else
                {
                    // submit and set new input text
                    string newinput = chat.OnSubmit(messageInput.text, channel);
                    messageInput.text = newinput;
                    messageInput.MoveTextEnd(false);

                    // unfocus the whole chat in any case. otherwise we would scroll or
                    // activate the chat window when doing wsad movement afterwards
                    UIUtils.DeselectCarefully();

                    state = chatState.hide;
                }
            }

            // end edit listener
            messageInput.onEndEdit.SetListener((value) =>
            {
                // submit key pressed? then submit and set new input text
                if (Utils.AnyKeyDown(activationKeys))
                {
                    string newinput = chat.OnSubmit(value, channel);
                    messageInput.text = newinput;
                    messageInput.MoveTextEnd(false);
                    eatActivation = true;
                }

                // unfocus the whole chat in any case. otherwise we would scroll or
                // activate the chat window when doing wsad movement afterwards
                UIUtils.DeselectCarefully();
            });

            // send button
            sendButton.onClick.SetListener(() =>
            {
                // submit and set new input text
                string newinput = chat.OnSubmit(messageInput.text, channel);
                messageInput.text = newinput;
                messageInput.MoveTextEnd(false);

                // unfocus the whole chat in any case. otherwise we would scroll or
                // activate the chat window when doing wsad movement afterwards
                UIUtils.DeselectCarefully();
            });

            allButton.interactable = channel != chatChannel.all;

            whisperButton.interactable = channel != chatChannel.whisper;

            scrollRectAll.gameObject.SetActive(channel == chatChannel.all);
            scrollRectWhisper.gameObject.SetActive(channel == chatChannel.whisper);
        }
        else panel.SetActive(false);
    }

    private void AutoScroll()
    {
        // update first so we don't ignore recently added messages, then scroll
        Canvas.ForceUpdateCanvases();

        scrollRectAll.verticalNormalizedPosition = 0;
        scrollRectWhisper.verticalNormalizedPosition = 0;
    }

    public void AddMessage(ChatMessage message, chatChannel channel)
    {
        // delete old messages so the UI doesn't eat too much performance.
        // => every Destroy call causes a lag because of a UI rebuild
        // => it's best to destroy a lot of messages at once so we don't
        //    experience that lag after every new chat message

        if (channel == chatChannel.gm ||
            (channel == chatChannel.all && toggleAll.isOn && ignoreListScript.CheckNameForAllChat(message.sender)) ||
            (channel == chatChannel.info && toggleInfo.isOn) ||
            (channel == chatChannel.battle && toggleBattle.isOn) ||
            (channel == chatChannel.trade && toggleTrade.isOn && ignoreListScript.CheckNameForAllChat(message.sender)) ||
            (channel == chatChannel.whisper && toggleWhisper.isOn && ignoreListScript.CheckNameForPm(message.sender) && ignoreListScript.CheckNameForAllChat(message.sender))
            )
        {
            if (contentAll.childCount >= keepHistory)
            {
                for (int i = 0; i < contentAll.childCount / 2; ++i)
                    Destroy(contentAll.GetChild(i).gameObject);
            }

            // instantiate and initialize text prefab
            GameObject temp = Instantiate(message.textPrefab, contentAll.transform, false);
            temp.GetComponent<Text>().text = message.Construct();
            temp.GetComponent<UIChatEntry>().message = message;
        }


        if (channel == chatChannel.whisper && ignoreListScript.CheckNameForPm(message.sender) && ignoreListScript.CheckNameForAllChat(message.sender))
        {
            if (contentWhisper.childCount >= keepHistory)
            {
                for (int i = 0; i < contentWhisper.childCount / 2; ++i)
                    Destroy(contentWhisper.GetChild(i).gameObject);
            }

            // instantiate and initialize text prefab
            GameObject go = Instantiate(message.textPrefab, contentWhisper.transform, false);
            go.GetComponent<Text>().text = message.Construct();
            go.GetComponent<UIChatEntry>().message = message;
        }

        AutoScroll();
    }

    // called by chat entries when clicked
    public void OnEntryClicked(UIChatEntry entry)
    {
        // any reply prefix?
        if (!string.IsNullOrWhiteSpace(entry.message.replyPrefix))
        {
            // set text to reply prefix
            messageInput.text = entry.message.replyPrefix;

            // activate
            messageInput.Select();

            // move cursor to end (doesn't work in here, needs small delay)
            Invoke(nameof(MoveTextEnd), 0.1f);
        }
    }

    private void MoveTextEnd()
    {
        messageInput.MoveTextEnd(false);
    }

    private void LoadDataFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("ChatExtended_All_ShowParty"))
        {
            toggleAll.isOn = PlayerPrefs.GetInt("ChatExtended_All_ShowAll").ToBool();
            toggleWhisper.isOn = PlayerPrefs.GetInt("ChatExtended_All_ShowWhisper").ToBool();
            toggleInfo.isOn = PlayerPrefs.GetInt("ChatExtended_All_ShowInfo").ToBool();
            toggleTrade.isOn = PlayerPrefs.GetInt("ChatExtended_All_ShowTrade").ToBool();
            toggleBattle.isOn = PlayerPrefs.GetInt("ChatExtended_All_ShowBattle").ToBool();
        }
        else
        {
            toggleAll.isOn = true;
            toggleWhisper.isOn = true;
            toggleInfo.isOn = true;
            toggleTrade.isOn = true;
            toggleBattle.isOn = true;

            SaveOptionsToPlayerPrefs();
        }
    }
    private void SaveOptionsToPlayerPrefs()
    {
        PlayerPrefs.SetInt("ChatExtended_All_ShowAll", toggleAll.isOn.ToInt());
        PlayerPrefs.SetInt("ChatExtended_All_ShowWhisper", toggleWhisper.isOn.ToInt());
        PlayerPrefs.SetInt("ChatExtended_All_ShowInfo", toggleInfo.isOn.ToInt());
        PlayerPrefs.SetInt("ChatExtended_All_ShowTrade", toggleTrade.isOn.ToInt());
        PlayerPrefs.SetInt("ChatExtended_All_ShowBattle", toggleBattle.isOn.ToInt());
    }

    public void ShowForGmTool()
    {
        //Debug.Log("chat");
        state = chatState.show;
        messageInput.Select();
        StartCoroutine("MoveTextToEnd");
    }

    private IEnumerator MoveTextToEnd()
    {
        //needs small delay)
        yield return new WaitForFixedUpdate();
        messageInput.MoveTextEnd(false);
    }
}
