﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    GameManager gm;
    public PauseMenuMove pauseMenuMove;
    public ScrollRect scrollRect;

    // Video
    public LinearRangeSlider gridOpacitySlider;
    public LinearRangeSlider shakeStrengthSlider;
    //public Toggle screenShakeToggle;
    public Toggle lockDelayDisplayToggle;
    public Toggle previewSpaceToggle;
    public Toggle fullScreenToggle;
    public TMP_Dropdown languageDropdown;

    float gridOpacity = 0f;
    float shakeStrength = 1f;
    //bool screenShakeEnabled = true;
    bool lockDelayDisplayEnabled = true;
    bool previewSpaceEnabled = false;
    bool fullScreenEnabled = true;
    int languageIndex = 0;

    // Audio
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    float masterVolume = 0.5f; //Max 0.5
    float musicVolume = 0.25f; //Max 0.5
    float soundVolume = 0.5f; //Max 1

    // Handling
    public LinearRangeSlider autoRepeatRateSlider;
    public LinearRangeSlider delayedAutoShiftSlider;
    public LinearRangeSlider dasCutDelaySlider;
    public LinearRangeSlider softDropFactorSlider;
    public LinearRangeSlider lineClearPreventMinesweepDelaySlider;
    public Toggle lineClearDelayToggle;

    float autoRepeatRateDefault = 50;
    float delayedAutoShiftDefault = 250;
    float dasCutDelayDefault = 17;
    float softDropFactorDefault = 12;
    float lineClearPreventMinesweepDelayDefault = 50;
    float autoRepeatRate = 50;
    float delayedAutoShift = 250;
    float dasCutDelay = 17;
    float softDropFactor = 12;
    float lineClearPreventMinesweepDelay = 50;
    bool lineClearDelayEnabled = true;

    void OnEnable()
    {
        InputManager.Instance.fullScreenTogglePress.started += _ => FullScreenToggleHotkey();
    }
    void OnDisable()
    {
        InputManager.Instance.fullScreenTogglePress.started -= _ => FullScreenToggleHotkey();
    }

    private void Awake()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", soundVolume);
        //screenShakeEnabled = (PlayerPrefs.GetInt("ScreenShakeEnabled", 1) != 0);

        gridOpacity = PlayerPrefs.GetFloat("GridOpacity", gridOpacity);
        shakeStrength = PlayerPrefs.GetFloat("ShakeStrength", shakeStrength);
        lockDelayDisplayEnabled = (PlayerPrefs.GetInt("LockDelayDisplayEnabled", 0) != 0);
        previewSpaceEnabled = (PlayerPrefs.GetInt("PreviewSpaceAboveBoardEnabled", 0) != 0);
        fullScreenEnabled = (PlayerPrefs.GetInt("FullScreenEnabled", 1) != 0);

#if !UNITY_WEBGL
        Screen.fullScreen = fullScreenEnabled;
#endif
#if !UNITY_EDITOR && UNITY_WEBGL
        scrollRect.scrollSensitivity *= 0.01f;
