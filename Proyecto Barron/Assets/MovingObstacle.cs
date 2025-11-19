using UnityEngine;
using System.Collections;

public class MovingObstacle : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 3f;
    public float moveDistance = 5f;
    public bool moveHorizontal = true;
    public bool moveVertical = false;

    [Header("Dirección Inicial")]
    public bool startMovingRight = true;
    public bool startMovingUp = true;

    [Header("Tipo de Movimiento")]
    public MovementType movementType = MovementType.PingPong;

    [Header("Movimiento Circular (Opcional)")]
    public bool circularMovement = false;
    public float circleRadius = 3f;
    public float circleSpeed = 2f;

    [Header("Pausa en los Extremos")]
    public bool pauseAtEnds = false;
    public float pauseDuration = 1f;

    [Header("Daño al Jugador")]
    public bool dealsDamage = true;
    public int damageAmount = 1;
    public float damageKnockback = 10f;

    [Header("Efectos Visuales")]
    public bool rotateWhileMoving = false;
    public float rotationSpeed = 50f;

    [Header("Audio (Opcional)")]
    public AudioClip movementSound;
    public AudioClip collisionSound;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentDirection = 1f;
    private bool isPaused = false;
    private float angle = 0f;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    public enum MovementType
    {
        PingPong,      // Va y viene
        Loop,          // Regresa al inicio instantáneamente
        Circular       // Movimiento circular
    }

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configurar dirección inicial
        currentDirection = startMovingRight ? 1f : -1f;

        // Calcular posición objetivo
        CalculateTargetPosition();

        // Configurar componentes necesarios
        SetupComponents();

        Debug.Log($"✅ MovingObstacle inicializado - Posición: {startPosition}, Velocidad: {moveSpeed}");

        // Reproducir sonido de movimiento en loop si existe
        if (audioSource != null && movementSound != null)
        {
            audioSource.clip = movementSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void SetupComponents()
    {
        // Asegurar que tenga Collider2D NO TRIGGER
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = false; // ✅ IMPORTANTE: NO es trigger
            Debug.LogWarning("⚠️ MovingObstacle no tenía Collider2D, se agregó BoxCollider2D");
        }
        else
        {
            col.isTrigger = false; // ✅ Asegurar que NO sea trigger
        }

        // Asegurar que tenga SpriteRenderer para visualización
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                Debug.LogWarning("⚠️ MovingObstacle no tenía SpriteRenderer, se agregó uno");
            }
        }

        // ✅ CRÍTICO: Rigidbody2D Kinematic para colisiones físicas sin gravedad
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Debug.Log($"✅ {gameObject.name} configurado: Kinematic, Sin Gravedad, Collider NO Trigger");
    }

    void CalculateTargetPosition()
    {
        if (moveHorizontal)
        {
            targetPosition = startPosition + Vector3.right * moveDistance * currentDirection;
        }
        else if (moveVertical)
        {
            targetPosition = startPosition + Vector3.up * moveDistance * (startMovingUp ? 1f : -1f);
        }
    }

    void Update()
    {
        if (isPaused) return;

        if (circularMovement)
        {
            MoveInCircle();
        }
        else
        {
            MoveLinear();
        }

        // Rotación opcional
        if (rotateWhileMoving)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    void MoveLinear()
    {
        // ✅ Usar MovePosition para colisiones físicas correctas
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector3 newPosition = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            rb.MovePosition(newPosition);
        }
        else
        {
            // Fallback si no hay Rigidbody
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        // Verificar si llegó al destino
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            OnReachTarget();
        }
    }

    void MoveInCircle()
    {
        angle += circleSpeed * Time.deltaTime;

        float x = startPosition.x + Mathf.Cos(angle) * circleRadius;
        float y = startPosition.y + Mathf.Sin(angle) * circleRadius;

        Vector3 newPosition = new Vector3(x, y, startPosition.z);

        // ✅ Usar MovePosition para colisiones físicas
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }

    void OnReachTarget()
    {
        if (pauseAtEnds)
        {
            StartCoroutine(PauseAtEnd());
        }
        else
        {
            ChangeDirection();
        }
    }

    IEnumerator PauseAtEnd()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
        ChangeDirection();
    }

    void ChangeDirection()
    {
        switch (movementType)
        {
            case MovementType.PingPong:
                // Invertir dirección
                currentDirection *= -1f;
                CalculateTargetPosition();
                break;

            case MovementType.Loop:
                // Regresar al inicio instantáneamente
                transform.position = startPosition;
                CalculateTargetPosition();
                break;

            case MovementType.Circular:
                // No hace nada, el movimiento circular es continuo
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisionó con un jugador
        if (dealsDamage && IsPlayer(collision.gameObject))
        {
            DamagePlayer(collision.gameObject, collision);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ REMOVIDO - Ya no usamos Triggers, solo colisiones físicas
        // Los obstáculos ahora chocan físicamente con los jugadores
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") ||
               obj.CompareTag("Player2") ||
               obj.CompareTag("Bot") ||
               obj.GetComponent<PlayerController>() != null;
    }

    private void DamagePlayer(GameObject player, Collision2D collision)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();

        if (playerController != null && playerController.IsAlive())
        {
            // Aplicar daño
            playerController.TakeDamage(damageAmount);

            // Aplicar knockback
            if (collision != null)
            {
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

                if (playerRb != null)
                {
                    playerRb.velocity = Vector2.zero;
                    playerRb.AddForce(knockbackDirection * damageKnockback, ForceMode2D.Impulse);
                }
            }

            // Reproducir sonido de colisión
            if (audioSource != null && collisionSound != null)
            {
                audioSource.PlayOneShot(collisionSound);
            }

            Debug.Log($"💥 Obstáculo golpeó a {player.name} - Daño: {damageAmount}");
        }
    }

    // Método público para cambiar velocidad en runtime
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    // Método público para pausar/reanudar
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    // Método público para resetear posición
    public void ResetPosition()
    {
        transform.position = startPosition;
        currentDirection = startMovingRight ? 1f : -1f;
        CalculateTargetPosition();
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar el rango de movimiento en el editor
        Vector3 start = Application.isPlaying ? startPosition : transform.position;

        if (circularMovement)
        {
            // Dibujar círculo
            Gizmos.color = Color.yellow;
            DrawCircle(start, circleRadius, 32);
        }
        else
        {
            // Dibujar línea de movimiento
            Gizmos.color = Color.cyan;

            if (moveHorizontal)
            {
                Vector3 end = start + Vector3.right * moveDistance;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 0.2f);
                Gizmos.DrawWireSphere(end, 0.2f);
            }
            else if (moveVertical)
            {
                Vector3 end = start + Vector3.up * moveDistance;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 0.2f);
                Gizmos.DrawWireSphere(end, 0.2f);
            }
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    void OnGUI()
    {
        // Debug info (opcional, puedes comentar esto)
        if (Application.isEditor)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0)
            {
                screenPos.y = Screen.height - screenPos.y;
                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 100, 20),
                    $"Vel: {moveSpeed:F1}");
            }
        }
    }
}