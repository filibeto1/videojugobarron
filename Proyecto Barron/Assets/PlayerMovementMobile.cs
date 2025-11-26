using UnityEngine;
using UnityEngine.UI;

// REMUEVE cualquier using JoystickPack si existe
// Si el Joystick Pack tiene namespace, necesitamos descubrir cuál es

public class PlayerMovementMobile : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;

    [Header("Controles Móviles")]
    // PRUEBA ESTAS OPCIONES UNA POR UNA:

    // OPCIÓN 1: Usar el componente base (más compatible)
    public Joystick movementJoystick;

    // OPCIÓN 2: Si la anterior no funciona, prueba con:
    // public VariableJoystick movementJoystick;

    // OPCIÓN 3: O incluso simplemente:
    // public Component movementJoystick;

    public bool useMobileControls = true;

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

        // Detectar plataforma
#if UNITY_ANDROID || UNITY_IOS
            useMobileControls = true;
            Debug.Log("📱 Modo MÓVIL activado");
#else
        useMobileControls = false;
        Debug.Log("🖥️ Modo PC activado");
#endif

        // BUSCAR JOYSTICK AUTOMÁTICAMENTE si no está asignado
        if (movementJoystick == null)
        {
            FindJoystickAutomatically();
        }

        if (movementJoystick != null)
        {
            Debug.Log($"✅ Joystick asignado: {movementJoystick.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ No se pudo encontrar ningún joystick");
        }
    }

    void FindJoystickAutomatically()
    {
        Debug.Log("🔍 Buscando joystick automáticamente...");

        // Buscar cualquier tipo de joystick
        Joystick joystick = FindObjectOfType<Joystick>();
        if (joystick != null)
        {
            movementJoystick = joystick;
            Debug.Log($"✅ Encontrado Joystick: {joystick.gameObject.name}");
            return;
        }

        // Si no encuentra Joystick, buscar FloatingJoystick específicamente
        MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour component in allComponents)
        {
            if (component.GetType().Name.Contains("Joystick"))
            {
                movementJoystick = component as Joystick;
                Debug.Log($"✅ Encontrado {component.GetType().Name}: {component.gameObject.name}");
                break;
            }
        }
    }

    void Update()
    {
        if (!controlsEnabled) return;

        // INPUT PARA MÓVIL Y PC
        if (useMobileControls && movementJoystick != null)
        {
            // Usar joystick para móvil
            movement.x = movementJoystick.Horizontal;
            movement.y = movementJoystick.Vertical;
        }
        else
        {
            // Usar teclado para PC (backup)
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        movement = movement.normalized;
        UpdateDirection();
    }

    void FixedUpdate()
    {
        if (!controlsEnabled) return;

        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateDirection()
    {
        if (movement.magnitude > 0.1f && Mathf.Abs(movement.x) > 0.1f)
        {
            spriteRenderer.flipX = movement.x < 0;
        }

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
        if (!enabled && rb != null)
        {
            rb.velocity = Vector2.zero;
            movement = Vector2.zero;
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
        Debug.Log($"🎮 Controles {(enabled ? "activados" : "desactivados")}");
    }
}