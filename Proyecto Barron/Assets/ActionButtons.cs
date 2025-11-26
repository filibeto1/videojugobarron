using UnityEngine;
using UnityEngine.UI;

public class ActionButtons : MonoBehaviour
{
    public PlayerMovementMobile player;
    public Button jumpButton;
    public Button attackButton;

    void Start()
    {
        // Configurar botón de salto
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJump);
        }

        // Configurar botón de ataque
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttack);
        }
    }

    void OnJump()
    {
        // Aquí llamas a tu función de salto existente
        Debug.Log("🦘 Botón de salto presionado");
    }

    void OnAttack()
    {
        // Aquí llamas a tu función de ataque existente
        Debug.Log("⚔️ Botón de ataque presionado");
    }
}