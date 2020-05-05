using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour
{

    public AudioMixer mixer;
    Resolution[] resolutions;
    public Dropdown resDrop;
    public Dropdown graphicsDrop;
    public Slider volumeSlider;

    public GameObject optionsPanel;

    public readonly static string graphicsKey = "graphics";
    public readonly static string volumeKey = "volume";

    private void Start()
    {
        resolutions = Screen.resolutions;
        resDrop.ClearOptions();

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

        if (PlayerPrefs.HasKey(graphicsKey))
            graphicsDrop.value = PlayerPrefs.GetInt(graphicsKey);

        if (PlayerPrefs.HasKey(volumeKey))
        {
            //mixer.SetFloat("volume", PlayerPrefs.GetFloat(volumeKey));
            volumeSlider.value = PlayerPrefs.GetFloat(volumeKey);
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
    
    public void Exit()
    {
        Application.Quit();
    }

    public void showOptions()
    {
        if (optionsPanel.activeSelf)
            optionsPanel.SetActive(false);
        else
            optionsPanel.SetActive(true);
    }

    public void setVolume(float vol)
    {
        mixer.SetFloat("volume", vol);
        PlayerPrefs.SetFloat(volumeKey, vol);
    }

    public void setQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
   
        PlayerPrefs.SetInt(graphicsKey, qualityIndex);
    }

    public void setFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
    public void setResolution(int resIndex)
    {
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
