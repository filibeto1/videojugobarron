using UnityEngine;
using System.Collections;

public class Player2Controller : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("References")]
    public MathQuestionManager questionManager;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private Transform groundCheck;
    private bool controlEnabled = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Crear punto de verificación de suelo
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(transform);
        groundCheck.localPosition = new Vector3(0, -0.5f, 0);

        // Buscar MathQuestionManager si no está asignado
        if (questionManager == null)
        {
            questionManager = FindObjectOfType<MathQuestionManager>();
        }

        Debug.Log("✅ Player2Controller inicializado");
    }

    void Update()
    {
        if (!controlEnabled) return;

        CheckGrounded();
        HandleMovement();
        HandleJump();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f,
            LayerMask.GetMask("Ground", "Default"));
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (rb != null)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        // Rotar sprite según dirección
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!controlEnabled) return;

        // Manejar checkpoints
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null)
        {
            Debug.Log($"🎯 Player2 alcanzó checkpoint {checkpoint.checkpointNumber}");
            HandleCheckpoint(checkpoint);
        }

        // Manejar meta
        if (other.CompareTag("Finish"))
        {
            Debug.Log("🎉 ¡Player2 ha llegado a la meta!");
            controlEnabled = false;
        }
    }

    void HandleCheckpoint(Checkpoint checkpoint)
    {
        if (questionManager != null)
        {
            // ✅ CORREGIDO: Usar ShowQuestion en lugar de ShowRandomQuestion
            questionManager.ShowQuestion(checkpoint.checkpointNumber);
            DisableControls();
        }
        else
        {
            Debug.LogError("❌ MathQuestionManager no encontrado");
        }
    }

    void DisableControls()
    {
        controlEnabled = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Método para habilitar controles (llamado cuando se responde correctamente)
    public void EnableControls()
    {
        controlEnabled = true;
        Debug.Log("✅ Controles de Player2 habilitados");
    }

    // Método público para verificar estado
    public bool IsControlEnabled()
    {
        return controlEnabled;
    }
}