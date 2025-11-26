using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance;

    [Header("Mobile Controls")]
    public bool enableMobileControls = true;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private bool isMobilePlatform = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DetectPlatform();
    }

    void Start()
    {
        if (isMobilePlatform)
        {
            EnableControls(true);
            Debug.Log("🎮 Controles móviles activados");
        }
        else
        {
            EnableControls(false);
            Debug.Log("⌨️ Controles de teclado activados");
        }
    }

    public void DetectPlatform()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
            isMobilePlatform = true;
#else
        isMobilePlatform = Application.isMobilePlatform;
#endif

        Debug.Log($"📱 Plataforma móvil detectada: {isMobilePlatform}");
    }

    public bool IsMobilePlatform()
    {
        return isMobilePlatform;
    }

    public void EnableControls(bool enable)
    {
        enableMobileControls = enable;

        if (enable)
        {
            Debug.Log("🎮 Controles móviles activados");
        }
        else
        {
            Debug.Log("⌨️ Controles de teclado activados");
        }
    }

    public float GetHorizontalInput()
    {
        // Este método sería usado si no hay VirtualJoystick conectado
        // En nuestro caso, los VirtualJoystick manejan el input directamente
        return 0f;
    }

    public bool GetJumpInput()
    {
        // Este método sería usado si no hay VirtualJoystick conectado
        return false;
    }

    public void SetControlsVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.cyan;

        GUI.Label(new Rect(10, Screen.height - 60, 300, 60),
            $"=== MOBILE INPUT ===\n" +
            $"Plataforma Móvil: {isMobilePlatform}\n" +
            $"Controles Activos: {enableMobileControls}",
            style);
    }
}