using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class SiropeMiniGameController : MonoBehaviour
{
    public enum NivelSeleccion
    {
        Bajo = 0,
        Medio = 1,
        Alto  = 2
    }

    [Header("Aguja / Medidor")]
    public Transform needle;
    public Transform meterBottomRef;
    public Transform meterTopRef;

    [Header("Zona verde (Target)")]
    public Transform zoneGreen;
    public Transform zoneBottomRef;
    public Transform zoneTopRef;

    [Header("UI Selección")]
    public GameObject selectionPanel;

    [Header("Rangos de zona (0–1)")]
    public Vector2 lowRange  = new Vector2(0.15f, 0.35f);
    public Vector2 medRange  = new Vector2(0.40f, 0.60f);
    public Vector2 highRange = new Vector2(0.65f, 0.85f);

    [Header("Movimiento de la aguja")]
    [Tooltip("Velocidad de oscilación de la aguja mientras se mantiene ESPACIO.")]
    public float oscSpeed = 4f;

    [Header("Partículas de sirope (decoración)")]
    public ParticleSystem syrupParticles;

    [Header("Feedback")]
    [Tooltip("Texto que muestra 'Chimba! / Melo! / Paila!'")]
    public TextMeshProUGUI feedbackText;
    [Tooltip("Tiempo que se muestra el feedback antes de volver a CristoRey.")]
    public float feedbackDuration = 1.5f;

    [Header("Resultado")]
    [Range(0f, 1f)] public float valorAccion;
    [Tooltip("Nivel de dulzor calculado (0=Bajo,1=Medio,2=Alto).")]
    public int DULZOR = 1;

    [Header("Debug")]
    public NivelSeleccion currentLevel = NivelSeleccion.Medio;
    public bool canPlay;
    public bool hasStarted;
    public bool finished;

    float _phase = 0f;
    float _lastValue = 0f;
    Vector3 _zoneOriginalScale;

    void Awake()
    {
        if (zoneGreen != null)
            _zoneOriginalScale = zoneGreen.localScale;
    }

    void Start()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            Debug.Log("[SIROPE] Start() -> SelectionPanel asignado y activado. " +
                      $"activeSelf={selectionPanel.activeSelf}, activeInHierarchy={selectionPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("[SIROPE] Start() -> SelectionPanel NO asignado en el inspector.");
        }

        canPlay    = false;
        hasStarted = false;
        finished   = false;

        MoveNeedleTo(0f);
        UpdateZoneVisual(medRange);

        if (syrupParticles != null)
        {
            syrupParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!canPlay || finished)
            return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (!hasStarted)
        {
            if (kb.spaceKey.wasPressedThisFrame)
            {
                hasStarted = true;
                _phase = -Mathf.PI * 0.5f;
                MoveNeedleTo(0f);
                Debug.Log("[SIROPE] Minijuego iniciado (primer SPACE).");
            }
            return;
        }

        if (hasStarted && !finished)
        {
            if (kb.spaceKey.isPressed)
            {
                _phase += oscSpeed * Time.deltaTime;
                float t = Mathf.Sin(_phase) * 0.5f + 0.5f; 
                _lastValue = t;
                MoveNeedleTo(t);
            }

            if (kb.spaceKey.wasReleasedThisFrame)
            {
                finished    = true;
                canPlay     = false;
                valorAccion = _lastValue;
                DULZOR      = MapValorToNivel(valorAccion);

                Debug.Log($"[SIROPE] Fin → valor={valorAccion:F2}, DULZOR={DULZOR}");

                var state = CholadoGameState.Instance;
                if (state != null)
                {
                    state.resultDulzor = DULZOR;
                    state.hasDulzor    = true;

                    Debug.Log($"[SIROPE] selectedDulzor={state.selectedDulzor}, " +
                              $"resultDulzor={state.resultDulzor}, idealDulzor={state.idealDulzor}");
                }

                if (syrupParticles != null)
                {
                    syrupParticles.Play();
                }

                int selected = (state != null) ? state.selectedDulzor : (int)currentLevel;
                string fb = GetFeedback(selected, DULZOR);
                ShowFeedback(fb);

                StartCoroutine(FinishAfterDelay());
            }
        }
    }


    public void OnSelectLow()    => SetDifficulty(NivelSeleccion.Bajo,   lowRange);
    public void OnSelectMedium() => SetDifficulty(NivelSeleccion.Medio,  medRange);
    public void OnSelectHigh()   => SetDifficulty(NivelSeleccion.Alto,   highRange);

    void SetDifficulty(NivelSeleccion nivel, Vector2 range)
    {
        currentLevel = nivel;

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        UpdateZoneVisual(range);

        canPlay    = true;
        hasStarted = false;
        finished   = false;
        _phase     = -Mathf.PI * 0.5f;
        _lastValue = 0f;

        MoveNeedleTo(0f);

        Debug.Log($"[SIROPE] Dificultad {nivel}, zona = [{range.x:F2}, {range.y:F2}]");

        var state = CholadoGameState.Instance;
        if (state != null)
        {
            state.selectedDulzor = (int)nivel;
        }
    }


    void MoveNeedleTo(float t)
    {
        if (needle == null || meterBottomRef == null || meterTopRef == null)
            return;

        t = Mathf.Clamp01(t);

        Vector3 bottom = meterBottomRef.localPosition;
        Vector3 top    = meterTopRef.localPosition;

        Vector3 newLocal = Vector3.Lerp(bottom, top, t);
        newLocal.z = needle.localPosition.z;

        needle.localPosition = newLocal;
    }

    void UpdateZoneVisual(Vector2 range)
    {
        if (zoneGreen == null || zoneBottomRef == null || zoneTopRef == null)
            return;

        float min = Mathf.Clamp01(range.x);
        float max = Mathf.Clamp01(range.y);

        Vector3 bottom = zoneBottomRef.localPosition;
        Vector3 top    = zoneTopRef.localPosition;

        float centerT   = (min + max) * 0.5f;
        Vector3 center  = Vector3.Lerp(bottom, top, centerT);
        center.z        = zoneGreen.localPosition.z;

        zoneGreen.localPosition = center;

        zoneGreen.localScale = _zoneOriginalScale;
    }

    int MapValorToNivel(float v)
    {
        v = Mathf.Clamp01(v);
        if (v < 0.33f)      return 0; 
        else if (v < 0.66f) return 1; 
        else                return 2; 
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
