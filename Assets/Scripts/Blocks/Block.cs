using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Constants;
using Prime31;

public class Block: NetworkBehaviour {
	protected CharacterController2D blockController;

	public Constant.FacingDirection shotDirection;
	public float speed;

	void Awake() {
		blockController = GetComponent<CharacterController2D>();
	}

	/// <summary>
	/// Fresh initializes a Block
	/// </summary>
	public void initialize(int numberSpawned, float speed) {
		initialize(numberSpawned, speed, Constant.FacingDirection.Down, Color.white);
	}
	public void initialize(int numberSpawned, float speed, Constant.FacingDirection facingDirection) {
		initialize(numberSpawned, speed, facingDirection, Color.white);
	}
	public void initialize(int numberSpawned, float speed, Constant.FacingDirection facingDirection, Color blockColor) {
		// Set GameObject name for sanity reasons
		gameObject.name = "Block "+numberSpawned;

		// Set block falling speed from Server
		this.speed = speed;

		// Set block facing based on Player
		this.shotDirection = facingDirection;

		// Set block color based on Player
		this.GetComponent<SpriteRenderer>().color = blockColor;
	}
}
