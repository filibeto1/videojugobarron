using UnityEngine;

public class ControlTester : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showInputDebug = true;

    void Update()
    {
        if (!showInputDebug) return;

        // Probar controles del Jugador 1 (WASD/Flechas)
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Debug.Log("🎮 P1/Flechas: ← presionada");
        if (Input.GetKeyDown(KeyCode.RightArrow)) Debug.Log("🎮 P1/Flechas: → presionada");
        if (Input.GetKeyDown(KeyCode.UpArrow)) Debug.Log("🎮 P1/Flechas: ↑ presionada");
        if (Input.GetKeyDown(KeyCode.A)) Debug.Log("🎮 P1/WASD: A presionada");
        if (Input.GetKeyDown(KeyCode.D)) Debug.Log("🎮 P1/WASD: D presionada");
        if (Input.GetKeyDown(KeyCode.W)) Debug.Log("🎮 P1/WASD: W presionada");

        // Verificar axes
        float p1Horizontal = Input.GetAxis("Horizontal");
        float p2Horizontal = Input.GetAxis("Horizontal_P2");

        if (Mathf.Abs(p1Horizontal) > 0.1f)
        {
            Debug.Log($"🎮 Horizontal (P1) axis: {p1Horizontal}");
        }

        if (Mathf.Abs(p2Horizontal) > 0.1f)
        {
            Debug.Log($"🎮 Horizontal_P2 (P2 - Bot) axis: {p2Horizontal}");
        }
    }
}