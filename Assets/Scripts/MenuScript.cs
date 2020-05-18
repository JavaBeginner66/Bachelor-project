using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections.Generic;

/*
 * Script that handles everything related to the menu
 */

public class MenuScript : MonoBehaviour
{

    public AudioMixer mixer;            // Volume object reference
    Resolution[] resolutions;           // Array of different resolutions
    public Dropdown resDrop;            // Dropdown UI object for resolutions
    public Dropdown graphicsDrop;       // Dropdown UI object for graphics
    public Slider volumeSlider;         // Slider UI object for volume
    public Toggle fullScreenToggle;     // Toggle UI object for fullscreen/windowed mode
    public Toggle minimalismToggle;     // Toggle UI object to activate/deactivate minimalism mode

    public GameObject optionsPanel;     // Gameobject reference to options panel
    public GameObject howToPlayPanel;   // Gameobject reference to how to play panel

    public GameObject[] minimalismObjects;  // Array of objects to be disabled/enabled

    // String keys for PlayerPref values
    public readonly static string graphicsKey = "graphics";
    public readonly static string volumeKey = "volume";
    public readonly static string fullscreenKey = "fullscreen";
    public readonly static string minimalismKey = "minimalism";

    private void Start()
    {
        // Get the systems possible resolutions
        resolutions = Screen.resolutions;
        resDrop.ClearOptions();
       
        // Adds the systems resolutions to the resolution dropdown component, and sets 
        // the most appropriate resolution as default
        int currentResIndex = 0;        
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResIndex = i;
        }

        resDrop.AddOptions(options);
        resDrop.value = currentResIndex;
        resDrop.RefreshShownValue();

        // Getting and setting PlayerPref values
        if (PlayerPrefs.HasKey(graphicsKey))
            graphicsDrop.value = PlayerPrefs.GetInt(graphicsKey);

        if (PlayerPrefs.HasKey(volumeKey))
        {
            //mixer.SetFloat("volume", PlayerPrefs.GetFloat(volumeKey));
            volumeSlider.value = PlayerPrefs.GetFloat(volumeKey);
        }
        if (PlayerPrefs.HasKey(fullscreenKey))
        {
            if (PlayerPrefs.GetInt(fullscreenKey) == 1)
                setFullScreen(true);
            else
                setFullScreen(false);
        }

        if (PlayerPrefs.HasKey(minimalismKey))
        {
            if (PlayerPrefs.GetInt(minimalismKey) == 1)
                minimalism(true);
            else
                minimalism(false);
        }
        else
        {
            minimalism(false);
        }
    }

    /*
     * Method to start the game which is detected on "Play" button click in menu
     */
    public void Play()
    {
        FadeTransition.fade.fadeTo(1);
        GameMasterScript.gameRunning = false;
    }

    /*
     * Method to exit the game which is detected on "Exit" button click in menu
     */
    public void Exit()
    {
        Application.Quit();
    }

    /*
     * Method to show the options panel which is detected on "Options" button click in menu
     */
    public void showOptions()
    {
        if (optionsPanel.activeSelf)
            optionsPanel.SetActive(false);
        else
            optionsPanel.SetActive(true);
    }

    /*
     * Method to show the how to play panel which is detected on "How to play" button click in menu
     */
    public void showHowToPlay()
    {
        if (howToPlayPanel.activeSelf)
            howToPlayPanel.SetActive(false);
        else
            howToPlayPanel.SetActive(true);
    }

    /*
     * Method to change the volume of the game in options panel. Gets called each time volume slider is moved.
     */
    public void setVolume(float vol)
    {
        mixer.SetFloat("volume", vol);
        PlayerPrefs.SetFloat(volumeKey, vol);
    }

    /*
     * Method to change quality in options panel
     */
    public void setQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
   
        PlayerPrefs.SetInt(graphicsKey, qualityIndex);
    }

    /*
     * Method removes/adds some of the objects from the scene and saves it to PlayerPrefs
     */
    public void minimalism(bool isActive)
    {
        PlayerPrefs.SetInt(minimalismKey, isActive ? 1 : 0);
        minimalismToggle.isOn = isActive;

        if (isActive)
            foreach (var item in minimalismObjects)
                item.SetActive(false);
        else
            foreach (var item in minimalismObjects)
                item.SetActive(true);
    }

    /*
     * Method to set window to fullscreen/windowed mode in options panel
     */
    public void setFullScreen(bool isFullScreen)
    {
        PlayerPrefs.SetInt(fullscreenKey, isFullScreen ? 1 : 0);
        fullScreenToggle.isOn = isFullScreen;
        Screen.fullScreen = isFullScreen;
        
    }

    /*
     * Method to set resolution in options panel
     */

    public void setResolution(int resIndex)
    {
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
