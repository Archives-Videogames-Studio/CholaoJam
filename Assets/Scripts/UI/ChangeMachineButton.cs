using UnityEngine;

public class ChangeMachineButton : MonoBehaviour
{
    [Header("Máquinas en orden")]
    public GameObject[] machines;   // 0 = Hielo, 1 = Sirope, 2 = Fruta

    [Header("Índice actual")]
    public int index = 0;

    [Header("Refs")]
    [SerializeField] private ClientSequenceManager clientSequence;

    bool _mireActive = false;

    void Start()
    {
        UpdateActiveMachine();
    }

    public void UpMachine()
    {
        if (machines == null || machines.Length == 0) return;

        index++;
        if (index >= machines.Length)
            index = 0;

        UpdateActiveMachine();
        RefreshMoodIfNeeded();
    }

    void UpdateActiveMachine()
    {
        if (machines == null) return;

        for (int i = 0; i < machines.Length; i++)
        {
            if (machines[i] != null)
                machines[i].SetActive(i == index);
        }
    }

    // >>> ESTE es el que llama el ClientSequenceManager cuando pulsas MIRE! <<<
    public void SetMireActive(bool active)
    {
        _mireActive = active;

        var mood = GetCurrentMoodAnimator();
        if (mood == null) return;

        if (active)
        {
            // Encendemos el animator y aplicamos estrellas/neutro
            mood.EnableAnimation();
            RefreshMoodIfNeeded();
        }
        else
        {
            // Lo apagamos y lo dejamos neutro (nuevo cliente, por ejemplo)
            mood.SetNeutral();
            mood.DisableAnimation();
        }
    }

    // ---------- lógica de humor ----------

    void RefreshMoodIfNeeded()
    {
        if (!_mireActive) return;

        var mood  = GetCurrentMoodAnimator();
        var state = CholadoGameState.Instance;

        if (mood == null || state == null) return;

        int ideal = 1;

        // 0 = Hielo/Frío, 1 = Sirope/Dulzor, 2 = Fruta
        switch (index)
        {
            case 0: ideal = state.idealFrio;   break;
            case 1: ideal = state.idealDulzor; break;
            case 2: ideal = state.idealFruta;  break;
        }

        bool stars = (ideal == 2);

        Debug.Log($"[MIRE] idx={index}, ideal={ideal}, stars={stars}");

        if (stars) mood.SetStars();
        else       mood.SetNeutral();
    }

    ClientMoodAnimator GetCurrentMoodAnimator()
    {
        if (clientSequence == null)
        {
#if UNITY_2023_1_OR_NEWER
            clientSequence = Object.FindFirstObjectByType<ClientSequenceManager>();
#else
            clientSequence = FindObjectOfType<ClientSequenceManager>();
#endif
        }

        return clientSequence != null ? clientSequence.CurrentMoodAnimator : null;
    }

    public int GetCurrentIndex() => index;
}
