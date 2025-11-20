using UnityEngine;
using UnityEngine.InputSystem; 

public class CupMouseMover : MonoBehaviour
{
    [Header("Límites horizontales en mundo")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Control")]
    public bool canMove = false;   

    float _fixedY;
    Camera _cam;

    bool _isDragging = false;
    Vector3 _dragOffset;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Start()
    {
        _fixedY = transform.position.y;
    }

    void Update()
    {
        if (!canMove) return;            
        if (_cam == null || Mouse.current == null)
            return;

        var mouse = Mouse.current;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            StartDrag(mouse);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            DragUpdate(mouse);
        }
    }

    void StartDrag(Mouse mouse)
    {
        Vector2 mouseScreen = mouse.position.ReadValue();
        float zDist = -_cam.transform.position.z;

        Vector3 mouseWorld = _cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, zDist)
        );

        _isDragging = true;
        _dragOffset = transform.position - mouseWorld;
    }

    void DragUpdate(Mouse mouse)
    {
        Vector2 mouseScreen = mouse.position.ReadValue();
        float zDist = -_cam.transform.position.z;

        Vector3 mouseWorld = _cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, zDist)
        );

        Vector3 targetPos = mouseWorld + _dragOffset;
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);

        transform.position = new Vector3(
            clampedX,
            _fixedY,
            transform.position.z
        );
    }
    public void EnableMovement(bool value)
    {
        canMove = value;
        if (!value)
            _isDragging = false;
    }
}
