using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour
{
    [Header("Configuración")]
    public int checkpointNumber = 1;
    public float activationRadius = 2f;

    private SequenceChallengeManager challengeManager;
    private bool isActive = true;

    // ✅ Lista para trackear qué jugadores han completado este checkpoint
    private HashSet<string> playersWhoCompleted = new HashSet<string>();
    private HashSet<string> playersInChallenge = new HashSet<string>();

    void Start()
    {
        challengeManager = FindObjectOfType<SequenceChallengeManager>();
        if (challengeManager == null)
        {
            Debug.LogError("❌ SequenceChallengeManager no encontrado en la escena");
        }
        else
        {
            Debug.Log($"✅ Checkpoint {checkpointNumber} conectado con ChallengeManager");
        }

        SetupCollider();
    }

    void SetupCollider()
    {
        Collider2D existingCollider2D = GetComponent<Collider2D>();
        Collider existingCollider3D = GetComponent<Collider>();

        if (existingCollider2D != null)
        {
            existingCollider2D.isTrigger = true;
            Debug.Log($"✅ Checkpoint {checkpointNumber} tiene Collider2D - Configurado como trigger");
        }
        else if (existingCollider3D != null)
        {
            Debug.LogWarning($"🔄 Checkpoint {checkpointNumber} tiene Collider3D, cambiando a Collider2D...");
            DestroyImmediate(existingCollider3D);

            BoxCollider2D newCollider = gameObject.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = true;
            newCollider.size = new Vector2(activationRadius * 2, activationRadius * 2);

            Debug.Log($"✅ Checkpoint {checkpointNumber} - Collider3D reemplazado por Collider2D");
        }
        else
        {
            BoxCollider2D newCollider = gameObject.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = true;
            newCollider.size = new Vector2(activationRadius * 2, activationRadius * 2);
            Debug.Log($"✅ Checkpoint {checkpointNumber} - Collider2D agregado");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        Debug.Log($"🔍 Checkpoint {checkpointNumber} - Detectado colisionador: {other.gameObject.name} (Tag: {other.tag})");

        string playerId = GetPlayerId(other.gameObject);

        // ✅ VERIFICAR SI EL JUGADOR YA COMPLETÓ ESTE CHECKPOINT
        if (playersWhoCompleted.Contains(playerId))
        {
            Debug.Log($"⏭️ {playerId} ya completó este checkpoint - Ignorando");
            return;
        }

        // ✅ VERIFICAR SI EL JUGADOR YA ESTÁ EN UN DESAFÍO
        if (playersInChallenge.Contains(playerId))
        {
            Debug.Log($"⏳ {playerId} ya está en un desafío - Ignorando activación");
            return;
        }

        // ✅ NUEVO: VERIFICAR SI ES UN BOT QUE DEBE IGNORAR CHECKPOINTS
        if (other.CompareTag("Bot"))
        {
            BotController bot = other.GetComponent<BotController>();
            if (bot != null && bot.ShouldIgnoreCheckpoint())
            {
                Debug.Log($"🚫 BOT IGNORANDO CHECKPOINT - {other.name} configurado para ignorar checkpoints");

                // ✅ MARCAR COMO COMPLETADO AUTOMÁTICAMENTE PARA EVITAR FUTURAS ACTIVACIONES
                playersWhoCompleted.Add(playerId);
                Debug.Log($"✅ Checkpoint {checkpointNumber} marcado como completado automáticamente para bot: {other.name}");

                return; // ← NO activar el desafío
            }
        }

        // ✅ DETECTAR TANTO PLAYER COMO BOT
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🎯 Checkpoint {checkpointNumber} activado por PLAYER1: {other.name}");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                TriggerChallenge(other.gameObject, playerId);
            }
            else
            {
                Debug.LogWarning($"⚠️ {other.name} tiene tag Player pero no tiene PlayerController");
            }
        }
        else if (other.CompareTag("Bot"))
        {
            Debug.Log($"🎯 Checkpoint {checkpointNumber} activado por PLAYER2/BOT: {other.name}");

            // ✅ INTENTAR OBTENER BOTCONTROLLER
            BotController bot = other.GetComponent<BotController>();
            if (bot != null)
            {
                Debug.Log($"✅ BotController encontrado en {other.name} - Activando desafío");
                TriggerChallenge(other.gameObject, playerId);
            }
            else
            {
                Debug.LogError($"❌ {other.name} tiene tag Bot pero no tiene BotController!");
                // Intentar agregar BotController si no existe
                bot = other.gameObject.AddComponent<BotController>();
                Debug.Log($"⚠️ BotController agregado automáticamente a {other.name}");
                TriggerChallenge(other.gameObject, playerId);
            }
        }
        else
        {
            Debug.Log($"⚠️ Checkpoint ignoró objeto {other.name} con tag: {other.tag}");
        }
    }

    // ✅ MÉTODO: Obtener ID único del jugador
    private string GetPlayerId(GameObject player)
    {
        // Usar el nombre del objeto como ID único
        return player.name;
    }

    void TriggerChallenge(GameObject playerWhoActivated, string playerId)
    {
        if (challengeManager != null)
        {
            Debug.Log($"❓ Activando desafío para {playerWhoActivated.tag} - {playerWhoActivated.name}");

            // ✅ MARCAR QUE ESTE JUGADOR ESTÁ EN UN DESAFÍO
            playersInChallenge.Add(playerId);

            challengeManager.ShowChallenge(this, playerWhoActivated);
        }
        else
        {
            Debug.LogError("❌ ChallengeManager no disponible");
        }
    }

    // ✅ MODIFICADO: Ahora acepta el jugador que completó el desafío
    public void CompleteChallenge(GameObject playerWhoCompleted = null)
    {
        string playerId = playerWhoCompleted != null ? GetPlayerId(playerWhoCompleted) : "Unknown";

        // ✅ MARCAR QUE ESTE JUGADOR COMPLETÓ EL CHECKPOINT
        playersWhoCompleted.Add(playerId);

        // ✅ REMOVER DE LA LISTA DE JUGADORES EN DESAFÍO
        playersInChallenge.Remove(playerId);

        Debug.Log($"✅ Checkpoint {checkpointNumber} completado por: {playerId}");
        Debug.Log($"📊 Estado - Completados: {playersWhoCompleted.Count}/2 jugadores");

        // ✅ NOTIFICAR AL GAME MANAGER
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckpointReached(checkpointNumber);
        }

        // ✅ VERIFICAR SI TODOS LOS JUGADORES HAN COMPLETADO EL CHECKPOINT
        CheckIfAllPlayersCompleted();

        // ✅ EL CHECKPOINT PERMANECE ACTIVO PARA OTROS JUGADORES
        // NO llamar a Deactivate() aquí
    }

    // ✅ MÉTODO: Verificar si todos los jugadores han completado
    private void CheckIfAllPlayersCompleted()
    {
        // Contar cuántos jugadores únicos han completado este checkpoint
        int totalPlayers = GetTotalPlayersCount();

        Debug.Log($"📊 Checkpoint {checkpointNumber} - Completados: {playersWhoCompleted.Count}/{totalPlayers} jugadores");

        if (playersWhoCompleted.Count >= totalPlayers)
        {
            Debug.Log($"🎉 ¡Todos los jugadores han completado el checkpoint {checkpointNumber}!");
            // Opcional: desactivar completamente después de que todos completen
            // Deactivate();
        }
    }

    // ✅ MÉTODO: Obtener el número total de jugadores
    private int GetTotalPlayersCount()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");

        int total = players.Length + bots.Length;
        Debug.Log($"👥 Jugadores totales en escena: {total} ({players.Length} Players + {bots.Length} Bots)");

        return total;
    }

    // ✅ MÉTODO: Para cuando un jugador falla o cancela el desafío
    public void CancelChallenge(GameObject playerWhoCanceled)
    {
        string playerId = GetPlayerId(playerWhoCanceled);
        playersInChallenge.Remove(playerId);
        Debug.Log($"❌ Desafío cancelado para: {playerId}");
    }

    public void Reactivate()
    {
        isActive = true;

        // ✅ OPCIONAL: Limpiar el historial si quieres reiniciar completamente
        // playersWhoCompleted.Clear();
        // playersInChallenge.Clear();

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;
        Debug.Log($"🔄 Checkpoint {checkpointNumber} reactivado");
    }

    public void Deactivate()
    {
        isActive = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        Debug.Log($"🚫 Checkpoint {checkpointNumber} desactivado permanentemente");
    }

    // ✅ MÉTODO: Para reiniciar el checkpoint (útil entre niveles)
    public void ResetCheckpoint()
    {
        playersWhoCompleted.Clear();
        playersInChallenge.Clear();
        isActive = true;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        Debug.Log($"🔄 Checkpoint {checkpointNumber} reiniciado completamente");
    }

    // ✅ MÉTODO: Verificar si un jugador específico ya completó este checkpoint
    public bool HasPlayerCompleted(GameObject player)
    {
        string playerId = GetPlayerId(player);
        return playersWhoCompleted.Contains(playerId);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

#if UNITY_EDITOR
        string statusText = $"Checkpoint {checkpointNumber}\n";
        statusText += $"Completados: {playersWhoCompleted.Count}";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, statusText);
#endif
    }
}