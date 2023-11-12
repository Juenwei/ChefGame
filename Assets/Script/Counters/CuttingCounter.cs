using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IShowProgress
{
	public event EventHandler<IShowProgress.OnProgressChangedArgs> OnProgressChanged;

	public event EventHandler OnCut;
	public static event EventHandler OnAnyCut; 

	new public static void ResetStaticData()
	{
		OnAnyCut = null;
	}

	[SerializeField] private CuttingRecipeSO[] cuttingKitchenObjectSOs;

	private int cuttingProgress;

	#region Pickup and drop Kitchen Object

	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			//There is Nothing on the Counter
			if (player.HasKitchenObject())
			{
				//CONDITION : There is Kitchen object hold by player
				//Set the kitchen object to the counter
				if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
				{
					var kitchenObject = player.GetKitchenObject();
					kitchenObject.SetKitchenObjectParent(this);

					InteractResetCutProgressServerRpc();
				}
			}
			else
			{//CONDITION :  There is nothing hold by player
			}
		}
		else
		{
			//There is Kitchen Object on the counter
			if (player.HasKitchenObject())
			{
				//CONDITION : There is KitchenObject hold by player
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
				{
					//IF Player is holding a plate
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
					}
				}
			}
			else
			{
				//CONDITION : There is nothing hold by the player
				GetKitchenObject().SetKitchenObjectParent(player);

				//EXTRA LOGIC ADD-ON : Reset while the Player take th object from Cutting counter
				InteractResetCutProgressServerRpc();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractResetCutProgressServerRpc()
	{
		InteractResetCutProgressClientRpc();
	}

	[ClientRpc]
	private void InteractResetCutProgressClientRpc()
	{
		cuttingProgress = 0;

		OnProgressChanged?.Invoke(this, new IShowProgress.OnProgressChangedArgs()
		{
			progressNormalized = 0f
		});
	}

	#endregion


	#region Cutting Kitchen Object Logic
	public override void InteractAlternate(Player player)
	{
		//CONDITION : There is kicthen object on the counter and it can be cutted accroding to the recipe
		if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			//The reason of seperate RPC is to avoid duplicate RPC called

			//RPC for Cutting logic (Progress) ,animation and UI
			CutKitchenObjectServerRpc();

			//RPC for Check progress, spawn and destroy object
			TestCuttingProgressServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void CutKitchenObjectServerRpc()
	{
		//Check does player can cut this before sending RPC (Prevent Delay cause error)
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			CutKitchenObjectClientRpc();
		}

	}

	[ClientRpc]
	private void CutKitchenObjectClientRpc()
	{
		//SUMMARY
		//1. Go though recipe
		//2. Destroy existing KitchenObject on counter (Uncutted Version)
		//2. Spwan new KitchenObject(Cut Version)
		OnCut?.Invoke(this, EventArgs.Empty);
		OnAnyCut?.Invoke(this, EventArgs.Empty);

		cuttingProgress++;

		var currentCuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());

		OnProgressChanged?.Invoke(this, new IShowProgress.OnProgressChangedArgs()
		{
			progressNormalized = (float)cuttingProgress / currentCuttingRecipeSO.cuttingProgressMax
		});
	}

	[ServerRpc(RequireOwnership = false)]
	private void TestCuttingProgressServerRpc()
	{
		//Check does player can cut this before sending RPC (Prevent Delay cause error)
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			var currentCuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());
			if (cuttingProgress >= currentCuttingRecipeSO.cuttingProgressMax)
			{
				var kitchenObjectSOOutput = GetOutputFromInput(GetKitchenObject().GetKitchenObjectSO());

				KitchenObject.DestroyKitchenObject(GetKitchenObject());

				KitchenObject.SpwanKitchenObject(kitchenObjectSOOutput, this);
			}
		}
	}

	#endregion
	private bool HasRecipeWithInput(KitchenObjectSO kitchenObjectSOInput)
	{
		return GetCuttingRecipeSOFromInput(kitchenObjectSOInput) != null;
	}

	private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSOInput)
	{
		var cuttingRecipeSO = GetCuttingRecipeSOFromInput(kitchenObjectSOInput);
		if(cuttingRecipeSO != null)
		{
			return cuttingRecipeSO.output;
		}
		else
		{
			return null;
		}
	}

	private CuttingRecipeSO GetCuttingRecipeSOFromInput(KitchenObjectSO kitchenObjectSOInput)
	{
		foreach (CuttingRecipeSO cuttingRecipe in cuttingKitchenObjectSOs)
		{
			if (kitchenObjectSOInput == cuttingRecipe.input)
			{
				return cuttingRecipe;
			}
		}
		Debug.LogWarning("Handled Exception : Unable Found Cutting Recipe for current Kitchen Object");
		return null;
	}
}
