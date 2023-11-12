using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeDeliveredText;
	[SerializeField] private Button playAgainButton;

	private void Start()
	{
		GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
		playAgainButton.onClick.AddListener(() =>
		{
			SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
		});
		Hide();
	}

	private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
	{
		if (GameManager.Instance.IsGameOver())
		{
			Show();
			recipeDeliveredText.text = DeliveryManager.instance.GetSucessfulRecipeCount().ToString();
		}
		else
		{
			Hide();
		}
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
