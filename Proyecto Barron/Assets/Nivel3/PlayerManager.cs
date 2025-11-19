using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public Transform[] spawnPoints;

    [Header("Camera Settings")]
    public Camera mainCamera;

    [Header("Debug")]
    public List<GameObject> players = new List<GameObject>();

    private bool isInitialized = false;
    public static PlayerManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PlayerManager creado y persistente");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("🔄 PlayerManager iniciando automáticamente...");

        if (!isInitialized)
        {
            StartCoroutine(InitializeWithDelay());
        }
    }

    public IEnumerator InitializeWithDelay()
    {
        Debug.Log("🔄 Inicializando PlayerManager con delay...");
        yield return new WaitForSeconds(0.1f);

        if (!isInitialized)
        {
            FindExistingPlayers();
            SetupPlayers();
            SetupCamera();
            isInitialized = true;
            InitializeSuccess();
        }
    }

    void FindExistingPlayers()
    {
        Debug.Log("Buscando jugadores en la escena...");

        // Buscar por tag Player
        GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"Encontrados {playersInScene.Length} objetos con tag 'Player'");

        foreach (GameObject player in playersInScene)
        {
            if (player != null && player.activeInHierarchy)
            {
                Debug.Log($"✅ Jugador encontrado: {player.name}");

                // ✅ VERIFICAR Y CORREGIR ESCALA
                if (player.transform.localScale.magnitude < 1f)
                {
                    Debug.LogWarning($"⚠️ {player.name} tiene escala pequeña: {player.transform.localScale}. Corrigiendo...");
                    player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }

                players.Add(player);
            }
        }

        // ✅ CORREGIDO: Usar nombre diferente para la variable de bots
        GameObject[] botObjects = GameObject.FindGameObjectsWithTag("Bot");
        Debug.Log($"Encontrados {botObjects.Length} objetos con tag 'Bot'");

        foreach (GameObject bot in botObjects)
        {
            if (bot != null && bot.activeInHierarchy)
            {
                Debug.Log($"✅ Bot encontrado: {bot.name}");

                // ✅ CORREGIR ESCALA Y POSICIÓN Z DEL BOT
                if (bot.transform.localScale.magnitude < 1f)
                {
                    Debug.LogWarning($"⚠️ {bot.name} tiene escala pequeña: {bot.transform.localScale}. Corrigiendo...");
                    bot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }

                // ✅ CORREGIR POSICIÓN Z
                Vector3 botPos = bot.transform.position;
                if (botPos.z != 0)
                {
                    Debug.LogWarning($"⚠️ {bot.name} tiene posición Z incorrecta: {botPos.z}. Corrigiendo a 0...");
                    botPos.z = 0f;
                    bot.transform.position = botPos;
                }

                players.Add(bot);
            }
        }

        if (players.Count == 0)
        {
            Debug.LogError("❌ No se encontraron jugadores en la escena!");
        }
    }

    void SetupPlayers()
    {
        Debug.Log($"🎮 Configurando {players.Count} jugador(es)...");

        // Si no hay jugadores, crear según el modo
        if (players.Count == 0)
        {
            // ✅ CORREGIDO: Usar GameModeSelector en lugar de GameSettings
            GameModeSelector gameModeSelector = FindObjectOfType<GameModeSelector>();
            bool isTwoPlayerMode = false;

            if (gameModeSelector != null)
            {
                isTwoPlayerMode = gameModeSelector.IsTwoPlayerMode();
            }
            else
            {
                // Fallback: buscar en la escena o usar valor por defecto
                Debug.LogWarning("⚠️ GameModeSelector no encontrado, usando modo 1 jugador por defecto");
                isTwoPlayerMode = false;
            }

            if (!isTwoPlayerMode)
            {
                Debug.Log("🎯 Modo: Un jugador - Creando jugador y bot");
                CreatePlayer();
                CreateBot();
            }
            else
            {
                Debug.Log("🎯 Modo: Dos jugadores - Creando dos jugadores");
                CreatePlayer();
                CreateSecondPlayer();
            }
        }
        else
        {
            Debug.Log("✅ Jugadores ya existen en escena, usando los existentes");
        }
    }

    void CreatePlayer()
    {
        if (playerPrefab != null && spawnPoints.Length > 0)
        {
            GameObject player = Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.identity);
            player.name = "Player1";
            player.tag = "Player";

            // ✅ ASEGURAR ESCALA CORRECTA
            player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            players.Add(player);
            Debug.Log("✅ Player1 creado exitosamente");
        }
        else
        {
            Debug.LogError("❌ No se puede crear Player1: playerPrefab o spawnPoints no asignados");
        }
    }

    void CreateBot()
    {
        if (botPrefab != null && spawnPoints.Length > 1)
        {
            GameObject bot = Instantiate(botPrefab, spawnPoints[1].position, Quaternion.identity);
            bot.name = "Bot";
            bot.tag = "Bot";

            // ✅ ASEGURAR ESCALA CORRECTA DEL BOT
            bot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            // ✅ AGREGAR BotController SI NO EXISTE
            BotController botController = bot.GetComponent<BotController>();
            if (botController == null)
            {
                botController = bot.AddComponent<BotController>();
                Debug.Log("✅ BotController agregado al Bot");
            }

            players.Add(bot);
            Debug.Log("🤖 Bot creado exitosamente");
        }
        else
        {
            Debug.LogWarning("⚠️ No se pudo crear el bot - botPrefab o spawnPoints insuficientes");
        }
    }

    void CreateSecondPlayer()
    {
        if (playerPrefab != null && spawnPoints.Length > 1)
        {
            GameObject secondPlayer = Instantiate(playerPrefab, spawnPoints[1].position, Quaternion.identity);
            secondPlayer.name = "Player2";
            secondPlayer.tag = "Player";

            // ✅ ASEGURAR ESCALA CORRECTA
            secondPlayer.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            // ✅ AGREGAR BotController PARA CONTROL HUMANO
            BotController botController = secondPlayer.GetComponent<BotController>();
            if (botController == null)
            {
                botController = secondPlayer.AddComponent<BotController>();
                Debug.Log("✅ BotController agregado a Player2");
            }

            // ✅ CONFIGURAR COMO CONTROLADO POR JUGADOR
            botController.SetPlayerControl(true);

            players.Add(secondPlayer);
            Debug.Log("✅ Player2 creado exitosamente");
        }
        else
        {
            Debug.LogError("❌ No se puede crear Player2: playerPrefab o spawnPoints insuficientes");
        }
    }

    void SetupCamera()
    {
        Debug.Log($"📷 Configurando cámara para {players.Count} jugador(es)");

        // ✅ VERIFICAR SI HAY GameModeSelector PARA DETERMINAR MODO
        GameModeSelector gameModeSelector = FindObjectOfType<GameModeSelector>();
        bool isTwoPlayerMode = false;

        if (gameModeSelector != null)
        {
            isTwoPlayerMode = gameModeSelector.IsTwoPlayerMode();
        }

        if (isTwoPlayerMode && players.Count >= 2)
        {
            SetupSplitScreenCamera();
        }
        else
        {
            SetupSingleCamera();
        }
    }

    void SetupSingleCamera()
    {
        if (players.Count > 0 && players[0] != null)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindObjectOfType<Camera>();
                }
            }

            if (mainCamera == null)
            {
                Debug.LogError("❌ No se encontró la cámara principal!");
                return;
            }

            mainCamera.enabled = true;
            mainCamera.orthographicSize = 5f;

            // ✅ CONFIGURAR CameraFollow CORRECTAMENTE
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
                Debug.Log("📷 CameraFollow añadido a la cámara principal");
            }

            cameraFollow.target = players[0].transform;
            cameraFollow.smoothSpeed = 0.125f;
            cameraFollow.offset = new Vector3(0f, 2f, -10f); // ✅ Offset con altura para mejor vista
            cameraFollow.lookAtTarget = false; // ✅ Importante para 2D

            Debug.Log($"📷 Cámara configurada para {players[0].name}");

            // ✅ POSICIONAR CÁMARA INMEDIATAMENTE
            if (players[0] != null)
            {
                Vector3 desiredPosition = players[0].transform.position + cameraFollow.offset;
                desiredPosition.z = cameraFollow.offset.z;
                mainCamera.transform.position = desiredPosition;
            }
        }
        else
        {
            Debug.LogError("❌ No hay jugadores para configurar la cámara");
        }
    }

    void SetupSplitScreenCamera()
    {
        Debug.Log("🖥️ Configurando pantalla dividida para 2 jugadores");

        // ✅ BUSCAR O CREAR CÁMARAS PARA AMBOS JUGADORES
        Camera player1Camera = FindCameraForPlayer("Player1");
        Camera player2Camera = FindCameraForPlayer("Player2");

        if (player1Camera == null || player2Camera == null)
        {
            Debug.LogError("❌ No se pudieron encontrar/crear las cámaras para pantalla dividida");
            SetupSingleCamera(); // Fallback a cámara simple
            return;
        }

        // ✅ CONFIGURAR CÁMARA PLAYER 1 (PARTE SUPERIOR)
        player1Camera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
        player1Camera.tag = "MainCamera";

        CameraFollow follow1 = player1Camera.GetComponent<CameraFollow>();
        if (follow1 == null) follow1 = player1Camera.gameObject.AddComponent<CameraFollow>();
        follow1.target = players[0].transform;
        follow1.offset = new Vector3(0f, 2f, -10f);

        // ✅ CONFIGURAR CÁMARA PLAYER 2 (PARTE INFERIOR)
        player2Camera.rect = new Rect(0f, 0f, 1f, 0.5f);
        player2Camera.tag = "CameraP2";

        CameraFollow follow2 = player2Camera.GetComponent<CameraFollow>();
        if (follow2 == null) follow2 = player2Camera.gameObject.AddComponent<CameraFollow>();
        follow2.target = players[1].transform;
        follow2.offset = new Vector3(0f, 2f, -10f);

        Debug.Log("✅ Pantalla dividida configurada: Player1 (arriba), Player2 (abajo)");
    }

    Camera FindCameraForPlayer(string playerName)
    {
        // Buscar cámara existente por tag o nombre
        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera cam in cameras)
        {
            if (playerName == "Player1" && (cam.tag == "MainCamera" || cam.name.Contains("Player1")))
                return cam;
            if (playerName == "Player2" && (cam.tag == "CameraP2" || cam.name.Contains("Player2")))
                return cam;
        }

        // Si no se encuentra, crear nueva cámara
        GameObject cameraObj = new GameObject($"{playerName}Camera");
        Camera newCamera = cameraObj.AddComponent<Camera>();
        newCamera.orthographic = true;
        newCamera.orthographicSize = 5f;

        if (playerName == "Player1")
            newCamera.tag = "MainCamera";
        else
            newCamera.tag = "CameraP2";

        Debug.Log($"✅ Nueva cámara creada para {playerName}");
        return newCamera;
    }

    void InitializeSuccess()
    {
        Debug.Log($"🎯 PlayerManager inicializado con {players.Count} jugador(es)");

        // ✅ DEBUG: Mostrar información de todos los jugadores
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                string playerType = player.CompareTag("Bot") ? "Bot" : "Player";
                Debug.Log($"🎮 {player.name} ({playerType}) - Posición: {player.transform.position} - Escala: {player.transform.localScale}");
            }
        }
    }

    public void ForceFindPlayers()
    {
        Debug.Log("🔄 ForceFindPlayers llamado");

        players.Clear();
        isInitialized = false;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(InitializeWithDelay());
        }
    }

    // ✅ MÉTODO AGREGADO: Para solucionar el error en Checkpoint.cs
    public bool IsInitializationComplete()
    {
        return isInitialized;
    }

    // ✅ MÉTODO AGREGADO: Para compatibilidad
    public void ManualInitialize()
    {
        if (!isInitialized)
        {
            StartCoroutine(InitializeWithDelay());
        }
    }

    public List<GameObject> GetPlayers()
    {
        return players;
    }

    public GameObject GetPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
            return players[index];
        return null;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    // ✅ NUEVO MÉTODO: Para obtener jugador por tag
    public GameObject GetPlayerByTag(string tag)
    {
        foreach (GameObject player in players)
        {
            if (player != null && player.CompareTag(tag))
            {
                return player;
            }
        }
        return null;
    }

    // ✅ NUEVO MÉTODO: Para reinicializar cuando cambia la escena
    public void OnSceneChanged()
    {
        Debug.Log("🔄 PlayerManager: Escena cambiada, reinicializando...");
        players.Clear();
        isInitialized = false;
        StartCoroutine(InitializeWithDelay());
    }
}