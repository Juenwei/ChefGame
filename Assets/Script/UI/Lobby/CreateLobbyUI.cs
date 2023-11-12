using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] Button closeButton, createPublicLobbyButton, createPrivateLobbyButton;
    [SerializeField] TMP_InputField lobbyNameTextField;

	private void Awake()
	{
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

		createPublicLobbyButton.onClick.AddListener(() =>
		{
			LobbyManager.Instance.CreateLobbyAsync(lobbyNameTextField.text, false);
		});

		createPrivateLobbyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobbyAsync(lobbyNameTextField.text, true);
        });

        Hide();
	}

	public void Show()
    {
        gameObject.SetActive(true);
		createPublicLobbyButton.Select();

	}

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
