using UnityEngine;

public class SistemaLimpieza : MonoBehaviour
{
    [ContextMenu("🧹 LIMPIAR TODO Y FORZAR MIGUEL")]
    public void LimpiarTodoYForzarMiguel()
    {
        // 1. Limpiar PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Forzar Miguel
        PlayerPrefs.SetInt("JugadorSeleccionado", 0);
        PlayerPrefs.SetInt("JugadorIndex", 0);
        PlayerPrefs.Save();

        // 3. Destruir todos los jugadores
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject jugador in jugadores)
        {
            DestroyImmediate(jugador);
        }

        // 4. Destruir PlayerScenePersister
        PlayerScenePersister persister = FindObjectOfType<PlayerScenePersister>();
        if (persister != null) DestroyImmediate(persister.gameObject);

        Debug.Log("✅ SISTEMA LIMPIO - Miguel forzado (índice 0)");
    }

    [ContextMenu("🔍 VER ESTADO ACTUAL")]
    public void VerEstadoActual()
    {
        Debug.Log("=== ESTADO ACTUAL ===");
        Debug.Log($"JugadorSeleccionado: {PlayerPrefs.GetInt("JugadorSeleccionado", -999)}");
        Debug.Log($"JugadorIndex: {PlayerPrefs.GetInt("JugadorIndex", -999)}");

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Jugador en escena: {(jugador != null ? jugador.name : "NO HAY")}");
        Debug.Log("=== FIN ESTADO ===");
    }
}