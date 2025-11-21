using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ForceSiropeCanvasOrder : MonoBehaviour
{
    public int sortingOrder = 50;

    void Awake()
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
    }
}
