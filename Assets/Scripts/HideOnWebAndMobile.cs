using UnityEngine;

/// <summary>
/// Hides things that do nothing in a browser or on a phone, such as the Exit
/// button: browsers ignore Application.Quit, and phone apps are expected to be
/// closed by the system rather than by the game.
/// </summary>
public class HideOnWebAndMobile : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID
        gameObject.SetActive(false);
#endif
    }
}
