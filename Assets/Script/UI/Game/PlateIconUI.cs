using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
	[SerializeField] private Transform iconTemplateTransform;



	private void Awake()
	{
		iconTemplateTransform.gameObject.SetActive(false);
	}

	private void Start()
	{
		plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
	}

	private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedArgs e)
	{
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		foreach(Transform child in transform)
		{
			if (child == iconTemplateTransform) continue;
			Destroy(child.gameObject);
		}

		foreach(var kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOs())
		{
			var iconImage = Instantiate(iconTemplateTransform, transform);
			iconImage.gameObject.SetActive(true);
			iconImage.GetComponent<PlateIconSingleUI>().SetIconImage(kitchenObjectSO);
		}
	}
}
