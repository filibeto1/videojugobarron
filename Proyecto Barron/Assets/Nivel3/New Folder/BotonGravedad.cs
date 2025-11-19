using UnityEngine;
using System.Collections;

public class BotonGravedad : MonoBehaviour
{
    [Header("Configuración del Botón")]
    public KeyCode activationKey = KeyCode.E;
    public float gravityDuration = 6f;
    public float floatSpeed = 3f;
    public bool canBeUsedMultipleTimes = true;
    public float cooldownTime = 2f;

    [Header("Configuración para Bots")]
    public bool autoActivateForBots = true;
    public float botActivationDistance = 5f;
    public bool infiniteDurationForBots = true;

    [Header("Efectos Visuales")]
    public Color normalColor = Color.green;
    public Color activeColor = Color.cyan;
    public Color cooldownColor = Color.gray;
    public Color botActiveColor = Color.magenta;

    [Header("UI Prompt")]
    public bool showPrompt = true;
    public string promptText = "Presiona [E] para activar Gravedad Cero";
    public float promptDistance = 3f;

    [Header("Audio (Opcional)")]
    public AudioClip activationSound;
    public AudioClip deactivationSound;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isPlayerNearby = false;
    private GameObject currentPlayer;
    private bool isOnCooldown = false;
    private Coroutine cooldownCoroutine;
    private bool isBotUsing = false;
    private GameObject currentBotUser;

    void Start()
    {
        // Obtener o crear componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.LogWarning("⚠️ BotonGravedad no tiene SpriteRenderer, se agregó uno automáticamente");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configurar collider si no existe
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.isTrigger = true;
            circleCol.radius = Mathf.Max(promptDistance, botActivationDistance);
            Debug.LogWarning("⚠️ BotonGravedad no tiene Collider2D, se agregó CircleCollider2D");
        }
        else
        {
            col.isTrigger = true;
        }

        // Color inicial
        UpdateButtonColor(normalColor);

