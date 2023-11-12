using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
	private void Awake()
	{
		//Desc : Since NetworkManager and kitchenMultiplayerManager will not be destroyed on switching Scene,
		//so it must be manually destroy due to those compnent are not in use in main menu scene
		if(NetworkManager.Singleton != null)
		{
			Destroy(NetworkManager.Singleton.gameObject);
		}

		if(KitchenMultiplayerManager.Instance != null)
		{
			Destroy(KitchenMultiplayerManager.Instance.gameObject);
		}

		if(LobbyManager.Instance != null)
		{
			Destroy(LobbyManager.Instance.gameObject);
		}
	}
}
