using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class FruitMiniGameController : MonoBehaviour
{
    public enum NivelSeleccion
    {
        Bajo = 0,
        Medio = 1,
        Alto = 2
    }

    [Header("Referencias de juego")]
    public Transform barraCorte;
    public Transform leftLimit;
    public Transform rightLimit;
    public FruitCutBarMover barMover;

    [Header("UI Selección")]
    public GameObject selectionPanel;   // Panel que tiene los botones

    [Header("Línea ideal de corte")]
    public Transform idealLine;

    [Header("Resultado (debug)")]
    [Range(0f, 1f)]
    public float valorAccion;   // 0–1 según posición de corte
    public int nivelFinal;      // 0, 1 o 2 (resultado del corte)
    public int FRUTA;           // variable final a guardar
    public NivelSeleccion seleccionInicial; // elección del jugador

    bool _hasCut = false;
    bool _canCut = false;      // solo se puede cortar después de elegir nivel

    void Start()
    {
        // Panel visible, barra parada
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (barMover != null)
            barMover.enabled = false;

        // Ocultar línea ideal al inicio
        if (idealLine != null)
            idealLine.gameObject.SetActive(false);

        _hasCut = false;
        _canCut = false;
    }


    void Update()
    {
        if (!_canCut || _hasCut)
            return;

        // New Input System: leer tecla espacio
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _hasCut = true;
            ProcesarCorte();
        }
    }

    // =========================
    //  BOTONES DE SELECCIÓN
    // =========================

    public void OnSelectLow()
    {
        SetSeleccion(NivelSeleccion.Bajo);
    }

    public void OnSelectMedium()
    {
        SetSeleccion(NivelSeleccion.Medio);
    }

    public void OnSelectHigh()
    {
        SetSeleccion(NivelSeleccion.Alto);
    }

    void SetSeleccion(NivelSeleccion nivel)
    {
        seleccionInicial = nivel;

        // 1) Calcular posición ideal según selección
        if (idealLine != null && leftLimit != null && rightLimit != null)
        {
            float t; // 0–1 entre izquierda y derecha

            switch (nivel)
            {
                case NivelSeleccion.Bajo:
                    t = 0.25f;   // más hacia la izquierda
                    break;
                case NivelSeleccion.Medio:
                    t = 0.5f;    // centro
                    break;
                case NivelSeleccion.Alto:
                    t = 0.75f;   // más hacia la derecha
                    break;
                default:
                    t = 0.5f;
                    break;
            }

            // Interpolamos entre los límites para hallar la posición X ideal
            Vector3 pos = Vector3.Lerp(leftLimit.position, rightLimit.position, t);

            // Alinear con la barra roja en Y/Z
            if (barraCorte != null)
            {
                pos.y = barraCorte.position.y;
                pos.z = barraCorte.position.z;
            }

            idealLine.position = pos;
            idealLine.gameObject.SetActive(true);
        }

        // 2) Ocultar panel de botones
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // 3) Activar movimiento de barra
        if (barMover != null)
            barMover.enabled = true;

        _canCut = true;

        Debug.Log($"[FRUTA] Selección inicial: {seleccionInicial}");
    }


    // =========================
    //  CÁLCULO DEL CORTE
    // =========================

    void ProcesarCorte()
    {
        if (!barraCorte || !leftLimit || !rightLimit)
        {
            Debug.LogWarning("[FRUTA] Faltan referencias en FruitMiniGameController.");
            return;
        }

        float minX = leftLimit.position.x;
        float maxX = rightLimit.position.x;
        float cutX = Mathf.Clamp(barraCorte.position.x, minX, maxX);

        // Normalizar a rango 0–1
        valorAccion = Mathf.InverseLerp(minX, maxX, cutX);

        // Mapear valorAccion (0–1) a nivel 0 / 1 / 2
        nivelFinal = MapValorToNivel(valorAccion);

        // Asignar resultado:
        FRUTA = nivelFinal;

        // Detener movimiento de la barra
        if (barMover != null)
            barMover.enabled = false;

        Debug.Log($"[FRUTA] Selección: {seleccionInicial} | Corte en {valorAccion:F2} → Nivel {nivelFinal} (FRUTA={FRUTA})");
    }

    int MapValorToNivel(float v)
    {
        // Ajusta umbrales si quieres
        if (v < 0.33f) return 0;   // poca fruta
        if (v < 0.66f) return 1;   // fruta media
        return 2;                  // mucha fruta
    }
}
