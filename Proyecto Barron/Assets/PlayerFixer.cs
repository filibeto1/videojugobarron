using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFixer : MonoBehaviour
{
    [Header("Emergency Fix Settings")]
    public bool autoFixOnStart = true;
    public float fixDelay = 1.0f;

    [Header("Player Prefabs")]
    public GameObject defaultPlayerPrefab;

    private bool fixApplied = false;

    void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(EmergencyFixCoroutine());
        }
    }

    IEnumerator EmergencyFixCoroutine()
    {
        if (fixApplied) yield break;

        Debug.Log("🛠️ INICIANDO REPARACIÓN DE EMERGENCIA...");
        yield return new WaitForSeconds(fixDelay);

        // 1. ELIMINAR DUPLICADOS
        RemoveDuplicatePlayers();

        // 2. ASEGURAR QUE HAY AL MENOS UN JUGADOR
        EnsurePlayerExists();

        // 3. REPARAR COMPONENTES
        RepairPlayerComponents();

        // 4. CONFIGURAR CÁMARAS
        SetupCameras();

        fixApplied = true;
        Debug.Log("✅ REPARACIÓN DE EMERGENCIA COMPLETADA");
    }

    void RemoveDuplicatePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"🔍 Encontrados {players.Length} objetos con tag 'Player'");

        if (players.Length > 1)
        {
            Debug.LogWarning($"⚠️ Eliminando {players.Length - 1} duplicados...");

            // Mantener el primero, eliminar los demás
            GameObject keepPlayer = players[0];
            List<GameObject> duplicates = new List<GameObject>();

            for (int i = 1; i < players.Length; i++)
            {
                duplicates.Add(players[i]);
            }

            foreach (GameObject duplicate in duplicates)
            {
                if (duplicate != keepPlayer)
                {
                    Debug.Log($"🗑️ Eliminando duplicado: {duplicate.name} (InstanceID: {duplicate.GetInstanceID()})");
                    DestroyImmediate(duplicate);
                }
            }

            Debug.Log($"✅ Jugador mantenido: {keepPlayer.name}");
        }
        else if (players.Length == 1)
        {
            Debug.Log($"✅ Jugador único: {players[0].name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay jugadores en la escena");
        }
    }

    void EnsurePlayerExists()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null && defaultPlayerPrefab != null)
        {
            Debug.Log("🎮 Creando nuevo jugador desde prefab...");
            player = Instantiate(defaultPlayerPrefab, Vector3.zero, Quaternion.identity);
            player.name = "Player1";
            player.tag = "Player";
            DontDestroyOnLoad(player);

            Debug.Log($"✅ Jugador creado: {player.name}");
        }
        else if (player == null)
        {
            Debug.LogError("❌ No se puede crear jugador - No hay prefab asignado");
        }
    }

    void RepairPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Asegurar PlayerController
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.Log("🔧 Agregando PlayerController...");
            pc = player.AddComponent<PlayerController>();
        }

        // Asegurar Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("🔧 Agregando Rigidbody2D...");
            rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }

        // Asegurar Collider2D
        Collider2D coll = player.GetComponent<Collider2D>();
        if (coll == null)
        {
            Debug.Log("🔧 Agregando BoxCollider2D...");
            BoxCollider2D boxColl = player.AddComponent<BoxCollider2D>();
            boxColl.size = new Vector2(0.8f, 1.8f);
        }

        Debug.Log("✅ Componentes del jugador reparados");
    }
    void SetupCameras()
    {
        // Buscar cámaras
        Camera[] cameras = FindObjectsOfType<Camera>();
        Debug.Log($"📷 Configurando {cameras.Length} cámaras...");

        foreach (Camera cam in cameras)
        {
            // Configurar cámara principal
            if (cam.CompareTag("MainCamera"))
            {
                Debug.Log($"✅ Cámara principal: {cam.name}");
                cam.enabled = true;
            }
            // Configurar cámara del Bot
            else if (cam.CompareTag("CameraP2"))
            {
                BotCameraFollow botFollow = cam.GetComponent<BotCameraFollow>();
                if (botFollow != null)
                {
                    Debug.Log($"✅ Cámara del Bot: {cam.name}");
                    // ✅ ESTA ES LA LÍNEA CORREGIDA - Ahora el método existe
                    botFollow.ForceFindTarget();
                }
            }
        }
    }

    // Método público para forzar reparación
    [ContextMenu("Forzar Reparación")]
    public void ForceFix()
    {
        if (!fixApplied)
        {
            StartCoroutine(EmergencyFixCoroutine());
        }
    }

    void OnDestroy()
    {
        Debug.Log("🛠️ PlayerFixer destruido");
    }
}