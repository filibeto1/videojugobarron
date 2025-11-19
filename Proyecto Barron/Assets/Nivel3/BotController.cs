using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
{
    [Header("Bot Movement Settings")]
    public float moveSpeed = 15f;
    public float jumpForce = 25f;
    public float jumpCooldown = 0.8f;
    public float decisionRate = 0.3f;

    [Header("Bot Behavior")]
    public float followDistance = 3f;
    public float attackDistance = 2f;

    [Header("AI Navigation to Goal")]
    public bool autoNavigateToGoal = true;
    public Transform goalTarget;
    public float goalReachTime = 120f;
    private float calculatedSpeed = 0f;
    private bool isNavigatingToGoal = false;
    private Vector3 startPositionForGoal;

    [Header("Question System")]
    public float questionAnswerDelay = 1.5f;

    [Header("Control Settings")]
    public bool isPlayerControlled = false;
    public bool controlsEnabled = true;

    [Header("Multi-Keyboard Settings")]
    public int playerNumber = 2; // 1 para Player1, 2 para Player2
    public bool useMultiKeyboardSystem = true;

    [Header("Scale Protection")]
    public Vector3 originalScale;
    private bool scaleInitialized = false;

    [Header("Collision Settings")]
    public bool canPassThroughPlayers = true;
    public bool canPassThroughBots = true;

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

    // VARIABLES PARA SISTEMA DE CHECKPOINTS
    private bool isAnsweringChallenge = false;
    private Checkpoint currentCheckpoint;
    private SequenceChallengeManager challengeManager;

    // ✅ NUEVA VARIABLE: Ignorar checkpoints en modo bot
    private bool shouldIgnoreCheckpoints = true;

    // ✅ NUEVAS VARIABLES: Para navegación mejorada durante flotación
    private float floatingStartTime;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float maxStuckTime = 3f;
    private bool isStuck = false;

    // ✅ NUEVAS VARIABLES: Para mensaje de Game Over
    private bool gameOver = false;
    private float gameOverTime = 0f;
    private string gameOverMessage = "";
    private GUIStyle gameOverStyle;

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

        // CONFIGURACIÓN DE FÍSICA MEJORADA
        if (rb != null)
        {
            rb.gravityScale = 4f;
            rb.drag = 0.5f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // VERIFICAR CONFIGURACIÓN DE CAPAS
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

        // ✅ CONFIGURAR IGNORAR CHECKPOINTS AUTOMÁTICAMENTE EN MODO IA
        if (!isPlayerControlled)
        {
            shouldIgnoreCheckpoints = ignoreCheckpointsInAIMode;
            Debug.Log($"🤖 Bot configurado para IGNORAR checkpoints: {shouldIgnoreCheckpoints}");
        }

        // ✅ CONFIGURAR COLISIONES - DEBE SER LO PRIMERO
        SetupAllCollisions();

        // ✅ BUSCAR META AUTOMÁTICAMENTE
        if (goalTarget == null)
        {
            FindGoalTarget();
        }

        // ✅ SI NO SE ENCUENTRA META, CREAR UNA TEMPORAL
        if (goalTarget == null && !isPlayerControlled && autoNavigateToGoal)
        {
            CreateTemporaryGoal();
        }

        // ✅ CONFIGURAR NAVEGACIÓN A META SI ES IA
        if (!isPlayerControlled && autoNavigateToGoal && goalTarget != null)
        {
            SetupGoalNavigation();
        }

        if (!isPlayerControlled)
        {
            FindPlayerTarget();
        }

        // Asegurar que no esté saltando al inicio
        canJump = true;
        isGrounded = false;

        // Forzar verificación de suelo después de un breve delay
        StartCoroutine(InitialGroundCheck());

        Debug.Log($"🤖 Bot inicializado - Control: {(isPlayerControlled ? "JUGADOR" : "IA")}");
        Debug.Log($"📏 Escala original: {originalScale}");
        Debug.Log($"🔍 Configuración de detección de suelo - LayerMask: {groundLayerMask.value}");
    }

    IEnumerator InitialGroundCheck()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateGroundDetection();
    }

    // ✅ MÉTODO MEJORADO: Configurar TODAS las colisiones
    private void SetupAllCollisions()
    {
        if (collider2d == null)
        {
            Debug.LogError("❌ No hay Collider2D en Bot: " + gameObject.name);
            return;
        }

        Debug.Log($"🔧 Configurando colisiones para Bot: {gameObject.name}");

        // Buscar TODOS los jugadores en la escena
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        BotController[] allBots = FindObjectsOfType<BotController>();

        int playersIgnored = 0;
        int botsIgnored = 0;

        // ✅ DESACTIVAR COLISIONES CON PLAYERS
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

        // ✅ DESACTIVAR COLISIONES CON OTROS BOTS (si está configurado)
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

        Debug.Log($"✅ Colisiones configuradas - Players ignorados: {playersIgnored}, Bots ignorados: {botsIgnored}");

        // ✅ CONFIGURAR LAYERS PARA EVITAR COLISIONES
        SetupLayerCollisions();
    }

    // ✅ NUEVO MÉTODO: Configurar colisiones por layers
    private void SetupLayerCollisions()
    {
        // Asegurar que el bot esté en una layer específica
        string botLayerName = "Bot";
        int botLayer = LayerMask.NameToLayer(botLayerName);

        if (botLayer == -1)
        {
            Debug.LogWarning($"⚠️ Layer '{botLayerName}' no existe. Creándola...");
            // En un proyecto real, deberías crear la layer manualmente en Project Settings
            botLayer = 8; // Asumiendo que la layer 8 está disponible
        }

        gameObject.layer = botLayer;

        // Desactivar colisiones con layers de players
        int playerLayer = LayerMask.NameToLayer("Player");
        int defaultLayer = LayerMask.NameToLayer("Default");

        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(botLayer, playerLayer, true);
            Debug.Log($"🎯 Colisiones desactivadas entre layers: Bot <> Player");
        }

        if (defaultLayer != -1 && defaultLayer != botLayer)
        {
            Physics2D.IgnoreLayerCollision(botLayer, defaultLayer, false); // Mantener colisión con suelo
        }

        Debug.Log($"🏷️ Bot {gameObject.name} asignado a layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    void FixedUpdate()
    {
        // Usar FixedUpdate para física más estable
        UpdateGroundDetection();

        // ✅ NUEVO: Verificar si está atascado durante flotación
        if (isFloating && isNavigatingToGoal)
        {
            CheckIfStuck();
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

    // ========================================
    // MÉTODOS DE DEBUG MEJORADOS
    // ========================================

    void DebugBotState()
    {
        if (Time.frameCount % 120 == 0) // Log cada 120 frames para no saturar
        {
            string floatingStatus = isFloating ? "🎈 FLOTANDO" : "⬇️ NORMAL";
            string groundedStatus = isGrounded ? "🏠 EN SUELO" : "🌪️ EN AIRE";
            string stuckStatus = isStuck ? "🚧 ATASCADO" : "🏃 MOVIÉNDOSE";

            Debug.Log($"🤖 Estado Bot - {floatingStatus}, {groundedStatus}, {stuckStatus}, " +
                     $"VelY: {(rb != null ? rb.velocity.y.ToString("F2") : "N/A")}, " +
                     $"PosY: {transform.position.y:F2}");
        }
    }

    void DebugGoalNavigation()
    {
        if (Time.frameCount % 120 == 0) // Log cada 120 frames
        {
            string floatingStatus = isFloating ? "🎈 FLOTANDO" : "⬇️ NORMAL";

            Debug.Log($"🎯 DEBUG META - {floatingStatus}, GoalTarget: {(goalTarget != null ? goalTarget.name : "NULL")}, " +
                     $"Navigating: {isNavigatingToGoal}, AutoNavigate: {autoNavigateToGoal}, " +
                     $"PlayerControl: {isPlayerControlled}");

            if (goalTarget != null)
            {
                float distance = Vector2.Distance(transform.position, goalTarget.position);
                Debug.Log($"📍 Distancia a meta: {distance:F2}, Posición Y: {transform.position.y:F2}");
            }

            if (rb != null)
            {
                Debug.Log($"🏃 Velocidad Bot - X: {rb.velocity.x:F2}, Y: {rb.velocity.y:F2}");
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

            // DEBUG VISUAL
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckRadius,
                         isGrounded ? Color.green : Color.red);

            // Si acaba de tocar el suelo, permitir saltar nuevamente
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

    // CONTROL POR JUGADOR HUMANO (PLAYER2) - MODIFICADO PARA MULTI-TECLADO
    void HandlePlayerControl()
    {
        float moveHorizontal = 0f;
        bool jumpPressed = false;
        bool jumpHeld = false;

        if (useMultiKeyboardSystem && MultiKeyboardInputManager.Instance != null)
        {
            // Usar el nuevo sistema de múltiples teclados
            moveHorizontal = MultiKeyboardInputManager.Instance.GetHorizontal(playerNumber);
            jumpPressed = MultiKeyboardInputManager.Instance.GetJump(playerNumber);
            jumpHeld = MultiKeyboardInputManager.Instance.GetJumpHeld(playerNumber);
        }
        else
        {
            // Sistema antiguo (compatibilidad)
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

    // COMPORTAMIENTO DE IA
    void MakeDecision()
    {
        if (Time.time - lastDecisionTime < decisionRate) return;

        lastDecisionTime = Time.time;

        // ✅ SI ESTÁ EN MODO NAVEGACIÓN A META, IR DIRECTO A LA META
        if (!isPlayerControlled && isNavigatingToGoal && goalTarget != null)
        {
            MoveTowardsGoal();
            return;
        }

        // Comportamiento normal de seguir al jugador
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

        // MEJORAR LÓGICA DE SALTO - SOLO SALTAR SI ESTÁ EN SUELO Y NO ESTÁ YA SALTANDO
        if (isGrounded && canJump && Mathf.Abs(rb.velocity.y) < 0.1f && !isFloating)
        {
            if (Random.Range(0, 100) < 20) // Reducir probabilidad de salto
            {
                Jump();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerTarget == null || rb == null) return;

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

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
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

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
            rb.velocity = new Vector2(randomDirection * moveSpeed * 0.5f, rb.velocity.y);

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

    // ========================================
    // SISTEMA DE NAVEGACIÓN A META MEJORADO
    // ========================================

    void FindGoalTarget()
    {
        Debug.Log("🔍 Buscando meta automáticamente...");

        // Buscar por tag "Finish" o "Goal"
        GameObject goal = GameObject.FindGameObjectWithTag("Finish");
        if (goal == null)
        {
            goal = GameObject.FindGameObjectWithTag("Goal");
            Debug.Log("🔍 Buscando con tag 'Goal'...");
        }

        // Buscar por nombre "FinishLine" o similar
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

        // BUSCAR POR TIPO si no se encuentra por nombre/tag
        if (goal == null)
        {
            // Buscar cualquier objeto que pueda ser la meta
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

        // Colocar la meta a una distancia razonable del bot (50 unidades a la derecha)
        Vector3 goalPosition = transform.position + new Vector3(50f, 0f, 0f);
        tempGoal.transform.position = goalPosition;

        goalTarget = tempGoal.transform;

        Debug.Log($"🎯 Meta temporal creada en posición: {goalPosition}");

        // Configurar navegación
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
            FindGoalTarget(); // Buscar nuevamente
            if (goalTarget == null) return;
        }

        isNavigatingToGoal = true;
        startPositionForGoal = transform.position;

        // Calcular distancia total a la meta
        float distanceToGoal = Vector2.Distance(startPositionForGoal, goalTarget.position);

        // Calcular velocidad necesaria para llegar en exactamente goalReachTime segundos
        calculatedSpeed = distanceToGoal / goalReachTime;

        // Asegurar velocidad mínima
        if (calculatedSpeed < 5f) calculatedSpeed = 5f;

        Debug.Log($"🚀 Bot navegando a meta automáticamente");
        Debug.Log($"📍 Distancia a meta: {distanceToGoal:F2} unidades");
        Debug.Log($"⏱️ Tiempo objetivo: {goalReachTime} segundos");
        Debug.Log($"🏃 Velocidad calculada: {calculatedSpeed:F2} unidades/segundo");
        Debug.Log($"🎯 Posición meta: {goalTarget.position}");
    }

    void MoveTowardsGoal()
    {
        if (goalTarget == null || rb == null)
        {
            Debug.LogWarning("⚠️ No se puede mover a meta: goalTarget o rb es null");
            return;
        }

        // ✅ SI ESTÁ FLOTANDO, COMPORTAMIENTO ESPECIAL
        if (isFloating)
        {
            MoveWhileFloating();
            return;
        }

        // Calcular dirección hacia la meta (ignorando Z)
        Vector3 targetPosition = new Vector3(goalTarget.position.x, goalTarget.position.y, transform.position.z);
        Vector2 direction = (targetPosition - transform.position).normalized;

        // DEBUG de dirección
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🧭 Dirección a meta: ({direction.x:F2}, {direction.y:F2}), " +
                     $"Distancia: {Vector2.Distance(transform.position, targetPosition):F2}");
            Debug.Log($"🎯 Posición Meta: {goalTarget.position}, Posición Bot: {transform.position}");
        }

        // Mover hacia la meta con la velocidad calculada
        float currentSpeed = Mathf.Max(calculatedSpeed, 5f); // Velocidad mínima
        rb.velocity = new Vector2(direction.x * currentSpeed, rb.velocity.y);

        // Voltear sprite según dirección
        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        // Saltar solo si hay obstáculos y está en suelo
        if (isGrounded && canJump && ShouldJumpForObstacle() && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            // Reducir probabilidad de salto para evitar saltos constantes
            if (Random.Range(0, 100) < 40) // 40% de probabilidad
            {
                Jump();
            }
        }

        // Verificar si llegó a la meta (usando posición corregida)
        float distanceToGoal = Vector2.Distance(transform.position, targetPosition);
        if (distanceToGoal < 2f) // Aumentar rango de detección
        {
            OnReachGoal();
        }
    }

    // ✅ NUEVO MÉTODO MEJORADO: Movimiento mientras flota - IGNORA OBSTÁCULOS
    void MoveWhileFloating()
    {
        if (goalTarget == null || rb == null) return;

        // Calcular dirección hacia la meta (ignorando Z)
        Vector3 targetPosition = new Vector3(goalTarget.position.x, goalTarget.position.y, transform.position.z);
        Vector2 direction = (targetPosition - transform.position).normalized;

        // ✅ NUEVO: Si está atascado, intentar una ruta alternativa
        if (isStuck)
        {
            HandleStuckSituation(direction);
            return;
        }

        // ✅ MEJORADO: Fuerza de movimiento más agresiva durante flotación
        float currentSpeed = Mathf.Max(calculatedSpeed * 1.5f, 8f); // Mayor velocidad cuando flota

        // ✅ MEJORADO: Aplicar fuerza constante en lugar de solo velocidad
        Vector2 desiredVelocity = new Vector2(direction.x * currentSpeed, floatSpeed);

        // Suavizar el movimiento pero mantener fuerza constante
        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, 0.3f);

        // ✅ NUEVO: Fuerza adicional si se está moviendo muy lento
        if (Mathf.Abs(rb.velocity.x) < 2f)
        {
            rb.AddForce(new Vector2(direction.x * currentSpeed * 10f, 0f));
        }

        // Voltear sprite según dirección horizontal
        if (direction.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        // Debug de flotación
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎈 Bot FLOTANDO - Velocidad: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), " +
                     $"Fuerza X: {direction.x * currentSpeed:F2}, Posición Y: {transform.position.y:F2}");
        }

        // Verificar si llegó a la meta
        float distanceToGoal = Vector2.Distance(transform.position, targetPosition);
        if (distanceToGoal < 2f)
        {
            OnReachGoal();
        }
    }

    // ✅ NUEVO MÉTODO: Manejar situación de atascamiento
    void HandleStuckSituation(Vector2 originalDirection)
    {
        Debug.Log($"🔄 Bot ATASCADO - Intentando solución...");

        // ✅ ESTRATEGIA 1: Intentar moverse más arriba
        if (rb.velocity.y < floatSpeed * 0.5f)
        {
            rb.velocity = new Vector2(rb.velocity.x, floatSpeed * 1.5f);
            Debug.Log($"⬆️ Intentando subir más alto...");
        }

        // ✅ ESTRATEGIA 2: Movimiento lateral más agresivo
        float aggressiveSpeed = calculatedSpeed * 2f;
        rb.AddForce(new Vector2(originalDirection.x * aggressiveSpeed * 20f, 0f));

        // ✅ ESTRATEGIA 3: Pequeño movimiento aleatorio para desatascar
        Vector2 randomDirection = new Vector2(originalDirection.x + Random.Range(-0.5f, 0.5f), 1f).normalized;
        rb.AddForce(randomDirection * aggressiveSpeed * 5f);

        // ✅ ESTRATEGIA 4: Temporizador para resetear el stuck
        stuckTimer += Time.deltaTime;
        if (stuckTimer > maxStuckTime)
        {
            Debug.Log($"🔄 Reseteando estado de atascamiento...");
            isStuck = false;
            stuckTimer = 0f;

            // Dar un impulso fuerte
            rb.velocity = new Vector2(originalDirection.x * calculatedSpeed * 3f, floatSpeed * 2f);
        }
    }

    // ✅ NUEVO MÉTODO: Verificar si el bot está atascado
    void CheckIfStuck()
    {
        // Guardar posición actual
        Vector3 currentPosition = transform.position;

        // Verificar si se está moviendo muy lento
        bool isMovingVerySlow = rb.velocity.magnitude < 1f;

        // Verificar si la posición no ha cambiado significativamente
        bool positionNotChanging = Vector3.Distance(currentPosition, lastPosition) < 0.1f;

        if ((isMovingVerySlow || positionNotChanging) && !isStuck)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 1.5f) // 1.5 segundos de inmovilidad
            {
                isStuck = true;
                Debug.Log($"🚧 Bot DETECTADO COMO ATASCADO - Velocidad: {rb.velocity.magnitude:F2}");
            }
        }
        else if (!isMovingVerySlow && !positionNotChanging)
        {
            // Se está moviendo normalmente, resetear timer
            stuckTimer = 0f;
            isStuck = false;
        }

        // Actualizar última posición
        lastPosition = currentPosition;
    }

    bool ShouldJumpForObstacle()
    {
        // No saltar si está flotando
        if (isFloating) return false;

        // Raycast hacia adelante para detectar obstáculos
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float rayDistance = 2f; // Aumentar distancia de detección

        Debug.DrawRay(transform.position, direction * rayDistance, Color.yellow);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, groundLayerMask);

        if (hit.collider != null)
        {
            Debug.Log($"🚧 Obstáculo detectado: {hit.collider.gameObject.name} a distancia {hit.distance:F2}");
            return true;
        }

        return false;
    }

    // ✅ MÉTODO MEJORADO: Cuando llega a la meta
    void OnReachGoal()
    {
        isNavigatingToGoal = false;
        hasReachedGoal = true;
        isStuck = false; // Resetear estado de atascamiento

        if (rb != null)
        {
            rb.velocity = Vector2.zero;

            // ✅ Restaurar gravedad si estaba flotando
            if (isFloating)
            {
                SetFloatingMode(false);
            }
        }

        // ✅ NUEVO: Mostrar mensaje de Game Over
        ShowGameOverMessage();

        Debug.Log($"🎉 ¡Bot llegó a la meta!");
        Debug.Log($"⏱️ Tiempo transcurrido: {Time.time} segundos");
    }

    // ✅ NUEVO MÉTODO: Mostrar mensaje de Game Over
    void ShowGameOverMessage()
    {
        if (!showGameOverMessage) return;

        gameOver = true;
        gameOverTime = Time.time;
        gameOverMessage = "GAME OVER\nEl Bot ha ganado!";

        Debug.Log($"🎮 {gameOverMessage}");

        // ✅ Desactivar controles del jugador si existe
        DisablePlayerControls();

        // ✅ Opcional: Pausar el juego
        // Time.timeScale = 0f;
    }

    // ✅ NUEVO MÉTODO: Desactivar controles del jugador
    void DisablePlayerControls()
    {
        // Buscar y desactivar controles del Player1
        PlayerController player1 = FindObjectOfType<PlayerController>();
        if (player1 != null)
        {
            player1.SetControlsEnabled(false);
            Debug.Log($"🎮 Controles de Player1 desactivados");
        }

        // Buscar y desactivar otros bots controlados por jugador
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

    // ✅ NUEVO MÉTODO: Dibujar mensaje de Game Over en pantalla
    void OnGUI()
    {
        if (!gameOver || !showGameOverMessage) return;

        // Verificar si el mensaje aún debe mostrarse
        if (Time.time - gameOverTime > messageDisplayTime)
        {
            gameOver = false;
            return;
        }

        // Crear estilo para el mensaje si no existe
        if (gameOverStyle == null)
        {
            gameOverStyle = new GUIStyle(GUI.skin.label);
            gameOverStyle.alignment = TextAnchor.MiddleCenter;
            gameOverStyle.fontSize = messageFontSize;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.normal.textColor = messageColor;
        }

        // Calcular posición centrada
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Rect messageRect = new Rect(0, screenHeight * 0.3f, screenWidth, 150);

        // Fondo semi-transparente
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(0, screenHeight * 0.3f - 10, screenWidth, 170), "");
        GUI.color = Color.white;

        // Mostrar mensaje
        GUI.Label(messageRect, gameOverMessage, gameOverStyle);

        // Mostrar mensaje adicional más pequeño
        GUIStyle smallStyle = new GUIStyle(GUI.skin.label);
        smallStyle.alignment = TextAnchor.MiddleCenter;
        smallStyle.fontSize = 18;
        smallStyle.normal.textColor = Color.white;

        Rect subMessageRect = new Rect(0, screenHeight * 0.3f + 80, screenWidth, 30);
        GUI.Label(subMessageRect, $"Tiempo: {Time.time:F1} segundos", smallStyle);
    }

    // Método público para activar/desactivar navegación a meta
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

    // Método público para cambiar el tiempo objetivo
    public void SetGoalReachTime(float seconds)
    {
        goalReachTime = seconds;

        if (isNavigatingToGoal && goalTarget != null)
        {
            SetupGoalNavigation(); // Recalcular velocidad
        }

        Debug.Log($"⏱️ Tiempo objetivo cambiado a: {seconds} segundos ({seconds / 60f:F1} minutos)");
    }

    // Método público para establecer meta manualmente
    public void SetGoalTarget(Transform goal)
    {
        goalTarget = goal;

        if (!isPlayerControlled && autoNavigateToGoal)
        {
            SetupGoalNavigation();
        }

        Debug.Log($"🎯 Meta establecida manualmente: {goal.name}");
    }

    // ========================================
    // SISTEMA DE FLOTACIÓN PARA BOT
    // ========================================

    // ✅ NUEVO: Método para activar/desactivar modo flotación
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
                // Aplicar velocidad inicial de flotación
                rb.velocity = new Vector2(rb.velocity.x, floatSpeed);

                // ✅ NUEVO: Impulso inicial fuerte
                rb.AddForce(new Vector2(0f, floatSpeed * 50f));
            }
        }
        else
        {
            Debug.Log($"⬇️ Bot saliendo de modo flotación - Gravedad restaurada");
            if (rb != null)
            {
                rb.gravityScale = 4f; // Gravedad normal
            }
        }
    }

    // ✅ NUEVO: Método para verificar si llegó a la meta
    public bool HasReachedGoal()
    {
        return hasReachedGoal;
    }

    // ✅ NUEVO: Método para verificar si está flotando
    public bool IsFloating()
    {
        return isFloating;
    }

    // ========================================
    // SISTEMA PARA IGNORAR CHECKPOINTS EN MODO BOT
    // ========================================

    // Método público para configurar si ignora checkpoints
    public void SetIgnoreCheckpoints(bool ignore)
    {
        shouldIgnoreCheckpoints = ignore;
        Debug.Log($"🤖 Bot ignore checkpoints: {shouldIgnoreCheckpoints}");
    }

    // Método para verificar si debe ignorar un checkpoint
    public bool ShouldIgnoreCheckpoint()
    {
        // En modo IA, siempre ignorar checkpoints
        if (!isPlayerControlled && shouldIgnoreCheckpoints)
        {
            return true;
        }
        return false;
    }

    // MÉTODOS PARA SISTEMA DE CHECKPOINTS - MODIFICADO
    public void TriggerChallenge(Checkpoint checkpoint)
    {
        // ✅ NUEVO: Ignorar checkpoints si está en modo bot
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

    // ========================================
    // CONTROL DE JUGADOR
    // ========================================

    // ✅ MÉTODO PARA CONTROL DE JUGADOR
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

    // MÉTODOS GETTER PARA DEBUG
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
            // Dibujar rangos de seguimiento al jugador
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, followDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);

            // Dibujar línea hacia la meta si está navegando
            if (isNavigatingToGoal && goalTarget != null)
            {
                Gizmos.color = isFloating ? Color.magenta : Color.yellow;
                Gizmos.DrawLine(transform.position, goalTarget.position);
                Gizmos.DrawWireSphere(goalTarget.position, 1f);
            }
        }
    }

    // ✅ NUEVO: Verificar colisiones en tiempo real
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el bot colisiona con un jugador, ignorar la colisión
        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Player2") ||
            collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log($"⚠️ Bot colisionó con jugador: {collision.gameObject.name} - Ignorando colisión");
            Physics2D.IgnoreCollision(collider2d, collision.collider, true);
        }
    }
}