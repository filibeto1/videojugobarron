using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class PreguntaSistemaFlotante : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text textoPregunta;
    public TMP_Text[] textosOpciones;
    public GameObject panelPreguntaFlotante;
    public GameObject panelResultadoFlotante;
    public TMP_Text textoResultado;

    [Header("Referencias de Botones UI")]
    public Button botonRespuesta1;
    public Button botonRespuesta2;
    public Button botonRespuesta3;

    [Header("Referencias del Botón")]
    public BotonFlotante botonFlotante;

    [Header("Configuración Flotante")]
    [Tooltip("📏 ALTURA: Qué tan alto llega (30-100 recomendado)")]
    public float alturaMaxima = 30f;

    [Tooltip("⏱️ TIEMPO ARRIBA: Cuántos segundos se queda flotando arriba (2-5 recomendado)")]
    public float tiempoArriba = 3f;

    [Tooltip("Permitir movimiento horizontal mientras flota")]
    public bool permitirMovimientoEnFlotacion = true;

    [Tooltip("Velocidad de movimiento horizontal durante flotación")]
    public float velocidadMovimientoFlotante = 5f;

    [Header("Configuración de Preguntas")]
    public string[] operadores = { "+", "-", "×" };
    public int minNumero = 1;
    public int maxNumero = 10;

    private GameObject jugador;
    private Rigidbody2D rbJugador;
    private MonoBehaviour playerController;
    private float gravedadOriginal;
    private int respuestaCorrectaIndex;
    private int respuestaCorrectaValor;
    private bool modoFlotante = false;
    private bool yaRespondio = false; // ✅ Evitar respuestas duplicadas

    void Start()
    {
        Debug.Log("🔍 Inicializando PreguntaSistemaFlotante...");

        ConfigurarBotonesUI();

        if (textoPregunta == null) Debug.LogError("❌ textoPregunta NO asignado");
        if (textosOpciones == null || textosOpciones.Length == 0) Debug.LogError("❌ textosOpciones NO asignados");
        if (botonRespuesta1 == null) Debug.LogError("❌ botonRespuesta1 NO asignado");
        if (botonRespuesta2 == null) Debug.LogError("❌ botonRespuesta2 NO asignado");
        if (botonRespuesta3 == null) Debug.LogError("❌ botonRespuesta3 NO asignado");
        if (panelPreguntaFlotante == null) Debug.LogError("❌ panelPreguntaFlotante NO asignado");
        if (panelResultadoFlotante == null) Debug.LogError("❌ panelResultadoFlotante NO asignado");
        if (textoResultado == null) Debug.LogError("❌ textoResultado NO asignado");
        if (botonFlotante == null) Debug.LogError("❌ botonFlotante NO asignado");

        if (panelPreguntaFlotante != null) panelPreguntaFlotante.SetActive(false);
        if (panelResultadoFlotante != null) panelResultadoFlotante.SetActive(false);
    }

    void FixedUpdate()
    {
        // ✅ Movimiento horizontal durante flotación
        if (modoFlotante && permitirMovimientoEnFlotacion && jugador != null && rbJugador != null)
        {
            float inputHorizontal = Input.GetAxisRaw("Horizontal");

            if (inputHorizontal != 0)
            {
                Vector2 velocidadActual = rbJugador.velocity;
                velocidadActual.x = inputHorizontal * velocidadMovimientoFlotante;
                rbJugador.velocity = velocidadActual;
            }
        }
    }

    private void ConfigurarBotonesUI()
    {
        if (botonRespuesta1 != null)
        {
            botonRespuesta1.onClick.RemoveAllListeners();
            botonRespuesta1.onClick.AddListener(() => OnBotonPresionado(0));
            Debug.Log("✅ BotonRespuesta1 configurado");
        }

        if (botonRespuesta2 != null)
        {
            botonRespuesta2.onClick.RemoveAllListeners();
            botonRespuesta2.onClick.AddListener(() => OnBotonPresionado(1));
            Debug.Log("✅ BotonRespuesta2 configurado");
        }

        if (botonRespuesta3 != null)
        {
            botonRespuesta3.onClick.RemoveAllListeners();
            botonRespuesta3.onClick.AddListener(() => OnBotonPresionado(2));
            Debug.Log("✅ BotonRespuesta3 configurado");
        }

        Debug.Log("🎯 Todos los botones UI configurados con eventos de clic");
    }

    private void OnBotonPresionado(int indiceBoton)
    {
        if (yaRespondio)
        {
            Debug.Log("⚠️ Ya se respondió esta pregunta, ignorando clic");
            return;
        }

        Debug.Log($"🖱️ Botón {indiceBoton} presionado");
        VerificarRespuesta(indiceBoton);
    }

    public void ConfigurarJugador(GameObject jugadorObj)
    {
        jugador = jugadorObj;
        rbJugador = jugador.GetComponent<Rigidbody2D>();

        // Buscar el PlayerController
        playerController = jugador.GetComponent<MonoBehaviour>();
        Component[] components = jugador.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp.GetType().Name == "PlayerController")
            {
                playerController = (MonoBehaviour)comp;
                Debug.Log($"✅ PlayerController encontrado: {comp.GetType().Name}");
                break;
            }
        }

        if (rbJugador != null)
        {
            gravedadOriginal = rbJugador.gravityScale;
            Debug.Log($"✅ Jugador configurado - Gravedad original: {gravedadOriginal}");
        }
        else
        {
            Debug.LogError("❌ El jugador no tiene Rigidbody2D");
        }
    }

    public void GenerarPreguntaFlotante()
    {
        Debug.Log("🎲 Generando pregunta flotante...");

        yaRespondio = false; // ✅ Resetear flag

        if (textoPregunta == null)
        {
            Debug.LogError("❌ textoPregunta es NULL");
            return;
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

        respuestaCorrectaValor = CalcularRespuesta(num1, num2, operador);

        textoPregunta.text = $"{num1} {operador} {num2} = ?";
        textoPregunta.color = Color.white;

        int[] opciones = GenerarOpciones(respuestaCorrectaValor);
        respuestaCorrectaIndex = MezclarOpciones(opciones);

        if (panelPreguntaFlotante != null)
        {
            panelPreguntaFlotante.SetActive(true);
            Debug.Log("📋 PanelPreguntaFlotante ACTIVADO");
        }

        Debug.Log($"📝 Pregunta: {num1} {operador} {num2} = {respuestaCorrectaValor}");
        Debug.Log($"🎯 Respuesta correcta en botón: {respuestaCorrectaIndex}");
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
        do { opcion1 = respuestaCorrecta + Random.Range(-5, 6); }
        while (opcion1 == respuestaCorrecta || opcion1 <= 0);

        do { opcion2 = respuestaCorrecta + Random.Range(-5, 6); }
        while (opcion2 == respuestaCorrecta || opcion2 <= 0 || opcion2 == opcion1);

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
        for (int i = 0; i < opciones.Length && i < textosOpciones.Length; i++)
        {
            if (textosOpciones[i] != null)
            {
                textosOpciones[i].text = opciones[i].ToString();
                textosOpciones[i].color = Color.black;
                Debug.Log($"✅ Botón {i} texto: {textosOpciones[i].text}");
            }

            if (opciones[i] == respuestaCorrectaValor)
            {
                correctIndex = i;
            }
        }

        return correctIndex;
    }

    public void VerificarRespuesta(int botonIndex)
    {
        if (yaRespondio)
        {
            Debug.Log("⚠️ Ya se verificó la respuesta, ignorando");
            return;
        }

        yaRespondio = true; // ✅ Marcar como respondido

        Debug.Log($"🎯 Verificando respuesta - Botón: {botonIndex}, Correcto: {respuestaCorrectaIndex}");

        if (botonIndex == respuestaCorrectaIndex)
        {
            Debug.Log("✅ ¡RESPUESTA CORRECTA! Activando modo flotante");
            MostrarResultado(true);

            if (botonFlotante != null)
            {
                botonFlotante.OnRespuestaCorrecta();
            }

            StartCoroutine(ActivarModoFlotante());
        }
        else
        {
            Debug.Log("❌ Respuesta incorrecta");
            MostrarResultado(false);
            StartCoroutine(CerrarPanelesDespuesDeTiempo(2f));
            yaRespondio = false; // Permitir otro intento
        }
    }

    void MostrarResultado(bool esCorrecta)
    {
        if (panelResultadoFlotante != null && textoResultado != null)
        {
            if (esCorrecta)
            {
                textoResultado.text = "¡CORRECTO!\n\n¡Prepárate para flotar!";
                textoResultado.color = Color.green;
            }
            else
            {
                textoResultado.text = $"INCORRECTO\n\nLa respuesta era: {respuestaCorrectaValor}";
                textoResultado.color = Color.red;
            }

            panelResultadoFlotante.SetActive(true);
        }
    }

    IEnumerator ActivarModoFlotante()
    {
        yield return new WaitForSeconds(2f);

        if (panelPreguntaFlotante != null) panelPreguntaFlotante.SetActive(false);
        if (panelResultadoFlotante != null) panelResultadoFlotante.SetActive(false);

        if (rbJugador == null || jugador == null)
        {
            Debug.LogError("❌ No hay Rigidbody2D o jugador");
            yaRespondio = false;
            yield break;
        }

        Debug.Log("🎈 INICIANDO FLOTACIÓN");
        modoFlotante = true;

        // Guardar posición inicial
        float posicionInicialY = jugador.transform.position.y;
        float posicionObjetivoY = posicionInicialY + alturaMaxima;

        Debug.Log($"🎈 Subiendo desde Y:{posicionInicialY:F2} hasta Y:{posicionObjetivoY:F2}");

        // ✅ VERIFICAR QUE LOS COLLIDERS ESTÉN ACTIVOS
        Collider2D[] colliders = jugador.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (!col.enabled)
            {
                Debug.LogWarning($"⚠️ Collider {col.GetType().Name} estaba desactivado, activándolo");
                col.enabled = true;
            }
        }

        // ✅ DESACTIVAR PlayerController
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("🔇 PlayerController DESACTIVADO");
        }

        // ✅ QUITAR GRAVEDAD (pero mantener masa y fricción)
        rbJugador.gravityScale = 0f;
        rbJugador.velocity = Vector2.zero;

        // Asegurar que el Rigidbody2D esté configurado correctamente
        RigidbodyType2D tipoOriginal = rbJugador.bodyType;
        if (rbJugador.bodyType != RigidbodyType2D.Dynamic)
        {
            rbJugador.bodyType = RigidbodyType2D.Dynamic;
            Debug.Log("✅ Rigidbody2D configurado como Dynamic");
        }

        // ✅ SUBIR USANDO AddForce (más suave que velocity)
        float velocidadSubida = 2.5f;
        float tiempoMaximo = alturaMaxima / velocidadSubida + 5f; // Tiempo estimado
        float tiempoTranscurrido = 0f;

        while (jugador.transform.position.y < posicionObjetivoY && tiempoTranscurrido < tiempoMaximo)
        {
            // Aplicar fuerza hacia arriba
            rbJugador.velocity = new Vector2(rbJugador.velocity.x, velocidadSubida);

            tiempoTranscurrido += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Detener movimiento vertical
        rbJugador.velocity = new Vector2(rbJugador.velocity.x, 0);

        Debug.Log($"✅ Llegó a altura: Y={jugador.transform.position.y:F2}");
        Debug.Log($"🎈 Flotando por {tiempoArriba} segundos...");

        // ✅ QUEDARSE FLOTANDO
        yield return new WaitForSeconds(tiempoArriba);

        // ✅ RESTAURAR TODO
        Debug.Log("🎈 Restaurando gravedad y control");

        modoFlotante = false;

        // ✅ FORZAR BODY TYPE A DYNAMIC
        rbJugador.bodyType = RigidbodyType2D.Dynamic;

        // ✅ RESETEAR VELOCIDAD Y RESTAURAR GRAVEDAD
        rbJugador.velocity = Vector2.zero;
        rbJugador.angularVelocity = 0f;
        rbJugador.gravityScale = gravedadOriginal;

        Debug.Log($"✅ Gravedad restaurada a: {gravedadOriginal}");
        Debug.Log($"✅ Velocidad reseteada: {rbJugador.velocity}");
        Debug.Log($"✅ BodyType FORZADO a: {rbJugador.bodyType}");

        // ✅ VERIFICAR COLLIDERS
        Collider2D[] collidersFinales = jugador.GetComponents<Collider2D>();
        foreach (Collider2D col in collidersFinales)
        {
            if (col.enabled)
            {
                Debug.Log($"✅ Collider activo: {col.GetType().Name} - IsTrigger: {col.isTrigger}");
            }
        }

        Debug.Log($"✅ Layer del jugador: {LayerMask.LayerToName(jugador.layer)} ({jugador.layer})");

        // ✅ FORZAR ACTUALIZACIÓN DE FÍSICA
        Physics2D.SyncTransforms();

        // Pequeña pausa
        yield return new WaitForSeconds(0.1f);

        // ✅ VERIFICAR QUE SIGA EN DYNAMIC ANTES DE REACTIVAR PLAYERCONTROLLER
        Debug.Log($"🔍 BodyType antes de reactivar: {rbJugador.bodyType}");

        // Reactivar PlayerController
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("✅ PlayerController REACTIVADO");
        }

        // ✅ VERIFICAR DE NUEVO DESPUÉS DE REACTIVAR
        yield return new WaitForSeconds(0.1f);

        Debug.Log($"🔍 BodyType DESPUÉS de reactivar PlayerController: {rbJugador.bodyType}");

        // ✅ SI SE CAMBIÓ A KINEMATIC, FORZARLO DE VUELTA A DYNAMIC
        if (rbJugador.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning($"⚠️ PlayerController cambió bodyType a {rbJugador.bodyType}! Forzando a Dynamic...");
            rbJugador.bodyType = RigidbodyType2D.Dynamic;
            rbJugador.gravityScale = gravedadOriginal;
        }

        Debug.Log($"✅ Estado final - BodyType: {rbJugador.bodyType}, Gravity: {rbJugador.gravityScale}");

        yaRespondio = false;

        Debug.Log("✅ FLOTACIÓN TERMINADA - Control devuelto");
    }

    IEnumerator CerrarPanelesDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        if (panelPreguntaFlotante != null) panelPreguntaFlotante.SetActive(false);
        if (panelResultadoFlotante != null) panelResultadoFlotante.SetActive(false);
    }

    public void DetenerModoFlotante()
    {
        if (modoFlotante && rbJugador != null)
        {
            rbJugador.gravityScale = gravedadOriginal;

            if (playerController != null)
            {
                playerController.enabled = true;
            }

            modoFlotante = false;
            yaRespondio = false;
            Debug.Log("🛑 Modo flotante detenido manualmente");
        }
    }
}