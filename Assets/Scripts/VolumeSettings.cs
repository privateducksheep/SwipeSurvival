using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadMusicVolume();
        }
        else
        {
            SetMusicVolume();
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            LoadSFXVolume();
        }
        else
        {
            SetSFXVolume();
        }
    }
    public void SetMusicVolume()
    {
        float volume = volumeSlider.value;
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void LoadMusicVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SetMusicVolume();
    }

    private void LoadSFXVolume()
    {
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetSFXVolume();
    }
}