using UnityEngine;

public class CholadoGameState : MonoBehaviour
{
    public static CholadoGameState Instance { get; private set; }

    public enum SatisfactionLevel
    {
        Bad,
        Ok,
        Perfect
    }

    [Header("Cliente actual")]
    public ClientProfile currentClient;
    public int currentClientIndex = -1;

    [Header("Cholado ideal del cliente actual (0=Bajo,1=Medio,2=Alto)")]
    public int idealFrio   => currentClient != null ? currentClient.idealFrio   : 1;
    public int idealDulzor => currentClient != null ? currentClient.idealDulzor : 1;
    public int idealFruta  => currentClient != null ? currentClient.idealFruta  : 1;

    [Header("Resultado minijuegos (0=Bajo,1=Medio,2=Alto)")]
    public int resultFrio   = 1;
    public int resultDulzor = 1;
    public int resultFruta  = 1;

    [Header("Selección del jugador (0=Bajo,1=Medio,2=Alto)")]
    public int selectedFrio   = 1;
    public int selectedDulzor = 1;
    public int selectedFruta  = 1;

    [Header("Progreso de preparación del cholado")]
    public bool hasFrio   = false;   
    public bool hasDulzor = false;   
    public bool hasFruta  = false;   

    [Header("Roots visuales principales")]
    public GameObject cristoReyRoot;

    public bool IsCholadoReady()
    {
        return hasFrio && hasDulzor && hasFruta;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetCurrentClient(ClientProfile profile, int index)
    {
        currentClient = profile;
        currentClientIndex = index;

        resultFrio   = 1;
        resultDulzor = 1;
        resultFruta  = 1;

        selectedFrio   = 1;
        selectedDulzor = 1;
        selectedFruta  = 1;

        hasFrio   = false;
        hasDulzor = false;
        hasFruta  = false;

        Debug.Log($"[STATE] Cliente actual: {profile.clientName} " +
                  $"Ideal => Frio={idealFrio}, Dulzor={idealDulzor}, Fruta={idealFruta}");
    }

    int EvaluateDimension(int ideal, int selected, int result)
    {
        if (selected == ideal)
        {
            if (result == ideal)
                return 2; 

            if (Mathf.Abs(result - ideal) == 1)
                return 1; 

            return 0; 
        }
        else
        {
            if (result == ideal)
                return 1;   
            return 0;
        }
    }

    public SatisfactionLevel EvaluateCurrentCholado()
    {
        int scoreFrio   = EvaluateDimension(idealFrio,   selectedFrio,   resultFrio);
        int scoreDulzor = EvaluateDimension(idealDulzor, selectedDulzor, resultDulzor);
        int scoreFruta  = EvaluateDimension(idealFruta,  selectedFruta,  resultFruta);

        int totalScore = scoreFrio + scoreDulzor + scoreFruta;

        Debug.Log($"[STATE] Scores → Frio={scoreFrio}, Dulzor={scoreDulzor}, Fruta={scoreFruta}, Total={totalScore}");

        if (totalScore >= 5)       return SatisfactionLevel.Perfect;
        else if (totalScore >= 3)  return SatisfactionLevel.Ok;
        else                       return SatisfactionLevel.Bad;
    }

    public string GetReactionLine(out SatisfactionLevel level)
    {
        level = EvaluateCurrentCholado();

        if (currentClient == null)
            return "…";

        string[] pool = null;

        switch (level)
        {
            case SatisfactionLevel.Perfect:
                pool = currentClient.reactionPerfectLines;
                break;
            case SatisfactionLevel.Ok:
                pool = currentClient.reactionOkLines;
                break;
            case SatisfactionLevel.Bad:
                pool = currentClient.reactionBadLines;
                break;
        }

        if (pool == null || pool.Length == 0)
            return "Gracias…";

        int idx = Random.Range(0, pool.Length);
        return pool[idx];
    }
}
