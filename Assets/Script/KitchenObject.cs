using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

	private IKitchenObjectParent currentKitchenObjectParent;
	private FollowTransform followTransform;

	protected virtual void Awake()
	{
		followTransform = GetComponent<FollowTransform>();
	}

	//Static function allow the spawning function no need any kitchen object reference to run it.
	public static void SpwanKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
	{
		KitchenMultiplayerManager.Instance.SpwanKitchenObject(kitchenObjectSO, kitchenObjectParent);
	}

	//No need to be Static function but just make it to follow the standard 
	public static void DestroyKitchenObject(KitchenObject kitchenObject)
	{
		KitchenMultiplayerManager.Instance.DestroyKitchenObject(kitchenObject);
	}

	public KitchenObjectSO GetKitchenObjectSO() { return kitchenObjectSO; }

	public IKitchenObjectParent GetKitchenObject() { return currentKitchenObjectParent; }
	
	//Function for Assign the Kitchen Object to the new counter
	public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
	{
		SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
	}

	//Allow every client node calls server for setting new Kitchen
	[ServerRpc(RequireOwnership = false)]
	private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
	{
		SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
	}


	//Assigning new parent for every client node
	[ClientRpc]
	private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
	{
		kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject networkObject);
		var kitchenObjectParent = networkObject.GetComponent<IKitchenObjectParent>();

		//Clear current Counter and replace new counter's data
		if (currentKitchenObjectParent != null)
		{
			currentKitchenObjectParent.ClearKitchenObject();
		}

		currentKitchenObjectParent = kitchenObjectParent;

		if (kitchenObjectParent.HasKitchenObject())
		{
			Debug.LogError("The Target Counter already has a kitchen object, THis should be never happened");
		}

		//Set the new object as the parent object 
		kitchenObjectParent.SetKitchenObject(this);

		//Update the mesh to follow parent.
		followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectParentFollowTransform());
	}

	public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
	{
		if(this is PlateKitchenObject)
		{
			plateKitchenObject = (PlateKitchenObject)this;
			return true;
		}
		else
		{
			plateKitchenObject=null;
			return false;
		}
	}

	public void DestroySelf()
	{
		Destroy(gameObject);
	}

	public void ClearKitchenObjectParent()
	{
		currentKitchenObjectParent.ClearKitchenObject();
	}

}