        Debug.Log($"✅ BotonGravedad inicializado - Tecla: {activationKey}, Duración: {gravityDuration}s");
        Debug.Log($"🤖 Configuración Bots: Auto-activar: {autoActivateForBots}, Distancia: {botActivationDistance}, Duración infinita: {infiniteDurationForBots}");
    }

    void Update()
    {
        // Detectar si el jugador presiona la tecla E
        if (isPlayerNearby && currentPlayer != null && !isOnCooldown)
        {
            if (Input.GetKeyDown(activationKey))
            {
                ActivateZeroGravity(currentPlayer);
            }
        }

        // ✅ NUEVO: Detectar y activar automáticamente para bots
        if (autoActivateForBots && !isOnCooldown)
        {
            CheckForNearbyBots();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar cuando un jugador entra en el rango
        if (IsPlayer(other.gameObject))
        {
            isPlayerNearby = true;
            currentPlayer = other.gameObject;
            Debug.Log($"🎯 Jugador {other.name} cerca del BotonGravedad");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Detectar cuando un jugador sale del rango
        if (IsPlayer(other.gameObject) && other.gameObject == currentPlayer)
        {
            isPlayerNearby = false;
            currentPlayer = null;
            Debug.Log($"🚶 Jugador {other.name} se alejó del BotonGravedad");
        }
    }

    // ✅ NUEVO MÉTODO: Buscar bots cercanos automáticamente
    private void CheckForNearbyBots()
    {
        if (isBotUsing) return;

        // Buscar todos los bots en la escena
        BotController[] allBots = FindObjectsOfType<BotController>();

        foreach (BotController bot in allBots)
        {
            // Solo bots controlados por IA (no Player2)
            if (bot != null && !bot.isPlayerControlled && bot.controlsEnabled)
            {
                float distance = Vector2.Distance(transform.position, bot.transform.position);

                if (distance <= botActivationDistance)
                {
                    Debug.Log($"🤖 Bot {bot.name} detectado cerca del botón (distancia: {distance:F2})");
                    ActivateZeroGravityForBot(bot.gameObject);
                    break; // Activar solo para el primer bot cercano
                }
            }
        }
    }

    // ✅ NUEVO MÉTODO: Activar gravedad cero específicamente para bots
    private void ActivateZeroGravityForBot(GameObject bot)
    {
        if (isOnCooldown || isBotUsing) return;

        BotController botController = bot.GetComponent<BotController>();
        if (botController != null && !botController.isPlayerControlled)
        {
            Rigidbody2D rb = bot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                isBotUsing = true;
                currentBotUser = bot;

                float duration = infiniteDurationForBots ? Mathf.Infinity : gravityDuration;

                StartCoroutine(ApplyZeroGravityToBot(rb, duration, floatSpeed, botController));
                OnActivated(true);

                Debug.Log($"🚀🤖 Gravedad Cero ACTIVADA para Bot {bot.name} - Duración: {(infiniteDurationForBots ? "INFINITA" : duration + "s")}");
            }
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        // Verificar si es un jugador (Player, Player2, o Bot controlado)
        return obj.CompareTag("Player") ||
               obj.CompareTag("Player2") ||
               obj.CompareTag("Bot") ||
               obj.GetComponent<PlayerController>() != null ||
               obj.GetComponent<BotController>() != null;
    }

    private void ActivateZeroGravity(GameObject player)
    {
        if (isOnCooldown) return;

        // Intentar activar en PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ActivateZeroGravity(gravityDuration, floatSpeed);
            OnActivated(false);
            return;
        }

        // Intentar activar en BotController (si es Player2)
        BotController botController = player.GetComponent<BotController>();
        if (botController != null && botController.isPlayerControlled)
        {
            Rigidbody2D rb = botController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                StartCoroutine(ApplyZeroGravityToBot(rb, gravityDuration, floatSpeed, botController));
                OnActivated(false);
                return;
            }
        }

        Debug.LogWarning("⚠️ No se pudo activar gravedad cero en: " + player.name);
    }

    private void OnActivated(bool isBotActivation)
    {
        Debug.Log($"🚀 ¡Gravedad Cero activada! - Tipo: {(isBotActivation ? "BOT AUTOMÁTICO" : "JUGADOR MANUAL")}");

        // Efectos visuales
        UpdateButtonColor(isBotActivation ? botActiveColor : activeColor);

        // Efectos de sonido
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Iniciar cooldown si está configurado (solo para jugadores humanos)
        if (!isBotActivation)
        {
            if (!canBeUsedMultipleTimes)
            {
                isOnCooldown = true;
                UpdateButtonColor(cooldownColor);
            }
            else if (cooldownTime > 0)
            {
                if (cooldownCoroutine != null)
                {
                    StopCoroutine(cooldownCoroutine);
                }
                cooldownCoroutine = StartCoroutine(CooldownRoutine());
            }
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        UpdateButtonColor(cooldownColor);

        yield return new WaitForSeconds(cooldownTime);

        isOnCooldown = false;
        UpdateButtonColor(normalColor);
        Debug.Log("✅ BotonGravedad listo para usar nuevamente");
    }

    // ✅ MÉTODO MODIFICADO: Gravedad cero para bots con duración infinita
    private IEnumerator ApplyZeroGravityToBot(Rigidbody2D rb, float duration, float floatSpeed, BotController botController)
    {
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Aplicar velocidad de flotación inicial
        rb.velocity = new Vector2(rb.velocity.x, floatSpeed);

        // ✅ NUEVO: Notificar al bot que está en modo flotación
        if (botController != null)
        {
            botController.SetFloatingMode(true, floatSpeed);
        }

        Debug.Log($"🤖 Bot flotando - Gravedad: 0, Velocidad Y: {floatSpeed}");

        if (duration < Mathf.Infinity)
        {
            // Duración limitada (para Player2 controlado por jugador)
            yield return new WaitForSeconds(duration);

            rb.gravityScale = originalGravity;

            if (botController != null)
            {
                botController.SetFloatingMode(false, 0f);
            }

            if (audioSource != null && deactivationSound != null)
            {
                audioSource.PlayOneShot(deactivationSound);
            }

            Debug.Log($"🤖 Gravedad del Bot restaurada a: {originalGravity}");
        }
        else
        {
            // ✅ DURACIÓN INFINITA para bots IA - No restaurar automáticamente
            Debug.Log($"🎯 Bot en flotación INFINITA - Debe restaurarse manualmente al llegar a la meta");

            // Esperar hasta que el bot llegue a la meta o se desactive manualmente
            while (isBotUsing && botController != null && !botController.HasReachedGoal())
            {
                yield return new WaitForSeconds(1f);

                // Mantener velocidad de flotación si es necesario
                if (rb.velocity.y < floatSpeed * 0.5f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, floatSpeed);
                }
            }

            // Restaurar cuando el bot llegue a la meta
            if (botController != null && botController.HasReachedGoal())
            {
                rb.gravityScale = originalGravity;
                botController.SetFloatingMode(false, 0f);
                isBotUsing = false;
                currentBotUser = null;
                Debug.Log($"🎉 Gravedad del Bot restaurada al llegar a la meta");
            }
        }
    }

    // ✅ NUEVO MÉTODO: Para restaurar gravedad manualmente (desde otro script)
    public void RestoreGravityForBot(GameObject bot)
    {
        if (currentBotUser == bot && isBotUsing)
        {
            Rigidbody2D rb = bot.GetComponent<Rigidbody2D>();
            BotController botController = bot.GetComponent<BotController>();

            if (rb != null && botController != null)
            {
                // Restaurar valores normales
                rb.gravityScale = 3f; // Valor por defecto
                botController.SetFloatingMode(false, 0f);

                isBotUsing = false;
                currentBotUser = null;

                Debug.Log($"🔄 Gravedad restaurada manualmente para Bot: {bot.name}");
            }
        }
    }

    // ✅ NUEVO MÉTODO: Verificar si un bot está usando el botón
    public bool IsBotUsingButton(GameObject bot)
    {
        return isBotUsing && currentBotUser == bot;
    }

    private void UpdateButtonColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    void OnGUI()
    {
        if (!showPrompt) return;

        string displayText = "";
        Color textColor = Color.white;

        if (isBotUsing)
        {
            displayText = $"🤖 Bot usando Gravedad Cero";
            textColor = Color.magenta;
        }
        else if (isOnCooldown)
        {
            displayText = "⏳ Botón en enfriamiento...";
            textColor = Color.gray;
        }
        else if (isPlayerNearby)
        {
            displayText = promptText;
            textColor = Color.cyan;
        }
        else
        {
            return;
        }

        // Calcular posición en pantalla
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);

        if (screenPos.z > 0) // Solo mostrar si está frente a la cámara
        {
            // Convertir coordenadas (Unity GUI usa origen arriba-izquierda)
            screenPos.y = Screen.height - screenPos.y;

            // Estilo del texto
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.normal.textColor = textColor;
            style.fontStyle = FontStyle.Bold;

            // Fondo semi-transparente
            Vector2 textSize = style.CalcSize(new GUIContent(displayText));
            Rect bgRect = new Rect(screenPos.x - textSize.x / 2 - 10, screenPos.y - textSize.y / 2 - 5,
                                   textSize.x + 20, textSize.y + 10);
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(bgRect, "");
            GUI.color = Color.white;

            // Texto
            GUI.Label(new Rect(screenPos.x - 150, screenPos.y - 15, 300, 30), displayText, style);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar el rango de activación para jugadores
        Gizmos.color = isOnCooldown ? Color.red : (isBotUsing ? Color.magenta : Color.cyan);
        Gizmos.DrawWireSphere(transform.position, promptDistance);

        // ✅ NUEVO: Visualizar rango de activación para bots
        if (autoActivateForBots)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, botActivationDistance);
        }
    }

    // ✅ NUEVO: Para debug en el editor
    void OnDrawGizmos()
    {
        if (isBotUsing && currentBotUser != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentBotUser.transform.position);
            Gizmos.DrawWireSphere(currentBotUser.transform.position, 1f);
        }
    }
}