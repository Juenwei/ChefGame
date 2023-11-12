using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Player player;
    private float footstepTimer;
    private float footstepTimerMax = 0.2f, footstepVolume = 1f;

    void Awake()
    {
        player = GetComponent<Player>();  
    }

    // Update is called once per frame
    void Update()
    {
        footstepTimer -= Time.deltaTime;
        if(footstepTimer <= 0f)
        {
			footstepTimer = footstepTimerMax;
            if(player.IsWalking)
			{
				SoundEffectManager.Instance.PlayFootstepSound(player.transform.position, footstepVolume);

			}
		}
    }
}
