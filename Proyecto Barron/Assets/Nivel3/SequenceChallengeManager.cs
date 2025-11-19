using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SequenceChallenge
{
    public string sequence;
    public string correctAnswer;
    public string hint;
}

[System.Serializable]
public class PlayerChallengeState
{
    public GameObject player;
    public SequenceChallenge currentChallenge;
    public Checkpoint currentCheckpoint;
    public bool isInChallenge;
    public bool isInputEnabled;
}

public class SequenceChallengeManager : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    public Camera player1Camera;
    public Camera player2Camera;

    [Header("UI Referencias por Jugador")]
    public GameObject challengePanel_Player1;
    public GameObject challengePanel_Player2;
    public TextMeshProUGUI sequenceText_Player1;
    public TextMeshProUGUI sequenceText_Player2;
    public TextMeshProUGUI hintText_Player1;
    public TextMeshProUGUI hintText_Player2;
    public TMP_InputField answerInput_Player1;
    public TMP_InputField answerInput_Player2;
    public Button submitButton_Player1;
    public Button submitButton_Player2;
    public TextMeshProUGUI feedbackText_Player1;
    public TextMeshProUGUI feedbackText_Player2;

    [Header("Configuración de Input")]
    public KeyCode submitKey = KeyCode.Return;
    public bool autoFocusInputField = true;

    [Header("Desafíos de Secuencia")]
    public List<SequenceChallenge> challenges = new List<SequenceChallenge>();

    private Queue<SequenceChallenge> challengeQueue;
    private Dictionary<string, PlayerChallengeState> playerStates = new Dictionary<string, PlayerChallengeState>();
    private PlayerController player1Controller;
    private BotController player2Controller;
    private bool playersInitialized = false;
    private TMP_InputField currentActiveInputField;
    private bool isAnyPanelActive = false;
    private bool isGameStarted = false;

    void Start()
    {
        Debug.Log("🔍 Inicializando SequenceChallengeManager...");

        FindCameras();
        InitializePlayerStates();
        VerifyUIReferences();

        if (submitButton_Player1 != null)
        {
            submitButton_Player1.onClick.AddListener(() => {
                if (CanProcessInput(true))
                    CheckAnswer(true);
            });
        }

        if (submitButton_Player2 != null)
        {
            submitButton_Player2.onClick.AddListener(() => {
                if (CanProcessInput(false))
                    CheckAnswer(false);
            });
        }

        SetupInputFieldListeners();
        SetAllPanelsInactive();
        ShuffleChallenges();

        StartCoroutine(FindPlayersWithDelay());
        Debug.Log("✅ SequenceChallengeManager inicializado correctamente");
    }

    private void FindCameras()
    {
        if (player1Camera == null)
        {
            GameObject cam1 = GameObject.Find("Player1Camera");
            if (cam1 != null)
            {
                player1Camera = cam1.GetComponent<Camera>();
                Debug.Log($"✅ Player1Camera encontrada: {player1Camera.name}");
            }
            else
            {
                Debug.LogError("❌ Player1Camera no encontrada");
            }
        }

        if (player2Camera == null)
        {
            GameObject cam2 = GameObject.Find("Player2Camera");
            if (cam2 != null)
            {
                player2Camera = cam2.GetComponent<Camera>();
                Debug.Log($"✅ Player2Camera encontrada: {player2Camera.name}");
            }
            else
            {
                Debug.LogError("❌ Player2Camera no encontrada");
            }
        }

        ConfigureSplitScreenCameras();
    }

    private void ConfigureSplitScreenCameras()
    {
        if (player1Camera != null && player2Camera != null)
        {
            // Player1Camera en la parte superior
            player1Camera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            // Player2Camera en la parte inferior
            player2Camera.rect = new Rect(0f, 0f, 1f, 0.5f);

            Debug.Log("🎥 Cámaras configuradas para pantalla dividida");
            Debug.Log($"   Player1Camera: {player1Camera.rect}");
            Debug.Log($"   Player2Camera: {player2Camera.rect}");
        }
    }

    private void InitializePlayerStates()
    {
        playerStates.Clear();
        playerStates.Add("Player1", new PlayerChallengeState
        {
            isInChallenge = false,
            isInputEnabled = false
        });
        playerStates.Add("Player2", new PlayerChallengeState
        {
            isInChallenge = false,
            isInputEnabled = false
        });
        Debug.Log("✅ Estados de jugadores inicializados: Player1 y Player2");
    }

    void Update()
    {
        if (isAnyPanelActive && isGameStarted && Input.GetKeyDown(submitKey))
        {
            HandleEnterKeyPress();
        }
    }

    public void OnGameStarted()
    {
        isGameStarted = true;
        Debug.Log("🎮 SequenceChallengeManager: Juego iniciado - Input habilitado");
    }

    private void HandleEnterKeyPress()
    {
        if (!isGameStarted) return;

        if (challengePanel_Player1 != null && challengePanel_Player1.activeInHierarchy)
        {
            if (CanProcessInput(true))
            {
                CheckAnswer(true);
            }
        }
        else if (challengePanel_Player2 != null && challengePanel_Player2.activeInHierarchy)
        {
            if (CanProcessInput(false))
            {
                CheckAnswer(false);
            }
        }
    }

    private bool CanProcessInput(bool isPlayer1)
    {
        string playerId = isPlayer1 ? "Player1" : "Player2";

        if (!playerStates.ContainsKey(playerId))
        {
            Debug.LogError($"❌ {playerId} no tiene estado registrado");
            return false;
        }

        var state = playerStates[playerId];
        return state.isInChallenge && state.isInputEnabled && state.currentChallenge != null;
    }

    private void SetupInputFieldListeners()
    {
        if (answerInput_Player1 != null)
        {
            answerInput_Player1.onSelect.AddListener((text) => OnInputFieldSelected(answerInput_Player1, true));
        }

        if (answerInput_Player2 != null)
        {
            answerInput_Player2.onSelect.AddListener((text) => OnInputFieldSelected(answerInput_Player2, false));
        }
    }

    private void OnInputFieldSelected(TMP_InputField inputField, bool isPlayer1)
    {
        currentActiveInputField = inputField;
        isAnyPanelActive = true;

        string playerId = isPlayer1 ? "Player1" : "Player2";

        if (playerStates.ContainsKey(playerId))
        {
            playerStates[playerId].isInputEnabled = true;
        }
    }

    IEnumerator FindPlayersWithDelay()
    {
        yield return new WaitForSeconds(1f);
        FindPlayerControllers();
        playersInitialized = true;
    }

    public void FindPlayerControllers()
    {
        // Buscar Player1 por nombre y tag
        GameObject player1Obj = GameObject.Find("Player1");
        if (player1Obj == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.name == "Player1" || !player.name.Contains("Player2"))
                {
                    player1Obj = player;
                    break;
                }
            }
        }

        // Buscar Player2 por nombre y tag
        GameObject player2Obj = GameObject.Find("Player2");
        if (player2Obj == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.name == "Player2" || player.name.Contains("Player2"))
                {
                    player2Obj = player;
                    break;
                }
            }
        }

        if (player1Obj != null)
        {
            player1Controller = player1Obj.GetComponent<PlayerController>();
            if (playerStates.ContainsKey("Player1"))
            {
                playerStates["Player1"].player = player1Obj;
            }
            Debug.Log($"✅ Player1 identificado: {player1Obj.name} | Tag: {player1Obj.tag}");
        }

        if (player2Obj != null)
        {
            player2Controller = player2Obj.GetComponent<BotController>();
            if (player2Controller == null)
            {
                // Si no tiene BotController, buscar PlayerController
                PlayerController pc = player2Obj.GetComponent<PlayerController>();
                if (pc != null && playerStates.ContainsKey("Player2"))
                {
                    playerStates["Player2"].player = player2Obj;
                    Debug.Log($"✅ Player2 identificado como Player: {player2Obj.name} | Tag: {player2Obj.tag}");
                }
            }
            else if (playerStates.ContainsKey("Player2"))
            {
                playerStates["Player2"].player = player2Obj;
                Debug.Log($"✅ Player2 identificado como Bot: {player2Obj.name} | Tag: {player2Obj.tag}");
            }
        }
    }

    void VerifyUIReferences()
    {
        Debug.Log("🔍 Verificando referencias UI...");

        if (challengePanel_Player1 == null) Debug.LogError("❌ challengePanel_Player1 no asignado");
        if (challengePanel_Player2 == null) Debug.LogError("❌ challengePanel_Player2 no asignado");
    }

    void SetAllPanelsInactive()
    {
        if (challengePanel_Player1 != null)
        {
            challengePanel_Player1.SetActive(false);
            ConfigurePanelForCamera(challengePanel_Player1, true);
        }
        if (challengePanel_Player2 != null)
        {
            challengePanel_Player2.SetActive(false);
            ConfigurePanelForCamera(challengePanel_Player2, false);
        }

        isAnyPanelActive = false;
        currentActiveInputField = null;
        Debug.Log("📱 Todos los paneles desactivados y configurados para sus cámaras respectivas");
    }

    void ConfigurePanelForCamera(GameObject panel, bool isPlayer1Panel)
    {
        if (panel == null) return;

        Canvas canvas = panel.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = panel.AddComponent<Canvas>();
            panel.AddComponent<GraphicRaycaster>();
        }

        // ✅ SOLUCIÓN: Usar Screen Space - Overlay con configuración manual de viewport
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = isPlayer1Panel ? 10000 : 9999;

        // Configurar CanvasScaler
        CanvasScaler scaler = panel.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = panel.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // ✅ CONFIGURACIÓN MANUAL DE VIEWPORT PARA CADA PANEL
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            if (isPlayer1Panel)
            {
                // Panel Player1: Mitad SUPERIOR de la pantalla
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                Debug.Log($"✅ Panel {panel.name} configurado para MITAD SUPERIOR (Player1)");
            }
            else
            {
                // Panel Player2: Mitad INFERIOR de la pantalla
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(1f, 0.5f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                Debug.Log($"✅ Panel {panel.name} configurado para MITAD INFERIOR (Player2)");
            }

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    void ShuffleChallenges()
    {
        if (challengeQueue == null)
            challengeQueue = new Queue<SequenceChallenge>();

        challengeQueue.Clear();

        if (challenges == null || challenges.Count == 0)
        {
            CreateEmergencyChallenges();
        }

        List<SequenceChallenge> shuffled = new List<SequenceChallenge>(challenges);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SequenceChallenge temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        foreach (SequenceChallenge challenge in shuffled)
        {
            challengeQueue.Enqueue(challenge);
        }

        Debug.Log($"✅ {challengeQueue.Count} desafíos mezclados y listos");
    }

    void CreateEmergencyChallenges()
    {
        challenges = new List<SequenceChallenge>
        {
            new SequenceChallenge { sequence = "2, 4, 6, 8, ?", correctAnswer = "10", hint = "Suma 2 cada vez" },
            new SequenceChallenge { sequence = "1, 1, 2, 3, 5, ?", correctAnswer = "8", hint = "Fibonacci - suma los dos anteriores" },
            new SequenceChallenge { sequence = "A, C, E, G, ?", correctAnswer = "I", hint = "Letras saltando una" }
        };
    }

    public void ShowChallenge(Checkpoint checkpoint, GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("❌ ShowChallenge: player es NULL");
            return;
        }

        string playerTag = player.tag;
        string playerName = player.name;

        Debug.Log($"🎯 ShowChallenge llamado - Player: {playerName} | Tag: {playerTag} | Checkpoint: {checkpoint?.name}");

        // ✅ DETECCIÓN MEJORADA DE JUGADORES
        bool isPlayer1 = DeterminePlayerCorrectly(player);

        // OBTENER EL DESAFÍO ACTUAL
        SequenceChallenge currentChallenge = GetCurrentChallengeForPlayer(player);
        if (currentChallenge == null)
        {
            Debug.LogError("❌ No se pudo obtener desafío actual");
            return;
        }

        // ASIGNAR ESTADO AL JUGADOR
        string playerId = isPlayer1 ? "Player1" : "Player2";
        if (playerStates.ContainsKey(playerId))
        {
            playerStates[playerId].currentChallenge = currentChallenge;
            playerStates[playerId].currentCheckpoint = checkpoint;
            playerStates[playerId].isInChallenge = true;
            playerStates[playerId].isInputEnabled = true;
            playerStates[playerId].player = player;
        }

        // MOSTRAR PANEL CORRECTO
        if (isPlayer1)
        {
            Debug.Log($"🎯 Mostrando desafío para PLAYER1");
            ShowPlayer1Challenge(currentChallenge);
        }
        else
        {
            Debug.Log($"🎯 Mostrando desafío para PLAYER2");
            ShowPlayer2Challenge(currentChallenge, player);
        }
    }

    private bool DeterminePlayerCorrectly(GameObject player)
    {
        string playerName = player.name;

        // ✅ DETECCIÓN POR NOMBRE EXACTO PRIMERO
        if (playerName == "Player1")
        {
            Debug.Log($"✅ Identificado como Player1 por nombre exacto");
            return true;
        }
        if (playerName == "Player2")
        {
            Debug.Log($"✅ Identificado como Player2 por nombre exacto");
            return false;
        }

        // ✅ DETECCIÓN POR CONTENIDO DEL NOMBRE
        if (playerName.Contains("Player1") && !playerName.Contains("Player2"))
        {
            Debug.Log($"✅ Identificado como Player1 por contenido del nombre");
            return true;
        }
        if (playerName.Contains("Player2"))
        {
            Debug.Log($"✅ Identificado como Player2 por contenido del nombre");
            return false;
        }

        // ✅ DETECCIÓN POR ORDEN DE CREACIÓN (fallback)
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        if (allPlayers.Length >= 2)
        {
            // El primer jugador encontrado es Player1, el segundo es Player2
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i].gameObject == player)
                {
                    bool isPlayer1 = (i == 0);
                    Debug.Log($"✅ Identificado como {(isPlayer1 ? "Player1" : "Player2")} por orden (índice {i})");
                    return isPlayer1;
                }
            }
        }

        // ✅ ÚLTIMO FALLBACK: Por posición (Player1 a la izquierda, Player2 a la derecha)
        bool isPlayer1ByPosition = player.transform.position.x < 8.0f; // Umbral aproximado
        Debug.Log($"⚠️ Identificado como {(isPlayer1ByPosition ? "Player1" : "Player2")} por posición de fallback");
        return isPlayer1ByPosition;
    }

    private SequenceChallenge GetCurrentChallengeForPlayer(GameObject player)
    {
        if (challengeQueue == null || challengeQueue.Count == 0)
        {
            ShuffleChallenges();
        }

        if (challengeQueue.Count > 0)
        {
            return challengeQueue.Dequeue();
        }

        Debug.LogError("❌ No hay desafíos disponibles");
        return null;
    }

    private void ShowPlayer1Challenge(SequenceChallenge challenge)
    {
        if (challengePanel_Player1 != null)
        {
            // ✅ FORZAR RECONFIGURACIÓN ANTES DE ACTIVAR
            ConfigurePanelForCamera(challengePanel_Player1, true);

            challengePanel_Player1.SetActive(true);
            isAnyPanelActive = true;

            if (sequenceText_Player1 != null)
                sequenceText_Player1.text = challenge.sequence;
            if (hintText_Player1 != null)
                hintText_Player1.text = challenge.hint;
            if (answerInput_Player1 != null)
                answerInput_Player1.text = "";
            if (feedbackText_Player1 != null)
                feedbackText_Player1.text = "";

            if (answerInput_Player1 != null)
            {
                answerInput_Player1.interactable = true;
            }

            if (autoFocusInputField && answerInput_Player1 != null)
            {
                StartCoroutine(SelectInputFieldAfterFrame(answerInput_Player1, true));
            }

            Debug.Log("📱 Panel Player1 ACTIVADO en MITAD SUPERIOR");
        }
    }

    private void ShowPlayer2Challenge(SequenceChallenge challenge, GameObject player)
    {
        if (challengePanel_Player2 != null)
        {
            // ✅ FORZAR RECONFIGURACIÓN ANTES DE ACTIVAR
            ConfigurePanelForCamera(challengePanel_Player2, false);

            challengePanel_Player2.SetActive(true);
            isAnyPanelActive = true;

            if (sequenceText_Player2 != null)
                sequenceText_Player2.text = challenge.sequence;
            if (hintText_Player2 != null)
                hintText_Player2.text = challenge.hint;
            if (answerInput_Player2 != null)
                answerInput_Player2.text = "";
            if (feedbackText_Player2 != null)
                feedbackText_Player2.text = "";

            // Verificar si es bot para respuesta automática
            BotController botController = player.GetComponent<BotController>();
            if (botController != null && !botController.isPlayerControlled)
            {
                StartCoroutine(AutoAnswerForBot("Player2"));
            }
            else if (autoFocusInputField && answerInput_Player2 != null)
            {
                StartCoroutine(SelectInputFieldAfterFrame(answerInput_Player2, false));
            }

            Debug.Log("📱 Panel Player2 ACTIVADO en MITAD INFERIOR");
        }
    }

    private IEnumerator SelectInputFieldAfterFrame(TMP_InputField inputField, bool isPlayer1)
    {
        yield return new WaitForEndOfFrame();

        if (inputField != null && inputField.gameObject.activeInHierarchy)
        {
            inputField.Select();
            inputField.ActivateInputField();
            currentActiveInputField = inputField;
        }
    }

    public void CheckAnswer(bool isPlayer1)
    {
        string playerId = isPlayer1 ? "Player1" : "Player2";

        if (!CanProcessInput(isPlayer1)) return;

        var playerChallenge = playerStates[playerId].currentChallenge;
        string userAnswer = GetUserAnswer(isPlayer1);

        if (string.IsNullOrEmpty(userAnswer))
        {
            ShowFeedback(isPlayer1, false, "Respuesta vacía");
            return;
        }

        bool isCorrect = userAnswer.Trim().ToLower() == playerChallenge.correctAnswer.Trim().ToLower();
        playerStates[playerId].isInputEnabled = false;

        ShowFeedback(isPlayer1, isCorrect, isCorrect ? "¡Correcto!" : "Incorrecto");

        if (isCorrect)
        {
            StartCoroutine(CloseChallengeAndAdvance(playerId));
        }
        else
        {
            StartCoroutine(NextChallengeWithDelay(playerId, 2f));
        }
    }

    private string GetUserAnswer(bool isPlayer1)
    {
        if (isPlayer1 && answerInput_Player1 != null)
            return answerInput_Player1.text.Trim();
        else if (!isPlayer1 && answerInput_Player2 != null)
            return answerInput_Player2.text.Trim();
        return "";
    }

    private void ShowFeedback(bool isPlayer1, bool isCorrect, string message)
    {
        TextMeshProUGUI feedbackText = isPlayer1 ? feedbackText_Player1 : feedbackText_Player2;
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isCorrect ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(feedbackText, 2f));
        }
    }

    private IEnumerator NextChallengeWithDelay(string playerId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerStates.ContainsKey(playerId))
        {
            if (challengeQueue.Count > 0)
            {
                playerStates[playerId].currentChallenge = challengeQueue.Dequeue();
                playerStates[playerId].isInputEnabled = true;
                ShowChallengeForPlayer(playerId);
            }
            else
            {
                CompleteChallengeForPlayer(playerId);
            }
        }
    }

    IEnumerator CloseChallengeAndAdvance(string playerId)
    {
        yield return new WaitForSeconds(1.5f);

        bool isPlayer1 = playerId == "Player1";

        if (isPlayer1 && challengePanel_Player1 != null)
        {
            challengePanel_Player1.SetActive(false);
        }
        else if (!isPlayer1 && challengePanel_Player2 != null)
        {
            challengePanel_Player2.SetActive(false);
        }

        isAnyPanelActive = false;
        currentActiveInputField = null;
        CompleteChallengeForPlayer(playerId);
    }

    private void CompleteChallengeForPlayer(string playerId)
    {
        if (!playerStates.ContainsKey(playerId)) return;

        var playerState = playerStates[playerId];

        if (playerState.currentCheckpoint != null && playerState.player != null)
        {
            playerState.currentCheckpoint.CompleteChallenge(playerState.player);
        }

        playerState.isInChallenge = false;
        playerState.currentChallenge = null;
        playerState.currentCheckpoint = null;
        playerState.isInputEnabled = false;

        ReactivatePlayerControls(playerId);
    }

    private void ReactivatePlayerControls(string playerId)
    {
        if (!playerStates.ContainsKey(playerId)) return;

        var player = playerStates[playerId].player;
        if (player == null) return;

        bool isPlayer1 = playerId == "Player1";

        if (isPlayer1 && player1Controller != null)
        {
            player1Controller.SetControlsEnabled(true);
        }
        else if (!isPlayer1)
        {
            BotController botController = player.GetComponent<BotController>();
            if (botController != null)
            {
                botController.SetControlsEnabled(true);
            }
            else
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.SetControlsEnabled(true);
                }
            }
        }
    }

    private IEnumerator AutoAnswerForBot(string playerId)
    {
        if (!playerStates.ContainsKey(playerId) || playerStates[playerId].currentChallenge == null)
            yield break;

        yield return new WaitForSeconds(Random.Range(1.5f, 3f));

        var challenge = playerStates[playerId].currentChallenge;

        if (answerInput_Player2 != null)
        {
            answerInput_Player2.text = challenge.correctAnswer;
            yield return new WaitForSeconds(0.5f);
        }

        CheckAnswer(false);
    }

    private IEnumerator ClearFeedbackAfterDelay(TextMeshProUGUI feedbackText, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    private void ShowChallengeForPlayer(string playerId)
    {
        if (!playerStates.ContainsKey(playerId) || playerStates[playerId].currentChallenge == null) return;

        var playerState = playerStates[playerId];
        var player = playerState.player;
        var challenge = playerState.currentChallenge;

        bool isPlayer1 = playerId == "Player1";

        if (isPlayer1)
        {
            ShowPlayer1Challenge(challenge);
        }
        else
        {
            ShowPlayer2Challenge(challenge, player);
        }
    }

    public void OnChallengeCompleted(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning("⚠️ OnChallengeCompleted llamado con jugador nulo");
            return;
        }

        string playerId = GetPlayerId(player);
        Debug.Log($"🏁 OnChallengeCompleted llamado para: {playerId}");

        if (playerStates.ContainsKey(playerId))
        {
            CompleteChallengeForPlayer(playerId);
            Debug.Log($"✅ Desafío marcado como completado para: {playerId}");
        }
        else
        {
            Debug.LogError($"❌ No se pudo completar desafío - {playerId} no encontrado en estados");
        }
    }

    private string GetPlayerId(GameObject player)
    {
        if (player == null) return "Unknown";

        bool isPlayer1 = DeterminePlayerCorrectly(player);
        return isPlayer1 ? "Player1" : "Player2";
    }

    public string GetCorrectAnswer()
    {
        foreach (var state in playerStates.Values)
        {
            if (state.currentChallenge != null)
            {
                return state.currentChallenge.correctAnswer;
            }
        }
        return "";
    }

    [ContextMenu("🔧 FORZAR DETECCIÓN DE JUGADORES")]
    public void ForzarDeteccionJugadores()
    {
        Debug.Log("🔄 Forzando detección de jugadores...");
        FindPlayerControllers();

        // Verificar estado actual
        Debug.Log("🔍 ESTADO ACTUAL DE JUGADORES:");
        foreach (var kvp in playerStates)
        {
            Debug.Log($"   {kvp.Key}: {(kvp.Value.player != null ? kvp.Value.player.name : "NULL")}");
        }
    }

    [ContextMenu("🎯 SOLUCIÓN DEFINITIVA PANELES")]
    public void SolucionDefinitivaPaneles()
    {
        Debug.Log("🎯 APLICANDO SOLUCIÓN DEFINITIVA PARA PANELES...");

        // Reconfigurar ambos paneles
        if (challengePanel_Player1 != null)
        {
            ConfigurePanelForCamera(challengePanel_Player1, true);
            Debug.Log($"✅ Panel Player1 reconfigurado para MITAD SUPERIOR");
        }

        if (challengePanel_Player2 != null)
        {
            ConfigurePanelForCamera(challengePanel_Player2, false);
            Debug.Log($"✅ Panel Player2 reconfigurado para MITAD INFERIOR");
        }

        Debug.Log("🎯 SOLUCIÓN DEFINITIVA APLICADA");
    }
}