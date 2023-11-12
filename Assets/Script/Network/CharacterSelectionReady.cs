using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectionReady : NetworkBehaviour
{
	//Singetlon
	public static CharacterSelectionReady Instance { get; private set; }

	public event EventHandler OnReadyChanged;

	private Dictionary<ulong, bool> playerReadyDictionary;  //Reason use dictionary is it use the specfied key(List use sequantial index) to identify data. 


	private void Awake()
	{
		Instance = this;
		playerReadyDictionary = new Dictionary<ulong, bool>();
	}

	//Desc : Function that allow public to call for player ready 
	public void SetPlayerReady()
	{
		//Calling Server to add curret client id to the ready dictionary.
		SetPlayerReadyServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
	{
		//Get the CLient NEtwork ID from client as the receiver ID params
		var clientOwnerId = serverRpcParams.Receive.SenderClientId;

		//Save to the Dictionary, This data value with "clientOwnerID" key is true
		playerReadyDictionary[clientOwnerId] = true;

		//Sync the ready dictionary among all clinet for the chracterSelction.
		SetPlayerReadyClientRpc(clientOwnerId);

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
			//Delete the lobby since no needed
			LobbyManager.Instance.DeleteLobbyAsync();

			//Load the game scene once all players are ready
			SceneLoader.LoadNetwork(SceneLoader.Scene.GameScene);
		}

	}

	[ClientRpc]
	private void SetPlayerReadyClientRpc(ulong clientOwnerId)
	{
		playerReadyDictionary[clientOwnerId] = true;
		OnReadyChanged?.Invoke(this, EventArgs.Empty);
	}

	public bool IsPlayerReady(ulong playerId)
	{
		return playerReadyDictionary.ContainsKey(playerId) && playerReadyDictionary[playerId];
	}
}
