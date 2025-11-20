using System.Collections.Generic;
using UnityEngine;

public class ParticleCount : MonoBehaviour
{
    [Header("Referencia al emisor (lechera)")]
    public ParticlesMaxScript source;

    private ParticleSystem ps;
    private readonly List<ParticleSystem.Particle> inside = new();

    [SerializeField] private int   particleInside;
    [SerializeField] private int   roundTotalParticles;
    [SerializeField] private float porcentaje;

    [Header("Frecuencia de log (debug)")]
    public float logInterval = 0.5f;
    private float _logTimer;

    private void Start()
    {
        if (source == null || source.ps == null)
        {
            Debug.LogError("[ParticleCount] Falta asignar ParticlesMaxScript o su ParticleSystem.");
            enabled = false;
            return;
        }

        ps = source.ps;
    }

    private void Update()
    {
        if (roundTotalParticles <= 0) return;

        porcentaje = (float)particleInside / roundTotalParticles * 100f;

        _logTimer += Time.deltaTime;
        if (_logTimer >= logInterval)
        {
            _logTimer = 0f;
        }
    }

    private void OnParticleTrigger()
    {
        if (ps == null) return;

        int entered = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, inside);
        if (entered > 0)
        {
            particleInside += entered;
        }
    }

    public void ResetCount()
    {
        particleInside = 0;
    }

    public int GetCollectedCount() => particleInside;

    public void SetRoundTotalParticles(int total)
    {
        roundTotalParticles = total;
    }
}
