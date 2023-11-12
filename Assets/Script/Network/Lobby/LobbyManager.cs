using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
	//Singetlon
	public static LobbyManager Instance;

	private const float MAX_LOBBY_HEARTBEAT_TIMER = 15f, MAX_REFRESH_LOBBIES_TIMER = 3f;
	private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

	//Event
	public event EventHandler OnLobbyCreateStarted, OnLobbyCreateFailed;
	public event EventHandler OnJoinStarted, OnJoinFailed, OnQuickJoinFailed;
	public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;

	public class OnLobbyListChangedEventArgs : EventArgs
	{
		public List<Lobby> lobbies;
	}

	//TODO: Destroy once got Replacement
	private string playerName = "JuenWei";

	private Lobby joinedLobby;
	private float lobbyHeartBeatTimer = 0f, refreshLobbiesTimer = 0f;

	private void Awake()
	{
		Instance = this;

		DontDestroyOnLoad(gameObject);

		playerName += UnityEngine.Random.Range(1, 99);

		InitializeAuthentication();

	}

	private void Update()
	{
		SendHeartBeatToLobbyAsync();
		PeriodicRefereshLobbies();
	}

	private async void InitializeAuthentication()
	{
		//Prevent multiple initializing
		if(UnityServices.State == ServicesInitializationState.Initialized || UnityServices.State == ServicesInitializationState.Initializing)
		{
			return;
		}

		//TODO : Following code is just to avoid same account id, remove it once have replacement.
		//Set profile
		var initializationOptions = new InitializationOptions();
		initializationOptions.SetProfile(playerName);

		//Initizalize Sevice
		await UnityServices.InitializeAsync(initializationOptions);

		//Subsribe for print player Id.
		AuthenticationService.Instance.SignedIn += () => {
			// do nothing
			Debug.Log("Signed in! Id : " + AuthenticationService.Instance.PlayerId + " for player : " + playerName) ;
		};

		//Sign in
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}

	public async void CreateLobbyAsync(string lobbyName, bool isPrivate)
	{
		OnLobbyCreateStarted?.Invoke(this, EventArgs.Empty);
		try
		{
			joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenMultiplayerManager.MAX_PLAYER_COUNT, new CreateLobbyOptions()
			{
				IsPrivate = isPrivate
			});

			//Create Relay allocation (somting like create a room in RelayServer)
			var allocation = await AllocateRelay();

			//Generate Join Code based on the created allocation.
			var relayJoinCode = await GetRelayJoinCode(allocation);

			//Save the join code as the lobby data for haring code among all member who join lobby.
			await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions()
			{
				Data = new Dictionary<string, DataObject>()
				{
					{ KEY_RELAY_JOIN_CODE ,new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
				}
			});
			
			//Set the Unity Transport for joining to relay server.
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

			KitchenMultiplayerManager.Instance.StartHost();
			SceneLoader.LoadNetwork(SceneLoader.Scene.CharacterSelectScene);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
			OnLobbyCreateFailed?.Invoke(this,EventArgs.Empty);
		}

	}

	public async void QuickJoinLobbyAsync()
	{
		OnJoinStarted?.Invoke(this, EventArgs.Empty);
		try
		{
			joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
			var allocation = await JoinRelay(joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

			KitchenMultiplayerManager.Instance.StartClient();
		}
		catch(LobbyServiceException e) 
		{
			OnQuickJoinFailed?.Invoke(this,EventArgs.Empty);
			Debug.LogError(e.Message);
		}
	}

	public async void JoinLobbyByLobbyCodeAsync(string lobbyCode)
	{
		OnJoinStarted?.Invoke(this, EventArgs.Empty);
		try
		{
			joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
			var allocation = await JoinRelay(joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

			KitchenMultiplayerManager.Instance.StartClient();
		}
		catch (LobbyServiceException e)
		{
			OnJoinFailed?.Invoke(this, EventArgs.Empty);
			Debug.LogError(e.Message);
		}
	}

	public async void JoinLobbyByLobbyIdAsync(string lobbyId)
	{
		OnJoinStarted?.Invoke(this, EventArgs.Empty);
		try
		{
			joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
			var allocation = await JoinRelay(joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

			KitchenMultiplayerManager.Instance.StartClient();
		}
		catch (LobbyServiceException e)
		{
			OnJoinFailed?.Invoke(this, EventArgs.Empty);
			Debug.LogError(e.Message);
		}
	}

	public async void DeleteLobbyAsync()
	{
		if(joinedLobby ==  null) { return; }

		try
		{
			await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
			joinedLobby = null;
		}
		catch(LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	public async void LeaveLobbyAsync()
	{
		if (joinedLobby == null) { return; };

		try
		{
			await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e.Message);
		}
	}

	public async void KickPlayerAsync(string playerId)
	{
		if (!IsHostLobby()) { return; };

		try
		{
			await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e.Message);
		}
	}

	private void PeriodicRefereshLobbies()
	{
		if(joinedLobby != null || !AuthenticationService.Instance.IsSignedIn || 
			SceneManager.GetActiveScene().name != SceneLoader.Scene.LobbyScene.ToString()) { return; }

		refreshLobbiesTimer -= Time.deltaTime;
		if(refreshLobbiesTimer <= 0)
		{
			refreshLobbiesTimer = MAX_REFRESH_LOBBIES_TIMER;

			ListLobbiesAsync();
		}
	}

	private async void ListLobbiesAsync()
	{
		try
		{
			var queryLobbiesOption = new QueryLobbiesOptions
			{
				Filters = new List<QueryFilter>() {
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				},
			};
			var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOption);
			OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs()
			{
				lobbies = queryResponse.Results
			});

		}catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	private async void SendHeartBeatToLobbyAsync()
	{
		if(IsHostLobby())
		{
			lobbyHeartBeatTimer -= Time.deltaTime;
			if(lobbyHeartBeatTimer <= 0f)
			{
				lobbyHeartBeatTimer = MAX_LOBBY_HEARTBEAT_TIMER;

				await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
			}
		}
	}

	#region RELAY
	private async Task<Allocation> AllocateRelay()
	{
		try
		{
			var allocation = await RelayService.Instance.CreateAllocationAsync(KitchenMultiplayerManager.MAX_PLAYER_COUNT - 1);
			return allocation;
		}
		catch (RelayServiceException e)
		{
			Debug.LogError(e.Message);
			return default;
		}
		
	}

	private async Task<string> GetRelayJoinCode(Allocation allocation)
	{
		try
		{
			var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			return joinCode;
		}
		catch (RelayServiceException e)
		{
			Debug.LogError(e.Message);
			return default;
		}
	}

	private async Task<JoinAllocation> JoinRelay(string joinCode)
	{
		try
		{
			var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
			return joinAllocation;
		}
		catch (RelayServiceException e)
		{
			Debug.LogError(e.Message);
			return default;
		}
	}
	#endregion


	//GETTER SETTER
	public Lobby GetLobby()
	{
		return joinedLobby;
	}

	public bool IsHostLobby()
	{
		//If current lobby's host Id is equal current player id then current lobby is host lobby. 
		if(joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
		{
			return true;
		}
		else { return false; }
	}

	public string GetPlayerName()
	{
		return playerName;
	}

	public string GetPlayerLobbyId()
	{
		return AuthenticationService.Instance.PlayerId;
	}
}
