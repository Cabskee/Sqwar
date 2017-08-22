using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler: MonoBehaviour {
	public static GameHandler Instance = null;

	[Header("Player")]
	public GameObject playerObject;
	public int startingLives;

	[Header("Gameplay")]
	public GameObject shootingBlock;

	public List<GameObject> players = new List<GameObject>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	public void createPlayer(uLink.NetworkPlayer owner) {
		uLink.Network.Instantiate(owner, playerObject, Vector3.zero, Quaternion.identity, 0, startingLives);
	}
}