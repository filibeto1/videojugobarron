using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerNivel3 : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Checkpoints")]
    public List<Transform> checkpoints;
    public int currentCheckpoint = 0;

    private Rigidbody2D rb;
    private bool canMove = true;
    private QuestionSystem questionSystem;
    private bool waitingForAnswer = false;
    private bool alreadyAtCheckpoint = false; // ✅ NUEVO: Evitar detección inmediata

    void Start()
    {
        // Esperar un momento para que GameManager configure todo
        StartCoroutine(InitializeAfterDelay());
    }

    IEnumerator InitializeAfterDelay()
    {
        // Esperar 0.2 segundos para que GameManager termine
        yield return new WaitForSeconds(0.2f);
        InitializePlayer();
    }

    void InitializePlayer()
    {
        Debug.Log("🎯 Iniciando PlayerControllerNivel3");

        // Obtener Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("⏳ Rigidbody2D no encontrado inmediatamente, esperando...");
            StartCoroutine(RetryRigidbodySearch());
            return;
        }

        CompleteInitialization();
    }

    IEnumerator RetryRigidbodySearch()
    {
        yield return new WaitForSeconds(0.1f);

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("⚠️ Creando Rigidbody2D de emergencia");
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        CompleteInitialization();
    }

    void CompleteInitialization()
    {
        // Configurar física
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // Buscar QuestionSystem
        questionSystem = FindObjectOfType<QuestionSystem>();
        if (questionSystem == null)
        {
            Debug.LogError("❌ QuestionSystem no encontrado en la escena!");
        }

        // Buscar checkpoints automáticamente
        FindCheckpoints();

        // ✅ CAMBIO: Posicionar en StartPoint en lugar de Checkpoint1
        if (checkpoints.Count > 0)
        {
            // Buscar StartPoint específicamente
            Transform startPoint = FindStartPoint();
            if (startPoint != null)
            {
                transform.position = startPoint.position;
                currentCheckpoint = 1; // Empezar desde Checkpoint1
                Debug.Log($"📍 Posicionado en StartPoint, siguiente checkpoint: {currentCheckpoint}");
            }
            else
            {
                // Fallback: usar el primer checkpoint
                transform.position = checkpoints[0].position;
                currentCheckpoint = 1;
                Debug.Log($"📍 Posicionado en checkpoint 0, siguiente: {currentCheckpoint}");
            }
        }

        canMove = true;
        alreadyAtCheckpoint = false; // ✅ Permitir detección después de inicializar
        Debug.Log("✅ PlayerControllerNivel3 completamente inicializado y listo");
    }

    Transform FindStartPoint()
    {
        foreach (Transform checkpoint in checkpoints)
        {
            if (checkpoint.name == "StartPoint" || checkpoint.name.Contains("Start"))
            {
                return checkpoint;
            }
        }
        return null;
    }

    void FindCheckpoints()
    {
        checkpoints = new List<Transform>();

        GameObject checkpointParent = GameObject.Find("Checkpoints");
        if (checkpointParent != null)
        {
            foreach (Transform child in checkpointParent.transform)
            {
                checkpoints.Add(child);
            }

            // Ordenar checkpoints por nombre
            checkpoints.Sort((a, b) => string.Compare(a.name, b.name));
            Debug.Log($"📌 Total checkpoints encontrados: {checkpoints.Count}");
        }
        else
        {
            Debug.LogError("❌ No se encontró el objeto 'Checkpoints' en la escena");
        }
    }

    void Update()
    {
        if (rb == null) return;

        if (!canMove)
        {
            rb.velocity = Vector2.zero;

            if (Input.GetKeyDown(KeyCode.E) && waitingForAnswer)
            {
                if (questionSystem != null)
                {
                    questionSystem.MostrarPregunta();
                }
                else
                {
                    ContinuarMovimiento();
                }
            }
            return;
        }

        // MOVIMIENTO NORMAL
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb.velocity = movement * speed;

        // ✅ MEJORA: Solo verificar checkpoints si nos estamos moviendo
        if (movement.magnitude > 0.1f && currentCheckpoint < checkpoints.Count && checkpoints[currentCheckpoint] != null)
        {
            float distance = Vector2.Distance(transform.position, checkpoints[currentCheckpoint].position);
            if (distance < 0.5f)
            {
                LlegarCheckpoint();
            }
        }
    }

    void LlegarCheckpoint()
    {
        // ✅ PREVENIR: No activar si ya estamos en un checkpoint
        if (!canMove || waitingForAnswer) return;

        canMove = false;
        waitingForAnswer = true;
        rb.velocity = Vector2.zero;

        Debug.Log($"🎯 Llegó al checkpoint {currentCheckpoint} - {checkpoints[currentCheckpoint].name}");

        if (questionSystem != null)
        {
            questionSystem.MostrarPregunta();
        }
        else
        {
            ContinuarMovimiento();
        }
    }

    public void ContinuarMovimiento()
    {
        canMove = true;
        waitingForAnswer = false;
        currentCheckpoint++;
        Debug.Log($"➡️ Avanzando al checkpoint {currentCheckpoint}");

        if (currentCheckpoint >= checkpoints.Count)
        {
            Debug.Log("🎉 ¡Jugador llegó a la meta!");
            canMove = false;
            rb.velocity = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (rb == null) return;

        if (other.CompareTag("Bot"))
        {
            Debug.Log("🤖 Encuentro con el bot");
            if (questionSystem != null)
            {
                questionSystem.MostrarPregunta();
                canMove = false;
                waitingForAnswer = true;
                rb.velocity = Vector2.zero;
            }
        }
    }
}