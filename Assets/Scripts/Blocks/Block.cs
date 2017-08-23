using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;
using Prime31;

public class Block: MonoBehaviour {
	protected CharacterController2D blockController;

	public Constant.FacingDirection shotDirection;
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
	public void initialize(int numberSpawned, float speed) {
		initialize(numberSpawned, speed, uLink.Network.AllocateViewID(uLink.NetworkPlayer.server));
	}
	public void initialize(int numberSpawned, float speed, uLink.NetworkViewID viewID) {
		initialize(numberSpawned, speed, Constant.FacingDirection.Down, Color.white, viewID);
	}
	public void initialize(int numberSpawned, float speed, Constant.FacingDirection facingDirection, Color blockColor, uLink.NetworkViewID viewID) {
		// Set GameObject name for sanity reasons
		gameObject.name = "Block "+numberSpawned;

		// Set NetworkViewID & owner
		uLink.NetworkView.Get(this).SetViewID(viewID, viewID.allocator);

		// Set block falling speed from Server
		this.speed = speed;

		// Set block color based on Player
		this.GetComponent<SpriteRenderer>().color = blockColor;

		// Set block facing based on Player
		this.shotDirection = facingDirection;
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
