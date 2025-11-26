using UnityEngine;
using UnityEngine.UI;

public class SimpleJoystickController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Joystick Parts - DRAG THESE")]
    public GameObject joystickArea;        // Arrastra "JoystickArea"
    public GameObject joystickBackground;  // Arrastra "JoystickBackground"  
    public GameObject joystickHandle;      // Arrastra "JoystickHandle"

    [Header("Settings")]
    public float joystickRange = 50f;

    // Private
    private Vector2 inputDirection;
    private Vector2 joystickStartPos;
    private bool isDragging = false;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (joystickBackground != null)
        {
            RectTransform bgRect = joystickBackground.GetComponent<RectTransform>();
            joystickStartPos = bgRect.anchoredPosition;
        }

        Debug.Log("✅ Joystick Controller Ready");
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }

    void HandleInput()
    {
        // Mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                StartDrag(touchPos);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                UpdateDrag(touchPos);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndDrag();
            }
        }

        // Mouse input for testing
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    void StartDrag(Vector2 screenPos)
    {
        if (joystickBackground == null) return;

        // Check if touch is within joystick area
        RectTransform bgRect = joystickBackground.GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bgRect, screenPos, null, out localPoint))
        {
            if (localPoint.magnitude <= 100f) // Activation radius
            {
                isDragging = true;
                UpdateDrag(screenPos);
            }
        }
    }

    void UpdateDrag(Vector2 screenPos)
    {
        if (!isDragging || joystickBackground == null || joystickHandle == null) return;

        RectTransform bgRect = joystickBackground.GetComponent<RectTransform>();
        RectTransform handleRect = joystickHandle.GetComponent<RectTransform>();

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bgRect, screenPos, null, out localPoint))
        {
            // Calculate input direction
            inputDirection = localPoint / joystickRange;

            // Limit to circle
            if (inputDirection.magnitude > 1f)
                inputDirection = inputDirection.normalized;

            // Move handle visually
            handleRect.anchoredPosition = inputDirection * joystickRange;

            Debug.Log($"🎮 Direction: {inputDirection}");
        }
    }

    void EndDrag()
    {
        isDragging = false;
        inputDirection = Vector2.zero;

        if (joystickHandle != null)
        {
            RectTransform handleRect = joystickHandle.GetComponent<RectTransform>();
            handleRect.anchoredPosition = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (rb != null && inputDirection.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + inputDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Horizontal", inputDirection.x);
            animator.SetFloat("Vertical", inputDirection.y);
            animator.SetFloat("Speed", inputDirection.magnitude);
        }
    }
}