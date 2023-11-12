using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
	public static event EventHandler OnObjectPlaced;
	[SerializeField] private Transform kitchenObjectHoldPoint;

	public static void ResetStaticData()
	{
		OnObjectPlaced = null;
	}

	private KitchenObject currentKitchenObject;
	public virtual void Interact(Player player)
    {
        Debug.LogError("The BaseCounter.Interact() WAS called ,this should never be happen");
    }

	public virtual void InteractAlternate(Player player)
	{
		//Debug.LogError("The BaseCounter.InteractAlternate() WAS called ,this should never be happen"); Quick Fix
	}

	public Transform GetKitchenObjectParentFollowTransform()
	{
		return kitchenObjectHoldPoint;
	}

	public KitchenObject GetKitchenObject()
	{
		return currentKitchenObject;
	}

	public void SetKitchenObject(KitchenObject kitchenObject)
	{
		currentKitchenObject = kitchenObject;
		if(kitchenObject != null)
		{
			OnObjectPlaced?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ClearKitchenObject()
	{
		currentKitchenObject = null;
	}

	public bool HasKitchenObject()
	{
		return currentKitchenObject != null;
	}

	public NetworkObject GetNetworkObject()
	{
		return NetworkObject;
	}
}
