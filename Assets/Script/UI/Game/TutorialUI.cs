using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveLeftKeyText, moveRightKeyText, moveUpKeyText, moveDownKeyText;
    [SerializeField] private TextMeshProUGUI interactKeyText, altKeyText, pauseKeyText;

    [SerializeField] private TextMeshProUGUI interactGamepadText, altGamepadText , pauseGamepadText;

	private void Start()
	{
		GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
		GameInput.instance.OnBindingRebind += GameInput_OnBindingRebind;

		UpdateVisual();
	}

	private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
	{
		if (GameManager.Instance.IsLocalPlayerReady())
		{
			Hide();
		}
	}

	private void GameInput_OnBindingRebind(object sender, System.EventArgs e)
	{
		UpdateVisual();
	}

	private void UpdateVisual()
    {
		moveUpKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Up);
		moveDownKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Down);
		moveLeftKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Left);
		moveRightKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Right);
		interactKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Interact);
		altKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Interact_Alternate);
		pauseKeyText.text = GameInput.instance.GetBindingText(GameInput.Binding.Pause);
		interactGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
		altGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Interact_Alternate);
		pauseGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
	}

	private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}
}
