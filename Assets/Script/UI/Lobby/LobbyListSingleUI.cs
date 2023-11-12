using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    private Lobby lobby;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			//Cannot use lobby code since it is null due to the reference type lobby cannot get the lobby code.
			LobbyManager.Instance.JoinLobbyByLobbyIdAsync(lobby.Id);
		});
	}

	public void SetLobby(Lobby lobby)
    {
        lobbyNameText.text = lobby.Name;
		this.lobby = lobby;
    }


}