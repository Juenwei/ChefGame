using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
	public event EventHandler<OnIngredientAddedArgs> OnIngredientAdded;
	public class OnIngredientAddedArgs : EventArgs
	{
		public KitchenObjectSO kitchenObjectSO;
	}


	[SerializeField] private List<KitchenObjectSO> validKitchenSOs;
    private List<KitchenObjectSO> currentKitchenObjectSOs;

	protected override void Awake()
	{
		base.Awake();
		currentKitchenObjectSOs = new List<KitchenObjectSO>();

	}

	public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
	{
		if(!validKitchenSOs.Contains(kitchenObjectSO))
		{
			return false;
		}

		//Check Duplucate
		if (currentKitchenObjectSOs.Contains(kitchenObjectSO))
		{
			return false;
		}
		else
		{
			GetIngredientServerRpc(KitchenMultiplayerManager.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));
			return true;
		}


	}

	[ServerRpc(RequireOwnership = false)]
	private void GetIngredientServerRpc(int kitchenObjectSOIndex)
	{
		GetIngredientClientRpc(kitchenObjectSOIndex);
	}

	[ClientRpc]
	private void GetIngredientClientRpc(int kitchenObjectSOIndex)
	{
		var kitchenObjectSO = KitchenMultiplayerManager.Instance.GetKitchenObjectSOByIndex(kitchenObjectSOIndex);
		currentKitchenObjectSOs.Add(kitchenObjectSO);

		OnIngredientAdded?.Invoke(this, new OnIngredientAddedArgs
		{
			kitchenObjectSO = kitchenObjectSO
		});
	}

	public List<KitchenObjectSO> GetKitchenObjectSOs() { return currentKitchenObjectSOs; }
}
