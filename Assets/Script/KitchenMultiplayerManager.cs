using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenMultiplayerManager : NetworkBehaviour
{
	public static KitchenMultiplayerManager Instance { get; private set; }

	public const int MAX_PLAYER_COUNT = 4;
	private const string PLAYERPREF_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

	public event EventHandler OnFailToConnectGame, OnTryToConnectGame;
	public event EventHandler OnPlayerNetworkListChanged;

	[SerializeField] private KitchenObjectListSO KitchenObjectListSO;

	[SerializeField] private NetworkList<PlayerData> playerDataNetworkList;

	[SerializeField] private List<Color> playerColors;

	private string playerName;
	public static bool IsMultiplayerMode = true;

	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
		playerDataNetworkList  = new NetworkList<PlayerData>();

		playerName = PlayerPrefs.GetString(PLAYERPREF_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(0,100));

		playerDataNetworkList.OnListChanged += PlayerNetworkDatas_OnListChanged;
	}

	private void Start()
	{
		if(!IsMultiplayerMode)
		{
			StartHost();
			SceneLoader.LoadNetwork(SceneLoader.Scene.GameScene);
		}
	}

	#region Host/Join Game 
	private void PlayerNetworkDatas_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
	{
		OnPlayerNetworkListChanged?.Invoke(this,EventArgs.Empty);
	}

	private void NetworkManager_Server_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
	{
		//Desc : Player only can get approve join in characterSelectScene, else will get kick out

		//CAUTION : String Comparing is dangerous, please handle it properly
		if(SceneManager.GetActiveScene().name != SceneLoader.Scene.CharacterSelectScene.ToString())
		{
			connectionApprovalResponse.Approved = false;
			connectionApprovalResponse.Reason = "Game has already started";
			return;
		}
		else if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_COUNT)
		{
			connectionApprovalResponse.Approved = false;
			connectionApprovalResponse.Reason = "The Game Exceed Maximum Player Number";
			return;
		}

		connectionApprovalResponse.Approved = true;
	}

	public void StartHost()
	{
		//Subsribe to the approving callback to ensure run the method for player late joins
		NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_Server_ConnectionApprovalCallback;

		//Subsribe to the connected callback to ensure the new client will be record to the playerData list
		NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;

		//Subsribe to the disconnected callback for remove player in characterSelctioScne 
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;

		NetworkManager.Singleton.StartHost();
	}

	private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
	{
		Debug.Log("Player Data Network List Count : " + playerDataNetworkList.Count);

		for(int i=0;i < playerDataNetworkList.Count;i++)
		{
			var playerData = playerDataNetworkList[i];
			if (playerData.clientId == clientId)
			{
				Debug.Log("Remove Clinet ID : " + clientId);
				playerDataNetworkList.RemoveAt(i);
			}
		}
	}

	private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
	{
		//Update the playerData while any new player joins the game
		playerDataNetworkList.Add(new PlayerData
		{
			clientId = clientId,
			//Iterate for getting default color.
			colorId = GetFirstUnusedColorId(),
		});

		SetPlayerNameServerRpc(GetPlayerName());

		if (!IsMultiplayerMode) return;
		//Get the Player Id for kick out lobby function
		SetPlayerLobbyIdServerRpc(LobbyManager.Instance.GetPlayerLobbyId());
	}

	public void StartClient()
	{
		//Send Event for Current Activate Connecting Messgae
		OnTryToConnectGame.Invoke(this, EventArgs.Empty);

		//Subsribe to the disconnect call back (Client fail join) for trigger the fail connect event
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
		NetworkManager.Singleton.OnClientConnectedCallback+= NetworkManager_Client_OnClientConnectedCallback;
		NetworkManager.Singleton.StartClient();
	}

	private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
	{
		//While client connected send the player name to the server.
		SetPlayerNameServerRpc(GetPlayerName());

		//Get the Player Id for kick out lobby function
		SetPlayerLobbyIdServerRpc(LobbyManager.Instance.GetPlayerLobbyId());
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
	{
		//Get Player Data based on the client Id
		var playerIndex = GetPlayerDataIndexByClientId(serverRpcParams.Receive.SenderClientId);
		var playerData = GetPlayerDataByPlayerIndex(playerIndex);

		playerData.playerName = playerName;

		playerDataNetworkList[playerIndex] = playerData;
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerLobbyIdServerRpc(string playerLobbyId, ServerRpcParams serverRpcParams = default)
	{
		//Get Player Data based on the client Id
		var playerIndex = GetPlayerDataIndexByClientId(serverRpcParams.Receive.SenderClientId);
		var playerData = GetPlayerDataByPlayerIndex(playerIndex);

		playerData.playerLobbyId = playerLobbyId;

		playerDataNetworkList[playerIndex] = playerData;
	}

	private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
	{
		//Activate UI
		OnFailToConnectGame?.Invoke(this, EventArgs.Empty);
	}

	//BOOLEAN
	public bool IsPlayerIndexConnected(int playerIndex)
	{
		//Desc : Since the player index start from 0,1,2,3,... ,So use the count can know how many player join the server.
		//Example : if the count of list is 2 which have 2 player ,so the player index 0 and 1 will be less/< than the count of list which is 2

		return playerIndex < playerDataNetworkList.Count;
	}

	#endregion

	#region Spawn Network Logic
	//Seperate static and RPC to Solve RPC Attribute cannt apply on the static function. 
	public void SpwanKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
	{
		SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
	}

	//DESC : Network object reference class is the topmost parent class of all network related class ,just like "object".
	[ServerRpc(RequireOwnership = false)]
	private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
	{
		//Use MAPPER due to RPC function only accet value type as param
		var kitchenObjectSO = GetKitchenObjectSOByIndex(kitchenObjectSOIndex);

		kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParenyNetworkObject);
		var kitchenObjectParent = kitchenObjectParenyNetworkObject.GetComponent<IKitchenObjectParent>();

		//Check does the target parent has object ,if yes just return (Avoid Delay cause Error)
		if (kitchenObjectParent.HasKitchenObject())
			return;

		//Spawn Object
		var kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab).transform;
		var kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
		kitchenObjectNetworkObject.Spawn(true);

		//Set the Reference Relationship Between KitchenObj and Counter
		var kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

		kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
	}

	#endregion

	#region Destroy Network Logic
	public void DestroyKitchenObject(KitchenObject kitchenObject)
	{
		DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
	}

	[ServerRpc(RequireOwnership = false)]
	public void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
	{
		kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);

		//IF there is no object to destroy ,return the function (Avoid Delay cause Error)
		if(kitchenObjectNetworkObject == null) { return;  }
		var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

		ClearKitchenObjectParentClientRpc(kitchenObject.NetworkObject);

		kitchenObject.DestroySelf();
	}

	[ClientRpc]
	public void ClearKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
	{
		kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
		var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

		kitchenObject.ClearKitchenObjectParent();
	}
	#endregion

	#region Select COLOR Logic
	public void ChangePlayerColor(int colorId)
	{
		//Notify Server that you want change to spcific color
		ChangePlayerColorServerRpc(colorId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
	{
		if(!IsColorAvailable(colorId)) { return; }

		//If Color IS Available

		//Convert ClientId to PlayerDataIndex
		var playerDataIndex = GetPlayerDataIndexByClientId(serverRpcParams.Receive.SenderClientId);
		var playerData = playerDataNetworkList[playerDataIndex];

		//Change color
		playerData.colorId = colorId;

		//Replace the player Data back to the list
		playerDataNetworkList[playerDataIndex] = playerData;
	}

	private bool IsColorAvailable(int colorId)
	{
		foreach(var playerData in playerDataNetworkList)
		{
			if(playerData.colorId == colorId)
				return false;
		}
		return true;
	}

	private int GetFirstUnusedColorId()
	{
		for (int i =0; i < playerColors.Count; i++)
		{
			if (IsColorAvailable(i))
			{
				return i;
			}
		}
		return -1;
	}

	#endregion

	public void KickPlayer(ulong clientId)
	{
		NetworkManager.Singleton.DisconnectClient(clientId);
		NetworkManager_Server_OnClientDisconnectCallback(clientId);
	}

	//MAPPER
	public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
	{
		return KitchenObjectListSO.kitchenObjectSOs.IndexOf(kitchenObjectSO);
	}

	public KitchenObjectSO GetKitchenObjectSOByIndex(int kitchenObjectSOIndex)
	{
		return KitchenObjectListSO.kitchenObjectSOs[kitchenObjectSOIndex];
	}

	public PlayerData GetPlayerDataByPlayerIndex(int playerIndex)
	{
		return playerDataNetworkList[playerIndex];
	} 

	public Color GetPlayerColor(int colorId)
	{
		return playerColors[colorId];
	}

	public PlayerData GetPlayerDataByClientId(ulong clientId)
	{

		foreach (PlayerData playerData in playerDataNetworkList)
		{
			if(playerData.clientId == clientId)
				return playerData;
		}
		return default;
	}

	public int GetPlayerDataIndexByClientId(ulong clientId)
	{
		for(int i = 0;i  < playerDataNetworkList.Count;i++)
		{
			if (playerDataNetworkList[i].clientId == clientId)
				return i;
		}
		return default;
	}

	public PlayerData GetPlayerData()
	{
		//Send current Device/Client ID to search ,Since Every player Data initial value have a color bind with clinet Id 
		//Get Local ID -> Search Seleted Color based on ID -> Return Data
		return GetPlayerDataByClientId(NetworkManager.Singleton.LocalClientId);
 	}

	public string GetPlayerName()
	{
		return playerName;
	}

	public void SetPlayerName(string playerName)
	{
		this.playerName = playerName;
		PlayerPrefs.SetString(PLAYERPREF_PLAYER_NAME_MULTIPLAYER, playerName);
	}
}
