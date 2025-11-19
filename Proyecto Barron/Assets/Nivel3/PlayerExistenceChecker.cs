using UnityEngine;

public class PlayerExistenceChecker : MonoBehaviour
{
    void Start()
    {
        Invoke("CheckPlayers", 1f); // Esperar 1 segundo para que todo se inicialice
    }

    void CheckPlayers()
    {
        Debug.Log("🔍 VERIFICACIÓN DE EXISTENCIA DE JUGADORES:");

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");

        Debug.Log($"👤 Jugadores con tag 'Player': {players.Length}");
        foreach (GameObject player in players)
        {
            Debug.Log($"   - {player.name} | Pos: {player.transform.position} | Activo: {player.activeInHierarchy}");
        }

        Debug.Log($"🤖 Bots con tag 'Bot': {bots.Length}");
        foreach (GameObject bot in bots)
        {
            Debug.Log($"   - {bot.name} | Pos: {bot.transform.position} | Activo: {bot.activeInHierarchy}");
        }

        if (players.Length > 1)
        {
            Debug.LogError("🚨 HAY MÚLTIPLES JUGADORES - CONFLICTO DE SPAWN!");
        }
    }
}