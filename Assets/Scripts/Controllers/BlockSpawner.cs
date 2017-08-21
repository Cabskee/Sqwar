using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner: MonoBehaviour {
	public GameObject fallingBlock;
	public GameObject fallingBlockParent;

	// Spawn Variables
	public Vector2 mapSpawnWidth;
	public Vector2 spawnTimeInterval;
	float lastSpawnTime;

	// Speed Variables
	public float increaseFallSpeedIn;
	public float increaseFallSpeedBy;
	public float baseFallSpeed;
	public float currentFallSpeed;
	float fallSpeedCounter;

	public int blocksSpawned = 1;

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
		newBlock.GetComponent<FallingBlock>().initialize(blocksSpawned, currentFallSpeed, uLink.Network.AllocateViewID(uLink.NetworkPlayer.server));
		blocksSpawned++;
	}
}