using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Platformer.Mechanics;

public class PreguntaSistema : MonoBehaviour
{
    public static PreguntaSistema Instance;

    [Header("UI References")]
    public TMP_Text textoPregunta;
    public TMP_Text[] textosOpciones;
    public GameObject panelResultado;
    public TMP_Text textoResultado;
    public GameObject panelMensajeInicio;
    public TMP_Text textoMensajeInicio;

    [Header("Múltiples Puntos de Reinicio")]
    public List<Transform> puntosReinicio = new List<Transform>();
    private int puntoReinicioIndex = 0;

    [Header("Configuración del Jugador")]
    [SerializeField] private GameObject jugador;

    [Header("Configuración de Preguntas")]
    public string[] operadores = { "+", "-", "×" };
    public int minNumero = 1;
    public int maxNumero = 10;

    [Header("Configuración Tiempo")]
    public float tiempoMensajeInicio = 3f;

    private int respuestaCorrectaIndex;
    private bool preguntaActiva = false;
    private Vector3 posicionInicialJugador;
    private int respuestaCorrectaActual;

    // ✅ NUEVO: Variable para controlar si ya se generó la primera pregunta
    private bool primeraVezActivada = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(BuscarJugadorConReintento());
        OcultarPregunta(); // ✅ IMPORTANTE: Asegurar que esté oculto al inicio
        OcultarResultado();
        MostrarMensajeInicio();
    }

    public void MostrarMensajeInicio()
    {
        if (panelMensajeInicio != null && textoMensajeInicio != null)
        {
            textoMensajeInicio.text = "¡ATENCIÓN! SELECCIONA LA OPCIÓN CORRECTA\nANTES QUE SE ACABE EL TIEMPO";
            textoMensajeInicio.color = Color.yellow;
            textoMensajeInicio.fontSize = 36;

            RectTransform rectTransform = panelMensajeInicio.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

            panelMensajeInicio.SetActive(true);
            Debug.Log("📢 Mensaje de inicio mostrado");

            StartCoroutine(OcultarMensajeInicioDespuesDeTiempo(tiempoMensajeInicio));
        }
        else
        {
            Debug.LogError("❌ Referencias de mensaje inicio no asignadas");
        }
    }

    IEnumerator OcultarMensajeInicioDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        OcultarMensajeInicio();
    }

    public void OcultarMensajeInicio()
    {
        if (panelMensajeInicio != null)
        {
            panelMensajeInicio.SetActive(false);
            Debug.Log("📢 Mensaje de inicio ocultado");
        }
    }

    public void ConfigurarPuntoReinicio(int nuevoIndex)
    {
        Debug.Log($"🔧 CONFIGURANDO PUNTO REINICIO - Solicitado: {nuevoIndex}");
        Debug.Log($"   - Puntos en lista: {puntosReinicio.Count}");

        if (nuevoIndex >= 0 && nuevoIndex < puntosReinicio.Count)
        {
            puntoReinicioIndex = nuevoIndex;

            if (puntosReinicio[puntoReinicioIndex] != null)
            {
                Debug.Log($"📍 Punto reinicio configurado a índice: {nuevoIndex} - Objeto: {puntosReinicio[puntoReinicioIndex].name}");
            }
            else
            {
                Debug.LogError($"❌ Punto reinicio en índice {nuevoIndex} es NULL!");
            }
        }
        else
        {
            Debug.LogError($"❌ Índice de punto reinicio inválido: {nuevoIndex} (máximo: {puntosReinicio.Count - 1})");
        }

        Debug.Log($"🔧 Índice actual después de configurar: {puntoReinicioIndex}");
    }

    public void MostrarResultado(bool esCorrecta, int respuestaCorrecta = 0)
    {
        if (panelResultado != null && textoResultado != null)
        {
            if (esCorrecta)
            {
                textoResultado.text = "🎉 ¡TU RESPUESTA ES CORRECTA!";
                textoResultado.color = Color.green;
                textoResultado.fontSize = 48; // Tamaño grande
            }
            else
            {
                textoResultado.text = $"❌ TU RESPUESTA ES INCORRECTA\n\nLA CORRECTA ES: {respuestaCorrecta}";
                textoResultado.color = Color.red;
                textoResultado.fontSize = 42; // Tamaño grande
            }

            panelResultado.SetActive(true);
            Debug.Log($"📋 Panel de resultado mostrado - Correcta: {esCorrecta}");

            // El panel se ocultará automáticamente después de 3 segundos
            // gracias al script PanelResultadoSigueJugador
        }
        else
        {
            Debug.LogError("❌ Referencias de resultado no asignadas");
        }
    }

    public void OcultarResultado()
    {
        if (panelResultado != null)
        {
            panelResultado.SetActive(false);
            Debug.Log("📋 Panel de resultado ocultado");
        }
    }

    void BuscarJugador()
    {
        Debug.Log("🔍 BUSQUEDA MEJORADA: Buscando jugador...");

        jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            Debug.Log($"✅ ¡Jugador encontrado por TAG! {jugador.name}");
            posicionInicialJugador = jugador.transform.position;
            VerificarComponentesJugador();
            return;
        }

        string[] nombresJugador = {
            "Player(Clone)", "Jugador", "Player", "Personaje",
            "JugadorEmergencia", "JugadorDirecto", "JugadorRespaldo"
        };

        foreach (string nombre in nombresJugador)
        {
            jugador = GameObject.Find(nombre);
            if (jugador != null)
            {
                Debug.Log($"✅ ¡Jugador encontrado por NOMBRE! {jugador.name}");
                if (jugador.tag != "Player")
                {
                    jugador.tag = "Player";
                    Debug.Log($"🏷️ Tag asignado: Player");
                }
                posicionInicialJugador = jugador.transform.position;
                VerificarComponentesJugador();
                return;
            }
        }

        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            GameObject obj = sprite.gameObject;

            if (obj.name.Contains("UI") || obj.name.Contains("Panel") ||
                obj.name.Contains("Background") || obj.name.Contains("Tile") ||
                obj.GetComponentInParent<Canvas>() != null)
                continue;

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                jugador = obj;
                Debug.Log($"✅ ¡Jugador encontrado por COMPONENTES! {jugador.name}");
                if (jugador.tag != "Player")
                {
                    jugador.tag = "Player";
                    Debug.Log($"🏷️ Tag asignado: Player");
                }
                posicionInicialJugador = jugador.transform.position;
                VerificarComponentesJugador();
                return;
            }
        }

        Debug.Log("⏳ Jugador no encontrado, esperando que InicioJugador lo cree...");
    }

    void VerificarComponentesJugador()
    {
        if (jugador == null) return;

        Debug.Log($"=== ✅ VERIFICACIÓN JUGADOR: {jugador.name} ===");
        Debug.Log($"   - Tag: {jugador.tag}");
        Debug.Log($"   - SpriteRenderer: {jugador.GetComponent<SpriteRenderer>() != null}");
        Debug.Log($"   - Rigidbody2D: {jugador.GetComponent<Rigidbody2D>() != null}");
        Debug.Log($"   - Collider2D: {jugador.GetComponent<Collider2D>() != null}");
        Debug.Log($"   - PlayerController: {jugador.GetComponent("PlayerController") != null}");
        Debug.Log($"   - Posición: {jugador.transform.position}");
        Debug.Log("=== FIN VERIFICACIÓN ===");
    }

    public IEnumerator BuscarJugadorConReintento()
    {
        Debug.Log("🔄 INICIANDO BÚSQUEDA CON REINTENTOS MEJORADA");

        int intentos = 0;
        int maxIntentos = 25;

        while (jugador == null && intentos < maxIntentos)
        {
            Debug.Log($"🔄 Búsqueda de jugador - Intento {intentos + 1}/{maxIntentos}");
            BuscarJugador();

            if (jugador != null)
            {
                Debug.Log($"🎉 ¡JUGADOR ENCONTRADO en intento {intentos + 1}!: {jugador.name}");
                break;
            }
            else
            {
                Debug.Log($"⏳ Jugador no encontrado, reintentando en 0.5s...");
                intentos++;
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (jugador != null)
        {
            Debug.Log($"✅ BÚSQUEDA EXITOSA: Jugador '{jugador.name}' listo para usar");
            Debug.Log($"📍 Posición: {jugador.transform.position}");
        }
        else
        {
            Debug.LogError($"💀 CRÍTICO: No se pudo encontrar jugador después de {maxIntentos} intentos");
            Debug.Log("❌ PreguntaSistema no creará jugador de emergencia - depende de InicioJugador");
        }
    }

    // ✅ MÉTODO MODIFICADO: Solo generar pregunta cuando el botón lo active
    public void GenerarNuevaPregunta()
    {
        Debug.Log("🔄 Generando nueva pregunta...");

        if (jugador == null)
        {
            Debug.Log("🔍 Jugador no encontrado, buscando nuevamente...");
            BuscarJugador();

            if (jugador == null)
            {
                Debug.LogError("❌ No se puede generar pregunta: jugador no encontrado");
                return;
            }
        }

        int num1 = Random.Range(minNumero, maxNumero + 1);
        int num2 = Random.Range(minNumero, maxNumero + 1);
        string operador = operadores[Random.Range(0, operadores.Length)];

        if (operador == "-" && num1 < num2)
        {
            int temp = num1;
            num1 = num2;
            num2 = temp;
        }

        respuestaCorrectaActual = CalcularRespuesta(num1, num2, operador);
        string pregunta = $"{num1} {operador} {num2} = ?";

        if (textoPregunta != null)
            textoPregunta.text = pregunta;

        int[] opciones = GenerarOpciones(respuestaCorrectaActual);
        respuestaCorrectaIndex = MezclarOpciones(opciones);

        preguntaActiva = true;
        primeraVezActivada = true; // ✅ Marcar que ya se activó la primera vez
        MostrarPregunta();
        OcultarResultado();

        Debug.Log($"📝 Pregunta: {pregunta} - Correcta: {respuestaCorrectaActual} (Camino {respuestaCorrectaIndex + 1})");
        Debug.Log($"🎯 Estado pregunta activa: {preguntaActiva}");
    }

    int CalcularRespuesta(int a, int b, string operador)
    {
        switch (operador)
        {
            case "+": return a + b;
            case "-": return a - b;
            case "×": return a * b;
            default: return a + b;
        }
    }

    int[] GenerarOpciones(int respuestaCorrecta)
    {
        int[] opciones = new int[3];
        opciones[0] = respuestaCorrecta;

        int opcion1, opcion2;
        do
        {
            opcion1 = respuestaCorrecta + Random.Range(-3, 4);
        } while (opcion1 == respuestaCorrecta || opcion1 <= 0);

        do
        {
            opcion2 = respuestaCorrecta + Random.Range(-3, 4);
        } while (opcion2 == respuestaCorrecta || opcion2 <= 0 || opcion2 == opcion1);

        opciones[1] = opcion1;
        opciones[2] = opcion2;

        return opciones;
    }

    int MezclarOpciones(int[] opciones)
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            int temp = opciones[i];
            int randomIndex = Random.Range(i, opciones.Length);
            opciones[i] = opciones[randomIndex];
            opciones[randomIndex] = temp;
        }

        int correctIndex = 0;
        for (int i = 0; i < opciones.Length; i++)
        {
            if (textosOpciones[i] != null)
                textosOpciones[i].text = opciones[i].ToString();

            string[] partesPregunta = textoPregunta.text.Split(' ');
            int num1 = int.Parse(partesPregunta[0]);
            int num2 = int.Parse(partesPregunta[2]);
            string operador = partesPregunta[1];
            int respuestaCorrectaReal = CalcularRespuesta(num1, num2, operador);

            if (opciones[i] == respuestaCorrectaReal)
            {
                correctIndex = i;
            }
        }

        return correctIndex;
    }

    // ✅ MÉTODO MODIFICADO: Solo verificar si ya se activó la primera vez
    public void VerificarRespuesta(int caminoIndex)
    {
        // ✅ NUEVO: Ignorar si no se ha activado la primera vez (al presionar botón)
        if (!primeraVezActivada)
        {
            Debug.Log("⚠️ Pregunta aún no activada por botón. Ignorando sensor...");
            return;
        }

        Debug.Log("🎯 Verificando respuesta...");
        Debug.Log($"   - Pregunta activa: {preguntaActiva}");
        Debug.Log($"   - Camino seleccionado: {caminoIndex}");
        Debug.Log($"   - Respuesta correcta: {respuestaCorrectaIndex}");

        if (!preguntaActiva)
        {
            Debug.Log("❌ Pregunta no activa, ignorando...");
            return;
        }

        Debug.Log($"🎯 Camino seleccionado: {caminoIndex}, Correcto: {respuestaCorrectaIndex}");

        if (caminoIndex == respuestaCorrectaIndex)
        {
            Debug.Log("✅ ¡Respuesta Correcta! Avanzando...");
            MostrarResultado(true, respuestaCorrectaActual);
            preguntaActiva = false;
            StartCoroutine(OcultarPreguntaDespuesDeTiempo(2f));
        }
        else
        {
            Debug.Log("❌ Respuesta Incorrecta! Reiniciando...");
            MostrarResultado(false, respuestaCorrectaActual);
            preguntaActiva = false;
            StartCoroutine(ReiniciarDespuesDeTiempo(2f));
        }
    }

    IEnumerator OcultarPreguntaDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        OcultarPregunta();
        OcultarResultado();
        primeraVezActivada = false; // ✅ Resetear para la próxima pregunta
        Debug.Log("✅ Flujo completado - Respuesta correcta");
    }

    IEnumerator ReiniciarDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        OcultarPregunta();
        OcultarResultado();
        ReiniciarJugador();
    }

    void ReiniciarJugador()
    {
        Debug.Log($"🎯 INICIANDO REINICIO - Índice actual: {puntoReinicioIndex}");

        if (puntosReinicio.Count > puntoReinicioIndex)
        {
            if (puntosReinicio[puntoReinicioIndex] != null)
            {
                Debug.Log($"📍 Punto reinicio válido: {puntosReinicio[puntoReinicioIndex].name} en posición: {puntosReinicio[puntoReinicioIndex].position}");
            }
            else
            {
                Debug.LogError($"❌ Punto reinicio en índice {puntoReinicioIndex} es NULL!");
            }
        }
        else
        {
            Debug.LogError($"❌ Índice {puntoReinicioIndex} fuera de rango. Lista tiene {puntosReinicio.Count} elementos");
        }

        if (jugador != null)
        {
            Debug.Log("🔄 Iniciando reinicio del jugador...");
            OcultarPregunta();

            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            Vector2 velocidadOriginal = Vector2.zero;
            bool teniaRB = false;

            if (rb != null)
            {
                velocidadOriginal = rb.velocity;
                rb.velocity = Vector2.zero;
                rb.simulated = false;
                teniaRB = true;
                Debug.Log("🔧 Rigidbody desactivado temporalmente");
            }

            Collider2D[] colliders = jugador.GetComponents<Collider2D>();
            bool[] collidersEstado = new bool[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                collidersEstado[i] = colliders[i].enabled;
                colliders[i].enabled = false;
            }
            Debug.Log($"🔧 {colliders.Length} colliders desactivados");

            SpriteRenderer sprite = jugador.GetComponent<SpriteRenderer>();
            bool spriteActivo = true;
            if (sprite != null)
            {
                spriteActivo = sprite.enabled;
                sprite.enabled = false;
                Debug.Log("🔧 SpriteRenderer desactivado temporalmente");
            }

            MonoBehaviour[] scripts = jugador.GetComponents<MonoBehaviour>();
            bool[] scriptsEstado = new bool[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
            {
                if (scripts[i] != this &&
                    !scripts[i].GetType().Name.Contains("GameController") &&
                    scripts[i].enabled)
                {
                    scriptsEstado[i] = true;
                    scripts[i].enabled = false;
                }
            }
            Debug.Log($"🔧 Scripts de movimiento desactivados");

            StartCoroutine(ReaparecerConNuevaPregunta(velocidadOriginal, teniaRB, colliders, collidersEstado, spriteActivo, scripts, scriptsEstado));
        }
        else
        {
            Debug.LogError("❌ No se puede reiniciar: jugador no encontrado");

            BuscarJugador();
            if (jugador != null)
            {
                Debug.Log("✅ Jugador encontrado después de búsqueda de emergencia");
                ReiniciarJugador();
            }
        }
    }

    IEnumerator ReaparecerConNuevaPregunta(Vector2 velocidadOriginal, bool teniaRB, Collider2D[] colliders, bool[] collidersEstado, bool spriteActivo, MonoBehaviour[] scripts, bool[] scriptsEstado)
    {
        Debug.Log("⏳ Esperando 3 segundos antes de reaparecer...");
        yield return new WaitForSeconds(3f);

        Debug.Log("🎮 Reapareciendo jugador...");

        if (puntosReinicio.Count > puntoReinicioIndex && puntosReinicio[puntoReinicioIndex] != null)
        {
            Vector3 posicionReinicio = puntosReinicio[puntoReinicioIndex].position;
            posicionReinicio.z = 0;
            jugador.transform.position = posicionReinicio;
            Debug.Log($"📍 Jugador posicionado en: {puntosReinicio[puntoReinicioIndex].name} - {posicionReinicio}");
        }
        else
        {
            posicionInicialJugador.z = 0;
            jugador.transform.position = posicionInicialJugador;
            Debug.Log($"📍 Jugador posicionado en posición inicial: {posicionInicialJugador}");
        }

        SpriteRenderer sprite = jugador.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = spriteActivo;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (i < collidersEstado.Length)
            {
                colliders[i].enabled = collidersEstado[i];
            }
        }

        if (teniaRB)
        {
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
                rb.velocity = velocidadOriginal;
            }
        }

        for (int i = 0; i < scripts.Length; i++)
        {
            if (i < scriptsEstado.Length && scriptsEstado[i])
            {
                scripts[i].enabled = true;
            }
        }

        yield return new WaitForSeconds(0.5f);
        GenerarNuevaPregunta();
        Debug.Log("✅ Reinicio completado exitosamente");
    }

    public void MostrarPregunta()
    {
        if (textoPregunta != null && textoPregunta.transform.parent != null)
        {
            textoPregunta.transform.parent.gameObject.SetActive(true);
            Debug.Log("📋 Panel de pregunta MOSTRADO");
        }
    }

    public void OcultarPregunta()
    {
        if (textoPregunta != null && textoPregunta.transform.parent != null)
        {
            textoPregunta.transform.parent.gameObject.SetActive(false);
            Debug.Log("📋 Panel de pregunta OCULTADO");
        }
    }

    public void ForzarBusquedaJugador()
    {
        BuscarJugador();
    }
}