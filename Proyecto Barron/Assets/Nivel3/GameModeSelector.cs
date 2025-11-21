using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameModeSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject modeSelectionPanel;
    public Button singlePlayerButton;
    public Button twoPlayersButton;

    [Header("Player Prefabs")]
    public GameObject playerPrefab;
    public GameObject botPrefab;

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform player2SpawnPoint;
    public Transform botSpawnPoint;

    [Header("Game References")]
    public CountdownManager countdownManager;
    public SplitScreenManager splitScreenManager;

    private bool gameStarted = false;
    private bool twoPlayerMode = false;
    private bool vsBotMode = false;
    private GameObject currentPlayer1;
    private GameObject currentPlayer2;
    private GameObject currentBot;

    void Start()
    {
        Debug.Log("✅ GameModeSelector iniciado");

        gameStarted = false;
        twoPlayerMode = false;
        vsBotMode = false;

        FindSpawnPointsIfNull();

        if (splitScreenManager == null)
            splitScreenManager = FindObjectOfType<SplitScreenManager>();

        if (countdownManager == null)
            countdownManager = FindObjectOfType<CountdownManager>();

        SetupPanels();

        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.RemoveAllListeners();
            singlePlayerButton.onClick.AddListener(() => StartCoroutine(SelectGameMode(true)));
            Debug.Log("✅ Single Player Button configurado como MODO VS BOT");
        }

        if (twoPlayersButton != null)
        {
            twoPlayersButton.onClick.RemoveAllListeners();
            twoPlayersButton.onClick.AddListener(() => StartCoroutine(SelectGameMode(false)));
            Debug.Log("✅ Two Players Button configurado como MODO 2 JUGADORES");
        }

        DisableAllControls();
    }

    void FindSpawnPointsIfNull()
    {
        if (playerSpawnPoint == null)
        {
            GameObject playerSpawn = GameObject.Find("PlayerSpawnPoint");
            if (playerSpawn != null)
            {
                playerSpawnPoint = playerSpawn.transform;
            }
        }

        if (player2SpawnPoint == null)
        {
            GameObject player2Spawn = GameObject.Find("Player2SpawnPoint");
            if (player2Spawn != null)
            {
                player2SpawnPoint = player2Spawn.transform;
            }
            else if (playerSpawnPoint != null)
            {
                GameObject spawnObj = new GameObject("Player2SpawnPoint");
                spawnObj.transform.position = playerSpawnPoint.position + new Vector3(2f, 0f, 0f);
                player2SpawnPoint = spawnObj.transform;
            }
        }

        if (botSpawnPoint == null)
        {
            GameObject botSpawn = GameObject.Find("BotSpawnPoint");
            if (botSpawn != null)
            {
                botSpawnPoint = botSpawn.transform;
            }
            else
            {
                GameObject spawnObj = new GameObject("BotSpawnPoint");

                GameObject existingPlayer1 = GameObject.Find("Player1");
                if (existingPlayer1 != null)
                {
                    spawnObj.transform.position = existingPlayer1.transform.position + new Vector3(-2f, 0f, 0f);
                    Debug.Log($"📍 Spawn point del bot creado cerca de Player1 existente: {spawnObj.transform.position}");
                }
                else if (playerSpawnPoint != null)
                {
                    spawnObj.transform.position = playerSpawnPoint.position + new Vector3(-2f, 0f, 0f);
                    Debug.Log($"📍 Spawn point del bot creado cerca del spawn del jugador: {spawnObj.transform.position}");
                }
                else
                {
                    spawnObj.transform.position = new Vector3(5.10f, -53.98f, 0f);
                    Debug.Log($"📍 Spawn point del bot creado en posición de emergencia: {spawnObj.transform.position}");
                }

                botSpawnPoint = spawnObj.transform;
            }
        }
    }

    void SetupPanels()
    {
        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(true);
        }

        CleanupExistingPlayers();
    }

    void CleanupExistingPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.name != "Player1" && player.name != "Player2")
            {
                Destroy(player);
            }
        }

        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
        foreach (GameObject bot in bots)
        {
            if (bot != currentBot)
            {
                Destroy(bot);
            }
        }
    }

    IEnumerator SelectGameMode(bool isVsBotMode)
    {
        if (gameStarted) yield break;

        string modeName = isVsBotMode ? "VS BOT" : "2 JUGADORES";
        Debug.Log($"🎮 Usuario seleccionó: {modeName}");

        twoPlayerMode = !isVsBotMode;
        vsBotMode = isVsBotMode;
        gameStarted = true;

        ForceHideSelectionPanelImmediately();
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(StartGameplayCoroutine());
    }

    void ForceHideSelectionPanelImmediately()
    {
        Debug.Log("🚨 FORZANDO OCULTACIÓN INMEDIATA DEL PANEL...");

        if (modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(false);
            Debug.Log($"📋 Panel principal FORZADO A FALSE: {modeSelectionPanel.name}");
        }

        MonoBehaviour[] allUIComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour component in allUIComponents)
        {
            if (component != null && component.gameObject != null)
            {
                GameObject obj = component.gameObject;

                if (obj.GetComponent<Canvas>() != null ||
                    obj.GetComponent<Button>() != null ||
                    obj.GetComponent<Image>() != null)
                {
                    string objName = obj.name.ToLower();
                    if (objName.Contains("selection") ||
                        objName.Contains("mode") ||
                        objName.Contains("menu") ||
                        objName.Contains("panel"))
                    {
                        obj.SetActive(false);
                        Debug.Log($"🎯 UI Element ocultado: {obj.name}");
                    }
                }
            }
        }

        Debug.Log("✅ Ocultación forzada completada");
    }

    IEnumerator StartGameplayCoroutine()
    {
        Debug.Log("🔄 Iniciando configuración del juego...");

        if (vsBotMode)
        {
            yield return StartCoroutine(SetupVsBotMode());
        }
        else
        {
            yield return StartCoroutine(SetupTwoPlayerMode());
        }

        yield return new WaitForSeconds(0.3f);

        // ✅ CORREGIDO: Configurar modo de pantalla según el modo de juego
        ConfigureScreenMode();


        // ✅ CORREGIDO: Desactivar movimiento del Bot ANTES del countdown
        if (vsBotMode && currentBot != null)
        {
            BotController botController = currentBot.GetComponent<BotController>();
            if (botController != null)
            {
                botController.SetControlsEnabled(false);
                Debug.Log("🤖 Movimiento del Bot DESACTIVADO durante countdown");
            }
        }

        ActivateCountdownManager();
        VerifyPanelIsHidden();

        Debug.Log($"🎯 ¡Juego configurado! Modo: {(vsBotMode ? "VS Bot" : "2 Jugadores")}");
    }

    void VerifyPanelIsHidden()
    {
        Debug.Log("🔍 VERIFICANDO QUE EL PANEL ESTÉ OCULTO...");

        if (modeSelectionPanel != null && modeSelectionPanel.activeInHierarchy)
        {
            Debug.LogError("❌ EL PANEL PRINCIPAL SIGUE ACTIVO - FORZANDO DESACTIVACIÓN");
            modeSelectionPanel.SetActive(false);
        }

        Debug.Log("✅ Verificación de paneles completada");
    }

    void ActivateCountdownManager()
    {
        if (countdownManager != null)
        {
            countdownManager.enabled = true;

            // ✅ CORREGIDO: Pasar referencia del GameModeSelector al CountdownManager
            countdownManager.SetGameModeSelector(this);
            countdownManager.OnGameModeSelected();
        }
        else
        {
            StartCoroutine(EnableControlsAfterDelay(1f));
        }
    }

    IEnumerator EnableControlsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableControls();
    }

    IEnumerator SetupVsBotMode()
    {
        Debug.Log("🤖 Configurando modo VS Bot...");

        currentPlayer1 = FindOrCreatePlayer("Player1", playerSpawnPoint, 1);

        if (currentPlayer1 != null)
        {
            ConfigurePlayer(currentPlayer1, 1);
            UpdateBotSpawnPointBasedOnPlayer1();
        }

        yield return new WaitForSeconds(0.1f);

        currentBot = FindOrCreateBot("Bot", botSpawnPoint);

        if (currentBot != null)
        {
            ConfigureBot(currentBot);

            Debug.Log($"📍 Player1 posición: {currentPlayer1.transform.position}");
            Debug.Log($"📍 Bot posición: {currentBot.transform.position}");
            Debug.Log($"📍 Distancia entre ellos: {Vector3.Distance(currentPlayer1.transform.position, currentBot.transform.position)}");

            Debug.Log($"🤖 Bot configurado exitosamente para modo VS Bot");
        }
        else
        {
            Debug.LogError("❌ No se pudo crear el Bot");
        }

        Debug.Log("✅ Modo VS Bot configurado");
    }

    void UpdateBotSpawnPointBasedOnPlayer1()
    {
        if (currentPlayer1 != null && botSpawnPoint != null)
        {
            Vector3 newBotPosition = currentPlayer1.transform.position + new Vector3(-2f, 0f, 0f);
            botSpawnPoint.position = newBotPosition;
            Debug.Log($"📍 Spawn point del bot actualizado a: {newBotPosition}");
        }
    }

    IEnumerator SetupTwoPlayerMode()
    {
        Debug.Log("👥 Configurando modo 2 jugadores...");

        FindExistingPlayer1();

        if (currentPlayer1 == null)
        {
            Debug.LogError("❌ No se encontró Player1 en la escena");
            yield break;
        }

        yield return new WaitForSeconds(0.1f);

        currentPlayer2 = FindOrCreatePlayer("Player2", player2SpawnPoint, 2);

        if (currentPlayer2 != null)
        {
            ConfigurePlayer(currentPlayer2, 2);
            Debug.Log($"🎮 Player2 configurado exitosamente");
        }
        else
        {
            Debug.LogError("❌ No se pudo crear Player2");
        }

        Debug.Log("✅ Modo 2 jugadores configurado");
    }

    GameObject FindOrCreatePlayer(string playerName, Transform spawnPoint, int playerNumber)
    {
        GameObject player = GameObject.Find(playerName);

        if (player != null)
        {
            Debug.Log($"✅ {playerName} ya existe en la escena");
            return player;
        }

        if (playerPrefab == null)
        {
            playerPrefab = Resources.Load<GameObject>("PlayerDog");
            if (playerPrefab == null)
            {
                GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
                if (existingPlayer != null)
                {
                    playerPrefab = existingPlayer;
                    Debug.Log($"✅ Usando jugador existente como prefab: {existingPlayer.name}");
                }
                else
                {
                    Debug.LogError($"❌ No se pudo encontrar playerPrefab para {playerName}");
                    return null;
                }
            }
        }

        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject($"{playerName}SpawnPoint");
            if (playerNumber == 1)
            {
                spawnObj.transform.position = new Vector3(7.10f, -53.98f, 0f);
            }
            else
            {
                GameObject player1 = GameObject.Find("Player1");
                if (player1 != null)
                {
                    spawnObj.transform.position = player1.transform.position + new Vector3(2f, 0f, 0f);
                }
                else
                {
                    spawnObj.transform.position = new Vector3(10.10f, -53.98f, 0f);
                }
            }
            spawnPoint = spawnObj.transform;
            Debug.Log($"📍 Spawn point de emergencia creado para {playerName} en {spawnPoint.position}");
        }

        try
        {
            player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.name = playerName;
            player.tag = "Player";

            Debug.Log($"✅ {playerName} creado exitosamente en {spawnPoint.position}");
            return player;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Error al crear {playerName}: {e.Message}");
            return null;
        }
    }

    GameObject FindOrCreateBot(string botName, Transform spawnPoint)
    {
        GameObject bot = GameObject.Find(botName);

        if (bot != null)
        {
            Debug.Log($"✅ {botName} ya existe en la escena");
            return bot;
        }

        if (botPrefab == null)
        {
            botPrefab = Resources.Load<GameObject>("Bot");
            if (botPrefab == null)
            {
                GameObject existingBot = GameObject.FindGameObjectWithTag("Bot");
                if (existingBot != null)
                {
                    botPrefab = existingBot;
                    Debug.Log($"✅ Usando bot existente como prefab: {existingBot.name}");
                }
                else
                {
                    if (playerPrefab != null)
                    {
                        botPrefab = playerPrefab;
                        Debug.Log($"✅ Usando playerPrefab como base para el bot");
                    }
                    else
                    {
                        Debug.LogError($"❌ No se pudo encontrar botPrefab para {botName}");
                        return null;
                    }
                }
            }
        }

        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject($"{botName}SpawnPoint");
            GameObject player1 = GameObject.Find("Player1");
            if (player1 != null)
            {
                spawnObj.transform.position = player1.transform.position + new Vector3(-2f, 0f, 0f);
            }
            else
            {
                spawnObj.transform.position = new Vector3(5.10f, -53.98f, 0f);
            }
            spawnPoint = spawnObj.transform;
            Debug.Log($"📍 Spawn point de emergencia creado para {botName} en {spawnPoint.position}");
        }

        try
        {
            bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
            bot.name = botName;
            bot.tag = "Bot";

            Debug.Log($"✅ {botName} creado exitosamente en {spawnPoint.position}");
            return bot;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 Error al crear {botName}: {e.Message}");
            return null;
        }
    }

    void FindExistingPlayer1()
    {
        if (currentPlayer1 == null)
        {
            currentPlayer1 = GameObject.Find("Player1");
            if (currentPlayer1 == null)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length > 0)
                {
                    currentPlayer1 = players[0];
                    currentPlayer1.name = "Player1";
                    Debug.Log($"✅ Player1 encontrado por tag: {currentPlayer1.name}");
                }
            }

            if (currentPlayer1 != null)
            {
                Debug.Log($"✅ Player1 configurado: {currentPlayer1.name}");
            }
        }
    }

    void ConfigurePlayer(GameObject player, int playerNumber)
    {
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = player.AddComponent<PlayerController>();
        }

        playerController.playerNumber = playerNumber;
        playerController.useKeyboardInput = true;
        playerController.controlEnabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 4f;
        }

        Debug.Log($"✅ {player.name} configurado - Player Number: {playerNumber}");
    }

    void ConfigureBot(GameObject bot)
    {
        if (bot == null) return;

        BotController botController = bot.GetComponent<BotController>();
        if (botController != null)
        {
            botController.isPlayerControlled = false;
            botController.controlsEnabled = false; // ✅ INICIALMENTE DESACTIVADO
            botController.autoNavigateToGoal = true;
            botController.SetAutoNavigateToGoal(true);
            botController.ignoreCheckpointsInAIMode = true;
            botController.SetIgnoreCheckpoints(true);

            Rigidbody2D rb = bot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 4f;
            }

            Debug.Log($"🤖 {bot.name} configurado - Modo IA, Controles INICIALMENTE DESACTIVADOS");
        }
        else
        {
            Debug.LogError("❌ No se pudo encontrar BotController en el bot");
        }
    }

    // ✅ CORREGIDO: Método mejorado para configurar modo de pantalla
    void ConfigureScreenMode()
    {
        if (splitScreenManager != null)
        {
            if (vsBotMode)
            {
                // ✅ MODO VS BOT: Pantalla completa (solo Player1)
                splitScreenManager.SetSinglePlayer();
                Debug.Log("🖥️ Modo VS Bot - Pantalla completa activada (solo Player1)");
            }
            else
            {
                // ✅ MODO 2 JUGADORES: Pantalla dividida
                splitScreenManager.SetTwoPlayers();
                Debug.Log("🖥️ Modo 2 Jugadores - Pantalla dividida activada");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ SplitScreenManager no encontrado");
        }
    }

    void DisableAllControls()
    {
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayers)
        {
            player.SetControlsEnabled(false);
        }

        BotController[] allBots = FindObjectsOfType<BotController>();
        foreach (BotController bot in allBots)
        {
            bot.SetControlsEnabled(false);
        }
    }

    public void EnableControls()
    {
        Debug.Log("🔓 Activando controles...");

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayers)
        {
            player.SetControlsEnabled(true);
        }

        // ✅ Solo activar bots controlados por jugador (no IA)
        BotController[] allBots = FindObjectsOfType<BotController>();
        foreach (BotController bot in allBots)
        {
            if (bot.isPlayerControlled)
            {
                bot.SetControlsEnabled(true);
                Debug.Log($"🤖 Controles activados para bot jugador: {bot.gameObject.name}");
            }
        }
    }

    // ✅ NUEVO MÉTODO: Para que CountdownManager pueda activar controles específicos del Bot
    public void EnableBotControls()
    {
        if (currentBot != null)
        {
            BotController botController = currentBot.GetComponent<BotController>();
            if (botController != null)
            {
                botController.SetControlsEnabled(true);
                Debug.Log($"🤖 Controles del Bot activados después del countdown");
            }
        }
    }

    public bool IsTwoPlayerMode()
    {
        return twoPlayerMode;
    }

    public bool IsVsBotMode()
    {
        return vsBotMode;
    }

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    public GameObject GetPlayer1()
    {
        return currentPlayer1;
    }

    public GameObject GetPlayer2()
    {
        return currentPlayer2;
    }

    public GameObject GetBot()
    {
        return currentBot;
    }
}