using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class Block: MonoBehaviour {
	protected CharacterController2D blockController;

	public float speed;

	void Awake() {
		blockController = GetComponent<CharacterController2D>();
	}

	/// <summary>
	/// Fresh initializes a Block with a name, speed, and ViewID/Owner.
	/// </summary>
	/// <param name="numberSpawned">Block Name</param>
	/// <param name="speed">Movement/Fall Speed</param>
	/// <param name="viewID">View ID and Owner</param>
	public void initialize(int numberSpawned, float speed, uLink.NetworkViewID viewID) {
		// Set GameObject name for sanity reasons
		gameObject.name = "Falling Block "+numberSpawned;

		// Set NetworkViewID & owner
		uLink.NetworkView.Get(this).SetViewID(viewID, viewID.allocator);

		// Set block falling speed
		this.speed = speed;
	}

	// Refreshes the position of this Block on a specific or all client(s)
	[RPC]
	void requestCurrentPosition(uLink.NetworkPlayer requester) {
		uLink.NetworkView.Get(this).RPC("receiveCurrentPosition", requester, transform.position);
	}

	[RPC]
	void receiveCurrentPosition(Vector3 currentPos) {
		transform.position = currentPos;
		blockController.collisionState.reset();
	}
}
