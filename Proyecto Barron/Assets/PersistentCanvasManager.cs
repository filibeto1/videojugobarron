using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentCanvasManager : MonoBehaviour
{
    private static PersistentCanvasManager instance;

    [Header("Configuración")]
    public bool persistAcrossScenes = true;
    public bool showDebugInfo = true;

    // Referencias privadas
    private Canvas canvas;
    private MobileInputManager mobileInputManager;
    private VirtualJoystick movementJoystick;

    void Awake()
    {
        // ✅ PROTECCIÓN ANTI-DUPLICADOS MEJORADA
        if (instance != null && instance != this)
        {
            Debug.Log($"🗑️ PersistentCanvasManager duplicado destruido: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // ✅ VERIFICAR QUE NO ESTÉ EN UN PREFAB
        if (transform.parent != null && transform.parent.name.Contains("Player"))
        {
            Debug.LogError("❌ ERROR: PersistentCanvasManager NO debe estar en el prefab del jugador!");
            Debug.LogError("💡 SOLUCIÓN: Elimina este componente del prefab del jugador");
            Destroy(this);
            return;
        }

        instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PersistentCanvasManager creado y persistente");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        FindAllReferences();
        EnsureCanvasIsVisible();
    }

    private void FindAllReferences()
    {
        Debug.Log("🔍 Buscando referencias del Canvas...");

        // 1. Buscar Canvas
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
                canvas = FindObjectOfType<Canvas>();
        }

        // 2. Buscar MobileInputManager
        if (mobileInputManager == null)
        {
            mobileInputManager = GetComponentInChildren<MobileInputManager>();
            if (mobileInputManager == null)
                mobileInputManager = FindObjectOfType<MobileInputManager>();
        }

        // 3. Buscar MovementJoystick
        if (movementJoystick == null)
        {
            VirtualJoystick[] allJoysticks = GetComponentsInChildren<VirtualJoystick>();
            if (allJoysticks.Length == 0)
                allJoysticks = FindObjectsOfType<VirtualJoystick>();

            foreach (VirtualJoystick joystick in allJoysticks)
            {
                if (joystick.gameObject.name.Contains("Movement"))
                {
                    movementJoystick = joystick;
                    break;
                }
            }

            if (movementJoystick == null && allJoysticks.Length > 0)
                movementJoystick = allJoysticks[0];
        }

        Debug.Log($"Canvas: {(canvas != null ? "✅" : "❌")}");
        Debug.Log($"MobileInputManager: {(mobileInputManager != null ? "✅" : "❌")}");
        Debug.Log($"MovementJoystick: {(movementJoystick != null ? "✅" : "❌")}");
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🎬 PersistentCanvas: Escena cargada - {scene.name}");
        StartCoroutine(ReconnectAfterSceneLoad());
    }

    private IEnumerator ReconnectAfterSceneLoad()
    {
        yield return new WaitForSeconds(1.5f); // ✅ Esperar MÁS tiempo

        FindAllReferences();
        EnsureCanvasIsVisible();
        ReconnectJoysticks();

        yield return new WaitForSeconds(0.5f);
        ReconnectToPlayer();
    }

    private void EnsureCanvasIsVisible()
    {
        if (canvas == null)
        {
            FindAllReferences();
            if (canvas == null) return;
        }

        canvas.gameObject.SetActive(true);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        Debug.Log("🎨 Canvas visible y configurado");
    }

    private void ReconnectJoysticks()
    {
        Debug.Log("🔄 Reconectando joysticks...");

        VirtualJoystick[] joysticks = FindObjectsOfType<VirtualJoystick>();

        foreach (VirtualJoystick joystick in joysticks)
        {
            joystick.gameObject.SetActive(true);

            if (joystick.joystickHandle == null)
            {
                Transform handleTransform = joystick.transform.Find("Handle");
                if (handleTransform != null)
                {
                    joystick.joystickHandle = handleTransform.GetComponent<RectTransform>();
                }
            }

            joystick.ReconnectToPlayer();
        }

        Debug.Log($"🕹️ {joysticks.Length} joysticks reconectados");
    }

    private void ReconnectToPlayer()
    {
        GameObject player = FindAnyPlayer();

        if (player == null)
        {
            Debug.LogWarning("⚠️ Player NO encontrado aún, reintentando...");
            StartCoroutine(RetryPlayerConnection());
            return;
        }

        Debug.Log($"✅ Player encontrado: {player.name}");

        if (player.tag != "Player")
        {
            player.tag = "Player";
            Debug.Log("🏷️ Tag cambiado a 'Player'");
        }

        if (mobileInputManager != null)
        {
            mobileInputManager.gameObject.SetActive(true);
            if (mobileInputManager.IsMobilePlatform())
            {
                mobileInputManager.EnableControls(true);
            }
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            VirtualJoystick[] joysticks = FindObjectsOfType<VirtualJoystick>();
            foreach (VirtualJoystick joystick in joysticks)
            {
                playerController.SetVirtualJoystick(joystick);
            }
            playerController.ForceReconnectJoysticks();
            Debug.Log("✅ Joysticks conectados al PlayerController");
        }
    }

    private IEnumerator RetryPlayerConnection()
    {
        yield return new WaitForSeconds(1f);
        ReconnectToPlayer();
    }

    private GameObject FindAnyPlayer()
    {
        // Buscar por tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;

        // Buscar por componente
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) return pc.gameObject;

        // Buscar por nombre
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player") && obj.GetComponent<PlayerController>() != null)
            {
                return obj;
            }
        }

        return null;
    }

    void Update()
    {
        if (canvas != null && !canvas.gameObject.activeSelf)
        {
            canvas.gameObject.SetActive(true);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.cyan;
        style.fontStyle = FontStyle.Bold;

        VirtualJoystick[] joysticks = FindObjectsOfType<VirtualJoystick>();
        int activeJoysticks = 0;
        foreach (var j in joysticks)
        {
            if (j.gameObject.activeInHierarchy) activeJoysticks++;
        }

        GameObject player = FindAnyPlayer();
        string playerInfo = player != null ? $"✅ {player.name}" : "❌ NO ENCONTRADO";

        string info = $"=== CANVAS PERSISTENTE ===\n" +
                     $"Escena: {SceneManager.GetActiveScene().name}\n" +
                     $"Canvas: {(canvas != null && canvas.gameObject.activeSelf ? "✅" : "❌")}\n" +
                     $"Player: {playerInfo}\n" +
                     $"Input Manager: {(mobileInputManager != null ? "✅" : "❌")}\n" +
                     $"Joysticks: {activeJoysticks}/{joysticks.Length}";

        GUI.Label(new Rect(10, Screen.height - 140, 600, 140), info, style);
    }

    public static PersistentCanvasManager Instance
    {
        get { return instance; }
    }

    public void ForceShowControls()
    {
        FindAllReferences();
        EnsureCanvasIsVisible();
        ReconnectJoysticks();

        if (mobileInputManager != null)
        {
            mobileInputManager.EnableControls(true);
        }

        Debug.Log("🔄 Controles forzados");
    }

    public void ReconnectAllJoysticks()
    {
        ReconnectJoysticks();
        ReconnectToPlayer();
    }
}