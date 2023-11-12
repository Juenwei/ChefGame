using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container, recipeTemplate;

	private void Awake()
	{
		recipeTemplate.gameObject.SetActive(false);
	}

	void Start()
    {
		DeliveryManager.instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
		DeliveryManager.instance.OnRecipeDelivered += DeliveryManager_OnRecipeDelivered;
		UpdateVisual();
	}

	private void DeliveryManager_OnRecipeDelivered(object sender, System.EventArgs e)
	{
		UpdateVisual();
	}

	private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
	{
		UpdateVisual();
	}

	public void UpdateVisual()
	{
		foreach(Transform child in container)
		{
			if (child == recipeTemplate) continue;
			Destroy(child.gameObject);
		}

		foreach(RecipeSO recipeSO in DeliveryManager.instance.GetWaitingRecipeSOs())
		{
			var currentRecipeTemplate = Instantiate(recipeTemplate, container);
			currentRecipeTemplate.gameObject.SetActive(true);
			currentRecipeTemplate.GetComponent<DeliveryManagerSingleUI>().SetRecipeSO(recipeSO);
		}
	}
}
