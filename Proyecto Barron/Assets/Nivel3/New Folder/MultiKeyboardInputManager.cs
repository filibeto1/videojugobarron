using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiKeyboardInputManager : MonoBehaviour
{
    [Header("Configuración de Teclados")]
    public bool enableMultiKeyboard = true;
    public float deviceCheckInterval = 2f;

    [Header("Controles Player1 (WASD + Space)")]
    public KeyCode player1Left = KeyCode.A;
    public KeyCode player1Right = KeyCode.D;
    public KeyCode player1Jump = KeyCode.Space;
    public KeyCode player1JumpAlt = KeyCode.W;
    public KeyCode player1Action = KeyCode.E;

    [Header("Controles Player2 (Flechas + RCtrl)")]
    public KeyCode player2Left = KeyCode.LeftArrow;
    public KeyCode player2Right = KeyCode.RightArrow;
    public KeyCode player2Jump = KeyCode.UpArrow;
    public KeyCode player2Action = KeyCode.RightControl;

    [Header("Controles Alternativos Player2 (IJKL + O)")]
    public bool useAlternativePlayer2Controls = false;
    public KeyCode player2LeftAlt = KeyCode.J;
    public KeyCode player2RightAlt = KeyCode.L;
    public KeyCode player2JumpAlt = KeyCode.I;
    public KeyCode player2ActionAlt = KeyCode.O;

    [Header("Debug Info")]
    public int detectedKeyboards = 0;
    public string keyboardStatus = "Checking...";

    // Singleton
    public static MultiKeyboardInputManager Instance { get; private set; }

    // Variables para prevenir inputs cruzados
    private Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();
    private float lastPlayer1Input = 0f;
    private float lastPlayer2Input = 0f;
    private float inputDebounceTime = 0.05f;

    // Para debug de inputs
    private bool lastP1Jump = false;
    private bool lastP2Jump = false;

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

        InitializeKeyStates();
        Debug.Log("🎮 MultiKeyboardInputManager inicializado");
    }

    void Start()
    {
        StartCoroutine(ContinuousDeviceCheck());
        LogKeyboardConfiguration();
    }

    void InitializeKeyStates()
    {
        keyStates.Clear();

        // Player 1
        keyStates[player1Left] = false;
        keyStates[player1Right] = false;
        keyStates[player1Jump] = false;
        keyStates[player1JumpAlt] = false;
        keyStates[player1Action] = false;

        // Player 2
        keyStates[player2Left] = false;
        keyStates[player2Right] = false;
        keyStates[player2Jump] = false;
        keyStates[player2Action] = false;

        // Player 2 alternativo
        if (useAlternativePlayer2Controls)
        {
            keyStates[player2LeftAlt] = false;
            keyStates[player2RightAlt] = false;
            keyStates[player2JumpAlt] = false;
            keyStates[player2ActionAlt] = false;
        }
    }

    void LogKeyboardConfiguration()
    {
        Debug.Log("⌨️ === CONFIGURACIÓN DE CONTROLES ===");
        Debug.Log($"Player 1:");
        Debug.Log($"  - Izquierda: {player1Left}");
        Debug.Log($"  - Derecha: {player1Right}");
        Debug.Log($"  - Saltar: {player1Jump} (Alternativo: {player1JumpAlt})");
        Debug.Log($"  - Acción: {player1Action}");
        Debug.Log($"Player 2:");
        Debug.Log($"  - Izquierda: {player2Left}");
        Debug.Log($"  - Derecha: {player2Right}");
        Debug.Log($"  - Saltar: {player2Jump}");
        Debug.Log($"  - Acción: {player2Action}");

        if (useAlternativePlayer2Controls)
        {
            Debug.Log($"Player 2 (Alternativo):");
            Debug.Log($"  - Izquierda: {player2LeftAlt}");
            Debug.Log($"  - Derecha: {player2RightAlt}");
            Debug.Log($"  - Saltar: {player2JumpAlt}");
            Debug.Log($"  - Acción: {player2ActionAlt}");
        }
    }

    IEnumerator ContinuousDeviceCheck()
    {
        while (enableMultiKeyboard)
        {
            yield return new WaitForSeconds(deviceCheckInterval);
            CheckConnectedDevices();
        }
    }

    void CheckConnectedDevices()
    {
        string[] devices = Input.GetJoystickNames();
        int keyboardCount = 1;

        foreach (string device in devices)
        {
            if (!string.IsNullOrEmpty(device) && IsKeyboardDevice(device))
            {
                keyboardCount++;
            }
        }

        detectedKeyboards = keyboardCount;
        keyboardStatus = keyboardCount >= 2
            ? "✅ 2+ Teclados (cada jugador usa teclas diferentes)"
            : "⚠️ 1 Teclado (ambos jugadores comparten)";
    }

    bool IsKeyboardDevice(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName)) return false;

        deviceName = deviceName.ToLower();
        string[] keyboardPatterns = {
            "keyboard", "teclado", "usb", "hid", "generic", "logitech",
            "razer", "corsair", "steelseries", "hyperx"
        };

        foreach (string pattern in keyboardPatterns)
        {
            if (deviceName.Contains(pattern))
                return true;
        }

        return false;
    }

    // ========================================
    // MÉTODOS PÚBLICOS PARA OBTENER INPUT
    // ========================================

    public float GetHorizontal(int playerNumber)
    {
        if (!enableMultiKeyboard)
            return GetFallbackHorizontal(playerNumber);

        switch (playerNumber)
        {
            case 1:
                return GetPlayer1Horizontal();
            case 2:
                return GetPlayer2Horizontal();
            default:
                return 0f;
        }
    }
    public bool GetJump(int playerNumber)
    {
        if (!enableMultiKeyboard)
        {
            bool fallbackResult = GetFallbackJump(playerNumber);
            Debug.Log($"🎮 GetJump({playerNumber}) - Fallback: {fallbackResult}");
            return fallbackResult;
        }

        bool jumpPressed = false;

        switch (playerNumber)
        {
            case 1:
                bool spacePressed = GetKeyDownWithDebounce(player1Jump, ref lastPlayer1Input);
                bool wPressed = GetKeyDownWithDebounce(player1JumpAlt, ref lastPlayer1Input);
                jumpPressed = spacePressed || wPressed;

                // DEBUG DETALLADO
                if (spacePressed || wPressed)
                {
                    Debug.Log($"🔄 PLAYER 1 - SALTO DETECTADO - Space:{spacePressed}, W:{wPressed}");
                }
                break;

            case 2:
                bool upPressed = GetKeyDownWithDebounce(player2Jump, ref lastPlayer2Input);
                bool iPressed = useAlternativePlayer2Controls && GetKeyDownWithDebounce(player2JumpAlt, ref lastPlayer2Input);
                jumpPressed = upPressed || iPressed;

                // DEBUG DETALLADO
                if (upPressed || iPressed)
                {
                    Debug.Log($"🔄 PLAYER 2 - SALTO DETECTADO - UpArrow:{upPressed}, I:{iPressed}");
                }
                break;
        }

        return jumpPressed;
    }

    public bool GetJumpHeld(int playerNumber)
    {
        if (!enableMultiKeyboard)
            return GetFallbackJumpHeld(playerNumber);

        switch (playerNumber)
        {
            case 1:
                return Input.GetKey(player1Jump) || Input.GetKey(player1JumpAlt);
            case 2:
                bool mainHeld = Input.GetKey(player2Jump);
                bool altHeld = useAlternativePlayer2Controls && Input.GetKey(player2JumpAlt);
                return mainHeld || altHeld;
            default:
                return false;
        }
    }

    public bool GetAction(int playerNumber)
    {
        if (!enableMultiKeyboard)
            return GetFallbackAction(playerNumber);

        switch (playerNumber)
        {
            case 1:
                return GetKeyDownWithDebounce(player1Action, ref lastPlayer1Input);
            case 2:
                bool mainAction = GetKeyDownWithDebounce(player2Action, ref lastPlayer2Input);
                bool altAction = useAlternativePlayer2Controls && GetKeyDownWithDebounce(player2ActionAlt, ref lastPlayer2Input);
                return mainAction || altAction;
            default:
                return false;
        }
    }

    // ========================================
    // MÉTODOS PRIVADOS CON ANTI-CRUCE
    // ========================================

    private float GetPlayer1Horizontal()
    {
        float input = 0f;

        if (Input.GetKey(player1Left))
        {
            input -= 1f;
            lastPlayer1Input = Time.time;
        }

        if (Input.GetKey(player1Right))
        {
            input += 1f;
            lastPlayer1Input = Time.time;
        }

        return input;
    }

    private float GetPlayer2Horizontal()
    {
        float input = 0f;

        // Controles principales (Flechas)
        if (Input.GetKey(player2Left))
        {
            input -= 1f;
            lastPlayer2Input = Time.time;
        }

        if (Input.GetKey(player2Right))
        {
            input += 1f;
            lastPlayer2Input = Time.time;
        }

        // Controles alternativos (IJKL)
        if (useAlternativePlayer2Controls)
        {
            if (Input.GetKey(player2LeftAlt))
            {
                input -= 1f;
                lastPlayer2Input = Time.time;
            }

            if (Input.GetKey(player2RightAlt))
            {
                input += 1f;
                lastPlayer2Input = Time.time;
            }
        }

        return input;
    }

    private bool GetKeyDownWithDebounce(KeyCode key, ref float lastInputTime)
    {
        if (Input.GetKeyDown(key))
        {
            if (Time.time - lastInputTime >= inputDebounceTime)
            {
                lastInputTime = Time.time;

                // Debug de tecla presionada
                Debug.Log($"⌨️ Tecla presionada: {key} - Tiempo: {Time.time}");

                return true;
            }
        }
        return false;
    }

    // ========================================
    // MÉTODOS FALLBACK (Sistema antiguo)
    // ========================================

    private float GetFallbackHorizontal(int playerNumber)
    {
        switch (playerNumber)
        {
            case 1:
                return Input.GetAxis("Horizontal");
            case 2:
                try
                {
                    return Input.GetAxis("Horizontal_P2");
                }
                catch
                {
                    float input = 0f;
                    if (Input.GetKey(KeyCode.LeftArrow)) input -= 1f;
                    if (Input.GetKey(KeyCode.RightArrow)) input += 1f;
                    return input;
                }
            default:
                return 0f;
        }
    }

    private bool GetFallbackJump(int playerNumber)
    {
        bool jumpPressed = false;

        switch (playerNumber)
        {
            case 1:
                jumpPressed = Input.GetButtonDown("Jump");
                break;
            case 2:
                try
                {
                    jumpPressed = Input.GetButtonDown("Jump_P2");
                }
                catch
                {
                    jumpPressed = Input.GetKeyDown(KeyCode.UpArrow);
                }
                break;
            default:
                jumpPressed = false;
                break;
        }

        if (jumpPressed)
        {
            Debug.Log($"🎮 GetJump({playerNumber}) = TRUE - Sistema Fallback");
        }

        return jumpPressed;
    }

    private bool GetFallbackJumpHeld(int playerNumber)
    {
        switch (playerNumber)
        {
            case 1:
                return Input.GetButton("Jump");
            case 2:
                try
                {
                    return Input.GetButton("Jump_P2");
                }
                catch
                {
                    return Input.GetKey(KeyCode.UpArrow);
                }
            default:
                return false;
        }
    }

    private bool GetFallbackAction(int playerNumber)
    {
        switch (playerNumber)
        {
            case 1:
                return Input.GetKeyDown(KeyCode.E);
            case 2:
                return Input.GetKeyDown(KeyCode.RightControl);
            default:
                return false;
        }
    }

    // ========================================
    // MÉTODOS DE DEBUG MEJORADOS
    // ========================================

    void Update()
    {
        // Debug en tiempo real de inputs de salto
        bool currentP1Jump = GetJump(1);
        bool currentP2Jump = GetJump(2);

        if (currentP1Jump && !lastP1Jump)
        {
            Debug.Log($"🔄 PLAYER 1 - BOTÓN SALTO DETECTADO (Update)");
        }

        if (currentP2Jump && !lastP2Jump)
        {
            Debug.Log($"🔄 PLAYER 2 - BOTÓN SALTO DETECTADO (Update)");
        }

        lastP1Jump = currentP1Jump;
        lastP2Jump = currentP2Jump;
    }

    public void TestInputs()
    {
        Debug.Log("🧪 === TEST DE INPUTS ===");
        Debug.Log($"Player 1 Horizontal: {GetHorizontal(1)}");
        Debug.Log($"Player 1 Jump Pressed: {GetJump(1)}");
        Debug.Log($"Player 1 Jump Held: {GetJumpHeld(1)}");
        Debug.Log($"Player 2 Horizontal: {GetHorizontal(2)}");
        Debug.Log($"Player 2 Jump Pressed: {GetJump(2)}");
        Debug.Log($"Player 2 Jump Held: {GetJumpHeld(2)}");

        // Test de teclas específicas
        Debug.Log($"Tecla Space: {Input.GetKey(KeyCode.Space)}");
        Debug.Log($"Tecla UpArrow: {Input.GetKey(KeyCode.UpArrow)}");
        Debug.Log($"Input Manager Jump: {Input.GetButtonDown("Jump")}");
        Debug.Log($"Input Manager Jump_P2: {Input.GetButtonDown("Jump_P2")}");
    }

    // ========================================
    // MÉTODOS PÚBLICOS DE UTILIDAD
    // ========================================

    public void SetPlayer2AlternativeControls(bool enabled)
    {
        useAlternativePlayer2Controls = enabled;
        InitializeKeyStates();
        LogKeyboardConfiguration();
        Debug.Log($"🔧 Controles alternativos Player2: {(enabled ? "ACTIVADOS" : "DESACTIVADOS")}");
    }

    public string GetInputStatus()
    {
        return keyboardStatus;
    }

    void OnGUI()
    {
        if (!enableMultiKeyboard) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        style.alignment = TextAnchor.UpperLeft;

        string statusText = $"🎮 === ESTADO DE CONTROLES ===\n";
        statusText += $"Teclados detectados: {detectedKeyboards}\n";
        statusText += $"Estado: {keyboardStatus}\n\n";
        statusText += $"Player 1: A/D + SPACE/W\n";
        statusText += $"Player 2: ←/→ + ↑\n";

        if (useAlternativePlayer2Controls)
        {
            statusText += $"Player 2 Alt: J/L + I\n";
        }

        // Mostrar inputs activos en tiempo real
        statusText += $"\n🎯 INPUTS ACTIVOS:\n";

        float p1H = GetHorizontal(1);
        if (p1H != 0) statusText += $"P1 Horizontal: {p1H:F1}\n";
        if (GetJump(1)) statusText += $"P1 SALTO PRESIONADO\n";
        if (GetJumpHeld(1)) statusText += $"P1 Saltando (Mantener)\n";

        float p2H = GetHorizontal(2);
        if (p2H != 0) statusText += $"P2 Horizontal: {p2H:F1}\n";
        if (GetJump(2)) statusText += $"P2 SALTO PRESIONADO\n";
        if (GetJumpHeld(2)) statusText += $"P2 Saltando (Mantener)\n";

        GUI.Label(new Rect(10, 100, 400, 300), statusText, style);
    }
    public void DebugInputStatus(int playerNumber)
    {
        Debug.Log($"🔍 DIAGNÓSTICO INPUT Player {playerNumber}:");

        // Verificar sistema multi-teclado
        if (enableMultiKeyboard)
        {
            Debug.Log($"  MultiKeyboard: ACTIVO");
            switch (playerNumber)
            {
                case 1:
                    Debug.Log($"  Tecla Space: {Input.GetKey(KeyCode.Space)}");
                    Debug.Log($"  Tecla W: {Input.GetKey(KeyCode.W)}");
                    Debug.Log($"  GetJump(1): {GetJump(1)}");
                    break;
                case 2:
                    Debug.Log($"  Tecla UpArrow: {Input.GetKey(KeyCode.UpArrow)}");
                    Debug.Log($"  Tecla I: {Input.GetKey(KeyCode.I)}");
                    Debug.Log($"  GetJump(2): {GetJump(2)}");
                    break;
            }
        }
        else
        {
            Debug.Log($"  MultiKeyboard: INACTIVO - Usando InputManager");
            switch (playerNumber)
            {
                case 1:
                    Debug.Log($"  Input.GetButtonDown('Jump'): {Input.GetButtonDown("Jump")}");
                    break;
                case 2:
                    Debug.Log($"  Input.GetButtonDown('Jump_P2'): {Input.GetButtonDown("Jump_P2")}");
                    break;
            }
        }
    }
}