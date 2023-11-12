using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> KitchenObjectSO_GameObjects;

    void Start()
    {
		plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach(var currentKitchenObjectSO in KitchenObjectSO_GameObjects)
        {
            currentKitchenObjectSO.gameObject.SetActive(false);
        }
    }

	private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedArgs e)
	{
		foreach(var currentKitchenObjectSO in KitchenObjectSO_GameObjects)
        {
            if(e.kitchenObjectSO == currentKitchenObjectSO.kitchenObjectSO)
            {
                currentKitchenObjectSO.gameObject.SetActive(true);
            }
        }
	}
}
