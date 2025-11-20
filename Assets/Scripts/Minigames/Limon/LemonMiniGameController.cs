using UnityEngine;
using UnityEngine.InputSystem;   // New Input System
using TMPro;                    // Para TextMeshProUGUI

/// Minijuego Limón:
/// - Elegir dificultad (Bajo/Medio/Alto).
/// - Alternar A y D para llenar la barra.
/// - Tienes totalTime segundos.
/// - ACIDEZ depende de dificultad + si completaste o no.
///
/// Valores ACIDEZ:
/// 0 = Bajo, 1 = Medio (neutro), 2 = Alto
public class LemonMiniGameController : MonoBehaviour
{
    public enum NivelSeleccion
    {
        Bajo = 0,
        Medio = 1,
        Alto  = 2
    }

    [Header("Barra")]
    public LemonPressureBar pressureBar;

    [Header("UI Selección")]
    public GameObject selectionPanel;   // Panel con botones Bajo/Medio/Alto

    [Header("UI Tiempo")]
    public TextMeshProUGUI timerLabel;  // Texto donde mostramos el tiempo

    [Header("Tiempo")]
    public float totalTime = 10f;       // Tiempo límite en segundos
    [SerializeField] 
    private float remainingTime;        // Tiempo restante (debug)

    [Header("Incremento por golpe (dificultad)")]
    public float tapAmountLow    = 0.08f;
    public float tapAmountMedium = 0.05f;
    public float tapAmountHigh   = 0.03f;

    [Header("Resultado")]
    [Range(0f, 1f)]
    public float valorAccion;          // Llenado final 0–1 (solo informativo)
    public int   ACIDEZ = 1;           // 0 = bajo, 1 = medio, 2 = alto (neutro = 1)
    public NivelSeleccion currentLevel;
    public float tapAmountCurrent;
    public bool canPlay;

    bool _expectingLeft = true;        // true = espero A, false = espero D
    bool _finished = false;

    void Start()
    {
        // Mostrar panel de selección al inicio
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        // Barra vacía
        if (pressureBar != null)
            pressureBar.SetFill(0f);

        canPlay       = false;
        _finished     = false;
        _expectingLeft = true;

        // Valor neutro por defecto
        ACIDEZ        = 1;
        remainingTime = 0f;

        UpdateTimerUI();
    }

    void Update()
    {
        // Si no se está jugando, solo actualiza el timer a "--"
        if (!canPlay || _finished || pressureBar == null)
        {
            UpdateTimerUI();
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // 1) Tiempo
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerUI();
            FinishMiniGame(completedInTime: false);
            return;
        }

        // 2) Alternar A y D
        if (_expectingLeft)
        {
            if (kb.aKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = false; // ahora toca D
            }
        }
        else
        {
            if (kb.dKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = true; // ahora toca A
            }
        }

        // 3) Si llenó la barra antes de tiempo, termina (éxito)
        if (pressureBar.fill >= 0.999f)
        {
            FinishMiniGame(completedInTime: true);
            return;
        }

        // 4) Actualizar HUD del tiempo
        UpdateTimerUI();
    }

    // ========================
    //  UI TIEMPO
    // ========================

    void UpdateTimerUI()
    {
        if (timerLabel == null) return;

        // Si no se está jugando, mostramos "--"
        if (!canPlay || _finished)
        {
            timerLabel.text = "--";
            return;
        }

        float t = Mathf.Max(0f, remainingTime);
        timerLabel.text = t.ToString("0.0") + " s";
    }

    // ========================
    //  BOTONES DE SELECCIÓN
    // ========================

    public void OnSelectLow()
    {
        SetDifficulty(NivelSeleccion.Bajo);
    }

    public void OnSelectMedium()
    {
        SetDifficulty(NivelSeleccion.Medio);
    }

    public void OnSelectHigh()
    {
        SetDifficulty(NivelSeleccion.Alto);
    }

    void SetDifficulty(NivelSeleccion level)
    {
        currentLevel = level;

        switch (level)
        {
            case NivelSeleccion.Bajo:
                tapAmountCurrent = tapAmountLow;
                break;
            case NivelSeleccion.Medio:
                tapAmountCurrent = tapAmountMedium;
                break;
            case NivelSeleccion.Alto:
                tapAmountCurrent = tapAmountHigh;
                break;
        }

        // Ocultar panel
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Reset de barra y tiempo
        if (pressureBar != null)
            pressureBar.SetFill(0f);

        remainingTime  = totalTime;
        canPlay        = true;
        _finished      = false;
        _expectingLeft = true;

        UpdateTimerUI();

        Debug.Log($"[LIMON] Dificultad {level}, tap={tapAmountCurrent}, tiempo={totalTime}s");
    }

    // ========================
    //  FIN DEL MINIJUEGO
    // ========================

    void FinishMiniGame(bool completedInTime)
    {
        if (_finished) return;

        _finished = true;
        canPlay   = false;

        valorAccion = pressureBar ? pressureBar.fill : 0f;

        // Lógica de ACIDEZ:
        // Bajo  → completo = 0, fallo = 1
        // Medio → completo = 1, fallo = 0
        // Alto  → completo = 2, fallo = 1
        ACIDEZ = ComputeAcidez(currentLevel, completedInTime);

        UpdateTimerUI();

        Debug.Log($"[LIMON] FIN → completado={completedInTime}, llenado={valorAccion:F2}, ACIDEZ={ACIDEZ}");
        // Aquí luego llamas a tu GameManager para guardar ACIDEZ y salir del minijuego.
    }

    int ComputeAcidez(NivelSeleccion level, bool completed)
    {
        switch (level)
        {
            case NivelSeleccion.Bajo:
                return completed ? 0 : 1;

            case NivelSeleccion.Medio:
                return completed ? 1 : 0;

            case NivelSeleccion.Alto:
                return completed ? 2 : 1;

            default:
                return 1; // neutro por seguridad
        }
    }
}
