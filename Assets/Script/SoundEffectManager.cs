using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
	private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectVolume";
	public static SoundEffectManager Instance { get; private set; }

	[SerializeField] private AudioClipRefsSO audioClipRefsSO;

	private float volume = 1f;

	private void Awake()
	{
		Instance = this;
		volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME,1f);
	}

	private void Start()
	{
		DeliveryManager.instance.OnRecipeSuccessed += DeliveryManager_OnRecipeSuccessed;
		DeliveryManager.instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
		CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
		Player.OnAnyPlayerPickUpSomething += Player_OnAnyPlayerPickUpSomething;
		BaseCounter.OnObjectPlaced += BaseCounter_OnObjectPlaced;
		TrashCounter.OnObjectDispose += TrashCounter_OnObjectDispose;
	}

	private void TrashCounter_OnObjectDispose(object sender, System.EventArgs e)
	{
		var trashCounter = sender as TrashCounter;
		PlaySound(audioClipRefsSO.trashClips[Random.Range(0, audioClipRefsSO.trashClips.Length - 1)], trashCounter.transform.position);
	}

	private void BaseCounter_OnObjectPlaced(object sender, System.EventArgs e)
	{
		var baseCounter = sender as BaseCounter;
		PlaySound(audioClipRefsSO.objectDropClips[Random.Range(0, audioClipRefsSO.objectDropClips.Length - 1)], baseCounter.transform.position);
	}

	private void Player_OnAnyPlayerPickUpSomething(object sender, System.EventArgs e)
	{
		var player = sender as Player;
		PlaySound(audioClipRefsSO.objectPickupClips[Random.Range(0, audioClipRefsSO.chopClips.Length - 1)], player.transform.position);
	}

	private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
	{
		var cuttingCounter = sender as CuttingCounter;
		PlaySound(audioClipRefsSO.chopClips[Random.Range(0, audioClipRefsSO.chopClips.Length - 1)], cuttingCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
	{
		var deliveryCounter = DeliveryCounter.Instance;
		PlaySound(audioClipRefsSO.deliveryFailClips, deliveryCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeSuccessed(object sender, System.EventArgs e)
	{
		var deliveryCounter = DeliveryCounter.Instance;
		PlaySound(audioClipRefsSO.deliverySuccessClips, deliveryCounter.transform.position);
	}
	public void PlayFootstepSound(Vector3 position, float volumeMultiplier)
	{
		PlaySound(audioClipRefsSO.footsetpClips[Random.Range(0,audioClipRefsSO.footsetpClips.Length -1)], position, volumeMultiplier);
	}

	public void PlayCoundownSound()
	{
		PlaySound(audioClipRefsSO.warningClips[Random.Range(0, audioClipRefsSO.warningClips.Length - 1)], transform.position);
	}

	public void PlayWarningSound(Vector3 position)
	{
		PlaySound(audioClipRefsSO.warningClips[Random.Range(0,audioClipRefsSO.warningClips.Length -1)],position);
	}
	private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume * volumeMultiplier);
    }


	private void PlaySound(AudioClip[] audioClips,  Vector3 position, float volumeMultiplier = 1f)
	{
		AudioSource.PlayClipAtPoint(audioClips[Random.Range(0, audioClips.Length - 1)], position, volume * volumeMultiplier);
	}



	public void ChangeVolume()
	{
		volume += 0.1f;
		if(volume > 1f) {
			volume = 0f;
		}

		PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
		PlayerPrefs.Save();
	}

	public float GetVolume() { return volume; }
}
