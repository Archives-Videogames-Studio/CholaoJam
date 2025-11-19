using System.Collections.Generic;
using UnityEngine;

public class ParticleCount : MonoBehaviour
{
    [Header("Referencia al emisor")]
    public ParticlesMaxScript ps;   // arrastras aquí el objeto de la lechera

    private ParticleSystem m_ps;
    private int maxParticle;

    [SerializeField]
    private int particleInside;     // cuántas han entrado al vaso

    [SerializeField, Range(0f, 100f)]
    private float m_porcentaje;     // 0–100 %

    // Lista temporal para leer partículas que están "Inside"
    private List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();

    [Header("Frecuencia de log (debug)")]
    public float logInterval = 0.5f;
    private float _logTimer = 0f;

    void Start()
    {
        if (ps == null)
        {
            Debug.LogError("[ParticleCount] Falta asignar ParticlesMaxScript (lechera).");
            enabled = false;
            return;
        }

        maxParticle = ps.maxParticle;
        m_ps = ps.ps;

        if (m_ps == null)
        {
            Debug.LogError("[ParticleCount] Falta la referencia al ParticleSystem en ParticlesMaxScript.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (maxParticle <= 0) return;

        // Cast a float para no perder precisión
        m_porcentaje = (float)particleInside / maxParticle * 100f;

        // Solo para debug visual en consola cada cierto tiempo
        _logTimer += Time.deltaTime;
        if (_logTimer >= logInterval)
        {
            _logTimer = 0f;
            Debug.Log($"[ParticleCount] Recogido: {particleInside}/{maxParticle} ({m_porcentaje:F1}%)");
        }

        // Aquí más adelante puedes usar m_porcentaje para escalar el sprite del vaso,
        // llenar una barra, etc.
    }

    // Se llama cuando hay eventos de Trigger en el ParticleSystem (modulo Trigger)
    void OnParticleTrigger()
    {
        // Leer cuántas están "Inside" del volumen del vaso
        int entered = m_ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, inside);

        if (entered > 0)
        {
            particleInside += entered;
            // Si quisieras "eliminar" las partículas que caen en el vaso:
            // inside.Clear();  // y luego m_ps.SetTriggerParticles(Inside, inside);
        }
    }

    public float GetPercentage() => m_porcentaje;  // por si quieres leerlo desde otro script
}