using UnityEngine;

/// <summary>
/// Grows a background sprite so it always fills the camera view, keeping its
/// proportions. It never shrinks below the scale set in the scene, so standard
/// screens look exactly as designed and only unusual shapes are adjusted.
/// The object should sit at the scene root without rotation.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFit : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 designScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        designScale = transform.localScale;
    }

    //Runs after CameraFit has set this frame's view
    void LateUpdate()
    {
        Camera viewCamera = Camera.main;
        if (viewCamera == null || spriteRenderer.sprite == null)
            return;

        Vector2 halfView = CameraFit.HalfViewAt(viewCamera, transform.position.z);
        Vector2 offset = transform.position - viewCamera.transform.position;
        Bounds sprite = spriteRenderer.sprite.bounds;

        //Half size the sprite must reach in every direction from its own pivot
        float neededX = halfView.x + Mathf.Abs(offset.x) + Mathf.Abs(sprite.center.x * designScale.x);
        float neededY = halfView.y + Mathf.Abs(offset.y) + Mathf.Abs(sprite.center.y * designScale.y);

        float grow = Mathf.Max(
            1f,
            neededX / (sprite.extents.x * designScale.x),
            neededY / (sprite.extents.y * designScale.y)
        );

        transform.localScale = new Vector3(designScale.x * grow, designScale.y * grow, designScale.z);
    }
}
