using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
{
    [Header("Bot Movement Settings")]
    public float moveSpeed = 0.5f; // ✅ VELOCIDAD REDUCIDA (era 1f)
    public float jumpForce = 15f;
    public float jumpCooldown = 3f;
    public float decisionRate = 0.3f;

    [Header("Bot Behavior")]
    public float followDistance = 3f;
    public float attackDistance = 2f;

    [Header("AI Navigation to Goal")]
    public bool autoNavigateToGoal = true;
    public Transform goalTarget;
    public float goalReachTime = 120f;
    private bool isNavigatingToGoal = false;
    private Vector3 startPositionForGoal;

    [Header("Question System")]
    public float questionAnswerDelay = 1.5f;

    [Header("Control Settings")]
    public bool isPlayerControlled = false;
    public bool controlsEnabled = true;

    [Header("Multi-Keyboard Settings")]
    public int playerNumber = 2;
    public bool useMultiKeyboardSystem = true;

    [Header("Scale Protection")]
    public Vector3 originalScale;
    private bool scaleInitialized = false;

    [Header("Collision Settings")]
    public bool canPassThroughPlayers = true;
    public bool canPassThroughBots = true;
    public bool canPassThroughWalls = true;

    [Header("Checkpoint Behavior")]
    public bool ignoreCheckpointsInAIMode = true;

    [Header("Floating Settings")]
    public bool isFloating = false;
    public float floatSpeed = 3f;
    public bool ignoreObstaclesWhenFloating = true;
    private bool hasReachedGoal = false;

    [Header("Game Over Settings")]
    public bool showGameOverMessage = true;
    public float messageDisplayTime = 5f;
    public Color messageColor = Color.red;
    public int messageFontSize = 32;

    private Rigidbody2D rb;
    private Collider2D collider2d;
    private Transform playerTarget;
    private bool canJump = true;
    private bool isGrounded = false;
    private float lastDecisionTime = 0f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1;

    private bool isAnsweringChallenge = false;
    private Checkpoint currentCheckpoint;
    private SequenceChallengeManager challengeManager;
    private bool shouldIgnoreCheckpoints = true;
    private float floatingStartTime;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float maxStuckTime = 3f;
    private bool isStuck = false;
    private bool gameOver = false;
    private float gameOverTime = 0f;
    private string gameOverMessage = "";
    private GUIStyle gameOverStyle;
    private float constantSpeed = 4f; // ✅ VELOCIDAD REDUCIDA (era 8f)
    private float wallCollisionTime = 0f;
    private float wallCollisionCooldown = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();

        originalScale = transform.localScale;
        scaleInitialized = true;

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("BotGroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (rb != null)
        {
            rb.gravityScale = 4f;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        constantSpeed = moveSpeed;

        if (groundLayerMask.value == 0)
        {
            Debug.LogWarning("⚠️ groundLayerMask no está configurado! Usando capa por defecto.");
            groundLayerMask = 1 << LayerMask.NameToLayer("Default");
        }

        challengeManager = FindObjectOfType<SequenceChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogWarning("⚠️ SequenceChallengeManager no encontrado en la escena");
        }

        if (!isPlayerControlled)
        {
            shouldIgnoreCheckpoints = ignoreCheckpointsInAIMode;
            Debug.Log($"🤖 Bot configurado para IGNORAR checkpoints: {shouldIgnoreCheckpoints}");
        }

        SetupAllCollisions();

        if (goalTarget == null)
        {
            FindGoalTarget();
        }

        if (goalTarget == null && !isPlayerControlled && autoNavigateToGoal)
        {
            CreateTemporaryGoal();
        }

        if (!isPlayerControlled && autoNavigateToGoal && goalTarget != null)
        {
            SetupGoalNavigation();
        }

        if (!isPlayerControlled)
        {
            FindPlayerTarget();
        }

        canJump = true;
        isGrounded = false;

        StartCoroutine(InitialGroundCheck());

        Debug.Log($"🤖 Bot inicializado - Control: {(isPlayerControlled ? "JUGADOR" : "IA")}");
        Debug.Log($"📏 Escala original: {originalScale}");
        Debug.Log($"🔍 Configuración de detección de suelo - LayerMask: {groundLayerMask.value}");
        Debug.Log($"🏃 Velocidad constante configurada: {constantSpeed}");
        Debug.Log($"🧱 Atravesar paredes: {canPassThroughWalls}");
    }

    IEnumerator InitialGroundCheck()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateGroundDetection();
    }

    private void SetupAllCollisions()
    {
        if (collider2d == null)
        {
            Debug.LogError("❌ No hay Collider2D en Bot: " + gameObject.name);
            return;
        }

        Debug.Log($"🔧 Configurando colisiones para Bot: {gameObject.name}");

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        BotController[] allBots = FindObjectsOfType<BotController>();

        int playersIgnored = 0;
        int botsIgnored = 0;
        int wallsIgnored = 0;

        foreach (PlayerController player in allPlayers)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(collider2d, playerCollider, true);
                playersIgnored++;
                Debug.Log($"🚫 Bot {gameObject.name} ahora atraviesa a Player: {player.gameObject.name}");
            }
        }

        if (canPassThroughBots)
        {
            foreach (BotController otherBot in allBots)
            {
                if (otherBot != this)
                {
                    Collider2D otherBotCollider = otherBot.GetComponent<Collider2D>();
                    if (otherBotCollider != null)
                    {
                        Physics2D.IgnoreCollision(collider2d, otherBotCollider, true);
                        botsIgnored++;
                        Debug.Log($"🚫 Bot {gameObject.name} ahora atraviesa a Bot: {otherBot.gameObject.name}");
                    }
                }
            }
        }

        if (canPassThroughWalls)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("wall") ||
                    obj.name.ToLower().Contains("pared") ||
                    obj.name.ToLower().Contains("obstacle") ||
                    obj.name.ToLower().Contains("obstaculo") ||
                    obj.name.ToLower().Contains("barrier") ||
                    obj.name.ToLower().Contains("barrera") ||
                    obj.name.ToLower().Contains("block") ||
                    obj.name.ToLower().Contains("bloque"))
                {
                    Collider2D objCollider = obj.GetComponent<Collider2D>();
                    if (objCollider != null && objCollider != collider2d)
                    {
                        Physics2D.IgnoreCollision(collider2d, objCollider, true);
                        wallsIgnored++;
                        Debug.Log($"🧱 Bot {gameObject.name} ahora atraviesa: {obj.name}");
                    }
                }
            }

            Debug.Log($"🧱 Paredes/obstáculos ignorados: {wallsIgnored}");
        }

        Debug.Log($"✅ Colisiones configuradas - Players ignorados: {playersIgnored}, Bots ignorados: {botsIgnored}, Paredes ignoradas: {wallsIgnored}");

        SetupLayerCollisions();
    }

    private void SetupLayerCollisions()
    {
        string botLayerName = "Bot";
        int botLayer = LayerMask.NameToLayer(botLayerName);

        if (botLayer == -1)
        {
            Debug.LogWarning($"⚠️ Layer '{botLayerName}' no existe. Usando layer por defecto.");
            botLayer = gameObject.layer;
        }
        else
        {
            gameObject.layer = botLayer;
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        int defaultLayer = LayerMask.NameToLayer("Default");

        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(botLayer, playerLayer, true);
            Debug.Log($"🎯 Colisiones desactivadas entre layers: Bot <> Player");
        }

        if (canPassThroughWalls)
        {
            string[] possibleWallLayers = { "Wall", "Walls", "Obstacle", "Obstacles", "Barrier", "Block" };

            foreach (string layerName in possibleWallLayers)
            {
                int wallLayer = LayerMask.NameToLayer(layerName);
                if (wallLayer != -1)
                {
                    Physics2D.IgnoreLayerCollision(botLayer, wallLayer, true);
                    Debug.Log($"🧱 Colisiones desactivadas entre layers: Bot <> {layerName}");
                }
            }
        }

        Debug.Log($"🏷️ Bot {gameObject.name} asignado a layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    void FixedUpdate()
    {
        UpdateGroundDetection();

        if (isFloating && isNavigatingToGoal)
        {
            CheckIfStuck();
        }

        if (!isPlayerControlled && controlsEnabled && isNavigatingToGoal && goalTarget != null && !isAnsweringChallenge)
        {
            ApplyConstantSpeed();
        }

        if (!isPlayerControlled && controlsEnabled && isNavigatingToGoal && !isAnsweringChallenge)
        {
            ForceMovementThroughWalls();
        }
    }

    void Update()
    {
        DebugBotState();
        DebugGoalNavigation();

        if (isAnsweringChallenge)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (isPlayerControlled && controlsEnabled)
        {
            HandlePlayerControl();
        }
        else if (!isPlayerControlled && controlsEnabled)
        {
            MakeDecision();
        }
    }

    void ForceMovementThroughWalls()
    {
        if (goalTarget == null || rb == null) return;

        if (Mathf.Abs(rb.velocity.x) < 0.5f && Time.time - wallCollisionTime > wallCollisionCooldown)
        {
            Vector3 targetPosition = new Vector3(goalTarget.position.x, goalTarget.position.y, transform.position.z);
            Vector2 direction = (targetPosition - transform.position).normalized;

            rb.AddForce(new Vector2(direction.x * constantSpeed * 100f, 0f));

            wallCollisionTime = Time.time;

            Debug.Log($"🚧 Bot empujando a través de obstáculo - Aplicando fuerza extra");
        }
    }

    void DebugBotState()
    {
        if (Time.frameCount % 120 == 0)
        {
            string floatingStatus = isFloating ? "🎈 FLOTANDO" : "⬇️ NORMAL";
            string groundedStatus = isGrounded ? "🏠 EN SUELO" : "🌪️ EN AIRE";
            string stuckStatus = isStuck ? "🚧 ATASCADO" : "🏃 MOVIÉNDOSE";

            Debug.Log($"🤖 Estado Bot - {floatingStatus}, {groundedStatus}, {stuckStatus}, " +
                     $"VelX: {(rb != null ? rb.velocity.x.ToString("F2") : "N/A")}, " +
                     $"VelY: {(rb != null ? rb.velocity.y.ToString("F2") : "N/A")}, " +
                     $"PosX: {transform.position.x:F2}");
        }
    }

    void DebugGoalNavigation()
    {
        if (Time.frameCount % 120 == 0)
        {
            string floatingStatus = isFloating ? "🎈 FLOTANDO" : "⬇️ NORMAL";

            Debug.Log($"🎯 DEBUG META - {floatingStatus}, GoalTarget: {(goalTarget != null ? goalTarget.name : "NULL")}, " +
                     $"Navigating: {isNavigatingToGoal}, AutoNavigate: {autoNavigateToGoal}, " +
                     $"Velocidad: {constantSpeed}");

            if (goalTarget != null)
            {
                float distance = Vector2.Distance(transform.position, goalTarget.position);
                Debug.Log($"📍 Distancia a meta: {distance:F2}, Posición X: {transform.position.x:F2}");
            }
        }
    }

    void LateUpdate()
    {
        if (scaleInitialized)
        {
            float currentXScale = Mathf.Abs(transform.localScale.x);
            float targetXScale = Mathf.Abs(originalScale.x);

            if (Mathf.Abs(currentXScale - targetXScale) > 0.01f ||
                Mathf.Abs(transform.localScale.y - originalScale.y) > 0.01f)
            {
                float direction = Mathf.Sign(transform.localScale.x);
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

    void UpdateGroundDetection()
    {
        if (groundCheck != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckRadius,
                         isGrounded ? Color.green : Color.red);

            if (!wasGrounded && isGrounded)
            {
                canJump = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void ApplyConstantSpeed()
    {
        if (goalTarget == null || rb == null) return;

        Vector3 targetPosition = new Vector3(goalTarget.position.x, goalTarget.position.y, transform.position.z);
        Vector2 direction = (targetPosition - transform.position).normalized;

        float currentSpeed = constantSpeed;

        rb.velocity = new Vector2(direction.x * currentSpeed, rb.velocity.y);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        if (isGrounded && canJump && Mathf.Abs(rb.velocity.y) < 0.1f && !isFloating)
        {
            if (Random.Range(0, 100) < 2)
            {
                Jump();
            }
        }

        float distanceToGoal = Vector2.Distance(transform.position, targetPosition);
        if (distanceToGoal < 2f)
        {
            OnReachGoal();
        }
    }

    void HandlePlayerControl()
    {
        float moveHorizontal = 0f;
        bool jumpPressed = false;
        bool jumpHeld = false;

        if (useMultiKeyboardSystem && MultiKeyboardInputManager.Instance != null)
        {
            moveHorizontal = MultiKeyboardInputManager.Instance.GetHorizontal(playerNumber);
            jumpPressed = MultiKeyboardInputManager.Instance.GetJump(playerNumber);
            jumpHeld = MultiKeyboardInputManager.Instance.GetJumpHeld(playerNumber);
        }
        else
        {
            moveHorizontal = Input.GetAxis("Horizontal_P2");
            jumpPressed = Input.GetButtonDown("Jump_P2");
            jumpHeld = Input.GetButton("Jump_P2");
        }

        if (rb != null)
        {
            Vector2 movement = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
            rb.velocity = movement;

            if (moveHorizontal > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
            else if (moveHorizontal < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }

        if (jumpPressed && isGrounded && canJump && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            PlayerJump();
        }
    }

    void PlayerJump()
    {
        if (isGrounded && rb != null && canJump && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            canJump = false;
            isGrounded = false;

            StartCoroutine(JumpCooldown());
            Debug.Log($"🦘 Player2 saltó - Fuerza: {jumpForce}");
        }
    }

    void MakeDecision()
    {
        if (Time.time - lastDecisionTime < decisionRate) return;

        lastDecisionTime = Time.time;

        if (!isPlayerControlled && isNavigatingToGoal && goalTarget != null)
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > followDistance)
        {
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer < attackDistance)
        {
            MoveAwayFromPlayer();
        }
        else
        {
            RandomMovement();
        }

        if (isGrounded && canJump && Mathf.Abs(rb.velocity.y) < 0.1f && !isFloating)
        {
            if (Random.Range(0, 100) < 5)
            {
                Jump();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerTarget == null || rb == null) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * constantSpeed, rb.velocity.y);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    void MoveAwayFromPlayer()
    {
        if (playerTarget == null || rb == null) return;

        Vector2 direction = (transform.position - playerTarget.position).normalized;
        rb.velocity = new Vector2(direction.x * constantSpeed, rb.velocity.y);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    void RandomMovement()
    {
        if (rb == null) return;

        if (Random.Range(0, 100) < 20)
        {
            float randomDirection = Random.Range(-1f, 1f);
            rb.velocity = new Vector2(randomDirection * constantSpeed * 0.5f, rb.velocity.y);

            if (randomDirection > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
            else if (randomDirection < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

    void Jump()
    {
        if (isGrounded && canJump && rb != null && Mathf.Abs(rb.velocity.y) < 0.1f && !isFloating)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            canJump = false;
            isGrounded = false;

            StartCoroutine(JumpCooldown());
            Debug.Log($"🤖 Bot saltó - Fuerza: {jumpForce}, Velocidad Y: {rb.velocity.y}");
        }
    }

    IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
        Debug.Log("🔄 Bot puede saltar nuevamente");
    }

    void FindPlayerTarget()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.Find("Player1");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        if (playerTarget != null)
        {
            Debug.Log($"🎯 Bot encontró target: {playerTarget.name}");
        }
    }

    void FindGoalTarget()
    {
        Debug.Log("🔍 Buscando meta automáticamente...");

        GameObject goal = GameObject.FindGameObjectWithTag("Finish");
        if (goal == null)
        {
            goal = GameObject.FindGameObjectWithTag("Goal");
            Debug.Log("🔍 Buscando con tag 'Goal'...");
        }

        if (goal == null)
        {
            goal = GameObject.Find("FinishLine");
            Debug.Log("🔍 Buscando objeto 'FinishLine'...");
        }

        if (goal == null)
        {
            goal = GameObject.Find("Goal");
            Debug.Log("🔍 Buscando objeto 'Goal'...");
        }

        if (goal == null)
        {
            goal = GameObject.Find("Meta");
            Debug.Log("🔍 Buscando objeto 'Meta'...");
        }

        if (goal == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("finish") ||
                    obj.name.ToLower().Contains("goal") ||
                    obj.name.ToLower().Contains("meta") ||
                    obj.name.ToLower().Contains("end"))
                {
                    goal = obj;
                    Debug.Log($"🎯 Encontrado por nombre: {obj.name}");
                    break;
                }
            }
        }

        if (goal != null)
        {
            goalTarget = goal.transform;
            Debug.Log($"🎯 Bot encontró meta: {goal.name} en posición {goal.transform.position}");
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ NINGUNA META EN LA ESCENA!");
            Debug.Log("⚠️ Asegúrate de que existe un objeto con tag 'Finish', 'Goal' o nombre 'FinishLine'");
        }
    }

    void CreateTemporaryGoal()
    {
        Debug.Log("🏗️ Creando meta temporal...");

        GameObject tempGoal = new GameObject("TemporaryGoal");
        tempGoal.tag = "Finish";

        Vector3 goalPosition = transform.position + new Vector3(50f, 0f, 0f);
        tempGoal.transform.position = goalPosition;

        goalTarget = tempGoal.transform;

        Debug.Log($"🎯 Meta temporal creada en posición: {goalPosition}");

        if (!isPlayerControlled && autoNavigateToGoal)
        {
            SetupGoalNavigation();
        }
    }

    void SetupGoalNavigation()
    {
        if (goalTarget == null)
        {
            Debug.LogWarning("⚠️ No se puede configurar navegación: goalTarget es null");
            FindGoalTarget();
            if (goalTarget == null) return;
        }

        isNavigatingToGoal = true;
        startPositionForGoal = transform.position;

        constantSpeed = moveSpeed;

        Debug.Log($"🚀 Bot navegando a meta automáticamente");
        Debug.Log($"📍 Distancia a meta: {Vector2.Distance(startPositionForGoal, goalTarget.position):F2} unidades");
        Debug.Log($"🏃 VELOCIDAD CONSTANTE: {constantSpeed} unidades/segundo");
        Debug.Log($"🎯 Posición meta: {goalTarget.position}");
    }

    void MoveTowardsGoal()
    {
    }

    void MoveWhileFloating()
    {
        if (goalTarget == null || rb == null) return;

        Vector3 targetPosition = new Vector3(goalTarget.position.x, goalTarget.position.y, transform.position.z);
        Vector2 direction = (targetPosition - transform.position).normalized;

        if (isStuck)
        {
            HandleStuckSituation(direction);
            return;
        }

        float currentSpeed = constantSpeed;

        rb.velocity = new Vector2(direction.x * currentSpeed, floatSpeed);

        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎈 Bot FLOTANDO - Velocidad: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), " +
                     $"Posición X: {transform.position.x:F2}");
        }

        float distanceToGoal = Vector2.Distance(transform.position, targetPosition);
        if (distanceToGoal < 2f)
        {
            OnReachGoal();
        }
    }

    void HandleStuckSituation(Vector2 originalDirection)
    {
        Debug.Log($"🔄 Bot ATASCADO - Intentando solución...");

        if (rb.velocity.y < floatSpeed * 0.5f)
        {
            rb.velocity = new Vector2(rb.velocity.x, floatSpeed * 1.5f);
            Debug.Log($"⬆️ Intentando subir más alto...");
        }

        float aggressiveSpeed = constantSpeed * 2f;
        rb.AddForce(new Vector2(originalDirection.x * aggressiveSpeed * 20f, 0f));

        Vector2 randomDirection = new Vector2(originalDirection.x + Random.Range(-0.5f, 0.5f), 1f).normalized;
        rb.AddForce(randomDirection * aggressiveSpeed * 5f);

        stuckTimer += Time.deltaTime;
        if (stuckTimer > maxStuckTime)
        {
            Debug.Log($"🔄 Reseteando estado de atascamiento...");
            isStuck = false;
            stuckTimer = 0f;

            rb.velocity = new Vector2(originalDirection.x * constantSpeed * 3f, floatSpeed * 2f);
        }
    }

    void CheckIfStuck()
    {
        Vector3 currentPosition = transform.position;

        bool isMovingVerySlow = rb.velocity.magnitude < 1f;

        bool positionNotChanging = Vector3.Distance(currentPosition, lastPosition) < 0.1f;

        if ((isMovingVerySlow || positionNotChanging) && !isStuck)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 1.5f)
            {
                isStuck = true;
                Debug.Log($"🚧 Bot DETECTADO COMO ATASCADO - Velocidad: {rb.velocity.magnitude:F2}");
            }
        }
        else if (!isMovingVerySlow && !positionNotChanging)
        {
            stuckTimer = 0f;
            isStuck = false;
        }

        lastPosition = currentPosition;
    }

    bool ShouldJumpForObstacle()
    {
        if (isFloating) return false;

        if (Mathf.Abs(rb.velocity.x) < 0.1f) return false;

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float rayDistance = 1.0f;

        Debug.DrawRay(transform.position, direction * rayDistance, Color.yellow);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, groundLayerMask);

        if (hit.collider != null)
        {
            if (hit.distance < 0.5f)
            {
                float obstacleHeight = hit.collider.bounds.size.y;
                if (obstacleHeight > 0.5f)
                {
                    Debug.Log($"🚧 Obstáculo detectado: {hit.collider.gameObject.name} a distancia {hit.distance:F2} - SALTANDO");
                    return true;
                }
            }
        }

        return false;
    }

    void OnReachGoal()
    {
        isNavigatingToGoal = false;
        hasReachedGoal = true;
        isStuck = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;

            if (isFloating)
            {
                SetFloatingMode(false);
            }
        }

        ShowGameOverMessage();

        Debug.Log($"🎉 ¡Bot llegó a la meta!");
        Debug.Log($"⏱️ Tiempo transcurrido: {Time.time} segundos");
        Debug.Log($"🏃 Velocidad final: {constantSpeed}");
    }

    void ShowGameOverMessage()
    {
        if (!showGameOverMessage) return;

        gameOver = true;
        gameOverTime = Time.time;
        gameOverMessage = "GAME OVER\nEl Bot ha ganado!";

        Debug.Log($"🎮 {gameOverMessage}");

        DisablePlayerControls();
    }

    void DisablePlayerControls()
    {
        PlayerController player1 = FindObjectOfType<PlayerController>();
        if (player1 != null)
        {
            player1.SetControlsEnabled(false);
            Debug.Log($"🎮 Controles de Player1 desactivados");
        }

        BotController[] allBots = FindObjectsOfType<BotController>();
        foreach (BotController bot in allBots)
        {
            if (bot.isPlayerControlled)
            {
                bot.SetControlsEnabled(false);
                Debug.Log($"🎮 Controles de {bot.name} desactivados");
            }
        }
    }

    void OnGUI()
    {
        if (!gameOver || !showGameOverMessage) return;

        if (Time.time - gameOverTime > messageDisplayTime)
        {
            gameOver = false;
            return;
        }

        if (gameOverStyle == null)
        {
            gameOverStyle = new GUIStyle(GUI.skin.label);
            gameOverStyle.alignment = TextAnchor.MiddleCenter;
            gameOverStyle.fontSize = messageFontSize;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.normal.textColor = messageColor;
        }

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Rect messageRect = new Rect(0, screenHeight * 0.3f, screenWidth, 150);

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(0, screenHeight * 0.3f - 10, screenWidth, 170), "");
        GUI.color = Color.white;

        GUI.Label(messageRect, gameOverMessage, gameOverStyle);

        GUIStyle smallStyle = new GUIStyle(GUI.skin.label);
        smallStyle.alignment = TextAnchor.MiddleCenter;
        smallStyle.fontSize = 18;
        smallStyle.normal.textColor = Color.white;

        Rect subMessageRect = new Rect(0, screenHeight * 0.3f + 80, screenWidth, 30);
        GUI.Label(subMessageRect, $"Tiempo: {Time.time:F1} segundos", smallStyle);
    }

    public void SetAutoNavigateToGoal(bool navigate)
    {
        autoNavigateToGoal = navigate;

        if (navigate && !isPlayerControlled && goalTarget != null)
        {
            SetupGoalNavigation();
        }
        else
        {
            isNavigatingToGoal = false;
        }

        Debug.Log($"🤖 Bot auto-navegación a meta: {(navigate ? "ACTIVADA" : "DESACTIVADA")}");
    }

    public void SetConstantSpeed(float speed)
    {
        constantSpeed = speed;
        moveSpeed = speed;
        Debug.Log($"🏃 Velocidad constante configurada a: {constantSpeed}");
    }

    public void SetGoalReachTime(float seconds)
    {
        goalReachTime = seconds;
        Debug.Log($"⏱️ Tiempo objetivo cambiado a: {seconds} segundos");
    }

    public void SetGoalTarget(Transform goal)
    {
        goalTarget = goal;

        if (!isPlayerControlled && autoNavigateToGoal)
        {
            SetupGoalNavigation();
        }

        Debug.Log($"🎯 Meta establecida manualmente: {goal.name}");
    }

    public void SetFloatingMode(bool floating, float speed = 3f)
    {
        isFloating = floating;
        floatSpeed = speed;

        if (floating)
        {
            Debug.Log($"🎈 Bot entrando en modo FLOTACIÓN - Velocidad: {floatSpeed}");
            floatingStartTime = Time.time;
            lastPosition = transform.position;
            isStuck = false;
            stuckTimer = 0f;

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = new Vector2(rb.velocity.x, floatSpeed);

                rb.AddForce(new Vector2(0f, floatSpeed * 50f));
            }
        }
        else
        {
            Debug.Log($"⬇️ Bot saliendo de modo flotación - Gravedad restaurada");
            if (rb != null)
            {
                rb.gravityScale = 4f;
            }
        }
    }

    public bool HasReachedGoal()
    {
        return hasReachedGoal;
    }

    public bool IsFloating()
    {
        return isFloating;
    }

    public void SetIgnoreCheckpoints(bool ignore)
    {
        shouldIgnoreCheckpoints = ignore;
        Debug.Log($"🤖 Bot ignore checkpoints: {shouldIgnoreCheckpoints}");
    }

    public bool ShouldIgnoreCheckpoint()
    {
        if (!isPlayerControlled && shouldIgnoreCheckpoints)
        {
            return true;
        }
        return false;
    }

    public void TriggerChallenge(Checkpoint checkpoint)
    {
        if (!isPlayerControlled && shouldIgnoreCheckpoints)
        {
            Debug.Log($"🚫 Bot ignorando checkpoint {checkpoint.checkpointNumber} - Yendo directo a meta");
            return;
        }

        if (isAnsweringChallenge) return;

        isAnsweringChallenge = true;
        currentCheckpoint = checkpoint;

        Debug.Log($"❓ {(isPlayerControlled ? "Player2" : "Bot")} activó desafío en checkpoint {checkpoint.checkpointNumber}");

        if (!isPlayerControlled)
        {
            StartCoroutine(AutoAnswerChallenge());
        }
    }

    private IEnumerator AutoAnswerChallenge()
    {
        Debug.Log($"🤖 Bot comenzando a responder desafío...");
        yield return new WaitForSeconds(questionAnswerDelay);

        string randomAnswer = GenerateRandomAnswer();
        CompleteChallenge(randomAnswer);
    }

    private string GenerateRandomAnswer()
    {
        int answerLength = Random.Range(3, 6);
        string answer = "";

        for (int i = 0; i < answerLength; i++)
        {
            if (Random.Range(0, 2) == 0)
            {
                answer += Random.Range(1, 10).ToString();
            }
            else
            {
                answer += (char)Random.Range('A', 'D' + 1);
            }
        }

        return answer;
    }

    public void CompleteChallenge(string answer)
    {
        if (!isAnsweringChallenge) return;

        Debug.Log($"✅ {(isPlayerControlled ? "Player2" : "Bot")} respondió desafío con: {answer}");

        isAnsweringChallenge = false;

        if (currentCheckpoint != null)
        {
            currentCheckpoint.CompleteChallenge();
            currentCheckpoint = null;
        }

        if (challengeManager != null)
        {
            challengeManager.OnChallengeCompleted(gameObject);
        }
    }

    public bool IsAnsweringChallenge()
    {
        return isAnsweringChallenge;
    }

    public void AnswerQuestion(int questionType = 0)
    {
        Debug.Log($"🤖 {(isPlayerControlled ? "Player2" : "Bot")} respondiendo pregunta tipo: {questionType}");
        StartCoroutine(AnswerWithDelay(questionType));
    }

    private IEnumerator AnswerWithDelay(int questionType = 0)
    {
        yield return new WaitForSeconds(questionAnswerDelay);
        Debug.Log($"✅ {(isPlayerControlled ? "Player2" : "Bot")} respondió pregunta tipo: {questionType}");
    }

    public void SetPlayerControl(bool shouldBePlayerControlled)
    {
        isPlayerControlled = shouldBePlayerControlled;

        if (isPlayerControlled)
        {
            gameObject.tag = "Bot";
            Debug.Log("🎮 Bot ahora es Player 2 - Controlado por JUGADOR (tag: Bot)");
        }
        else
        {
            gameObject.tag = "Bot";
            Debug.Log("🤖 Bot ahora es controlado por IA");
            FindPlayerTarget();
        }
    }

    public string GetHorizontalAxis()
    {
        return "Horizontal_P2";
    }

    public string GetJumpButton()
    {
        return "Jump_P2";
    }

    public void DebugControls()
    {
        Debug.Log($"🎮 Bot Controls - PlayerControl: {isPlayerControlled}, Horizontal: 'Horizontal_P2', Jump: 'Jump_P2', Grounded: {isGrounded}, AnsweringChallenge: {isAnsweringChallenge}");
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (!enabled && rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (!isPlayerControlled && !enabled)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }

        Debug.Log($"🎮 BotController - Controles {(enabled ? "activados" : "desactivados")} para: {gameObject.name}");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (!isPlayerControlled)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, followDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);

            if (isNavigatingToGoal && goalTarget != null)
            {
                Gizmos.color = isFloating ? Color.magenta : Color.yellow;
                Gizmos.DrawLine(transform.position, goalTarget.position);
                Gizmos.DrawWireSphere(goalTarget.position, 1f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Player2") ||
            collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log($"⚠️ Bot colisionó con jugador: {collision.gameObject.name} - Ignorando colisión");
            Physics2D.IgnoreCollision(collider2d, collision.collider, true);
        }

        if (canPassThroughWalls)
        {
            string objectName = collision.gameObject.name.ToLower();
            if (objectName.Contains("wall") ||
                objectName.Contains("pared") ||
                objectName.Contains("obstacle") ||
                objectName.Contains("obstaculo") ||
                objectName.Contains("barrier") ||
                objectName.Contains("barrera") ||
                objectName.Contains("block") ||
                objectName.Contains("bloque"))
            {
                Debug.Log($"🧱 Bot colisionó con pared: {collision.gameObject.name} - Ignorando colisión y continuando");
                Physics2D.IgnoreCollision(collider2d, collision.collider, true);

                if (rb != null && goalTarget != null)
                {
                    Vector2 direction = (goalTarget.position - transform.position).normalized;
                    rb.AddForce(direction * constantSpeed * 150f);
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (canPassThroughWalls)
        {
            string objectName = collision.gameObject.name.ToLower();
            if (objectName.Contains("wall") ||
                objectName.Contains("pared") ||
                objectName.Contains("obstacle") ||
                objectName.Contains("obstaculo") ||
                objectName.Contains("barrier") ||
                objectName.Contains("barrera") ||
                objectName.Contains("block") ||
                objectName.Contains("bloque"))
            {
                if (rb != null && goalTarget != null && isNavigatingToGoal)
                {
                    Vector2 direction = (goalTarget.position - transform.position).normalized;
                    rb.AddForce(direction * constantSpeed * 50f);

                    Physics2D.IgnoreCollision(collider2d, collision.collider, true);
                }
            }
        }
    }
}