using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IShowProgress
{
	public event EventHandler<IShowProgress.OnProgressChangedArgs> OnProgressChanged;
	public event EventHandler<OnStateChangeEventArgs> OnStateChange;
	public class OnStateChangeEventArgs : EventArgs
	{
		public State state;
	}

	public enum State
	{
		Idle,
		Frying,
		Fried,
		Burned
	}

	[SerializeField] private FryingRecipeSO[] fryingRecipeSOs;
	[SerializeField] private BurningRecipeSO[] burningRecipeSOs;


	private FryingRecipeSO currentFryingRecipeSO;
	private BurningRecipeSO currentBurningRecipeSO;

	private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f)
		, burnedTimer = new NetworkVariable<float>(0f);
	private NetworkVariable<State> currentState = new NetworkVariable<State>(State.Idle);

	private void Start()
	{

	}

	public override void OnNetworkSpawn()
	{
		currentState.Value = State.Idle;

		//Sync Timer
		fryingTimer.OnValueChanged += FryingTimerOnValueChanged;
		burnedTimer.OnValueChanged += BurningTimerOnValueChanged;

		//Sync State
		currentState.OnValueChanged += CurrentStateOnValueChanged;
	}

	private void FryingTimerOnValueChanged(float previousValue, float newValue)
	{
		//Check Current Recipe is exist to avoid null exception on max timer var.
		var fryingTimerMax = currentFryingRecipeSO != null ? currentFryingRecipeSO.fryingTimerMax : 1f;

		OnProgressChanged?.Invoke(this, new IShowProgress.OnProgressChangedArgs()
		{
			progressNormalized = fryingTimer.Value / fryingTimerMax
		});
	}

	private void BurningTimerOnValueChanged(float previousValue, float newValue)
	{
		var burningTimerMax = currentBurningRecipeSO != null ? currentBurningRecipeSO.burningTimerMax : 1f;
		OnProgressChanged?.Invoke(this, new IShowProgress.OnProgressChangedArgs()
		{
			progressNormalized = burnedTimer.Value / burningTimerMax
		});
	}

	private void CurrentStateOnValueChanged(State previousValue, State newValue)
	{
		OnStateChange?.Invoke(this, new OnStateChangeEventArgs()
		{
			state = currentState.Value,
		});

		//Hidning the bar if IDle or burned
		if (currentState.Value == State.Idle || currentState.Value == State.Burned)
		{
			OnProgressChanged?.Invoke(this, new IShowProgress.OnProgressChangedArgs()
			{
				progressNormalized = 0f
			});
		}
	}

	private void Update()
	{
		if(!IsServer) { return; }

		if(HasKitchenObject())
		{
			switch (currentState.Value)
			{
				case State.Idle:
					break;

				case State.Frying:
					fryingTimer.Value += Time.deltaTime;


					if (fryingTimer.Value > currentFryingRecipeSO.fryingTimerMax)
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
						KitchenObject.SpwanKitchenObject(currentFryingRecipeSO.output, this);

						//Since it is only run on server so it's okay to access network varaible
						currentState.Value = State.Fried;
						burnedTimer.Value = 0f;

						//Change the Current Recipe to the frying for next state change (Cook -> Burned)
						SetBurningRecipeSOClientRpc(KitchenMultiplayerManager.Instance.GetKitchenObjectSOIndex(
							GetKitchenObject().GetKitchenObjectSO()));
					}
					break;

				case State.Fried:
					burnedTimer.Value += Time.deltaTime;

					if (burnedTimer.Value > currentBurningRecipeSO.burningTimerMax) 
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
						KitchenObject.SpwanKitchenObject(currentBurningRecipeSO.output, this);
						currentState.Value = State.Burned;
					}
					break;

				case State.Burned:
					break;
			}
		}
	}

	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			//There is Nothing on the Counter
			if (player.HasKitchenObject())
			{
				//Player Put Somthing on the stove
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
				{
					var kitchenObject = player.GetKitchenObject();
					kitchenObject.SetKitchenObjectParent(this);

					InteractLogicPlaceKitchenObjectOnStoveServerRpc(
						KitchenMultiplayerManager.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
				}
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
						SetIdleStateServerRpc();
					}
				}
			}
			else
			{
				//Player Take Item from Stove
				GetKitchenObject().SetKitchenObjectParent(player);
				SetIdleStateServerRpc();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetIdleStateServerRpc()
	{
		currentState.Value = State.Idle;
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicPlaceKitchenObjectOnStoveServerRpc(int kitchenObjectSOIndex)
	{
		//LEt the server do the network variable Initialization and Update value
		fryingTimer.Value = 0f;
		currentState.Value = State.Frying;

		SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
	}

	[ClientRpc]
	private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
	{
		currentFryingRecipeSO = GetFryingRecipeSOFromInput(
			KitchenMultiplayerManager.Instance.GetKitchenObjectSOByIndex(kitchenObjectSOIndex));
	}

	[ClientRpc]
	private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
	{
		currentBurningRecipeSO = GetBurningRecipeSOFromInput(
			KitchenMultiplayerManager.Instance.GetKitchenObjectSOByIndex(kitchenObjectSOIndex));
	}

	private bool HasRecipeWithInput(KitchenObjectSO KitchenObjectSOInput)
	{
		return GetFryingRecipeSOFromInput(KitchenObjectSOInput) != null;
	}

	private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSOInput)
	{
		var cuttingRecipeSO = GetFryingRecipeSOFromInput(kitchenObjectSOInput);
		if (cuttingRecipeSO != null)
		{
			return cuttingRecipeSO.output;
		}
		else
		{
			return null;
		}
	}

	private FryingRecipeSO GetFryingRecipeSOFromInput(KitchenObjectSO kitchenObjectSOInput)
	{
		foreach (FryingRecipeSO fryingRecipe in fryingRecipeSOs)
		{
			if (kitchenObjectSOInput == fryingRecipe.input)
			{
				return fryingRecipe;
			}
		}
		Debug.LogWarning("Handled Exception : Unable Found Frying Recipe for current Kitchen Object");
		return null;
	}

	private BurningRecipeSO GetBurningRecipeSOFromInput(KitchenObjectSO kitchenObjectSOInput)
	{
		foreach (BurningRecipeSO burningRecipe in burningRecipeSOs)
		{
			if (kitchenObjectSOInput == burningRecipe.input)
			{
				return burningRecipe;
			}
		}
		Debug.LogWarning("Handled Exception : Unable Found Burning Recipe for current Kitchen Object");
		return null;
	}

	public bool IsFried()
	{
		return currentState.Value == State.Fried;
	}
}
