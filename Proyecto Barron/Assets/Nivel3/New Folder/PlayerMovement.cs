using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;

    private bool controlsEnabled = true;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("? Rigidbody2D no encontrado en " + gameObject.name);
        }

        Debug.Log($"?? PlayerMovement inicializado para: {gameObject.name}");
    }

    void Update()
    {
        if (!controlsEnabled) return;

        // Input de movimiento
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalizar el vector para movimiento diagonal
        movement = movement.normalized;
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
            }
        }

        Debug.Log($"?? Controles {(enabled ? "activados" : "desactivados")} para: {gameObject.name}");
    }

    public bool IsControlsEnabled()
    {
        return controlsEnabled;
    }
}