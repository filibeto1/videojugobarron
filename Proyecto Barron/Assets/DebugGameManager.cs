using UnityEngine;

public class DebugGameManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 DEBUG GAMEMANAGER - INICIADO");

        if (GameManagerGeneral.Instance == null)
        {
            Debug.LogError("❌ GameManagerGeneral.Instance es NULL");
            return;
        }

        Debug.Log("✅ GameManagerGeneral.Instance encontrado");

        // Verificar PlayerPrefs
        int index = PlayerPrefs.GetInt("JugadorIndex", -999);
        Debug.Log($"🎯 PlayerPrefs JugadorIndex: {index}");

        // Verificar método CrearJugador
        var metodo = GameManagerGeneral.Instance.GetType().GetMethod("CrearJugador");
        if (metodo != null)
        {
            Debug.Log("✅ Método CrearJugador encontrado");
        }
        else
        {
            Debug.Log("❌ Método CrearJugador NO encontrado");
        }

        // Verificar propiedades y campos
        var tipo = GameManagerGeneral.Instance.GetType();

        Debug.Log("📋 BUSCANDO PROPIEDADES:");
        var propiedades = tipo.GetProperties();
        foreach (var prop in propiedades)
        {
            try
            {
                object valor = prop.GetValue(GameManagerGeneral.Instance);
                Debug.Log($"   - {prop.Name}: {valor}");
            }
            catch { }
        }

        Debug.Log("📋 BUSCANDO CAMPOS:");
        var campos = tipo.GetFields();
        foreach (var campo in campos)
        {
            try
            {
                object valor = campo.GetValue(GameManagerGeneral.Instance);
                Debug.Log($"   - {campo.Name}: {valor}");
            }
            catch { }
        }
    }
}