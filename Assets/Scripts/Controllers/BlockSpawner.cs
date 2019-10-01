using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;

[System.Serializable]
public class MinMax {
	public float min = 0.0f;
	public float max = 1.0f;
}

public class BlockSpawner: MonoBehaviour {
	public static BlockSpawner Instance = null;

	[Header("Block GameObjects")]
	public GameObject fallingBlock;
	public GameObject fallingBlockParent;

	public GameObject shootingBlock;
	public GameObject shootingBlockParent;

	// Spawn Variables
	[Header("Block Spawn Settings")]
	public MinMax mapSpawnWidth;
	public MinMax spawnTimeInterval;
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
		if (fallSpeedCounter < increaseFallSpeedIn) {
			fallSpeedCounter += Time.deltaTime;
		} else {
			fallSpeedCounter = 0;
			currentFallSpeed += increaseFallSpeedBy;
		}

		if (Time.time >= lastSpawnTime+Random.Range(spawnTimeInterval.min, spawnTimeInterval.max)) {
			createFallingBlockAtLocation(new Vector3(Random.Range(mapSpawnWidth.min, mapSpawnWidth.max), 10, 0));
			lastSpawnTime = Time.time;
		}
	}

	// FALLING BLOCKS
	void createFallingBlockAtLocation(Vector3 spawnPos) {
		GameObject newFallingBlock = TrashMan.Instantiate(fallingBlock, spawnPos, Quaternion.identity, fallingBlockParent.transform);
		newFallingBlock.GetComponent<FallingBlock>().initialize(blocksSpawned, currentFallSpeed);
		blocksSpawned++;
	}

	// SHOOTING BLOCKS
	public void createShootingBlockAtLocation(Vector3 spawnPos, Constant.FacingDirection directionFacing, Color playerColor, int playerID) {
		GameObject newShootingBlock = TrashMan.Instantiate(shootingBlock, spawnPos, Quaternion.identity, shootingBlockParent.transform);
		newShootingBlock.GetComponent<ShootingBlock>().initialize(blocksSpawned, currentShootingSpeed, playerID, directionFacing, playerColor);
		blocksSpawned++;
	}
}