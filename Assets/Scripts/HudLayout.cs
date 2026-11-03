using TMPro;
using UnityEngine;

/// <summary>
/// Keeps the day counter and the resource panel from overlapping. On wide
/// screens the counter sits in the top-left corner; when the screen is too
/// narrow for both on one row (phones held upright), it moves under the
/// resource panel on the right.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[DefaultExecutionOrder(-150)] //Before CameraFit, which keeps the card clear of the counter
public class HudLayout : MonoBehaviour
{
    public RectTransform resourcePanel;
    public TMP_Text dayCounter;
    public float gap = 24f;

    private Vector2 cornerPosition;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        cornerPosition = dayCounter.rectTransform.anchoredPosition;
    }

    void LateUpdate()
    {
        RectTransform area = (RectTransform)transform;
        RectTransform counter = dayCounter.rectTransform;

        //Resource panel edges in this container's local space
        resourcePanel.GetWorldCorners(corners);
        Vector3 panelBottomLeft = area.InverseTransformPoint(corners[0]);
        Vector3 panelTopRight = area.InverseTransformPoint(corners[2]);
        float panelLeft = panelBottomLeft.x - area.rect.xMin;
        float panelBelow = area.rect.yMax - panelBottomLeft.y;
        float panelRight = area.rect.xMax - panelTopRight.x;

        bool fitsOnOneRow = cornerPosition.x + counter.rect.width + gap <= panelLeft;

        if (fitsOnOneRow)
        {
            counter.anchorMin = counter.anchorMax = counter.pivot = new Vector2(0f, 1f);
            counter.anchoredPosition = cornerPosition;
            dayCounter.alignment = TextAlignmentOptions.TopLeft;
        }
        else
        {
            counter.anchorMin = counter.anchorMax = counter.pivot = new Vector2(1f, 1f);
            counter.anchoredPosition = new Vector2(-panelRight, -(panelBelow + gap * 0.5f));
            dayCounter.alignment = TextAlignmentOptions.TopRight;
        }
    }
}
