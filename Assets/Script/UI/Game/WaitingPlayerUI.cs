using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingPlayerUI : MonoBehaviour
{
	private void Start()
	{
		GameManager.Instance.OnLocalPlayerReadyChanged += GamaManager_OnLocalPlayerReadyChanged;
		GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

		Hide();
	}

	private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
	{
		if (GameManager.Instance.IsGameCountdownStarted())
		{
			Hide();
		}
	}

	private void GamaManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
	{
		//ADDED EXTRA LOGIC TO Prevent UI Show after Countdown start
		if(GameManager.Instance.IsLocalPlayerReady() && !GameManager.Instance.IsGameCountdownStarted()) 
		{
			Show();
		}
	}



	private void Show()
    {
		Debug.Log("Show");
		gameObject.SetActive(true);
    }

    private void Hide()
    {
		Debug.Log("Hide");
        gameObject.SetActive(false);
    }
}
