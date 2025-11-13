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

    private PlayerControllerNivel3 playerController;

    void Start()
    {
        // ✅ FORZAR que el panel esté oculto al inicio
        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
            Debug.Log("✅ Panel de preguntas oculto al inicio");
        }
        else
        {
            Debug.LogError("❌ preguntaPanel no está asignado en el Inspector");
        }

        // Buscar player controller
        playerController = FindObjectOfType<PlayerControllerNivel3>();

        // Configurar botón
        enviarButton.onClick.AddListener(VerificarRespuesta);

        Debug.Log("✅ QuestionSystem inicializado");
    }

    void OnEnable()
    {
        // ✅ Asegurar que el panel esté oculto cuando el objeto se active
        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
        }
    }

    public void MostrarPregunta()
    {
        if (preguntas.Count == 0)
        {
            Debug.LogError("❌ No hay preguntas configuradas en QuestionSystem");
            return;
        }

        // ✅ Verificar que el panel esté asignado
        if (preguntaPanel == null)
        {
            Debug.LogError("❌ preguntaPanel es null - no se puede mostrar pregunta");
            return;
        }

        // Seleccionar pregunta aleatoria
        preguntaActual = preguntas[Random.Range(0, preguntas.Count)];
        preguntaPanel.SetActive(true);

        // Configurar texto según tipo
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
                resultadoText.text = "INCORRECTO. Presiona E para intentar de nuevo.";
                resultadoText.color = Color.red;
                // Jugador queda inmovil - debe presionar E
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
        yield return new WaitForSeconds(2f);

        // ✅ Asegurar que el panel no sea null antes de ocultarlo
        if (preguntaPanel != null)
        {
            preguntaPanel.SetActive(false);
        }

        if (correcto && playerController != null)
        {
            playerController.ContinuarMovimiento();
        }
    }
}