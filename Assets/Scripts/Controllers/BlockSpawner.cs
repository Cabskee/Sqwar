using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner: MonoBehaviour {
	public static BlockSpawner Instance = null;

	[Header("GameObjects")]
	public GameObject fallingBlock;
	public GameObject fallingBlockParent;

	// Spawn Variables
	[Header("Spawn Timings")]
	public Vector2 mapSpawnWidth;
	public Vector2 spawnTimeInterval;
	float lastSpawnTime;

	// Speed Variables
	[Header("Fall Speed")]
	public float increaseFallSpeedIn;
	public float increaseFallSpeedBy;
	public float baseFallSpeed;
	public float currentFallSpeed;
	float fallSpeedCounter;

	[Header("Shooting Speed")]
	public float shootingSpeed;

	public int blocksSpawned = 1;

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

	public void createShootingBlockAtLocation(Vector3 spawnPos, float directionFacing, uLink.NetworkViewID viewID) {
		GameObject shootingBlock = TrashMan.Instantiate(GameHandler.Instance.shootingBlock, spawnPos, Quaternion.identity);
		shootingBlock.GetComponent<ShootingBlock>().initialize(blocksSpawned, shootingSpeed, directionFacing, viewID);
	}
}