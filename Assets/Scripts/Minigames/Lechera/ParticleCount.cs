using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCount : MonoBehaviour
{

    public ParticlesMaxScript ps;
    private ParticleSystem m_ps;
    public int maxParticle;
    [SerializeField]
    private int particleInside;
    private float m_porcentaje;

    private List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

    public void Start()
    {
        maxParticle = ps.maxParticle;
        m_ps = ps.ps;
    }


    void Update()
    {
        m_porcentaje = particleInside / maxParticle * 100;
        StartCoroutine(porcentaje());
        
    }

    IEnumerator porcentaje()
    {
        yield return new WaitForSeconds(5);
        GetPercentage();
    }


    void OnParticleTrigger()
    {
        int entered = m_ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, enter);
        particleInside += entered;
    }

    public void GetPercentage()
    {
        if (maxParticle == 0)
            Debug.Log("[ParticlesCount] las particulas emitidas son iguales a 0");

        Debug.Log($"[Particle Count] las particulas recogidas son igual a {m_porcentaje}");

    }

}