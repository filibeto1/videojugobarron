using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    public float movementSpeed = 2f;
    public float waitTimePerCheckpoint = 5f;

    [Header("Checkpoints")]
    public List<Transform> checkpoints;
    public int currentCheckpoint = 1; // Empieza después del spawn

    private Rigidbody2D rb;
    private bool isMoving = true;
    private QuestionSystem questionSystem;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        questionSystem = FindObjectOfType<QuestionSystem>();

        // Buscar checkpoints automáticamente
        FindCheckpoints();
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

            // Ordenar checkpoints
            checkpoints.Sort((a, b) => string.Compare(a.name, b.name));
            Debug.Log($"🤖 Bot: Encontrados {checkpoints.Count} checkpoints");
        }
    }

    void Update()
    {
        if (!isMoving) return;

        // Moverse hacia el siguiente checkpoint
        if (currentCheckpoint < checkpoints.Count && checkpoints[currentCheckpoint] != null)
        {
            Vector2 targetPosition = checkpoints[currentCheckpoint].position;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

            if (rb != null)
                rb.velocity = direction * movementSpeed;

            // Verificar si llegó al checkpoint
            float distance = Vector2.Distance(transform.position, targetPosition);
            if (distance < 0.3f)
            {
                StartCoroutine(ProcesarCheckpoint());
            }
        }
    }

    IEnumerator ProcesarCheckpoint()
    {
        isMoving = false;
        if (rb != null)
            rb.velocity = Vector2.zero;

        Debug.Log($"🤖 Bot en checkpoint {currentCheckpoint}");

        // Esperar 5 segundos (simulando respuesta correcta)
        yield return new WaitForSeconds(waitTimePerCheckpoint);

        AvanzarCheckpoint();
        isMoving = true;
    }

    void AvanzarCheckpoint()
    {
        currentCheckpoint++;
        if (currentCheckpoint >= checkpoints.Count)
        {
            Debug.Log("🎉 ¡Bot llegó a la meta!");
            isMoving = false;
            if (rb != null)
                rb.velocity = Vector2.zero;
        }
    }
}