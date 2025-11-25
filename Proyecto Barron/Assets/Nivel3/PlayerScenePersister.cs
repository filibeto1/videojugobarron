using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerScenePersister : MonoBehaviour
{
    public static PlayerScenePersister Instance;

    private GameObject _playerPrefab;
    public GameObject playerPrefab
    {
        get
        {
            if (_playerPrefab != null)
                return _playerPrefab;

            if (persistedPlayer != null)
                return persistedPlayer;

            if (GameManager.Instance != null)
            {
                Personaje personaje = GameManager.Instance.GetPersonajeSeleccionado();
                if (personaje != null && personaje.personajeJugable != null)
                {
                    return personaje.personajeJugable;
                }
            }

            return null;
        }
        set
        {
            _playerPrefab = value;
            Debug.Log($"✅ playerPrefab asignado manualmente: {(_playerPrefab != null ? _playerPrefab.name : "NULL")}");
        }
    }

    private GameObject persistedPlayer;
    private bool isInitializing = false;
    private int lastSelectedCharacter = -1;
    private bool isQuitting = false;

    void Awake()
    {
        Debug.Log("🔄 PlayerScenePersister Awake llamado");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PlayerScenePersister creado y persistente");

            // SUSCRIBIR EVENTOS UNA SOLA VEZ
            SceneManager.sceneLoaded -= OnSceneLoaded; // Limpiar primero por seguridad
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Debug.Log("⚠️ PlayerScenePersister duplicado - destruyendo copia");
            DestroyImmediate(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("🎮 PlayerScenePersister Start");

        // Inicializar jugador si estamos en una escena de juego
        if (IsGameScene(SceneManager.GetActiveScene().name))
        {
            StartCoroutine(InitializePlayerDelayed());
        }
    }

    void OnEnable()
    {
        Debug.Log("🔵 PlayerScenePersister habilitado");
    }

    void OnDisable()
    {
        Debug.Log("🔴 PlayerScenePersister deshabilitado");
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
        Debug.Log("🚪 PlayerScenePersister - Aplicación cerrada");
    }

    IEnumerator InitializePlayerDelayed()
    {
        if (isInitializing || isQuitting)
        {
            Debug.Log("⏳ PlayerScenePersister ya está inicializando o cerrando, esperando...");
            yield break;
        }

        isInitializing = true;
        Debug.Log("🔄 Iniciando creación de jugador con retraso...");

        // ESPERAR MÁS TIEMPO PARA ESTABILIDAD
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        if (isQuitting)
        {
            isInitializing = false;
            yield break;
        }

        InitializePlayer();

        isInitializing = false;
        Debug.Log("✅ Proceso de inicialización de jugador completado");
    }

    void InitializePlayer()
    {
        Debug.Log("🔍 Buscando jugador existente...");

        // ✅ PRIMERO LIMPIAR JUGADORES VIEJOS
        LimpiarJugadoresViejos();

        GameObject existingPlayer = FindPersistedPlayer();

        if (existingPlayer != null)
        {
            Debug.Log($"✅ Jugador persistido encontrado: {existingPlayer.name}");
            persistedPlayer = existingPlayer;

            // ✅ VERIFICACIÓN MEJORADA - No destruir si el nombre es genérico
            if (GameManager.Instance != null && !existingPlayer.name.Contains("Emergency"))
            {
                Personaje personajeActual = GameManager.Instance.GetPersonajeSeleccionado();
                if (personajeActual != null && !persistedPlayer.name.Contains(personajeActual.nombre))
                {
                    Debug.Log($"🔄 Jugador viejo no coincide. Actualizando: {persistedPlayer.name} -> {personajeActual.nombre}");

                    // ✅ EN LUGAR DE DESTRUIR, ACTUALIZAR EL PREFAB MANTENIENDO LA INSTANCIA
                    ActualizarJugadorExistente(existingPlayer, personajeActual);
                    return;
                }
            }

            SetupPlayerSafely(persistedPlayer);
            return;
        }

        Debug.Log("🎮 No se encontró jugador persistido, creando nuevo...");
        CrearJugadorDesdeSeleccion();
    }
    // ✅ MÉTODO CORREGIDO - No intenta destruir el prefab, simplemente crea uno nuevo
    void ActualizarJugadorExistente(GameObject existingPlayer, Personaje nuevoPersonaje)
    {
        if (existingPlayer == null || nuevoPersonaje == null || nuevoPersonaje.personajeJugable == null)
        {
            Debug.LogError("❌ No se puede actualizar jugador - parámetros inválidos");
            return;
        }

        Debug.Log($"🔄 El jugador '{existingPlayer.name}' no coincide con '{nuevoPersonaje.nombre}'. Creando nuevo jugador...");

        // ✅ NO INTENTAR DESTRUIR - Simplemente crear el nuevo jugador
        // El viejo es el prefab original del proyecto y no se debe/puede destruir

        Vector3 spawnPosition = FindSpawnPosition();
        persistedPlayer = Instantiate(nuevoPersonaje.personajeJugable, spawnPosition, Quaternion.identity);
        persistedPlayer.name = "Player_" + nuevoPersonaje.nombre;
        persistedPlayer.tag = "Player";
        DontDestroyOnLoad(persistedPlayer);

        Debug.Log($"✅ NUEVO JUGADOR CREADO Y PERSISTIDO: {persistedPlayer.name}");
        SetupPlayerSafely(persistedPlayer);
    }
    // ✅ NUEVO MÉTODO: Limpiar jugadores viejos
    void LimpiarJugadoresViejos()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject jugador in jugadores)
        {
            if (jugador != persistedPlayer && jugador.scene.buildIndex != -1)
            {
                Debug.Log($"🗑️ Destruyendo jugador viejo: {jugador.name}");
                DestroyImmediate(jugador);
            }
        }
    }

    GameObject FindPersistedPlayer()
    {
        // Buscar por tag primero
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.scene.buildIndex == -1)
        {
            Debug.Log($"✅ Jugador persistido encontrado por tag: {player.name}");
            return player;
        }

        // Buscar en DontDestroyOnLoad
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Player") && obj.scene.buildIndex == -1)
            {
                Debug.Log($"✅ Jugador con tag encontrado en DontDestroyOnLoad: {obj.name}");
                return obj;
            }
        }

        Debug.Log("🔍 No se encontró jugador persistido");
        return null;
    }
    Vector3 FindSpawnPosition()
    {
        try
        {
            Debug.Log("🔍 INICIANDO BÚSQUEDA DE SPAWN POINT...");
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"🎯 Escena actual: {currentScene}");

            // ✅ 1. BUSCAR POR TAG (MÁS CONFIABLE - PRIORIDAD MÁXIMA)
            GameObject spawnByTag = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (spawnByTag != null)
            {
                Debug.Log($"🎉 SPAWNPOINT ENCONTRADO POR TAG: '{spawnByTag.name}'");
                Debug.Log($"📍 POSICIÓN EXACTA: X={spawnByTag.transform.position.x}, Y={spawnByTag.transform.position.y}, Z={spawnByTag.transform.position.z}");
                return spawnByTag.transform.position;
            }
            else
            {
                Debug.LogError("❌ CRÍTICO: No se encontró objeto con tag 'SpawnPoint'");
                Debug.Log("💡 SOLUCIÓN: Asigna el tag 'SpawnPoint' al objeto SpawnPoint en el editor");
            }

            // ✅ 2. DIAGNÓSTICO DETALLADO - Listar TODOS los objetos con tags
            Debug.Log("🔍 DIAGNÓSTICO: Listando todos los objetos con tags...");
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (!string.IsNullOrEmpty(obj.tag) && obj.tag != "Untagged")
                {
                    Debug.Log($"🏷️ Objeto con tag: '{obj.name}' -> Tag: '{obj.tag}'");
                }
            }

            // ✅ 3. BUSCAR POR NOMBRE EXACTO
            GameObject spawnByName = GameObject.Find("SpawnPoint");
            if (spawnByName != null)
            {
                Debug.Log($"📍 SpawnPoint encontrado por nombre: '{spawnByName.name}'");
                Debug.Log($"📍 Posición: {spawnByName.transform.position}");
                return spawnByName.transform.position;
            }

            // ✅ 4. BUSCAR OBJETOS POR NOMBRE PARCIAL
            Debug.Log("🔍 Buscando objetos con 'spawn' en el nombre...");
            bool foundAnySpawn = false;
            foreach (GameObject obj in allObjects)
            {
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("spawn") || lowerName.Contains("inicio") || lowerName.Contains("start"))
                {
                    Debug.Log($"📍 Objeto encontrado: '{obj.name}' en posición {obj.transform.position}");
                    foundAnySpawn = true;

                    // Si encontramos uno con "spawn" exacto, usarlo
                    if (lowerName.Contains("spawn"))
                    {
                        Debug.Log($"🎉 Usando '{obj.name}' como spawn point");
                        return obj.transform.position;
                    }
                }
            }

            if (foundAnySpawn)
            {
                Debug.Log("💡 Se encontraron objetos potenciales pero ninguno con 'spawn' exacto");
            }

            // ✅ 5. POSICIÓN FIJA PARA NIVEL2 (ÚLTIMO RECURSO)
            Debug.LogWarning("⚠️ Usando posición fija para Nivel2 como fallback");
            if (currentScene == "Nivel2")
            {
                Vector3 fixedPosition = new Vector3(5f, -54.3f, 0.30175f);
                Debug.Log($"📍 Posición fija usada: {fixedPosition}");
                return fixedPosition;
            }

            Debug.LogError("🚨 NO SE ENCONTRÓ SPAWN POINT - Usando (0,0,0)");
            return Vector3.zero;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ERROR CRÍTICO buscando spawn point: {e.Message}");
            Debug.LogError($"📋 StackTrace: {e.StackTrace}");
            return Vector3.zero;
        }
    }

    void CrearJugadorDesdeSeleccion()
    {
        if (isQuitting) return;

        Debug.Log("🎮 Creando jugador desde selección del GameManager");

        // Verificar que GameManager existe
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager no encontrado - No se puede crear jugador");
            CreateEmergencyPlayer();
            return;
        }

        // Obtener el personaje seleccionado
        Personaje personajeSeleccionado = GameManager.Instance.GetPersonajeSeleccionado();

        if (personajeSeleccionado == null)
        {
            Debug.LogError("❌ No hay personaje seleccionado en GameManager");
            CreateEmergencyPlayer();
            return;
        }

        // Verificar que el personaje tiene un prefab asignado
        if (personajeSeleccionado.personajeJugable == null)
        {
            Debug.LogError($"❌ El personaje '{personajeSeleccionado.nombre}' no tiene prefab asignado");
            CreateEmergencyPlayer();
            return;
        }

        // Obtener posición de spawn
        Vector3 spawnPosition = FindSpawnPosition();

        // Crear el jugador desde el prefab del personaje seleccionado
        persistedPlayer = Instantiate(personajeSeleccionado.personajeJugable, spawnPosition, Quaternion.identity);
        persistedPlayer.name = "Player_" + personajeSeleccionado.nombre;
        persistedPlayer.tag = "Player";
        DontDestroyOnLoad(persistedPlayer);

        // Actualizar última selección
        lastSelectedCharacter = GameManager.Instance.GetJugadorSeleccionado();

        Debug.Log($"✅ JUGADOR CREADO: {persistedPlayer.name} ({personajeSeleccionado.nombre}) en {spawnPosition}");
        SetupPlayerSafely(persistedPlayer);
    }

    [ContextMenu("🚨 SOLUCIÓN URGENTE: Crear jugador manualmente")]
    public void CrearJugadorUrgente()
    {
        if (isQuitting) return;

        Debug.Log("🚨 EJECUTANDO SOLUCIÓN URGENTE - CREANDO JUGADOR MANUALMENTE");

        // Verificar si ya existe
        GameObject jugadorExistente = GameObject.FindGameObjectWithTag("Player");
        if (jugadorExistente != null && jugadorExistente.scene.buildIndex == -1)
        {
            Debug.Log($"✅ Jugador ya existe y es persistente: {jugadorExistente.name}");
            persistedPlayer = jugadorExistente;
            return;
        }

        // Limpiar y crear nuevo
        LimpiarJugadoresViejos();
        CrearJugadorDesdeSeleccion();
    }

    void CreateEmergencyPlayer()
    {
        Debug.LogWarning("🚨 CREANDO JUGADOR DE EMERGENCIA");

        persistedPlayer = new GameObject("Player1_Emergency");
        persistedPlayer.tag = "Player";

        persistedPlayer.AddComponent<Rigidbody2D>();
        persistedPlayer.AddComponent<BoxCollider2D>();

        persistedPlayer.transform.position = Vector3.zero;
        DontDestroyOnLoad(persistedPlayer);

        Debug.Log("✅ Jugador de emergencia creado");
    }

    void SetupPlayerSafely(GameObject player)
    {
        if (player == null || isQuitting)
        {
            Debug.LogError("❌ SetupPlayerSafely: player es NULL o aplicación cerrando");
            return;
        }

        Debug.Log($"🛠️ Configurando jugador: {player.name}");

        // ACTIVAR SI ESTÁ DESACTIVADO
        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
            Debug.Log("🔓 Jugador activado (estaba desactivado)");
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;

            Debug.Log($"🛑 Velocidad forzada a cero en: {player.name}");
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.controlEnabled = false;
            Debug.Log("🎮 PlayerController encontrado - controles desactivados temporalmente");
            StartCoroutine(EnableControlsAfterDelay(pc));
        }
    }

    IEnumerator EnableControlsAfterDelay(PlayerController pc)
    {
        yield return new WaitForSeconds(0.5f);

        if (pc != null && !isQuitting)
        {
            pc.controlEnabled = true;
            Debug.Log("✅ Controles del jugador activados");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isQuitting) return;

        Debug.Log($"📍 PlayerScenePersister: Escena cargada - {scene.name}");

        // ✅ SOLUCIÓN: Desactivar completamente en Nivel2 1
        if (scene.name == "Nivel2 1")
        {
            Debug.Log("🚫 PlayerScenePersister DESACTIVADO en Nivel2 1 - InicioJugador manejará el jugador");
            if (persistedPlayer != null)
            {
                persistedPlayer.SetActive(false); // Ocultar jugador persistente
            }
            this.enabled = false; // Desactivar este script
            return;
        }

        if (IsGameScene(scene.name))
        {
            Debug.Log("🎮 Escena de juego detectada - Inicializando jugador...");
            StartCoroutine(InitializePlayerDelayed());
        }
        else
        {
            // ✅ EN MENÚ: Ocultar jugador pero mantenerlo
            if (persistedPlayer != null)
            {
                persistedPlayer.SetActive(false);
                Debug.Log("📋 Jugador ocultado en menú");
            }
        }
    }
    bool IsGameScene(string sceneName)
    {
        return sceneName.Contains("Nivel") ||
               sceneName.Contains("Level") ||
               sceneName == "SampleScene" ||
               sceneName.Contains("Game");
    }

    IEnumerator RepositionPlayerSafely()
    {
        if (isQuitting || persistedPlayer == null) yield break;

        Debug.Log("🔄 Reposicionando jugador en nueva escena...");

        yield return new WaitForSeconds(0.3f);

        Vector3 spawnPosition = FindSpawnPosition();

        Rigidbody2D rb = persistedPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        persistedPlayer.transform.position = spawnPosition;

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log($"📍 Player reposicionado en: {spawnPosition}");
    }

    public GameObject GetPlayer()
    {
        if (persistedPlayer == null && !isQuitting)
        {
            Debug.LogWarning("⚠️ GetPlayer(): persistedPlayer es NULL, buscando en escena...");
            persistedPlayer = GameObject.FindGameObjectWithTag("Player");
        }
        return persistedPlayer;
    }

    public void EnsurePlayerExists()
    {
        if (isQuitting) return;

        Debug.Log("🔍 EnsurePlayerExists llamado");

        if (persistedPlayer == null && !isInitializing)
        {
            Debug.Log("🔄 Solicitando recreación de jugador...");
            StartCoroutine(InitializePlayerDelayed());
        }
    }

    public void ForceRecreatePlayer()
    {
        if (isQuitting) return;

        Debug.Log("🚨 FORZANDO RECREACIÓN DE JUGADOR");

        if (persistedPlayer != null)
        {
            DestroyImmediate(persistedPlayer);
            persistedPlayer = null;
        }

        LimpiarJugadoresViejos();
        StartCoroutine(InitializePlayerDelayed());
    }

    void OnDestroy()
    {
        if (!isQuitting)
        {
            Debug.Log("🗑️ PlayerScenePersister destruido - Esto NO debería pasar durante el juego!");
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}