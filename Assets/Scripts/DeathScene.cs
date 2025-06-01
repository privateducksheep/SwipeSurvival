using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScene : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadSceneAsync("Playable");
    }
    public void Home()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
