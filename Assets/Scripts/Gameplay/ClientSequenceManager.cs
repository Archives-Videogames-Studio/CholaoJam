using UnityEngine;
using UnityEngine.InputSystem;

public class ClientSequenceManager : MonoBehaviour
{
    [Header("Clientes en orden (MVP)")]
    public ClientProfile[] clients;     

    [Header("Prefabs y puntos de ruta")]
    public ClientMover clientPrefab;    
    public Transform[] waypoints;       

    int _currentIndex = -1;
    ClientMover _currentClient;

    void Start()
    {
        SpawnNextClient();
    }

    void Update()
    {
        // TEMPORAL: simulamos que el cholado se entregó con la tecla Espacio
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            _currentClient != null)
        {
            Debug.Log("SPACE presionado -> AllowLeave del cliente actual");
            _currentClient.AllowLeave();
        }

    }

    void SpawnNextClient()
    {
        _currentIndex++;

        if (_currentIndex >= clients.Length)
        {
            Debug.Log("No hay más clientes en la secuencia (MVP completado).");
            return;
        }

        ClientProfile profile = clients[_currentIndex];

        _currentClient = Instantiate(clientPrefab);
        _currentClient.waypoints = waypoints;
        _currentClient.profile = profile;

        if (profile != null)
        {
            _currentClient.speed = profile.moveSpeed;
        }

        _currentClient.OnClientFinished += HandleClientFinished;
        _currentClient.OnClientReachedCounter += HandleClientReachedCounter;
    }

    void HandleClientReachedCounter(ClientMover mover)
    {
        Debug.Log($"Cliente llegó al puesto: {mover.profile.clientName}");
    }

    void HandleClientFinished(ClientMover mover)
    {
        mover.OnClientFinished -= HandleClientFinished;
        mover.OnClientReachedCounter -= HandleClientReachedCounter;

        _currentClient = null;
        SpawnNextClient(); 
    }
}
