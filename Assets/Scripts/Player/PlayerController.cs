using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class PlayerController: NetworkBehaviour {
	CharacterController2D charController;

	[SyncVar]
	public Color color;
	[SyncVar]
	public int livesLeft;
	[SyncVar]
	public Constant.FacingDirection facingDirection;

	public GameObject carriedBlock;

	// Movement Variables
	public float gravity;
	public float speed;
	public float groundDamping;
	public float airDamping;
	public float jumpHeight;

	bool jumpedSinceGrounded;
	float normalizedHorizontal;

	public Vector2 pickUpDistance;

	Vector2 clientInput;
	Vector3 clientVelocity;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	public override void OnStartServer() {
		livesLeft = GameHandler.Instance.startingLives;
	}

	void Start() {
		if (isLocalPlayer) {
			ProCamera2D.Instance.AddCameraTarget(transform);
		}

		carriedBlock.SetActive(false);

		// TODO: Use color of this player they set
		color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		GetComponent<SpriteRenderer>().color = color;
		carriedBlock.GetComponent<SpriteRenderer>().color = color;
	}

	void Update() {
		if (charController.isGrounded) {
			jumpedSinceGrounded = false;
			clientVelocity.y = 0;
		}

		if (isClient) {
			clientInput = Vector2.zero;
		}

		if (isLocalPlayer) { // Owner Movement & Actions
			normalizedHorizontal = Input.GetAxisRaw("Horizontal");
			clientInput.x = normalizedHorizontal;

			if (Input.GetKeyDown(KeyCode.Space) && (charController.isGrounded || !jumpedSinceGrounded)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
				jumpedSinceGrounded = true;
				clientInput.y = 1f;
			}

			if (Input.GetKey(KeyCode.W)) {
				facingDirection = Constant.FacingDirection.Up;
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
			} else if (Input.GetKey(KeyCode.A)) {
				facingDirection = Constant.FacingDirection.Left;
				transform.localScale = new Vector3(-1, 1, 1);
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			} else if (Input.GetKey(KeyCode.S)) {
				facingDirection = Constant.FacingDirection.Down;
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
			} else if (Input.GetKey(KeyCode.D)) {
				facingDirection = Constant.FacingDirection.Right;
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			}

			// Pickup a nearby block or fire a carried block
			if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.LeftShift)) {
				if (!isCarryingBlock()) { // Pick up the closest Block
					if (findNearestFallenBlock()) {
						// Tell the server you picked up this block and pick it up clientside
						CmdRequestToPickupBlock();
						carriedBlock.SetActive(true);
					}
				} else { // Fire a Carried Block
					CmdRequestToFireBlock(facingDirection);
				}
			}
		} else if (isServer) { // Server Movement (Using Input from Client)
			normalizedHorizontal = clientInput.x;
			if (clientInput.y == 1f && (charController.isGrounded || !jumpedSinceGrounded)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
				jumpedSinceGrounded = true;
			}
		}

		// Apply horizontal speed smoothing
		float smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		clientVelocity.x = Mathf.Lerp(clientVelocity.x, normalizedHorizontal * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		clientVelocity.y += -gravity * Time.deltaTime;

		// Update movement
		charController.move(clientVelocity * Time.deltaTime);

		// Update latest clientVelocity
		clientVelocity = charController.velocity;
	}

	// HELPER FUNCTIONS

	public bool isCarryingBlock() {
		return carriedBlock.activeSelf;
	}

	GameObject findNearestFallenBlock() {
		GameObject nearestBlock = null;
		Vector2 nearestDistance = Vector2.positiveInfinity;
		foreach (GameObject block in GameObject.FindGameObjectsWithTag("Falling Block")) {
			Vector2 newDist = new Vector2(Mathf.Abs(transform.position.x - block.transform.position.x), Mathf.Abs(transform.position.y - block.transform.position.y));
			if (newDist.x <= pickUpDistance.x && newDist.y <= pickUpDistance.y && newDist.x <= nearestDistance.x && newDist.y <= nearestDistance.y) {
				nearestDistance = newDist;
				nearestBlock = block;
			}
		};
		return nearestBlock;
	}

	// PICKING UP BLOCK

	[Command]
	void CmdRequestToPickupBlock() {
		GameObject nearestBlock = null;
		if (!isCarryingBlock()) {
			nearestBlock = findNearestFallenBlock();
			if (nearestBlock) {
				NetworkServer.Destroy(nearestBlock);
				carriedBlock.SetActive(true);
				RpcPickedUpBlock(true);
			}
		}
	}

	[ClientRpc]
	void RpcPickedUpBlock(bool pickedUp) {
		if (isServer)
			return;

		carriedBlock.SetActive(pickedUp);
	}

	// THROWING BLOCK

	[Command]
	void CmdRequestToFireBlock(Constant.FacingDirection facingDirection) {
		if (isCarryingBlock()) {
			BlockSpawner.Instance.createShootingBlockAtLocation(carriedBlock.transform.position, facingDirection, color);

			carriedBlock.SetActive(false);
			RpcFiredBlock();
		}
	}

	[ClientRpc]
	void RpcFiredBlock() {
		if (isServer)
			return;

		carriedBlock.SetActive(false);
	}
}