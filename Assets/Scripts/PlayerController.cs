using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Prime31;

public class PlayerController: MonoBehaviour {
	CharacterController2D charController;

	public GameObject carriedBlock;

	// Movement Variables
	public float gravity;
	public float speed;
	public float groundDamping;
	public float airDamping;
	public float jumpHeight;

	float normalizedHorizontal;

	Vector2 clientInput;
	Vector3 clientVelocity;

	Vector3 serverPosition;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	void Start() {
		if (uLink.NetworkView.Get(this).isMine) {
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ProCamera2D>().AddCameraTarget(transform);
		}

		carriedBlock.SetActive(false);
	}

	void Update() {
		if (charController.isGrounded)
			clientVelocity.y = 0;

		if (uLink.Network.isClient)
			clientInput = Vector2.zero;

		if (uLink.NetworkView.Get(this).isMine) { // Owner Movement & Actions
			normalizedHorizontal = Input.GetAxisRaw("Horizontal");
			clientInput.x = normalizedHorizontal;

			if (charController.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
				clientInput.y = 1f;
			}

			// Rubberband player back to Server Position if he moves too far
			Debug.Log(Mathf.Abs(transform.position.y - serverPosition.y));
			if (Mathf.Abs(transform.position.x - serverPosition.x) >= 3f) {
				transform.position = new Vector3(serverPosition.x, transform.position.y, 0);
			}
			if (Mathf.Abs(transform.position.y - serverPosition.y) >= 3.8f) {
				transform.position = new Vector3(transform.position.x, serverPosition.y, 0);
			}

			// Pickup a nearby block or fire a carried block
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.LeftShift)) {
				if (!isCarryingBlock()) { // Pick up the closest Block
					GameObject pickUpBlock = findNearestFallenBlock();
					if (pickUpBlock) {
						// Tell the server you picked up this block and pick it up clientside
						uLink.NetworkView.Get(this).RPC("receiveClientPickUpInput", uLink.RPCMode.Server);
						carriedBlock.SetActive(true);
					}
				} else { // Fire a Carried Block
					Debug.Log("Fire!");
				}
			}
		} else if (uLink.Network.isServer) { // Server Movement (Using Input from Client)
			normalizedHorizontal = clientInput.x;
			if (clientInput.y == 1.0f) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			}
		} else if (uLink.Network.isClient) { // Proxy Movement (Carbon copy of Server movement)
			float proxyScale = (serverPosition.x > transform.position.x) ? 1 : -1;
			transform.localScale = new Vector3(proxyScale, 1, 1);
			transform.position = serverPosition;
			return;
		}

		// Apply horizontal speed smoothing
		var smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		clientVelocity.x = Mathf.Lerp(clientVelocity.x, normalizedHorizontal * speed, Time.deltaTime*smoothedMovement);

		// Face the player left or right based on movement
		if (normalizedHorizontal != 0f)
			transform.localScale = new Vector3(normalizedHorizontal, 1, 1);

		// Apply downward gravity
		clientVelocity.y += gravity * Time.deltaTime;

		// Send movement
		charController.move(clientVelocity * Time.deltaTime);

		// Update latest clientVelocity
		clientVelocity = charController.velocity;
	}

	public bool isCarryingBlock() {
		return carriedBlock.activeSelf;
	}

	GameObject findNearestFallenBlock() {
		GameObject[] nearbyGOs = GameObject.FindGameObjectsWithTag("Falling Block");
		GameObject nearestBlock = null;
		Vector2 nearestDistance = Vector2.positiveInfinity;
		foreach (GameObject block in nearbyGOs) {
			Vector2 newDist = new Vector2(Mathf.Abs(transform.position.x - block.transform.position.x), Mathf.Abs(transform.position.y - block.transform.position.y));
			if (newDist.x <= 1.3f && newDist.y <= 0.25f && newDist.x <= nearestDistance.x && newDist.y <= nearestDistance.y) {
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

		// Server sends current Velocity and Position to every Client every FixedUpdate
		if (uLink.Network.isServer) {
			uLink.NetworkView.Get(this).UnreliableRPC("receivePositionFromServer", uLink.RPCMode.All, transform.position);
		}
	}

	[RPC]
	void receiveClientPickUpInput() {
		if (uLink.Network.isServer) {
			GameObject nearestBlock = findNearestFallenBlock();
			if (nearestBlock && !isCarryingBlock()) {
				uLink.Network.Destroy(nearestBlock.GetComponent<uLink.NetworkView>().networkView);
			}
			uLink.NetworkView.Get(this).RPC("receivePickUpResponseFromServer", uLink.RPCMode.All, (nearestBlock && !isCarryingBlock()));
		}
	}

	[RPC]
	void receivePickUpResponseFromServer(bool pickedUp) {
		carriedBlock.SetActive(pickedUp);
	}

	// Server receiving input from a Client
	[RPC]
	void receiveClientMovementInput(Vector2 clientInput) {
		if (uLink.Network.isServer) {
			this.clientInput = clientInput;
		}
	}

	// Client receiving latest position from the Server
	[RPC]
	void receivePositionFromServer(Vector3 serverPosition) {
		this.serverPosition = serverPosition;
	}
}
