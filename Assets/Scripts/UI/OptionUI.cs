using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Fullscreen Toggle Buttons")]
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;

    private void Start()
    {
        if (masterSlider == null || sfxSlider == null || musicSlider == null)
        {
            Debug.LogError("One or more sliders are not assigned!");
            return;
        }

        masterSlider.value = Mathf.Clamp(
            PlayerPrefs.GetFloat("MasterVolume", 0.5f),
            masterSlider.minValue,
            masterSlider.maxValue
        );

        sfxSlider.value = Mathf.Clamp(
            PlayerPrefs.GetFloat("SFXVolume", 1f),
            sfxSlider.minValue,
            sfxSlider.maxValue
        );

        musicSlider.value = Mathf.Clamp(
            PlayerPrefs.GetFloat("MusicVolume", 0.5f),
            musicSlider.minValue,
            musicSlider.maxValue
        );


        int isFullScreen = PlayerPrefs.GetInt("FullScreen", 1);
        bool fullscreen = isFullScreen == 1;
        Screen.fullScreen = fullscreen;
        yesButton.SetActive(fullscreen);
        noButton.SetActive(!fullscreen);
    }

    // ==== Fullscreen Buttons ====
    public void FullScreen()
    {
        Screen.fullScreen = true;
        yesButton.SetActive(true);
        noButton.SetActive(false);

        PlayerPrefs.SetInt("FullScreen", 1);
        PlayerPrefs.Save();
    }

    public void NoFullScreen()
    {
        Screen.fullScreen = false;
        yesButton.SetActive(false);
        noButton.SetActive(true);

        PlayerPrefs.SetInt("FullScreen", 0);
        PlayerPrefs.Save();
    }
}
