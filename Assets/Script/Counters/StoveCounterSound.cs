using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource stoveAudioSource;
	private float warningStoveTimer;
	private bool isPlayingWarningSound;

	private void Awake()
	{
		stoveAudioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		stoveCounter.OnStateChange += StoveCounter_OnStateChange;
		stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
	}

	private void StoveCounter_OnProgressChanged(object sender, IShowProgress.OnProgressChangedArgs e)
	{
		var burnShowProgressAmount = 0.5f;

		isPlayingWarningSound = (stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount);
	}

	private void StoveCounter_OnStateChange(object sender, StoveCounter.OnStateChangeEventArgs e)
	{
		bool isFrying = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;
		if (isFrying)
		{
			stoveAudioSource.Play();
		}else
		{
			stoveAudioSource.Pause();
		}
	}

	private void Update()
	{
		if (isPlayingWarningSound)
		{
			warningStoveTimer -= Time.deltaTime;
			if (warningStoveTimer <= 0f)
			{
				float warningSoundTimerMax = .2f;
				warningStoveTimer = warningSoundTimerMax;
				SoundEffectManager.Instance.PlayWarningSound(stoveCounter.transform.position);
			}
		}
		
	}
}
