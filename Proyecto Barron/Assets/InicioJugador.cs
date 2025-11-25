using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InicioJugador : MonoBehaviour
{
    [Header("Prefabs de Jugadores")]
    public GameObject player1Prefab; // Prefab del Player (Cat)
    public GameObject player2Prefab; // Prefab del Player2 (Dog)

    [Header("Configuración de Spawn")]
    public Transform puntoSpawnJugador; // Donde aparecerá el jugador
    public Vector3 posicionSpawnPorDefecto = new Vector3(0, 0, 0);

    [Header("Debug")]
    public bool mostrarDebug = true;

    private GameObject jugadorInstanciado;

    void Start()
    {
        // ✅ DIAGNÓSTICO DETALLADO AL INICIO
        Debug.Log("🎮 ===== DIAGNÓSTICO INICIOJUGADOR =====");
        Debug.Log($"🔍 Player1Prefab: {(player1Prefab != null ? $"✅ ASIGNADO - {player1Prefab.name}" : "❌ NULL")}");
        Debug.Log($"🔍 Player2Prefab: {(player2Prefab != null ? $"✅ ASIGNADO - {player2Prefab.name}" : "❌ NULL")}");
        Debug.Log($"📍 PuntoSpawn: {(puntoSpawnJugador != null ? $"✅ ASIGNADO - {puntoSpawnJugador.position}" : "❌ NULL")}");
        Debug.Log($"🎯 Escena actual: {SceneManager.GetActiveScene().name}");

        // Verificar si los prefabs tienen componentes esenciales
        if (player1Prefab != null)
        {
            var rb1 = player1Prefab.GetComponent<Rigidbody2D>();
            var col1 = player1Prefab.GetComponent<Collider2D>();
            var sprite1 = player1Prefab.GetComponent<SpriteRenderer>();
            Debug.Log($"🔧 Player1 - Rigidbody2D: {(rb1 != null ? "✅" : "❌")}, Collider2D: {(col1 != null ? "✅" : "❌")}, SpriteRenderer: {(sprite1 != null ? "✅" : "❌")}");
        }

        if (player2Prefab != null)
        {
            var rb2 = player2Prefab.GetComponent<Rigidbody2D>();
            var col2 = player2Prefab.GetComponent<Collider2D>();
            var sprite2 = player2Prefab.GetComponent<SpriteRenderer>();
            Debug.Log($"🔧 Player2 - Rigidbody2D: {(rb2 != null ? "✅" : "❌")}, Collider2D: {(col2 != null ? "✅" : "❌")}, SpriteRenderer: {(sprite2 != null ? "✅" : "❌")}");
        }
        Debug.Log("=====================================");

        string currentScene = SceneManager.GetActiveScene().name;

        // ✅ NO ejecutar en escenas con GameModeSelector
        if (currentScene == "Nivel2 1" || currentScene == "Nivel3")
        {
            Debug.Log($"🚫 InicioJugador DESHABILITADO en {currentScene} - GameModeSelector manejará los jugadores");
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"🎮 INICIOJUGADOR - TOMANDO CONTROL en {currentScene}");

        // ✅ SOLUCIÓN DEFINITIVA: InicioJugador SIEMPRE toma control
        ForzarControlCompleto();

        CreateQuestionUI();
    }

    void ForzarControlCompleto()
    {
        // 1. DESTRUIR PlayerScenePersister SIEMPRE
        PlayerScenePersister persister = FindObjectOfType<PlayerScenePersister>();
        if (persister != null)
        {
            Debug.Log("🗑️ DESTRUYENDO PlayerScenePersister - InicioJugador toma control");
            DestroyImmediate(persister.gameObject);
        }

        // 2. LIMPIAR todos los jugadores existentes
        LimpiarJugadoresExistentes();

        // 3. CREAR NUEVO JUGADOR
        CrearJugadorDesdeCero();
    }

    void LimpiarJugadoresExistentes()
    {
        GameObject[] todosLosJugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject jugador in todosLosJugadores)
        {
            Debug.Log($"🗑️ Eliminando jugador: {jugador.name}");
            DestroyImmediate(jugador);
        }

        // Limpiar también de DontDestroyOnLoad
        GameObject[] todosLosObjetos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in todosLosObjetos)
        {
            if (obj.CompareTag("Player") && obj.scene.buildIndex == -1)
            {
                Debug.Log($"🗑️ Eliminando jugador persistente: {obj.name}");
                DestroyImmediate(obj);
            }
        }
    }

    void CrearJugadorDesdeCero()
    {
        Debug.Log("🔄 Creando jugador desde cero...");

        // ✅ VERIFICACIÓN EXTRA DE PREFABS
        if (player1Prefab == null)
        {
            Debug.LogError("❌ Player1Prefab es NULL - Intentando cargar desde recursos...");
            player1Prefab = Resources.Load<GameObject>("Player");
        }

        if (player2Prefab == null)
        {
            Debug.LogError("❌ Player2Prefab es NULL - Intentando cargar desde recursos...");
            player2Prefab = Resources.Load<GameObject>("Player2");
        }

        // Obtener GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("❌ No se encontró GameManager!");
            // CREAR JUGADOR DE EMERGENCIA
            CrearJugadorEmergencia();
            return;
        }

        // Determinar prefab a usar
        GameObject prefabAUsar = null;
        int indexSeleccionado = gm.jugadorSeleccionado;

        Debug.Log($"🎯 Índice seleccionado: {indexSeleccionado}");

        if (indexSeleccionado == 0)
        {
            if (player1Prefab != null)
            {
                prefabAUsar = player1Prefab;
                Debug.Log("🐱 Usando Player1 (Cat)");
            }
            else
            {
                Debug.LogError("❌ Player1Prefab no asignado! Usando emergencia");
                CrearJugadorEmergencia();
                return;
            }
        }
        else if (indexSeleccionado == 1)
        {
            if (player2Prefab != null)
            {
                prefabAUsar = player2Prefab;
                Debug.Log("🐶 Usando Player2 (Dog)");
            }
            else
            {
                Debug.LogError("❌ Player2Prefab no asignado! Usando emergencia");
                CrearJugadorEmergencia();
                return;
            }
        }

        if (prefabAUsar == null)
        {
            Debug.LogError("❌ No hay prefab válido! Usando emergencia");
            CrearJugadorEmergencia();
            return;
        }

        // Posición de spawn
        Vector3 posicionSpawn = puntoSpawnJugador != null ?
            puntoSpawnJugador.position : posicionSpawnPorDefecto;

        Debug.Log($"📍 Spawn en: {posicionSpawn}");

        // INSTANCIAR JUGADOR
        jugadorInstanciado = Instantiate(prefabAUsar, posicionSpawn, Quaternion.identity);

        if (jugadorInstanciado == null)
        {
            Debug.LogError("❌ Error al instanciar jugador! Usando emergencia");
            CrearJugadorEmergencia();
            return;
        }

        // Configurar jugador
        jugadorInstanciado.tag = "Player";
        jugadorInstanciado.name = "Jugador_" + (indexSeleccionado == 0 ? "Cat" : "Dog");

        // ✅ FORZAR ACTIVACIÓN
        if (!jugadorInstanciado.activeInHierarchy)
        {
            jugadorInstanciado.SetActive(true);
            Debug.Log("🔓 Jugador activado (estaba desactivado)");
        }

        Debug.Log($"✅ JUGADOR CREADO: {jugadorInstanciado.name} en posición: {jugadorInstanciado.transform.position}");

        // Notificar sistemas
        NotificarJugadorCreado();
    }

    // ✅ MÉTODO FALTANTE: Crear jugador de emergencia
    void CrearJugadorEmergencia()
    {
        Debug.LogWarning("🚨 CREANDO JUGADOR DE EMERGENCIA");

        jugadorInstanciado = new GameObject("Jugador_Emergencia");
        jugadorInstanciado.tag = "Player";

        // Agregar componentes básicos
        Rigidbody2D rb = jugadorInstanciado.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Para evitar que caiga
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var collider = jugadorInstanciado.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1, 1);

        // Agregar sprite (crear uno básico)
        var spriteRenderer = jugadorInstanciado.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;

        // Crear una textura blanca básica para el sprite de emergencia
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        spriteRenderer.transform.localScale = new Vector3(1, 1, 1);

        // Posición
        Vector3 posicionSpawn = puntoSpawnJugador != null ?
            puntoSpawnJugador.position : posicionSpawnPorDefecto;
        jugadorInstanciado.transform.position = posicionSpawn;

        Debug.Log($"✅ JUGADOR EMERGENCIA CREADO en {posicionSpawn}");
        NotificarJugadorCreado();
    }

    void NotificarJugadorCreado()
    {
        if (jugadorInstanciado == null) return;

        Debug.Log("📢 Notificando sistemas del jugador...");

        // Cámara
        SeguirJugador[] camaras = FindObjectsOfType<SeguirJugador>();
        foreach (SeguirJugador cam in camaras)
        {
            cam.SetPlayerTarget(jugadorInstanciado.transform);
            Debug.Log($"📷 Cámara '{cam.name}' configurada");
        }

        // ControladorTiempo
        ControladorTiempo controlador = FindObjectOfType<ControladorTiempo>();
        if (controlador != null)
        {
            var campo = controlador.GetType().GetField("jugador",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campo != null)
            {
                campo.SetValue(controlador, jugadorInstanciado);
                Debug.Log("⏱️ ControladorTiempo notificado");
            }
        }
    }

    void CreateQuestionUI()
    {
        // Verificar si ya existe la UI
        if (GameObject.Find("QuestionCanvas") != null)
        {
            Debug.Log("✅ La UI de preguntas ya existe (InicioJugador)");
            return;
        }

        Debug.Log("🔄 Creando UI de preguntas desde InicioJugador...");

        // Crear Canvas
        GameObject canvasGO = new GameObject("QuestionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Crear Panel
        GameObject panelGO = new GameObject("QuestionPanel");
        panelGO.transform.SetParent(canvasGO.transform);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Configurar RectTransform del panel
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.2f, 0.3f);
        panelRT.anchorMax = new Vector2(0.8f, 0.7f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Crear Texto de Pregunta
        GameObject questionGO = new GameObject("QuestionText");
        questionGO.transform.SetParent(panelGO.transform);
        TMP_Text questionText = questionGO.AddComponent<TextMeshProUGUI>();
        questionText.text = "Pregunta de matemáticas";
        questionText.color = Color.white;
        questionText.fontSize = 24;
        questionText.alignment = TextAlignmentOptions.Center;

        RectTransform questionRT = questionGO.GetComponent<RectTransform>();
        questionRT.anchorMin = new Vector2(0.1f, 0.6f);
        questionRT.anchorMax = new Vector2(0.9f, 0.9f);
        questionRT.offsetMin = Vector2.zero;
        questionRT.offsetMax = Vector2.zero;

        // Crear Input Field
        GameObject inputGO = new GameObject("AnswerInput");
        inputGO.transform.SetParent(panelGO.transform);

        // Agregar Image PRIMERO
        Image inputImage = inputGO.AddComponent<Image>();
        inputImage.color = Color.white;

        // Agregar el TMP_InputField
        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        // Configurar RectTransform del input
        RectTransform inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.2f, 0.3f);
        inputRT.anchorMax = new Vector2(0.8f, 0.5f);
        inputRT.offsetMin = Vector2.zero;
        inputRT.offsetMax = Vector2.zero;

        // Crear GameObject para el texto del placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputGO.transform);
        TMP_Text placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Escribe tu respuesta...";
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.fontStyle = FontStyles.Italic;

        RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = new Vector2(10, 2);
        placeholderRT.offsetMax = new Vector2(-10, -2);

        // Crear GameObject para el texto de entrada
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform);
        TMP_Text inputText = textGO.AddComponent<TextMeshProUGUI>();
        inputText.color = Color.black;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 2);
        textRT.offsetMax = new Vector2(-10, -2);

        // Configurar el InputField
        inputField.placeholder = placeholderText;
        inputField.textComponent = inputText;

        // Crear Botón
        GameObject buttonGO = new GameObject("SubmitButton");
        buttonGO.transform.SetParent(panelGO.transform);
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f);
        Button button = buttonGO.AddComponent<Button>();

        // Configurar RectTransform del botón
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.3f, 0.1f);
        buttonRT.anchorMax = new Vector2(0.7f, 0.2f);
        buttonRT.offsetMin = Vector2.zero;
        buttonRT.offsetMax = Vector2.zero;

        // Texto del botón
        GameObject buttonTextGO = new GameObject("ButtonText");
        buttonTextGO.transform.SetParent(buttonGO.transform);
        TMP_Text buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = "ENVIAR RESPUESTA";
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform buttonTextRT = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRT.anchorMin = Vector2.zero;
        buttonTextRT.anchorMax = Vector2.one;
        buttonTextRT.offsetMin = Vector2.zero;
        buttonTextRT.offsetMax = Vector2.zero;

        // Asignar al MathQuestionManager
        MathQuestionManager questionManager = FindObjectOfType<MathQuestionManager>();
        if (questionManager != null)
        {
            questionManager.questionPanel = panelGO;
            questionManager.questionText = questionText;
            questionManager.answerInput = inputField;
            questionManager.submitButton = button;
            Debug.Log("✅ UI creada y asignada automáticamente a MathQuestionManager (InicioJugador)");
        }

        // Desactivar panel inicialmente
        panelGO.SetActive(false);
    }

    // MÉTODO PÚBLICO - Para que otros scripts obtengan el jugador
    public GameObject GetJugador()
    {
        return jugadorInstanciado;
    }

    // DIAGNÓSTICO - Para debugging
    [ContextMenu("🔧 Diagnóstico Completo")]
    void DiagnosticoCompleto()
    {
        Debug.Log("=== DIAGNÓSTICO INICIO JUGADOR ===");
        Debug.Log($"🎮 Jugador instanciado: {(jugadorInstanciado != null ? jugadorInstanciado.name : "❌ NULL")}");
        Debug.Log($"📦 Player1 Prefab: {(player1Prefab != null ? "✅ Asignado" : "❌ NULL")}");
        Debug.Log($"📦 Player2 Prefab: {(player2Prefab != null ? "✅ Asignado" : "❌ NULL")}");
        Debug.Log($"📍 Punto Spawn: {(puntoSpawnJugador != null ? puntoSpawnJugador.position.ToString() : "Usando posición por defecto")}");

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            int indice = gm.jugadorSeleccionado;
            string nombrePersonaje = indice == 0 ? "Cat (Player1)" : "Dog (Player2)";
            Debug.Log($"🎯 Personaje seleccionado en GameManager: Índice {indice} = {nombrePersonaje}");
        }
        else
        {
            Debug.LogError("❌ No se encontró GameManager en la escena!");
        }

        GameObject[] jugadoresEnEscena = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"👥 Jugadores con tag 'Player' en escena: {jugadoresEnEscena.Length}");
        foreach (GameObject j in jugadoresEnEscena)
        {
            Debug.Log($"   - {j.name} en posición {j.transform.position} (Activo: {j.activeInHierarchy})");
        }

        // Verificar PlayerScenePersister
        PlayerScenePersister persister = FindObjectOfType<PlayerScenePersister>();
        Debug.Log($"🔄 PlayerScenePersister: {(persister != null ? "❌ PRESENTE (debería ser destruido)" : "✅ AUSENTE (correcto)")}");

        Debug.Log("==================================");
    }

    [ContextMenu("🚨 SOLUCIÓN DE EMERGENCIA")]
    void SolucionEmergencia()
    {
        Debug.Log("🚨 EJECUTANDO SOLUCIÓN DE EMERGENCIA");

        // Destruir TODO
        PlayerScenePersister[] persisters = FindObjectsOfType<PlayerScenePersister>();
        foreach (var p in persisters)
        {
            Debug.Log($"🗑️ Destruyendo PlayerScenePersister: {p.name}");
            DestroyImmediate(p.gameObject);
        }

        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (var j in jugadores)
        {
            Debug.Log($"🗑️ Destruyendo jugador: {j.name}");
            DestroyImmediate(j);
        }

        // Crear desde cero
        ForzarControlCompleto();
        Debug.Log("✅ Solución de emergencia completada");
    }

    // Método para forzar recreación desde otros scripts
    public void RecrearJugador()
    {
        Debug.Log("🔄 Recreando jugador por solicitud externa...");

        if (jugadorInstanciado != null)
        {
            DestroyImmediate(jugadorInstanciado);
        }

        ForzarControlCompleto();
    }
}