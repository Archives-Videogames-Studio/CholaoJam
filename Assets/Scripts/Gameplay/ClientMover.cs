using UnityEngine;
using System;

public class ClientMover : MonoBehaviour
{
    [Header("Datos")]
    public ClientProfile profile;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Waypoints (0=Entrada,1=Puesto,2=Salida)")]
    public Transform[] waypoints;

    [Header("Movimiento")]
    public float speed = 2f;

    [Header("Bob de caminar")]
    public bool enableBob = true;
    public float bobAmplitude = 0.05f;
    public float bobFrequency = 6f;

    public event Action<ClientMover> OnClientReachedCounter;
    public event Action<ClientMover> OnClientFinished;

    private enum ClientState { GoingToCounter, AtCounter, Leaving }
    private ClientState _state;
    private Transform _currentTarget;

    private Vector3 _logicalPosition;
    private float _bobTimer = 0f;

    // guardamos la cara neutra
    Sprite _defaultSprite;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            _defaultSprite = spriteRenderer.sprite;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length < 3)
        {
            Debug.LogError("ClientMover necesita 3 waypoints (P0,P1,P2)", this);
            enabled = false;
            return;
        }

        if (profile != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = profile.portrait;
            _defaultSprite = spriteRenderer.sprite;
        }

        if (profile != null)
            speed = profile.moveSpeed;

        _logicalPosition = waypoints[0].position;
        transform.position = _logicalPosition;

        _state = ClientState.GoingToCounter;
        _currentTarget = waypoints[1];
    }

    void Update()
    {
        switch (_state)
        {
            case ClientState.GoingToCounter:
            case ClientState.Leaving:
                MoveTowardsTarget();
                break;

            case ClientState.AtCounter:
                ApplyBob(false);
                break;
        }
    }

    void MoveTowardsTarget()
    {
        if (_currentTarget == null) return;

        _logicalPosition = Vector3.MoveTowards(
            _logicalPosition,
            _currentTarget.position,
            speed * Time.deltaTime
        );

        bool reached = Vector3.Distance(_logicalPosition, _currentTarget.position) < 0.001f;

        ApplyBob(!reached);

        if (reached)
        {
            _logicalPosition = _currentTarget.position;
            transform.position = _logicalPosition;
            OnReachedTarget();
        }
    }

    void ApplyBob(bool isMoving)
    {
        if (!enableBob)
        {
            transform.position = _logicalPosition;
            return;
        }

        if (isMoving)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
        }
        else
        {
            _bobTimer = 0f;
        }

        float offsetY = Mathf.Sin(_bobTimer) * bobAmplitude;
        transform.position = _logicalPosition + new Vector3(0f, offsetY, 0f);
    }

    void OnReachedTarget()
    {
        if (_state == ClientState.GoingToCounter)
        {
            _state = ClientState.AtCounter;
            _currentTarget = null;
            OnClientReachedCounter?.Invoke(this);
        }
        else if (_state == ClientState.Leaving)
        {
            OnClientFinished?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void AllowLeave()
    {
        if (waypoints == null || waypoints.Length < 3) return;

        _state = ClientState.Leaving;
        _currentTarget = waypoints[2];
    }

    // --------- NUEVO: sprites de reacción ---------

    public void SetFaceChimba()
    {
        if (profile != null && profile.reactionChimba != null && spriteRenderer != null)
            spriteRenderer.sprite = profile.reactionChimba;
    }

    public void SetFaceMelo()
    {
        if (profile != null && profile.reactionMelo != null && spriteRenderer != null)
            spriteRenderer.sprite = profile.reactionMelo;
    }

    public void SetFacePaila()
    {
        if (profile != null && profile.reactionPaila != null && spriteRenderer != null)
            spriteRenderer.sprite = profile.reactionPaila;
    }

    public void ResetFace()
    {
        if (spriteRenderer != null && _defaultSprite != null)
            spriteRenderer.sprite = _defaultSprite;
    }
}
