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
    public bool isPanelActive;
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

    [Header("Configuración de Validación")]
    public bool onlyNumbers = true;
    public int maxDigits = 3;

    [Header("Desafíos de Secuencia")]
    public List<SequenceChallenge> challenges = new List<SequenceChallenge>();

    private Queue<SequenceChallenge> challengeQueue;
    private Dictionary<string, PlayerChallengeState> playerStates = new Dictionary<string, PlayerChallengeState>();
    private PlayerController player1Controller;
    private BotController player2Controller;
    private bool playersInitialized = false;
    private List<TMP_InputField> activeInputFields = new List<TMP_InputField>();
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
        SetupInputFieldValidation();
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
        SetupCameraFollowScripts();
    }

    private void ConfigureSplitScreenCameras()
    {
        if (player1Camera != null && player2Camera != null)
        {
            player1Camera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            player2Camera.rect = new Rect(0f, 0f, 1f, 0.5f);

            Debug.Log("🎥 Cámaras configuradas para pantalla dividida");
            Debug.Log($"   Player1Camera: {player1Camera.rect}");
            Debug.Log($"   Player2Camera: {player2Camera.rect}");
        }
    }

    private void SetupCameraFollowScripts()
    {
        Debug.Log("🎯 Configurando scripts de seguimiento de cámara...");

        if (player1Camera != null)
        {
            SetupCameraForPlayer(player1Camera, "Player1", "Player1Camera");
        }

        if (player2Camera != null)
        {
            SetupCameraForPlayer(player2Camera, "Player2", "Player2Camera");
        }
    }

    private void SetupCameraForPlayer(Camera camera, string targetPlayerName, string cameraName)
    {
        SeguirJugador followScript = camera.GetComponent<SeguirJugador>();
        if (followScript == null)
        {
            followScript = camera.gameObject.AddComponent<SeguirJugador>();
            Debug.Log($"✅ Script SeguirJugador añadido a {cameraName}");
        }

        followScript.isTopScreen = (targetPlayerName == "Player1");
        followScript.playerTargetName = targetPlayerName;

        Debug.Log($"🎯 Configurando {cameraName} para seguir a {targetPlayerName} - TopScreen: {followScript.isTopScreen}");

        followScript.ForceFindTarget();
    }

    private void InitializePlayerStates()
    {
        playerStates.Clear();
        playerStates.Add("Player1", new PlayerChallengeState
        {
            isInChallenge = false,
            isInputEnabled = false,
            isPanelActive = false
        });
        playerStates.Add("Player2", new PlayerChallengeState
        {
            isInChallenge = false,
            isInputEnabled = false,
            isPanelActive = false
        });
        Debug.Log("✅ Estados de jugadores inicializados: Player1 y Player2");
    }

    void Update()
    {
        if (isGameStarted && Input.GetKeyDown(submitKey))
        {
            HandleEnterKeyPress();
        }
    }

    public void OnGameStarted()
    {
        isGameStarted = true;
        Debug.Log("🎮 SequenceChallengeManager: Juego iniciado - Input habilitado");

        StartCoroutine(ReinforceCameraSetup());
    }

    private IEnumerator ReinforceCameraSetup()
    {
        yield return new WaitForSeconds(0.5f);
        SetupCameraFollowScripts();
    }

    private void HandleEnterKeyPress()
    {
        if (!isGameStarted) return;

        bool player1Active = playerStates.ContainsKey("Player1") &&
                            playerStates["Player1"].isPanelActive &&
                            playerStates["Player1"].isInputEnabled;

        bool player2Active = playerStates.ContainsKey("Player2") &&
                            playerStates["Player2"].isPanelActive &&
                            playerStates["Player2"].isInputEnabled;

        if (player1Active && player2Active)
        {
            GameObject focusedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            if (focusedObject == answerInput_Player1?.gameObject)
            {
                CheckAnswer(true);
            }
            else if (focusedObject == answerInput_Player2?.gameObject)
            {
                CheckAnswer(false);
            }
            else
            {
                CheckAnswer(true);
            }
        }
        else if (player1Active)
        {
            CheckAnswer(true);
        }
        else if (player2Active)
        {
            CheckAnswer(false);
        }
    }

    private void SetupInputFieldValidation()
    {
        Debug.Log("🔢 Configurando validación para solo números...");

        if (answerInput_Player1 != null)
        {
            answerInput_Player1.contentType = TMP_InputField.ContentType.IntegerNumber;
            answerInput_Player1.onValidateInput = ValidateNumericInput;
            answerInput_Player1.characterLimit = maxDigits;
            answerInput_Player1.onValueChanged.AddListener((text) => OnInputValueChanged(text, true));
            Debug.Log("✅ Validación configurada para Player1 InputField");
        }

        if (answerInput_Player2 != null)
        {
            answerInput_Player2.contentType = TMP_InputField.ContentType.IntegerNumber;
            answerInput_Player2.onValidateInput = ValidateNumericInput;
            answerInput_Player2.characterLimit = maxDigits;
            answerInput_Player2.onValueChanged.AddListener((text) => OnInputValueChanged(text, false));
            Debug.Log("✅ Validación configurada para Player2 InputField");
        }
    }

    private char ValidateNumericInput(string text, int charIndex, char addedChar)
    {
        if (!onlyNumbers)
        {
            return addedChar;
        }

        if (char.IsDigit(addedChar))
        {
            if (text.Length < maxDigits)
            {
                return addedChar;
            }
            else
            {
                ShowTemporaryFeedback("¡Máximo " + maxDigits + " dígitos!");
                return '\0';
            }
        }
        else
        {
            if (!char.IsControl(addedChar))
            {
                ShowTemporaryFeedback("¡Solo se permiten números!");
            }
            return '\0';
        }
    }

    private void OnInputValueChanged(string newText, bool isPlayer1)
    {
        if (!onlyNumbers) return;

        string cleanedText = CleanNumericString(newText);

        if (cleanedText != newText)
        {
            if (isPlayer1 && answerInput_Player1 != null)
            {
                answerInput_Player1.text = cleanedText;
            }
            else if (!isPlayer1 && answerInput_Player2 != null)
            {
                answerInput_Player2.text = cleanedText;
            }
        }

        if (newText.Length > maxDigits)
        {
            string truncatedText = newText.Substring(0, maxDigits);
            if (isPlayer1 && answerInput_Player1 != null)
            {
                answerInput_Player1.text = truncatedText;
            }
            else if (!isPlayer1 && answerInput_Player2 != null)
            {
                answerInput_Player2.text = truncatedText;
            }

            ShowTemporaryFeedback("¡Máximo " + maxDigits + " dígitos!");
        }
    }

    private string CleanNumericString(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        System.Text.StringBuilder cleaned = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                cleaned.Append(c);
            }
        }
        return cleaned.ToString();
    }

    private void ShowTemporaryFeedback(string message)
    {
        Debug.Log($"⚠️ {message}");
        StartCoroutine(ClearTemporaryFeedback());
    }

    private IEnumerator ClearTemporaryFeedback()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private void SetupInputFieldListeners()
    {
        if (answerInput_Player1 != null)
        {
            answerInput_Player1.onSelect.AddListener((text) => OnInputFieldSelected(answerInput_Player1, true));
            answerInput_Player1.onDeselect.AddListener((text) => OnInputFieldDeselected(answerInput_Player1, true));
        }

        if (answerInput_Player2 != null)
        {
            answerInput_Player2.onSelect.AddListener((text) => OnInputFieldSelected(answerInput_Player2, false));
            answerInput_Player2.onDeselect.AddListener((text) => OnInputFieldDeselected(answerInput_Player2, false));
        }
    }

    private void OnInputFieldSelected(TMP_InputField inputField, bool isPlayer1)
    {
        string playerId = isPlayer1 ? "Player1" : "Player2";

        if (playerStates.ContainsKey(playerId))
        {
            playerStates[playerId].isInputEnabled = true;
        }

        if (!activeInputFields.Contains(inputField))
        {
            activeInputFields.Add(inputField);
        }

        Debug.Log($"🎯 InputField seleccionado: {playerId}");
    }

    private void OnInputFieldDeselected(TMP_InputField inputField, bool isPlayer1)
    {
        if (activeInputFields.Contains(inputField))
        {
            activeInputFields.Remove(inputField);
        }

        Debug.Log($"🎯 InputField deseleccionado: {(isPlayer1 ? "Player1" : "Player2")}");
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

        bool isPlayer1 = DeterminePlayerCorrectly(player);

        SequenceChallenge currentChallenge = GetCurrentChallengeForPlayer(player);
        if (currentChallenge == null)
        {
            Debug.LogError("❌ No se pudo obtener desafío actual");
            return;
        }

        // ✅ SOLO DESACTIVAR CONTROLES DEL JUGADOR QUE ACTIVÓ EL CHECKPOINT
        DisablePlayerControls(player);

        string playerId = isPlayer1 ? "Player1" : "Player2";
        if (playerStates.ContainsKey(playerId))
        {
            playerStates[playerId].currentChallenge = currentChallenge;
            playerStates[playerId].currentCheckpoint = checkpoint;
            playerStates[playerId].isInChallenge = true;
            playerStates[playerId].isInputEnabled = true;
            playerStates[playerId].isPanelActive = true;
            playerStates[playerId].player = player;
        }

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

        CheckMultipleActivePanels();
    }

    private void CheckMultipleActivePanels()
    {
        bool player1Active = playerStates.ContainsKey("Player1") && playerStates["Player1"].isPanelActive;
        bool player2Active = playerStates.ContainsKey("Player2") && playerStates["Player2"].isPanelActive;

        if (player1Active && player2Active)
        {
            Debug.Log("🎯 AMBOS PANELES ACTIVOS - Input simultáneo habilitado");

            if (answerInput_Player1 != null)
            {
                answerInput_Player1.interactable = true;
            }
            if (answerInput_Player2 != null)
            {
                answerInput_Player2.interactable = true;
            }

            if (autoFocusInputField)
            {
                StartCoroutine(FocusBothInputFields());
            }
        }
    }

    private IEnumerator FocusBothInputFields()
    {
        yield return new WaitForEndOfFrame();

        if (answerInput_Player1 != null && answerInput_Player1.gameObject.activeInHierarchy)
        {
            answerInput_Player1.Select();
            answerInput_Player1.ActivateInputField();

            if (!activeInputFields.Contains(answerInput_Player1))
            {
                activeInputFields.Add(answerInput_Player1);
            }
        }
    }

    private void ShowPlayer1Challenge(SequenceChallenge challenge)
    {
        if (challengePanel_Player1 != null)
        {
            ConfigurePanelForCamera(challengePanel_Player1, true);

            challengePanel_Player1.SetActive(true);

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

            bool otherPanelActive = playerStates.ContainsKey("Player2") && playerStates["Player2"].isPanelActive;

            if (autoFocusInputField && answerInput_Player1 != null && !otherPanelActive)
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
            ConfigurePanelForCamera(challengePanel_Player2, false);

            challengePanel_Player2.SetActive(true);

            if (sequenceText_Player2 != null)
                sequenceText_Player2.text = challenge.sequence;
            if (hintText_Player2 != null)
                hintText_Player2.text = challenge.hint;
            if (answerInput_Player2 != null)
                answerInput_Player2.text = "";
            if (feedbackText_Player2 != null)
                feedbackText_Player2.text = "";

            if (answerInput_Player2 != null)
            {
                answerInput_Player2.interactable = true;
            }

            BotController botController = player.GetComponent<BotController>();
            if (botController != null && !botController.isPlayerControlled)
            {
                StartCoroutine(AutoAnswerForBot("Player2"));
            }
            else if (autoFocusInputField && answerInput_Player2 != null)
            {
                bool otherPanelActive = playerStates.ContainsKey("Player1") && playerStates["Player1"].isPanelActive;

                if (!otherPanelActive)
                {
                    StartCoroutine(SelectInputFieldAfterFrame(answerInput_Player2, false));
                }
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

            if (!activeInputFields.Contains(inputField))
            {
                activeInputFields.Add(inputField);
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
        return state.isInChallenge && state.isInputEnabled && state.currentChallenge != null && state.isPanelActive;
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

        if (onlyNumbers && !IsValidNumericAnswer(userAnswer))
        {
            ShowFeedback(isPlayer1, false, "¡Solo se permiten números!");
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

    private bool IsValidNumericAnswer(string answer)
    {
        if (string.IsNullOrEmpty(answer)) return false;

        foreach (char c in answer)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }

    private string GetUserAnswer(bool isPlayer1)
    {
        string answer = "";

        if (isPlayer1 && answerInput_Player1 != null)
            answer = answerInput_Player1.text.Trim();
        else if (!isPlayer1 && answerInput_Player2 != null)
            answer = answerInput_Player2.text.Trim();

        if (onlyNumbers)
        {
            answer = CleanNumericString(answer);
        }

        return answer;
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

        if (isPlayer1 && challengePanel_Player1 != null && playerStates[playerId].isPanelActive)
        {
            challengePanel_Player1.SetActive(false);
        }
        else if (!isPlayer1 && challengePanel_Player2 != null && playerStates[playerId].isPanelActive)
        {
            challengePanel_Player2.SetActive(false);
        }

        TMP_InputField inputFieldToRemove = isPlayer1 ? answerInput_Player1 : answerInput_Player2;
        if (inputFieldToRemove != null && activeInputFields.Contains(inputFieldToRemove))
        {
            activeInputFields.Remove(inputFieldToRemove);
        }

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

        // ✅ SOLO REACTIVAR CONTROLES DEL JUGADOR QUE COMPLETÓ EL DESAFÍO
        if (playerState.player != null)
        {
            EnablePlayerControls(playerState.player);
        }

        playerState.isInChallenge = false;
        playerState.currentChallenge = null;
        playerState.currentCheckpoint = null;
        playerState.isInputEnabled = false;
        playerState.isPanelActive = false;

        Debug.Log($"✅ Desafío completado para: {playerId}");
    }

    // ✅ MÉTODO: Desactivar controles solo del jugador específico
    private void DisablePlayerControls(GameObject player)
    {
        if (player == null) return;

        // Desactivar PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetControlsEnabled(false);
            Debug.Log($"🎮 Controles DESACTIVADOS para: {player.name}");
        }

        // Desactivar BotController si es Player2 controlado por jugador
        BotController botController = player.GetComponent<BotController>();
        if (botController != null && botController.isPlayerControlled)
        {
            botController.SetControlsEnabled(false);
            Debug.Log($"🎮 Controles DESACTIVADOS para Bot (Player2): {player.name}");
        }

        // Detener movimiento físico
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            Debug.Log($"🛑 Movimiento detenido para: {player.name}");
        }
    }

    // ✅ MÉTODO: Reactivar controles solo del jugador específico
    private void EnablePlayerControls(GameObject player)
    {
        if (player == null) return;

        // Reactivar PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetControlsEnabled(true);
            Debug.Log($"🎮 Controles REACTIVADOS para: {player.name}");
        }

        // Reactivar BotController si es Player2 controlado por jugador
        BotController botController = player.GetComponent<BotController>();
        if (botController != null && botController.isPlayerControlled)
        {
            botController.SetControlsEnabled(true);
            Debug.Log($"🎮 Controles REACTIVADOS para Bot (Player2): {player.name}");
        }
    }

    private bool DeterminePlayerCorrectly(GameObject player)
    {
        string playerName = player.name;

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

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        if (allPlayers.Length >= 2)
        {
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

        bool isPlayer1ByPosition = player.transform.position.x < 8.0f;
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

        activeInputFields.Clear();
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

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = isPlayer1Panel ? 10000 : 9999;

        CanvasScaler scaler = panel.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = panel.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            if (isPlayer1Panel)
            {
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                Debug.Log($"✅ Panel {panel.name} configurado para MITAD SUPERIOR (Player1)");
            }
            else
            {
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
            new SequenceChallenge { sequence = "3, 6, 9, 12, ?", correctAnswer = "15", hint = "Multiplica por 3" }
        };
    }

    IEnumerator FindPlayersWithDelay()
    {
        yield return new WaitForSeconds(1f);
        FindPlayerControllers();
        playersInitialized = true;

        SetupCameraFollowScripts();
    }

    public void FindPlayerControllers()
    {
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

    [ContextMenu("🔧 FORZAR DETECCIÓN DE JUGADORES")]
    public void ForzarDeteccionJugadores()
    {
        Debug.Log("🔄 Forzando detección de jugadores...");
        FindPlayerControllers();

        Debug.Log("🔍 ESTADO ACTUAL DE JUGADORES:");
        foreach (var kvp in playerStates)
        {
            Debug.Log($"   {kvp.Key}: {(kvp.Value.player != null ? kvp.Value.player.name : "NULL")}");
        }
    }

    [ContextMenu("🎯 CONFIGURAR SEGUIMIENTO DE CÁMARAS")]
    public void ConfigurarSeguimientoCamaras()
    {
        Debug.Log("🎯 Configurando seguimiento de cámaras...");
        SetupCameraFollowScripts();
    }

    [ContextMenu("🎯 SOLUCIÓN DEFINITIVA PANELES")]
    public void SolucionDefinitivaPaneles()
    {
        Debug.Log("🎯 APLICANDO SOLUCIÓN DEFINITIVA PARA PANELES...");

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

    [ContextMenu("🔍 DIAGNÓSTICO COMPLETO DEL SISTEMA")]
    public void DiagnosticoCompletoSistema()
    {
        Debug.Log("=== DIAGNÓSTICO COMPLETO DEL SISTEMA ===");

        Debug.Log($"📷 Player1Camera: {player1Camera?.name} | Activa: {player1Camera?.isActiveAndEnabled}");
        Debug.Log($"📷 Player2Camera: {player2Camera?.name} | Activa: {player2Camera?.isActiveAndEnabled}");

        if (player1Camera != null)
        {
            SeguirJugador follow1 = player1Camera.GetComponent<SeguirJugador>();
            Debug.Log($"🎯 SeguirJugador en Player1Camera: {(follow1 != null ? "PRESENTE" : "AUSENTE")}");
        }

        if (player2Camera != null)
        {
            SeguirJugador follow2 = player2Camera.GetComponent<SeguirJugador>();
            Debug.Log($"🎯 SeguirJugador en Player2Camera: {(follow2 != null ? "PRESENTE" : "AUSENTE")}");
        }

        Debug.Log("🔍 JUGADORES EN ESCENA:");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Debug.Log($"   👤 {player.name} | Posición: {player.transform.position}");
        }

        Debug.Log("=== FIN DIAGNÓSTICO ===");
    }
}