using System.Collections;
using System.Collections.Generic;
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
	Vector3 velocity;

	void Awake() {
		charController = GetComponent<CharacterController2D>();
	}

	void Update() {
		if (charController.isGrounded)
			velocity.y = 0;

		// Get Input from the owner
		if (uLink.NetworkView.Get(this).isMine) {
			normalizedHorizontal = Input.GetAxisRaw("Horizontal");

			if (charController.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
				velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			}
		}

		// Apply horizontal speed smoothing
		var smoothedMovement = charController.isGrounded ? groundDamping : airDamping;
		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontal * speed, Time.deltaTime*smoothedMovement);

		// Apply downward gravity
		velocity.y += gravity * Time.deltaTime;

		// Send movement
		charController.move(velocity * Time.deltaTime);

		// Update latest velocity
		velocity = charController.velocity;
	}
}
