using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
	private const string FAIL_MSG = "DELIVERY\nFAIL", SUCCESS_MSG = "DELIVERY\nSUCCESS";
	private const string POP_UP_TRIGGER = "PopUp";
	
	[SerializeField] private Image backgroundImage, iconImage;
    [SerializeField] private TextMeshProUGUI messageText;
	[SerializeField] private Color successColor, failColor;
	[SerializeField] private Sprite sucessSprite, failSprite;

	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		DeliveryManager.instance.OnRecipeSuccessed += DeliveryManager_OnRecipeSuccessed;
		DeliveryManager.instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
		gameObject.SetActive(false);
	}

	private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
	{
		gameObject.SetActive(true);
		backgroundImage.color = failColor;
		messageText.text = FAIL_MSG;
		iconImage.sprite = failSprite;
		animator.SetTrigger(POP_UP_TRIGGER);
	}

	private void DeliveryManager_OnRecipeSuccessed(object sender, System.EventArgs e)
	{
		gameObject.SetActive(true);
		backgroundImage.color =successColor;
		messageText.text = SUCCESS_MSG;
		iconImage.sprite = sucessSprite;
		animator.SetTrigger(POP_UP_TRIGGER);
	}
}
