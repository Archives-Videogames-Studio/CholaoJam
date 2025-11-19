using UnityEngine;
using UnityEngine.InputSystem;

public class Guiro : MonoBehaviour
{
    
    public GameObject rueda;
    public bool girar = false;

    void Update()
    {
        if(girar)
        {
            rueda.transform.Rotate(0,0,1);
        }
    }

    public void OnGiro(InputAction.CallbackContext context)
    {
        if(context.action.IsPressed())
        {
            girar = true;
        }
        else
        {
            girar = false;
        }
    }

}
