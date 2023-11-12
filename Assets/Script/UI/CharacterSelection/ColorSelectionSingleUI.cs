using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectionSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image colorUIImage;
    [SerializeField] private GameObject selectedUIGameObject;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			//Sned msg to MultiplayerManager that current client want change to this color.
			KitchenMultiplayerManager.Instance.ChangePlayerColor(colorId);
		});
	}

	private void Start()
	{
		KitchenMultiplayerManager.Instance.OnPlayerNetworkListChanged += KitchenMultiplayerManager_OnPlayerNetworkListChanged;
		colorUIImage.color = KitchenMultiplayerManager.Instance.GetPlayerColor(colorId);

		//Initialzie the seleted state based on the initial color for each player.
		UpdateIsSelected();
	}

	private void KitchenMultiplayerManager_OnPlayerNetworkListChanged(object sender, System.EventArgs e)
	{
		//While New player joined
		UpdateIsSelected();
	}

	public void UpdateIsSelected()
	{
		//Search though the player data list to find does any player occupied this color.

		if (KitchenMultiplayerManager.Instance.GetPlayerData().colorId == colorId)
		{
			//If ANy playerData's Match with color ID 
			selectedUIGameObject.SetActive(true);
		}
		else
		{
			selectedUIGameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		KitchenMultiplayerManager.Instance.OnPlayerNetworkListChanged -= KitchenMultiplayerManager_OnPlayerNetworkListChanged;
	}
}
