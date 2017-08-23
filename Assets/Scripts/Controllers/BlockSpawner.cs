using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;

public class BlockSpawner: MonoBehaviour {
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
		if (!uLink.Network.isServer) {
			Destroy(gameObject);
		}

		currentFallSpeed = baseFallSpeed;
	}

	void Update() {
		if (uLink.Network.isServer) {
			if (fallSpeedCounter < increaseFallSpeedIn) {
				fallSpeedCounter += Time.deltaTime;
			} else {
				fallSpeedCounter = 0;
				currentFallSpeed += increaseFallSpeedBy;
			}

			if (Time.time >= lastSpawnTime+Random.Range(spawnTimeInterval.x, spawnTimeInterval.y)) {
				uLink.NetworkView.Get(this).RPC("createFallingBlockAtLocation", uLink.RPCMode.All, new Vector3(Random.Range(mapSpawnWidth.x, mapSpawnWidth.y), 10, 0));
				lastSpawnTime = Time.time;
			}
		}
	}

	[RPC]
	void createFallingBlockAtLocation(Vector3 spawnPos) {
		GameObject newBlock = TrashMan.Instantiate(fallingBlock, spawnPos, Quaternion.identity, fallingBlockParent.transform);

		// Set falling speed & ViewID
		newBlock.GetComponent<FallingBlock>().initialize(blocksSpawned, currentFallSpeed);
		blocksSpawned++;
	}

	public void createShootingBlockAtLocation(Vector3 spawnPos, Constant.FacingDirection directionFacing, Color playerColor, uLink.NetworkViewID viewID) {
		GameObject newShootingBlock = TrashMan.Instantiate(shootingBlock, spawnPos, Quaternion.identity, shootingBlockParent.transform);
		newShootingBlock.GetComponent<ShootingBlock>().initialize(blocksSpawned, currentShootingSpeed, directionFacing, playerColor, viewID);
	}
}