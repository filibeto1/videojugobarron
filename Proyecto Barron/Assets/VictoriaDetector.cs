using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoriaDetector : MonoBehaviour
{
    [Header("Configuración de Victoria")]
    public GameObject panelVictoria;
    public TextMeshProUGUI textoVictoria; // TextMeshPro en lugar de Text normal
    public int numeroJugador = 1;

    [Header("Configuración de Escenas")]
    [Tooltip("Deja vacío para que funcione en todas las escenas, o especifica nombres")]
    public string[] escenasPermitidas = new string[] { "Nivel2", "Nivel2 1" };
    public bool funcionarEnTodasLasEscenas = false;

    void Start()
    {
        // Si está configurado para funcionar en todas las escenas, activar directamente
        if (funcionarEnTodasLasEscenas)
        {
            Debug.Log($"✅ VictoriaDetector activado (modo: todas las escenas)");
            return;
        }

        // Verificar si estamos en alguna de las escenas permitidas
        string escenaActual = SceneManager.GetActiveScene().name;

        if (escenasPermitidas == null || escenasPermitidas.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay escenas permitidas configuradas. VictoriaDetector desactivado.");
            this.enabled = false;
            return;
        }

        bool escenaValida = false;
        foreach (string escena in escenasPermitidas)
        {
            if (escenaActual == escena)
            {
                escenaValida = true;
                break;
            }
        }

        if (escenaValida)
        {
            Debug.Log($"✅ VictoriaDetector activado para: {escenaActual}");
        }
        else
        {
            Debug.Log($"❌ VictoriaDetector desactivado - '{escenaActual}' no está en la lista de escenas permitidas");
            this.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.enabled) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Bot"))
        {
            ProcesarVictoria(collision.gameObject);
        }
    }

    void ProcesarVictoria(GameObject personaje)
    {
        int jugadorGanador = DeterminarJugadorGanador(personaje);
        MostrarVictoria(jugadorGanador);
    }

    int DeterminarJugadorGanador(GameObject personaje)
    {
        if (personaje.CompareTag("Bot")) return 2;
        if (personaje.name.Contains("2") || personaje.name.Contains("Player2")) return 2;
        return 1;
    }

    void MostrarVictoria(int jugadorGanador)
    {
        Time.timeScale = 0f;

        if (textoVictoria != null)
        {
            textoVictoria.text = $"¡Ganó Jugador {jugadorGanador}!";
        }
        else
        {
            Debug.LogError("❌ TextoVictoria (TextMeshPro) no asignado");
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ PanelVictoria no asignado");
        }

        Debug.Log($"🎉 Victoria del Jugador {jugadorGanador} en {SceneManager.GetActiveScene().name}");
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ✅ MÉTODO PÚBLICO - Agregar escena permitida dinámicamente
    public void AgregarEscenaPermitida(string nombreEscena)
    {
        if (escenasPermitidas == null)
        {
            escenasPermitidas = new string[] { nombreEscena };
            return;
        }

        // Verificar si ya existe
        foreach (string escena in escenasPermitidas)
        {
            if (escena == nombreEscena) return;
        }

        // Agregar nueva escena
        string[] nuevaLista = new string[escenasPermitidas.Length + 1];
        escenasPermitidas.CopyTo(nuevaLista, 0);
        nuevaLista[escenasPermitidas.Length] = nombreEscena;
        escenasPermitidas = nuevaLista;

        Debug.Log($"✅ Escena '{nombreEscena}' agregada a la lista de escenas permitidas");
    }

    // 🔧 DIAGNÓSTICO
    [ContextMenu("Diagnóstico VictoriaDetector")]
    void Diagnostico()
    {
        Debug.Log("=== DIAGNÓSTICO VICTORIA DETECTOR ===");
        Debug.Log($"Escena actual: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Funcionar en todas las escenas: {funcionarEnTodasLasEscenas}");
        Debug.Log($"Script habilitado: {this.enabled}");
        Debug.Log($"Panel Victoria asignado: {(panelVictoria != null ? "✅" : "❌")}");
        Debug.Log($"Texto Victoria asignado: {(textoVictoria != null ? "✅" : "❌")}");

        if (escenasPermitidas != null && escenasPermitidas.Length > 0)
        {
            Debug.Log($"Escenas permitidas ({escenasPermitidas.Length}):");
            foreach (string escena in escenasPermitidas)
            {
                Debug.Log($"  - {escena}");
            }
        }
        else
        {
            Debug.Log("⚠️ No hay escenas permitidas configuradas");
        }
        Debug.Log("=====================================");
    }
}