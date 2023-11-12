using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameInput;

public class GameInput : MonoBehaviour
{

	private const string PLAYER_PREFS_BINDING = "InputBindings";
	public static GameInput instance;
	
	public event EventHandler OnInteractAction;
	public event EventHandler OnInteractAlternateAction;
	public event EventHandler OnPauseTrigger;
	public event EventHandler OnBindingRebind;
	
	private PlayerInputAction playerInputAction;

	public enum Binding
	{
		Move_Up,
		Move_Down,
		Move_Left,
		Move_Right,
		Interact,
		Interact_Alternate,
		Pause,
		Gamepad_Interact,
		Gamepad_Interact_Alternate,
		Gamepad_Pause,
	}
	private void Awake()
	{
		instance = this;
		//Need to exceute before enable player input action
		if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDING))
		{
			playerInputAction.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDING));
		}
		playerInputAction = new PlayerInputAction();
		playerInputAction.Player.Enable();

		playerInputAction.Player.Interact.performed += Interact_performed;
		playerInputAction.Player.Interact_Alternate.performed += Interact_Alternate_performed;
		playerInputAction.Player.Pause.performed += Pause_performed;


	}

	private void OnDestroy()
	{
		playerInputAction.Player.Interact.performed -= Interact_performed;
		playerInputAction.Player.Interact_Alternate.performed -= Interact_Alternate_performed;
		playerInputAction.Player.Pause.performed -= Pause_performed;

		playerInputAction.Dispose();
	}

	private void Pause_performed(InputAction.CallbackContext obj)
	{
		OnPauseTrigger?.Invoke(this, EventArgs.Empty);
	}

	private void Interact_Alternate_performed(InputAction.CallbackContext context)
	{
		OnInteractAlternateAction?.Invoke(this,EventArgs.Empty);
	}

	private void Interact_performed(InputAction.CallbackContext context)
	{
		OnInteractAction?.Invoke(this,EventArgs.Empty);
	}

	public Vector2 GetNormalizedMovementVector()
	{
		//input Logic Layer, Leave it using Vector 2 since input only have 2 axis
		var inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();

		return inputVector.normalized;
	}

	public string GetBindingText(Binding binding)
	{
		//Move is more complex due to it have 4 binding in one input profile, the rest profile on have only one binding
		switch(binding)
		{
			default :
			case Binding.Move_Up:
				return playerInputAction.Player.Move.bindings[1].ToDisplayString();
			case Binding.Move_Down:
				return playerInputAction.Player.Move.bindings[2].ToDisplayString();
			case Binding.Move_Left:
				return playerInputAction.Player.Move.bindings[3].ToDisplayString();
			case Binding.Move_Right:
				return playerInputAction.Player.Move.bindings[4].ToDisplayString();

			case Binding.Interact:
				return playerInputAction.Player.Interact.bindings[0].ToDisplayString();
			case Binding.Interact_Alternate:
				return playerInputAction.Player.Interact_Alternate.bindings[0].ToDisplayString();
			case Binding.Pause:
				return playerInputAction.Player.Pause.bindings[0].ToDisplayString();
			case Binding.Gamepad_Interact:
				return playerInputAction.Player.Interact.bindings[1].ToDisplayString();
			case Binding.Gamepad_Interact_Alternate:
				return playerInputAction.Player.Interact_Alternate.bindings[1].ToDisplayString();
			case Binding.Gamepad_Pause:
				return playerInputAction.Player.Pause.bindings[1].ToDisplayString();
		}
	}

	public void RebindBinding(Binding bingdingType, Action onActionRebound)
	{
		playerInputAction.Player.Disable();
		InputAction inputAction;
		int bindingIndex;
		switch (bingdingType)
		{
			default:
			case GameInput.Binding.Move_Up:
				inputAction = playerInputAction.Player.Move;
				bindingIndex = 1;
				break;
			case GameInput.Binding.Move_Down:
				inputAction = playerInputAction.Player.Move;
				bindingIndex = 2;
				break;
			case GameInput.Binding.Move_Left:
				inputAction = playerInputAction.Player.Move;
				bindingIndex = 3;
				break;
			case GameInput.Binding.Move_Right:
				inputAction = playerInputAction.Player.Move;
				bindingIndex = 4;
				break;
			case GameInput.Binding.Interact:
				inputAction = playerInputAction.Player.Interact;
				bindingIndex = 0;
				break;
			case GameInput.Binding.Interact_Alternate:
				inputAction = playerInputAction.Player.Interact_Alternate;
				bindingIndex = 0;
				break;
			case GameInput.Binding.Pause:
				inputAction = playerInputAction.Player.Pause;
				bindingIndex = 0;
				break;
			case GameInput.Binding.Gamepad_Interact:
				inputAction = playerInputAction.Player.Interact;
				bindingIndex = 1;
				break;
			case GameInput.Binding.Gamepad_Interact_Alternate:
				inputAction = playerInputAction.Player.Interact_Alternate;
				bindingIndex = 1;
				break;
			case GameInput.Binding.Gamepad_Pause:
				inputAction = playerInputAction.Player.Pause;
				bindingIndex = 1;
				break;
		}
		inputAction.PerformInteractiveRebinding(bindingIndex).
			OnComplete(callback =>
			{
				//Debug.Log(callback.action.bindings[1].path);
				//Debug.Log(callback.action.bindings[1].overridePath);
				callback.Dispose();
				playerInputAction.Player.Enable();
				onActionRebound();

				PlayerPrefs.SetString(PLAYER_PREFS_BINDING, playerInputAction.SaveBindingOverridesAsJson());
				PlayerPrefs.Save();

				OnBindingRebind?.Invoke(this, EventArgs.Empty);
			}).Start();
	}
}
