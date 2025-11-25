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

    // NUEVAS VARIABLES PARA EL SISTEMA DE CARRERA
    private int currentCheckpoint = 0;
    private int totalCheckpoints = 4;
    private bool gameActive = false;

    private void Awake()
    {
        Debug.Log("🔄 GameManager Awake llamado");

        // VERIFICACIÓN MÁS ROBUSTA DEL SINGLETON
        if (Instance != null && Instance != this)
        {
            Debug.Log("⚠️ GameManager duplicado detectado - destruyendo copia");
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Suscribir eventos UNA SOLA VEZ
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("✅ GameManager creado y persistente");
    }

    void Start()
    {
        Debug.Log("🎮 GameManager Start - Cargando selección de personaje");

        // ✅ USAR SOLO UNA CLAVE CONSISTENTE
        jugadorSeleccionado = PlayerPrefs.GetInt("JugadorSeleccionado", 0);
        Debug.Log($"✅ Selección cargada de PlayerPrefs: {jugadorSeleccionado}");

        // VERIFICAR QUE LA SELECCIÓN ES VÁLIDA
        if (personajes != null && jugadorSeleccionado >= 0 && jugadorSeleccionado < personajes.Count)
        {
            Personaje p = personajes[jugadorSeleccionado];
            Debug.Log($"🎯 Personaje actual: {p.nombre}, Prefab: {(p.personajeJugable != null ? p.personajeJugable.name : "NULL")}");
        }
        else
        {
            Debug.LogError($"❌ Selección inválida: Index={jugadorSeleccionado}, Total personajes={(personajes != null ? personajes.Count : 0)}");

            // FORZAR SELECCIÓN POR DEFECTO SI ES INVÁLIDA
            if (personajes != null && personajes.Count > 0)
            {
                jugadorSeleccionado = 0;
                PlayerPrefs.SetInt("JugadorSeleccionado", 0);
                PlayerPrefs.Save();
                Debug.Log($"🔄 Forzando selección por defecto: {personajes[0].nombre}");
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📍 Escena cargada: {scene.name}");

        // ✅ GameManager YA NO SPAWNA JUGADORES
        Debug.Log("✅ GameManager: Dejando que PlayerScenePersister maneje los jugadores");

        // REINICIAR ESTADO SOLO SI ES UNA NUEVA PARTIDA
        if (ShouldResetGameState(scene.name))
        {
            ResetGameState();
        }
        else
        {
            Debug.Log("🔁 Manteniendo estado del juego para continuidad");
        }
    }

    private bool ShouldResetGameState(string sceneName)
    {
        if (sceneName == "MainMenu" || sceneName == "MenuPrincipal")
        {
            return true;
        }

        if (currentCheckpoint > 0)
        {
            return false;
        }

        return true;
    }

    // ========================================
    // MÉTODOS PARA EL SISTEMA DE CARRERA
    // ========================================

    public void ResetGameState()
    {
        currentCheckpoint = 0;
        gameActive = false;
        Debug.Log("🔄 Estado del juego reiniciado");
    }

    public void ResetCompleteGame()
    {
        currentCheckpoint = 0;
        gameActive = false;
        jugadorSeleccionado = 0;
        Debug.Log("🔄 JUEGO COMPLETAMENTE REINICIADO - Nueva partida");
    }

    public void StartGame()
    {
        gameActive = true;
        currentCheckpoint = 0;
        Debug.Log("🎮 ¡Juego iniciado! Los jugadores pueden moverse");

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

    private void GameCompleted()
    {
        gameActive = false;
        Debug.Log("🎉 ¡Juego completado! Todos los checkpoints superados");
        ShowVictoryScreen();
        DisablePlayerControls();
    }

    private void ShowVictoryScreen()
    {
        Debug.Log("🏆 ¡Mostrar pantalla de victoria!");
    }

    private void DisablePlayerControls()
    {
        GameObject player = GetPlayer();
        if (player != null)
        {
            MonoBehaviour moveScript = player.GetComponent<MonoBehaviour>();
            if (moveScript != null)
            {
                moveScript.enabled = false;
            }
        }

        GameObject bot = GameObject.FindGameObjectWithTag("Bot");
        if (bot != null)
        {
            MonoBehaviour botScript = bot.GetComponent<MonoBehaviour>();
            if (botScript != null)
            {
                botScript.enabled = false;
            }
        }
    }

    // ========================================
    // MÉTODOS DE PERSONAJES
    // ========================================

    public void SeleccionarPersonaje(int index)
    {
        if (personajes != null && index >= 0 && index < personajes.Count)
        {
            jugadorSeleccionado = index;

            // ✅ USAR SOLO UNA CLAVE
            PlayerPrefs.SetInt("JugadorSeleccionado", index);
            PlayerPrefs.Save();

            Personaje p = personajes[index];
            Debug.Log($"✅ Personaje seleccionado: {p.nombre} (Index: {index})");
            Debug.Log($"📁 Prefab asignado: {(p.personajeJugable != null ? p.personajeJugable.name : "NULL - ERROR!")}");
        }
        else
        {
            Debug.LogError($"⚠️ Índice de personaje inválido: {index} (Total: {personajes?.Count})");
        }
    }

    public Personaje GetPersonajeSeleccionado()
    {
        if (personajes != null && jugadorSeleccionado >= 0 && jugadorSeleccionado < personajes.Count)
        {
            return personajes[jugadorSeleccionado];
        }

        Debug.LogWarning("⚠️ No se pudo obtener personaje seleccionado, retornando null");
        return null;
    }

    public int GetJugadorSeleccionado()
    {
        return jugadorSeleccionado;
    }

    // ========================================
    // MÉTODOS AUXILIARES
    // ========================================

    public void ForceRespawnPlayers()
    {
        Debug.LogWarning("🔄 ForceRespawnPlayers llamado - Pidiendo a PlayerScenePersister que recree jugadores");

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

    public GameObject GetPlayer()
    {
        if (PlayerScenePersister.Instance != null)
        {
            return PlayerScenePersister.Instance.GetPlayer();
        }
        return GameObject.FindGameObjectWithTag("Player");
    }

    public bool HasPlayer()
    {
        return GetPlayer() != null;
    }

    // ========================================
    // MÉTODOS PARA OBTENER ESTADO DEL JUEGO
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

    public void LoadLevel(int level)
    {
        Debug.Log($"🔄 Cargando nivel {level}");

        if (level == 1)
        {
            ResetCompleteGame();
        }

        SceneManager.LoadScene($"Nivel{level}");
    }

    // ========================================
    // LIMPIEZA
    // ========================================

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("🗑️ GameManager principal destruido - Esto NO debería pasar!");
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("🚪 Aplicación cerrada - GameManager finalizado");
    }
}