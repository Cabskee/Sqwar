using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner: MonoBehaviour {
	public GameObject fallingBlock;

	// Spawn Variables
	public Vector2 mapWidth;
	public Vector2 spawnTimeInterval;
	float lastSpawnTime;

	// Speed Variables
	public float increaseFallSpeedIn;
	public float increaseFallSpeedBy;
	public float baseFallSpeed;
	public float currentFallSpeed;
	float fallSpeedCounter;

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
				GameObject newObj = uLink.Network.Instantiate(uLink.Network.player, fallingBlock, new Vector3(Random.Range(mapWidth.x, mapWidth.y), 10, 0), Quaternion.identity, 0, currentFallSpeed);
				newObj.name = "Falling Block "+Random.Range(0,5000000);
				lastSpawnTime = Time.time;
			}
		}
	}
}