using UnityEngine;
using UnityEngine.InputSystem; 

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
    public GameObject selectionPanel;   

    [Header("Línea ideal de corte")]
    public Transform idealLine;

    [Header("Resultado (debug)")]
    [Range(0f, 1f)]
    public float valorAccion;   
    public int nivelFinal;      
    public int FRUTA;          
    public NivelSeleccion seleccionInicial; 
    bool _hasCut = false;
    bool _canCut = false;     

    void Start()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (barMover != null)
            barMover.enabled = false;

        if (idealLine != null)
            idealLine.gameObject.SetActive(false);

        _hasCut = false;
        _canCut = false;
    }


    void Update()
    {
        if (!_canCut || _hasCut)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _hasCut = true;
            ProcesarCorte();
        }
    }


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

        if (idealLine != null && leftLimit != null && rightLimit != null)
        {
            float t; 

            switch (nivel)
            {
                case NivelSeleccion.Bajo:
                    t = 0.25f;   
                    break;
                case NivelSeleccion.Medio:
                    t = 0.5f;    
                    break;
                case NivelSeleccion.Alto:
                    t = 0.75f;   
                    break;
                default:
                    t = 0.5f;
                    break;
            }

           
            Vector3 pos = Vector3.Lerp(leftLimit.position, rightLimit.position, t);

            if (barraCorte != null)
            {
                pos.y = barraCorte.position.y;
                pos.z = barraCorte.position.z;
            }

            idealLine.position = pos;
            idealLine.gameObject.SetActive(true);
        }

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (barMover != null)
            barMover.enabled = true;

        _canCut = true;

        Debug.Log($"[FRUTA] Selección inicial: {seleccionInicial}");
    }


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

        valorAccion = Mathf.InverseLerp(minX, maxX, cutX);

        nivelFinal = MapValorToNivel(valorAccion);

        FRUTA = nivelFinal;

        if (barMover != null)
            barMover.enabled = false;

        Debug.Log($"[FRUTA] Selección: {seleccionInicial} | Corte en {valorAccion:F2} → Nivel {nivelFinal} (FRUTA={FRUTA})");
    }

    int MapValorToNivel(float v)
    {
        if (v < 0.33f) return 0;   
        if (v < 0.66f) return 1;   
        return 2;                  
    }
}
