using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionSystem : MonoBehaviour
{
    [System.Serializable]
    public class SecuenciaPregunta
    {
        public string secuencia;
        public int respuestaCorrecta;
        public string tipoPregunta; // "mal", "falta", "sigue"
    }

    [Header("Preguntas")]
    public List<SecuenciaPregunta> preguntas = new List<SecuenciaPregunta>();
    private SecuenciaPregunta preguntaActual;

    [Header("UI References")]
    public GameObject preguntaPanel;
    public TMP_Text preguntaText;
    public TMP_InputField respuestaInput;
    public TMP_Text resultadoText;
    public Button enviarButton;

    private System.Action<bool> onAnswerSubmitted;

    void Start()
    {
        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
            Debug.Log("✅ Panel de preguntas oculto al inicio");
        }
        else
        {
            Debug.LogError("❌ preguntaPanel no está asignado en el Inspector");
        }

        enviarButton.onClick.AddListener(VerificarRespuesta);
        Debug.Log("✅ QuestionSystem inicializado");
    }

    void OnEnable()
    {
        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
        }
    }

    public void MostrarPregunta(System.Action<bool> callback = null)
    {
        onAnswerSubmitted = callback;

        if (preguntas.Count == 0)
        {
            Debug.LogError("❌ No hay preguntas configuradas en QuestionSystem");
            callback?.Invoke(false);
            return;
        }

        if (preguntaPanel == null)
        {
            Debug.LogError("❌ preguntaPanel es null - no se puede mostrar pregunta");
            callback?.Invoke(false);
            return;
        }

        Time.timeScale = 0;
        preguntaActual = preguntas[Random.Range(0, preguntas.Count)];
        preguntaPanel.SetActive(true);

        switch (preguntaActual.tipoPregunta)
        {
            case "mal":
                preguntaText.text = $"¿Qué número está MAL en la secuencia?\n{preguntaActual.secuencia}";
                break;
            case "falta":
                preguntaText.text = $"¿Qué número FALTA en la secuencia?\n{preguntaActual.secuencia}";
                break;
            case "sigue":
                preguntaText.text = $"¿Qué número SIGUE en la secuencia?\n{preguntaActual.secuencia}";
                break;
        }

        respuestaInput.text = "";
        resultadoText.text = "";
        respuestaInput.Select();
        respuestaInput.ActivateInputField();

        Debug.Log("🔵 Pregunta mostrada: " + preguntaActual.secuencia);
    }

    public void VerificarRespuesta()
    {
        if (string.IsNullOrEmpty(respuestaInput.text))
        {
            resultadoText.text = "Ingresa una respuesta";
            resultadoText.color = Color.yellow;
            return;
        }

        int respuestaUsuario;
        if (int.TryParse(respuestaInput.text, out respuestaUsuario))
        {
            if (respuestaUsuario == preguntaActual.respuestaCorrecta)
            {
                resultadoText.text = "¡CORRECTO! Puedes continuar.";
                resultadoText.color = Color.green;
                StartCoroutine(OcultarPregunta(true));
            }
            else
            {
                resultadoText.text = "INCORRECTO. Intenta de nuevo.";
                resultadoText.color = Color.red;
                respuestaInput.text = "";
                respuestaInput.Select();
                respuestaInput.ActivateInputField();
            }
        }
        else
        {
            resultadoText.text = "Ingresa un número válido";
            resultadoText.color = Color.yellow;
        }
    }

    IEnumerator OcultarPregunta(bool correcto)
    {
        yield return new WaitForSecondsRealtime(2f);

        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
        }

        Time.timeScale = 1;
        onAnswerSubmitted?.Invoke(correcto);
    }

    public void CerrarPregunta()
    {
        if (preguntaPanel != null && preguntaPanel.activeInHierarchy)
        {
            StartCoroutine(OcultarPregunta(false));
        }
    }

    void Update()
    {
        if (preguntaPanel != null && preguntaPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CerrarPregunta();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                VerificarRespuesta();
            }
        }
    }

    public void AgregarPregunta(string secuencia, int respuesta, string tipo)
    {
        preguntas.Add(new SecuenciaPregunta
        {
            secuencia = secuencia,
            respuestaCorrecta = respuesta,
            tipoPregunta = tipo
        });
    }

    public bool IsQuestionActive()
    {
        return preguntaPanel != null && preguntaPanel.activeInHierarchy;
    }
}