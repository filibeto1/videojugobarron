using UnityEngine;
using System.Collections;

public class PlayerCollisionManager : MonoBehaviour
{
    public static PlayerCollisionManager Instance;

    [Header("Player Collision Settings")]
    public bool playersCanPassThrough = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(ConfigureWithDelay());
    }

    private IEnumerator ConfigureWithDelay()
    {
        // Esperar un frame para que todos los jugadores estén inicializados
        yield return new WaitForEndOfFrame();
        ConfigurePlayerCollisions();
    }

    public void ConfigurePlayerCollisions()
    {
        // ✅ SOLUCIÓN: Usar colisiones basadas en TAGS en lugar de LAYERS
        // Ya que todos los jugadores están en la misma layer "Default"

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] allPlayer2s = GameObject.FindGameObjectsWithTag("Player2");
        GameObject[] allBots = GameObject.FindGameObjectsWithTag("Bot");

        // Combinar todos los arrays
        GameObject[] allPlayerObjects = new GameObject[allPlayers.Length + allPlayer2s.Length + allBots.Length];
        allPlayers.CopyTo(allPlayerObjects, 0);
        allPlayer2s.CopyTo(allPlayerObjects, allPlayers.Length);
        allBots.CopyTo(allPlayerObjects, allPlayers.Length + allPlayer2s.Length);

        if (playersCanPassThrough)
        {
            // Ignorar colisiones entre todos los jugadores
            for (int i = 0; i < allPlayerObjects.Length; i++)
            {
                for (int j = i + 1; j < allPlayerObjects.Length; j++)
                {
                    Collider2D collider1 = allPlayerObjects[i].GetComponent<Collider2D>();
                    Collider2D collider2 = allPlayerObjects[j].GetComponent<Collider2D>();

                    if (collider1 != null && collider2 != null)
                    {
                        Physics2D.IgnoreCollision(collider1, collider2, true);
                    }
                }
            }
            Debug.Log($"✅ Jugadores pueden atravesarse - Total: {allPlayerObjects.Length} jugadores configurados");
        }
        else
        {
            // Permitir colisiones entre todos los jugadores
            for (int i = 0; i < allPlayerObjects.Length; i++)
            {
                for (int j = i + 1; j < allPlayerObjects.Length; j++)
                {
                    Collider2D collider1 = allPlayerObjects[i].GetComponent<Collider2D>();
                    Collider2D collider2 = allPlayerObjects[j].GetComponent<Collider2D>();

                    if (collider1 != null && collider2 != null)
                    {
                        Physics2D.IgnoreCollision(collider1, collider2, false);
                    }
                }
            }
            Debug.Log($"❌ Jugadores NO pueden atravesarse - Total: {allPlayerObjects.Length} jugadores configurados");
        }
    }

    public void TogglePlayerCollisions(bool enabled)
    {
        playersCanPassThrough = !enabled;
        ConfigurePlayerCollisions();
    }

    // ✅ NUEVO MÉTODO: Para agregar un jugador dinámicamente (cuando se crea Player2)
    public void RegisterNewPlayer(GameObject newPlayer)
    {
        Debug.Log($"📝 Registrando nuevo jugador: {newPlayer.name} con tag: {newPlayer.tag}");

        // Reconfigurar todas las colisiones para incluir al nuevo jugador
        StartCoroutine(ReconfigureAfterDelay());
    }

    private IEnumerator ReconfigureAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        ConfigurePlayerCollisions();
    }
}