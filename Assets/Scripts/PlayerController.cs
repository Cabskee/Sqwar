using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Prime31;

public class PlayerController: MonoBehaviour {
	CharacterController2D charController;

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
	Vector3 serverVelocity;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	void Start() {
		if (uLink.NetworkView.Get(this).isMine) {
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ProCamera2D>().AddCameraTarget(transform);
		}
	}

	void Update() {
		if (charController.isGrounded)
			clientVelocity.y = 0;

		if (uLink.Network.isClient) {
			clientInput = Vector2.zero;
		}

		if (uLink.NetworkView.Get(this).isMine) {
			normalizedHorizontal = Input.GetAxisRaw("Horizontal");
			clientInput.x = normalizedHorizontal;

			if (charController.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
				clientInput.y = 1f;
			}
		} else if (uLink.Network.isServer) {
			normalizedHorizontal = clientInput.x;
			if (clientInput.y == 1.0f) {
				clientVelocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			}
		} else if (uLink.Network.isClient) {
			transform.position = serverPosition;
			return;
		}

		// Apply horizontal speed smoothing
		var smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		clientVelocity.x = Mathf.Lerp(clientVelocity.x, normalizedHorizontal * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		clientVelocity.y += gravity * Time.deltaTime;

		// Send movement
		charController.move(clientVelocity * Time.deltaTime);

		// Update latest clientVelocity
		clientVelocity = charController.velocity;
	}

	void FixedUpdate() {
		// Owner sends current Input to the Server every FixedUpdate
		if (uLink.NetworkView.Get(this).isMine) {
			uLink.NetworkView.Get(this).UnreliableRPC("receiveClientInput", uLink.RPCMode.Server, clientInput);
		}

		// Server sends current Velocity and Position to every Client every FixedUpdate
		if (uLink.Network.isServer) {
			uLink.NetworkView.Get(this).UnreliableRPC("receivePositionFromServer", uLink.RPCMode.Others, clientVelocity, transform.position);
		}
	}

	// Server receiving input from a Client
	[RPC]
	void receiveClientInput(Vector2 clientInput) {
		if (uLink.Network.isServer) {
			this.clientInput = clientInput;
		}
	}

	// Client receiving latest position from the Server
	[RPC]
	void receivePositionFromServer(Vector3 serverVelocity, Vector3 serverPosition) {
		this.serverVelocity = serverVelocity;
		this.serverPosition = serverPosition;
	}
}
