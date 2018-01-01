using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class PlayerController: NetworkBehaviour {
	CharacterController2D charController;

	[SyncVar] public string playerName;
	[SyncVar] public Color color;
	[SyncVar] public int livesLeft;
	[SyncVar] public Constant.FacingDirection facingDirection;
	[SyncVar] public Constant.PlayerState state;

	public GameObject carriedBlock;

	// TODO: Make these readonly
	[Header("Movement Properties")]
	public float gravity;
	public float speed;
	public float groundDamping;
	public float airDamping;
	public float jumpHeight;
	Vector3 playerVelocity;

	[SyncVar] bool jumpedSinceGrounded;

	public Vector2 pickUpDistance;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	public override void OnStartServer() {
		livesLeft = GameHandler.Instance.startingLives;

		setInvulnerable();
	}

	public override void OnStartLocalPlayer() {
		color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		playerName = "Player "+Random.Range(0, 250);
		// TODO:
		// Eventually send CmdRequestToSetPlayerColor() with the player's selected color and move all this server
		// initialization stuff into its own callback that waits for all this shit or something like that

		// Send this player's initializations
		CmdSendLocalPlayerInitializations(playerName, color);

		// Set camera follow
		ProCamera2D.Instance.AddCameraTarget(transform);
	}

	void Start() {
		if (isLocalPlayer) {
			Prime31.ZestKit.ActionTask.every(0.033f, this, task => {
				(task.context as PlayerController).CmdSendPositionToServer(transform.position);
			});
		}

		carriedBlock.SetActive(false);

		applyPlayerColor();

		if (isServer)
			charController.onControllerCollidedEvent += boundaryTriggerEvent;
	}

	void boundaryTriggerEvent(RaycastHit2D ray) {
		if (didCollideWithLayer(ray, Constant.LAYER_BOUNDARY)) {
			killPlayer();
		}
	}

	public bool didCollideWithLayer(RaycastHit2D ray, string layerName) {
		return ray.transform.gameObject.layer == LayerMask.NameToLayer(layerName);
	}

	void applyRotation() {
		switch(facingDirection) {
			case Constant.FacingDirection.Up:
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
				break;
			case Constant.FacingDirection.Right:
			default:
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
				break;
			case Constant.FacingDirection.Down:
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
				break;
			case Constant.FacingDirection.Left:
				transform.localScale = new Vector3(-1, 1, 1);
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
				break;
		}
	}

	void Update() {
		if (!isLocalPlayer) {
			applyRotation();

			return;
		}

		if (isInState(Constant.PlayerState.Dead))
			return;

		if (charController.isGrounded) {
			jumpedSinceGrounded = false;
			playerVelocity.y = 0;
		}

		if (Input.GetKeyDown(KeyCode.Space) && (charController.isGrounded || !jumpedSinceGrounded)) {
			playerVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
			jumpedSinceGrounded = true;
		}

		Constant.FacingDirection oldDirection = facingDirection;
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			facingDirection = Constant.FacingDirection.Up;
		}  else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (isCarryingBlock() && facingDirection == Constant.FacingDirection.Down && charController.isGrounded)) {
			facingDirection = Constant.FacingDirection.Right;
		} else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && (!isCarryingBlock() || !charController.isGrounded)) {
			facingDirection = Constant.FacingDirection.Down;
		} else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			facingDirection = Constant.FacingDirection.Left;
		}

		applyRotation();

		// Only send new direction if it has changed
		if (facingDirection != oldDirection)
			CmdSendRotationToServer(facingDirection);

		// Pickup a nearby block or fire a carried block
		if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.LeftShift)) {
			if (!isCarryingBlock()) { // Pick up the closest Block
				if (findNearestBlockToPickup()) {
					// Tell the server you picked up this block and pick it up clientside
					CmdRequestToPickupBlock();
					carriedBlock.SetActive(true);
				}
			} else { // Fire a Carried Block
				CmdRequestToFireBlock(facingDirection);
			}
		}

		// Apply horizontal speed smoothing
		float smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		playerVelocity.x = Mathf.Lerp(playerVelocity.x, Input.GetAxisRaw("Horizontal") * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		playerVelocity.y += -gravity * Time.deltaTime;

		// Update movement
		charController.move(playerVelocity * Time.deltaTime);

		// Update latest clientVelocity
		playerVelocity = charController.velocity;
	}

	[Command]
	void CmdSendRotationToServer(Constant.FacingDirection newDirection) {
		facingDirection = newDirection;
	}

	[Command]
	void CmdSendPositionToServer(Vector3 newPosition) {
		transform.position = newPosition;

		RpcSendPositionToClient(newPosition);
	}

	[ClientRpc]
	void RpcSendPositionToClient(Vector3 newPosition) {
		if (isLocalPlayer) {
			if (Mathf.Abs(transform.position.x - newPosition.x) >= 2f || Mathf.Abs(transform.position.y - newPosition.y) >= 4f) {
				transform.position = newPosition; // Rubberband
			}
		} else {
			transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref playerVelocity, Time.deltaTime*1f);
		}
	}

	// PLAYER STATE FUNCTIONS

	[ServerCallback]
	public void killPlayer() {
		if (isInState(Constant.PlayerState.Alive)) {
			setPlayerState(Constant.PlayerState.Dead);

			livesLeft -= 1;

			Prime31.ZestKit.ActionTask.afterDelay(5f, this, task => {
				(task.context as PlayerController).respawnPlayer();
			});

			// TODO: Update scoreboard
		}
	}

	[ServerCallback]
	public void respawnPlayer() {
		// TODO: Move this player to a respawn point

		setInvulnerable();
	}

	[ServerCallback]
	void setInvulnerable() {
		setPlayerState(Constant.PlayerState.Invulnerable);

		Prime31.ZestKit.ActionTask.afterDelay(3f, this, task => {
			(task.context as PlayerController).setPlayerState(Constant.PlayerState.Alive);
		});
	}

	bool isInvulnerable() {
		return isInState(Constant.PlayerState.Invulnerable);
	}
	bool isInStates(Constant.PlayerState[] checkStates) {
		foreach (Constant.PlayerState state in checkStates) {
			if (isInState(state))
				return true;
		}
		return false;
	}
	bool isInState(Constant.PlayerState checkState) {
		return state == checkState;
	}

	[ServerCallback]
	void setPlayerState(Constant.PlayerState newState) {
		state = newState;
	}

	// HELPER FUNCTIONS

	public bool isCarryingBlock() {
		return carriedBlock.activeSelf;
	}

	GameObject findNearestBlockToPickup() {
		GameObject nearestBlock = null;
		Vector2 nearestDistance = Vector2.positiveInfinity;
		foreach (GameObject block in GameObject.FindGameObjectsWithTag(Constant.TAG_PICKUPBLOCK)) {
			Vector2 newDist = new Vector2(Mathf.Abs(transform.position.x - block.transform.position.x), Mathf.Abs(transform.position.y - block.transform.position.y));
			if (newDist.x <= pickUpDistance.x && newDist.y <= pickUpDistance.y && newDist.x <= nearestDistance.x && newDist.y <= nearestDistance.y) {
				nearestDistance = newDist;
				nearestBlock = block;
			}
		};
		return nearestBlock;
	}

	[Command]
	void CmdSendLocalPlayerInitializations(string newName, Color newColor) {
		playerName = newName;
		color = newColor;

		applyPlayerColor();

		GameHandler.Instance.addPlayer(this);
	}

	void applyPlayerColor() {
		GetComponent<SpriteRenderer>().color = color;
		carriedBlock.GetComponent<SpriteRenderer>().color = color;
	}

	// PICKING UP BLOCK

	[Command]
	void CmdRequestToPickupBlock() {
		GameObject nearestBlock = null;
		if (!isCarryingBlock()) {
			nearestBlock = findNearestBlockToPickup();
			if (nearestBlock) {
				NetworkServer.Destroy(nearestBlock);
				carriedBlock.SetActive(true);
				RpcToggleCarriedBlock(true);
				return;
			}
		}
		RpcToggleCarriedBlock(false);
	}

	// THROWING BLOCK

	[Command]
	[ServerCallback]
	void CmdRequestToFireBlock(Constant.FacingDirection facingDirection) {
		if (isCarryingBlock()) {
			BlockSpawner.Instance.createShootingBlockAtLocation(carriedBlock.transform.position, facingDirection, color, GetComponent<NetworkIdentity>());

			carriedBlock.SetActive(false);
			RpcToggleCarriedBlock(false);
		} else {
			CmdRequestToPickupBlock();
		}
	}

	[ClientRpc]
	[ClientCallback]
	void RpcToggleCarriedBlock(bool toggle) {
		carriedBlock.SetActive(toggle);
	}
}