using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using uSurvival;

public class UIGraphicsSettings : MonoBehaviour
{
    public GameObject panelOptions;

    public Dropdown dropdownResolution;
    public Toggle toggleFullScreen;

    public Dropdown dropdownMonitor;

    [Header("Качество графики")]
    public Dropdown dropdownQuality;
    private int valueQuality;

    [Header("Качество Текстур")]
    public Dropdown DropdownTexture;

    [Header("Вертикальная синхронизация")]
    public Toggle ToggleVSync;
    private int valueVSync;

    [Header("Сглаживание")]
    public Dropdown DropdownAntiAliasing;

    [Header("Качество теней")]
    public Dropdown DropdownQualityShadow;

    [Header("Дальность отрисовки теней")]
    public Slider SliderRangeShadow;
    public Text TextRangeShadow;
    private int valueRangeShadow;

    [Header("Количество источников освещения")]
    public Slider SliderCountLight;
    public Text TextCountLight;
    private int valueCountLight;

    [Header("FPS")]
    public Text textFpsValue;
    private double FpsTimeEnd;
    public Toggle toggleFpsInGame;
    public GameObject panelFpsInGame;
    public TextMeshProUGUI textFpsInGame;

    public Button ApplyButton;

    private List<Resolution> filterResolutions = new List<Resolution>();
    private float currentRefreshRate;

    private void Start()
    {
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = "Monitor " + i.ToString();
            dropdownMonitor.options.Add(option);
        }
        

