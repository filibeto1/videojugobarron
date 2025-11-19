using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerScenePersister : MonoBehaviour
{
    public static PlayerScenePersister Instance;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    private GameObject persistedPlayer;
    private bool isInitializing = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PlayerScenePersister creado");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ✅ ESPERAR ANTES DE INICIALIZAR
        StartCoroutine(InitializePlayerDelayed());

        // ✅ SUSCRIBIRSE A CAMBIOS DE ESCENA
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    IEnumerator InitializePlayerDelayed()
    {
        if (isInitializing) yield break;
        isInitializing = true;

        // ✅ ESPERAR 2 FRAMES COMPLETOS
        yield return null;
        yield return null;

        InitializePlayer();

        isInitializing = false;
    }

    void InitializePlayer()
    {
        // ✅ VERIFICAR SI YA EXISTE
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            Debug.Log($"✅ Player ya existe: {existingPlayer.name}");
            persistedPlayer = existingPlayer;
            DontDestroyOnLoad(persistedPlayer);

            // ✅ FORZAR ESTADO INICIAL SEGURO
            SetupPlayerSafely(persistedPlayer);
            return;
        }

        // ✅ CREAR NUEVO PLAYER
        if (playerPrefab != null)
        {
            Debug.Log("🎮 Creando nuevo Player desde prefab");

            // Buscar spawn point
            Vector3 spawnPosition = Vector3.zero;
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.transform.position;
                Debug.Log($"📍 Spawn point encontrado: {spawnPosition}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró PlayerSpawnPoint, usando (0,0,0)");
            }

            // Crear player
            persistedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            persistedPlayer.name = "Player1";
            persistedPlayer.tag = "Player";
            DontDestroyOnLoad(persistedPlayer);

            // ✅ CONFIGURAR SEGURAMENTE
            SetupPlayerSafely(persistedPlayer);

            Debug.Log($"✅ Player creado: {persistedPlayer.name} en {spawnPosition}");
        }
        else
        {
            Debug.LogError("❌ No hay playerPrefab asignado");
        }
    }

    void SetupPlayerSafely(GameObject player)
    {
        if (player == null) return;

        // ✅ FORZAR RIGIDBODY A ESTADO SEGURO
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // ✅ ASEGURAR CONFIGURACIÓN CORRECTA
            if (rb.gravityScale < 2f)
            {
                rb.gravityScale = 3f;
                Debug.Log($"⚙️ Gravedad ajustada a: {rb.gravityScale}");
            }

            rb.freezeRotation = true;

            Debug.Log($"🛑 Velocidad forzada a cero en: {player.name}");
        }

        // ✅ VERIFICAR PLAYERCONTROLLER
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Desactivar controles temporalmente
            pc.controlEnabled = false;

            // Reactivar después de un momento
            StartCoroutine(EnableControlsAfterDelay(pc));
        }
    }

    IEnumerator EnableControlsAfterDelay(PlayerController pc)
    {
        yield return new WaitForSeconds(0.5f);

        if (pc != null)
        {
            pc.controlEnabled = true;
            Debug.Log("✅ Controles del jugador activados");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📍 Escena cargada: {scene.name}");

        // ✅ VERIFICAR QUE EL PLAYER SIGA EXISTIENDO
        if (persistedPlayer == null)
        {
            Debug.LogWarning("⚠️ Player perdido, recreando...");
            StartCoroutine(InitializePlayerDelayed());
        }
        else
        {
            // ✅ REPOSICIONAR EN SPAWN POINT DE LA NUEVA ESCENA
            StartCoroutine(RepositionPlayerSafely());
        }
    }

    IEnumerator RepositionPlayerSafely()
    {
        // ✅ ESPERAR A QUE LA ESCENA CARGUE COMPLETAMENTE
        yield return new WaitForSeconds(0.3f);

        if (persistedPlayer == null) yield break;

        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            Rigidbody2D rb = persistedPlayer.GetComponent<Rigidbody2D>();

            // ✅ FORZAR VELOCIDAD CERO ANTES DE MOVER
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // ✅ MOVER PLAYER
            persistedPlayer.transform.position = spawnPoint.transform.position;

            // ✅ FORZAR VELOCIDAD CERO DESPUÉS DE MOVER
            yield return new WaitForFixedUpdate();

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Debug.Log($"📍 Player reposicionado en: {spawnPoint.transform.position}");
        }
    }

    public GameObject GetPlayer()
    {
        return persistedPlayer;
    }

    public void EnsurePlayerExists()
    {
        if (persistedPlayer == null && !isInitializing)
        {
            StartCoroutine(InitializePlayerDelayed());
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}