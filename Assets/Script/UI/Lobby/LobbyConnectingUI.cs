using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyConnectingUI : MonoBehaviour
{
	private void Start()
	{
		//Due to the KictheMultiplayerMAnager will not Destroy and this script will be destroy (Different lifetime) ,so the event but be unsubcribe
		KitchenMultiplayerManager.Instance.OnTryToConnectGame += KitchenMultiplayerManager_OnTryToConnectGame;
		KitchenMultiplayerManager.Instance.OnFailToConnectGame += KitchenMultiplayerManager_OnFailToConnectGame;
		Hide();
	}

	private void KitchenMultiplayerManager_OnFailToConnectGame(object sender, System.EventArgs e)
	{
		//Once client connect fail close the connecting UI
		Hide();
	}

	private void KitchenMultiplayerManager_OnTryToConnectGame(object sender, System.EventArgs e)
	{
		Show();
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
		KitchenMultiplayerManager.Instance.OnTryToConnectGame -= KitchenMultiplayerManager_OnTryToConnectGame;
		KitchenMultiplayerManager.Instance.OnFailToConnectGame -= KitchenMultiplayerManager_OnFailToConnectGame;
	}
}
