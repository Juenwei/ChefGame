using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

	public override void Interact(Player player)
    {
		if(!HasKitchenObject())
		{
			//There is Nothing on the Counter
			if(player.HasKitchenObject())
			{
				//CONDITION : There is Kitchen object hold by player
				//Set the kitchen object to the counter
				player.GetKitchenObject().SetKitchenObjectParent(this);
			}
			else{//CONDITION :  There is nothing hold by player
			}
		}
		else
		{
			//There is Kitchen Object on the counter
			if(player.HasKitchenObject())
			{
				//CONDITION : There is KitchenObject hold by player
				if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
				{
					//IF Player is holding a plate
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
					}
				}
				else
				{
					if(GetKitchenObject().TryGetPlate(out plateKitchenObject))
					{
						//If the plate is on clear counter
						if(plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
						{
							KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
						}
					}
				}
			}
			else
			{
				//CONDITION : There is nothing hold by the player
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
		
	}
}
