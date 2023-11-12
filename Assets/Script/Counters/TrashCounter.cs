using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
	//Event Use for Play SOund effect
	public static event EventHandler OnObjectDispose;

	new public static void ResetStaticData()
	{
		OnObjectDispose = null;
	}

	public override void Interact(Player player)
	{
		if(player.HasKitchenObject())
		{
			//Call for destroy
			KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

			//RPC for Sound effect
			InteractLogicServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicServerRpc()
	{
		InteractLogicClientRpc();
	}

	[ClientRpc]
	private void InteractLogicClientRpc()
	{
		OnObjectDispose?.Invoke(this, EventArgs.Empty);
	}

}
