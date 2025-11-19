using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";

    [Header("Player Settings")]
    public int playerNumber = 1;
    public bool useKeyboardInput = true;

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

    // Cache de parámetros del Animator
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

    void CacheAnimatorParameters()
    {
        hasAnimator = animator != null;

        if (!hasAnimator)
        {
            Debug.LogWarning($"⚠️ No se encontró Animator en: {gameObject.name}");
            return;
        }

        isMovingHash = Animator.StringToHash("IsMoving");
        isGroundedHash = Animator.StringToHash("IsGrounded");
        isJumpingHash = Animator.StringToHash("IsJumping");
        isFallingHash = Animator.StringToHash("IsFalling");
        speedHash = Animator.StringToHash("speed");
        yVelocityHash = Animator.StringToHash("yVelocity");
        takeDamageHash = Animator.StringToHash("TakeDamage");
        hurtHash = Animator.StringToHash("hurt");
        dieHash = Animator.StringToHash("Die");
        deadHash = Animator.StringToHash("dead");
        respawnHash = Animator.StringToHash("Respawn");
        victoryHash = Animator.StringToHash("Victory");
    }

    void Start()
    {
        originalScale = transform.localScale;
        scaleInitialized = true;

        if (groundLayerMask.value == 1)
        {
            groundLayerMask = LayerMask.GetMask("Ground", "Default", "Platform");
        }

        if (canPassThroughPlayers)
        {
            SetupPlayerCollisions();
        }

        StartCoroutine(InitializeWithDelay());

        Debug.Log("✅ PlayerController inicializado en Start: " + gameObject.name);
        Debug.Log($"   - Player Number: {playerNumber}");
        Debug.Log($"   - Use Keyboard Input: {useKeyboardInput}");
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

        bool wasGrounded = isGrounded;

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
        float moveHorizontal = 0f;

        if (useKeyboardInput)
        {
            moveHorizontal = GetKeyboardHorizontalInput();
        }
        else
        {
            moveHorizontal = Input.GetAxis(horizontalAxis);
        }

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

    void HandleJump()
    {
        bool jumpPressed = false;

        if (useKeyboardInput)
        {
            jumpPressed = GetKeyboardJumpInput();
        }
        else
        {
            jumpPressed = Input.GetButtonDown(jumpButton);
        }

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

    private float GetKeyboardHorizontalInput()
    {
        switch (playerNumber)
        {
            case 1:
                // Jugador 1: Flechas izquierda/derecha
                if (Input.GetKey(KeyCode.RightArrow)) return 1f;
                if (Input.GetKey(KeyCode.LeftArrow)) return -1f;
                break;
            case 2:
                // Jugador 2: A/D
                if (Input.GetKey(KeyCode.D)) return 1f;
                if (Input.GetKey(KeyCode.A)) return -1f;
                break;
        }
        return 0f;
    }

    private bool GetKeyboardJumpInput()
    {
        switch (playerNumber)
        {
            case 1:
                // Jugador 1: Flecha arriba
                return Input.GetKeyDown(KeyCode.UpArrow);
            case 2:
                // Jugador 2: W
                return Input.GetKeyDown(KeyCode.W);
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

    private IEnumerator EnableJumpAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        jumpCooldown = false;
    }

    // Sistema de gravedad cero (sin cambios)
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

        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        bool isFalling = rb.velocity.y < -0.1f;
        bool isJumping = rb.velocity.y > 0.1f;

        SafeSetBool(isMovingHash, isMoving);
        SafeSetBool(isGroundedHash, isGrounded);
        SafeSetBool(isJumpingHash, isJumping);
        SafeSetBool(isFallingHash, isFalling);
        SafeSetFloat(speedHash, Mathf.Abs(rb.velocity.x));
        SafeSetFloat(yVelocityHash, rb.velocity.y);
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

    void OnGUI()
    {
        if (!isInitialized)
        {
            GUI.Label(new Rect(10, 10, 300, 40), "⏳ INICIALIZANDO...");
            return;
        }

        string playerType = "Player1";
        if (gameObject.tag == "Player2") playerType = "Player2";
        else if (gameObject.tag == "Bot") playerType = "Bot";

        float yPosition = 10f;
        if (playerType == "Player2") yPosition = 200f;
        else if (playerType == "Bot") yPosition = 400f;

        string gravedadStatus = isZeroGravityActive ? " 🚀 GRAVEDAD CERO" : "";

        GUI.Label(new Rect(10, yPosition, 400, 220),
            $"{playerType} - {gameObject.name}{gravedadStatus}\n" +
            $"Player Number: {playerNumber}\n" +
            $"Use Keyboard Input: {useKeyboardInput}\n" +
            $"EnSuelo: {isGrounded}\n" +
            $"VelY: {rb.velocity.y:F2}\n" +
            $"Cooldown: {jumpCooldown}\n" +
            $"Estado: {jumpState}\n" +
            $"Gravedad: {rb.gravityScale}\n" +
            $"Salud: {health}\n" +
            $"Controles: {controlEnabled}\n" +
            $"ZeroG Activo: {isZeroGravityActive}");
    }
}