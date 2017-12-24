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
		if (!isServer)
			return;

		boundaryTriggerEvent(ray);

		if (!isBlockDestroying()) { // If block didn't hit boundary
			if (didCollideWithALayer(ray, collisionLayers)) { // If block hit something noteworth
				if (!didCollideWithLayer(ray, Constant.LAYER_PLATFORM)) { // If block did NOT hit the platform
					if (didCollideWithLayer(ray, Constant.LAYER_PLAYER)) { // If the block hit a player
						if (!isPlayerOwnerOfBlock(ray.transform.gameObject.GetComponent<NetworkIdentity>().playerControllerId)) {
							// If block hit another player
							ray.transform.GetComponent<PlayerController>().killPlayer();
						} else {
							// If block hit yourself
							Debug.Log("This is your own block, not sure what to do");
						}
					} else {
						// If block hit another block (Falling or Placed)
						ray.transform.GetComponent<Block>().collidedWithBlock();
					}
				}

				// Block hit something, it should destroy itself
				destroyBlock();
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
