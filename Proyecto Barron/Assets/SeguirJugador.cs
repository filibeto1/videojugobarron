using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    private Transform objetivo; // Transform del personaje a seguir
    public Vector3 offset;      // Desplazamiento de la cámara respecto al personaje
    public float suavizado = 0.125f; // Suavizado para un movimiento más fluido

    public void SetObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }

    private void LateUpdate()
    {
        if (objetivo != null)
        {
            // Calcula la nueva posición de la cámara con el desplazamiento
            Vector3 posicionDeseada = objetivo.position + offset;
            // Interpola suavemente la posición de la cámara
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
            transform.position = posicionSuavizada;
        }
    }
}
