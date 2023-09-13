using UnityEngine;

public class StaticCell : MonoBehaviour
{
    public RectTransform staticCell_rect;

    private RectTransform content_rect;    
    private float x_Spacing = 30f; // Adjust this spacing as needed
    private float y_Spacing = 40f;

    private void Start()
    {
        content_rect=GetComponentInParent<GameObject>().gameObject.transform as RectTransform;
    }
    private void Update()
    {
        // Calculate the position for the static cell
        float xOffset = (content_rect.childCount - 1) * (staticCell_rect.rect.x + x_Spacing);
        float yOffset = (content_rect.childCount - 1) * (staticCell_rect.rect.y + y_Spacing);
        staticCell_rect.anchoredPosition = new Vector2(xOffset, -yOffset);
    }
}