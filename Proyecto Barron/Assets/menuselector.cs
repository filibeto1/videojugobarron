using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuSelector : MonoBehaviour
{
    private int index;

    [SerializeField] private Image imagen;
    [SerializeField] private TextMeshProUGUI nombre;

    private GameManager gameManager;

    private void Start()
    {
        Debug.Log("🎮 MenuSelector Iniciando...");

        gameManager = GameManager.Instance;

        // ✅ MEJORADO: Cargar de múltiples fuentes posibles
        if (PlayerPrefs.HasKey("JugadorSeleccionado"))
        {
            index = PlayerPrefs.GetInt("JugadorSeleccionado", 0);
            Debug.Log($"🔄 Índice cargado de JugadorSeleccionado: {index}");
        }
        else if (PlayerPrefs.HasKey("JugadorIndex"))
        {
            index = PlayerPrefs.GetInt("JugadorIndex", 0);
            Debug.Log($"🔄 Índice cargado de JugadorIndex: {index}");
        }
        else
        {
            index = 0;
            Debug.Log("🔄 Usando índice por defecto: 0");
        }

        // ✅ MEJORADO: Verificación más robusta del índice
        if (gameManager.personajes == null || gameManager.personajes.Count == 0)
        {
            Debug.LogError("❌ No hay personajes en el GameManager");
            return;
        }

        if (index >= gameManager.personajes.Count)
        {
            Debug.LogWarning($"⚠️ Índice inválido: {index}, forzando a 0");
            index = 0;
        }

        // ✅ NUEVO: Sincronizar con GameManager
        if (gameManager != null)
        {
            gameManager.SeleccionarPersonaje(index);
        }

        // Inicializa la pantalla al comenzar.
        CambiarPantalla();

        // ✅ NUEVO: Verificación final
        Debug.Log($"🔍 Estado inicial - PlayerPrefs: {PlayerPrefs.GetInt("JugadorSeleccionado", -999)}, UI: {index}");
    }

    private void CambiarPantalla()
    {
        // Asegúrate de que el índice esté dentro del rango.
        if (index >= 0 && index < gameManager.personajes.Count)
        {
            // ✅ MEJORADO: Guardar INMEDIATAMENTE al cambiar
            GuardarSeleccionInmediata();

            imagen.sprite = gameManager.personajes[index].imagen;
            nombre.text = gameManager.personajes[index].nombre;

            Debug.Log($"🔄 UI Actualizada: {gameManager.personajes[index].nombre} (Índice: {index})");

            // ✅ NUEVO: Mostrar prefab asignado
            if (gameManager.personajes[index].personajeJugable != null)
            {
                Debug.Log($"📁 Prefab listo: {gameManager.personajes[index].personajeJugable.name}");
            }
            else
            {
                Debug.LogError($"❌ NO HAY PREFAB para {gameManager.personajes[index].nombre}");
            }
        }
        else
        {
            Debug.LogWarning("Índice fuera de rango: " + index);
        }
    }

    // ✅ NUEVO MÉTODO: Guardar selección inmediatamente
    private void GuardarSeleccionInmediata()
    {
        if (gameManager != null && gameManager.personajes != null &&
            index >= 0 && index < gameManager.personajes.Count)
        {
            var personaje = gameManager.personajes[index];

            // 1. Actualizar GameManager
            gameManager.SeleccionarPersonaje(index);

            // 2. Guardar en PlayerPrefs (MÚLTIPLES CLAVES)
            PlayerPrefs.SetInt("JugadorSeleccionado", index);
            PlayerPrefs.SetInt("JugadorIndex", index);
            PlayerPrefs.SetString("UltimaSeleccion", personaje.nombre);

            // 3. FORZAR GUARDADO
            PlayerPrefs.Save();

            Debug.Log($"💾 GUARDADO INMEDIATO: {personaje.nombre} (Índice: {index})");

            // 4. VERIFICACIÓN
            int guardadoGM = gameManager.GetJugadorSeleccionado();
            int guardadoPP = PlayerPrefs.GetInt("JugadorSeleccionado", -1);
            Debug.Log($"🔍 VERIFICACIÓN: GameManager={guardadoGM}, PlayerPrefs={guardadoPP}");
        }
        else
        {
            Debug.LogError("❌ No se puede guardar selección - Datos inválidos");
        }
    }

    public void SiguientePersonaje()
    {
        Debug.Log("🔄 Botón SIGUIENTE presionado");

        if (index == gameManager.personajes.Count - 1)
        {
            index = 0; // Volver al primer personaje
        }
        else
        {
            index += 1; // Avanzar al siguiente personaje
        }

        Debug.Log($"➡️ Cambiando a índice: {index}");
        CambiarPantalla();
    }

    public void AnteriorPersonaje()
    {
        Debug.Log("🔄 Botón ANTERIOR presionado");

        if (index == 0)
        {
            index = gameManager.personajes.Count - 1; // Volver al último personaje
        }
        else
        {
            index -= 1; // Retroceder al personaje anterior
        }

        Debug.Log($"⬅️ Cambiando a índice: {index}");
        CambiarPantalla();
    }

    public void IniciarJuego()
    {
        Debug.Log("🎮 Botón Jugar presionado...");

        // ✅ MEJORADO: Confirmar selección antes de jugar
        GuardarSeleccionInmediata();

        // ✅ NUEVO: Verificación extra del personaje final
        if (gameManager != null)
        {
            Personaje personajeFinal = gameManager.GetPersonajeSeleccionado();
            Debug.Log($"🎯 PERSONAJE FINAL CONFIRMADO: {personajeFinal.nombre} (Índice: {index})");

            if (personajeFinal.personajeJugable != null)
            {
                Debug.Log($"📁 PREFAB ASIGNADO: {personajeFinal.personajeJugable.name}");
            }
            else
            {
                Debug.LogError($"❌ ERROR: No hay prefab para {personajeFinal.nombre}");
            }
        }

        // VERIFICACIÓN FINAL
        int seleccionFinal = PlayerPrefs.GetInt("JugadorSeleccionado", -1);
        Debug.Log($"✅ CONFIRMACIÓN FINAL: Índice {seleccionFinal} - Cargando escena...");

        // Cargar la siguiente escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ✅ NUEVO: Método para debugging
    [ContextMenu("🔍 Debug Estado Selección")]
    public void DebugEstadoSeleccion()
    {
        Debug.Log("=== DEBUG SELECCIÓN ===");
        Debug.Log($"Índice UI: {index}");

        if (gameManager != null && gameManager.personajes != null &&
            index >= 0 && index < gameManager.personajes.Count)
        {
            Debug.Log($"Personaje UI: {gameManager.personajes[index].nombre}");
        }

        Debug.Log($"PlayerPrefs JugadorSeleccionado: {PlayerPrefs.GetInt("JugadorSeleccionado", -999)}");
        Debug.Log($"PlayerPrefs JugadorIndex: {PlayerPrefs.GetInt("JugadorIndex", -999)}");

        if (gameManager != null)
        {
            Debug.Log($"GameManager: {gameManager.GetJugadorSeleccionado()}");
            Personaje seleccionado = gameManager.GetPersonajeSeleccionado();
            if (seleccionado != null)
            {
                Debug.Log($"Personaje GameManager: {seleccionado.nombre}");
            }
        }
        Debug.Log("=== FIN DEBUG ===");
    }

    // ✅ NUEVO: Método para testing rápido
    [ContextMenu("🔄 Forzar Selección Dog (Índice 1)")]
    public void ForzarSeleccionDog()
    {
        index = 1; // Asumiendo que Dog es índice 1
        CambiarPantalla();
        Debug.Log($"🎯 SELECCIÓN FORZADA: Dog (Índice: {index})");
    }
}