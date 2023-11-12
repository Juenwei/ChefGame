using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSettingUI : MonoBehaviour
{
	public static GameSettingUI Instance { get; private set; }

	//Audio Setting
    [SerializeField] private Button soundEffectButton, musicButton, backButton;
	[SerializeField] private TextMeshProUGUI soundEffectText, musicText;

	//KeyBinding Setting
	[SerializeField] private Button moveUpButton, moveDownButton, moveLeftButton, moveRightButton;
	[SerializeField] private Button interactButton, interactAltButton, pauseButton, interactGamepadButton, interactAltGamepadButton, pauseGamepadButton;

	[SerializeField]
	private TextMeshProUGUI moveUpText, moveDownText, moveLeftText, moveRightText, interactText,
		interactAltText, pauseText, interactGamepadText, interactAltGamepadText, pauseGamepadText;

	[SerializeField] private Transform pressToRebindUITransform;

	Action OnCloseButtonAction;

	private void Awake()
	{
		Instance = this;
		soundEffectButton.onClick.AddListener(() =>
		{
			SoundEffectManager.Instance.ChangeVolume();
			UpdateVisual();
		});

		musicButton.onClick.AddListener(() =>
		{
			MusicManager.Instance.ChangeVolume();
			UpdateVisual();
		});
		backButton.onClick.AddListener(() =>
		{
			Hide();
			OnCloseButtonAction();
		});

		moveUpButton.onClick.AddListener(() =>	{RebindBinding(GameInput.Binding.Move_Up);});
		moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
		moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
		moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
		interactButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
		interactAltButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact_Alternate); });
		pauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Pause); });
		interactGamepadButton.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Gamepad_Interact);});
		interactAltButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Interact_Alternate); });
		pauseGamepadButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Interact_Alternate); });
	}

	private void Start()
	{
		GameManager.Instance.OnLocalGameResumed += GameManager_OnLocalGameResumed;
		UpdateVisual();
		Hide();
		HidePressToRebindKey();
	}

	private void GameManager_OnLocalGameResumed(object sender, System.EventArgs e)
	{
		Hide();
	}

	private void UpdateVisual()
	{
		soundEffectText.text = "Sound Effect : " + Mathf.Round(SoundEffectManager.Instance.GetVolume() * 10f);
		musicText.text = "Music : " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

		moveUpText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Up);
		moveDownText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Down);
		moveLeftText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Left);
		moveRightText.text = GameInput.instance.GetBindingText(GameInput.Binding.Move_Right);
		interactText.text = GameInput.instance.GetBindingText(GameInput.Binding.Interact);
		interactAltText.text = GameInput.instance.GetBindingText(GameInput.Binding.Interact_Alternate);
		pauseText.text = GameInput.instance.GetBindingText (GameInput.Binding.Pause);
		interactGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
		interactAltGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Interact_Alternate);
		pauseGamepadText.text = GameInput.instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
	}

	public void Show(Action OnCloseButtonAction)
	{
		this.OnCloseButtonAction = OnCloseButtonAction;
		gameObject.SetActive(true);
		soundEffectButton.Select();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void ShowPressToRebindKey()
	{
		pressToRebindUITransform.gameObject.SetActive(true);
	}

	public void HidePressToRebindKey()
	{
		pressToRebindUITransform.gameObject.SetActive(false);
	}

	private void RebindBinding(GameInput.Binding binding)
	{
		ShowPressToRebindKey();
		GameInput.instance.RebindBinding(binding, ()=> {
			HidePressToRebindKey();
			UpdateVisual();
		});
	}
}