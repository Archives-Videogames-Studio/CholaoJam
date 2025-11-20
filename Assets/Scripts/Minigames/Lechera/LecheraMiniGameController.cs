using System.Collections;
using UnityEngine;

/// Minijuego de la lechera:
/// - El jugador elige Bajo / Medio / Alto.
/// - Emitimos N partículas según la selección.
/// - Se mueve botella y vaso.
/// - Al final mostramos el % atrapado y si fue suficiente para ese nivel.
public class LecheraMiniGameController : MonoBehaviour
{
    [Header("Referencias")]
    public ParticlesMaxScript particlesMax;
    public ParticleCount particleCount;
    public LecheraMover lecheraMover;
    public CupMouseMover cupMover;

    [Header("Cantidades por nivel (partículas a emitir)")]
    public int lowAmount    = 200;
    public int mediumAmount = 350;
    public int highAmount   = 500;

    [Header("Duración base del chorro (segundos)")]
    public float streamDuration = 2f;

    [Header("Retardo extra tras el chorro (segundos)")]
    public float extraEndDelay = 0.5f;

    [Header("UI de selección")]
    public GameObject selectionPanel;

    [Header("Umbral 'bien' (porcentaje mínimo atrapado)")]
    [Range(0f, 100f)]
    public float goodThreshold = 30f;   // 30% o más = "bien" para ese nivel

    // Info de la última ronda (para debug / otros sistemas)
    [HideInInspector] public int   lastRoundTotal  = 0;
    [HideInInspector] public int   lastCollected   = 0;
    [HideInInspector] public float lastPercentage  = 0f;
    [HideInInspector] public bool  roundRunning    = false;
    [HideInInspector] public bool  roundFinished   = false;

    private Coroutine _currentRound;

    private void Start()
    {
        // Mostrar panel al inicio
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        // Desactivar movimiento al inicio
        if (lecheraMover != null)
            lecheraMover.EnableMovement(false);

        if (cupMover != null)
            cupMover.EnableMovement(false);
    }

    #region Botones

    public void OnSelectLow()
    {
        if (!roundRunning)
            StartRound(lowAmount, "BAJO");
    }

    public void OnSelectMedium()
    {
        if (!roundRunning)
            StartRound(mediumAmount, "MEDIO");
    }

    public void OnSelectHigh()
    {
        if (!roundRunning)
            StartRound(highAmount, "ALTO");
    }

    #endregion

    private void StartRound(int totalToEmit, string labelNivel)
    {
        if (particlesMax == null || particleCount == null)
        {
            Debug.LogError("[LecheraMiniGame] Faltan referencias (ParticlesMaxScript o ParticleCount).");
            return;
        }

        if (_currentRound != null)
            StopCoroutine(_currentRound);

        _currentRound = StartCoroutine(RoundRoutine(totalToEmit, labelNivel));
    }

    private IEnumerator RoundRoutine(int totalToEmit, string labelNivel)
    {
        roundRunning  = true;
        roundFinished = false;

        lastRoundTotal = totalToEmit;

        // Ocultar panel durante la ronda
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Reset contadores
        particleCount.ResetCount();

        // Habilitar movimiento
        if (lecheraMover != null)
            lecheraMover.EnableMovement(true);

        if (cupMover != null)
            cupMover.EnableMovement(true);

        // Emitir exactamente totalToEmit partículas
        particlesMax.PlayAsStream(totalToEmit, streamDuration);

        // -------- ESPERAR HASTA QUE NO QUEDE NINGUNA PARTÍCULA VIVA --------
        ParticleSystem ps = particlesMax.ps;
        float elapsed = 0f;
        const float safetyTimeout = 30f; // por si algo se buguea

        while (ps != null && ps.IsAlive(true) && elapsed < safetyTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        // -------------------------------------------------------------------

        // Pequeño margen para que terminen de caer y entrar al vaso
        yield return new WaitForSeconds(extraEndDelay);

        // Detener movimiento
        if (lecheraMover != null)
            lecheraMover.EnableMovement(false);

        if (cupMover != null)
            cupMover.EnableMovement(false);

        // Guardar resultados
        lastCollected  = particleCount.GetCollectedCount();
        lastPercentage = (totalToEmit > 0)
            ? (float)lastCollected / totalToEmit * 100f
            : 0f;

        bool esBueno = lastPercentage >= goodThreshold;

        Debug.Log(
            $"[LecheraMiniGame] Selección: {labelNivel} (emitidas ≈{totalToEmit}). " +
            $"Atrapaste: {lastCollected} ({lastPercentage:F1}%). " +
            $"Resultado: {(esBueno ? "OK" : "POCO")} para {labelNivel}."
        );

        roundRunning  = false;
        roundFinished = true;

        // Permitir rejugar
        if (selectionPanel != null)
            selectionPanel.SetActive(true);
    }
}
