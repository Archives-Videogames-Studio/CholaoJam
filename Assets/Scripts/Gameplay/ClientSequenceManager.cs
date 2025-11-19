using UnityEngine;

public class ClientSequenceManager : MonoBehaviour
{
    [Header("Clientes en orden (MVP)")]
    public ClientProfile[] clients;

    [Header("Prefabs y waypoints")]
    public ClientMover clientPrefab;
    public Transform[] waypoints;

    [Header("UI")]
    public DialogueController dialogueController;   
    public GameObject buttonsRoot;                 

    int _currentIndex = -1;
    ClientMover _currentClient;

    void Start()
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(false);

        SpawnNextClient();
    }

    void SpawnNextClient()
    {
        _currentIndex++;

        if (_currentIndex >= clients.Length)
        {
            Debug.Log("No hay más clientes en la secuencia (MVP completo).");
            dialogueController?.Hide();
            if (buttonsRoot != null) buttonsRoot.SetActive(false);
            return;
        }

        var profile = clients[_currentIndex];

        _currentClient = Instantiate(clientPrefab);
        _currentClient.waypoints = waypoints;
        _currentClient.profile = profile;

        _currentClient.OnClientReachedCounter += HandleClientReachedCounter;
        _currentClient.OnClientFinished += HandleClientFinished;

        Debug.Log($"Spawn cliente: {profile.clientName}");
    }

    void HandleClientReachedCounter(ClientMover mover)
    {
        Debug.Log($"Cliente llegó al puesto: {mover.profile.clientName}");

        if (dialogueController != null)
        {
            if (buttonsRoot != null) buttonsRoot.SetActive(false);

            dialogueController.OnDialogueFinished += HandleDialogueFinished;
            dialogueController.PlayDialogue(mover.profile);
        }
    }

    void HandleDialogueFinished()
    {
        dialogueController.OnDialogueFinished -= HandleDialogueFinished;

        if (buttonsRoot != null) buttonsRoot.SetActive(true);
    }

    void HandleClientFinished(ClientMover mover)
    {
        mover.OnClientReachedCounter -= HandleClientReachedCounter;
        mover.OnClientFinished -= HandleClientFinished;

        _currentClient = null;
        SpawnNextClient();
    }

    public void OnPressOiga()
    {
        dialogueController?.ReplayLast();
    }

    public void OnPressVea()
    {
        if (_currentClient == null) return;

        if (buttonsRoot != null) buttonsRoot.SetActive(false);

        dialogueController?.Hide();     
        _currentClient.AllowLeave();
    }
}
