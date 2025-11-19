using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    [Header("Referencias UI por Jugador")]
    public GameObject countdownPanel_Player1;
    public GameObject countdownPanel_Player2;
    public TextMeshProUGUI countdownText_Player1;
    public TextMeshProUGUI countdownText_Player2;
    public TextMeshProUGUI instructionText_Player1;
    public TextMeshProUGUI instructionText_Player2;

    [Header("Challenge Canvases")]
    public GameObject challengeCanvas_Player1;
    public GameObject challengeCanvas_Player2;

    [Header("Configuración")]
    public Transform player1;
    public Transform player2;
    public Transform startPoint;
    public float activationRadius = 10f;
    public float searchInterval = 0.5f;

    private bool countdownStarted = false;
    private bool gameStarted = false;
    private float lastSearchTime = 0f;
    private bool isEnabled = false;
    private bool waitingForModeSelection = true;
    private bool hasBeenActivated = false;

    void Start()
    {
        if (!hasBeenActivated)
        {
            Debug.Log("🛑 COUNTDOWNMANAGER - INICIANDO EN MODO DESACTIVADO");

            this.enabled = false;
            isEnabled = false;
            waitingForModeSelection = true;
            countdownStarted = false;
            gameStarted = false;

            ForceDisableAllPanels();

            Debug.Log("✅ CountdownManager inicializado en estado DESACTIVADO");
        }
        else
        {
            Debug.Log("✅ CountdownManager ya activado - Ignorando Start()");
        }
    }

    void ForceDisableAllPanels()
    {
        if (countdownPanel_Player1 != null)
        {
            countdownPanel_Player1.SetActive(false);
            Debug.Log("🚫 CountdownPanel_Player1 FORZADO A FALSE");
        }
        if (countdownPanel_Player2 != null)
        {
            countdownPanel_Player2.SetActive(false);
            Debug.Log("🚫 CountdownPanel_Player2 FORZADO A FALSE");
        }
        if (challengeCanvas_Player1 != null)
        {
            challengeCanvas_Player1.SetActive(false);
            Debug.Log("🚫 ChallengeCanvas_Player1 FORZADO A FALSE");
        }
        if (challengeCanvas_Player2 != null)
        {
            challengeCanvas_Player2.SetActive(false);
            Debug.Log("🚫 ChallengeCanvas_Player2 FORZADO A FALSE");
        }
    }

    public void OnGameModeSelected()
    {
        Debug.Log("✅ CountdownManager: SOLICITUD DE ACTIVACIÓN RECIBIDA");

        hasBeenActivated = true;
        this.enabled = true;
        isEnabled = true;
        waitingForModeSelection = false;

        ForceDisableAllPanels();

        Debug.Log("🔄 CountdownManager: Iniciando búsqueda de jugadores...");

        if (this.isActiveAndEnabled)
        {
            StartCoroutine(FindPlayersDelayed());
        }
        else
        {
            Debug.LogError("❌ CountdownManager no está activo - no se puede iniciar corrutina");
        }
    }

    IEnumerator FindPlayersDelayed()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("🔍 CountdownManager: Buscando jugadores activos...");
        FindPlayersIfMissing();

        if (player1 != null && player2 != null && startPoint != null)
        {
            Debug.Log($"📍 Posiciones iniciales - Player1: {player1.position}, Player2: {player2.position}, StartPoint: {startPoint.position}");
            Debug.Log($"📍 Distancias - P1->SP: {Vector3.Distance(player1.position, startPoint.position):F1}, P2->SP: {Vector3.Distance(player2.position, startPoint.position):F1}");
        }

        while (!gameStarted && isEnabled)
        {
            if (player1 == null || player2 == null)
            {
                FindPlayersIfMissing();
            }

            if (player1 != null && player2 != null && startPoint != null && Time.frameCount % 60 == 0)
            {
                float dist1 = Vector3.Distance(player1.position, startPoint.position);
                float dist2 = Vector3.Distance(player2.position, startPoint.position);
                Debug.Log($"📍 CountdownManager - Player1: {dist1:F1}u, Player2: {dist2:F1}u, Radio: {activationRadius}u");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void FindPlayersIfMissing()
    {
        if (player1 == null)
        {
            PlayerController[] controllers = FindObjectsOfType<PlayerController>();
            Debug.Log($"🔍 Total PlayerControllers encontrados: {controllers.Length}");

            foreach (PlayerController pc in controllers)
            {
                if (pc.gameObject.activeInHierarchy && pc.gameObject.name == "Player1")
                {
                    player1 = pc.transform;
                    Debug.Log($"✅ Player1 encontrado: {player1.name} en {player1.position}");
                    break;
                }
            }

            if (player1 == null)
            {
                GameObject p1 = GameObject.FindGameObjectWithTag("Player");
                if (p1 != null && p1.activeInHierarchy && p1.name != "Player2")
                {
                    player1 = p1.transform;
                    Debug.Log($"✅ Player1 encontrado por tag: {player1.name}");
                }
            }
        }

        if (player2 == null)
        {
            GameObject p2 = GameObject.Find("Player2");
            if (p2 != null && p2.activeInHierarchy)
            {
                player2 = p2.transform;
                Debug.Log($"✅ Player2 encontrado por nombre: {player2.name} en {player2.position}");
            }
            else
            {
                GameObject bot = GameObject.FindGameObjectWithTag("Bot");
                if (bot != null && bot.activeInHierarchy)
                {
                    player2 = bot.transform;
                    Debug.Log($"✅ Player2 encontrado por tag Bot: {player2.name}");
                }
            }
        }

        if (startPoint == null)
        {
            GameObject sp = GameObject.Find("StartPoint");
            if (sp != null)
            {
                startPoint = sp.transform;
                Debug.Log($"✅ StartPoint encontrado: {startPoint.name} en {startPoint.position}");
            }
            else
            {
                CreateAutoStartPoint();
            }
        }

        if (player1 == null) Debug.LogWarning("⚠️ Player1 NO ENCONTRADO");
        if (player2 == null) Debug.LogWarning("⚠️ Player2 NO ENCONTRADO");
        if (startPoint == null) Debug.LogError("❌ StartPoint NO ENCONTRADO");
    }

    void CreateAutoStartPoint()
    {
        Debug.Log("📍 Creando StartPoint automático...");

        GameObject startPointObj = new GameObject("AutoStartPoint");
        startPoint = startPointObj.transform;

        if (player1 != null)
        {
            startPoint.position = player1.position;
        }
        else
        {
            startPoint.position = new Vector3(0, -50, 0);
        }

        Debug.Log($"✅ StartPoint automático creado en: {startPoint.position}");
    }

    void Update()
    {
        if (!isEnabled || waitingForModeSelection || gameStarted || countdownStarted)
            return;

        if (Time.time - lastSearchTime > searchInterval)
        {
            if (player1 == null || player2 == null || startPoint == null)
            {
                FindPlayersIfMissing();
                lastSearchTime = Time.time;
            }
        }

        if (player1 != null && player2 != null && startPoint != null)
        {
            CheckPlayersPosition();
        }
    }

    void CheckPlayersPosition()
    {
        float dist1 = Vector3.Distance(player1.position, startPoint.position);
        float dist2 = Vector3.Distance(player2.position, startPoint.position);

        bool player1InZone = dist1 <= activationRadius;
        bool player2InZone = dist2 <= activationRadius;

        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"📍 CountdownManager - Player1: {dist1:F1}u | Player2: {dist2:F1}u | Radio: {activationRadius}u | EnZona: P1={player1InZone}, P2={player2InZone}");
        }

        if (player1InZone && player2InZone && !countdownStarted)
        {
            Debug.Log("🎯 AMBOS jugadores en zona - Iniciando countdown!");
            StartCoroutine(StartCountdown());
        }
        else if ((player1InZone || player2InZone) && !countdownStarted)
        {
            UpdateWaitingUI(player1InZone, player2InZone);
        }
        else if (!player1InZone && !player2InZone)
        {
            HideAllPanels();
        }
    }

    void UpdateWaitingUI(bool p1InZone, bool p2InZone)
    {
        if (p1InZone && countdownPanel_Player1 != null)
        {
            countdownPanel_Player1.SetActive(true);
            if (countdownText_Player1 != null)
                countdownText_Player1.text = "Esperando al otro jugador...";
            Debug.Log("📺 CountdownPanel_Player1 mostrado: Esperando al otro jugador");
        }

        if (p2InZone && countdownPanel_Player2 != null)
        {
            countdownPanel_Player2.SetActive(true);
            if (countdownText_Player2 != null)
                countdownText_Player2.text = "Esperando al otro jugador...";
            Debug.Log("📺 CountdownPanel_Player2 mostrado: Esperando al otro jugador");
        }
    }

    void HideAllPanels()
    {
        if (countdownPanel_Player1 != null)
            countdownPanel_Player1.SetActive(false);
        if (countdownPanel_Player2 != null)
            countdownPanel_Player2.SetActive(false);
    }

    IEnumerator StartCountdown()
    {
        countdownStarted = true;

        if (countdownPanel_Player1 != null)
        {
            countdownPanel_Player1.SetActive(true);
            Debug.Log("📺 Panel Player1 activado");
        }

        if (countdownPanel_Player2 != null)
        {
            countdownPanel_Player2.SetActive(true);
            Debug.Log("📺 Panel Player2 activado");
        }

        if (instructionText_Player1 != null)
            instructionText_Player1.text = "¡Ambos jugadores listos!";
        if (instructionText_Player2 != null)
            instructionText_Player2.text = "¡Ambos jugadores listos!";

        Debug.Log("🔴 COUNTDOWN INICIADO - 3 segundos");

        for (int i = 3; i > 0; i--)
        {
            if (countdownText_Player1 != null)
                countdownText_Player1.text = i.ToString();
            if (countdownText_Player2 != null)
                countdownText_Player2.text = i.ToString();

            Debug.Log($"⏱️ Countdown: {i}");
            yield return new WaitForSeconds(1f);

            if (!ArePlayersInZone())
            {
                InterruptCountdown();
                yield break;
            }
        }

        CompleteCountdown();
    }

    bool ArePlayersInZone()
    {
        if (player1 == null || player2 == null || startPoint == null)
            return false;

        float dist1 = Vector3.Distance(player1.position, startPoint.position);
        float dist2 = Vector3.Distance(player2.position, startPoint.position);

        return dist1 <= activationRadius && dist2 <= activationRadius;
    }

    void InterruptCountdown()
    {
        if (countdownText_Player1 != null)
            countdownText_Player1.text = "¡Interrumpido!";
        if (countdownText_Player2 != null)
            countdownText_Player2.text = "¡Interrumpido!";

        Debug.Log("❌ Countdown INTERRUMPIDO");

        StartCoroutine(HidePanelsAfterDelay(2f));
    }

    void CompleteCountdown()
    {
        if (countdownText_Player1 != null)
            countdownText_Player1.text = "¡GO!";
        if (countdownText_Player2 != null)
            countdownText_Player2.text = "¡GO!";

        Debug.Log("✅ COUNTDOWN COMPLETADO - ¡GO!");

        StartCoroutine(FinishCountdownSequence());
    }

    IEnumerator HidePanelsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        HideAllPanels();
        countdownStarted = false;
    }

    IEnumerator FinishCountdownSequence()
    {
        yield return new WaitForSeconds(1f);

        HideAllPanels();

        if (challengeCanvas_Player1 != null)
        {
            challengeCanvas_Player1.SetActive(true);
            Debug.Log("📺 ChallengeCanvas_Player1 ACTIVADO");
        }
        if (challengeCanvas_Player2 != null)
        {
            challengeCanvas_Player2.SetActive(true);
            Debug.Log("📺 ChallengeCanvas_Player2 ACTIVADO");
        }

        gameStarted = true;
        Debug.Log("🎮 JUEGO INICIADO - CountdownManager completado");

        EnableAllPlayerControls();

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
        }

        // ✅ NUEVO: Notificar al SequenceChallengeManager que el juego comenzó
        SequenceChallengeManager sequenceManager = FindObjectOfType<SequenceChallengeManager>();
        if (sequenceManager != null)
        {
            sequenceManager.OnGameStarted();
            Debug.Log("🎮 Notificando SequenceChallengeManager que el juego comenzó");
        }

        Debug.Log("✅ CONTROLES ACTIVADOS después del countdown");
    }

    void EnableAllPlayerControls()
    {
        Debug.Log("🔓 ACTIVANDO CONTROLES PARA TODOS LOS JUGADORES");

        PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController pc in playerControllers)
        {
            if (pc.gameObject.activeInHierarchy)
            {
                pc.SetControlsEnabled(true);
                Debug.Log($"🎮 Controles activados para: {pc.gameObject.name}");
            }
        }

        BotController[] botControllers = FindObjectsOfType<BotController>();
        foreach (BotController bot in botControllers)
        {
            if (bot.gameObject.activeInHierarchy && bot.isPlayerControlled)
            {
                bot.SetControlsEnabled(true);
                Debug.Log($"🤖 Controles activados para bot jugador: {bot.gameObject.name}");
            }
        }
    }

    public void ShowCountdown()
    {
        Debug.Log("🟢 CountdownManager: ShowCountdown() llamado");
    }

    public System.Collections.IEnumerator WaitForCountdown()
    {
        Debug.Log("⏳ CountdownManager: Esperando que countdown se complete...");
        yield return new WaitUntil(() => gameStarted);
        Debug.Log("✅ CountdownManager: Countdown completado");
    }

    public void OnPlayer2Created(Transform newPlayer2)
    {
        player2 = newPlayer2;
        Debug.Log($"🎮 CountdownManager: Player2 asignado - {player2.name} en {player2.position}");
    }

    public void HideCountdown()
    {
        Debug.Log("🔴 CountdownManager: HideCountdown() llamado");
        ForceDisableAllPanels();
        countdownStarted = false;
        gameStarted = false;
        isEnabled = false;
    }

    public void ForceStartGame()
    {
        Debug.Log("🚀 CountdownManager: Forzando inicio de juego");
        gameStarted = true;
        ForceDisableAllPanels();
    }
}