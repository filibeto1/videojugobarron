using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ControladorTiempo : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    public Slider barraTiempo;
    public float tiempoTotal = 60f;
    private float tiempoRestante;
    private bool tiempoActivo = true;
    private float tiempoVisual; // 🆕 NUEVA VARIABLE para control visual

    [Header("Game Over")]
    public GameObject panelGameOver;

    [Header("Sistema de Enemigos")]
    public float distanciaDeteccion = 5f;
    public float velocidadEnemigos = 5f;
    public float tiempoPenalizacion = 5f;
    private GameObject jugador;
    private List<GameObject> enemigos = new List<GameObject>();
    private bool puedeSerPenalizado = true;

    void Start()
    {
        // Configurar tiempo
        barraTiempo.minValue = 0;
        barraTiempo.maxValue = tiempoTotal;
        barraTiempo.value = tiempoTotal;
        tiempoRestante = tiempoTotal;
        tiempoVisual = tiempoTotal; // 🆕 Inicializar tiempo visual

        // Ocultar panel Game Over
        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        StartCoroutine(IniciarConDelay());

        Debug.Log("🎮 ControladorTiempo INICIADO");
    }

    IEnumerator IniciarConDelay()
    {
        yield return null;

        Debug.Log("🔄 Iniciando búsqueda de jugador...");

        BuscarJugador();

        if (jugador == null)
        {
            Debug.Log("⏳ Jugador no encontrado, esperando 0.5 segundos...");
            yield return new WaitForSeconds(0.5f);
            BuscarJugador();
        }

        BuscarEnemigos();

        StartCoroutine(ActualizarTiempo());
        StartCoroutine(ControlarEnemigos());
        StartCoroutine(ActualizarBarraSuavemente()); // 🆕 NUEVO Coroutine para barra suave

        if (jugador != null)
        {
            Debug.Log("🎮 Sistema INICIADO - Jugador: " + jugador.name);
        }
        else
        {
            Debug.LogError("🚨 Sistema iniciado SIN JUGADOR");
            StartCoroutine(BusquedaContinuaJugador());
        }
    }

    // 🆕 NUEVO MÉTODO - Actualización suave de la barra
    IEnumerator ActualizarBarraSuavemente()
    {
        while (tiempoActivo)
        {
            // Suavizar el movimiento de la barra hacia el tiempo real
            if (Mathf.Abs(tiempoVisual - tiempoRestante) > 0.1f)
            {
                tiempoVisual = Mathf.Lerp(tiempoVisual, tiempoRestante, Time.deltaTime * 5f);
                barraTiempo.value = tiempoVisual;

                Debug.Log("📊 BARRA: Visual=" + tiempoVisual.ToString("F1") +
                         " | Real=" + tiempoRestante.ToString("F1") +
                         " | Diferencia=" + (tiempoVisual - tiempoRestante).ToString("F2"));
            }
            else
            {
                // Cuando están cerca, igualar directamente
                tiempoVisual = tiempoRestante;
                barraTiempo.value = tiempoVisual;
            }

            yield return null;
        }
    }

    IEnumerator ActualizarTiempo()
    {
        while (tiempoActivo && tiempoRestante > 0)
        {
            yield return null;

            // 🆕 ACTUALIZACIÓN MÁS SUAVE del tiempo real
            float tiempoAnterior = tiempoRestante;
            tiempoRestante -= Time.deltaTime;

            // Debug solo si hay cambio significativo
            if (Mathf.Abs(tiempoAnterior - tiempoRestante) > 0.5f)
            {
                Debug.Log("⏱️ Tiempo actualizado: " + tiempoRestante.ToString("F1") + "s");
            }

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                TiempoAgotado();
            }
        }
    }

    void PenalizarTiempo()
    {
        if (!puedeSerPenalizado) return;

        // 🆕 PENALIZACIÓN MEJORADA - No afecta la barra inmediatamente
        float tiempoAnterior = tiempoRestante;
        tiempoRestante -= tiempoPenalizacion;

        // Asegurar que no sea negativo
        if (tiempoRestante < 0) tiempoRestante = 0;

        Debug.Log("⏰ Penalización! -" + tiempoPenalizacion +
                 " segundos. Tiempo: " + tiempoAnterior.ToString("F1") + " → " + tiempoRestante.ToString("F1"));

        // 🆕 EFECTO VISUAL MEJORADO para penalización
        StartCoroutine(EfectoPenalizacionMejorado());
        StartCoroutine(InmunidadTemporal());

        if (tiempoRestante <= 0)
        {
            TiempoAgotado();
        }
    }

    // 🆕 NUEVO MÉTODO - Efecto visual mejorado
    IEnumerator EfectoPenalizacionMejorado()
    {
        Image fillImage = barraTiempo.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            Color colorOriginal = fillImage.color;

            // Parpadeo más pronunciado
            for (int i = 0; i < 3; i++)
            {
                fillImage.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                fillImage.color = colorOriginal;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // 🆕 NUEVO MÉTODO - Forzar sincronización de barra
    void SincronizarBarra()
    {
        tiempoVisual = tiempoRestante;
        barraTiempo.value = tiempoVisual;
        Debug.Log("🔄 Barra sincronizada: " + tiempoVisual.ToString("F1"));
    }

    // Los demás métodos permanecen igual (BuscarJugador, SeguirJugador, etc.)
    void BuscarJugador()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            Debug.Log("✅ Jugador encontrado por TAG: " + jugador.name);
            return;
        }

        jugador = GameObject.Find("Player1(Clone)");
        if (jugador != null)
        {
            Debug.Log("✅ Jugador encontrado por NOMBRE: Player1(Clone)");
            return;
        }

        jugador = GameObject.Find("Player2(Clone)");
        if (jugador != null)
        {
            Debug.Log("✅ Jugador encontrado por NOMBRE: Player2(Clone)");
            return;
        }

        GameObject[] todosObjetos = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in todosObjetos)
        {
            if (obj.activeInHierarchy && obj.name.Contains("Player"))
            {
                jugador = obj;
                Debug.Log("✅ Jugador encontrado por CONTENIDO: " + jugador.name);
                return;
            }
        }

        Debug.LogError("❌ NO se pudo encontrar el jugador con ningún método");
    }

    IEnumerator BusquedaContinuaJugador()
    {
        int intentos = 0;
        while (jugador == null && intentos < 10)
        {
            intentos++;
            Debug.Log("🔍 Búsqueda continua de jugador... Intento: " + intentos);
            BuscarJugador();

            if (jugador == null) yield return new WaitForSeconds(1f);
        }

        if (jugador != null)
            Debug.Log("✅ Jugador encontrado después de " + intentos + " intentos: " + jugador.name);
        else
            Debug.LogError("❌ No se pudo encontrar el jugador después de " + intentos + " intentos");
    }

    void BuscarEnemigos()
    {
        GameObject[] enemigosEnEscena = GameObject.FindGameObjectsWithTag("Enemy");
        enemigos.Clear();
        enemigos.AddRange(enemigosEnEscena);
        Debug.Log("🔍 Enemigos encontrados: " + enemigos.Count);
    }

    IEnumerator ControlarEnemigos()
    {
        while (tiempoActivo)
        {
            yield return new WaitForSeconds(0.2f);

            if (jugador == null)
            {
                BuscarJugador();
                if (jugador == null) continue;
            }

            if (enemigos.Count == 0) continue;

            for (int i = 0; i < enemigos.Count; i++)
            {
                GameObject enemigo = enemigos[i];
                if (enemigo != null && enemigo.activeInHierarchy)
                {
                    float distancia = Vector3.Distance(enemigo.transform.position, jugador.transform.position);

                    if (distancia <= distanciaDeteccion)
                    {
                        Debug.Log("🎯 Enemigo " + enemigo.name + " DETECTÓ jugador - Distancia: " + distancia.ToString("F2"));
                        SeguirJugador(enemigo);
                    }

                    VerificarPenalizacion(enemigo, distancia);
                }
            }
        }
    }

    void SeguirJugador(GameObject enemigo)
    {
        if (jugador == null) return;

        Vector3 direccion = (jugador.transform.position - enemigo.transform.position).normalized;
        float velocidadReal = velocidadEnemigos * Time.deltaTime * 8f;
        enemigo.transform.position += direccion * velocidadReal;

        Debug.DrawLine(enemigo.transform.position, jugador.transform.position, Color.red, 0.1f);
    }

    void VerificarPenalizacion(GameObject enemigo, float distancia)
    {
        if (jugador == null || !puedeSerPenalizado) return;

        if (distancia < 1.5f)
        {
            Debug.Log("💥 COLISIÓN DETECTADA con " + enemigo.name + " - Distancia: " + distancia.ToString("F2"));
            PenalizarTiempo();
        }
    }

    IEnumerator InmunidadTemporal()
    {
        puedeSerPenalizado = false;
        yield return new WaitForSeconds(1f);
        puedeSerPenalizado = true;
        Debug.Log("🛡️ Inmunidad terminada");
    }

    void TiempoAgotado()
    {
        tiempoActivo = false;
        tiempoRestante = 0;
        tiempoVisual = 0;
        barraTiempo.value = 0;

        Debug.Log("🛑 GAME OVER - Tiempo agotado");

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        Time.timeScale = 0f;
    }

    public void AgregarEnemigo(GameObject nuevoEnemigo)
    {
        if (!enemigos.Contains(nuevoEnemigo))
        {
            enemigos.Add(nuevoEnemigo);
            Debug.Log("✅ Enemigo agregado: " + nuevoEnemigo.name);
        }
    }

    public void RemoverEnemigo(GameObject enemigo)
    {
        if (enemigos.Contains(enemigo))
        {
            enemigos.Remove(enemigo);
            Debug.Log("🗑️ Enemigo removido: " + enemigo.name);
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}