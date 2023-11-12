using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectedUI : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;

	private void Start()
	{
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
		playAgainButton.onClick.AddListener(() =>
		{
			//Reset Time Scale
			Time.timeScale = 1.0f;

			//Shut down Network onve player back to main menuy
			NetworkManager.Singleton.Shutdown();

			//Remove player from lobby
			LobbyManager.Instance.LeaveLobbyAsync();

			//Load Main Menu Scene
			SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
		});
		Hide();
	}

	private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
	{
		if(clientId == NetworkManager.ServerClientId)
		{
			//If Server is the one who disconnect
			Show();
		}
	}

	private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

	private void OnDestroy()
	{
		if(NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
		}

	}
}
