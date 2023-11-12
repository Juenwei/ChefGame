using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePauseMultiplayerUI : MonoBehaviour
{

	private void Start()
	{
		GameManager.Instance.OnMultiplayerPaused += GameManager_OnMultiplayerPaused;
		GameManager.Instance.OnMultiplayerResumed += GameManager_OnMultiplayerResumed;

		Hide();
	}

	private void GameManager_OnMultiplayerResumed(object sender, System.EventArgs e)
	{
		Hide();
	}

	private void GameManager_OnMultiplayerPaused(object sender, System.EventArgs e)
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

}
