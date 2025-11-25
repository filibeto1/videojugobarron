using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Configuración")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Opciones")]
    public bool seguimientoInstantaneo = true;
    public float suavizado = 5f;

    // ✅ PROPIEDADES QUE FALTABAN - necesarias para otros scripts
    public bool isTopScreen = false;
    public string playerTargetName = "Player"; // ✅ IMPORTANTE: Configurar diferente para cada cámara

    [Header("Debug")]
    public bool mostrarDebug = false;

    private bool jugadorEncontrado = false;
    private float tiempoBusqueda = 0f;
    private float intervaloBusqueda = 0.5f;
    private int intentosBusqueda = 0;

    void Start()
    {
        if (mostrarDebug)
            Debug.Log($"🚀 SEGUIR JUGADOR INICIADO - Buscando: '{playerTargetName}'");

        BuscarJugadorInmediato();

        if (player != null)
        {
            PosicionarCamaraInmediatamente();
        }
    }

    void Update()
    {
        if (player == null && !jugadorEncontrado)
        {
            tiempoBusqueda += Time.deltaTime;
            if (tiempoBusqueda >= intervaloBusqueda)
            {
                BuscarJugadorInmediato();
                tiempoBusqueda = 0f;
            }
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            if (mostrarDebug && Time.frameCount % 300 == 0)
                Debug.LogWarning($"⏳ LateUpdate: Esperando jugador '{playerTargetName}'...");
            return;
        }

        Vector3 posicionDeseada = player.position + offset;
        posicionDeseada.z = offset.z;

        if (seguimientoInstantaneo)
        {
            transform.position = posicionDeseada;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
        }
    }

    void BuscarJugadorInmediato()
    {
        intentosBusqueda++;

        if (mostrarDebug && intentosBusqueda <= 3)
            Debug.Log($"🔍 BUSCANDO JUGADOR: '{playerTargetName}' (Intento {intentosBusqueda})");

        GameObject jugadorObj = null;

        // ✅ PRIORIDAD 1: Buscar por el nombre específico configurado
        if (!string.IsNullOrEmpty(playerTargetName))
        {
            jugadorObj = GameObject.Find(playerTargetName);

            if (jugadorObj == null)
            {
                // Buscar con "(Clone)" al final (común en instancias)
                jugadorObj = GameObject.Find(playerTargetName + "(Clone)");
            }
        }

        // ✅ PRIORIDAD 2: Si no se configuró nombre específico, buscar por tag
        if (jugadorObj == null && string.IsNullOrEmpty(playerTargetName))
        {
            jugadorObj = GameObject.FindGameObjectWithTag("Player");
        }

        // ✅ PRIORIDAD 3: Búsqueda avanzada solo si aún no se encontró
        if (jugadorObj == null)
        {
            GameObject[] todosObjetos = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in todosObjetos)
            {
                // Excluir objetos que NO son jugadores reales
                if (obj.name.Contains("PlayerScenePersister") ||
                    obj.name.Contains("PlayerManager") ||
                    obj.name.Contains("GameManager"))
                {
                    continue;
                }

                // Buscar coincidencia con playerTargetName
                if (obj.name.Contains(playerTargetName) && obj.activeInHierarchy)
                {
                    // Verificar que tenga componentes de jugador
                    if (obj.GetComponent<Rigidbody2D>() != null ||
                        obj.GetComponent<PlayerController>() != null ||
                        obj.GetComponent<BotController>() != null)
                    {
                        jugadorObj = obj;
                        break;
                    }
                }
            }
        }

        if (jugadorObj != null)
        {
            player = jugadorObj.transform;
            jugadorEncontrado = true;

            Debug.Log($"✅ ¡JUGADOR ENCONTRADO! → {player.name} (Buscando: '{playerTargetName}', Intento {intentosBusqueda})");
            PosicionarCamaraInmediatamente();
        }
        else
        {
            if (intentosBusqueda == 10)
            {
                Debug.LogWarning($"⏳ Esperando jugador '{playerTargetName}'... ({intentosBusqueda} intentos)");
            }
            else if (intentosBusqueda > 30 && mostrarDebug)
            {
                Debug.LogWarning($"⚠️ NO SE ENCONTRÓ '{playerTargetName}' después de {intentosBusqueda} intentos");
            }
        }
    }

    void PosicionarCamaraInmediatamente()
    {
        if (player != null)
        {
            Vector3 posicionInicial = player.position + offset;
            posicionInicial.z = offset.z;
            transform.position = posicionInicial;

            if (mostrarDebug)
                Debug.Log($"📌 CÁMARA POSICIONADA: {transform.position}");
        }
    }

    // ✅ MÉTODOS QUE FALTABAN - necesarios para otros scripts

    public void SetPlayerTarget(Transform newTarget)
    {
        player = newTarget;
        if (newTarget != null)
        {
            jugadorEncontrado = true;
            intentosBusqueda = 0;
            Debug.Log($"✅ Jugador asignado manualmente: {newTarget.name}");

            if (seguimientoInstantaneo)
            {
                PosicionarCamaraInmediatamente();
            }
        }
    }

    public void ForceFindTarget()
    {
        if (mostrarDebug)
            Debug.Log($"🔄 ForceFindTarget llamado - Buscando '{playerTargetName}' forzadamente");

        jugadorEncontrado = false;
        player = null;
        tiempoBusqueda = 0f;
        intentosBusqueda = 0;

        BuscarJugadorInmediato();
    }

    public void SetSeguimientoInstantaneo(bool instantaneo)
    {
        seguimientoInstantaneo = instantaneo;
        if (mostrarDebug)
        {
            Debug.Log($"⚡ Seguimiento {(instantaneo ? "INSTANTÁNEO" : "SUAVIZADO")}");
        }
    }

    [ContextMenu("Diagnóstico Rápido")]
    public void DiagnosticoRapido()
    {
        Debug.Log("=== DIAGNÓSTICO CÁMARA ===");
        Debug.Log($"🎯 Jugador: {(player != null ? player.name : "NULL")}");
        Debug.Log($"🔎 Buscando: '{playerTargetName}'");
        Debug.Log($"📍 Pos Cámara: {transform.position}");
        Debug.Log($"🚀 Offset: {offset}");
        Debug.Log($"⚡ Modo: {(seguimientoInstantaneo ? "Instantáneo" : "Suavizado")}");
        Debug.Log($"🔍 Encontrado: {jugadorEncontrado}");
        Debug.Log($"🔢 Intentos de búsqueda: {intentosBusqueda}");

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"👥 Jugadores con tag 'Player': {players.Length}");

        foreach (GameObject p in players)
        {
            Debug.Log($"   - {p.name} en posición: {p.transform.position}");
        }

        // Buscar objetos con "Player" en el nombre
        GameObject[] todosObjetos = FindObjectsOfType<GameObject>();
        int contadorPlayer = 0;
        foreach (GameObject obj in todosObjetos)
        {
            if (obj.name.Contains("Player"))
            {
                contadorPlayer++;
                Debug.Log($"   🔍 Encontrado: {obj.name} (Activo: {obj.activeInHierarchy})");
            }
        }
        Debug.Log($"📊 Total objetos con 'Player' en nombre: {contadorPlayer}");
        Debug.Log("========================");
    }

    public void PrintEstado()
    {
        Debug.Log("=== ESTADO SEGUIRJUGADOR ===");
        Debug.Log($"🎯 Jugador: {(player != null ? player.name : "NO ASIGNADO")}");
        Debug.Log($"🔎 Buscando: '{playerTargetName}'");
        Debug.Log($"📍 Posición Cámara: {transform.position}");
        Debug.Log($"🚀 Posición Jugador: {(player != null ? player.position.ToString() : "N/A")}");
        Debug.Log($"⚡ Modo: {(seguimientoInstantaneo ? "Instantáneo" : "Suavizado")}");
        Debug.Log($"🔍 Jugador Encontrado: {jugadorEncontrado}");
        Debug.Log($"🔢 Intentos: {intentosBusqueda}");
        Debug.Log("=============================");
    }
}