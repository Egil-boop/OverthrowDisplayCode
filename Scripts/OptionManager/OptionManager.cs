using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
*/

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown colorBlindDropDown;
    [SerializeField] public ColorBlind colorBlind;
    [SerializeField] private ScriptableObjectTeamInformation teamInformation;

    [SerializeField] private Slider mouseSensSlider;

    [SerializeField] private TMP_Dropdown videoSettingsDropDown;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    [SerializeField] private Toggle pointToFlagToggle;
    [SerializeField] private Toggle advanceWayPointToggle;
    [SerializeField] private Toggle pingWaypoint;
    [SerializeField] private Slider pingFrequency;
    [SerializeField] private Toggle controllerRumble;
    [SerializeField] private Toggle edgewarningVis;
    [SerializeField] private Toggle edgeWarningAud;
    [SerializeField] private Toggle audioVisualIndicator;

    [SerializeField] private Slider uiSizeSlider;
    public event Action ChangeColorInMainMeny;

    public AudioController audioController;

  

    private PlayerSettings playerSettings;

 
    private void ChangeColorMainMeny()
    {
        ChangeColorInMainMeny?.Invoke();
    }

    private void Start()
    {
        try
        {
            playerSettings = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().PlayerSettings;
        }
        catch (NullReferenceException)
        {
          
        }
        
        audioController = FindObjectOfType<AudioController>();
        colorBlind = FindObjectOfType<ColorBlind>();
      
        FetchOptionData();
       
        teamInformation.colorBlindSettings = colorBlindDropDown.value;
        colorBlind.InitChange(colorBlindDropDown.value);
        ChangeColorMainMeny();
        colorBlind.ColorChangeEvent();
       
      
        colorBlindDropDown.onValueChanged.AddListener(delegate(int arg0)
        {
            teamInformation.colorBlindSettings = colorBlindDropDown.value;
            colorBlind.InitChange(colorBlindDropDown.value);
            ChangeColorMainMeny();
            colorBlind.ColorChangeEvent();
            teamInformation.SaveSettings();
        });


        uiSizeSlider.value = teamInformation.UISize;
        uiSizeSlider.onValueChanged.AddListener((arg0 =>
        {
            teamInformation.UISize = arg0;
            teamInformation.SaveSettings();
        }));

        mouseSensSlider.onValueChanged.AddListener(delegate(float arg0)
        {
            teamInformation.MouseSensitivity = mouseSensSlider.value;
            teamInformation.SaveSettings();
     
            if (playerSettings != null)
            {
              
                playerSettings.UpdateLookSensitivity();
            }
        });

        videoSettingsDropDown.onValueChanged.AddListener(delegate(int arg0)
        {
            teamInformation.VideoSetting = videoSettingsDropDown.value;
            FullScreen(videoSettingsDropDown.value);
            teamInformation.SaveSettings();
        });

        musicSlider.onValueChanged.AddListener(delegate(float arg0)
        {
            teamInformation.Music = musicSlider.value;
            audioController.SetVolume(FMODMixerGroup.Music, musicSlider.value);
            teamInformation.SaveSettings();
        });

        sfxSlider.onValueChanged.AddListener(delegate(float arg0)
        {
            teamInformation.SFX = sfxSlider.value;
            audioController.SetVolume(FMODMixerGroup.SFX, sfxSlider.value);
            teamInformation.SaveSettings();
        });

        pointToFlagToggle.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.PointToFlag = arg0;
            teamInformation.SaveSettings();
        });

        advanceWayPointToggle.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.AdvancedWayPoint = arg0;
            teamInformation.SaveSettings();
        });

        pingWaypoint.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.PingWaypoint = arg0;
            teamInformation.SaveSettings();
        });
        
        pingFrequency.onValueChanged.AddListener(delegate(float arg0)
        {
            teamInformation.PingFrequency = (int)pingFrequency.value;
            teamInformation.SaveSettings();
        });


        controllerRumble.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.ControllerRumble = arg0;
            teamInformation.SaveSettings();
        });

        edgewarningVis.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.EdgewarningVis = arg0;
            teamInformation.SaveSettings();
        });

        edgeWarningAud.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.EdgeWarningAud = arg0;
            teamInformation.SaveSettings();
        });

        audioVisualIndicator.onValueChanged.AddListener(delegate(bool arg0)
        {
            teamInformation.AudioVisualIndicator = arg0;
            
            audioController.activateVisualIndicators = arg0;
            teamInformation.SaveSettings();
        });
    }

    private void FullScreen(int n)
    {
        switch (n)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(Screen.resolutions[^1].width, Screen.resolutions[^1].height,
                    FullScreenMode.ExclusiveFullScreen);
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                Screen.SetResolution(Screen.resolutions[^1].width, Screen.resolutions[^1].height,
                    FullScreenMode.MaximizedWindow);
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(Screen.resolutions[^1].width, Screen.resolutions[^1].height,
                    FullScreenMode.ExclusiveFullScreen);
                break;
        }
    }

    public void FetchOptionData()
    {
        teamInformation.LoadSettings();

        #region SettingOptionsValueAtStart

        colorBlindDropDown.value = teamInformation.colorBlindSettings;
        mouseSensSlider.value = teamInformation.MouseSensitivity;
        videoSettingsDropDown.value = teamInformation.VideoSetting;
        musicSlider.value = PlayerPrefs.HasKey("Music") ? teamInformation.Music :  0.5f;
        sfxSlider.value = PlayerPrefs.HasKey("SFX") ? teamInformation.SFX : 0.5f;
        pointToFlagToggle.isOn = teamInformation.PointToFlag;
        advanceWayPointToggle.isOn = teamInformation.AdvancedWayPoint;
        pingWaypoint.isOn = teamInformation.PingWaypoint;
        pingFrequency.value = (float)teamInformation.PingFrequency;
        controllerRumble.isOn = teamInformation.ControllerRumble;
        edgewarningVis.isOn = teamInformation.EdgewarningVis;
        edgeWarningAud.isOn = teamInformation.EdgeWarningAud;
        audioVisualIndicator.isOn = teamInformation.AudioVisualIndicator;

        #endregion
    }
}