using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playSingleplayerButton, playMultiplayerButton, quitButton;

	private void Start()
	{
		playMultiplayerButton.onClick.AddListener(() =>
		{
			KitchenMultiplayerManager.IsMultiplayerMode = true;
			SceneLoader.Load(SceneLoader.Scene.LobbyScene);
		});

		playSingleplayerButton.onClick.AddListener(() =>
		{
			KitchenMultiplayerManager.IsMultiplayerMode = false;
			SceneLoader.Load(SceneLoader.Scene.LobbyScene);
		});

		quitButton.onClick.AddListener(() =>
		{
			Application.Quit();
		});
	}
}
