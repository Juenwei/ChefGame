using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{

	[SerializeField] private BaseCounter baseCounter;
	[SerializeField] private GameObject[] visualCounterObjects;

    void Start()
    {
		//Add Script into the "SelectedCounter" Channel
		if(Player.LocalInstance != null)
		{
			Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;

		}
		else
		{
			Player.OnAnyPlayerSpawn += Player_OnAnyPlayerSpawn;
		}
    }

	private void Player_OnAnyPlayerSpawn(object sender, EventArgs e)
	{
		if (Player.LocalInstance != null)
		{
			//To Avoid duplicate subsription
			Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
			Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
		}
		
	}

	private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedArgs e)
	{
		if(e.selectedCounter == baseCounter)
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
		foreach(var visualCounterObject in visualCounterObjects) { 
			visualCounterObject.SetActive(true);
		}
	}

	private void Hide()
	{
		foreach (var gameObject in visualCounterObjects)
		{
			gameObject.SetActive(false);
		}
	}
}
