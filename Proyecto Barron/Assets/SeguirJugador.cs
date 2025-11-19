using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Target")]
    public Transform objetivo; // Transform del personaje a seguir

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Desplazamiento de la cámara
    public float suavizado = 0.125f; // Suavizado para un movimiento más fluido

    [Header("Split Screen Config")]
    public bool isTopScreen = true; // true = pantalla superior (Player 1)

    private Camera playerCamera;

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
                Debug.Log("🎥 Cámara Player 1: PANTALLA SUPERIOR");
            }
            else
            {
                // Pantalla inferior (por si se usa para otro jugador)
                playerCamera.rect = new Rect(0f, 0f, 1f, 0.5f);
                Debug.Log("🎥 Cámara configurada: PANTALLA INFERIOR");
            }
        }

        // Si no hay objetivo asignado, buscar al jugador
        if (objetivo == null)
        {
            FindPlayer();
        }
    }

    public void SetObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        Debug.Log($"🎯 Objetivo de cámara establecido: {nuevoObjetivo.name}");
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            FindPlayer();
            return;
        }

        // Calcula la nueva posición de la cámara con el desplazamiento
        Vector3 posicionDeseada = objetivo.position + offset;

        // Interpola suavemente la posición de la cámara
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
        transform.position = posicionSuavizada;
    }

    void FindPlayer()
    {
        if (objetivo == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                objetivo = player.transform;
                Debug.Log($"🎥 Cámara encontró objetivo: {objetivo.name}");
            }
            else
            {
                // Debug cada 60 frames para no saturar la consola
                if (Time.frameCount % 60 == 0)
                {
                    Debug.LogWarning("⚠️ Cámara: Esperando que aparezca el jugador...");
                }
            }
        }
    }

    // Debug visual en el editor
    void OnDrawGizmos()
    {
        if (objetivo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, objetivo.position);
            Gizmos.DrawWireSphere(objetivo.position, 0.5f);
        }
    }
}