using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
	public static Player LocalInstance { get; private set; }
	public static event EventHandler OnAnyPlayerSpawn;
	public static event EventHandler OnAnyPlayerPickUpSomething;

	public static void ResetStaticData()
	{
		OnAnyPlayerSpawn = null;
		OnAnyPlayerPickUpSomething = null;
	}
	public event EventHandler<OnSelectedCounterChangedArgs> OnSelectedCounterChanged;
	public event EventHandler OnPickUp;
	public class OnSelectedCounterChangedArgs : EventArgs
	{
		public BaseCounter selectedCounter;
	}


	[SerializeField] private float moveSpeed = 7f, rotateSpeed = 10f, playerRadius = .7f
		, interactDistance = 2f;
	[SerializeField] private LayerMask counterLayerMask, collisionLayerMask;
	[SerializeField] private Transform kitchenObjectHoldPoint;
	[SerializeField] private List<Vector3> playerSpawnPositions;
	[SerializeField] private PlayerVisual playerVisual;

	private Vector2 inputVector;
	private Vector3 lastInteractDirection;

	private BaseCounter selectedCounter;
	private KitchenObject currentKitchenObject;

	public bool IsWalking { get; private set;}

	private void Start()
	{
		GameInput.instance.OnInteractAction += GameInput_OnInteractAction;
		GameInput.instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

		//Initialize Player Color Based on the Network Client ID
		var playerData = KitchenMultiplayerManager.Instance.GetPlayerDataByClientId(OwnerClientId);

		playerVisual.SetPlayerColor(KitchenMultiplayerManager.Instance.GetPlayerColor(playerData.colorId));
	}

	private void Update()
    {
		if (!IsOwner) return;
		HandleMovement();
		HandleCounterDetection();
		
	}

	public override void OnNetworkSpawn()
	{
		if (IsOwner)
		{
			LocalInstance = this;
		}

		transform.position = playerSpawnPositions[KitchenMultiplayerManager.Instance.GetPlayerDataIndexByClientId(OwnerClientId)];

		OnAnyPlayerSpawn?.Invoke(this, EventArgs.Empty);

		//Add a disconnect listener to the player script that run on server. 
		if (IsServer)
		{
			NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
		}

	}

	//Desc : The function broadcast to all player while the any player disconnect.
	private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
	{
		//If Current Player is the player who quit game and hold a kitchenObject
		if(clientId == OwnerClientId && HasKitchenObject())
		{
			KitchenObject.DestroyKitchenObject(GetKitchenObject());
		}
	}

	private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
	{
		if (!GameManager.Instance.IsGamePlaying())
			return;
		if (selectedCounter != null)
		{
			selectedCounter.InteractAlternate(this);
		}
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e)
	{
		if (!GameManager.Instance.IsGamePlaying())
			return;
		if (selectedCounter != null)
		{
			selectedCounter.Interact(this);
		}
	}

	//A region of code that handle the boolean logic that related with interaction (Logic runs in Update)
	private void HandleCounterDetection()
	{
		/* 1. Do Raycast detection based on Current Face Direction
		   2. Get The BaseCounter from the collided object (100% Get the class due to the standarized counter)
		   3. Fire Event to all counter with param to locate selected counter. (To Activate visual)
		 */

		inputVector = GameInput.instance.GetNormalizedMovementVector();
		var moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

		if (moveDirection != Vector3.zero)
		{
			lastInteractDirection = moveDirection;
		}

		if(Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance, counterLayerMask))
		{
			if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
			{
				//Has Counter
				if(this.selectedCounter != baseCounter)
				{
					SetSelectedCounter(baseCounter);
				}
			}
			else
			{
				//Raycast hit something other than counter
				SetSelectedCounter(null);
			}
		}
		else
		{
			//Raycast Doesn't hit anything
			SetSelectedCounter(null);
		}
	
	}

	private void HandleMovement()
	{
		inputVector = GameInput.instance.GetNormalizedMovementVector();
		var moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

		//Assign for Animator
		IsWalking = moveDirection != Vector3.zero;

		var moveDistance = moveSpeed * Time.deltaTime;

		//Assign for Colliding
		var canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirection,
				Quaternion.identity, moveDistance, collisionLayerMask);

		if (!canMove)
		{
			//Attemp X axis movement
			var moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
			canMove = (moveDirectionX.x < -0.5f || moveDirectionX.x > +0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionX,
				Quaternion.identity, moveDistance,collisionLayerMask);

			//If obstacle on front, move to the left or right
			if (canMove)
			{
				//Elminate other vector value
				moveDirection = moveDirectionX;
			}
			else
			{
				//Attemp Z axis movement
				var moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
				canMove = (moveDirectionZ.z < -0.5f || moveDirectionZ.z > +0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionZ,
				Quaternion.identity, moveDistance,collisionLayerMask);

				if (canMove)
					moveDirection = moveDirectionZ;
				else
				{
					//Cannot move in any direction
				}
			}
		}
		//Assign for Move Position
		if (canMove)
			transform.position += moveDirection * moveDistance;

		transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
	}

	private void SetSelectedCounter(BaseCounter selectedCounter)
	{
		this.selectedCounter = selectedCounter;
		//Fire Event
		OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedArgs { selectedCounter = selectedCounter });
	}

	public Transform GetKitchenObjectParentFollowTransform()
	{
		return kitchenObjectHoldPoint;
	}

	public KitchenObject GetKitchenObject()
	{
		return currentKitchenObject;
	}

	public void SetKitchenObject(KitchenObject kitchenObject)
	{
		currentKitchenObject = kitchenObject;
		if (selectedCounter != null)
		{
			OnPickUp?.Invoke(this, EventArgs.Empty);
			OnAnyPlayerPickUpSomething?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ClearKitchenObject()
	{
		currentKitchenObject = null;
	}

	public bool HasKitchenObject()
	{
		return currentKitchenObject != null;
	}

	public NetworkObject GetNetworkObject()
	{
		return NetworkObject;
	}



	#region Server AUTH
	//private void HandleMovementServerAuth()
	//{
	//	inputVector = GameInput.instance.GetNormalizedMovementVector();
	//	HandleMovementServerRpc(inputVector);
	//}

	//[ServerRpc(RequireOwnership = false)]
	//private void HandleMovementServerRpc(Vector2 inputVector)
	//{
	//	var moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

	//	//Assign for Animator
	//	IsWalking = moveDirection != Vector3.zero;

	//	var moveDistance = moveSpeed * Time.deltaTime;

	//	//Assign for Colliding
	//	var canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight
	//		, playerRadius, moveDirection, playerRadius);

	//	if (!canMove)
	//	{
	//		//Attemp X axis movement
	//		var moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
	//		canMove = (moveDirectionX.x < -5f || moveDirectionX.x > +5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
	//			playerRadius, moveDirectionX, playerRadius);

	//		//If obstacle on front, move to the left or right
	//		if (canMove)
	//		{
	//			//Elminate other vector value
	//			moveDirection = moveDirectionX;
	//		}
	//		else
	//		{
	//			//Attemp Z axis movement
	//			var moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
	//			canMove = (moveDirectionZ.z < -5f || moveDirectionZ.z > +5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
	//				playerRadius, moveDirectionZ, playerRadius);

	//			if (canMove)
	//				moveDirection = moveDirectionZ;
	//			else
	//			{
	//				//Cannot move in any direction
	//			}
	//		}
	//	}
	//	//Assign for Move Position
	//	if (canMove)
	//		transform.position += moveDirection * moveDistance;

	//	transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
	//}

	#endregion
}


