using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class FruitMiniGameController : MonoBehaviour
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

    [Header("Referencias de juego")]
    public Transform barraCorte;
    public Transform leftLimit;
    public Transform rightLimit;
    public FruitCutBarMover barMover;

    [Header("UI Selección")]
    public GameObject selectionPanel;

    [Header("Línea ideal de corte")]
    public Transform idealLine;

    [Header("Feedback (texto opcional)")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1.5f;

    [Header("Feedback visual (sprites)")]
    public SpriteRenderer feedbackSprite;
    public Sprite chimbaSprite;
    public Sprite meloSprite;
    public Sprite pailaSprite;

    [Header("Resultado (debug)")]
    [Range(0f, 1f)] public float valorAccion;
    public int nivelFinal;
    public int FRUTA;
    public NivelSeleccion seleccionInicial;

    bool _hasCut = false;
    bool _canCut = false;
    bool _finished = false;

    float _idealBaseY;
    float _idealBaseZ;

    void Start()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (barMover != null)
            barMover.enabled = false;

        if (idealLine != null)
        {
            _idealBaseY = idealLine.position.y;
            _idealBaseZ = idealLine.position.z;
            idealLine.gameObject.SetActive(false);
        }

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (feedbackSprite != null)
            feedbackSprite.gameObject.SetActive(false);

        _hasCut   = false;
        _canCut   = false;
        _finished = false;
    }

    void Update()
    {
        if (_finished || !_canCut || _hasCut)
            return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame)
        {
            _hasCut = true;
            ProcesarCorte();
        }
    }

    public void OnSelectLow()    => SetSeleccion(NivelSeleccion.Bajo);
    public void OnSelectMedium() => SetSeleccion(NivelSeleccion.Medio);
    public void OnSelectHigh()   => SetSeleccion(NivelSeleccion.Alto);

    void SetSeleccion(NivelSeleccion nivel)
    {
        seleccionInicial = nivel;

        if (idealLine != null && leftLimit != null && rightLimit != null)
        {
            float t;
            switch (nivel)
            {
                case NivelSeleccion.Bajo:  t = 0.25f; break;
                case NivelSeleccion.Medio: t = 0.50f; break;
                case NivelSeleccion.Alto:  t = 0.75f; break;
                default:                   t = 0.50f; break;
            }

            Vector3 pos = Vector3.Lerp(leftLimit.position, rightLimit.position, t);
            pos.y = _idealBaseY;
            pos.z = _idealBaseZ;
            idealLine.position = pos;
            idealLine.gameObject.SetActive(true);
        }

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (barMover != null)
            barMover.enabled = true;

        _canCut   = true;
        _hasCut   = false;
        _finished = false;

        Debug.Log($"[FRUTA] Selección inicial: {seleccionInicial}");

        var state = CholadoGameState.Instance;
        if (state != null)
        {
            state.selectedFruta = (int)seleccionInicial;
        }
    }

    void ProcesarCorte()
    {
        if (!barraCorte || !leftLimit || !rightLimit)
        {
            Debug.LogWarning("[FRUTA] Faltan referencias en FruitMiniGameController.");
            return;
        }

        if (barMover != null)
            barMover.enabled = false;

        float minX = leftLimit.position.x;
        float maxX = rightLimit.position.x;
        float cutX = Mathf.Clamp(barraCorte.position.x, minX, maxX);

        valorAccion = Mathf.InverseLerp(minX, maxX, cutX);

        nivelFinal = MapValorToNivel(valorAccion);
        FRUTA      = nivelFinal;

        Debug.Log($"[FRUTA] Selección: {seleccionInicial} | Corte en {valorAccion:F2} → Nivel {nivelFinal} (FRUTA={FRUTA})");

        var state = CholadoGameState.Instance;
        int selected = (int)seleccionInicial;
        if (state != null)
        {
            state.resultFruta = nivelFinal;
            state.hasFruta    = true;
            Debug.Log($"[FRUTA] selectedFruta={state.selectedFruta}, resultFruta={state.resultFruta}, idealFruta={state.idealFruta}");
        }
        if (state != null && state.choladoVisual != null)
        {
            state.choladoVisual.RefreshFromState();
        }

        FeedbackType fbType = GetFeedbackType(selected, nivelFinal);
        ShowFeedback(fbType);

        _finished = true;
        StartCoroutine(FinishAfterDelay());
    }

    int MapValorToNivel(float v)
    {
        if (v < 0.33f) return 0;
        if (v < 0.66f) return 1;
        return 2;
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

    IEnumerator FinishAfterDelay()
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