#endif

        //languageIndex = PlayerPrefs.GetInt("LanguageIndex", 1);
        languageIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        //controlScheme = PlayerPrefs.GetInt("ControlScheme", 0);
        //abTest = PlayerPrefs.GetInt("ABTest", 0);

        autoRepeatRate = PlayerPrefs.GetFloat("AutoRepeatRate", autoRepeatRateDefault);
        delayedAutoShift = PlayerPrefs.GetFloat("DelayedAutoShift", delayedAutoShiftDefault);
        dasCutDelay = PlayerPrefs.GetFloat("DASCutDelay", dasCutDelayDefault);
        softDropFactor = PlayerPrefs.GetFloat("SoftDropFactor", softDropFactorDefault);
        lineClearPreventMinesweepDelay = PlayerPrefs.GetFloat("LineClearPreventMinesweepDelay", lineClearPreventMinesweepDelay);
        lineClearDelayEnabled = (PlayerPrefs.GetInt("LineClearDelayEnabled", 1) != 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        //if (ScoreKeeper.masterVolume != null)
            //masterVolume = ScoreKeeper.masterVolume;

        // Audio
        masterVolumeSlider.value = masterVolume;
        masterVolumeSlider.onValueChanged.AddListener(delegate { MasterVolumeSlider(); });
        musicVolumeSlider.value = musicVolume;
        musicVolumeSlider.onValueChanged.AddListener(delegate { MusicVolumeSlider(); });
        soundVolumeSlider.value = soundVolume;
        soundVolumeSlider.onValueChanged.AddListener(delegate { SoundVolumeSlider(); });

        // Vidoe
        gridOpacitySlider.SetAdjustedValue(gridOpacity);
        gridOpacitySlider.slider.onValueChanged.AddListener(delegate { GridOpacitySlider(); });
        shakeStrengthSlider.SetAdjustedValue(shakeStrength);
        shakeStrengthSlider.slider.onValueChanged.AddListener(delegate { ShakeStrengthSlider(); });

        /*screenShakeToggle.isOn = !screenShakeEnabled;
        screenShakeToggle.onValueChanged.AddListener(delegate  { ScreenShakeToggle(); });*/
        lockDelayDisplayToggle.isOn = lockDelayDisplayEnabled;
        lockDelayDisplayToggle.onValueChanged.AddListener(delegate  { LockDelayDisplayToggle(); });
        previewSpaceToggle.isOn = previewSpaceEnabled;
        previewSpaceToggle.onValueChanged.AddListener(delegate { PreviewSpaceToggle(); }); //fullScreenToggle
        fullScreenToggle.isOn = fullScreenEnabled;
        fullScreenToggle.onValueChanged.AddListener(delegate { FullScreenToggle(); }); //fullScreenToggle

        languageDropdown.value = languageIndex;
        languageDropdown.onValueChanged.AddListener(delegate { LanguageSelectDropdown(); });
        //StartCoroutine(SetLocale(languageIndex));

        // Handling
        autoRepeatRateSlider.SetAdjustedValue(autoRepeatRate);
        autoRepeatRateSlider.slider.onValueChanged.AddListener(delegate { AutoRepeatRateSlider(); });

        delayedAutoShiftSlider.SetAdjustedValue(delayedAutoShift);
        delayedAutoShiftSlider.slider.onValueChanged.AddListener(delegate { DelayedAutoShiftSlider(); });

        dasCutDelaySlider.SetAdjustedValue(dasCutDelay);
        dasCutDelaySlider.slider.onValueChanged.AddListener(delegate { DASCutDelaySlider(); });

        softDropFactorSlider.slider.value = softDropFactor;
        softDropFactorSlider.slider.onValueChanged.AddListener(delegate { SoftDropFactorSlider(); });

        lineClearPreventMinesweepDelaySlider.SetAdjustedValue(lineClearPreventMinesweepDelay);
        lineClearPreventMinesweepDelaySlider.slider.onValueChanged.AddListener(delegate { LineClearPreventMinesweepDelaySlider(); });

        lineClearDelayToggle.isOn = lineClearDelayEnabled;
        lineClearDelayToggle.onValueChanged.AddListener(delegate { LineClearDelayToggle(); });

        //lineClearPreventMinesweepDelay
    }

    // Update is called once per frame
    /*void Update()
    {        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            if (pauseMenuMove.GetIsActive())
                masterVolumeSlider.interactable = true;
            else
                masterVolumeSlider.interactable = false;
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            if (pauseMenuMove.GetIsActive())
                musicVolumeSlider.interactable = true;
            else
                musicVolumeSlider.interactable = false;
        }
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.value = soundVolume;
            if (pauseMenuMove.GetIsActive())
                soundVolumeSlider.interactable = true;
            else
                soundVolumeSlider.interactable = false;
        }
        if (screenShakeToggle != null)
        {
            screenShakeToggle.isOn = !screenShakeEnabled;
            if (pauseMenuMove.GetIsActive())
                screenShakeToggle.interactable = true;
            else
                screenShakeToggle.interactable = false;
        }
        if (lockDelayDisplayToggle != null)
        {
            lockDelayDisplayToggle.isOn = lockDelayDisplayEnabled;
            if (pauseMenuMove.GetIsActive())
                lockDelayDisplayToggle.interactable = true;
            else
                lockDelayDisplayToggle.interactable = false;
        }
        if (languageDropdown != null)
        {
            languageDropdown.value = languageIndex;
            if (pauseMenuMove.GetIsActive())
                languageDropdown.interactable = true;
            else
                languageDropdown.interactable = false;
        }
    }*/

    public void MasterVolumeSlider() // Sets the Master Volume Slider from PlayerPrefs
    {
        masterVolume = masterVolumeSlider.value;
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        //ScoreKeeper.masterVolume = masterVolume;
    }
    public void MusicVolumeSlider() // Sets the Music Volume Slider from PlayerPrefs
    {
        musicVolume = musicVolumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    public void SoundVolumeSlider() // Sets the SFX Volume Slider from PlayerPrefs
    {
        soundVolume = soundVolumeSlider.value;
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
    }

    public void GridOpacitySlider() // Sets the Master Volume Slider from PlayerPrefs
    {
        gridOpacity = gridOpacitySlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("GridOpacity", gridOpacity);
        gm.SetGridOpacity();
    }

    public void ShakeStrengthSlider() // Sets the Master Volume Slider from PlayerPrefs
    {
        shakeStrength = shakeStrengthSlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("ShakeStrength", shakeStrength);
    }
    /*public void ScreenShakeToggle() // Sets the Screen Shake from PlayerPrefs
    {
        screenShakeEnabled = !screenShakeToggle.isOn;
        PlayerPrefs.SetInt("ScreenShakeEnabled", (screenShakeEnabled ? 1 : 0));
    }*/
    public void LockDelayDisplayToggle() // Sets the Lock Delay Display from PlayerPrefs
    {
        lockDelayDisplayEnabled = lockDelayDisplayToggle.isOn;
        PlayerPrefs.SetInt("LockDelayDisplayEnabled", (lockDelayDisplayEnabled ? 1 : 0));
    }
    public void PreviewSpaceToggle()
    {
        previewSpaceEnabled = previewSpaceToggle.isOn;
        PlayerPrefs.SetInt("PreviewSpaceAboveBoardEnabled", (previewSpaceEnabled ? 1 : 0));
        gm.SetCameraScale(false);
    }
    public void FullScreenToggle()
    {
        fullScreenEnabled = fullScreenToggle.isOn;
        PlayerPrefs.SetInt("FullScreenEnabled", (fullScreenEnabled ? 1 : 0));
        Screen.fullScreen = fullScreenEnabled;
    }
    public void FullScreenToggleHotkey()
    {
        fullScreenEnabled = !fullScreenEnabled;
        Screen.fullScreen = fullScreenEnabled;
        fullScreenToggle.isOn = fullScreenEnabled;
        PlayerPrefs.SetInt("FullScreenEnabled", (fullScreenEnabled ? 1 : 0));        
    }
    public void LanguageSelectDropdown()
    {
        languageIndex = languageDropdown.value;
        //PlayerPrefs.SetInt("LanguageIndex", languageIndex);
        StartCoroutine(SetLocale(languageIndex));
    }

    IEnumerator SetLocale(int _localeID)
    {
        //Debug.Log("_localeID " + _localeID);
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
    }

    public void HoverMusicEnter()
    {
        gm.soundManager.EnablePauseFilter();
    }

    public void HoverMusicExit()
    {
        gm.soundManager.DisablePauseFilter();
    }

    public void AutoRepeatRateSlider()
    {
        autoRepeatRate = autoRepeatRateSlider.GetAdjustedValue();        
        PlayerPrefs.SetFloat("AutoRepeatRate", autoRepeatRate);
        gm.tetrominoSpawner.currentTetromino.GetComponent<Group>().UpdateInputValues();
    }
    public void DelayedAutoShiftSlider()
    {
        delayedAutoShift = delayedAutoShiftSlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("DelayedAutoShift", delayedAutoShift);
        gm.tetrominoSpawner.currentTetromino.GetComponent<Group>().UpdateInputValues();
    }
    public void DASCutDelaySlider()
    {
        dasCutDelay = dasCutDelaySlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("DASCutDelay", dasCutDelay);
        gm.tetrominoSpawner.currentTetromino.GetComponent<Group>().UpdateInputValues();
    }

    public void SoftDropFactorSlider()
    {
        softDropFactor = softDropFactorSlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("SoftDropFactor", softDropFactor);
        gm.tetrominoSpawner.currentTetromino.GetComponent<Group>().UpdateInputValues();
    }

    public void LineClearPreventMinesweepDelaySlider()
    {
        lineClearPreventMinesweepDelay = lineClearPreventMinesweepDelaySlider.GetAdjustedValue();
        PlayerPrefs.SetFloat("LineClearPreventMinesweepDelay", lineClearPreventMinesweepDelay);
    }

    public void LineClearDelayToggle()
    {
        lineClearDelayEnabled = lineClearDelayToggle.isOn;
        PlayerPrefs.SetInt("LineClearDelayEnabled", (lineClearDelayEnabled ? 1 : 0));
    }

    public void ResetDefaultsHandling()
    {
        /*float autoRepeatRate = 50;
        float delayedAutoShift = 250;
        float dasCutDelay = 17;
        float softDropFactor = 12;*/

        autoRepeatRate = autoRepeatRateDefault;
        delayedAutoShift = delayedAutoShiftDefault;
        dasCutDelay = dasCutDelayDefault;
        softDropFactor = softDropFactorDefault;
        lineClearPreventMinesweepDelay = lineClearPreventMinesweepDelayDefault;

        autoRepeatRateSlider.SetAdjustedValue(autoRepeatRateDefault);
        delayedAutoShiftSlider.SetAdjustedValue(delayedAutoShiftDefault);
        dasCutDelaySlider.SetAdjustedValue(dasCutDelayDefault);
        softDropFactorSlider.slider.value = softDropFactorDefault;
        lineClearPreventMinesweepDelaySlider.SetAdjustedValue(lineClearPreventMinesweepDelayDefault);

        lineClearDelayEnabled = true;
        lineClearDelayToggle.isOn = true;
        PlayerPrefs.SetInt("LineClearDelayEnabled", 1);
    }
}
