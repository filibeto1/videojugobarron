using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Personaje> personajes;
    public int jugadorSeleccionado = 0;

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform botSpawnPoint;

    [Header("Prefabs")]
    public GameObject playerPrefab;

    private void Awake()
    {
        Debug.Log("🔄 GameManager Awake llamado");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManager creado y persistente");

            if (personajes == null)
            {
                personajes = new List<Personaje>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Nivel3")
        {
            SpawnPlayers();
        }
    }

    public void SeleccionarPersonaje(int index)
    {
        if (personajes != null && index >= 0 && index < personajes.Count)
        {
            jugadorSeleccionado = index;
            PlayerPrefs.SetInt("JugadorIndex", index);
            PlayerPrefs.Save();
            Debug.Log($"✅ Personaje seleccionado: {personajes[index].nombre}");
        }
    }

    void SpawnPlayers()
    {
        SpawnPlayer();
        SpawnBot();
    }

    void SpawnPlayer()
    {
        if (playerSpawnPoint != null && playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            player.name = "Player1";
            player.tag = "Player";

            // Asegurar que tenga los componentes necesarios
            SetupPlayerForNivel3(player);

            Debug.Log($"✅ Jugador spawnedo desde prefab: {playerPrefab.name}");
        }
        else
        {
            Debug.LogError("❌ PlayerSpawnPoint o playerPrefab no asignado");
            if (playerSpawnPoint == null) Debug.LogError("❌ PlayerSpawnPoint es null");
            if (playerPrefab == null) Debug.LogError("❌ PlayerPrefab es null");
        }
    }

    void SetupPlayerForNivel3(GameObject player)
    {
        Debug.Log($"🔧 Configurando player: {player.name}");

        // Verificar Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"❌ {player.name} NO tiene Rigidbody2D!");
            rb = player.AddComponent<Rigidbody2D>();
        }
        else
        {
            Debug.Log($"✅ {player.name} tiene Rigidbody2D");
        }

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // Verificar Collider2D
        if (player.GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"❌ {player.name} NO tiene Collider2D!");
            player.AddComponent<BoxCollider2D>();
        }
        else
        {
            Debug.Log($"✅ {player.name} tiene Collider2D");
        }

        // ✅ COMENTADO: NO añadir PlayerControllerNivel3 si ya existe en el prefab
        // if (player.GetComponent<PlayerControllerNivel3>() == null)
        // {
        //     Debug.LogError($"❌ {player.name} NO tiene PlayerControllerNivel3!");
        //     player.AddComponent<PlayerControllerNivel3>();
        // }
        // else
        // {
        //     Debug.Log($"✅ {player.name} tiene PlayerControllerNivel3");
        // }

        Debug.Log($"🎯 Configuración completada para: {player.name}");
    }

    void SpawnBot()
    {
        if (botSpawnPoint != null)
        {
            GameObject bot = new GameObject("Bot");
            bot.transform.position = botSpawnPoint.position;
            bot.tag = "Bot";

            // Añadir Sprite
            SpriteRenderer botSprite = bot.AddComponent<SpriteRenderer>();
            botSprite.color = Color.red;
            botSprite.sprite = CreateCircleSprite(32);

            // Añadir componentes físicos
            Rigidbody2D rb = bot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            bot.AddComponent<CircleCollider2D>();

            // Añadir controlador del bot
            BotController botController = bot.AddComponent<BotController>();

            Debug.Log("✅ Bot spawnedo");
        }
    }

    Sprite CreateCircleSprite(int radius)
    {
        Texture2D texture = new Texture2D(radius * 2, radius * 2);
        Color[] colors = new Color[texture.width * texture.height];

        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                colors[y * texture.width + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
}