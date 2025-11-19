using UnityEngine;
using UnityEngine.InputSystem;

public class Ice : MonoBehaviour
{
    public ParticlesMaxScriptUpdate ps;

    private bool restando = false;

    public void OnRueda(InputAction.CallbackContext context)
    {
        if (context.performed)   
        {
            ps.maxParticle = 100;
            restando = false;    
        }
        else if (context.canceled)  
        {
            restando = true;     
        }
    }

    void Update()
    {
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
    }
}
