using UnityEngine;
using UnityEngine.InputSystem;   
using TMPro;                    

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
    public GameObject selectionPanel;   

    [Header("UI Tiempo")]
    public TextMeshProUGUI timerLabel;  

    [Header("Tiempo")]
    public float totalTime = 10f;       
    [SerializeField] 
    private float remainingTime;        

    [Header("Incremento por golpe (dificultad)")]
    public float tapAmountLow    = 0.08f;
    public float tapAmountMedium = 0.05f;
    public float tapAmountHigh   = 0.03f;

    [Header("Resultado")]
    [Range(0f, 1f)]
    public float valorAccion;          
    public int   ACIDEZ = 1;           
    public NivelSeleccion currentLevel;
    public float tapAmountCurrent;
    public bool canPlay;

    bool _expectingLeft = true;        
    bool _finished = false;

    void Start()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (pressureBar != null)
            pressureBar.SetFill(0f);

        canPlay       = false;
        _finished     = false;
        _expectingLeft = true;

        ACIDEZ        = 1;
        remainingTime = 0f;

        UpdateTimerUI();
    }

    void Update()
    {
        if (!canPlay || _finished || pressureBar == null)
        {
            UpdateTimerUI();
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerUI();
            FinishMiniGame(completedInTime: false);
            return;
        }

        if (_expectingLeft)
        {
            if (kb.aKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = false; 
            }
        }
        else
        {
            if (kb.dKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = true; 
            }
        }

        if (pressureBar.fill >= 0.999f)
        {
            FinishMiniGame(completedInTime: true);
            return;
        }

        UpdateTimerUI();
    }


    void UpdateTimerUI()
    {
        if (timerLabel == null) return;

        if (!canPlay || _finished)
        {
            timerLabel.text = "--";
            return;
        }

        float t = Mathf.Max(0f, remainingTime);
        timerLabel.text = t.ToString("0.0") + " s";
    }

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

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (pressureBar != null)
            pressureBar.SetFill(0f);

        remainingTime  = totalTime;
        canPlay        = true;
        _finished      = false;
        _expectingLeft = true;

        UpdateTimerUI();

        Debug.Log($"[LIMON] Dificultad {level}, tap={tapAmountCurrent}, tiempo={totalTime}s");
    }


    void FinishMiniGame(bool completedInTime)
    {
        if (_finished) return;

        _finished = true;
        canPlay   = false;

        valorAccion = pressureBar ? pressureBar.fill : 0f;

        ACIDEZ = ComputeAcidez(currentLevel, completedInTime);

        UpdateTimerUI();

        Debug.Log($"[LIMON] FIN → completado={completedInTime}, llenado={valorAccion:F2}, ACIDEZ={ACIDEZ}");
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
                return 1; 
        }
    }
}
