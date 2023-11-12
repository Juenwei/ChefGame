using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
	public Transform GetKitchenObjectParentFollowTransform();

	public KitchenObject GetKitchenObject();

	public void SetKitchenObject(KitchenObject kitchenObjectParent);

	public void ClearKitchenObject();

	public bool HasKitchenObject();

	public NetworkObject GetNetworkObject();
}
