using UnityEngine;

public class InputDebugger : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("⚠️ InputDebugger: No se encontró PlayerController en el mismo GameObject");
        }
    }

    void Update()
    {
        if (playerController != null && Time.frameCount % 60 == 0)
        {
            float horizontalInput = GetCurrentHorizontalInput();

            Debug.Log($"🎮 DEBUG INPUT - UseMobile: {playerController.useMobileInput}, " +
                     $"MovJoystick: {playerController.movementJoystick != null}, " +
                     $"JumpJoystick: {playerController.jumpJoystick != null}, " +
                     $"Horizontal Input: {horizontalInput}");
        }
    }

    private float GetCurrentHorizontalInput()
    {
        if (playerController.useMobileInput && playerController.movementJoystick != null)
        {
            // Verificación adicional para evitar errores
            if (playerController.movementJoystick != null)
            {
                return playerController.movementJoystick.Horizontal();
            }
        }
        return 0f;
    }
}