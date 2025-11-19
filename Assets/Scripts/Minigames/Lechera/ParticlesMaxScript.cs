using UnityEngine;

public class ParticlesMaxScript : MonoBehaviour
{
    public int maxParticle;

    public ParticleSystem ps;

    void Awake() {
        var main = ps.main;
        main.maxParticles = maxParticle;
    }

}
