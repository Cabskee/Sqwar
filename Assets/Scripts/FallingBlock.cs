using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class FallingBlock: MonoBehaviour {
	CharacterController2D blockController;

	// Movement Speeds
	public float fallSpeed;

	void Awake() {
		blockController = GetComponent<CharacterController2D>();

		blockController.onControllerCollidedEvent += onTrigger;
	}
	
	void Update () {
		if (!blockController.isGrounded) {
			blockController.move(new Vector3(0, -1*fallSpeed*Time.deltaTime, 0));
		}
	}

	void onTrigger(RaycastHit2D collision) {
		if (collision.transform.gameObject.layer != LayerMask.NameToLayer("Falling Block") && collision.transform.gameObject.layer != LayerMask.NameToLayer("Platform")) {
			blockController.collisionState.reset();
		}
	}

	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
		fallSpeed = info.networkView.initialData.Read<float>();
	}
}