        currentRefreshRate = Screen.currentResolution.refreshRate;

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].refreshRate == currentRefreshRate)
                filterResolutions.Add(Screen.resolutions[i]);
        }

        //fill Dropdown Resolution
        dropdownResolution.ClearOptions();
        for (int i = 0; i < filterResolutions.Count; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = filterResolutions[i].ToString();
            dropdownResolution.options.Add(option);
        }

        //show current Resolution in dropdown
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.width == Screen.resolutions[i].width && Screen.height == Screen.resolutions[i].height && Screen.currentResolution.refreshRate == Screen.resolutions[i].refreshRate)
            {
                dropdownResolution.value = i;
                break;
            }
        }

        toggleFullScreen.isOn = Screen.fullScreen;

        dropdownQuality.ClearOptions();
        //load Dropdown Preset
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            Dropdown.OptionData index = new Dropdown.OptionData();
            index.text = QualitySettings.names[i].ToString();
            dropdownQuality.options.Add(index);
        }

        //load DropdownPreset
        dropdownQuality.value = QualitySettings.GetQualityLevel();

        SetGraphicsParametrs();
    }

    private void Update()
    {
        panelFpsInGame.SetActive(toggleFpsInGame.isOn);

        if (toggleFpsInGame.isOn)
        {
            if (FpsTimeEnd < NetworkTime.time)
            {
                int current = (int)(1f / Time.unscaledDeltaTime);
                textFpsInGame.text = "fps " + current.ToString();
                FpsTimeEnd = NetworkTime.time + 1f;
            }
        }


        if (panelOptions.activeSelf)
        {
            ////fps
            //if (FpsTimeEnd < NetworkTime.time)
            //{
            //    int current = (int)(1f / Time.unscaledDeltaTime);
            //    textFpsValue.text = current.ToString() + " ";
            //    FpsTimeEnd = NetworkTime.time + 1f;
            //}

            //Presets
            if (dropdownQuality.value != valueQuality)
            {
                valueQuality = dropdownQuality.value;
                SetGraphicsParametrs();
            }

            //vSync on/off
            if (ToggleVSync.isOn == true) valueVSync = 2;
            else valueVSync = 1;

            valueRangeShadow = (int)SliderRangeShadow.value;
            valueCountLight = (int)SliderCountLight.value;

            //Отображение Дальность теней
            TextRangeShadow.text = "Range of Shadow: " + SliderRangeShadow.value + "м";

            //view Count Light
            TextCountLight.text = "Light count: " + SliderCountLight.value + "x";

            ApplyButton.onClick.SetListener(() => { ApplyGraphicsSettings(); });
        }
    }

    private void ApplyGraphicsSettings()
    {
        //Set Resolution
        Screen.SetResolution(filterResolutions[dropdownResolution.value].width, filterResolutions[dropdownResolution.value].height, toggleFullScreen.isOn);

        //Set Quality Level
        QualitySettings.SetQualityLevel(dropdownQuality.value);

        //Set Texture quality
        QualitySettings.globalTextureMipmapLimit = DropdownTexture.value;

        //Set antiAliasing
        switch (DropdownAntiAliasing.value)
        {
            case 0: QualitySettings.antiAliasing = 0; break;
            case 1: QualitySettings.antiAliasing = 2; break;
            case 2: QualitySettings.antiAliasing = 4; break;
            case 3: QualitySettings.antiAliasing = 8; break;
        }

        // Set VSync
        switch (valueVSync)
        {
            case 1: QualitySettings.vSyncCount = 0; break;
            case 2: QualitySettings.vSyncCount = 1; break;
            case 3: QualitySettings.vSyncCount = 2; break;
        }

        //Set Quality Shadow
        switch (DropdownQualityShadow.value)
        {
            case 0: QualitySettings.shadowResolution = ShadowResolution.Low; break;
            case 1: QualitySettings.shadowResolution = ShadowResolution.Medium; break;
            case 2: QualitySettings.shadowResolution = ShadowResolution.High; break;
            case 3: QualitySettings.shadowResolution = ShadowResolution.VeryHigh; break;
        }

        //Set 
        QualitySettings.shadowDistance = valueRangeShadow;

        //Set Light Count
        QualitySettings.pixelLightCount = valueCountLight;

        //ChangeMonitorAsync();
    }

    private void SetGraphicsParametrs()
    {
        if (valueQuality == 0)
        {
            DropdownTexture.value = 1;
            DropdownAntiAliasing.value = 0;
            ToggleVSync.isOn = false;
            DropdownQualityShadow.value = 1;
            SliderRangeShadow.value = 15;
            SliderCountLight.value = 0;
        }
        else if (valueQuality == 1)
        {
            DropdownTexture.value = 0;
            DropdownAntiAliasing.value = 0;
            ToggleVSync.isOn = false;
            DropdownQualityShadow.value = 2;
            SliderRangeShadow.value = 20;
            SliderCountLight.value = 0;
        }
        else if (valueQuality == 2)
        {
            DropdownTexture.value = 0;
            DropdownAntiAliasing.value = 0;
            ToggleVSync.isOn = false;
            DropdownQualityShadow.value = 3;
            SliderRangeShadow.value = 20;
            SliderCountLight.value = 1;
        }
        else if (valueQuality == 3)
        {
            DropdownTexture.value = 0;
            DropdownAntiAliasing.value = 0;
            ToggleVSync.isOn = true;
            DropdownQualityShadow.value = 4;
            SliderRangeShadow.value = 40;
            SliderCountLight.value = 2;
        }
        else if (valueQuality == 4)
        {
            DropdownTexture.value = 0;
            DropdownAntiAliasing.value = 1;
            ToggleVSync.isOn = true;
            DropdownQualityShadow.value = 3;
            SliderRangeShadow.value = 70;
            SliderCountLight.value = 3;
        }
        else if (valueQuality == 5)
        {
            DropdownTexture.value = 0;
            DropdownAntiAliasing.value = 1;
            ToggleVSync.isOn = true;
            DropdownQualityShadow.value = 4;
            SliderRangeShadow.value = 150;
            SliderCountLight.value = 4;
        }
    }

    public IEnumerator ChangeMonitorAsync()
    {
        if (dropdownMonitor.value >= Display.displays.Length)
        {
            dropdownMonitor.value = 0;
        }
        PlayerPrefs.SetInt("UnitySelectMonitor", dropdownMonitor.value);
        //Screen.SetResolution(800, 600, toggleFullScreen.isOn);
        yield return null;
        //Resolution = Screen.resolutions.Length - 1;
        //Apply();
    }
}
