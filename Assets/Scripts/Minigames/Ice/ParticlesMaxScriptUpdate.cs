using UnityEngine;

public class ParticlesMaxScriptUpdate : MonoBehaviour
{
    [Header("Límite de partículas emitidas")]
    public int maxParticle = 100;

    [Header("Referencia al ParticleSystem")]
    public ParticleSystem ps;

    void Update() 
    {
        var main = ps.main;
        main.maxParticles = maxParticle;
    }
}