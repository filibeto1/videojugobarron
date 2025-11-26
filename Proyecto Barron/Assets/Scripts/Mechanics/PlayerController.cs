using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";

    [Header("Player Settings")]
    public int playerNumber = 1;
    public bool useKeyboardInput = true;

    [Header("Mobile Input")]
    public bool useMobileInput = false;
    private MobileInputManager mobileInputManager;

    [Header("Joystick Connection")]
    public VirtualJoystick movementJoystick;
    public VirtualJoystick jumpJoystick;
    public string jumpButtonName = "Jump";

    [Header("Player Components")]
    public Rigidbody2D rb;
    public Collider2D collider2d;
    public Animator animator;
    public AudioSource audioSource;

    [Header("Player Stats")]
    public float maxSpeed = 7f;
    public int health = 3;
    public bool controlEnabled = true;

    [Header("Audio Clips")]
    public AudioClip jumpAudio;
    public AudioClip damageAudio;
    public AudioClip victoryAudio;

    [Header("Physics")]
    public Vector3 velocity;
    public float jumpForce = 22f;
    public float moveSpeed = 5f;
    public float gravityScale = 1f;
    public float maxFallSpeed = -35f;

    [Header("Player State")]
    public JumpState jumpState = JumpState.Grounded;
    public bool isGrounded = true;
    public bool isAlive = true;
    private bool jumpCooldown = false;
    private bool isInitialized = false;
    private float initializationDelay = 0.3f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayerMask = 1;
    private float lastGroundedTime = 0f;
    private float groundRememberTime = 0.1f;

    [Header("Scale Protection")]
    private Vector3 originalScale;
    private bool scaleInitialized = false;

    [Header("Collision Settings")]
    public bool canPassThroughPlayers = true;

    [Header("Zero Gravity System")]
    private bool isZeroGravityActive = false;
    private Coroutine zeroGravityCoroutine;

    private bool hasAnimator = false;
    private int isMovingHash;
    private int isGroundedHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int speedHash;
    private int yVelocityHash;
    private int takeDamageHash;
    private int hurtHash;
    private int dieHash;
    private int deadHash;
    private int respawnHash;
    private int victoryHash;

    // Variables para debug de input
    private float lastHorizontalInput = 0f;
    private string lastInputSource = "Ninguno";

    public Bounds Bounds
    {
        get
        {
            return collider2d != null ? collider2d.bounds : new Bounds(transform.position, Vector3.one);
        }
    }

    public enum JumpState
    {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    void Awake()
    {
        PreventDuplicateInstances();
        InitializeComponents();
        CacheAnimatorParameters();

        Debug.Log($"✅ PlayerController inicializado en Awake: {gameObject.name}");
    }

    private void PreventDuplicateInstances()
    {
        PlayerController[] existingPlayers = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in existingPlayers)
        {
            if (player != this && IsDuplicatePlayer(player))
            {
                Debug.LogWarning($"🚨 Destruyendo PlayerController DUPLICADO: {gameObject.name}");
                DestroyImmediate(gameObject);
                return;
            }
        }
    }

    private bool IsDuplicatePlayer(PlayerController otherPlayer)
    {
        if (otherPlayer.gameObject.name == this.gameObject.name &&
            Vector3.Distance(otherPlayer.transform.position, this.transform.position) < 2f)
        {
            return true;
        }

        if (otherPlayer.gameObject.name == this.gameObject.name &&
            otherPlayer.gameObject.scene != this.gameObject.scene)
        {
            return true;
        }

        return false;
    }

    private void InitializeComponents()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (collider2d == null) collider2d = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.gravityScale = gravityScale;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;

            Debug.Log($"✅ Gravedad configurada: {rb.gravityScale}");
            Debug.Log($"✅ Velocidad inicial forzada a cero");
        }

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.8f, 0);
            groundCheck = groundCheckObj.transform;
            Debug.Log($"📍 GroundCheck creado en posición: {groundCheck.localPosition}");
        }

        velocity = Vector3.zero;
    }

    private void CacheAnimatorParameters()
    {
        hasAnimator = animator != null;

        if (!hasAnimator)
        {
            Debug.LogWarning($"⚠️ No se encontró Animator en: {gameObject.name}");
            return;
        }

        isMovingHash = Animator.StringToHash("IsMoving");     // Usa el FLOAT que ya tienes
        isGroundedHash = Animator.StringToHash("grounded");   // ✅ Este SÍ existe (Bool)
        isJumpingHash = Animator.StringToHash("IsJumping");   // Lo mantienes pero será 0
        isFallingHash = Animator.StringToHash("IsFalling");   // Lo mantienes pero será 0  
        speedHash = Animator.StringToHash("IsMoving");        // Usa IsMoving para velocidad también
        yVelocityHash = Animator.StringToHash("velocityY");   // ✅ Este SÍ existe (Float)

        takeDamageHash = Animator.StringToHash("TakeDamage");
        hurtHash = Animator.StringToHash("Hurt");
        dieHash = Animator.StringToHash("Die");
        deadHash = Animator.StringToHash("IsDead");
        respawnHash = Animator.StringToHash("Respawn");
        victoryHash = Animator.StringToHash("Victory");

        Debug.Log("✅ Parámetros del Animator configurados");
    }

    void Start()
    {
        originalScale = transform.localScale;
        scaleInitialized = true;
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }
        if (groundLayerMask.value == 1)
        {
            groundLayerMask = LayerMask.GetMask("Ground", "Default", "Platform");
        }
        if (canPassThroughPlayers)
        {
            SetupPlayerCollisions();
        }
        DetectPlatformCorrectly();
        if (useMobileInput)
        {
            FindAndConnectJoysticks();
        }
        StartCoroutine(InitializeWithDelay());
    }

    void OnEnable()
    {
        if (animator != null)
        {
            Debug.Log("🎬 ========== DIAGNÓSTICO DEL ANIMATOR ==========");
            Debug.Log($"🎬 Animator Controller: {animator.runtimeAnimatorController?.name ?? "NULL"}");
            Debug.Log("🎬 PARÁMETROS ENCONTRADOS:");

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                Debug.Log($"   ✅ {param.name} (Tipo: {param.type})");
            }

            Debug.Log("🎬 ===============================================");
        }
        else
        {
            Debug.LogError("❌ NO HAY ANIMATOR en " + gameObject.name);
        }
    }

    private void DetectPlatformCorrectly()
    {
#if UNITY_EDITOR
        useMobileInput = true;
        useKeyboardInput = true;
        Debug.Log("🎮 Editor: JOYSTICKS Y TECLADO ACTIVADOS para pruebas");
        return;
#endif

        mobileInputManager = FindObjectOfType<MobileInputManager>();

        if (mobileInputManager != null)
        {
            bool isMobile = mobileInputManager.IsMobilePlatform();

            if (isMobile)
            {
                useMobileInput = true;
                useKeyboardInput = false;
                Debug.Log("📱 Plataforma móvil detectada - Controles móviles ACTIVADOS");
            }
            else
            {
                useMobileInput = false;
                useKeyboardInput = true;
                Debug.Log("⌨️ Plataforma de escritorio - Controles de teclado ACTIVADOS");
            }
        }
        else
        {
            bool isMobile = Application.isMobilePlatform ||
                            Application.platform == RuntimePlatform.Android ||
                            Application.platform == RuntimePlatform.IPhonePlayer;

            if (isMobile)
            {
                useMobileInput = true;
                useKeyboardInput = false;
                Debug.Log("📱 Plataforma móvil nativa detectada");
            }
            else
            {
                useMobileInput = false;
                useKeyboardInput = true;
                Debug.Log("⌨️ Plataforma de escritorio nativa detectada");
            }
        }
    }

    private void FindAndConnectJoysticks()
    {
        if (!useMobileInput)
        {
            Debug.Log("⌨️ Modo teclado - No se buscan joysticks");
            return;
        }

        Debug.Log("🔍 Buscando joysticks...");

        VirtualJoystick[] allJoysticks = FindObjectsOfType<VirtualJoystick>(true);
        Debug.Log($"🕹️ Joysticks encontrados: {allJoysticks.Length}");

        foreach (VirtualJoystick joystick in allJoysticks)
        {
            joystick.gameObject.SetActive(true);

            if (joystick.gameObject.name.Contains("Movement"))
            {
                movementJoystick = joystick;
                Debug.Log($"✅ Movement Joystick asignado: {joystick.gameObject.name}");
            }
            else if (joystick.gameObject.name.Contains("Jump"))
            {
                jumpJoystick = joystick;
                Debug.Log($"✅ Jump Joystick asignado: {joystick.gameObject.name}");
            }
            else if (joystick.isMovementJoystick && movementJoystick == null)
            {
                movementJoystick = joystick;
                Debug.Log($"✅ Movement Joystick asignado por tipo: {joystick.gameObject.name}");
            }
            else if (!joystick.isMovementJoystick && jumpJoystick == null)
            {
                jumpJoystick = joystick;
                Debug.Log($"✅ Jump Joystick asignado por tipo: {joystick.gameObject.name}");
            }
        }

        if (movementJoystick == null)
        {
            Debug.LogWarning("⚠️ No se encontró joystick de movimiento");
        }

        if (jumpJoystick == null)
        {
            Debug.LogWarning("⚠️ No se encontró joystick de salto");
        }
    }

    public void SetVirtualJoystick(VirtualJoystick joystick)
    {
        if (joystick == null) return;

        joystick.gameObject.SetActive(true);

        if (joystick.gameObject.name.Contains("Movement") || joystick.isMovementJoystick)
        {
            movementJoystick = joystick;
            Debug.Log($"🎮 Joystick de MOVIMIENTO conectado: {joystick.gameObject.name} -> {gameObject.name}");
        }
        else
        {
            jumpJoystick = joystick;
            Debug.Log($"🎮 Joystick de SALTO conectado: {joystick.gameObject.name} -> {gameObject.name}");
        }

        if (Application.isMobilePlatform || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            useMobileInput = true;
            useKeyboardInput = false;
            Debug.Log($"📱 Input móvil ACTIVADO para: {gameObject.name}");
        }
    }

    public void ForceReconnectJoysticks()
    {
        Debug.Log($"🔄 Forzando reconexión de joysticks para: {gameObject.name}");

        VirtualJoystick[] allJoysticks = FindObjectsOfType<VirtualJoystick>(true);

        if (allJoysticks.Length == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron joysticks para reconectar");
            return;
        }

        foreach (VirtualJoystick joystick in allJoysticks)
        {
            joystick.gameObject.SetActive(true);
            SetVirtualJoystick(joystick);
        }

        Debug.Log($"✅ {allJoysticks.Length} joysticks reconectados a: {gameObject.name}");
    }

    private void SetupPlayerCollisions()
    {
        if (collider2d == null)
        {
            Debug.LogError("❌ No hay Collider2D en " + gameObject.name);
            return;
        }

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        BotController[] allBots = FindObjectsOfType<BotController>();

        foreach (PlayerController otherPlayer in allPlayers)
        {
            if (otherPlayer != this && otherPlayer.collider2d != null)
            {
                Physics2D.IgnoreCollision(collider2d, otherPlayer.collider2d, true);
            }
        }

        foreach (BotController bot in allBots)
        {
            Collider2D botCollider = bot.GetComponent<Collider2D>();
            if (botCollider != null)
            {
                Physics2D.IgnoreCollision(collider2d, botCollider, true);
            }
        }
    }

    private IEnumerator InitializeWithDelay()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        isGrounded = false;
        jumpState = JumpState.InFlight;
        controlEnabled = false;
        jumpCooldown = true;

        yield return new WaitForSeconds(initializationDelay);

        UpdateGroundDetection();
        isInitialized = true;

        if (ShouldEnableControls())
        {
            controlEnabled = true;
        }

        jumpCooldown = false;

        Debug.Log($"✅ {gameObject.name} listo! EnSuelo: {isGrounded}, Controles: {controlEnabled}");
    }

    private bool ShouldEnableControls()
    {
        if (gameObject.tag == "Player" || gameObject.name == "Player")
            return true;

        if (gameObject.tag == "Player2" || gameObject.name == "Player2")
        {
            GameModeSelector selector = FindObjectOfType<GameModeSelector>();
            if (selector != null && selector.IsTwoPlayerMode())
                return true;
        }

        return false;
    }

    void Update()
    {
        if (!isInitialized) return;

        if (rb != null)
        {
            velocity = rb.velocity;
        }

        UpdateGroundDetection();

        if (controlEnabled && isAlive)
        {
            HandlePlayerControl();

            if (!isZeroGravityActive)
            {
                HandleJump();
            }
        }

        ClampVelocity();
        UpdateAnimations();

        if (Time.frameCount % 60 == 0)
        {
            DebugInputStatus();
        }
    }

    private void DebugInputStatus()
    {
        float currentInput = GetCurrentHorizontalInput();
        if (Mathf.Abs(currentInput) > 0.1f || Mathf.Abs(lastHorizontalInput) > 0.1f)
        {
            Debug.Log($"🎮 INPUT DEBUG - Fuente: {lastInputSource}, Valor: {currentInput:F2}, " +
                     $"Móvil: {useMobileInput}, Teclado: {useKeyboardInput}, " +
                     $"JoystickMov: {(movementJoystick != null ? "✅" : "❌")}, " +
                     $"Velocidad: {rb.velocity.x:F2}");
        }
        lastHorizontalInput = currentInput;
    }

    void ClampVelocity()
    {
        if (rb != null && !isZeroGravityActive)
        {
            if (rb.velocity.y < maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
            }

            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            {
                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
            }
        }
    }

    void UpdateGroundDetection()
    {
        if (!isInitialized || groundCheck == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayerMask);
        bool touchingGround = colliders.Length > 0;

        if (touchingGround && rb.velocity.y <= 0.5f)
        {
            lastGroundedTime = Time.time;
            if (!isGrounded)
            {
                isGrounded = true;
                jumpState = JumpState.Grounded;
                jumpCooldown = false;
            }
        }
        else
        {
            bool canRememberGround = (Time.time - lastGroundedTime) <= groundRememberTime;

            if (rb.velocity.y > 0.5f)
            {
                isGrounded = false;
                jumpState = JumpState.Jumping;
            }
            else if (rb.velocity.y < -0.5f)
            {
                isGrounded = false;
                jumpState = JumpState.InFlight;
            }
            else if (!canRememberGround)
            {
                isGrounded = false;
                jumpState = JumpState.InFlight;
            }
        }
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        if (scaleInitialized)
        {
            float scaleX = transform.localScale.x;

            if (Mathf.Abs(Mathf.Abs(scaleX) - Mathf.Abs(originalScale.x)) > 0.1f ||
                Mathf.Abs(transform.localScale.y - originalScale.y) > 0.1f)
            {
                float direction = Mathf.Sign(scaleX);
                transform.localScale = new Vector3(
                    direction * Mathf.Abs(originalScale.x),
                    originalScale.y,
                    originalScale.z
                );
            }
        }

        if (Mathf.Abs(transform.position.z) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;
        }
    }

    void HandlePlayerControl()
    {
        float moveHorizontal = GetCurrentHorizontalInput();

        if (rb != null)
        {
            if (isZeroGravityActive)
            {
                rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
            }
            else
            {
                Vector2 movement = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
                rb.velocity = movement;
            }

            if (moveHorizontal > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
            else if (moveHorizontal < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

    private float GetCurrentHorizontalInput()
    {
        float input = 0f;

        if (useMobileInput && movementJoystick != null)
        {
            input = movementJoystick.Horizontal();
            if (Mathf.Abs(input) > 0.01f)
            {
                lastInputSource = "Joystick";
                return input;
            }
        }

        if (useKeyboardInput)
        {
            input = GetKeyboardHorizontalInput();
            if (Mathf.Abs(input) > 0.1f)
            {
                lastInputSource = "Teclado";
                return input;
            }
        }

        input = GetInputSystemHorizontal();
        if (Mathf.Abs(input) > 0.1f)
        {
            lastInputSource = "Sistema";
            return input;
        }

        lastInputSource = "Ninguno";
        return 0f;
    }

    void HandleJump()
    {
        bool jumpPressed = GetCurrentJumpInput();

        if (jumpPressed)
        {
            bool canRememberGround = (Time.time - lastGroundedTime) <= groundRememberTime;
            bool canJump = (isGrounded || canRememberGround) && !jumpCooldown && isInitialized;

            if (canJump)
            {
                Jump();
            }
        }
    }

    private bool GetCurrentJumpInput()
    {
        if (GetInputSystemJump())
        {
            lastInputSource = "Botón Salto";
            Debug.Log("🦘 SALTO DETECTADO desde Botón!");
            return true;
        }

        if (useMobileInput && jumpJoystick != null)
        {
            bool jumpPressed = jumpJoystick.Vertical() > 0.5f || jumpJoystick.IsJumpPressed();
            if (jumpPressed)
            {
                lastInputSource = "Joystick Salto";
                Debug.Log("🦘 SALTO DETECTADO desde Joystick!");
                return true;
            }
        }

        if (useKeyboardInput)
        {
            bool jumpPressed = GetKeyboardJumpInput();
            if (jumpPressed)
            {
                lastInputSource = "Teclado Salto";
                Debug.Log("🦘 SALTO DETECTADO desde Teclado!");
                return true;
            }
        }

        return false;
    }

    // ✅ NUEVO: Métodos corregidos para Input System
    private float GetKeyboardHorizontalInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return 0f;

        switch (playerNumber)
        {
            case 1:
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) return 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) return -1f;
                break;
            case 2:
                if (keyboard.rightArrowKey.isPressed) return 1f;
                if (keyboard.leftArrowKey.isPressed) return -1f;
                break;
        }
        return 0f;
    }

    private bool GetKeyboardJumpInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        switch (playerNumber)
        {
            case 1:
                return keyboard.wKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;
            case 2:
                return keyboard.upArrowKey.wasPressedThisFrame;
        }
        return false;
    }

    private float GetInputSystemHorizontal()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            return gamepad.leftStick.x.ReadValue();
        }
        return 0f;
    }

    private bool GetInputSystemJump()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }

        // También verificar el teclado como fallback
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    public void Jump()
    {
        if (!isInitialized || rb == null) return;

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        isGrounded = false;
        jumpState = JumpState.Jumping;
        jumpCooldown = true;

        StartCoroutine(EnableJumpAfterDelay());

        if (audioSource != null && jumpAudio != null)
        {
            audioSource.PlayOneShot(jumpAudio);
        }
    }

    public void TryJump()
    {
        bool canRememberGround = (Time.time - lastGroundedTime) <= groundRememberTime;
        bool canJump = (isGrounded || canRememberGround) && !jumpCooldown && isInitialized;

        if (canJump)
        {
            Jump();
        }
        else
        {
            Debug.Log($"❌ No puede saltar - Grounded: {isGrounded}, Cooldown: {jumpCooldown}, Init: {isInitialized}");
        }
    }

    private IEnumerator EnableJumpAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        jumpCooldown = false;
    }

    public void ActivateZeroGravity(float duration, float floatSpeed)
    {
        if (isZeroGravityActive) return;

        if (zeroGravityCoroutine != null)
        {
            StopCoroutine(zeroGravityCoroutine);
        }

        zeroGravityCoroutine = StartCoroutine(ZeroGravityRoutine(duration, floatSpeed));
    }

    private IEnumerator ZeroGravityRoutine(float duration, float floatSpeed)
    {
        isZeroGravityActive = true;
        float originalGravity = rb.gravityScale;

        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, floatSpeed);

        float timer = 0f;
        while (timer < duration)
        {
            if (controlEnabled && isAlive)
            {
                float moveHorizontal = GetKeyboardHorizontalInput();
                float targetVelocityY = Mathf.Lerp(rb.velocity.y, floatSpeed * 0.3f, Time.deltaTime * 2f);
                rb.velocity = new Vector2(moveHorizontal * moveSpeed, targetVelocityY);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        isZeroGravityActive = false;
    }

    public bool IsZeroGravityActive()
    {
        return isZeroGravityActive;
    }

    void UpdateAnimations()
    {
        if (!hasAnimator || !isInitialized) return;

        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        bool isMoving = horizontalSpeed > 0.1f;
        bool isFalling = rb.velocity.y < -0.1f;
        bool isJumping = rb.velocity.y > 0.1f;

        SafeSetFloat(isMovingHash, horizontalSpeed);
        SafeSetBool(isGroundedHash, isGrounded);
        SafeSetFloat(yVelocityHash, rb.velocity.y);

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎬 ANIMACIÓN - Moving: {isMoving} (Speed: {horizontalSpeed:F2}), " +
                     $"Grounded: {isGrounded}, velocityY: {rb.velocity.y:F2}");
        }
    }

    void SafeSetBool(int paramHash, bool value)
    {
        if (hasAnimator && HasAnimatorParameter(animator, paramHash))
        {
            animator.SetBool(paramHash, value);
        }
    }

    void SafeSetFloat(int paramHash, float value)
    {
        if (hasAnimator && HasAnimatorParameter(animator, paramHash))
        {
            animator.SetFloat(paramHash, value);
        }
    }

    void SafeSetTrigger(int paramHash)
    {
        if (hasAnimator && HasAnimatorParameter(animator, paramHash))
        {
            animator.SetTrigger(paramHash);
        }
    }

    private bool HasAnimatorParameter(Animator animator, int parameterHash)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == parameterHash)
                return true;
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isInitialized) return;
        CheckGroundCollision(collision.gameObject, "Enter");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isInitialized) return;
        CheckGroundCollision(collision.gameObject, "Stay");
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!isInitialized) return;

        if (((1 << collision.gameObject.layer) & groundLayerMask) != 0)
        {
            isGrounded = false;
        }
    }

    void CheckGroundCollision(GameObject other, string type)
    {
        if (((1 << other.layer) & groundLayerMask) != 0)
        {
            if (rb.velocity.y <= 0.5f && rb.velocity.y >= -15f)
            {
                if (!isGrounded && type == "Enter")
                {
                    isGrounded = true;
                    jumpState = JumpState.Grounded;
                    jumpCooldown = false;
                }
                else if (!isGrounded)
                {
                    isGrounded = true;
                    jumpState = JumpState.Grounded;
                }
            }
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlEnabled = enabled;

        if (!enabled && rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    public void ProcessCheckpoint(int checkpointNumber)
    {
        Debug.Log($"🎯 Checkpoint {checkpointNumber} alcanzado");
    }

    public void Bounce(float bounceForce)
    {
        if (rb != null && isInitialized)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
        }
    }

    public void Teleport(Vector3 newPosition)
    {
        transform.position = newPosition;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (!isAlive || !isInitialized) return;

        health -= damageAmount;

        if (audioSource != null && damageAudio != null)
        {
            audioSource.PlayOneShot(damageAudio);
        }

        SafeSetTrigger(takeDamageHash);
        SafeSetTrigger(hurtHash);

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isAlive = false;
        controlEnabled = false;

        SafeSetTrigger(dieHash);
        SafeSetBool(deadHash, true);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void Respawn(Vector3 spawnPosition)
    {
        isAlive = true;
        controlEnabled = true;
        health = 3;
        jumpCooldown = false;
        isInitialized = true;

        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        SafeSetTrigger(respawnHash);
        SafeSetBool(deadHash, false);
    }

    public void PlayVictory()
    {
        if (audioSource != null && victoryAudio != null)
        {
            audioSource.PlayOneShot(victoryAudio);
        }

        SafeSetTrigger(victoryHash);
    }

    public void AddHealth(int healthAmount)
    {
        health += healthAmount;
    }

    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
        moveSpeed = newMaxSpeed;
    }

    public void BoostSpeed(float boostMultiplier, float duration)
    {
        if (isInitialized)
        {
            StartCoroutine(SpeedBoostCoroutine(boostMultiplier, duration));
        }
    }

    private IEnumerator SpeedBoostCoroutine(float boostMultiplier, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= boostMultiplier;

        yield return new WaitForSeconds(duration);

        moveSpeed = originalSpeed;
    }

    public void SuperJump(float jumpMultiplier)
    {
        if (rb != null && isGrounded && isInitialized)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
            isGrounded = false;
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public int GetHealth()
    {
        return health;
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    private void OnDestroy()
    {
        if (movementJoystick != null)
        {
            // Limpiar referencias si es necesario
        }

        if (jumpJoystick != null)
        {
            // Limpiar referencias si es necesario
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 1f);
        }
    }
}