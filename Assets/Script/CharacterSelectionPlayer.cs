using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

//CLASS DESC : <<This class is the basic class that existed on the player game object in chracterSelectionScene, is roles is update the visual based on the request.>>
public class CharacterSelectionPlayer : MonoBehaviour
{
	[SerializeField] private int playerIndex;
	[SerializeField] private GameObject readyIconGameObject;
	[SerializeField] private PlayerVisual playerVisual;
	[SerializeField] private Button kickButton;
	[SerializeField] private Canvas playerWorldCanvas;
	[SerializeField] private TextMeshPro playerName;


	private void Awake()
	{
		playerWorldCanvas.worldCamera = Camera.main;

		kickButton.onClick.AddListener(() =>
		{
			var playerData = KitchenMultiplayerManager.Instance.GetPlayerDataByPlayerIndex(playerIndex);
			LobbyManager.Instance.KickPlayerAsync(playerData.playerLobbyId.ToString());
			KitchenMultiplayerManager.Instance.KickPlayer(playerData.clientId);
		});
	}

	private void Start()
	{
		KitchenMultiplayerManager.Instance.OnPlayerNetworkListChanged += KitchenMultiplayer_OnPlayerNetworkListChanged;
		CharacterSelectionReady.Instance.OnReadyChanged += CharacterSelectionReady_OnReadyChanged;

		kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

		//For Hiding the Player Visual at the beginning
		UpdatePlayer();
	}

	private void CharacterSelectionReady_OnReadyChanged(object sender, System.EventArgs e)
	{
		UpdatePlayer();
	}

	private void KitchenMultiplayer_OnPlayerNetworkListChanged(object sender, System.EventArgs e)
	{
		UpdatePlayer();
	}

	private void UpdatePlayer()
	{
		if(KitchenMultiplayerManager.Instance.IsPlayerIndexConnected(playerIndex))
		{
			Show();

			//Show Ready Text Based on the Player Ready Status.
			var playerData = KitchenMultiplayerManager.Instance.GetPlayerDataByPlayerIndex(playerIndex);
			readyIconGameObject.SetActive(CharacterSelectionReady.Instance.IsPlayerReady(playerData.clientId));

			//Update Player Name from PlayerPref
			playerName.text = playerData.playerName.ToString();

			//Update Player Visual's color based on the player index ,SO EVERY PLAYER HAVE DIFFERENT COLOR AT THE BEGINNING
			playerVisual.SetPlayerColor(KitchenMultiplayerManager.Instance.GetPlayerColor(playerData.colorId));
		}
		else
		{
			Hide();
		}
	}

	private void Show() { gameObject.SetActive(true); }

	private void Hide() { gameObject.SetActive(false); }

	private void OnDestroy()
	{
		KitchenMultiplayerManager.Instance.OnPlayerNetworkListChanged -= KitchenMultiplayer_OnPlayerNetworkListChanged;
	}
}
