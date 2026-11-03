using UnityEngine;

/// <summary>
/// Shrinks this full-screen UI container to the screen's safe area, so its
/// contents never sit behind a notch, rounded corner or camera cutout.
/// In a normal desktop window the safe area is the whole window.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private Rect appliedArea;
    private Vector2Int appliedScreen;

    void Awake()
    {
        Apply();
    }

    void Update()
    {
        Apply();
    }

    void Apply()
    {
        Rect area = Screen.safeArea;
        Vector2Int screen = new Vector2Int(Screen.width, Screen.height);
        if (area == appliedArea && screen == appliedScreen)
            return;
        if (screen.x <= 0 || screen.y <= 0)
            return;

        appliedArea = area;
        appliedScreen = screen;

        RectTransform rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(area.xMin / screen.x, area.yMin / screen.y);
        rect.anchorMax = new Vector2(area.xMax / screen.x, area.yMax / screen.y);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
