using UnityEngine;
using UnityEngine.InputSystem;

public class Guiro : MonoBehaviour
{
    public GameObject rueda;
    public bool girar = false;

    void Update()
    {
        if (girar && rueda != null)
        {
            rueda.transform.Rotate(0, 0, 1);
        }
    }

    public void OnGiro(InputAction.CallbackContext context)
    {
        if (!IceMiniGameController.MachineInputEnabled)
        {
            girar = false;
            return;
        }

        if (context.action.IsPressed())
        {
            girar = true;
        }
        else
        {
            girar = false;
        }
    }

    public void ForceStop()
    {
        girar = false;
    }
}
