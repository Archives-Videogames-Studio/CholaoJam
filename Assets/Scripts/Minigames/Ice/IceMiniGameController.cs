using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class IceMiniGameController : MonoBehaviour
{
    public enum NivelSeleccion
    {
        Bajo = 0,
        Medio = 1,
        Alto  = 2
    }

    // === NUEVO: tipos de feedback visual ===
    public enum FeedbackType
    {
        Chimba,
        Melo,
        Paila
    }

    public static bool MachineInputEnabled { get; private set; } = false;

    [Header("Barra de Hielo")]
    public IceFillBar fillBar;

    [Header("Target / Objetivo")]
    public Transform targetMark;
    public Transform targetBottomRef;
    public Transform targetTopRef;

    [Tooltip("Porcentaje ideal para dificultad Baja (0–1).")]
    public float targetLow  = 0.30f;
    [Tooltip("Porcentaje ideal para dificultad Media (0–1).")]
    public float targetMed  = 0.55f;
    [Tooltip("Porcentaje ideal para dificultad Alta (0–1).")]
    public float targetHigh = 0.80f;

    [Header("UI Selección")]
    public GameObject selectionPanel;

    [Header("Velocidad de llenado por dificultad (por segundo)")]
    public float fillPerSecondLow    = 0.18f;
    public float fillPerSecondMedium = 0.25f;
    public float fillPerSecondHigh   = 0.32f;

    [Header("Extra al soltar espacio (progresivo)")]
    [Tooltip("Cantidad adicional de llenado al soltar ESPACIO (0–1).")]
    public float extraFillOnRelease = 0.07f;
    [Tooltip("Duración en segundos del extra progresivo.")]
    public float extraFillDuration = 0.25f;

    [Header("Feedback (texto opcional)")]
    [Tooltip("Texto que muestra 'Chimba! / Melo! / Paila!' (puedes dejarlo vacío si solo usas sprites).")]
    public TextMeshProUGUI feedbackText;
    [Tooltip("Tiempo que se muestra el feedback antes de volver a CristoRey.")]
    public float feedbackDuration = 1.5f;

    [Header("Feedback visual (sprites)")]
    [Tooltip("SpriteRenderer donde se pintará CHIMBA / MELO / PAILA.")]
    public SpriteRenderer feedbackSprite;
    public Sprite chimbaSprite;
    public Sprite meloSprite;
    public Sprite pailaSprite;

    [Header("Referencias máquina (rueda + partículas)")]
    public Guiro guiro;
    public Ice ice;

    [Header("Debug")]
    public NivelSeleccion currentLevel;
    public float currentFillPerSecond;
    public bool canPlay;

    bool _hasStarted = false;
    bool _finished   = false;

    bool  _addingExtra      = false;
    float _extraTime        = 0f;
    float _extraStartFill   = 0f;
    float _extraTargetFill  = 0f;

    void Start()
    {
        SetMachineInput(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (fillBar != null)
            fillBar.SetFill(0f);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (feedbackSprite != null)
            feedbackSprite.gameObject.SetActive(false);

        canPlay        = false;
        currentLevel   = NivelSeleccion.Medio;
        currentFillPerSecond = 0f;
        _hasStarted    = false;
        _finished      = false;

        UpdateTargetPosition(targetMed);
    }

    void OnDestroy()
    {
        SetMachineInput(false);
    }

    void Update()
    {
        if (!canPlay || fillBar == null)
            return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (_addingExtra)
        {
            _extraTime += Time.deltaTime;
            float t = Mathf.Clamp01(_extraTime / extraFillDuration);
            float newFill = Mathf.Lerp(_extraStartFill, _extraTargetFill, t);
            fillBar.SetFill(newFill);

            if (t >= 1f)
            {
                _addingExtra = false;
                _finished    = true;
                canPlay      = false;

                Debug.Log($"[HIELO] Minijuego terminado (extra completado). Llenado final = {fillBar.fill:F2}");

                CompleteMinigame();
            }

            return;
        }

        if (_finished)
            return;

        if (!_hasStarted && kb.spaceKey.wasPressedThisFrame)
        {
            _hasStarted = true;
            Debug.Log("[HIELO] Minijuego iniciado (primer SPACE).");
        }

        if (_hasStarted && !_finished)
        {
            if (kb.spaceKey.isPressed)
            {
                float delta = currentFillPerSecond * Time.deltaTime;
                fillBar.AddFill(delta);
            }

            if (kb.spaceKey.wasReleasedThisFrame)
            {
                SetMachineInput(false);
                if (guiro != null) guiro.ForceStop();
                if (ice   != null) ice.ForceStopEmission();

                _addingExtra     = true;
                _extraTime       = 0f;
                _extraStartFill  = fillBar.fill;
                _extraTargetFill = Mathf.Clamp01(fillBar.fill + extraFillOnRelease);

                Debug.Log($"[HIELO] SPACE suelto, iniciando extra. De {_extraStartFill:F2} a {_extraTargetFill:F2}");
            }
        }
    }

    public void OnSelectLow()    => SetDifficulty(NivelSeleccion.Bajo);
    public void OnSelectMedium() => SetDifficulty(NivelSeleccion.Medio);
    public void OnSelectHigh()   => SetDifficulty(NivelSeleccion.Alto);

    void SetDifficulty(NivelSeleccion level)
    {
        currentLevel = level;

        switch (level)
        {
            case NivelSeleccion.Bajo:
                currentFillPerSecond = fillPerSecondLow;
                UpdateTargetPosition(targetLow);
                break;
            case NivelSeleccion.Medio:
                currentFillPerSecond = fillPerSecondMedium;
                UpdateTargetPosition(targetMed);
                break;
            case NivelSeleccion.Alto:
                currentFillPerSecond = fillPerSecondHigh;
                UpdateTargetPosition(targetHigh);
                break;
        }

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (fillBar != null)
            fillBar.SetFill(0f);

        canPlay      = true;
        _hasStarted  = false;
        _finished    = false;
        _addingExtra = false;

        SetMachineInput(true);

        Debug.Log($"[HIELO] Dificultad {level}, fillPerSecond={currentFillPerSecond}");

        if (CholadoGameState.Instance != null)
        {
            CholadoGameState.Instance.selectedFrio = (int)currentLevel;
        }
    }

    void UpdateTargetPosition(float porcentaje)
    {
        if (targetMark == null || targetBottomRef == null || targetTopRef == null)
            return;

        float t = Mathf.Clamp01(porcentaje);

        Vector3 bottom = targetBottomRef.localPosition;
        Vector3 top    = targetTopRef.localPosition;

        Vector3 newPos = Vector3.Lerp(bottom, top, t);
        newPos.z = targetMark.localPosition.z;

        targetMark.localPosition = newPos;
    }

    void SetMachineInput(bool enabled)
    {
        MachineInputEnabled = enabled;
    }

    int MapFillToLevel(float fill)
    {
        float v = Mathf.Clamp01(fill);

        if (v < 0.33f) return 0;
        else if (v < 0.66f) return 1;
        else return 2;
    }

    // ===== NUEVO: feedback como enum =====
    FeedbackType GetFeedbackType(int selected, int result)
    {
        switch (selected)
        {
            case 0: // eligió BAJO
                if (result == 0) return FeedbackType.Chimba;
                if (result == 1) return FeedbackType.Melo;
                return FeedbackType.Paila;

            case 1: // eligió MEDIO
                if (result == 1) return FeedbackType.Chimba;
                return FeedbackType.Melo;

            case 2: // eligió ALTO
                if (result == 2) return FeedbackType.Chimba;
                if (result == 1) return FeedbackType.Melo;
                return FeedbackType.Paila;
        }
        return FeedbackType.Melo;
    }

    string FeedbackTypeToText(FeedbackType type)
    {
        switch (type)
        {
            case FeedbackType.Chimba: return "¡Chimba!";
            case FeedbackType.Melo:   return "Melo!";
            case FeedbackType.Paila:  return "Paila!";
        }
        return "Melo!";
    }

    void CompleteMinigame()
    {
        float finalFill = fillBar != null ? fillBar.fill : 0f;
        int levelFromFill = MapFillToLevel(finalFill);

        var state = CholadoGameState.Instance;
        int selected = (state != null) ? state.selectedFrio : (int)currentLevel;

        if (state != null)
        {
            state.resultFrio = levelFromFill;
            state.hasFrio    = true;

            Debug.Log($"[HIELO] selectedFrio={state.selectedFrio}, " +
                      $"resultFrio={state.resultFrio}, idealFrio={state.idealFrio}");
        }
        if (state != null && state.choladoVisual != null)
        {
            state.choladoVisual.RefreshFromState();
        }

        FeedbackType fbType = GetFeedbackType(selected, levelFromFill);
        ShowFeedback(fbType);

        StartCoroutine(FinishAfterDelay());
    }

    void ShowFeedback(FeedbackType type)
    {
        // Texto opcional
        if (feedbackText != null)
        {
            feedbackText.text = FeedbackTypeToText(type);
            feedbackText.gameObject.SetActive(true);
        }

        // Sprite visual
        if (feedbackSprite != null)
        {
            switch (type)
            {
                case FeedbackType.Chimba:
                    feedbackSprite.sprite = chimbaSprite;
                    break;
                case FeedbackType.Melo:
                    feedbackSprite.sprite = meloSprite;
                    break;
                case FeedbackType.Paila:
                    feedbackSprite.sprite = pailaSprite;
                    break;
            }

            feedbackSprite.gameObject.SetActive(true);
        }
    }

    System.Collections.IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);

        var state = CholadoGameState.Instance;

        System.Action midAction = () =>
        {
            if (state != null && state.cristoReyRoot != null)
            {
                state.cristoReyRoot.SetActive(true);
            }

            Scene thisScene = gameObject.scene;
            SceneManager.UnloadSceneAsync(thisScene);
        };

        if (ScreenCurtain.Instance != null)
            ScreenCurtain.Instance.RunTransition(midAction);
        else
            midAction();
    }
}
