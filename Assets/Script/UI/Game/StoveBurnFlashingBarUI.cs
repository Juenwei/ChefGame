using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
	private const string IS_BAR_FLASHING = "IsFlashing";
	[SerializeField] private StoveCounter stoveCounter;

	private Animator animator;

	private void Start()
	{
		stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
		animator = GetComponent<Animator>();
		animator.SetBool(IS_BAR_FLASHING, false);
	}

	private void StoveCounter_OnProgressChanged(object sender, IShowProgress.OnProgressChangedArgs e)
	{
		var burnShowProgressAmount = 0.5f;

		var isWarning = (stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount);
		animator.SetBool(IS_BAR_FLASHING, isWarning);
		
	}

}
