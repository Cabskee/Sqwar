using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class ShootingBlock: Block {
	readonly string[] collisionLayers = new string[]{
		Constant.LAYER_PLATFORM,
		Constant.LAYER_PLACEDBLOCK,
		Constant.LAYER_FALLINGBLOCK,
		Constant.LAYER_SHOOTINGBLOCK,
		Constant.LAYER_PLAYER
	};

	void Start() {
		if (isServer)
			blockController.onControllerCollidedEvent += onShootingBlockCollision;
	}

	void onShootingBlockCollision(RaycastHit2D ray) {
		boundaryTriggerEvent(ray);

		if (!isBlockDestroying()) {
			if (didCollideWithALayer(ray, collisionLayers)) {
				destroyBlock(); // Collided with something noteworthy, destroy self

				if (!didCollideWithLayer(ray, Constant.LAYER_PLATFORM)) {
					if (didCollideWithLayer(ray, Constant.LAYER_PLAYER)) { // Collided with player
						if (isPlayerOwnerOfBlock(ray.transform.gameObject.GetComponent<NetworkIdentity>().playerControllerId)) {
							Debug.Log("This is your own block!");
						} else {
							Debug.Log("You hit someone else");
						}
					} else {
						// SEND MESSAGE TO OTHER BLOCK THAT IT SHOULD KILL ITSELF
					}
				}
			}
		}
	}

	void Update() {
		Vector3 moveDirection = Vector3.zero;
		switch(shotDirection) {
			case Constant.FacingDirection.Up:
				moveDirection.y = 1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Down:
				moveDirection.y = -1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Right:
			default:
				moveDirection.x = 1 * speed * Time.deltaTime;
				break;
			case Constant.FacingDirection.Left:
				moveDirection.x = -1 * speed * Time.deltaTime;
				break;
		}

		blockController.move(moveDirection);
	}
}
