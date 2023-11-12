using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyResponseMessageUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI messageText;
	[SerializeField] private Button closeButton;

	private void Awake()
	{
		closeButton.onClick.AddListener(Hide);
	}

	private void Start()
	{
		//Due to the KictheMultiplayerMAnager will not Destroy and this script will be destroy (Different lifetime) ,so the event but be unsubcribe
		KitchenMultiplayerManager.Instance.OnFailToConnectGame += KitchenMultiplayerManager_OnFailToConnectGame;
		LobbyManager.Instance.OnLobbyCreateStarted += LobbyManager_OnLobbyCreateStarted;
		LobbyManager.Instance.OnLobbyCreateFailed += LobbyManager_OnLobbyCreateFailed;
		LobbyManager.Instance.OnJoinFailed += LobbyManager_OnJoinFailed;
		LobbyManager.Instance.OnJoinStarted += LobbyManager_OnJoinStarted;
		LobbyManager.Instance.OnQuickJoinFailed += LobbyManager_OnQuickJoinFailed;

		Hide();
	}

	private void LobbyManager_OnQuickJoinFailed(object sender, System.EventArgs e)
	{
		ShowMessage("Could not found any Lobby to Quick Join!");
	}

	private void LobbyManager_OnJoinStarted(object sender, System.EventArgs e)
	{
		ShowMessage("Joining Lobby....");
	}

	private void LobbyManager_OnJoinFailed(object sender, System.EventArgs e)
	{
		ShowMessage("Cannot found the Lobby that matches criteria!");
	}

	private void LobbyManager_OnLobbyCreateFailed(object sender, System.EventArgs e)
	{
		ShowMessage("Failed to Create Lobby!");
	}

	private void LobbyManager_OnLobbyCreateStarted(object sender, System.EventArgs e)
	{
		ShowMessage("Creating Lobby....");
	}

	private void KitchenMultiplayerManager_OnFailToConnectGame(object sender, System.EventArgs e)
	{
		Show();

		if (NetworkManager.Singleton.DisconnectReason == "")
		{
			//Connection Timeout
			ShowMessage("Failed To Connect");
		}
		else
		{
			//Print out the disconnect reason.
			ShowMessage(NetworkManager.Singleton.DisconnectReason);
		}

	}

	private void ShowMessage(string message)
	{
		Show();
		messageText.text = message;
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
		KitchenMultiplayerManager.Instance.OnFailToConnectGame -= KitchenMultiplayerManager_OnFailToConnectGame;
		LobbyManager.Instance.OnLobbyCreateStarted -= LobbyManager_OnLobbyCreateStarted;
		LobbyManager.Instance.OnLobbyCreateFailed -= LobbyManager_OnLobbyCreateFailed;
		LobbyManager.Instance.OnJoinFailed -= LobbyManager_OnJoinFailed;
		LobbyManager.Instance.OnJoinStarted -= LobbyManager_OnJoinStarted;
		LobbyManager.Instance.OnQuickJoinFailed -= LobbyManager_OnQuickJoinFailed;
	}
}
