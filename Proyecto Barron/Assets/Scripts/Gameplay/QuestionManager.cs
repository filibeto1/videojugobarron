using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject questionPanel;
    public TMP_Text questionText;  // CAMBIADO: Text por TMP_Text
    public Button option1Button;
    public Button option2Button;

    [Header("Configuración de Tiempo")]
    public float tiempoLimite = 6f;

    private GameController gameController;
    private int correctAnswerIndex;
    private Coroutine temporizadorCoroutine;
    private bool preguntaActiva = false;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();

        // Verificar que todos los elementos UI estén asignados
        if (questionPanel == null)
            Debug.LogError("QuestionPanel no está asignado en el Inspector");
        if (questionText == null)
            Debug.LogError("QuestionText no está asignado en el Inspector");
        if (option1Button == null)
            Debug.LogError("Option1Button no está asignado en el Inspector");
        if (option2Button == null)
            Debug.LogError("Option2Button no está asignado en el Inspector");

        if (questionPanel != null)
            questionPanel.SetActive(false);

        // Solo agregar listeners si los botones existen
        if (option1Button != null)
            option1Button.onClick.AddListener(() => CheckAnswer(0));
        if (option2Button != null)
            option2Button.onClick.AddListener(() => CheckAnswer(1));
    }

    public void ShowQuestion()
    {
        // Verificar que todos los elementos necesarios estén presentes
        if (questionPanel == null || questionText == null || option1Button == null || option2Button == null)
        {
            Debug.LogError("Faltan referencias UI en QuestionManager. No se puede mostrar pregunta.");
            return;
        }

        Time.timeScale = 0;
        questionPanel.SetActive(true);
        preguntaActiva = true;

        GenerateRandomQuestion();

        // Iniciar temporizador de 6 segundos
        if (temporizadorCoroutine != null)
            StopCoroutine(temporizadorCoroutine);
        temporizadorCoroutine = StartCoroutine(TemporizadorPregunta());
    }

    IEnumerator TemporizadorPregunta()
    {
        yield return new WaitForSecondsRealtime(tiempoLimite);

        if (preguntaActiva)
        {
            Debug.Log("Tiempo agotado - Pregunta cerrada automáticamente");
            HideQuestion();
        }
    }

    void GenerateRandomQuestion()
    {
        // Verificar que los elementos UI existan antes de usarlos
        if (questionText == null || option1Button == null || option2Button == null)
        {
            Debug.LogError("Elementos UI nulos en GenerateRandomQuestion");
            return;
        }

        int randomNumber = Random.Range(1, 20);
        bool isEven = (randomNumber % 2 == 0);

        questionText.text = "¿El número " + randomNumber + " es par o impar?";

        // CAMBIADO: Usar TMP_Text en lugar de Text
        TMP_Text textBoton1 = option1Button.GetComponentInChildren<TMP_Text>();
        TMP_Text textBoton2 = option2Button.GetComponentInChildren<TMP_Text>();

        if (textBoton1 == null || textBoton2 == null)
        {
            Debug.LogError("No se encontraron componentes TMP_Text en los botones");
            return;
        }

        if (Random.Range(0, 2) == 0)
        {
            textBoton1.text = "PAR";
            textBoton2.text = "IMPAR";
            correctAnswerIndex = isEven ? 0 : 1;
        }
        else
        {
            textBoton1.text = "IMPAR";
            textBoton2.text = "PAR";
            correctAnswerIndex = isEven ? 1 : 0;
        }
    }

    void CheckAnswer(int selectedIndex)
    {
        preguntaActiva = false;
        if (temporizadorCoroutine != null)
            StopCoroutine(temporizadorCoroutine);

        if (selectedIndex == correctAnswerIndex)
        {
            Debug.Log("¡Respuesta correcta!");
            if (gameController != null)
                gameController.AddLife();
        }
        else
        {
            Debug.Log("Respuesta incorrecta");
            if (gameController != null)
                gameController.LoseLife();
        }

        HideQuestion();
    }

    void HideQuestion()
    {
        preguntaActiva = false;

        if (questionPanel != null)
            questionPanel.SetActive(false);

        Time.timeScale = 1;

        if (temporizadorCoroutine != null)
        {
            StopCoroutine(temporizadorCoroutine);
            temporizadorCoroutine = null;
        }
    }
}