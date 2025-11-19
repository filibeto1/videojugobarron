using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Nivel3
{
    public class MathQuestionUICreator : MonoBehaviour
    {
        void Start()
        {
            CreateQuestionUI();
        }

        void CreateQuestionUI()
        {
            // Verificar si ya existe la UI
            if (GameObject.Find("QuestionCanvas") != null)
            {
                Debug.Log("✅ La UI de preguntas ya existe (Nivel3)");
                return;
            }

            Debug.Log("🔄 Creando UI de preguntas desde Nivel3...");

            // Crear Canvas
            GameObject canvasGO = new GameObject("QuestionCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Crear Panel
            GameObject panelGO = new GameObject("QuestionPanel");
            panelGO.transform.SetParent(canvasGO.transform);
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Configurar RectTransform del panel
            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.3f);
            panelRT.anchorMax = new Vector2(0.8f, 0.7f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Crear Texto de Pregunta
            GameObject questionGO = new GameObject("QuestionText");
            questionGO.transform.SetParent(panelGO.transform);
            TMP_Text questionText = questionGO.AddComponent<TextMeshProUGUI>();
            questionText.text = "Pregunta de matemáticas";
            questionText.color = Color.white;
            questionText.fontSize = 24;
            questionText.alignment = TextAlignmentOptions.Center;

            RectTransform questionRT = questionGO.GetComponent<RectTransform>();
            questionRT.anchorMin = new Vector2(0.1f, 0.6f);
            questionRT.anchorMax = new Vector2(0.9f, 0.9f);
            questionRT.offsetMin = Vector2.zero;
            questionRT.offsetMax = Vector2.zero;

            // Crear Input Field usando el método correcto
            GameObject inputGO = new GameObject("AnswerInput");
            inputGO.transform.SetParent(panelGO.transform);
            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

            // Configurar RectTransform del input
            RectTransform inputRT = inputGO.GetComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0.2f, 0.3f);
            inputRT.anchorMax = new Vector2(0.8f, 0.5f);
            inputRT.offsetMin = Vector2.zero;
            inputRT.offsetMax = Vector2.zero;

            // Crear GameObject para el texto del placeholder
            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(inputGO.transform);
            TMP_Text placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Escribe tu respuesta...";
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.fontStyle = FontStyles.Italic;

            RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.offsetMin = new Vector2(10, 2);
            placeholderRT.offsetMax = new Vector2(-10, -2);

            // Crear GameObject para el texto de entrada
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform);
            TMP_Text inputText = textGO.AddComponent<TextMeshProUGUI>();
            inputText.color = Color.black;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 2);
            textRT.offsetMax = new Vector2(-10, -2);

            // Configurar el InputField
            inputField.placeholder = placeholderText;
            inputField.textComponent = inputText;

            // Crear Botón
            GameObject buttonGO = new GameObject("SubmitButton");
            buttonGO.transform.SetParent(panelGO.transform);
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.5f, 0.8f);
            Button button = buttonGO.AddComponent<Button>();

            // Configurar RectTransform del botón
            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.3f, 0.1f);
            buttonRT.anchorMax = new Vector2(0.7f, 0.2f);
            buttonRT.offsetMin = Vector2.zero;
            buttonRT.offsetMax = Vector2.zero;

            // Texto del botón
            GameObject buttonTextGO = new GameObject("ButtonText");
            buttonTextGO.transform.SetParent(buttonGO.transform);
            TMP_Text buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = "ENVIAR RESPUESTA";
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            RectTransform buttonTextRT = buttonTextGO.GetComponent<RectTransform>();
            buttonTextRT.anchorMin = Vector2.zero;
            buttonTextRT.anchorMax = Vector2.one;
            buttonTextRT.offsetMin = Vector2.zero;
            buttonTextRT.offsetMax = Vector2.zero;

            // Asignar al MathQuestionManager
            MathQuestionManager questionManager = FindObjectOfType<MathQuestionManager>();
            if (questionManager != null)
            {
                questionManager.questionPanel = panelGO;
                questionManager.questionText = questionText;
                questionManager.answerInput = inputField;
                questionManager.submitButton = button;
                Debug.Log("✅ UI creada y asignada automáticamente a MathQuestionManager (Nivel3)");
            }

            // Desactivar panel inicialmente
            panelGO.SetActive(false);
        }
    }
}