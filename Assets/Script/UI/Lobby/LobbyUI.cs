using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton, createLobbyButton, quickJoinLobbyButton, joinLobbyByCodeButton;

	[SerializeField] CreateLobbyUI createLobbyUI;

	[SerializeField] TMP_InputField lobbyCodeTextField, playerNameTextField;

	[SerializeField] Transform lobbyTemplate, lobbyContainer;

	private void Awake()
	{
		mainMenuButton.onClick.AddListener(() =>
		{
			LobbyManager.Instance.LeaveLobbyAsync();
			SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
		});

		createLobbyButton.onClick.AddListener(() =>
		{
			createLobbyUI.Show();

		});

		quickJoinLobbyButton.onClick.AddListener(() =>
		{
			LobbyManager.Instance.QuickJoinLobbyAsync();
		});

		joinLobbyByCodeButton.onClick.AddListener(() =>
		{
			LobbyManager.Instance.JoinLobbyByLobbyCodeAsync(lobbyCodeTextField.text);
		});


	}

	private void Start()
	{
		//Listen Event
		LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;

		//Set initial player name to the text field
		playerNameTextField.text = KitchenMultiplayerManager.Instance.GetPlayerName();
		playerNameTextField.onValueChanged.AddListener((string newName) =>
		{
			KitchenMultiplayerManager.Instance.SetPlayerName(newName);
		});

		lobbyTemplate.gameObject.SetActive(false);

		//Initizalize a empty list
		UpdateLobbyListUI(new List<Lobby>());
	}

	private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
	{
		UpdateLobbyListUI(e.lobbies);
	}

	private void UpdateLobbyListUI(List<Lobby> lobbies)
	{
		//Clean Up the list
		foreach(Transform child in lobbyContainer)
		{
			if (child == lobbyTemplate) continue;
			Destroy(child.gameObject);
		}

		//Adding new lobbies to te list
		foreach(var lobby in lobbies)
		{
			var lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
			lobbyTransform.gameObject.SetActive(true);
			lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
		}
	}

	private void OnDestroy()
	{
		LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
	}
}
