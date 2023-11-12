using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnWarningUI : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;

	private void Start()
	{
		stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
		Hide();
	}

	private void StoveCounter_OnProgressChanged(object sender, IShowProgress.OnProgressChangedArgs e)
	{
		var burnShowProgressAmount = 0.5f;

		if (stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount)
		{
			Show();
		}
		else
			Hide();
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
