using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Target Configuration")]
    public Transform objetivo; // Transform del personaje a seguir
    public string playerTargetName = ""; // Nombre específico del jugador a seguir

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Desplazamiento de la cámara
    public float suavizado = 0.125f; // Suavizado para un movimiento más fluido

    [Header("Split Screen Config")]
    public bool isTopScreen = true; // true = pantalla superior (Player 1)

    private Camera playerCamera;
    private bool targetFound = false;

    void Start()
    {
        playerCamera = GetComponent<Camera>();

        // Configurar viewport para split-screen
        if (playerCamera != null)
        {
            if (isTopScreen)
            {
                // Pantalla superior (Player 1)
                playerCamera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
                Debug.Log($"🎥 {gameObject.name}: PANTALLA SUPERIOR - Buscando: {playerTargetName}");
            }
            else
            {
                // Pantalla inferior (Player 2)
                playerCamera.rect = new Rect(0f, 0f, 1f, 0.5f);
                Debug.Log($"🎥 {gameObject.name}: PANTALLA INFERIOR - Buscando: {playerTargetName}");
            }
        }

        // Buscar el jugador objetivo inmediatamente
        FindSpecificPlayer();
    }

    public void SetObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        targetFound = objetivo != null;
        if (targetFound)
        {
            Debug.Log($"🎯 {gameObject.name} estableció objetivo: {objetivo.name}");
        }
    }

    public void SetPlayerTargetName(string playerName)
    {
        playerTargetName = playerName;
        FindSpecificPlayer();
    }

    private void LateUpdate()
    {
        if (objetivo == null && !targetFound)
        {
            FindSpecificPlayer();
            return;
        }

        if (objetivo != null)
        {
            // Calcula la nueva posición de la cámara con el desplazamiento
            Vector3 posicionDeseada = objetivo.position + offset;

            // Interpola suavemente la posición de la cámara
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
            transform.position = posicionSuavizada;
        }
    }

    void FindSpecificPlayer()
    {
        // Si ya tenemos un objetivo asignado, no buscar
        if (objetivo != null)
        {
            targetFound = true;
            return;
        }

        // Si tenemos un nombre específico, buscar por nombre
        if (!string.IsNullOrEmpty(playerTargetName))
        {
            GameObject specificPlayer = GameObject.Find(playerTargetName);
            if (specificPlayer != null)
            {
                objetivo = specificPlayer.transform;
                targetFound = true;
                Debug.Log($"✅ {gameObject.name} encontró objetivo específico: {objetivo.name}");
                return;
            }
        }

        // Buscar por tag y filtrar por nombre
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (allPlayers.Length > 0)
        {
            // Si hay nombre específico, buscar coincidencia
            if (!string.IsNullOrEmpty(playerTargetName))
            {
                foreach (GameObject player in allPlayers)
                {
                    if (player.name == playerTargetName ||
                        player.name.Contains(playerTargetName) ||
                        (playerTargetName == "Player1" && !player.name.Contains("2")) ||
                        (playerTargetName == "Player2" && player.name.Contains("2")))
                    {
                        objetivo = player.transform;
                        targetFound = true;
                        Debug.Log($"✅ {gameObject.name} encontró objetivo filtrado: {objetivo.name}");
                        return;
                    }
                }
            }

            // Si no hay nombre específico o no se encontró, asignar por posición
            if (objetivo == null)
            {
                System.Array.Sort(allPlayers, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

                if (isTopScreen && allPlayers.Length >= 1)
                {
                    // Cámara superior sigue al jugador más a la izquierda (Player1)
                    objetivo = allPlayers[0].transform;
                    Debug.Log($"🎯 {gameObject.name} asignado a Player1 por posición: {objetivo.name}");
                }
                else if (!isTopScreen && allPlayers.Length >= 2)
                {
                    // Cámara inferior sigue al jugador más a la derecha (Player2)
                    objetivo = allPlayers[allPlayers.Length - 1].transform;
                    Debug.Log($"🎯 {gameObject.name} asignado a Player2 por posición: {objetivo.name}");
                }
                else if (allPlayers.Length == 1)
                {
                    // Solo hay un jugador
                    objetivo = allPlayers[0].transform;
                    Debug.Log($"🎯 {gameObject.name} asignado al único jugador: {objetivo.name}");
                }

                targetFound = objetivo != null;
            }
        }
        else
        {
            // Debug ocasional para no saturar
            if (Time.frameCount % 120 == 0)
            {
                Debug.LogWarning($"⚠️ {gameObject.name}: Esperando que aparezcan jugadores...");
            }
        }
    }

    // Método para forzar la búsqueda del objetivo
    public void ForceFindTarget()
    {
        targetFound = false;
        objetivo = null;
        FindSpecificPlayer();
    }

    // Debug visual en el editor
    void OnDrawGizmos()
    {
        if (objetivo != null)
        {
            Gizmos.color = isTopScreen ? Color.green : Color.blue;
            Gizmos.DrawLine(transform.position, objetivo.position);
            Gizmos.DrawWireSphere(objetivo.position, 0.5f);
        }
    }
}