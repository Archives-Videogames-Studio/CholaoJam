using UnityEngine;

public class LemonPressureBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fill = 0f;

    Vector3 _initialScale;

    void Awake()
    {
        _initialScale = transform.localScale;

        if (_initialScale.x == 0f) _initialScale.x = 1f;
        if (_initialScale.y == 0f) _initialScale.y = 0.6f;
        if (_initialScale.z == 0f) _initialScale.z = 1f;

        ApplyFill();
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;
        _initialScale = transform.localScale;
        ApplyFill();
    }

    void ApplyFill()
    {
        float clamped = Mathf.Clamp01(fill);
        var s = _initialScale;
        s.x = _initialScale.x * clamped;
        transform.localScale = s;
    }

    public void SetFill(float value)
    {
        fill = Mathf.Clamp01(value);
        ApplyFill();
    }

    public void AddFill(float delta)
    {
        SetFill(fill + delta);
        Debug.Log($"[LIMON] fill = {fill:F2}");
    }
}
