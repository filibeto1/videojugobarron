using UnityEngine;
using System.Collections;

public class PanelResultadoSigueJugador : MonoBehaviour
{
    [Header("Configuración de Posición")]
    public Vector3 offsetPosicion = new Vector3(0, 2, 0); // Centro-arriba del jugador
    [Tooltip("Velocidad de seguimiento (mayor = más rápido)")]
    public float velocidadSeguimiento = 10f;
    [Tooltip("Tiempo que permanece visible antes de ocultarse")]
    public float tiempoVisible = 3f;

    private Transform jugador;
    private Canvas canvas;
    private Camera camaraJuego;
    private Coroutine coroutinaOcultar;

    void Start()
    {
        // Buscar la cámara principal
        camaraJuego = Camera.main;

        // Verificar si este panel es parte de un Canvas
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                Debug.Log("✅ PanelResultado configurado en World Space");
            }
            else
            {
                Debug.LogWarning("⚠️ PanelResultado no está en World Space. Cambia el Canvas a 'World Space'");
            }
        }

        // Ocultar al inicio
        gameObject.SetActive(false);
    }

    void BuscarJugador()
    {
        if (jugador != null) return; // Ya lo tenemos

        // Buscar al jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");

        if (jugadorObj == null)
        {
            string[] nombresJugador = { "Player(Clone)", "Jugador", "Player", "Personaje" };
            foreach (string nombre in nombresJugador)
            {
                jugadorObj = GameObject.Find(nombre);
                if (jugadorObj != null)
                {
                    Debug.Log($"✅ Jugador encontrado para PanelResultado: {jugadorObj.name}");
                    break;
                }
            }
        }

        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
    }

    void OnEnable()
    {
        // Buscar jugador cada vez que se activa el panel
        BuscarJugador();

        // Cancelar coroutine anterior si existe
        if (coroutinaOcultar != null)
        {
            StopCoroutine(coroutinaOcultar);
        }

        // Iniciar temporizador para ocultar
        coroutinaOcultar = StartCoroutine(OcultarDespuesDeTiempo());
    }

    void LateUpdate()
    {
        if (!gameObject.activeSelf) return;

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

            // Hacer que el panel siempre mire a la cámara
            if (camaraJuego != null)
            {
                transform.LookAt(transform.position + camaraJuego.transform.rotation * Vector3.forward,
                                camaraJuego.transform.rotation * Vector3.up);
            }
        }
    }

    IEnumerator OcultarDespuesDeTiempo()
    {
        Debug.Log($"⏱️ PanelResultado se ocultará en {tiempoVisible} segundos");
        yield return new WaitForSeconds(tiempoVisible);

        gameObject.SetActive(false);
        Debug.Log("✅ PanelResultado ocultado automáticamente");
    }

    // Método público para mostrar el panel manualmente
    public void MostrarPanel()
    {
        gameObject.SetActive(true);
    }

    // Método público para ocultar el panel manualmente
    public void OcultarPanel()
    {
        if (coroutinaOcultar != null)
        {
            StopCoroutine(coroutinaOcultar);
        }
        gameObject.SetActive(false);
    }
}