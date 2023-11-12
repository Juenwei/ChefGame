using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton, startClientButton;

	private void Awake()
	{
		startHostButton.onClick.AddListener(() =>
		{
			Debug.Log("Host Selected");
			KitchenMultiplayerManager.Instance.StartHost();
			Hide();
		});

		startClientButton.onClick.AddListener(() =>
		{
			Debug.Log("Client Selected");
			KitchenMultiplayerManager.Instance.StartClient();
			Hide();
		});
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}
}
