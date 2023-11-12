using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeDelivered;
	public event EventHandler OnRecipeSuccessed;
	public event EventHandler OnRecipeFailed;

	public static DeliveryManager instance { get; private set; }

    [SerializeField] private AllRecipeListSO allRecipeListSO;

    private List<RecipeSO> waitingRecipeSOs;

    private float spawnRecipeTimer = 3f, spawnRecipeTimerMax = 4f;
	private int waitingRecipeMax = 4, successfulRecipeCount = 0;

	private void Awake()
	{
		instance = this;
		waitingRecipeSOs = new List<RecipeSO>();
	}

	private void Update()
	{
        if (!IsServer) { return; }
        spawnRecipeTimer -= Time.deltaTime;
		if(spawnRecipeTimer <= 0)
		{
			spawnRecipeTimer = spawnRecipeTimerMax;

			if(GameManager.Instance.IsGamePlaying() && waitingRecipeSOs.Count < waitingRecipeMax )
			{
				var recipeSOIndex = UnityEngine.Random.Range(0, allRecipeListSO.recipeSOs.Count);
				SpawnNewRecipeClientRpc(recipeSOIndex);
			}

		}
		
	}

	#region RPC

	[ClientRpc]
	private void SpawnNewRecipeClientRpc(int waitingRecipeSOIndex)
	{
		var recipeSO = allRecipeListSO.recipeSOs[waitingRecipeSOIndex];
		waitingRecipeSOs.Add(recipeSO);
		OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DeliverCorrectRecipeServerRpc(int completedRecipeSOIndex)
	{
		//DEF : Send the RPC to the Server while client complete the recipe, and trigger broadcast
		DeliverCorrectRecipeClientRpc(completedRecipeSOIndex);
	}

	[ClientRpc]
	private void DeliverCorrectRecipeClientRpc(int completedRecipeSOIndex)
	{
		//DEF : Broadcast the RPC to all client.
		successfulRecipeCount++;
		waitingRecipeSOs.RemoveAt(completedRecipeSOIndex);
		OnRecipeDelivered?.Invoke(this, EventArgs.Empty);
		OnRecipeSuccessed?.Invoke(this, EventArgs.Empty);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DeliveryWrongRecipeServerRpc()
	{
		DeliveryWrongRecipeClientRpc();
	}

	[ClientRpc]
	private void DeliveryWrongRecipeClientRpc()
	{
		Debug.Log("Invalid recipe for the plate handed by player");
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);
	}

	#endregion
	public void DeliveryPlate(PlateKitchenObject plateKitchenObject)
	{
		for(int i = 0;i < waitingRecipeSOs.Count; i++)
		{
			var waitingRecipeSO = waitingRecipeSOs[i];
			
			if(waitingRecipeSO.kitchenObjectSOs.Count == plateKitchenObject.GetKitchenObjectSOs().Count)
			{
				var plateIngredientMatchRecipe = true;

				//Cycling though the waiting recipe
				foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOs)
				{
					var ingredientFound = false;
					//Cycling though the recipe on plate
					foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOs())
					{
						if (plateKitchenObjectSO == recipeKitchenObjectSO)
						{
							//IF found same Ingredient
							ingredientFound = true;
							break;
						}
					}

					if (!ingredientFound)
					{
						//The Recipe Ingridents doesnt match with recipe
						plateIngredientMatchRecipe = false;
						Debug.Log("Not Match with " + recipeKitchenObjectSO.name + " from " + waitingRecipeSO.recipeName);
					}
				}

				if (plateIngredientMatchRecipe)
				{
					//Player Deliver the correct plate.
					//Debug.Log("Player Deliver the " + waitingRecipeSO.recipeName + " sucessfully");

					DeliverCorrectRecipeServerRpc(i);
					return;
				}
			}
			

		}

		//Player Deliver the plate that doesnt match any of the recipe in waiting list
		DeliveryWrongRecipeServerRpc();
	}


	public List<RecipeSO> GetWaitingRecipeSOs()
	{
		return waitingRecipeSOs;
	}

	public int GetSucessfulRecipeCount()
	{
		return successfulRecipeCount;
	}
}
