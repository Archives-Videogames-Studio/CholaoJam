using UnityEngine;

/// <summary>
/// Mueve la lechera horizontalmente entre dos límites.
/// Hace trayectos largos (no zig-zag rápidos) y da la vuelta
/// cerca de los bordes con un poco de aleatoriedad.
/// </summary>
public class LecheraMover : MonoBehaviour
{
    [Header("Límites de movimiento")]
    public Transform leftLimit;   // waypoint izquierdo
    public Transform rightLimit;  // waypoint derecho

    [Header("Movimiento")]
    public float speed = 1.2f;    // velocidad de la lechera

    [Header("Zona donde puede girar (porcentaje del tramo)")]
    [Range(0.5f, 1f)]
    [Tooltip("Qué tan cerca del borde mínimo empieza a considerar girar (0.8 = 80% del tramo).")]
    public float minEdgeFactor = 0.7f;

    [Range(0.5f, 1f)]
    [Tooltip("Hasta qué porcentaje máximo del tramo puede llegar antes de girar.")]
    public float maxEdgeFactor = 0.95f;

    private bool _canMove = false;
    private int _direction = 1;      // +1 derecha, -1 izquierda
    private float _nextTurnX;        // x donde debe girar

    private void Start()
    {
        if (leftLimit == null || rightLimit == null)
        {
            Debug.LogError("[LecheraMover] Faltan límites izquierdo/derecho.");
            enabled = false;
            return;
        }

        ClampInsideLimits();
        PickNextTurn();
    }

    /// <summary>
    /// Llamado desde el minijuego para activar/desactivar el movimiento.
    /// </summary>
    public void EnableMovement(bool enable)
    {
        _canMove = enable;

        if (!enable)
            return;

        // Cuando empezamos, elegimos una dirección aleatoria
        _direction = Random.value < 0.5f ? -1 : 1;
        ClampInsideLimits();
        PickNextTurn();
    }

    private void Update()
    {
        if (!_canMove) return;

        Vector3 pos = transform.position;

        // Movimiento base
        pos.x += _direction * speed * Time.deltaTime;

        float leftX  = leftLimit.position.x;
        float rightX = rightLimit.position.x;

        // Aseguramos que no salga de los límites
        pos.x = Mathf.Clamp(pos.x, leftX, rightX);
        transform.position = pos;

        // ¿Ya pasamos el punto de giro o tocamos el borde?
        if (_direction > 0)
        {
            // Vamos a la derecha
            if (pos.x >= _nextTurnX || pos.x >= rightX - 0.01f)
            {
                _direction = -1;
                PickNextTurn();
            }
        }
        else
        {
            // Vamos a la izquierda
            if (pos.x <= _nextTurnX || pos.x <= leftX + 0.01f)
            {
                _direction = 1;
                PickNextTurn();
            }
        }
    }

    /// <summary>
    /// Mantiene la lechera dentro de los límites al iniciar.
    /// </summary>
    private void ClampInsideLimits()
    {
        float leftX  = leftLimit.position.x;
        float rightX = rightLimit.position.x;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftX, rightX);
        transform.position = pos;
    }

    /// <summary>
    /// Elige un nuevo punto de giro cerca del borde hacia el que se mueve.
    /// No gira en medio del camino, sino en el último 70–95% del tramo.
    /// </summary>
    private void PickNextTurn()
    {
        float leftX   = leftLimit.position.x;
        float rightX  = rightLimit.position.x;
        float current = transform.position.x;

        float width = rightX - leftX;
        if (width <= 0f)
        {
            _nextTurnX = current;
            return;
        }

        // Aseguramos orden correcto de factores
        float fMin = Mathf.Min(minEdgeFactor, maxEdgeFactor);
        float fMax = Mathf.Max(minEdgeFactor, maxEdgeFactor);

        fMin = Mathf.Clamp(fMin, 0.5f, 1f);
        fMax = Mathf.Clamp(fMax, fMin, 1f);

        float factor = Random.Range(fMin, fMax);

        if (_direction > 0)
        {
            float fromLeft = (current - leftX) / width;
            float targetFactor = Mathf.Max(fromLeft, factor);

            _nextTurnX = Mathf.Lerp(leftX, rightX, targetFactor);
        }
        else
        {
            float fromRight = (rightX - current) / width;
            float targetFactor = Mathf.Max(fromRight, factor);

            float t = 1f - targetFactor;
            _nextTurnX = Mathf.Lerp(leftX, rightX, t);
        }
    }
}
