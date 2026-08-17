using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScene : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip click;
    public TMP_Text deathText;
    public TMP_Text highScore;

    void Start()
    {
        Debug.Log("deathText found: " + deathText.name);
        deathText.text = "You have survived " + GameStats.daysSurvived + " days.";
        highScore.text = "High Score: " + GameStats.highScore + " days.";
    }


    public void RestartGame()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("Playable");
    }
    public void Home()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
