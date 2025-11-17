using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsView : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            // Set sliders to current saved values
            musicSlider.value = AudioManager.instance.GetMusicVolume();
            sfxSlider.value = AudioManager.instance.GetSFXVolume();

            // Subscribe to change events
            musicSlider.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found in scene!");
        }
    }
}



