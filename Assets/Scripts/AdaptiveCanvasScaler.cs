using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the UI scale for any screen: desktop windows of every shape, browser
/// windows and phones in either orientation.
///
/// Landscape screens use the original 800 x 600 layout. Portrait screens use a
/// narrower layout so the UI is not shrunk to fit 800 units across a phone.
/// On phones and in browsers the UI is also kept at a readable, tappable size,
/// as long as the layout still fits on screen.
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
[DefaultExecutionOrder(-200)]
public class AdaptiveCanvasScaler : MonoBehaviour
{
    public Vector2 landscapeReference = new Vector2(800f, 600f);
    public Vector2 portraitReference = new Vector2(450f, 800f);
    [Tooltip("Smallest UI unit, in points (CSS pixels in a browser), on phones and in browsers.")]
    public float minimumUnitInPoints = 0.8f;
    [Tooltip("The layout needs at least this many UI units, so the minimum size never pushes UI off screen.")]
    public Vector2 minimumLandscapeCanvas = new Vector2(560f, 440f);
    public Vector2 minimumPortraitCanvas = new Vector2(330f, 560f);

    /// <summary>For layout testing only: pretend the screen has this many pixels per point.</summary>
    public static float pixelsPerPointOverride = 0f;

    private CanvasScaler scaler;
    private Vector2Int appliedScreen;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern float SwipeSurvival_CanvasCssWidth();
#endif

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        Apply();
    }

    void Update()
    {
        Apply();
    }

    void Apply()
    {
        Vector2Int screen = new Vector2Int(Screen.width, Screen.height);
        if (screen == appliedScreen || screen.x <= 0 || screen.y <= 0)
            return;
        appliedScreen = screen;

        bool portrait = screen.y > screen.x;
        Vector2 reference = portrait ? portraitReference : landscapeReference;
        Vector2 minimumCanvas = portrait ? minimumPortraitCanvas : minimumLandscapeCanvas;

        //Same as the Canvas Scaler's "Expand" mode: the whole reference layout always fits
        float scale = Mathf.Min(screen.x / reference.x, screen.y / reference.y);

        float pixelsPerPoint = PixelsPerPoint();
        if (pixelsPerPoint > 0f)
        {
            float readable = minimumUnitInPoints * pixelsPerPoint;
            float stillFits = Mathf.Min(screen.x / minimumCanvas.x, screen.y / minimumCanvas.y);
            if (scale < readable)
                scale = Mathf.Max(scale, Mathf.Min(readable, stillFits));
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = scale;
    }

    /// <summary>
    /// Physical pixel density, or 0 where the original desktop scaling should be kept.
    /// </summary>
    static float PixelsPerPoint()
    {
        if (pixelsPerPointOverride > 0f)
            return pixelsPerPointOverride;
#if UNITY_WEBGL && !UNITY_EDITOR
        float cssWidth = SwipeSurvival_CanvasCssWidth();
        return cssWidth > 0f ? Screen.width / cssWidth : 0f;
#else
        if (Application.isMobilePlatform && Screen.dpi > 0f)
            return Screen.dpi / 160f;
        return 0f;
#endif
    }
}
