using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
	[SerializeField] private Button mainMenuButton, resumeButton, optionButton;

	private void Awake()
	{
		resumeButton.onClick.AddListener(() => {
			//For Resume Game
			GameManager.Instance.TogglePauseGame();
		});
		mainMenuButton.onClick.AddListener(() =>
		{
			Time.timeScale = 1;
			NetworkManager.Singleton.Shutdown();
			SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
		});
		optionButton.onClick.AddListener(() =>
		{
			Hide();
			GameSettingUI.Instance.Show(Show);
		});
	}

	private void Start()
	{
		GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
		GameManager.Instance.OnLocalGameResumed += GameManager_OnGameLocalResumed;

		Hide();
	}

	private void GameManager_OnGameLocalResumed(object sender, System.EventArgs e)
	{
		Hide();
	}

	private void GameManager_OnLocalGamePaused(object sender, System.EventArgs e)
	{
		Show();
	}

	public void Show()
    {
        gameObject.SetActive(true);
		resumeButton.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


}
