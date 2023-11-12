using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class GameStartCountdownSingleUI : MonoBehaviour
{
	private const string NUMBER_POPUP = "NumberPopUp";

    [SerializeField] private TextMeshProUGUI countdownText;

	private Animator animator;

	private int previousNumber;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
		Hide();
	}

	private void Update()
	{
		var currentNumber = Mathf.CeilToInt(GameManager.Instance.GetCountdownTimer());
		countdownText.text = currentNumber.ToString();
		if(previousNumber != currentNumber)
		{
			previousNumber = currentNumber;
			animator.SetTrigger(NUMBER_POPUP);
			SoundEffectManager.Instance.PlayCoundownSound();
		}
	}

	private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
	{
		if(GameManager.Instance.IsGameCountdownStarted())
		{
			Show();
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
