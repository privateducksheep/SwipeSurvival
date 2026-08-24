using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SFXVolume";
    private const float DefaultVolume = 1f;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        LoadSavedVolumes();
    }
    public void SetMusicVolume()
    {
        SaveAndApply(MusicVolumeKey, "Music", volumeSlider.value);
    }
    public void SetSFXVolume()
    {
        SaveAndApply(SfxVolumeKey, "SFX", SFXSlider.value);
    }

    private void LoadSavedVolumes()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);

        volumeSlider.SetValueWithoutNotify(musicVolume);
        SFXSlider.SetValueWithoutNotify(sfxVolume);

        AudioPreferences.ApplyVolume(audioMixer, "Music", musicVolume);
        AudioPreferences.ApplyVolume(audioMixer, "SFX", sfxVolume);
    }

    private void SaveAndApply(string preferenceKey, string mixerParameter, float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(preferenceKey, volume);
        PlayerPrefs.Save();
        AudioPreferences.ApplyVolume(audioMixer, mixerParameter, volume);
    }
}

/// <summary>
/// Restores saved mixer levels before each scene starts, including the main menu.
/// This means players do not need to revisit Options after reopening the game.
/// </summary>
public static class AudioPreferences
{
    private const float MinimumDecibels = -80f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= ApplySavedVolumes;
        SceneManager.sceneLoaded += ApplySavedVolumes;
    }

    private static void ApplySavedVolumes(Scene scene, LoadSceneMode mode)
    {
        float musicVolume = PlayerPrefs.GetFloat(VolumeSettings.MusicVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(VolumeSettings.SfxVolumeKey, 1f);
        HashSet<AudioMixer> mixers = new HashSet<AudioMixer>();

        foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source.outputAudioMixerGroup != null)
            {
                mixers.Add(source.outputAudioMixerGroup.audioMixer);
            }
        }

        foreach (AudioMixer mixer in mixers)
        {
            ApplyVolume(mixer, "Music", musicVolume);
            ApplyVolume(mixer, "SFX", sfxVolume);
        }
    }

    public static void ApplyVolume(AudioMixer mixer, string parameter, float linearVolume)
    {
        if (mixer == null)
            return;

        float decibels = linearVolume <= 0.0001f
            ? MinimumDecibels
            : Mathf.Log10(Mathf.Clamp01(linearVolume)) * 20f;

        mixer.SetFloat(parameter, decibels);
    }
}
