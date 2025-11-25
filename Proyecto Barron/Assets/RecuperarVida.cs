using UnityEngine;

public class RecuperarVida : MonoBehaviour
{
    [Header("Configuración")]
    public int vidasARecuperar = 1;

    [Header("Efectos (opcionales)")]
    public GameObject efectoRecoleccion;
    public AudioClip sonidoRecoleccion;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎁 Comodín recolectado por el jugador");

            // Buscar cualquier componente que tenga métodos de vida
            bool vidaRecuperada = BuscarYRecuperarVida(other.gameObject);

            if (vidaRecuperada)
            {
                Debug.Log($"❤️ +{vidasARecuperar} vida(s) recuperada(s)");
            }
            else
            {
                Debug.Log("💡 Comodín recolectado (pero no se encontró sistema de vidas)");
            }

            // Reproducir efectos
            ReproducirEfectos();

            // Destruir el comodín
            Destroy(gameObject);
        }
    }

    private bool BuscarYRecuperarVida(GameObject jugador)
    {
        MonoBehaviour[] scripts = jugador.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            System.Type tipo = script.GetType();

            // Intentar métodos comunes de recuperación de vida
            var metodoRecuperarVida = tipo.GetMethod("RecuperarVida");
            var metodoHeal = tipo.GetMethod("Heal");
            var metodoAddHealth = tipo.GetMethod("AddHealth");
            var metodoAumentarVida = tipo.GetMethod("AumentarVida");

            if (metodoRecuperarVida != null)
            {
                metodoRecuperarVida.Invoke(script, new object[] { vidasARecuperar });
                return true;
            }
            else if (metodoHeal != null)
            {
                metodoHeal.Invoke(script, new object[] { vidasARecuperar });
                return true;
            }
            else if (metodoAddHealth != null)
            {
                metodoAddHealth.Invoke(script, new object[] { vidasARecuperar });
                return true;
            }
            else if (metodoAumentarVida != null)
            {
                metodoAumentarVida.Invoke(script, new object[] { vidasARecuperar });
                return true;
            }
        }

        Debug.LogWarning("⚠️ No se encontró ningún método de recuperación de vida");
        return false;
    }

    private void ReproducirEfectos()
    {
        // Efecto visual
        if (efectoRecoleccion != null)
        {
            Instantiate(efectoRecoleccion, transform.position, Quaternion.identity);
        }

        // Efecto de sonido
        if (sonidoRecoleccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
        }
    }

    // Para debugging en el Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}