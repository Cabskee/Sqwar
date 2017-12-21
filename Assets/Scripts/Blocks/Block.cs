using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class Block: NetworkBehaviour {
	protected CharacterController2D blockController;

	[SyncVar] public Constant.FacingDirection shotDirection;
	[SyncVar] public float speed;
	[SyncVar] public Color color;

	bool isDestroying = false;

	short ownerPlayerControllerId;

	void Awake() {
		blockController = GetComponent<CharacterController2D>();
	}

	void Start() {
		if (isServer)
			blockController.onControllerCollidedEvent += boundaryTriggerEvent;
	}

	public override void OnStartClient() {
		GetComponent<SpriteRenderer>().color = color;
	}

	protected void boundaryTriggerEvent(RaycastHit2D ray) {
		if (!isDestroying && didCollideWithLayer(ray, Constant.LAYER_BOUNDARY))
			destroyBlock();
	}

	protected bool isBlockDestroying() {
		return isDestroying;
	}
	protected void destroyBlock() {
		if (!isDestroying) {
			isDestroying = true;
			NetworkServer.Destroy(gameObject);
		}
	}

	/// <summary>
	/// Initializes a white block server-wide that falls downwards.
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	public void initialize(int numberSpawned, float speed) {
		initialize(numberSpawned, speed, GetComponent<NetworkIdentity>().playerControllerId, Constant.FacingDirection.Down, Color.white);
	}

	/// <summary>
	/// Initializes a white block that travels a certain direction
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	/// <param name="facingDirection">Direction the block will travel.</param>
	public void initialize(int numberSpawned, float speed, Constant.FacingDirection facingDirection) {
		initialize(numberSpawned, speed, GetComponent<NetworkIdentity>().playerControllerId, facingDirection, Color.white);
	}

	/// <summary>
	/// Initializes a colored block that travels a certain direction.
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	/// <param name="playerControllerId">ID of the player who spawned the block.</param>
	/// <param name="facingDirection">Direction the block will travel.</param>
	/// <param name="blockColor">Color of the block.</param>
	public void initialize(int numberSpawned, float speed, short ownerPlayerControllerId, Constant.FacingDirection facingDirection, Color blockColor) {
		// Set GameObject name for sanity reasons
		gameObject.name = "Block "+numberSpawned;

		// Set block falling speed from Server
		this.speed = speed;

		// Set block facing based on Player
		this.shotDirection = facingDirection;

		// Set the player who controls this block
		this.ownerPlayerControllerId = ownerPlayerControllerId;

		// Set block color based on Player
		this.color = blockColor;
		this.GetComponent<SpriteRenderer>().color = blockColor;
	}

	/// <summary>
	/// Checks if player of playerId is the owner of this block.
	/// </summary>
	/// <returns><c>true</c>, if playerId is the owner of this block, <c>false</c> otherwise.</returns>
	/// <param name="playerId">Player NetworkIdentity Controller ID.</param>
	protected bool isPlayerOwnerOfBlock(short playerId) {
		return playerId == ownerPlayerControllerId;
	}

	protected bool didCollideWithALayer(RaycastHit2D ray, string[] layerNames) {
		foreach (string layerName in layerNames) {
			if (didCollideWithLayer(ray, layerName)) {
				return true;
			}
		}
		return false;
	}
	protected bool didCollideWithLayer(RaycastHit2D ray, string layerName) {
		return ray.transform.gameObject.layer == LayerMask.NameToLayer(layerName);
	}
}
