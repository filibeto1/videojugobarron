using UnityEngine;
using System.Collections;

public class SceneJoystickConnector : MonoBehaviour
{
    [Header("Referencias Locales")]
    public VirtualJoystick movementJoystick;
    public VirtualJoystick jumpJoystick;

    [Header("Configuración")]
    public float reconnectDelay = 0.5f;
    public int maxRetries = 5;

    void Start()
    {
        StartCoroutine(ConnectJoysticksToPlayer());
    }

    private IEnumerator ConnectJoysticksToPlayer()
    {
        // Esperar a que la escena esté completamente cargada
        yield return new WaitForSeconds(reconnectDelay);

        int retries = 0;
        bool connected = false;

        while (!connected && retries < maxRetries)
        {
            GameObject player = FindPlayer();

            if (player != null)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();

                if (playerController != null)
                {
                    // Conectar joysticks
                    if (movementJoystick != null)
                    {
                        playerController.SetVirtualJoystick(movementJoystick);
                        Debug.Log($"✅ MovementJoystick conectado a {player.name}");
                    }

                    if (jumpJoystick != null)
                    {
                        playerController.SetVirtualJoystick(jumpJoystick);
                        Debug.Log($"✅ JumpJoystick conectado a {player.name}");
                    }

                    connected = true;
                    Debug.Log($"🎮 Joysticks conectados correctamente en intento {retries + 1}");
                }
            }

            if (!connected)
            {
                retries++;
                Debug.LogWarning($"⚠️ Reintento {retries}/{maxRetries} en {reconnectDelay} segundos...");
                yield return new WaitForSeconds(reconnectDelay);
            }
        }

        if (!connected)
        {
            Debug.LogError("❌ No se pudo conectar los joysticks después de varios intentos");
        }
    }

    private GameObject FindPlayer()
    {
        // Buscar por tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;

        // Buscar por componente
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) return pc.gameObject;

        return null;
    }
}