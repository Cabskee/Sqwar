using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;
using Prime31;

public class Block: MonoBehaviour {
	protected CharacterController2D blockController;

	public Constant.FacingDirection shotDirection;
	public float speed;
	public Color color;

	public int ownerID;

	bool isDestroying = false;

	void Awake() {
		blockController = GetComponent<CharacterController2D>();

		GetComponent<SpriteRenderer>().color = color;
	}

	void OnEnable() {
		blockController.onControllerCollidedEvent += boundaryTriggerEvent;
	}

	void OnDisable() {
		blockController.onControllerCollidedEvent -= boundaryTriggerEvent;
	}

	protected void boundaryTriggerEvent(RaycastHit2D ray) {
		if (!isDestroying && didCollideWithLayer(ray, Constant.LAYER_BOUNDARY))
			destroyBlock();
	}

	protected bool isBlockDestroying() => isDestroying;
	public void destroyBlock() {
		if (!isBlockDestroying()) {
			isDestroying = true;
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Initializes a white block that falls downwards.
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	public void initialize(int numberSpawned, float speed) {
		initialize(numberSpawned, speed, 0, Constant.FacingDirection.Down, Color.white);
	}

	/// <summary>
	/// Initializes a white block that travels a certain direction
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	/// <param name="facingDirection">Direction the block will travel.</param>
	public void initialize(int numberSpawned, float speed, Constant.FacingDirection facingDirection) {
		initialize(numberSpawned, speed, 0, facingDirection, Color.white);
	}

	/// <summary>
	/// Initializes a colored block owned by a specific playerID that travels a certain direction.
	/// </summary>
	/// <param name="numberSpawned">ID of the block.</param>
	/// <param name="speed">Speed of the block.</param>
	/// <param name="playerID">ID of the player who spawned the block.</param>
	/// <param name="facingDirection">Direction the block will travel.</param>
	/// <param name="blockColor">Color of the block.</param>
	public void initialize(int numberSpawned, float speed, int playerID, Constant.FacingDirection facingDirection, Color blockColor) {
		// Set GameObject name for sanity reasons
		gameObject.name = "Block "+numberSpawned;

		// Set block falling speed from Server
		this.speed = speed;

		// Set block facing based on Player
		this.shotDirection = facingDirection;

		// Set the player who controls this block
		this.ownerID = playerID;

		// Set block color based on Player
		this.color = blockColor;
		this.GetComponent<SpriteRenderer>().color = blockColor;
	}

	/// <summary>
	/// Checks if player of playerId is the owner of this block.
	/// </summary>
	/// <returns><c>true</c>, if playerId is the owner of this block, <c>false</c> otherwise.</returns>
	/// <param name="playerId">Player ID.</param>
	protected bool isPlayerOwnerOfBlock(int playerID) => playerID == ownerID;

	protected bool didCollideWithALayer(RaycastHit2D ray, string[] layerNames) {
		foreach (string layerName in layerNames) {
			if (didCollideWithLayer(ray, layerName))
				return true;
		}
		return false;
	}
	protected bool didCollideWithLayer(RaycastHit2D ray, string layerName) {
		return ray.transform.gameObject.layer == LayerMask.NameToLayer(layerName);
	}
}
