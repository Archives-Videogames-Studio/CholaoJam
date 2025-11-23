using UnityEngine;

public class IceFillBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fill = 0f;

    Vector3 _fullScale;

    void Awake()
    {
        // Escala completa (barra llena)
        _fullScale = transform.localScale;
        ApplyFill();
    }

    void ApplyFill()
    {
        float clamped = Mathf.Clamp01(fill);

        // Escalamos solo en Y, desde el pivote (que ahora está en la base)
        Vector3 s = _fullScale;
        s.y = _fullScale.y * clamped;
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
    }
}
