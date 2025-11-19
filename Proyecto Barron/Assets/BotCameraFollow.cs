using UnityEngine;
using System.Collections;

public class BotCameraFollow : MonoBehaviour
{
    [Header("Target Configuration")]
    public Transform botTarget;
    public string botTag = "Bot";
    public string player2Name = "Player2";

    [Header("Camera Settings")]
    public float smoothSpeed = 0.1f;
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Split Screen Config")]
    public bool isTopScreen = false;

    private Camera botCamera;
    private bool targetFound = false;
    private float searchCooldown = 2f;
    private float lastSearchTime = 0f;
    private bool needsInitialPosition = true;

    void Start()
    {
        botCamera = GetComponent<Camera>();

        if (botCamera != null)
        {
            gameObject.tag = "CameraP2";

            // Desactivar AudioListener en cámara secundaria
            AudioListener audioListener = GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
                Debug.Log("🔇 Audio Listener deshabilitado en cámara del Bot");
            }

            // Inicialmente desactivada hasta encontrar target
            botCamera.enabled = false;
        }

        Debug.Log("🎥 Cámara del Bot: Inicializando búsqueda de Bot/Player2...");

        // Buscar inmediatamente
        FindBotTargetImmediate();
    }

    void FindBotTargetImmediate()
    {
        Debug.Log("🔍 Búsqueda INMEDIATA de Bot...");

        // 1. Buscar por tag
        GameObject targetObj = GameObject.FindGameObjectWithTag(botTag);

        // 2. Buscar por nombre
        if (targetObj == null)
        {
            targetObj = GameObject.Find(player2Name);
            if (targetObj != null) Debug.Log("✅ Bot encontrado por nombre: " + player2Name);
        }

        // 3. Buscar cualquier objeto que contenga "Bot" en el nombre
        if (targetObj == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Bot") || obj.name.Contains("Player2"))
                {
                    targetObj = obj;
                    Debug.Log("✅ Bot encontrado por nombre parcial: " + obj.name);
                    break;
                }
            }
        }

        if (targetObj != null)
        {
            botTarget = targetObj.transform;
            targetFound = true;

            if (botCamera != null)
            {
                botCamera.enabled = true;
                Debug.Log("✅ Cámara del Bot ACTIVADA");
            }

            // Posicionamiento inmediato
            transform.position = botTarget.position + offset;
            needsInitialPosition = false;

            Debug.Log($"🎥 Cámara del Bot CONECTADA a: {botTarget.name}");
            Debug.Log($"🎥 Posición del Bot: {botTarget.position}");
            Debug.Log($"🎥 Posición de la cámara: {transform.position}");
        }
        else
        {
            Debug.LogWarning("❌ Bot/Player2 no encontrado en búsqueda inmediata");
            // Programar siguiente búsqueda
            lastSearchTime = Time.time;
        }
    }

    void LateUpdate()
    {
        if (!targetFound || botTarget == null)
        {
            // Búsqueda con cooldown para no saturar
            if (Time.time - lastSearchTime > searchCooldown)
            {
                FindBotTargetImmediate();
            }
            return;
        }

        // Posicionamiento inicial inmediato
        if (needsInitialPosition)
        {
            transform.position = botTarget.position + offset;
            needsInitialPosition = false;
            Debug.Log($"🎥 Cámara del Bot: Posición inicial establecida en: {transform.position}");
        }

        // Seguimiento suave
        Vector3 desiredPosition = botTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // 🔥 MÉTODO NUEVO - Para forzar búsqueda desde otros scripts
    public void ForceFindTarget()
    {
        Debug.Log("🎥 Forzando búsqueda de target...");
        targetFound = false;
        botTarget = null;
        needsInitialPosition = true;

        if (botCamera != null)
        {
            botCamera.enabled = false;
        }

        FindBotTargetImmediate();
    }

    // Método público para asignar target manualmente
    public void SetTarget(Transform newTarget)
    {
        botTarget = newTarget;
        targetFound = true;

        if (botCamera != null)
        {
            botCamera.enabled = true;
        }

        needsInitialPosition = true;
        Debug.Log($"🎥 Target asignado manualmente: {newTarget.name}");
    }

    void OnDrawGizmos()
    {
        if (botTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, botTarget.position);
            Gizmos.DrawWireSphere(botTarget.position, 0.5f);

            // Dibujar área de la cámara
            Gizmos.color = Color.blue;
            if (botCamera != null)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(botCamera.orthographicSize * 2 * botCamera.aspect, botCamera.orthographicSize * 2, 0));
            }
        }
        else
        {
            // Dibujar icono de cámara sin target
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));
            Gizmos.DrawIcon(transform.position + Vector3.up, "Camera Gizmo");
        }
    }
}