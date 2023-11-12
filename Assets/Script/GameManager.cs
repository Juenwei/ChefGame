using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    //Singetlon
    public static GameManager Instance { get; private set; }

    private enum GameState
    {
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver,
    }

    //Events
    public event EventHandler OnGameStateChanged;
    public event EventHandler OnLocalGamePaused, OnLocalGameResumed;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerPaused, OnMultiplayerResumed;

    //State and Condition
    private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private bool isLocalPlayerReady = false, isLocalGamePaused = false;
    private Dictionary<ulong, bool> playerReadyDictionary;  //Reason use dictionary is it use the specfied key(List use sequantial index) to identify data. 
    private Dictionary<ulong, bool> playerPausedDictionary; //Record who did pause the game
    private bool isAutoTestPlayerPause;

    //Timers
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 300f;

    //GameObject and Prefabs Reference
    [SerializeField] private Transform playerPrefab;

	private void Awake()
	{
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
		playerPausedDictionary = new Dictionary<ulong, bool>();
	}

	void Start()
    {
		GameInput.instance.OnPauseTrigger += GameInput_OnPauseTrigger;
        GameInput.instance.OnInteractAction += GameInput_OnInteractAction;
	}

	private void LateUpdate()
	{
		if(isAutoTestPlayerPause)
        {
            isAutoTestPlayerPause = false;
            TestGamePauseState();
        }
	}

	public override void OnNetworkSpawn()
	{
		gamePlayingTimer.Value = gamePlayingTimerMax;
		gameState.OnValueChanged += GameState_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
			NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

            //Event triggered after all clinet loaded into network
			NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
	}

	private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
	{
		foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
	}

	private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
	{
        //Desc : It Should Call the TestGamPauseState to check, but the connecting list still not updated yet
        //(Disconnected Client still in pause list), so it should run in next frame
        isAutoTestPlayerPause = true;
	}

	private void GameInput_OnInteractAction(object sender, EventArgs e)
	{
        if(gameState.Value == GameState.WaitingToStart)
        {
            //Update Status
            isLocalPlayerReady = true;

            //Send the ClientNetwork ID to the server for update player is ready
            SetPlayerReadyServerRpc();

			//Close Tutorial UI
			OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
		}
	}

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
		OnGameStateChanged?.Invoke(this, EventArgs.Empty);
	}

    private void IsGamePaused_OnValueChanged(bool previousValue,bool newValue)
    {
        if(isGamePaused.Value)
        {
			Time.timeScale = 0;
            OnMultiplayerPaused?.Invoke(this, EventArgs.Empty);
		}
        else
        {
			Time.timeScale = 1;
			OnMultiplayerResumed?.Invoke(this, EventArgs.Empty);
		}
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //Get the CLient NEtwork ID from client as the receiver ID params
        var clientOwnerId = serverRpcParams.Receive.SenderClientId;

        //Save to the Dictionary, This data value with "clientOwnerID" key is true
        playerReadyDictionary[clientOwnerId] = true;

        //Iterate the Diction to check all players is ready
        var isAllPlayerReady = true;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //If currennt Dictionary does not contain current client ID or this client ID is not ready yet
                isAllPlayerReady = false;
                break;
            }
        }

        if (isAllPlayerReady)
        {
            gameState.Value = GameState.CountDownToStart;
        }

    }

    private void GameInput_OnPauseTrigger(object sender, EventArgs e)
	{
        //IF Pause Key Triggered.
        TogglePauseGame();
	}

	void Update()
    {
        if(!IsServer) { return; }

        switch (gameState.Value)
        {
            case GameState.WaitingToStart:
                break;

            case GameState.CountDownToStart:
				countDownToStartTimer.Value -= Time.deltaTime;
				if (countDownToStartTimer.Value <= 0f)
				{
					gameState.Value = GameState.GamePlaying;
				}
				break;

            case GameState.GamePlaying:
				gamePlayingTimer.Value -= Time.deltaTime;
				if (gamePlayingTimer.Value <= 0)
				{
					gameState.Value = GameState.GameOver;
				}
				break;

            case GameState.GameOver:
                break;
        }
		//Debug.Log(gameState);
	}

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public bool IsGameCountdownStarted()
    {
        return gameState.Value == GameState.CountDownToStart;
    }
    
    public bool IsGamePlaying()
    {
        return gameState.Value == GameState.GamePlaying;
    }

    public bool IsGameOver()
    {
        return gameState.Value == GameState.GameOver;
    }    

    public bool IsWaitingToStart()
    {
        return gameState.Value == GameState.WaitingToStart;
    }


	public float GetCountdownTimer()
    {
        return countDownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - gamePlayingTimer.Value / gamePlayingTimerMax;
    }

    public void TogglePauseGame()
    {
        //Desc : Check current condition to determine need pause or resume game.
		isLocalGamePaused = !isLocalGamePaused;
        if(isLocalGamePaused)
        {
            //Activate RPC to send message to client
            PauseGameServerRpc();

            //Activate Pause Menu UI
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);

        }else
        {
			//Activate RPC to send message to client
			ResumeGameServerRpc();

			//Activate Pause Menu UI
			OnLocalGameResumed?.Invoke(this, EventArgs.Empty);

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //Add current Clinet Id and pause condition to the server's "playerPauseDictionary"
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        //Do the checking condition for Pausing game based on the clinet Id
        TestGamePauseState();
	}

	[ServerRpc(RequireOwnership = false)]
	private void ResumeGameServerRpc(ServerRpcParams serverRpcParams = default)
	{
		playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();
	}

    private void TestGamePauseState()
    {
        //Check does any of the player in connectedCLientID is pasuing the game.
        foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                //Player are Pausing the game
                isGamePaused.Value = true;
                return;
            }
        }

        //No player pausing the game
        isGamePaused.Value = false;
    }

}
