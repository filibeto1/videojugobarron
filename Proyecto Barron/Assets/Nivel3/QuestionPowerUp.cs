using UnityEngine;

public class QuestionPowerUp : MonoBehaviour
{
    private MonoBehaviour questionSystem; // NUEVO: Sistema compatible

    void Start()
    {
        // Buscar sistema de preguntas de forma compatible
        questionSystem = FindObjectOfType<MonoBehaviour>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Mostrar pregunta usando sistema compatible
            if (questionSystem != null)
            {
                var showMethod = questionSystem.GetType().GetMethod("ShowQuestion");
                if (showMethod != null)
                {
                    showMethod.Invoke(questionSystem, null);

                    // Desactivar el power-up después de usarlo
                    gameObject.SetActive(false);
                }
            }
        }
    }
}