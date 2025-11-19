using UnityEngine;

public class PlayerDestroyDetector : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 PlayerDestroyDetector iniciado - Buscando Player1...");

        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null)
        {
            Debug.Log("✅ Player1 encontrado: " + player1.name);

            // Agregar detector de destrucción
            PlayerController playerController = player1.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Crear un componente temporal para detectar destrucción
                player1.AddComponent<DestroyWatcher>();
            }
        }
        else
        {
            Debug.LogError("❌ Player1 NO encontrado al inicio");
        }
    }
}

public class DestroyWatcher : MonoBehaviour
{
    void OnDestroy()
    {
        Debug.LogError("🚨 ¡PLAYER1 DESTRUIDO! Stack Trace:");
        Debug.LogError(System.Environment.StackTrace);

        // Buscar qué objeto causó la destrucción
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Destroy") || obj.name.Contains("Cleanup") || obj.name.Contains("Manager"))
            {
                Debug.LogError("⚠️ Objeto sospechoso: " + obj.name + " - " + obj.GetType());
            }
        }
    }
}