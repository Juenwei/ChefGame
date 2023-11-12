using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
	public event EventHandler OnPlateSpawn;
	public event EventHandler OnPlateRemoved;

	[SerializeField] private KitchenObjectSO plateKitchenObjectSO;

	private float spawnPlateTimer;
	private float spawnPlateTimerMax = 4f;
	private int spawnPlateAmount, spawnPlateAmountMax = 4;

	private void Update()
	{
		//Let server only Update
		if(!IsServer) return;

		spawnPlateTimer += Time.deltaTime;
		if( spawnPlateTimer > spawnPlateTimerMax )
		{
			spawnPlateTimer = 0;

			if(GameManager.Instance.IsGamePlaying() && spawnPlateAmount < spawnPlateAmountMax )
			{
				SpawnPlateServerRpc();
			}
		}
	}

	[ServerRpc]
	private void SpawnPlateServerRpc()
	{
		SpawnPlateClientRpc();
	}

	[ClientRpc]
	private void SpawnPlateClientRpc()
	{
		spawnPlateAmount++;
		OnPlateSpawn?.Invoke(this, EventArgs.Empty);
		Debug.DrawRay(transform.position, Vector3.up, Color.blue, 3f);
	}

	public override void Interact(Player player)
	{
		//DO RPC
		if (!player.HasKitchenObject())
		{
			KitchenObject.SpwanKitchenObject(plateKitchenObjectSO, player);
			InteractLogicServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void InteractLogicServerRpc()
	{
		InteractLogicClientRpc();

	}

	[ClientRpc]
	public void InteractLogicClientRpc()
	{
		if (spawnPlateAmount > 0)
		{
			spawnPlateAmount--;
			OnPlateRemoved?.Invoke(this, EventArgs.Empty);
		}
	}
}
