using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lays out the options screen. On landscape screens the whole framed panel is
/// shown and the controls sit on it. Square and upright screens (phones, tablets
/// held upright) are too narrow for that: the controls would be squeezed into a
/// thin strip. There the panel fills the screen from top to bottom instead, and
/// the controls spread out over it.
/// </summary>
[RequireComponent(typeof(AspectRatioFitter))]
[DefaultExecutionOrder(-150)] //Before CameraFit
public class OptionsPanelLayout : MonoBehaviour
{
    public CameraFit cameraFit;
    [Tooltip("Size of the framed panel in world units.")]
    public Vector2 panelSize;
    [Tooltip("Screens narrower than this width-to-height ratio fill the height with the panel.")]
    public float fillHeightBelowAspect = 1.2f;

    private AspectRatioFitter fitter;
    private bool? appliedFillHeight;

    void Awake()
    {
        fitter = GetComponent<AspectRatioFitter>();
        Apply();
    }

    void Update()
    {
        Apply();
    }

    void Apply()
    {
        if (Screen.height <= 0)
            return;
        bool fillHeight = (float)Screen.width / Screen.height < fillHeightBelowAspect;
        if (appliedFillHeight == fillHeight)
            return;
        appliedFillHeight = fillHeight;

        //Wide screens show the whole panel; narrow ones fill the height and crop the sides
        cameraFit.designWidth = fillHeight ? 0f : panelSize.x;
        cameraFit.designHeight = panelSize.y;

        if (fillHeight)
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.None;
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = panelSize.x / panelSize.y;
        }
    }
}
