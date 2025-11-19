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

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Game References")]
    public CountdownManager countdownManager;
    public SplitScreenManager splitScreenManager;

    private bool gameStarted = false;
    private bool twoPlayerMode = false;
    private GameObject currentPlayer1;
    private GameObject currentPlayer2;

    void Start()
    {
        Debug.Log("✅ GameModeSelector iniciado");

        gameStarted = false;
        twoPlayerMode = false;

        FindSpawnPointsIfNull();

        if (splitScreenManager == null)
            splitScreenManager = FindObjectOfType<SplitScreenManager>();

        if (countdownManager == null)
            countdownManager = FindObjectOfType<CountdownManager>();

        SetupPanels();

        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.RemoveAllListeners();
            singlePlayerButton.onClick.AddListener(() => StartCoroutine(SelectGameMode(false)));
        }

        if (twoPlayersButton != null)
        {
            twoPlayersButton.onClick.RemoveAllListeners();
            twoPlayersButton.onClick.AddListener(() => StartCoroutine(SelectGameMode(true)));
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
    }

    IEnumerator SelectGameMode(bool isTwoPlayer)
    {
        if (gameStarted) yield break;

        Debug.Log($"🎮 Usuario seleccionó: {(isTwoPlayer ? "2 JUGADORES" : "1 JUGADOR")}");

        twoPlayerMode = isTwoPlayer;
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

        if (twoPlayerMode)
        {
            yield return StartCoroutine(SetupTwoPlayerMode());
        }
        else
        {
            yield return StartCoroutine(SetupSinglePlayerMode());
        }

        yield return new WaitForSeconds(0.3f);

        ConfigureScreenMode(twoPlayerMode);
        ActivateCountdownManager();
        VerifyPanelIsHidden();

        Debug.Log($"🎯 ¡Juego configurado! Modo: {(twoPlayerMode ? "2 Jugadores" : "1 Jugador")}");
    }

    void VerifyPanelIsHidden()
    {
        Debug.Log("🔍 VERIFICANDO QUE EL PANEL ESTÉ OCULTO...");

        if (modeSelectionPanel != null && modeSelectionPanel.activeInHierarchy)
        {
            Debug.LogError("❌ EL PANEL PRINCIPAL SIGUE ACTIVO - FORZANDO DESACTIVACIÓN");
            modeSelectionPanel.SetActive(false);
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                string canvasName = canvas.gameObject.name.ToLower();
                if ((canvasName.Contains("selection") || canvasName.Contains("mode")) &&
                    !canvasName.Contains("challenge") && !canvasName.Contains("countdown"))
                {
                    Debug.LogWarning($"⚠️ Canvas potencialmente problemático activo: {canvas.gameObject.name}");
                }
            }
        }

        Debug.Log("✅ Verificación de paneles completada");
    }

    void ActivateCountdownManager()
    {
        if (countdownManager != null)
        {
            countdownManager.enabled = true;
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

    IEnumerator SetupSinglePlayerMode()
    {
        Debug.Log("👤 Configurando modo 1 jugador...");

        currentPlayer1 = FindOrCreatePlayer("Player1", playerSpawnPoint, 1);

        if (currentPlayer1 != null)
        {
            ConfigurePlayer(currentPlayer1, 1);
        }

        Debug.Log("✅ Modo 1 jugador configurado");
        yield return null;
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

            if (splitScreenManager != null)
            {
                splitScreenManager.SetTwoPlayers();
                Debug.Log("🖥️ SplitScreenManager configurado para 2 jugadores");
            }

            Debug.Log($"🎮 Player2 configurado exitosamente");
        }
        else
        {
            Debug.LogError("❌ No se pudo crear Player2 - Cambiando a modo 1 jugador");
            twoPlayerMode = false;
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
                    spawnObj.transform.position = player1.transform.position + new Vector3(3f, 0f, 0f);
                    Debug.Log($"📍 Player2 creado a la derecha de Player1: {spawnObj.transform.position}");
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

    void ConfigureScreenMode(bool twoPlayers)
    {
        if (splitScreenManager != null)
        {
            if (twoPlayers)
            {
                splitScreenManager.SetTwoPlayers();
            }
            else
            {
                splitScreenManager.SetSinglePlayer();
            }
        }
    }

    void DisableAllControls()
    {
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayers)
        {
            player.SetControlsEnabled(false);
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
    }

    public bool IsTwoPlayerMode()
    {
        return twoPlayerMode;
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
}