using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;

public class ShootingBlock: Block {
	readonly string[] collisionLayers = new string[]{
		Constant.LAYER_PLATFORM,
		Constant.LAYER_PLACEDBLOCK,
		Constant.LAYER_FALLINGBLOCK,
		Constant.LAYER_SHOOTINGBLOCK,
		Constant.LAYER_PLAYER
	};

	void OnEnable() {
		blockController.onControllerCollidedEvent += onShootingBlockCollision;
	}

	void OnDisable() {
		blockController.onControllerCollidedEvent -= onShootingBlockCollision;
	}

	void onShootingBlockCollision(RaycastHit2D ray) {
		boundaryTriggerEvent(ray);

		if (!isBlockDestroying()) { // If block didn't hit boundary
			if (didCollideWithALayer(ray, collisionLayers)) { // If block hit something noteworthy
				if (didCollideWithLayer(ray, Constant.LAYER_PLAYER)) { // If the block hit a player
					if (!isPlayerOwnerOfBlock(ray.transform.gameObject.GetComponent<PlayerController>().playerID)) {
						// If block hit another player
						GameHandler.Instance.PlayerGotAKill(ownerID);
						ray.transform.GetComponent<PlayerController>().killPlayer();
					} else {
						return; // Block hit yourself, it should keep going
					}
				} else {
					// If block hit another block (Falling or Placed)
					ray.transform.GetComponent<Block>().destroyBlock();
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
