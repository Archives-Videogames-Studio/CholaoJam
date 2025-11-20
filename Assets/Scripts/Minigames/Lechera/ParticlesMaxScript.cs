using System.Collections;
using UnityEngine;

public class ParticlesMaxScript : MonoBehaviour
{
    [Header("Referencia al ParticleSystem")]
    public ParticleSystem ps;

    [Header("Límite de seguridad (max partículas vivas)")]
    public int safetyMaxParticles = 2000;

    [Header("Segmentos de chorro")]
    [Tooltip("Mínimo de partículas seguidas antes de hacer una pausa.")]
    public int minSegmentParticles = 20;

    [Tooltip("Máximo de partículas seguidas antes de hacer una pausa.")]
    public int maxSegmentParticles = 80;

    [Header("Pausa entre segmentos (segundos)")]
    [Tooltip("Tiempo mínimo de pausa entre segmentos.")]
    public float minPauseDuration = 0.2f;

    [Tooltip("Tiempo máximo de pausa entre segmentos.")]
    public float maxPauseDuration = 0.7f;

    private void Awake()
    {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        if (ps == null)
        {
            Debug.LogError("[ParticlesMaxScript] No se encontró ParticleSystem.");
            enabled = false;
            return;
        }

        var main = ps.main;
        main.maxParticles = safetyMaxParticles;

        var emission = ps.emission;
        emission.enabled = false;
    }

    public void PlayAsStream(int totalParticles, float streamDuration)
    {
        if (ps == null || totalParticles <= 0 || streamDuration <= 0f)
        {
            Debug.LogWarning("[ParticlesMaxScript] Parámetros inválidos en PlayAsStream.");
            return;
        }

        if (minSegmentParticles < 1) minSegmentParticles = 1;
        if (maxSegmentParticles < minSegmentParticles)
            maxSegmentParticles = minSegmentParticles;

        if (minPauseDuration < 0f) minPauseDuration = 0f;
        if (maxPauseDuration < minPauseDuration)
            maxPauseDuration = minPauseDuration;

        StartCoroutine(PlayStreamRoutine(totalParticles, streamDuration));
    }

    private IEnumerator PlayStreamRoutine(int totalParticles, float streamDuration)
    {
        var emission = ps.emission;
        emission.enabled = false;

        ps.Clear();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;

        ps.Play();

        float interval = streamDuration / totalParticles;
        int emitted = 0;

        while (emitted < totalParticles)
        {
            int remaining = totalParticles - emitted;

            int segmentCount = Random.Range(minSegmentParticles, maxSegmentParticles + 1);
            segmentCount = Mathf.Min(segmentCount, remaining);

            for (int i = 0; i < segmentCount; i++)
            {
                ps.Emit(1);
                emitted++;

                if (emitted < totalParticles)
                    yield return new WaitForSeconds(interval);
            }

            if (emitted < totalParticles)
            {
                float pause = Random.Range(minPauseDuration, maxPauseDuration);
                if (pause > 0f)
                    yield return new WaitForSeconds(pause);
            }
        }
    }

    public bool IsAlive()
    {
        return ps != null && ps.IsAlive();
    }
}
