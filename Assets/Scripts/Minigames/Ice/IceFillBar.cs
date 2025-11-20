using UnityEngine;

public class IceFillBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fill = 0f;   

    Vector3 _fullScale;        
    Vector3 _bottomLocalPos;   

    void Awake()
    {
        _fullScale = transform.localScale;

        _bottomLocalPos = transform.localPosition - new Vector3(0f, _fullScale.y * 0.5f, 0f);

        ApplyFill();
    }

    void ApplyFill()
    {
        float clamped = Mathf.Clamp01(fill);

        Vector3 s = _fullScale;
        s.y = _fullScale.y * clamped;
        transform.localScale = s;

        Vector3 newCenter = _bottomLocalPos + new Vector3(0f, s.y * 0.5f, 0f);
        transform.localPosition = newCenter;
    }

    public void SetFill(float value)
    {
        fill = Mathf.Clamp01(value);
        ApplyFill();
    }

    public void AddFill(float delta)
    {
        SetFill(fill + delta);
    }
}
