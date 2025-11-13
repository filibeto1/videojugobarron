using UnityEngine;

public class SensorCamino : MonoBehaviour
{
    [Header("Configuración")]
    public int numeroCamino; // 0, 1 o 2

    void Start()
    {
        Debug.Log($"✅ Sensor {numeroCamino} inicializado en: {gameObject.name}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🎯 Sensor {numeroCamino} activado por: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            Debug.Log($"🚀 Jugador entró en camino {numeroCamino + 1}");

            // Verificar si es la respuesta correcta
            if (PreguntaSistema.Instance != null)
            {
                PreguntaSistema.Instance.VerificarRespuesta(numeroCamino);
            }
            else
            {
                Debug.LogError("❌ PreguntaSistema.Instance es null!");
            }
        }
    }
}