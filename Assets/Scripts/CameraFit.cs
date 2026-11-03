using UnityEngine;

/// <summary>
/// Keeps a design area of the scene visible on every screen shape.
/// Wider screens see extra scenery at the sides (BackgroundFit stretches the
/// background to cover it); narrow or portrait windows zoom out instead of
/// cropping the content. Optionally keeps a content box clear of corner HUD panels.
/// </summary>
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class CameraFit : MonoBehaviour
{
    [Tooltip("Width in world units that must always be visible at the content plane.")]
    public float designWidth = 16f;
    [Tooltip("Height in world units that must always be visible at the content plane.")]
    public float designHeight = 10f;
    [Tooltip("Distance from the camera to the content plane. Only used by perspective cameras.")]
    public float contentDistance = 10f;

    [Header("Optional: top-corner HUD to keep clear of")]
    [Tooltip("Panels on a Screen Space Overlay canvas in the top corners. The content box never sits underneath them.")]
    public RectTransform[] keepClearOf;
    [Tooltip("World-space box (centred horizontally on the camera) that must stay clear of those panels.")]
    public Rect contentBox = new Rect(-3.3f, -3.7f, 6.6f, 9.7f);

    private Camera fitCamera;
    private float designCentreY;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        fitCamera = GetComponent<Camera>();
        designCentreY = transform.position.y;
        Fit();
    }

    //Runs every frame (it is cheap), which also covers window resizes and full
    //screen toggles. LateUpdate so the UI has already been rescaled this frame.
    void LateUpdate()
    {
        Fit();
    }

    public void Fit()
    {
        float aspect = fitCamera.aspect;
        float halfHeight = Mathf.Max(designHeight * 0.5f, designWidth * 0.5f / aspect);
        float centreY = designCentreY;

        //Two passes so that panels on both sides settle on one zoom and position
        for (int pass = 0; pass < 2; pass++)
        {
            ClearHud(aspect, ref halfHeight, ref centreY);
        }

        Vector3 position = transform.position;
        position.y = centreY;
        transform.position = position;

        if (fitCamera.orthographic)
        {
            fitCamera.orthographicSize = halfHeight;
        }
        else
        {
            fitCamera.fieldOfView = 2f * Mathf.Atan(halfHeight / contentDistance) * Mathf.Rad2Deg;
        }
    }

    /// <summary>
    /// If the content box would sit underneath a HUD panel, either zoom out until
    /// it clears the panel sideways, or zoom out a little less and raise the camera
    /// so the content sits below the panel, whichever keeps the content bigger.
    /// </summary>
    void ClearHud(float aspect, ref float halfHeight, ref float centreY)
    {
        if (keepClearOf == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        float contentHalfWidth = Mathf.Max(Mathf.Abs(contentBox.xMin), Mathf.Abs(contentBox.xMax));

        foreach (RectTransform panel in keepClearOf)
        {
            if (panel == null || !panel.gameObject.activeInHierarchy)
                continue;

            //On an overlay canvas, world corners are screen pixels
            panel.GetWorldCorners(corners);
            float left = corners[0].x / Screen.width;
            float right = corners[2].x / Screen.width;
            float bottom = corners[0].y / Screen.height;

            //Gap between the centre line and the panel's inner edge, as a screen fraction
            float sideGap = (left + right) > 1f ? left - 0.5f : 0.5f - right;

            bool overlapsSideways = contentHalfWidth / (2f * halfHeight * aspect) > sideGap;
            bool overlapsBelow = (contentBox.yMax - (centreY - halfHeight)) / (2f * halfHeight) > bottom;
            if (!overlapsSideways || !overlapsBelow)
                continue;

            float clearSideways = sideGap > 0f ? contentHalfWidth / (2f * sideGap * aspect) : float.MaxValue;
            float clearBelow = bottom > 0f ? contentBox.height / (2f * bottom) : float.MaxValue;

            if (clearSideways <= clearBelow)
            {
                halfHeight = Mathf.Max(halfHeight, clearSideways);
            }
            else
            {
                halfHeight = Mathf.Max(halfHeight, clearBelow);
                //Raise the camera so the top of the content lines up with the bottom of the panel
                centreY = Mathf.Max(centreY, contentBox.yMax + halfHeight - 2f * halfHeight * bottom);
            }
        }
    }

    /// <summary>
    /// Half the visible width and height at a given world depth.
    /// </summary>
    public static Vector2 HalfViewAt(Camera viewCamera, float worldZ)
    {
        float halfHeight = viewCamera.orthographic
            ? viewCamera.orthographicSize
            : Mathf.Abs(worldZ - viewCamera.transform.position.z) * Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return new Vector2(halfHeight * viewCamera.aspect, halfHeight);
    }
}
