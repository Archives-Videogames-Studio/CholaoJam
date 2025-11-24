using System.Collections;
using UnityEngine;

public class MinigameInstructionsController : MonoBehaviour
{
    [Header("Panel de instrucciones")]
    [Tooltip("El objeto del portapapeles (panel completo).")]
    public Transform panel;          // Portapapeles

    [Tooltip("Posición visible (en pantalla).")]
    public Transform shownPos;       // Empty en la posición final

    [Tooltip("Posición oculta (fuera de pantalla, abajo).")]
    public Transform hiddenPos;      // Empty abajo, fuera de cámara

    [Header("Animación")]
    [Tooltip("Duración de la animación de subir/bajar.")]
    public float animDuration = 0.35f;

    [Header("UI del minijuego")]
    [Tooltip("Panel de selección de nivel (Bajo/Medio/Alto).")]
    public GameObject selectionPanel;

    bool _isVisible = false;
    bool _isAnimating = false;
    Coroutine _animRoutine;

    void Start()
    {
        if (panel == null || shownPos == null || hiddenPos == null)
        {
            Debug.LogWarning("[MinigameInstructions] Falta asignar panel/shownPos/hiddenPos.");
            return;
        }

        // Aseguramos que el panel exista y esté activo
        panel.gameObject.SetActive(true);

        // Ocultamos el panel abajo al inicio
        panel.position = hiddenPos.position;
        _isVisible = false;

        // El selectionPanel NO debe estar activo al inicio
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Lanzamos la animación de entrada desde abajo
        ShowPanel();
    }

    // Llamable si quisieras reabrir instrucciones en cualquier momento
    public void ShowPanel()
    {
        if (_isAnimating || _isVisible) return;

        if (_animRoutine != null) StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimatePanel(show: true));
    }

    // Llamar esto desde el botón "Cerrar"
    public void HidePanel()
    {
        if (_isAnimating || !_isVisible) return;

        if (_animRoutine != null) StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimatePanel(show: false));
    }

    IEnumerator AnimatePanel(bool show)
    {
        _isAnimating = true;

        Vector3 start = show ? hiddenPos.position : shownPos.position;
        Vector3 end   = show ? shownPos.position   : hiddenPos.position;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            panel.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        panel.position = end;
        _isVisible = show;
        _isAnimating = false;

        // Cuando se ocultan las instrucciones → habilitamos el selectionPanel
        if (!show && selectionPanel != null)
            selectionPanel.SetActive(true);
    }

    // Método comodín para el botón (por si quieres llamarlo directo)
    public void OnPressClose()
    {
        HidePanel();
    }
}
