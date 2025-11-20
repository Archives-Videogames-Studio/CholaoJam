using UnityEngine;
using UnityEngine.InputSystem; 

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

    [Header("Incremento por golpe según dificultad")]
    public float tapAmountLow    = 0.15f;   
    public float tapAmountMedium = 0.10f;   
    public float tapAmountHigh   = 0.07f;   

    [Header("Debug")]
    public float tapAmountCurrent;
    public NivelSeleccion currentLevel;
    public bool canPlay;

    bool _expectingLeft = true; 

    void Start()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (pressureBar != null)
            pressureBar.SetFill(0f);

        canPlay = false;
        _expectingLeft = true;
    }

    void Update()
    {
        if (!canPlay || pressureBar == null)
            return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (_expectingLeft)
        {
            if (kb.aKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = false; 
                Debug.Log("[LIMON] A presionada");
            }
        }
        else
        {
            if (kb.dKey.wasPressedThisFrame)
            {
                pressureBar.AddFill(tapAmountCurrent);
                _expectingLeft = true; 
                Debug.Log("[LIMON] D presionada");
            }
        }
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

        canPlay = true;
        _expectingLeft = true;

        Debug.Log($"[LIMON] Dificultad seleccionada: {level}, tap={tapAmountCurrent}");
    }
}
