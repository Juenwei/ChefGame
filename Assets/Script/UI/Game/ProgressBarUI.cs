using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
	[SerializeField] private GameObject hasShowProgressGameObject;
	
	private IShowProgress showProgress;

	private void Start()
	{
		showProgress = hasShowProgressGameObject.GetComponent<IShowProgress>();
		if (showProgress == null)
			Debug.LogError("The Game Object "+ hasShowProgressGameObject + " reference doesn't included Interface");

		showProgress.OnProgressChanged += ShowProgress_OnProgressChanged;
		barImage.fillAmount = 0;
		Hide();
	}

	private void ShowProgress_OnProgressChanged(object sender, IShowProgress.OnProgressChangedArgs e)
	{
		barImage.fillAmount = e.progressNormalized;
		if(e.progressNormalized == 0 || e.progressNormalized == 1f)
		{
			Hide();
		}
		else
		{
			Show();
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
