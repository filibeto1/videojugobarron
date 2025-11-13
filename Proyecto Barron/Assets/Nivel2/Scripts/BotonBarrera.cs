using UnityEngine;

public class BotonBarrera : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelPregunta;
    public PreguntaSistema preguntaSistema;

    [Header("Configuración por Conjunto")]
    public int indicePuntoReinicio = 0;

    [Header("Configuración Visual")]
    public Color colorActivado = Color.green;

    private bool activado = false;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color;
        }

        Debug.Log($"=== CONFIGURACIÓN BOTÓN {indicePuntoReinicio} ===");
        Debug.Log($"Panel Pregunta: {panelPregunta != null}");
        Debug.Log($"PreguntaSistema: {preguntaSistema != null}");
        Debug.Log($"Índice Reinicio: {indicePuntoReinicio}");

        if (panelPregunta != null)
        {
            panelPregunta.SetActive(false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🔘 Botón {indicePuntoReinicio} tocado por: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Player") && !activado)
        {
            Debug.Log($"🎮 ¡Botón {indicePuntoReinicio} activado!");
            activado = true;

            if (spriteRenderer != null)
                spriteRenderer.color = colorActivado;

            if (preguntaSistema != null)
            {
                Debug.Log($"🔍 Configurando punto reinicio a índice: {indicePuntoReinicio}");

                // ✅ DEBUG ESPECÍFICO PARA VER QUÉ ÍNDICE SE ESTÁ ENVIANDO
                preguntaSistema.ConfigurarPuntoReinicio(indicePuntoReinicio);
                preguntaSistema.ForzarBusquedaJugador();

                if (panelPregunta != null)
                {
                    panelPregunta.SetActive(true);
                    Debug.Log("📋 Panel de pregunta activado");
                }

                preguntaSistema.GenerarNuevaPregunta();
                Debug.Log("❓ Nueva pregunta generada");
            }
            else
            {
                Debug.LogError("❌ PreguntaSistema no asignado!");
            }
        }
    }
}