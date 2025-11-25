using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;

    private bool controlsEnabled = true;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("❌ Rigidbody2D no encontrado en " + gameObject.name);
        }

        Debug.Log($"✅ PlayerMovement inicializado para: {gameObject.name}");
    }

    void Update()
    {
        if (!controlsEnabled) return;

        // Input de movimiento
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalizar el vector para movimiento diagonal
        movement = movement.normalized;

        // 🔄 ACTUALIZAR ROTACIÓN/ANIMACIÓN SEGÚN DIRECCIÓN
        UpdateDirection();
    }

    void FixedUpdate()
    {
        if (!controlsEnabled) return;

        // Movimiento físico
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateDirection()
    {
        // Si hay movimiento, actualizar la dirección
        if (movement.magnitude > 0.1f)
        {
            // Para movimiento horizontal - voltear sprite
            if (Mathf.Abs(movement.x) > 0.1f)
            {
                // Si se mueve a la derecha, sprite normal
                // Si se mueve a la izquierda, voltear sprite
                spriteRenderer.flipX = movement.x < 0;
            }

            // Opcional: También puedes rotar el GameObject si es 3D
            // transform.rotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.y));

            Debug.Log($"🎯 Movimiento: {movement}, FlipX: {spriteRenderer.flipX}");
        }

        // 🔄 Actualizar parámetros del Animator
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.magnitude);
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (rb != null)
        {
            if (!enabled)
            {
                // Detener el movimiento cuando se desactivan los controles
                rb.velocity = Vector2.zero;
                movement = Vector2.zero;

                // Detener animación
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
            }
        }

        Debug.Log($"🎮 Controles {(enabled ? "activados" : "desactivados")} para: {gameObject.name}");
    }

    public bool IsControlsEnabled()
    {
        return controlsEnabled;
    }
}