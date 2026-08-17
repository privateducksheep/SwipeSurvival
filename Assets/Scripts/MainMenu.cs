using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip click;

    public void PlayGame()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("Playable");
    }

    public void QuitGame()
    {
        audioSource.PlayOneShot(click);
        Application.Quit();
    }

    public void Options()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("OptionsMenu");
    }
}

