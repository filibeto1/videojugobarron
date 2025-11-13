using UnityEngine;

public class PanelSigueJugador : MonoBehaviour
{
    [Header("Configuración de Posición")]
    public Vector3 offsetPosicion = new Vector3(5, 0, 0); // ✅ Default: A la derecha
    [Tooltip("Velocidad de seguimiento (mayor = más rápido)")]
    public float velocidadSeguimiento = 8f;
    [Tooltip("Hacer que el panel siempre mire a la cámara")]
    public bool mirarAlJugador = false;

    private Transform jugador;
    private Canvas canvas;
    private Camera camaraJuego;

    void Start()
    {
        // Buscar la cámara principal
        camaraJuego = Camera.main;

        // Verificar si este panel es parte de un Canvas
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            // Si es Canvas World Space, configurarlo correctamente
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                Debug.Log("✅ Panel configurado en World Space");
            }
            else
            {
                Debug.LogWarning("⚠️ Panel no está en World Space. Cambia el Canvas a 'World Space' para mejor seguimiento");
            }
        }

        BuscarJugador();
    }

    void BuscarJugador()
    {
        // Buscar al jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");

        if (jugadorObj == null)
        {
            // Intentar con nombres comunes
            string[] nombresJugador = { "Player(Clone)", "Jugador", "Player", "Personaje" };
            foreach (string nombre in nombresJugador)
            {
                jugadorObj = GameObject.Find(nombre);
                if (jugadorObj != null)
                {
                    Debug.Log($"✅ Jugador encontrado por nombre: {jugadorObj.name}");
                    break;
                }
            }
        }
        else
        {
            Debug.Log($"✅ Jugador encontrado por tag: {jugadorObj.name}");
        }

        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró el jugador. El panel intentará buscarlo continuamente.");
        }
    }

    void LateUpdate()
    {
        // Si el panel está activo y visible
        if (!gameObject.activeSelf)
            return;

        // Si no tenemos jugador, intentar buscarlo
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        // Seguir al jugador con offset
        Vector3 nuevaPosicion = jugador.position + offsetPosicion;

        // Si es World Space, actualizar posición directamente
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = Vector3.Lerp(transform.position, nuevaPosicion, Time.deltaTime * velocidadSeguimiento);

            // Opcional: hacer que el panel siempre mire a la cámara
            if (mirarAlJugador && camaraJuego != null)
            {
                transform.LookAt(transform.position + camaraJuego.transform.rotation * Vector3.forward,
                                camaraJuego.transform.rotation * Vector3.up);
            }
        }
    }

    // Método público para forzar la búsqueda del jugador
    public void ActualizarJugador()
    {
        BuscarJugador();
    }
}