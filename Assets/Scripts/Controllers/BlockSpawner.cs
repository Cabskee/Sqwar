using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Constants;

public class BlockSpawner: NetworkBehaviour {
	public static BlockSpawner Instance = null;

	[Header("Block GameObjects")]
	public GameObject fallingBlock;
	public GameObject fallingBlockParent;

	public GameObject shootingBlock;
	public GameObject shootingBlockParent;

	// Spawn Variables
	[Header("Block Spawn Settings")]
	public Vector2 mapSpawnWidth;
	public Vector2 spawnTimeInterval;
	public int blocksSpawned = 1;
	float lastSpawnTime;

	// Speed Variables
	[Header("Block Fall Speeds")]
	public float increaseFallSpeedIn;
	public float increaseFallSpeedBy;
	public float baseFallSpeed;
	public float currentFallSpeed;
	float fallSpeedCounter;

	[Header("Block Shooting Speeds")]
	public float baseShootingSpeed;
	public float currentShootingSpeed;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	void Start() {
		currentFallSpeed = baseFallSpeed;
		currentShootingSpeed = baseShootingSpeed;
	}

	void Update() {
		if (!isServer)
			return;

		if (fallSpeedCounter < increaseFallSpeedIn) {
			fallSpeedCounter += Time.deltaTime;
		} else {
			fallSpeedCounter = 0;
			currentFallSpeed += increaseFallSpeedBy;
		}

		if (Time.time >= lastSpawnTime+Random.Range(spawnTimeInterval.x, spawnTimeInterval.y)) {
			createFallingBlockAtLocation(new Vector3(Random.Range(mapSpawnWidth.x, mapSpawnWidth.y), 10, 0));
			lastSpawnTime = Time.time;
		}
	}

	// FALLING BLOCKS

	void createFallingBlockAtLocation(Vector3 spawnPos) {
		if (!isServer)
			return;

		GameObject newFallingBlock = TrashMan.Instantiate(fallingBlock, spawnPos, Quaternion.identity, fallingBlockParent.transform);
		newFallingBlock.GetComponent<FallingBlock>().initialize(blocksSpawned, currentFallSpeed);
		blocksSpawned++;

		NetworkServer.Spawn(newFallingBlock);
	}

	// SHOOTING BLOCKS

	public void createShootingBlockAtLocation(Vector3 spawnPos, Constant.FacingDirection directionFacing, Color playerColor, NetworkIdentity ownerIdentity) {
		if (!isServer)
			return;

		GameObject newShootingBlock = TrashMan.Instantiate(shootingBlock, spawnPos, Quaternion.identity, shootingBlockParent.transform);
		newShootingBlock.GetComponent<ShootingBlock>().initialize(blocksSpawned, currentShootingSpeed, ownerIdentity.playerControllerId, directionFacing, playerColor);
		blocksSpawned++;

		NetworkServer.Spawn(newShootingBlock);
	}
}