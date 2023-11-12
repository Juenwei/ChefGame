using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform container;
    [SerializeField] private Transform iconTemplate;

	private void Awake()
	{
		iconTemplate.gameObject.SetActive(false);
	}

	public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        foreach(Transform child in container)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOs)
        {
            var currentIconTemplate = Instantiate(iconTemplate, container);
            currentIconTemplate.gameObject.SetActive(true);
            currentIconTemplate.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }
}
