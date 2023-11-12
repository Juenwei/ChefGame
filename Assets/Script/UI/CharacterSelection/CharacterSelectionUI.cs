using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton, readyButton;
	[SerializeField] private TextMeshProUGUI lobbyNameText, lobbyCodeText;

	private void Awake()
	{
		mainMenuButton.onClick.AddListener(() =>
		{
			//Shut Down Network
			NetworkManager.Singleton.Shutdown();

			//Leave Lobby
			LobbyManager.Instance.LeaveLobbyAsync();

			//Load Main MEnu Scene, now can use basic load since network manager already shut down.
			SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
		});

		readyButton.onClick.AddListener(() =>
		{
			//Msg Server for ready and Load Game Scene
			CharacterSelectionReady.Instance.SetPlayerReady();
		});
	}

	private void Start()
	{
		var lobby = LobbyManager.Instance.GetLobby();

		lobbyNameText.text = "Lobby Name : " + lobby.Name;
		lobbyCodeText.text = "Lobby Code : " + lobby.LobbyCode;
	}
}
