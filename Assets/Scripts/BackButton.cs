using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip click;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Back()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
