using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ClientMoodAnimator : MonoBehaviour
{
    public const string PARAM_MOOD = "Mood";

    Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("[MOOD] No encontré Animator en el cliente.");
        }
    }

    // ----- Encender / apagar animator -----

    public void EnableAnimation()
    {
        if (_anim == null) return;

        _anim.enabled = true;
        Debug.Log("[MOOD] Animator habilitado, enabled = " + _anim.enabled);
    }

    public void DisableAnimation()
    {
        if (_anim == null) return;

        _anim.enabled = false;
        Debug.Log("[MOOD] Animator deshabilitado, enabled = " + _anim.enabled);
    }

    // ----- Estados de humor -----

    public void SetNeutral()
    {
        if (_anim == null) return;
        _anim.SetInteger(PARAM_MOOD, 0);
        Debug.Log("[MOOD] SetNeutral -> Mood = 0");
    }

    public void SetStars()
    {
        if (_anim == null) return;
        _anim.SetInteger(PARAM_MOOD, 1);
        Debug.Log("[MOOD] SetStars -> Mood = 1");
    }
}
