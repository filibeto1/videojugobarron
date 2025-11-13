using UnityEngine;

public class EnemigoInteligente : MonoBehaviour
{
    private ControladorTiempo controladorTiempo;

    void Start()
    {
        // Buscar el controlador de tiempo automáticamente
        controladorTiempo = FindObjectOfType<ControladorTiempo>();

        if (controladorTiempo != null)
        {
            // 🔥 CAMBIAR A: AgregarEnemigo (sin "Manual")
            controladorTiempo.AgregarEnemigo(this.gameObject);
            Debug.Log("✅ Enemigo " + name + " registrado en ControladorTiempo");
        }
        else
        {
            Debug.LogError("❌ No se encontró ControladorTiempo en la escena");
        }
    }

    void OnDestroy()
    {
        if (controladorTiempo != null)
        {
            // 🔥 CAMBIAR A: RemoverEnemigo (sin "Manual")
            controladorTiempo.RemoverEnemigo(this.gameObject);
        }
    }

    void Update()
    {
        // Rotación visual para ver que está vivo
        transform.Rotate(0, 30 * Time.deltaTime, 0);
    }
}