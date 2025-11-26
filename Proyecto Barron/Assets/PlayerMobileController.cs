using UnityEngine;
using UnityEngine.UI;

public class PlayerMobileController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Joystick References - DRAG THE GAMEOBJECTS")]
    public GameObject joystickBackgroundObj;
    public GameObject joystickHandleObj;
    public float joystickRange = 50f;

    [Header("Mobile Controls")]
    public bool useMobileControls = true;

    // Private variables
    private RectTransform joystickBackground;
    private RectTransform joystickHandle;
    private Vector2 inputVector;
    private Vector2 joystickStartPos;
    private bool joystickActive = false;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Convertir GameObjects a RectTransforms
        if (joystickBackgroundObj != null)
            joystickBackground = joystickBackgroundObj.GetComponent<RectTransform>();
        if (joystickHandleObj != null)
            joystickHandle = joystickHandleObj.GetComponent<RectTransform>();

        // Auto-detect platform
#if UNITY_ANDROID || UNITY_IOS
            useMobileControls = true;
#else
        useMobileControls = false;
#endif

        // Setup joystick
        if (joystickBackground != null)
        {
            joystickStartPos = joystickBackground.anchoredPosition;
            joystickBackground.gameObject.SetActive(useMobileControls);
        }

        Debug.Log("🎮 Mobile Controller Started - " + (useMobileControls ? "MOBILE" : "PC"));
        Debug.Log("📱 Joystick Background: " + (joystickBackground != null ? "ASSIGNED" : "MISSING"));
        Debug.Log("🎯 Joystick Handle: " + (joystickHandle != null ? "ASSIGNED" : "MISSING"));
    }

    void Update()
    {
        if (useMobileControls)
        {
            ProcessMobileInput();
        }
        else
        {
            ProcessPCInput();
        }

        UpdateAnimation();
    }

    void ProcessMobileInput()
    {
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartJoystick(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    UpdateJoystick(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndJoystick();
                    break;
            }
        }

        // Mouse input for testing
        if (Input.GetMouseButtonDown(0))
        {
            StartJoystick(Input.mousePosition);
        }
        if (Input.GetMouseButton(0) && joystickActive)
        {
            UpdateJoystick(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndJoystick();
        }
    }

    void ProcessPCInput()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        inputVector = inputVector.normalized;
    }

    void StartJoystick(Vector2 screenPos)
    {
        if (joystickBackground == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.parent as RectTransform,
            screenPos, null, out localPoint))
        {
            float distance = Vector2.Distance(localPoint, joystickStartPos);
            if (distance <= 150f)
            {
                joystickActive = true;
                UpdateJoystick(screenPos);
            }
        }
    }

    void UpdateJoystick(Vector2 screenPos)
    {
        if (!joystickActive || joystickBackground == null || joystickHandle == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.parent as RectTransform,
            screenPos, null, out localPoint))
        {
            Vector2 direction = localPoint - joystickStartPos;

            if (direction.magnitude > joystickRange)
            {
                direction = direction.normalized * joystickRange;
            }

            inputVector = direction / joystickRange;
            joystickHandle.anchoredPosition = direction;

            // Debug
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"🎮 Input: {inputVector}");
            }
        }
    }

    void EndJoystick()
    {
        joystickActive = false;
        inputVector = Vector2.zero;
        if (joystickHandle != null)
            joystickHandle.anchoredPosition = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + inputVector * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Horizontal", inputVector.x);
            animator.SetFloat("Vertical", inputVector.y);
            animator.SetFloat("Speed", inputVector.magnitude);
        }

        if (spriteRenderer != null && Mathf.Abs(inputVector.x) > 0.1f)
        {
            spriteRenderer.flipX = inputVector.x < 0;
        }
    }

    // Para testing desde el inspector
    [ContextMenu("Test Joystick Connection")]
    void TestJoystickConnection()
    {
        Debug.Log("🧪 Testing Joystick Connection...");
        Debug.Log("Background: " + (joystickBackground != null ? joystickBackground.name : "NULL"));
        Debug.Log("Handle: " + (joystickHandle != null ? joystickHandle.name : "NULL"));
        Debug.Log("Input Vector: " + inputVector);
    }
}