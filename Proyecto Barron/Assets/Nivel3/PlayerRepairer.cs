using UnityEngine;

public class PlayerRepairer : MonoBehaviour
{
    [Header("Player Prefab Correcto")]
    public GameObject correctPlayerPrefab;

    [Header("Reparación Automática")]
    public bool autoRepair = true;

    void Start()
    {
        if (autoRepair)
        {
            RepairAllPlayers();
        }
    }

    [ContextMenu("Reparar Players")]
    public void RepairAllPlayers()
    {
        Debug.Log("?? Iniciando reparación de Players...");

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            Debug.LogWarning("?? No se encontraron Players en la escena");
            return;
        }

        foreach (GameObject player in players)
        {
            RepairPlayer(player);
        }
    }

    void RepairPlayer(GameObject player)
    {
        Debug.Log($"?? Reparando: {player.name}");

        // Verificar PlayerController
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"? {player.name} no tiene PlayerController");

            if (correctPlayerPrefab != null)
            {
                Debug.Log($"?? Reemplazando {player.name} con prefab correcto...");
                ReplacePlayerWithPrefab(player);
            }
            else
            {
                Debug.Log("?? Agregando PlayerController básico...");
                pc = player.AddComponent<PlayerController>();
                SetupBasicComponents(player);
            }
        }
        else
        {
            Debug.Log($"? {player.name} tiene PlayerController - OK");
        }
    }

    void ReplacePlayerWithPrefab(GameObject brokenPlayer)
    {
        Vector3 position = brokenPlayer.transform.position;
        Quaternion rotation = brokenPlayer.transform.rotation;
        Transform parent = brokenPlayer.transform.parent;

        GameObject newPlayer = Instantiate(correctPlayerPrefab, position, rotation, parent);
        newPlayer.name = brokenPlayer.name;
        newPlayer.tag = "Player";

        DestroyImmediate(brokenPlayer);

        Debug.Log($"? Player reemplazado: {newPlayer.name}");
    }

    void SetupBasicComponents(GameObject player)
    {
        // Asegurar componentes básicos
        if (player.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }

        if (player.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.8f);
        }

        Debug.Log($"? Componentes básicos agregados a {player.name}");
    }
}