using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public int vidas = 3;
    public int numerosParesAgarrados = 0;
    public int numerosParesMeta = 5;
    public Text contadorParesTexto;
    public Text puntuacionTexto;
    public Image[] corazones;
    public GameObject panelVictoria;
    public GameObject panelGameOver;
    public Text tiempoTexto;
    public Slider barraDeTiempo;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI contadorObjetivosText;
    public TextMeshProUGUI mensajeTexto; // NUEVA REFERENCIA para el MensajePanel

    private int puntuacionNivel = 100;
    private int puntosPorVida = 50;
    public float tiempoLimite = 60f;
    private float tiempoRestante;

    private bool recogiendoPares = true;
    private bool primeraRondaCompletada = false;

    private List<GameObject> numerosOriginales = new List<GameObject>();
    private List<Vector3> posicionesOriginales = new List<Vector3>();
    private List<GameObject> numerosActuales = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (corazones == null || corazones.Length == 0)
        {
            Debug.LogError("No se han asignado las referencias de corazones en el Inspector");
        }

        if (timerText == null)
        {
            Debug.LogError("TimerText no está asignado en el Inspector");
        }

        if (contadorObjetivosText == null)
        {
            Debug.LogError("ContadorObjetivosText no está asignado en el Inspector");
        }

        if (mensajeTexto == null)
        {
            Debug.LogError("MensajeTexto no está asignado en el Inspector");
        }

        GuardarNumerosOriginales();

        ActualizarVidas();
        ActualizarContadorPares();
        ActualizarContadorObjetivos();
        ActualizarMensajeModalidad(); // ACTUALIZAR EL MENSAJE AL INICIAR

        tiempoRestante = tiempoLimite;

        if (barraDeTiempo != null)
        {
            barraDeTiempo.maxValue = tiempoLimite;
            barraDeTiempo.value = tiempoRestante;
        }

        recogiendoPares = true;
        primeraRondaCompletada = false;
        ActualizarTextoDeTiempo();

        Debug.Log("✅ GameController iniciado - Modalidad: Recogiendo Números PARES");
    }

    // NUEVO MÉTODO: ACTUALIZAR EL MENSAJE DE MODALIDAD
    void ActualizarMensajeModalidad()
    {
        if (mensajeTexto != null)
        {
            if (recogiendoPares)
            {
                mensajeTexto.text = "¡AGARRA NÚMEROS PARES!";
                mensajeTexto.color = Color.blue;
            }
            else
            {
                mensajeTexto.text = "¡AGARRA NÚMEROS IMPARES!";
                mensajeTexto.color = Color.red;
            }

            Debug.Log("📝 Mensaje actualizado: " + mensajeTexto.text);
        }
    }

    void ActualizarContadorObjetivos()
    {
        if (contadorObjetivosText != null)
        {
            int numerosRestantes = numerosParesMeta - numerosParesAgarrados;
            string modalidad = recogiendoPares ? "PARES" : "IMPARES";

            contadorObjetivosText.text = $"OBJETIVO: {modalidad}\nFALTAN: {numerosRestantes}";

            if (numerosRestantes <= 2)
            {
                contadorObjetivosText.color = Color.green;
            }
            else if (numerosRestantes <= 3)
            {
                contadorObjetivosText.color = Color.yellow;
            }
            else
            {
                contadorObjetivosText.color = Color.white;
            }
        }
    }

    void GuardarNumerosOriginales()
    {
        GameObject[] numerosEnEscena = GameObject.FindGameObjectsWithTag("Numero");

        foreach (GameObject numero in numerosEnEscena)
        {
            if (numero != null)
            {
                numerosOriginales.Add(numero);
                posicionesOriginales.Add(numero.transform.position);
                numerosActuales.Add(numero);

                Debug.Log("📍 Número guardado: " + numero.name + " en posición: " + numero.transform.position);
            }
        }

        Debug.Log("✅ Se guardaron " + numerosOriginales.Count + " números originales");
    }

    void ReactivarTodosLosNumeros()
    {
        Debug.Log("🔄 Reactivando todos los números...");

        foreach (GameObject numeroOriginal in numerosOriginales)
        {
            if (numeroOriginal != null)
            {
                numeroOriginal.SetActive(true);

                int index = numerosOriginales.IndexOf(numeroOriginal);
                if (index < posicionesOriginales.Count)
                {
                    numeroOriginal.transform.position = posicionesOriginales[index];
                }

                Numeros scriptNumero = numeroOriginal.GetComponent<Numeros>();
                if (scriptNumero != null)
                {
                    scriptNumero.Invoke("ReiniciarNumero", 0f);
                }

                Debug.Log("🔢 Número reactivado: " + numeroOriginal.name);
            }
        }

        numerosActuales.Clear();
        foreach (GameObject numero in numerosOriginales)
        {
            if (numero != null && numero.activeInHierarchy)
            {
                numerosActuales.Add(numero);
            }
        }

        Debug.Log("✅ Se reactivaron " + numerosActuales.Count + " números");
    }

    void Update()
    {
        if (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;
            ActualizarBarraDeTiempo();
            ActualizarTextoDeTiempo();

            if (tiempoTexto != null)
            {
                tiempoTexto.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString();
            }
        }
        else
        {
            tiempoRestante = 0f;
            ActualizarTextoDeTiempo();

            if (!primeraRondaCompletada && recogiendoPares)
            {
                CambiarModalidadAImpares();
            }
            else
            {
                MostrarPanelGameOver();
            }
        }
    }

    void ActualizarTextoDeTiempo()
    {
        if (timerText != null)
        {
            int segundos = Mathf.CeilToInt(tiempoRestante);
            timerText.text = segundos.ToString() + "s";

            if (segundos <= 10)
            {
                timerText.color = Color.red;
            }
            else if (segundos <= 20)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    private void CambiarModalidadAImpares()
    {
        Debug.Log("🔄 Cambiando a modalidad IMPARES...");

        primeraRondaCompletada = true;
        recogiendoPares = false;
        tiempoLimite = 80f;
        tiempoRestante = tiempoLimite;

        // REINICIAR CONTADOR A 5 PARA LA NUEVA RONDA
        numerosParesAgarrados = 0;

        ReactivarTodosLosNumeros();

        if (barraDeTiempo != null)
        {
            barraDeTiempo.maxValue = tiempoLimite;
            barraDeTiempo.value = tiempoRestante;
        }

        ActualizarTextoDeTiempo();
        ActualizarContadorPares();
        ActualizarContadorObjetivos();
        ActualizarMensajeModalidad(); // ACTUALIZAR EL MENSAJE AL CAMBIAR MODALIDAD

        Debug.Log("✅ Cambiado a modalidad: Recogiendo Números IMPARES - Tiempo: 80s");
    }

    public void ProcesarNumero(int numero, GameObject objetoNumero)
    {
        bool esCorrecto = false;

        if (recogiendoPares)
        {
            esCorrecto = (numero % 2 == 0);
        }
        else
        {
            esCorrecto = (numero % 2 != 0);
        }

        if (esCorrecto)
        {
            AumentarContadorPares();
            Debug.Log("✅ Número correcto: " + numero + " (Modalidad: " + (recogiendoPares ? "PARES" : "IMPARES") + ")");
        }
        else
        {
            PerderVida();
            Debug.Log("❌ Número incorrecto: " + numero + " (Modalidad: " + (recogiendoPares ? "PARES" : "IMPARES") + ")");
        }

        if (objetoNumero != null)
        {
            objetoNumero.SetActive(false);
            numerosActuales.Remove(objetoNumero);
            Debug.Log("🔴 Número desactivado: " + objetoNumero.name);
        }
    }

    public void AumentarContadorPares()
    {
        numerosParesAgarrados++;
        ActualizarContadorPares();
        ActualizarContadorObjetivos();

        if (numerosParesAgarrados >= numerosParesMeta)
        {
            CalcularPuntuacion();
            MostrarPanelVictoria();
        }
    }

    void ActualizarBarraDeTiempo()
    {
        if (barraDeTiempo != null)
        {
            barraDeTiempo.value = tiempoRestante;
        }
    }

    public void ProcesarComodin(bool respuestaCorrecta)
    {
        if (respuestaCorrecta)
        {
            PerderVida();
            Debug.Log("❌ Comodín: Respuesta CORRECTA - Se pierde una vida");
        }
        else
        {
            AddLife();
            Debug.Log("✅ Comodín: Respuesta INCORRECTA - Se gana una vida");
        }
    }

    public void AddLife()
    {
        if (vidas < corazones.Length)
        {
            vidas++;
            ActualizarVidas();
            Debug.Log("❤️ Vida agregada. Total: " + vidas);
        }
    }

    public void LoseLife()
    {
        PerderVida();
    }

    public void PerderVida()
    {
        vidas--;
        ActualizarVidas();
        Debug.Log("💔 Vida perdida. Vidas restantes: " + vidas);

        if (vidas <= 0)
        {
            Debug.Log("💀 Game Over - Sin vidas");
            MostrarPanelGameOver();
        }
    }

    void CalcularPuntuacion()
    {
        int puntosTotalesNivel = puntuacionNivel + (vidas * puntosPorVida);
        PuntuacionTotal.AgregarPuntos(puntosTotalesNivel);

        if (puntuacionTexto != null)
        {
            puntuacionTexto.text = "Puntuación: " + PuntuacionTotal.ObtenerPuntos();
        }

        Debug.Log("🏆 Puntuación calculada: " + puntosTotalesNivel);
    }

    void ActualizarContadorPares()
    {
        if (contadorParesTexto != null)
        {
            string modalidad = recogiendoPares ? "Pares" : "Impares";
            contadorParesTexto.text = modalidad + ": " + numerosParesAgarrados + " / " + numerosParesMeta;
        }
    }

    void ActualizarVidas()
    {
        if (corazones != null)
        {
            for (int i = 0; i < corazones.Length; i++)
            {
                if (corazones[i] != null)
                {
                    corazones[i].enabled = (i < vidas);
                }
            }
        }
    }

    void MostrarPanelVictoria()
    {
        Time.timeScale = 0f;
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
        Debug.Log("🎉 Victoria - Nivel completado");
    }

    void MostrarPanelGameOver()
    {
        Time.timeScale = 0f;
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
        Debug.Log("💀 Game Over - Mostrando panel");
    }

    public void SiguienteNivel()
    {
        Time.timeScale = 1f;
        int siguienteNivel = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(siguienteNivel);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicio");
    }
}