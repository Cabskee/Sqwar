using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner: MonoBehaviour {
	public GameObject fallingBlock;

	// Spawn Variables
	public Vector2 spawnTimeInterval;
	float lastSpawnTime;

	// Speed Variables
	public float throwSpeed;

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
				uLink.Network.Instantiate(uLink.Network.player, fallingBlock, new Vector3(Random.Range(-25.0f, 25.0f), 10, 0), Quaternion.identity, 0, throwSpeed, currentFallSpeed);
				lastSpawnTime = Time.time;
			}
		}
	}
}