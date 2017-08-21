using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class FallingBlock: Block {
	void OnEnable() {
		blockController.onControllerCollidedEvent += onEnterTrigger;
	}

	void onEnterTrigger(RaycastHit2D collision) {
		// If boundary is hit on the Server, destroy this Block
		// If boundary is hit on the Client, request latest position from Server for this Block
		if (collision.transform.gameObject.layer == LayerMask.NameToLayer("Boundary")) {
			if (uLink.Network.isServer) {
				uLink.Network.Destroy(uLink.NetworkView.Get(this));
			} else {
				uLink.NetworkView.Get(this).RPC("requestCurrentPosition", uLink.RPCMode.Server, uLink.Network.player);
			}
		}

		if (collision.transform.gameObject.layer != LayerMask.NameToLayer("Falling Block") && collision.transform.gameObject.layer != LayerMask.NameToLayer("Platform")) {
			blockController.collisionState.reset();
		}
	}
	
	void Update () {
		if (!blockController.isGrounded) {
			blockController.move(new Vector3(0, -1*speed*Time.deltaTime, 0));
		}
	}
}