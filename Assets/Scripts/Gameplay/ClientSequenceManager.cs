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

    int _currentIndex = -1;
    ClientMover _currentClient;

    void Start()
    {
        SpawnNextClient();
    }

    void SpawnNextClient()
    {
        _currentIndex++;

        if (_currentIndex >= clients.Length)
        {
            Debug.Log("No hay más clientes en la secuencia (MVP completo).");
            dialogueController?.Hide();
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
            dialogueController.PlayDialogue(mover.profile);
        }
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

        dialogueController?.Hide();   
        _currentClient.AllowLeave();
    }
}
