using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Máquinas (MIRE)")]
    [SerializeField] private ChangeMachineButton machinesManager;

    [Header("Escena a la que volvemos cuando no hay más clientes")]
    public string mapSceneName = "Mapa";

    int _currentIndex = -1;
    ClientMover _currentClient;

    // Animador de humor del cliente actual (para MachinesManager)
    ClientMoodAnimator _currentClientAnimator;
    public ClientMoodAnimator CurrentMoodAnimator => _currentClientAnimator;

    bool _waitingReaction = false;
    bool _sequenceStarted = false;   // <-- NUEVO

    void Start()
    {
        // Solo aseguramos UI, pero NO empezamos la secuencia aquí
        if (buttonsRoot != null) buttonsRoot.SetActive(false);
    }

    /// <summary>
    /// Llamar una sola vez para arrancar la secuencia de clientes.
    /// </summary>
    public void BeginSequence()
    {
        if (_sequenceStarted) return;

        _sequenceStarted = true;
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

        // =========================
        //   YA NO HAY MÁS CLIENTES
        // =========================
        if (_currentIndex >= clients.Length)
        {
            Debug.Log("No hay más clientes en la secuencia (MVP completo).");
            dialogueController?.Hide();
            if (buttonsRoot != null) buttonsRoot.SetActive(false);

            // Limpiamos el vaso por si quedó algo
            var state = CholadoGameState.Instance;
            if (state != null && state.choladoVisual != null)
            {
                state.choladoVisual.ClearAll();
            }

            // Volvemos a la escena del mapa
            if (!string.IsNullOrEmpty(mapSceneName))
            {
                SceneManager.LoadScene(mapSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[ClientSequenceManager] mapSceneName está vacío, no puedo cargar el mapa.");
            }

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

        // 1) Sprite estático desde el ScriptableObject (portrait)
        SpriteRenderer sr = _currentClient.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && profile.portrait != null)
        {
            sr.sprite = profile.portrait;
        }

        // 2) Aplicar override de animación (pero no encender nada todavía)
        Animator animator = _currentClient.GetComponent<Animator>();
        if (animator != null && profile.animatorOverride != null)
        {
            animator.runtimeAnimatorController = profile.animatorOverride;
        }

        // 3) Cachear ClientMoodAnimator y APAGAR el animator
        _currentClientAnimator = _currentClient.GetComponent<ClientMoodAnimator>();
        if (_currentClientAnimator != null)
        {
            _currentClientAnimator.SetNeutral();
            _currentClientAnimator.DisableAnimation();
        }
        else
        {
            Debug.LogWarning("[ClientSequence] Cliente sin ClientMoodAnimator.");
        }

        // 4) Nuevo cliente => MIRE apagado
        if (machinesManager != null)
        {
            machinesManager.SetMireActive(false);
        }

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

        _currentClientAnimator?.SetNeutral();
    }

    void HandleClientFinished(ClientMover mover)
    {
        mover.OnClientReachedCounter -= HandleClientReachedCounter;
        mover.OnClientFinished       -= HandleClientFinished;

        _currentClient = null;
        _currentClientAnimator = null;

        // Siguiente cliente o volver al mapa
        SpawnNextClient();
    }

    // ===== OIGA =====
    public void OnPressOiga()
    {
        dialogueController?.ReplayLast();
    }

    // ===== MIRE! =====
    public void OnPressMire()
    {
        if (_currentClient == null) return;

        if (machinesManager != null)
        {
            machinesManager.SetMireActive(true);
        }
    }

    // ===== VEA =====
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

        // 1) Evaluar satisfacción
        CholadoGameState.SatisfactionLevel level;
        string reactionLine = state.GetReactionLine(out level);

        Debug.Log($"[VEA] Satisfacción={level}, reacción='{reactionLine}'");

        // 2) Apagar animaciones de MIRE
        if (machinesManager != null)
        {
            machinesManager.SetMireActive(false);
        }
        else if (_currentClientAnimator != null)
        {
            _currentClientAnimator.DisableAnimation();
        }

        // 3) Sprite estático de reacción
        var profile = state.currentClient;
        SpriteRenderer sr = _currentClient.GetComponentInChildren<SpriteRenderer>();

        if (sr != null && profile != null)
        {
            switch (level)
            {
                case CholadoGameState.SatisfactionLevel.Perfect:
                    if (profile.reactionChimba != null)
                        sr.sprite = profile.reactionChimba;
                    break;

                case CholadoGameState.SatisfactionLevel.Ok:
                    if (profile.reactionMelo != null)
                        sr.sprite = profile.reactionMelo;
                    break;

                case CholadoGameState.SatisfactionLevel.Bad:
                    if (profile.reactionPaila != null)
                        sr.sprite = profile.reactionPaila;
                    break;
            }
        }

        // 4) Diálogo de reacción
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

        // 1) El cliente se lleva el cholado
        var state = CholadoGameState.Instance;
        if (state != null && state.choladoVisual != null)
        {
            state.choladoVisual.ClearAll();
        }

        // 2) Dejamos que el cliente se vaya
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
