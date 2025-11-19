using UnityEngine;

public class ParticlesMaxScript : MonoBehaviour
{
    [Header("Límite de partículas emitidas")]
    public int maxParticle = 100;

    [Header("Referencia al ParticleSystem")]
    public ParticleSystem ps;

    void Awake()
    {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        var main = ps.main;
        main.maxParticles = maxParticle;
    }
}