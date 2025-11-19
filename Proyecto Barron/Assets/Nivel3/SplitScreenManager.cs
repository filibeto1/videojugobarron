using UnityEngine;
using UnityEngine.UI;

public class SplitScreenManager : MonoBehaviour
{
    [Header("Camera References")]
    public Camera player1Camera;
    public Camera player2Camera;

    [Header("Split Screen Settings")]
    public float dividerHeight = 0.5f;

    private SplitScreenDivider divider;

    void Start()
    {
        FindCameras();
        FindOrCreateDivider();
        SetSinglePlayer();
    }

    void FindCameras()
    {
        if (player1Camera == null)
        {
            GameObject p1CamObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (p1CamObj != null) player1Camera = p1CamObj.GetComponent<Camera>();
        }

        if (player2Camera == null)
        {
            GameObject p2CamObj = GameObject.FindGameObjectWithTag("CameraP2");
            if (p2CamObj != null) player2Camera = p2CamObj.GetComponent<Camera>();
        }
    }

    void FindOrCreateDivider()
    {
        divider = FindObjectOfType<SplitScreenDivider>();
        if (divider == null)
        {
            // Crear un Canvas para la línea divisoria
            GameObject canvasObj = new GameObject("DividerCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            // ✅ ELIMINADO CanvasScaler para evitar errores
            // CanvasScaler no es necesario para funcionalidad básica

            divider = canvasObj.AddComponent<SplitScreenDivider>();
            Debug.Log("📱 Canvas divisorio creado sin CanvasScaler");
        }
    }

    public void SetSinglePlayer()
    {
        if (player1Camera != null)
        {
            player1Camera.rect = new Rect(0f, 0f, 1f, 1f);
            player1Camera.enabled = true;
        }

        if (player2Camera != null)
        {
            player2Camera.enabled = false;
        }

        Debug.Log("🎮 Modo 1 jugador - Pantalla completa");
    }

    public void SetTwoPlayers()
    {
        if (player1Camera != null)
        {
            player1Camera.rect = new Rect(0f, dividerHeight, 1f, 1f - dividerHeight);
            player1Camera.enabled = true;
        }

        if (player2Camera != null)
        {
            player2Camera.rect = new Rect(0f, 0f, 1f, dividerHeight);
            player2Camera.enabled = true;
        }

        Debug.Log($"🖥️ Modo 2 jugadores - División en: {dividerHeight}");
    }

    [ContextMenu("Switch to 1 Player")]
    public void SwitchToSinglePlayer()
    {
        SetSinglePlayer();
    }

    [ContextMenu("Switch to 2 Players")]
    public void SwitchToTwoPlayers()
    {
        SetTwoPlayers();
    }

    // Método para ajustar la altura de la división
    public void SetDividerHeight(float newHeight)
    {
        dividerHeight = Mathf.Clamp(newHeight, 0.1f, 0.9f);
        if (player2Camera != null && player2Camera.enabled)
        {
            SetTwoPlayers(); // Re-aplicar configuración
        }
    }
}