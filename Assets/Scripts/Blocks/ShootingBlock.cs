using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class ShootingBlock: Block {
	void OnEnable() {
		blockController.onControllerCollidedEvent += checkForBoundary;
	}

	void checkForBoundary(RaycastHit2D hit) {
		// If boundary is hit on the Server, destroy this Block
		if (uLink.Network.isServer && hit.transform.gameObject.layer == LayerMask.NameToLayer("Boundary")) {
			uLink.Network.Destroy(uLink.NetworkView.Get(this));
		}
	}
	
	void Update () {
		Vector3 blockVelocity = new Vector3(transform.localScale.x * speed, 0f, 0f);
		blockController.move(blockVelocity * Time.deltaTime);
	}

	public void setDirectionFacing(float direction) {
		transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
	}
}
