using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Detección Automática")]
    public bool autoFindPlayer = true;
    public string playerTag = "Player";
    public float searchInterval = 1f;

    [Header("Camera Bounds (Optional)")]
    public bool useBounds = false;
    public float minX = -50f, maxX = 50f, minY = -50f, maxY = 50f;

    [Header("Debug")]
    public bool showDebug = true;

    // ✅ VARIABLES DE COMPATIBILIDAD para PlayerManager
    [Header("Compatibilidad (No afectan funcionamiento)")]
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private bool _lookAtTarget = false;
    [SerializeField] private float _smoothSpeed = 0.5f;

    // ✅ VARIABLE TARGET AHORA PUBLIC para PlayerManager
    [Header("Target (Asignación Manual)")]
    public Transform target; // ✅ CAMBIADO A PUBLIC

    // Variables privadas
    private Vector3 lastTargetPosition;
    private float lastSearchTime;
    private bool targetAssigned = false;

    void Start()
    {
        if (autoFindPlayer)
        {
            TryFindPlayer();
        }

        if (showDebug)
        {
            Debug.Log($"🔍 CameraFollow Iniciado - AutoBúsqueda: {autoFindPlayer}, Tag: '{playerTag}'");
        }
    }

    void Update()
    {
        // Búsqueda periódica del jugador si no hay target
        if (target == null && autoFindPlayer && Time.time - lastSearchTime > searchInterval)
        {
            TryFindPlayer();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer && Time.frameCount % 120 == 0 && showDebug)
            {
                Debug.Log("🔍 CameraFollow: Buscando jugador...");
            }
            return;
        }

        // ✅ SEGUIMIENTO INSTANTÁNEO - SIN RETRASO
        Vector3 desiredPosition = target.position + offset;

        // Aplicar límites si están habilitados
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // ✅ IMPORTANTE: Mantener la Z fija para juegos 2D
        desiredPosition.z = offset.z;

        // ✅ MOVIMIENTO DIRECTO SIN SUAVIDAD
        transform.position = desiredPosition;

        // Debug de movimiento
        if (showDebug && Vector3.Distance(target.position, lastTargetPosition) > 1f)
        {
            Debug.Log($"🎯 Cámara SIGUIENDO: {target.name} | Posición: {target.position}");
            lastTargetPosition = target.position;
        }
    }

    // ✅ MÉTODO PARA BUSCAR JUGADOR AUTOMÁTICAMENTE
    private void TryFindPlayer()
    {
        lastSearchTime = Time.time;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            SetTarget(player.transform);
        }
        else
        {
            // Buscar por nombres comunes de jugadores
            string[] commonPlayerNames = { "Player", "Player2", "Player_Dog", "Player_Miguel", "Jugador" };
            foreach (string name in commonPlayerNames)
            {
                GameObject playerObj = GameObject.Find(name);
                if (playerObj != null)
                {
                    SetTarget(playerObj.transform);
                    break;
                }
            }
        }

        if (target == null && showDebug && Time.frameCount % 180 == 0)
        {
            Debug.LogWarning("🔍 CameraFollow: No se encontró jugador. Buscando...");
        }
    }

    // ✅ PROPIEDADES DE COMPATIBILIDAD para PlayerManager
    public float smoothTime
    {
        get { return _smoothTime; }
        set
        {
            _smoothTime = value;
            if (showDebug) Debug.Log($"⚡ smoothTime: {value} (compatibilidad)");
        }
    }

    public float maxSpeed
    {
        get { return _maxSpeed; }
        set
        {
            _maxSpeed = value;
            if (showDebug) Debug.Log($"💨 maxSpeed: {value} (compatibilidad)");
        }
    }

    public bool lookAtTarget
    {
        get { return _lookAtTarget; }
        set
        {
            _lookAtTarget = value;
            if (showDebug && value) Debug.LogWarning("👀 lookAtTarget activado (solo 3D)");
        }
    }

    public float smoothSpeed
    {
        get { return _smoothSpeed; }
        set
        {
            _smoothSpeed = value;
            if (showDebug) Debug.Log($"🌀 smoothSpeed: {value} (compatibilidad)");
        }
    }

    // ✅ MÉTODO PÚBLICO PARA ASIGNAR TARGET (alternativa a target público)
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            if (showDebug) Debug.LogWarning("⚠️ Intento de asignar target nulo");
            return;
        }

        target = newTarget;
        targetAssigned = true;

        // ✅ MOVERSE INSTANTÁNEAMENTE AL NUEVO TARGET
        Vector3 newPosition = newTarget.position + offset;
        newPosition.z = offset.z;
        transform.position = newPosition;

        if (showDebug)
        {
            Debug.Log($"🎯 CameraFollow: Target asignado - {newTarget.name}");
            Debug.Log($"📍 Posición inicial: {transform.position}");
        }
    }

    // ✅ MÉTODO PARA QUE PlayerManager PUEDA ASIGNAR EL JUGADOR
    public void AssignPlayer(GameObject playerObject)
    {
        if (playerObject != null)
        {
            SetTarget(playerObject.transform);
            if (showDebug) Debug.Log($"✅ CameraFollow: Jugador asignado por Manager - {playerObject.name}");
        }
    }

    // ✅ MÉTODO PARA OBTENER EL TARGET ACTUAL (para PlayerManager)
    public Transform GetTarget()
    {
        return target;
    }

    // ✅ MÉTODO PARA VERIFICAR SI HAY TARGET
    public bool HasTarget()
    {
        return target != null;
    }

    // ✅ MÉTODO PARA FORZAR BÚSQUEDA INMEDIATA
    public void ForceFindPlayer()
    {
        TryFindPlayer();
        if (target != null && showDebug)
        {
            Debug.Log($"🔍 CameraFollow: Jugador encontrado forzadamente - {target.name}");
        }
    }

    // ✅ MÉTODO PARA VER ESTADO ACTUAL
    public void PrintStatus()
    {
        Debug.Log("=== ESTADO CAMERAFOLLOW ===");
        Debug.Log($"🎯 Target: {(target != null ? target.name : "NO ASIGNADO")}");
        Debug.Log($"📍 Posición Cámara: {transform.position}");
        Debug.Log($"🚀 Posición Jugador: {(target != null ? target.position.ToString() : "N/A")}");
        Debug.Log($"🔍 AutoBúsqueda: {autoFindPlayer}");
        Debug.Log($"🏷️ Tag Búsqueda: '{playerTag}'");
        Debug.Log("============================");
    }

    // ✅ GIZMOS PARA VISUALIZAR
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 1f);
            Gizmos.DrawWireCube(center, size);
        }

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}