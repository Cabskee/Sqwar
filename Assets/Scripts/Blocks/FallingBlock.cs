using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class FallingBlock: Block {
	[SyncVar] public bool hasLanded = false;

	readonly string[] collisionLayers = new string[]{
		Constant.LAYER_PLATFORM,
		Constant.LAYER_PLACEDBLOCK,
		Constant.LAYER_PLAYER
	};

	[ServerCallback]
	void Start() {
		blockController.onControllerCollidedEvent += onFallingBlockCollision;
	}

	[ServerCallback]
	void onFallingBlockCollision(RaycastHit2D ray) {
		if (!hasLanded) {
			boundaryTriggerEvent(ray);

			if (didCollideWithALayer(ray, collisionLayers)) {
				hasLanded = true;
				transform.gameObject.layer = LayerMask.NameToLayer(Constant.LAYER_PLACEDBLOCK);
				blockController.onControllerCollidedEvent -= onFallingBlockCollision;
			}
		}
	}
	
	void Update () {
		if (!hasLanded && !blockController.isGrounded) {
			// If the Falling Block has not landed yet, move it downwards
			blockController.move(new Vector3(0, -1*speed*Time.deltaTime, 0));
		}
	}
}