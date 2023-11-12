using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
	public event EventHandler OnPlayerGrabbedObject;
	


	[SerializeField] private KitchenObjectSO kitchenObjectSO;
	public override void Interact(Player player)
	{
		if(!player.HasKitchenObject())
		{
			KitchenObject.SpwanKitchenObject(kitchenObjectSO, player);

			InteractLogicServerRpc();
		}
	}

	//Allow all clinet send RPC to update the animation for all cleint 
	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicServerRpc()
	{
		InteractLogicClientRpc();
	}

	[ClientRpc]
	private void InteractLogicClientRpc()
	{
		OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
	}

}
