using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class PlayerController: NetworkBehaviour {
	CharacterController2D charController;

	[SyncVar] public Color color;
	[SyncVar] public int livesLeft;
	[SyncVar] public Constant.FacingDirection facingDirection;
	[SyncVar] public Constant.PlayerState state;

	public GameObject carriedBlock;

	// TODO: Eventually make these readonly
	[Header("Movement Properties")]
	public float gravity;
	public float speed;
	public float groundDamping;
	public float airDamping;
	public float jumpHeight;

	[SyncVar] bool jumpedSinceGrounded;

	public Vector2 pickUpDistance;

	Vector3 clientVelocity;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	public override void OnStartServer() {
		livesLeft = GameHandler.Instance.startingLives;

		setInvulnerable();
	}

	void Start() {
		if (isLocalPlayer) {
			ProCamera2D.Instance.AddCameraTarget(transform);

			color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			CmdRequestToSetPlayerColor(color);
		}

		applyPlayerColor();

		carriedBlock.SetActive(false);

		GameHandler.Instance.addPlayer(this);
	}

	void Update() {
		if (!isLocalPlayer) {
			if (facingDirection == Constant.FacingDirection.Left) {
				transform.localScale = new Vector3(-1, 1, 1);
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			}
			return;
		}

		if (charController.isGrounded) {
			jumpedSinceGrounded = false;
			clientVelocity.y = 0;
		}

		if (Input.GetKeyDown(KeyCode.Space) && (charController.isGrounded || !jumpedSinceGrounded)) {
			clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);

			if (!charController.isGrounded) {
				jumpedSinceGrounded = true;
			}
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			facingDirection = Constant.FacingDirection.Up;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
		}  else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (facingDirection == Constant.FacingDirection.Down && charController.isGrounded)) {
			facingDirection = Constant.FacingDirection.Right;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
		} else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && !charController.isGrounded) {
			facingDirection = Constant.FacingDirection.Down;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
		} else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			facingDirection = Constant.FacingDirection.Left;
			transform.localScale = new Vector3(-1, 1, 1);
			transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
		}

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
		clientVelocity.x = Mathf.Lerp(clientVelocity.x, Input.GetAxisRaw("Horizontal") * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		clientVelocity.y += -gravity * Time.deltaTime;

		// Update movement
		charController.move(clientVelocity * Time.deltaTime);

		// Update latest clientVelocity
		clientVelocity = charController.velocity;
	}

	// PLAYER STATE FUNCTIONS

	[Server]
	public void killPlayer() {
		if (!isInvulnerable()) {
			setPlayerState(Constant.PlayerState.Dead);

			// TODO: Other stuff
		}
	}

	[Server]
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

	[Server]
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
	void CmdRequestToSetPlayerColor(Color playerColor) {
		color = playerColor;
		applyPlayerColor();
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