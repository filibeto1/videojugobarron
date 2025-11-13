using UnityEngine;
using System.Collections;

public class BuscadorJugadorRapido : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(BuscarConDelay());
    }

    IEnumerator BuscarConDelay()
    {
        // Esperar un frame para que todos los objetos se instancien
        yield return null;

        Debug.Log("=== 🚀 BÚSQUEDA RÁPIDA DE JUGADOR ===");

        // Buscar por tag
        GameObject[] jugadoresTag = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"Objetos con tag 'Player': {jugadoresTag.Length}");
        foreach (GameObject obj in jugadoresTag)
        {
            Debug.Log($"   🏷️ {obj.name} (Sprite: {obj.GetComponent<SpriteRenderer>() != null})");
        }

        // Buscar por nombres
        string[] nombres = { "Player(Clone)", "Player", "Personaje", "Character" };
        foreach (string nombre in nombres)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null)
            {
                Debug.Log($"   📛 Encontrado por nombre '{nombre}': {obj.name}");
            }
        }

        // Buscar todos los SpriteRenderers
        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"Total de SpriteRenderers: {sprites.Length}");

        foreach (SpriteRenderer sprite in sprites)
        {
            GameObject obj = sprite.gameObject;
            if (obj.GetComponent<Rigidbody2D>() != null && obj.GetComponent<Collider2D>() != null)
            {
                Debug.Log($"   🎯 POSIBLE JUGADOR: {obj.name} (Tag: {obj.tag})");
                Debug.Log($"      - Posición: {obj.transform.position}");
                Debug.Log($"      - Components: Sprite=✓, Rigidbody=✓, Collider=✓");
            }
        }
    }
}