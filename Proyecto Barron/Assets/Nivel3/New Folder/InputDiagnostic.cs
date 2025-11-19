using UnityEngine;

public class InputDiagnostic : MonoBehaviour
{
    void Update()
    {
        // Diagnosticar cada 2 segundos
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log("=== DIAGNÓSTICO DE INPUTS ===");

            // Player 1
            Debug.Log($"P1 - Space: {Input.GetKey(KeyCode.Space)}, W: {Input.GetKey(KeyCode.W)}");
            Debug.Log($"P1 - Input.GetButtonDown('Jump'): {Input.GetButtonDown("Jump")}");

            // Player 2  
            Debug.Log($"P2 - UpArrow: {Input.GetKey(KeyCode.UpArrow)}, I: {Input.GetKey(KeyCode.I)}");
            Debug.Log($"P2 - Input.GetButtonDown('Jump_P2'): {Input.GetButtonDown("Jump_P2")}");

            // Verificar MultiKeyboard
            var multiKeyboard = FindObjectOfType<MultiKeyboardInputManager>();
            if (multiKeyboard != null)
            {
                Debug.Log($"MultiKeyboard activo: {multiKeyboard.enableMultiKeyboard}");
                Debug.Log($"Teclados detectados: {multiKeyboard.detectedKeyboards}");
            }
        }
    }
}