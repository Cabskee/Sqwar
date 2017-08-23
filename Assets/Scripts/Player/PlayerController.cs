using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Constants;
using Prime31;

public class PlayerController: MonoBehaviour {
	CharacterController2D charController;

	public Color color;
	public int livesLeft;

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

	Vector3 serverPosition;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	void OnEnable() {
		charController.onTriggerEnterEvent += onTriggerEnter;
	}

	void onTriggerEnter(Collider2D other) {
		Debug.Log("OnTriggerEnter");
	}
	void OnTriggerEnter2D(Collider2D other) {
		Debug.Log("OnTriggerEnter2D");
	}

	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
		livesLeft = info.networkView.initialData.Read<int>();
	}

	void Start() {
		if (uLink.NetworkView.Get(this).isMine) {
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

		if (uLink.Network.isClient)
			clientInput = Vector2.zero;

		if (uLink.NetworkView.Get(this).isMine) { // Owner Movement & Actions
			normalizedHorizontal = Input.GetAxisRaw("Horizontal");
			clientInput.x = normalizedHorizontal;

			if (Input.GetKeyDown(KeyCode.Space) && (charController.isGrounded || !jumpedSinceGrounded)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
				jumpedSinceGrounded = true;
				clientInput.y = 1f;
			}

			// Rubberband player back to Server Position if he moves too far
			if (Mathf.Abs(transform.position.x - serverPosition.x) >= (speed*0.5f)) {
				transform.position = new Vector3(serverPosition.x, transform.position.y, 0);
			}
			if (Mathf.Abs(transform.position.y - serverPosition.y) >= (jumpHeight*0.9f)) {
				transform.position = new Vector3(transform.position.x, serverPosition.y, 0);
			}

			// Pickup a nearby block or fire a carried block
			if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.LeftShift)) {
				if (!isCarryingBlock()) { // Pick up the closest Block
					GameObject pickUpBlock = findNearestFallenBlock();
					if (pickUpBlock) {
						// Tell the server you picked up this block and pick it up clientside
						uLink.NetworkView.Get(this).RPC("receiveClientPickUpInput", uLink.RPCMode.Server);
						carriedBlock.SetActive(true);
					}
				} else { // Fire a Carried Block
					Constant.FacingDirection facingDirection = Constant.FacingDirection.Right;
					if (Input.GetKey(KeyCode.W)) {
						facingDirection = Constant.FacingDirection.Up;
					} else if (Input.GetKey(KeyCode.S)) {
						facingDirection = Constant.FacingDirection.Down;
					} else if ((int)transform.localScale.x != 1) {
						facingDirection = Constant.FacingDirection.Left;
					}
					uLink.NetworkView.Get(this).RPC("clientRequestsToFireBlock", uLink.RPCMode.Server, facingDirection);
				}
			}
		} else if (uLink.Network.isServer) { // Server Movement (Using Input from Client)
			normalizedHorizontal = clientInput.x;
			if (clientInput.y == 1f && (charController.isGrounded || !jumpedSinceGrounded)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
				jumpedSinceGrounded = true;
			}
		} else if (uLink.Network.isClient) { // Proxy Movement (Carbon copy of Server movement)
			float proxyScale = (serverPosition.x > transform.position.x) ? 1 : -1;
			transform.localScale = new Vector3(proxyScale, 1, 1);
			transform.position = serverPosition;
			return;
		}

		// Apply horizontal speed smoothing
		float smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		clientVelocity.x = Mathf.Lerp(clientVelocity.x, normalizedHorizontal * speed, Time.deltaTime*smoothedMovement);

		// Face the player left or right based on movement
		if (normalizedHorizontal != 0f)
			transform.localScale = new Vector3(normalizedHorizontal, 1, 1);

		// Apply downward gravity
		clientVelocity.y += -gravity * Time.deltaTime;

		// Update movement
		charController.move(clientVelocity * Time.deltaTime);

		// Update latest clientVelocity
		clientVelocity = charController.velocity;
	}

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

	void FixedUpdate() {
		// Owner sends current Input to the Server every FixedUpdate
		if (uLink.NetworkView.Get(this).isMine) {
			uLink.NetworkView.Get(this).UnreliableRPC("receiveClientMovementInput", uLink.RPCMode.Server, clientInput);
		}

		// Server sends current Position to every Client every FixedUpdate
		if (uLink.Network.isServer) {
			uLink.NetworkView.Get(this).UnreliableRPC("receivePositionFromServer", uLink.RPCMode.All, transform.position);
		}
	}

	// THROWING BLOCK

	[RPC]
	void clientRequestsToFireBlock(Constant.FacingDirection facingDirection) {
		if (uLink.Network.isServer && isCarryingBlock()) {
			uLink.NetworkViewID blockViewID = uLink.Network.AllocateViewID(uLink.Network.player);
			BlockSpawner.Instance.createShootingBlockAtLocation(carriedBlock.transform.position, facingDirection, color, blockViewID);
			uLink.NetworkView.Get(this).RPC("clientFiredBlock", uLink.RPCMode.Others, carriedBlock.transform.position, facingDirection, color, blockViewID);
			carriedBlock.SetActive(false);
		}
	}

	[RPC]
	void clientFiredBlock(Vector3 spawnPos, Constant.FacingDirection facingDirection, Color playerColor, uLink.NetworkViewID viewID) {
		BlockSpawner.Instance.createShootingBlockAtLocation(spawnPos, facingDirection, playerColor, viewID);
		carriedBlock.SetActive(false);
	}

	// PICKING UP BLOCK

	[RPC]
	void receiveClientPickUpInput() {
		if (uLink.Network.isServer) {
			GameObject nearestBlock = findNearestFallenBlock();
			if (nearestBlock && !isCarryingBlock()) {
				uLink.Network.Destroy(nearestBlock.GetComponent<uLink.NetworkView>());
			}
			uLink.NetworkView.Get(this).RPC("receivePickUpResponseFromServer", uLink.RPCMode.All, (nearestBlock && !isCarryingBlock()));
		}
	}

	[RPC]
	void receivePickUpResponseFromServer(bool pickedUp) {
		carriedBlock.SetActive(pickedUp);
	}

	// MOVEMENT

	// Server receiving X/Y movement input from a Client
	[RPC]
	void receiveClientMovementInput(Vector2 clientInput) {
		if (uLink.Network.isServer) {
			this.clientInput = clientInput;
		}
	}

	// Client receiving latest X/Y movement from the Server
	[RPC]
	void receivePositionFromServer(Vector3 serverPosition) {
		this.serverPosition = serverPosition;
	}
}