using UnityEngine;
using TMPro;

public class ActivadorBotonFlotante : MonoBehaviour
{
    [Header("Referencias")]
    public BotonFlotante botonFlotante;
    public GameObject indicadorPresionar;
    public TMP_Text textoIndicador;

    [Header("Configuración")]
    public KeyCode teclaActivacion = KeyCode.E;
    public bool activarAutomaticamente = true; // Cambia a TRUE para auto-activar

    private bool jugadorCerca = false;

    void Start()
    {
        Debug.Log("🔄 ActivadorBotonFlotante iniciado");
        Debug.Log($"🔧 BotonFlotante asignado: {botonFlotante != null}");

        if (indicadorPresionar != null)
            indicadorPresionar.SetActive(false);
    }

    void Update()
    {
        if (!activarAutomaticamente && jugadorCerca && Input.GetKeyDown(teclaActivacion))
        {
            Debug.Log("⌨️ Tecla E presionada - Activando botón");
            ActivarBoton();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            Debug.Log("💡 Jugador cerca del botón flotante");
            Debug.Log($"🔧 Activación automática: {activarAutomaticamente}");

            if (activarAutomaticamente)
            {
                Debug.Log("🚀 Activando AUTOMÁTICAMENTE");
                ActivarBoton();
            }
            else if (indicadorPresionar != null)
            {
                indicadorPresionar.SetActive(true);
                Debug.Log("ℹ️ Mostrando indicador de tecla");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            Debug.Log("👋 Jugador salió del área");

            if (indicadorPresionar != null)
                indicadorPresionar.SetActive(false);
        }
    }

    private void ActivarBoton()
    {
        Debug.Log("🎯 Intentando activar botón...");

        if (botonFlotante == null)
        {
            Debug.LogError("❌ BotonFlotante NO está asignado en el Inspector!");
            return;
        }

        Debug.Log("✅ BotonFlotante encontrado, llamando ActivarPregunta()");
        botonFlotante.ActivarPregunta();

        if (indicadorPresionar != null)
            indicadorPresionar.SetActive(false);
    }
}