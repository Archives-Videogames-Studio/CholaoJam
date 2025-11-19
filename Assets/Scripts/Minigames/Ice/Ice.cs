using UnityEngine;
using UnityEngine.InputSystem;

public class Ice : MonoBehaviour
{
    public ParticlesMaxScriptUpdate ps;

    private bool restando = false;
    private bool suamndo = false;

    public void OnRueda(InputAction.CallbackContext context)
    {
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
            if(ps.maxParticle < 100)
            {
                ps.maxParticle += 1;
            }
            else
            {
                suamndo = false;
            }
        }
    }
}
