using UnityEngine;
using System.Collections;

public class ArregladorDefinitivo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("💥 ELIMINADOR TOTAL DE BARRERAS ACTIVADO");
        StartCoroutine(EliminacionTotal());
    }

    IEnumerator EliminacionTotal()
    {
        yield return new WaitForSeconds(0.5f);

        // ELIMINAR POR NOMBRE - MÁS AGRESIVO
        EliminarPorNombre();

        // ELIMINAR POR TIPO
        EliminarScriptsProblematicos();

        // ELIMINAR CLONES
        EliminarClones();

        Debug.Log("✅ ELIMINACIÓN TOTAL COMPLETADA");
    }

    void EliminarPorNombre()
    {
        string[] nombresMuerte = {
            "Sensor", "Barrera", "Boton", "Reinicio", "Panel", "Punto",
            "Camino", "Button", "Pregunta", "Botonn"
        };

        GameObject[] todos = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in todos)
        {
            if (obj != null && obj.scene.IsValid())
            {
                foreach (string nombre in nombresMuerte)
                {
                    if (obj.name.Contains(nombre))
                    {
                        Debug.Log($"💀 ELIMINANDO: {obj.name}");
                        Destroy(obj);
                        break;
                    }
                }
            }
        }
    }

    void EliminarScriptsProblematicos()
    {
        // Eliminar scripts que generan barreras
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null)
            {
                string nombreScript = script.GetType().Name;
                if (nombreScript.Contains("Sensor") ||
                    nombreScript.Contains("Barrera") ||
                    nombreScript.Contains("Boton") ||
                    nombreScript.Contains("Reinicio"))
                {
                    Debug.Log($"🛑 ELIMINANDO SCRIPT: {nombreScript} en {script.gameObject.name}");
                    Destroy(script);
                }
            }
        }
    }

    void EliminarClones()
    {
        GameObject[] todos = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in todos)
        {
            if (obj != null && obj.name.Contains("(Clone)"))
            {
                if (obj.name.Contains("Sensor") || obj.name.Contains("Barrera"))
                {
                    Debug.Log($"💀 ELIMINANDO CLON: {obj.name}");
                    Destroy(obj);
                }
            }
        }
    }
}