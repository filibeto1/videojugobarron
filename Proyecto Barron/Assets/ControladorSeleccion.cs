using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ControladorSeleccion : MonoBehaviour
{
    public Image personajeImagen;
    public TextMeshProUGUI textoNombre;

    private int indiceActual = 0;

    void Start()
    {
        Debug.Log("🎮 ControladorSeleccion Iniciando...");

        // ✅ CORREGIDO: Sincronización más robusta
        if (GameManager.Instance != null)
        {
            // Primero cargar del GameManager
            indiceActual = GameManager.Instance.GetJugadorSeleccionado();
            Debug.Log($"🔄 Índice cargado del GameManager: {indiceActual}");

            // ✅ NUEVO: Sincronizar inmediatamente con PlayerPrefs
            PlayerPrefs.SetInt("JugadorSeleccionado", indiceActual);
            PlayerPrefs.Save();
        }
        else
        {
            // Fallback a PlayerPrefs si no hay GameManager
            indiceActual = PlayerPrefs.GetInt("JugadorSeleccionado", 0);
            Debug.Log($"🔄 Índice cargado de PlayerPrefs: {indiceActual}");
        }

        ActualizarPersonaje();

        // VERIFICAR estado actual
        Debug.Log($"🔍 Estado inicial - PlayerPrefs: {PlayerPrefs.GetInt("JugadorSeleccionado", -999)}, UI: {indiceActual}");
    }

    public void SiguientePersonaje()
    {
        Debug.Log("🔄 Botón SIGUIENTE presionado");

        indiceActual++;
        if (indiceActual >= GameManager.Instance.personajes.Count)
        {
            indiceActual = 0;
        }

        Debug.Log($"➡️ Cambiando a índice: {indiceActual}");
        GuardarSeleccionInmediata();
        ActualizarPersonaje();
    }

    public void PersonajeAnterior()
    {
        Debug.Log("🔄 Botón ANTERIOR presionado");

        indiceActual--;
        if (indiceActual < 0)
        {
            indiceActual = GameManager.Instance.personajes.Count - 1;
        }

        Debug.Log($"⬅️ Cambiando a índice: {indiceActual}");
        GuardarSeleccionInmediata();
        ActualizarPersonaje();
    }

    // ✅ MEJORADO: Guardar INMEDIATAMENTE y sincronizar
    private void GuardarSeleccionInmediata()
    {
        if (GameManager.Instance != null && GameManager.Instance.personajes != null &&
            indiceActual >= 0 && indiceActual < GameManager.Instance.personajes.Count)
        {
            var personaje = GameManager.Instance.personajes[indiceActual];

            // 1. Actualizar GameManager PRIMERO
            GameManager.Instance.SeleccionarPersonaje(indiceActual);

            // 2. Guardar en PlayerPrefs (SOLO UNA CLAVE)
            PlayerPrefs.SetInt("JugadorSeleccionado", indiceActual);

            // 3. FORZAR GUARDADO
            PlayerPrefs.Save();

            Debug.Log($"💾 GUARDADO INMEDIATO: {personaje.nombre} (Índice: {indiceActual})");

            // 4. VERIFICACIÓN EXTRA
            int guardadoGM = GameManager.Instance.GetJugadorSeleccionado();
            int guardadoPP = PlayerPrefs.GetInt("JugadorSeleccionado", -1);
            Debug.Log($"🔍 VERIFICACIÓN: GameManager={guardadoGM}, PlayerPrefs={guardadoPP}");
        }
        else
        {
            Debug.LogError("❌ No se puede guardar selección - Datos inválidos");
        }
    }

    public void Jugar()
    {
        Debug.Log("🎮 Botón Jugar presionado...");

        // ✅ CONFIRMAR selección antes de jugar
        GuardarSeleccionInmediata();

        // ✅ NUEVA VERIFICACIÓN EXTRA
        if (GameManager.Instance != null)
        {
            Personaje personajeFinal = GameManager.Instance.GetPersonajeSeleccionado();
            Debug.Log($"🎯 PERSONAJE FINAL CONFIRMADO: {personajeFinal.nombre} (Índice: {indiceActual})");

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

        SceneManager.LoadScene("SampleScene");
    }

    private void ActualizarPersonaje()
    {
        if (GameManager.Instance == null || GameManager.Instance.personajes == null ||
            GameManager.Instance.personajes.Count == 0)
        {
            Debug.LogError("❌ No hay personajes disponibles");
            return;
        }

        if (indiceActual < 0 || indiceActual >= GameManager.Instance.personajes.Count)
        {
            Debug.LogError($"❌ Índice inválido: {indiceActual}");
            indiceActual = 0;
        }

        var personaje = GameManager.Instance.personajes[indiceActual];

        // Actualizar UI
        if (personajeImagen != null && personaje.imagen != null)
        {
            personajeImagen.sprite = personaje.imagen;
        }

        if (textoNombre != null)
        {
            textoNombre.text = personaje.nombre;
        }

        Debug.Log($"🔄 UI Actualizada: {personaje.nombre} (Índice: {indiceActual})");

        // MOSTRAR PREFAB ASIGNADO
        if (personaje.personajeJugable != null)
        {
            Debug.Log($"📁 Prefab listo: {personaje.personajeJugable.name}");
        }
        else
        {
            Debug.LogError($"❌ NO HAY PREFAB para {personaje.nombre}");
        }
    }

    [ContextMenu("🔍 Debug Estado Selección")]
    public void DebugEstadoSeleccion()
    {
        Debug.Log("=== DEBUG SELECCIÓN ===");
        Debug.Log($"Índice UI: {indiceActual}");

        if (GameManager.Instance != null && GameManager.Instance.personajes != null &&
            indiceActual >= 0 && indiceActual < GameManager.Instance.personajes.Count)
        {
            Debug.Log($"Personaje UI: {GameManager.Instance.personajes[indiceActual].nombre}");
        }

        Debug.Log($"PlayerPrefs JugadorSeleccionado: {PlayerPrefs.GetInt("JugadorSeleccionado", -999)}");

        if (GameManager.Instance != null)
        {
            Debug.Log($"GameManager: {GameManager.Instance.GetJugadorSeleccionado()}");
            Personaje seleccionado = GameManager.Instance.GetPersonajeSeleccionado();
            if (seleccionado != null)
            {
                Debug.Log($"Personaje GameManager: {seleccionado.nombre}");
            }
        }
        Debug.Log("=== FIN DEBUG ===");
    }
}