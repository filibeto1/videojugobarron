using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MathQuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public TMP_Text questionText;
    public TMP_InputField answerInput;
    public Button submitButton;

    [Header("Question Settings")]
    public int currentCheckpoint = 0;

    private int correctAnswer;
    private Checkpoint currentCheckpointObj;

    void Start()
    {
        // Ocultar panel al inicio
        if (questionPanel != null)
            questionPanel.SetActive(false);

        // Configurar botón
        if (submitButton != null)
            submitButton.onClick.AddListener(CheckAnswer);
    }

    // ✅ MÉTODO EXISTENTE: Para mostrar pregunta en checkpoint específico
    public void ShowQuestion(int checkpointNumber)
    {
        currentCheckpoint = checkpointNumber;

        // Generar pregunta matemática simple
        int num1 = Random.Range(1, 10);
        int num2 = Random.Range(1, 10);
        correctAnswer = num1 + num2;

        // Mostrar pregunta
        if (questionText != null)
            questionText.text = $"¿Cuánto es {num1} + {num2}?";

        if (questionPanel != null)
            questionPanel.SetActive(true);

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
        }

        Debug.Log($"❓ Pregunta mostrada: {num1} + {num2} = {correctAnswer}");
    }

    // ✅ MÉTODO AGREGADO: Para compatibilidad con Player2Controller
    public void ShowRandomQuestion()
    {
        // Generar pregunta matemática simple
        int num1 = Random.Range(1, 10);
        int num2 = Random.Range(1, 10);
        correctAnswer = num1 + num2;

        // Mostrar pregunta
        if (questionText != null)
            questionText.text = $"¿Cuánto es {num1} + {num2}?";

        if (questionPanel != null)
            questionPanel.SetActive(true);

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
        }

        Debug.Log($"❓ Pregunta aleatoria mostrada: {num1} + {num2} = {correctAnswer}");
    }

    void CheckAnswer()
    {
        if (int.TryParse(answerInput.text, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                Debug.Log("✅ Respuesta correcta!");
                OnCorrectAnswer();
            }
            else
            {
                Debug.Log("❌ Respuesta incorrecta. Intenta de nuevo.");
                // Puedes agregar lógica para reintentos aquí
            }
        }
        else
        {
            Debug.Log("❌ Ingresa un número válido");
        }
    }

    void OnCorrectAnswer()
    {
        // Ocultar panel
        if (questionPanel != null)
            questionPanel.SetActive(false);

        // Notificar al checkpoint que se completó
        if (currentCheckpointObj != null)
        {
            currentCheckpointObj.Reactivate();
        }

        // Buscar checkpoint actual si no está asignado
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.checkpointNumber == currentCheckpoint)
            {
                checkpoint.Reactivate();
                break;
            }
        }

        Debug.Log($"✅ Checkpoint {currentCheckpoint} completado");
    }

    // Método para asignar checkpoint (opcional)
    public void SetCurrentCheckpoint(Checkpoint checkpoint)
    {
        currentCheckpointObj = checkpoint;
    }

    // ✅ MÉTODO AGREGADO: Para ocultar pregunta (si es necesario)
    public void HideQuestion()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);

        Debug.Log("🚫 Pregunta ocultada");
    }
}