using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;
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
		Vector3 moveDirection = Vector3.zero;
		switch(shotDirection) {
			case Constant.FacingDirection.Down:
				moveDirection.y = -1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Up:
				moveDirection.y = 1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Left:
				moveDirection.x = -1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Right:
			default:
				moveDirection.x = 1 * speed * Time.deltaTime;
				break;
		}

		blockController.move(moveDirection);
	}

	public void setDirectionFacing(float direction) {
		transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
	}
}
