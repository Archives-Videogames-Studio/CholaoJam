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

    [Header("Referencias de juego")]
    public Transform barraCorte;
    public Transform leftLimit;
    public Transform rightLimit;
    public FruitCutBarMover barMover;

    [Header("UI Selección")]
    public GameObject selectionPanel;

    [Header("Línea ideal de corte")]
    public Transform idealLine;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1.5f;

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

        string fb = GetFeedback(selected, nivelFinal);
        ShowFeedback(fb);

        _finished = true;
        StartCoroutine(FinishAfterDelay());
    }

    int MapValorToNivel(float v)
    {
        if (v < 0.33f) return 0;   
        if (v < 0.66f) return 1;   
        return 2;                  
    }

    string GetFeedback(int selected, int result)
    {
        switch (selected)
        {
            case 0: 
                if (result == 0) return "¡Chimba!";
                if (result == 1) return "Melo!";
                return "Paila!";

            case 1: 
                if (result == 1) return "¡Chimba!";
                return "Melo!";

            case 2:
                if (result == 2) return "¡Chimba!";
                if (result == 1) return "Melo!";
                return "Paila!";
        }
        return "Melo!";
    }

    void ShowFeedback(string text)
    {
        if (feedbackText == null) return;

        feedbackText.text = text;
        feedbackText.gameObject.SetActive(true);
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
