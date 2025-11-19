using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Personaje> personajes;
    public int jugadorSeleccionado = 0;

    [Header("Spawn Points - SOLO REFERENCIA")]
    [Tooltip("Estos spawn points son usados por PlayerScenePersister, no por GameManager")]
    public Transform playerSpawnPoint;
    public Transform botSpawnPoint;

    [Header("Prefabs - SOLO REFERENCIA")]
    [Tooltip("Estos prefabs son usados por PlayerScenePersister, no por GameManager")]
    public GameObject playerPrefab;
    public GameObject botPrefab;

    // NUEVAS VARIABLES PARA EL SISTEMA DE CHECKPOINTS
    private int currentCheckpoint = 0;
    private int totalCheckpoints = 4;
    private bool gameActive = false;

    private void Awake()
    {
        Debug.Log("🔄 GameManager Awake llamado");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManager creado y persistente");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📍 Escena cargada: {scene.name}");

        // ✅ GameManager YA NO SPAWNA JUGADORES
        // PlayerScenePersister se encarga de eso
        Debug.Log("✅ GameManager: Dejando que PlayerScenePersister maneje los jugadores");

        // Reiniciar estado del juego al cargar nueva escena
        if (scene.name != "MainMenu") // Asumiendo que tu menú principal tiene este nombre
        {
            ResetGameState();
        }
    }

    // ========================================
    // NUEVOS MÉTODOS PARA EL SISTEMA DE CARRERA
    // ========================================

    /// <summary>
    /// Reinicia el estado del juego para nueva partida
    /// </summary>
    public void ResetGameState()
    {
        currentCheckpoint = 0;
        gameActive = false;
        Debug.Log("🔄 Estado del juego reiniciado");
    }

    /// <summary>
    /// Llamado por CountdownManager cuando termina la cuenta regresiva
    /// </summary>
    public void StartGame()
    {
        gameActive = true;
        currentCheckpoint = 0;
        Debug.Log("🎮 ¡Juego iniciado! Los jugadores pueden moverse");

        // Activar controles de los jugadores si están desactivados
        GameObject player = GetPlayer();
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = true;
            }
            Debug.Log("✅ Controles del jugador activados");
        }

        // Activar bot si existe
        GameObject bot = GameObject.FindGameObjectWithTag("Bot");
        if (bot != null)
        {
            MonoBehaviour[] scripts = bot.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = true;
            }
            Debug.Log("✅ Controles del bot activados");
        }
    }

    /// <summary>
    /// Llamado cuando un checkpoint se completa correctamente
    /// </summary>
    public void CheckpointReached(int checkpointNumber)
    {
        if (!gameActive)
        {
            Debug.LogWarning("⚠️ Checkpoint alcanzado pero juego no activo");
            return;
        }

        if (checkpointNumber == currentCheckpoint + 1)
        {
            currentCheckpoint = checkpointNumber;
            Debug.Log($"✅ Checkpoint {checkpointNumber} completado! Progreso: {currentCheckpoint}/{totalCheckpoints}");

            if (currentCheckpoint >= totalCheckpoints)
            {
                GameCompleted();
            }
        }
        else if (checkpointNumber <= currentCheckpoint)
        {
            Debug.Log($"ℹ️ Checkpoint {checkpointNumber} ya fue completado antes");
        }
        else
        {
            Debug.LogWarning($"⚠️ Checkpoint {checkpointNumber} saltado. Esperando checkpoint {currentCheckpoint + 1}");
        }
    }

    /// <summary>
    /// Llamado cuando todos los checkpoints están completados
    /// </summary>
    private void GameCompleted()
    {
        gameActive = false;
        Debug.Log("🎉 ¡Juego completado! Todos los checkpoints superados");

        // Mostrar pantalla de victoria
        ShowVictoryScreen();

        // Desactivar controles de jugadores
        DisablePlayerControls();
    }

    /// <summary>
    /// Muestra pantalla de victoria (debes implementar tu UI)
    /// </summary>
    private void ShowVictoryScreen()
    {
        // Aquí implementa tu lógica de UI de victoria
        Debug.Log("🏆 ¡Mostrar pantalla de victoria!");

        // Ejemplo básico - puedes expandir esto
        // victoryPanel.SetActive(true);
        // Time.timeScale = 0f; // Pausar el juego
    }

    /// <summary>
    /// Desactiva controles de jugadores al terminar el juego
    /// </summary>
    private void DisablePlayerControls()
    {
        GameObject player = GetPlayer();
        if (player != null)
        {
            // Ejemplo: desactivar movimiento
            MonoBehaviour moveScript = player.GetComponent<MonoBehaviour>(); // Reemplaza con tu script de movimiento
            if (moveScript != null)
            {
                moveScript.enabled = false;
            }
        }

        GameObject bot = GameObject.FindGameObjectWithTag("Bot");
        if (bot != null)
        {
            MonoBehaviour botScript = bot.GetComponent<MonoBehaviour>(); // Reemplaza con tu script de bot
            if (botScript != null)
            {
                botScript.enabled = false;
            }
        }
    }

    // ========================================
    // MÉTODOS DE PERSONAJES (EXISTENTES)
    // ========================================

    public void SeleccionarPersonaje(int index)
    {
        if (personajes != null && index >= 0 && index < personajes.Count)
        {
            jugadorSeleccionado = index;
            PlayerPrefs.SetInt("JugadorIndex", index);
            PlayerPrefs.Save();
            Debug.Log($"✅ Personaje seleccionado: {personajes[index].nombre}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Índice de personaje inválido: {index}");
        }
    }

    public Personaje GetPersonajeSeleccionado()
    {
        if (personajes != null && jugadorSeleccionado >= 0 && jugadorSeleccionado < personajes.Count)
        {
            return personajes[jugadorSeleccionado];
        }
        return null;
    }

    public int GetJugadorSeleccionado()
    {
        return jugadorSeleccionado;
    }

    // ========================================
    // MÉTODOS AUXILIARES (EXISTENTES)
    // ========================================

    /// <summary>
    /// Fuerza el respawn de jugadores (SOLO usar si es absolutamente necesario)
    /// Normalmente PlayerScenePersister maneja esto automáticamente
    /// </summary>
    public void ForceRespawnPlayers()
    {
        Debug.LogWarning("🔄 ForceRespawnPlayers llamado - Pidiendo a PlayerScenePersister que recree jugadores");

        // Destruir jugadores existentes
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        GameObject existingBot = GameObject.FindGameObjectWithTag("Bot");

        if (existingPlayer != null)
        {
            Destroy(existingPlayer);
            Debug.Log("🗑️ Jugador destruido");
        }

        if (existingBot != null)
        {
            Destroy(existingBot);
            Debug.Log("🗑️ Bot destruido");
        }

        // Pedir a PlayerScenePersister que recree
        StartCoroutine(RequestPlayerRecreation());
    }

    private IEnumerator RequestPlayerRecreation()
    {
        yield return new WaitForSeconds(0.2f);

        if (PlayerScenePersister.Instance != null)
        {
            PlayerScenePersister.Instance.EnsurePlayerExists();
            Debug.Log("✅ Solicitado recreación de jugador a PlayerScenePersister");
        }
        else
        {
            Debug.LogError("❌ PlayerScenePersister no encontrado");
        }
    }

    /// <summary>
    /// Obtiene referencia al jugador actual
    /// </summary>
    public GameObject GetPlayer()
    {
        if (PlayerScenePersister.Instance != null)
        {
            return PlayerScenePersister.Instance.GetPlayer();
        }

        // Fallback: buscar por tag
        return GameObject.FindGameObjectWithTag("Player");
    }

    /// <summary>
    /// Verifica si hay un jugador en la escena
    /// </summary>
    public bool HasPlayer()
    {
        return GetPlayer() != null;
    }

    // ========================================
    // NUEVOS MÉTODOS PARA OBTENER ESTADO DEL JUEGO
    // ========================================

    public bool IsGameActive()
    {
        return gameActive;
    }

    public int GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    public int GetTotalCheckpoints()
    {
        return totalCheckpoints;
    }

    // ========================================
    // LIMPIEZA
    // ========================================

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("🗑️ GameManager destruido");
    }
}