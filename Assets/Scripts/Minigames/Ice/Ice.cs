using UnityEngine;
using UnityEngine.InputSystem;

public class Ice : MonoBehaviour
{
    public ParticlesMaxScriptUpdate ps;

    private bool restando = false;
    private bool suamndo = false;

    public void OnRueda(InputAction.CallbackContext context)
    {
        if (!IceMiniGameController.MachineInputEnabled)
            return;

        if (context.performed)
        {
            suamndo = true;
            restando = false;
        }
        else if (context.canceled)
        {
            restando = true;
            suamndo = false;
        }
    }

    void Update()
    {
        if (ps == null) return;

        if (restando)
        {
            if (ps.maxParticle > 0)
            {
                ps.maxParticle -= 1;
            }
            else
            {
                restando = false;
            }
        }

        if (suamndo)
        {
            if (ps.maxParticle < 100)
            {
                ps.maxParticle += 1;
            }
            else
            {
                suamndo = false;
            }
        }
    }
    public void ForceStopEmission()
    {
        suamndo = false;
        restando = false;

        if (ps != null)
            ps.maxParticle = 0;
    }
}
