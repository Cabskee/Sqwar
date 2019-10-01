using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Constants;
using Prime31;

public class PlayerController: MonoBehaviour {
	CharacterController2D charController;

	public string playerName;
	public int playerID;
	public Color color;

	// TODO: Make these readonly
	public Constant.FacingDirection facingDirection;
	public Constant.PlayerState state;

	public GameObject carriedBlock;

	// TODO: Make these readonly
	[Header("Movement Properties")]
	public float gravity;
	public float speed;
	public float groundDamping;
	public float airDamping;
	public float jumpHeight;

	bool jumpedSinceGrounded;

	public Vector2 pickUpDistance;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
		carriedBlock.SetActive(false);
	}

	public void initialize(int playerID, string playerName, Color color) {
		this.color = color;
		this.playerID = playerID;
		this.playerName = playerName;

		applyPlayerColor();
	}

	void Start() {
		// Set as camera target
		ProCamera2D.Instance.AddCameraTarget(transform);
	}

	void OnEnable() {
		charController.onControllerCollidedEvent += boundaryTriggerEvent;
	}

	void OnDisable() {
		charController.onControllerCollidedEvent -= boundaryTriggerEvent;
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

	Vector3 playerVelocity = Vector3.zero;
	void Update() {
		if (isInState(Constant.PlayerState.Dead))
			return;

		if (charController.isGrounded) {
			jumpedSinceGrounded = false;
			playerVelocity.y = 0;
		}

		if (Input.GetButtonDown(getInputName("jump")) && (charController.isGrounded || !jumpedSinceGrounded)) {
			playerVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
			jumpedSinceGrounded = !charController.isGrounded;
		}

		if (Input.GetAxis(getInputName("vertical")) > 0.0f) { // Looking up
			facingDirection = Constant.FacingDirection.Up;
		} else if (Input.GetAxis(getInputName("horizontal")) > 0.0f || (isCarryingBlock() && facingDirection == Constant.FacingDirection.Down && charController.isGrounded)) {
			facingDirection = Constant.FacingDirection.Right;
		} else if ((Input.GetAxis(getInputName("vertical"))) < 0.0f && (!isCarryingBlock() || !charController.isGrounded)) {
			facingDirection = Constant.FacingDirection.Down;
		} else if (Input.GetAxis(getInputName("horizontal")) < 0.0f) {
			facingDirection = Constant.FacingDirection.Left;
		}

		applyRotation();

		// Pickup a nearby block or fire a carried block
		if (Input.GetButtonDown(getInputName("pickup")) || Input.GetButtonDown(getInputName("fire"))) {
			ActionButtonClicked();
		}

		// Apply horizontal speed smoothing
		float smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		playerVelocity.x = Mathf.Lerp(playerVelocity.x, Input.GetAxisRaw(getInputName("horizontal")) * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		playerVelocity.y += -gravity * Time.deltaTime;

		// TODO: If you hold Down, you should move down quicker
		// if (!charController.isGrounded && Input.GetAxis(getInputName("vertical")) < 0.0f) {
		// 	playerVelocity.y += Input.GetAxisRaw(getInputName("vertical")) * speed;
		// }

		// Update movement
		charController.move(playerVelocity * Time.deltaTime);

		playerVelocity = charController.velocity;
	}

	string getInputName(string action) {
		switch (action) {
			case "horizontal":
				return $"Horizontal{playerID}";
			case "vertical":
				return $"Vertical{playerID}";
			case "jump":
				return $"Jump{playerID}";
			case "pickup":
			case "fire":
				return $"Action{playerID}";
		}

		return "";
	}

	// PLAYER STATE FUNCTIONS

	public void killPlayer() {
		if (isInState(Constant.PlayerState.Alive)) {
			setPlayerState(Constant.PlayerState.Dead);

			// Remove a life from this player
			GameHandler.Instance.PlayerLostALife(playerID);

			Prime31.ZestKit.ActionTask.afterDelay(5f, this, task => {
				(task.context as PlayerController).respawnPlayer();
			});
		}
	}

	public void respawnPlayer() {
		// TODO: Move this player to a respawn point

		setInvulnerable();
	}

	void setInvulnerable() {
		setPlayerState(Constant.PlayerState.Invulnerable);

		GetComponent<SpriteRenderer>().enabled = false;
		Prime31.ZestKit.ActionTask.afterDelay(0.5f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = true;
		});
		Prime31.ZestKit.ActionTask.afterDelay(1f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = false;
		});
		Prime31.ZestKit.ActionTask.afterDelay(1.5f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = true;
		});
		Prime31.ZestKit.ActionTask.afterDelay(2f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = false;
		});
		Prime31.ZestKit.ActionTask.afterDelay(2.5f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = true;
		});
		Prime31.ZestKit.ActionTask.afterDelay(3f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = false;
		});
		Prime31.ZestKit.ActionTask.afterDelay(3.5f, this, task => {
			(task.context as PlayerController).GetComponent<SpriteRenderer>().enabled = true;
		});
	}

	bool isInvulnerable() => isInState(Constant.PlayerState.Invulnerable);
	bool isInState(Constant.PlayerState checkState) => state == checkState;
	bool isInStates(Constant.PlayerState[] checkStates) {
		foreach (Constant.PlayerState state in checkStates) {
			if (isInState(state))
				return true;
		}
		return false;
	}

	void setPlayerState(Constant.PlayerState newState) => state = newState;

	// HELPER FUNCTIONS

	public bool isCarryingBlock() => carriedBlock.activeSelf;

	GameObject getNearestBlockInDirection() {
		GameObject nearestBlock = null;
		Vector2 nearestDistance = Vector2.positiveInfinity;
		Vector3 playerPos = transform.position;

		foreach (GameObject block in GameObject.FindGameObjectsWithTag(Constant.TAG_PICKUPBLOCK)) {
			Vector3 blockPos = block.transform.position;

			if (facingDirection == Constant.FacingDirection.Up && blockPos.y > playerPos.y
				|| facingDirection == Constant.FacingDirection.Right && blockPos.x > playerPos.x
				|| facingDirection == Constant.FacingDirection.Down && blockPos.y < playerPos.y
				|| facingDirection == Constant.FacingDirection.Left && blockPos.x < playerPos.x
			) {
				Vector2 newDist = new Vector2(Mathf.Abs(playerPos.x - blockPos.x), Mathf.Abs(playerPos.y - blockPos.y));
				if (newDist.x <= pickUpDistance.x && newDist.y <= pickUpDistance.y && newDist.x <= nearestDistance.x && newDist.y <= nearestDistance.y) {
					nearestDistance = newDist;
					nearestBlock = block;
				}
			}
		};

		return nearestBlock;
	}

	void applyPlayerColor() {
		GetComponent<SpriteRenderer>().color = color;
		carriedBlock.GetComponent<SpriteRenderer>().color = color;
	}

	void ActionButtonClicked() {
		if (!isCarryingBlock()) { // If not carrying a block
			PickupNearestBlock(); // Attempt to pickup the nearest in direction you're facing
		} else { // If already carrying a block
			FireBlock(); // Attempt to fire it
		}
	}

	void PickupNearestBlock() {
		if (isCarryingBlock()) {
			FireBlock();
			return;
		}

		GameObject nearestBlock = getNearestBlockInDirection();
		if (nearestBlock) {
			Destroy(nearestBlock);
			ToggleCarriedBlock(true);
			return;
		}
	}

	void FireBlock() {
		if (!isCarryingBlock()) {
			PickupNearestBlock();
			return;
		}

		BlockSpawner.Instance.createShootingBlockAtLocation(carriedBlock.transform.position, facingDirection, color, playerID);
		ToggleCarriedBlock(false);
	}

	void ToggleCarriedBlock(bool toggle) => carriedBlock.SetActive(toggle);
}