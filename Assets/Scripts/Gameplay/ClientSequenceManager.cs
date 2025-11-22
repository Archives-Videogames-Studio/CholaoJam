using UnityEngine;

public class ClientSequenceManager : MonoBehaviour
{
    [Header("Clientes en orden (MVP)")]
    public ClientProfile[] clients;

    [Header("Prefabs y waypoints")]
    public ClientMover clientPrefab;
    public Transform[] waypoints;

    [Header("Padre para los clientes")]
    public Transform clientParent;

    [Header("UI")]
    public DialogueController dialogueController;
    public GameObject buttonsRoot;
    public GameObject oigaButton;
    public GameObject veaButton;

    int _currentIndex = -1;
    ClientMover _currentClient;

    bool _waitingReaction = false;

    void Start()
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(false);
        SpawnNextClient();
    }

    void Update()
    {
        if (buttonsRoot != null && buttonsRoot.activeSelf)
        {
            UpdateVeaButton();
        }
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

        if (CholadoGameState.Instance != null)
        {
            CholadoGameState.Instance.SetCurrentClient(profile, _currentIndex);
        }

        Vector3 spawnPos = waypoints != null && waypoints.Length > 0
            ? waypoints[0].position
            : Vector3.zero;

        _currentClient = Instantiate(
            clientPrefab,
            spawnPos,
            Quaternion.identity,
            clientParent != null ? clientParent : null
        );

        _currentClient.waypoints = waypoints;
        _currentClient.profile   = profile;

        _currentClient.OnClientReachedCounter += HandleClientReachedCounter;
        _currentClient.OnClientFinished       += HandleClientFinished;

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

        if (oigaButton != null) oigaButton.SetActive(true);
        UpdateVeaButton();
    }

    void HandleClientFinished(ClientMover mover)
    {
        mover.OnClientReachedCounter -= HandleClientReachedCounter;
        mover.OnClientFinished       -= HandleClientFinished;

        _currentClient = null;
        SpawnNextClient();
    }

    public void OnPressOiga()
    {
        dialogueController?.ReplayLast();
    }

    public void OnPressVea()
    {
        if (_currentClient == null || dialogueController == null)
            return;

        var state = CholadoGameState.Instance;
        if (state == null || !state.IsCholadoReady())
        {
            Debug.Log("[VEA] Todavía no has preparado todo el cholado.");
            return;
        }

        if (_waitingReaction) return; 

        if (buttonsRoot != null) buttonsRoot.SetActive(false);

        if (state == null || state.currentClient == null)
        {
            dialogueController.Hide();
            _currentClient.AllowLeave();
            return;
        }

        CholadoGameState.SatisfactionLevel level;
        string reactionLine = state.GetReactionLine(out level);

        Debug.Log($"[VEA] Satisfacción={level}, reacción='{reactionLine}'");

        if (_currentClient != null)
        {
            switch (level)
            {
                case CholadoGameState.SatisfactionLevel.Perfect:
                    _currentClient.SetFaceChimba();
                    break;

                case CholadoGameState.SatisfactionLevel.Ok:
                    _currentClient.SetFaceMelo();
                    break;

                case CholadoGameState.SatisfactionLevel.Bad:
                    _currentClient.SetFacePaila();
                    break;
            }
        }

        if (string.IsNullOrEmpty(reactionLine))
        {
            dialogueController.Hide();
            _currentClient.AllowLeave();
            return;
        }

        _waitingReaction = true;
        dialogueController.Hide(); 
        dialogueController.OnDialogueFinished += HandleReactionFinished;
        dialogueController.PlayReaction(state.currentClient, reactionLine);
    }

    void HandleReactionFinished()
    {
        dialogueController.OnDialogueFinished -= HandleReactionFinished;
        _waitingReaction = false;

        _currentClient.AllowLeave();
    }

    void UpdateVeaButton()
    {
        if (veaButton == null) return;

        var state = CholadoGameState.Instance;
        bool ready = (state != null && state.IsCholadoReady());

        veaButton.SetActive(ready);
    }
}
