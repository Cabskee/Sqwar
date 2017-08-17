using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class FallingBlockMovement: MonoBehaviour {
	CharacterController2D blockController;

	// Movement Speeds
	public float throwSpeed;
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
		Debug.Log(collision.transform.gameObject);
		if (collision.transform.gameObject.layer == LayerMask.NameToLayer("Falling Block")) {
			Debug.Log("Box on box action");
		}
	}

	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
		throwSpeed = info.networkView.initialData.Read<float>();
		fallSpeed = info.networkView.initialData.Read<float>();
	}
}