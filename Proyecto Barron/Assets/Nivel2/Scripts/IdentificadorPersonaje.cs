using UnityEngine;
using System.Collections.Generic;

public class IdentificadorPersonaje : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🔍 IDENTIFICANDO PERSONAJE REAL ===");

        // Listar todos los GameObjects
        GameObject[] todosObjetos = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in todosObjetos)
        {
            // Mostrar objetos que podrían ser el personaje
            bool tieneSprite = obj.GetComponent<SpriteRenderer>() != null;
            bool tieneRigidbody = obj.GetComponent<Rigidbody2D>() != null;
            bool tieneCollider = obj.GetComponent<Collider2D>() != null;
            bool esPersonaje = obj.name.Contains("Player") || obj.name.Contains("Character");

            if (tieneSprite && tieneRigidbody && tieneCollider)
            {
                Debug.Log($"🎯 POSIBLE PERSONAJE: {obj.name} (Tag: {obj.tag})");
                Debug.Log($"   - Posición: {obj.transform.position}");
                Debug.Log($"   - Sprite: {tieneSprite}, Rigidbody: {tieneRigidbody}, Collider: {tieneCollider}");
            }
        }

        // Buscar específicamente "Player(Clone)" que aparece en tus logs
        GameObject playerClone = GameObject.Find("Player(Clone)");
        if (playerClone != null)
        {
            Debug.Log($"🎯 ¡ENCONTRADO! Player(Clone): {playerClone.name}");
            Debug.Log($"   - Tag actual: {playerClone.tag}");
            Debug.Log($"   - Posición: {playerClone.transform.position}");

            // Asignar tag correcto
            playerClone.tag = "Player";
            Debug.Log($"🏷️ Tag 'Player' asignado a {playerClone.name}");
        }
    }
}