using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPressed = false;
    private PlayerController playerController;

    void Start()
    {
        StartCoroutine(FindPlayerDelayed());
    }

    private System.Collections.IEnumerator FindPlayerDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
            if (allPlayers.Length > 0)
            {
                player = allPlayers[0].gameObject;
            }
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            Debug.Log($"✅ JumpButton conectado a: {player.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró jugador para el botón de salto");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        Debug.Log("🦘 BOTÓN DE SALTO PRESIONADO");

        if (playerController != null)
        {
            playerController.TryJump();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public bool IsPressed()
    {
        return isPressed;
    }
}