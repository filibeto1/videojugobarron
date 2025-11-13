using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotonFlotante : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelPreguntaFlotante;
    public PreguntaSistemaFlotante sistemaFlotante;
    public SpriteRenderer spriteBoton;
    public Color colorActivado = Color.gray;

    [Header("Configuración")]
    public bool activarAutomaticamente = false;
    public KeyCode teclaActivacion = KeyCode.E;

    [Header("Estado")]
    public bool yaActivado = false;
    public bool jugadorCerca = false;

    private void Update()
    {
        // ✅ Activación manual con tecla
        if (jugadorCerca && !yaActivado && Input.GetKeyDown(teclaActivacion))
        {
            ActivarPregunta();
        }
    }

    public void ActivarPregunta()
    {
        Debug.Log("🎯 BotonFlotante.ActivarPregunta() llamado");

        if (yaActivado)
        {
            Debug.Log("⚠️ Botón ya fue activado antes");
            return;
        }

        // ✅ DESACTIVAR sistema original de forma segura
        PreguntaSistema sistemaOriginal = FindObjectOfType<PreguntaSistema>();
        if (sistemaOriginal != null)
        {
            // Usar solo métodos públicos disponibles
            sistemaOriginal.OcultarPregunta();
            sistemaOriginal.OcultarResultado();
            Debug.Log("🔕 Sistema de preguntas original desactivado");
        }

        if (panelPreguntaFlotante == null)
        {
            Debug.LogError("❌ panelPreguntaFlotante no asignado");
            return;
        }

        // ✅ VERIFICACIÓN EXTREMA DE VISIBILIDAD
        Debug.Log($"🔍 Estado del panel antes: {panelPreguntaFlotante.activeInHierarchy}");

        // ✅ FORZAR configuración correcta del RectTransform
        RectTransform rt = panelPreguntaFlotante.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Cambiar anchors a centro
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Posición y tamaño central
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(900, 500);

            Debug.Log($"📐 RectTransform configurado - Pos: {rt.anchoredPosition}, Size: {rt.sizeDelta}");
        }

        // ✅ FORZAR que esté encima de todo
        panelPreguntaFlotante.transform.SetAsLastSibling();

        // ✅ ACTIVAR panel
        panelPreguntaFlotante.SetActive(true);

        // ✅ VERIFICACIÓN DE COMPONENTES VISUALES
        Image imagenFondo = panelPreguntaFlotante.GetComponent<Image>();
        if (imagenFondo != null)
        {
            Debug.Log($"🎨 Color de fondo: {imagenFondo.color}, Alpha: {imagenFondo.color.a}");
            // Forzar alpha completo si es necesario
            if (imagenFondo.color.a < 0.9f)
            {
                Color colorCorregido = imagenFondo.color;
                colorCorregido.a = 1f;
                imagenFondo.color = colorCorregido;
                Debug.Log("🔧 Alpha forzado a 1");
            }
        }

        // ✅ VERIFICAR que los componentes internos estén activos
        TMP_Text textoPregunta = panelPreguntaFlotante.GetComponentInChildren<TMP_Text>();
        if (textoPregunta != null)
        {
            Debug.Log($"📝 Texto de pregunta: '{textoPregunta.text}', Color: {textoPregunta.color}");
            // Forzar visibilidad del texto
            textoPregunta.color = new Color(textoPregunta.color.r, textoPregunta.color.g, textoPregunta.color.b, 1f);
        }
        else
        {
            Debug.LogError("❌ No se encontró el texto de pregunta");
        }

        // ✅ VERIFICAR BOTONES
        Button[] botones = panelPreguntaFlotante.GetComponentsInChildren<Button>();
        Debug.Log($"🔘 Número de botones encontrados: {botones.Length}");

        foreach (Button boton in botones)
        {
            Image imgBoton = boton.GetComponent<Image>();
            if (imgBoton != null)
            {
                Debug.Log($"🎨 Botón {boton.name} - Color: {imgBoton.color}, Alpha: {imgBoton.color.a}");
                // Forzar visibilidad de botones
                if (imgBoton.color.a < 0.9f)
                {
                    Color colorBoton = imgBoton.color;
                    colorBoton.a = 1f;
                    imgBoton.color = colorBoton;
                }
            }

            // Verificar textos de botones
            TMP_Text textoBoton = boton.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
            {
                textoBoton.color = new Color(textoBoton.color.r, textoBoton.color.g, textoBoton.color.b, 1f);
            }
        }

        // ✅ SISTEMA FLOTANTE
        if (sistemaFlotante != null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                sistemaFlotante.ConfigurarJugador(jugador);
                sistemaFlotante.GenerarPreguntaFlotante();
                Debug.Log("✅ Sistema flotante configurado");
            }
            else
            {
                Debug.LogError("❌ No se encontró el jugador");
            }
        }
        else
        {
            Debug.LogError("❌ Sistema flotante no asignado");
        }

        Debug.Log($"🎉 Panel flotante ACTIVADO - Estado final: {panelPreguntaFlotante.activeInHierarchy}");
    }

    // ✅ DETECCIÓN DE PROXIMIDAD
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !yaActivado)
        {
            jugadorCerca = true;
            Debug.Log("💡 Jugador cerca del botón flotante");

            if (activarAutomaticamente)
            {
                Debug.Log("🚀 Activando AUTOMÁTICAMENTE");
                ActivarPregunta();
            }
            else
            {
                Debug.Log("⌨️ Presiona " + teclaActivacion + " para activar");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            Debug.Log("👋 Jugador se alejó del botón flotante");
        }
    }

    // ✅ MÉTODO QUE FALTABA
    public void OnRespuestaCorrecta()
    {
        yaActivado = true;
        Debug.Log("✅ Botón marcado como usado (desde OnRespuestaCorrecta)");

        // Cambiar color del botón para indicar que ya fue usado
        if (spriteBoton != null)
        {
            spriteBoton.color = colorActivado;
        }

        // Ocultar el panel después de respuesta correcta
        if (panelPreguntaFlotante != null)
        {
            panelPreguntaFlotante.SetActive(false);
            Debug.Log("📋 Panel flotante ocultado después de respuesta correcta");
        }
    }
}