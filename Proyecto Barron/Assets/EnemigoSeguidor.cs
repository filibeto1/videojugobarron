using UnityEngine;

public class EnemigoSeguidor : MonoBehaviour
{
    private ControladorTiempo controladorTiempo;

    void Start()
    {
        // Buscar el controlador de tiempo automáticamente
        controladorTiempo = FindObjectOfType<ControladorTiempo>();

        if (controladorTiempo != null)
        {
            controladorTiempo.AgregarEnemigo(this.gameObject);
            Debug.Log("✅ Enemigo " + name + " registrado en ControladorTiempo");
        }
        else
        {
            Debug.LogError("❌ No se encontró ControladorTiempo en la escena");
            // Reintentar después de un tiempo
            Invoke("ReintentarRegistro", 1f);
        }
    }

    void ReintentarRegistro()
    {
        controladorTiempo = FindObjectOfType<ControladorTiempo>();
        if (controladorTiempo != null)
        {
            controladorTiempo.AgregarEnemigo(this.gameObject);
            Debug.Log("✅ Enemigo " + name + " registrado en segundo intento");
        }
    }

    void OnDestroy()
    {
        if (controladorTiempo != null)
        {
            controladorTiempo.RemoverEnemigo(this.gameObject);
        }
    }

    void Update()
    {
        // Rotación visual para ver que está vivo
        transform.Rotate(0, 30 * Time.deltaTime, 0);
    }
}