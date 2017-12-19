using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler: MonoBehaviour {
	public static GameHandler Instance = null;

	[Header("Player")]
	public GameObject playerObject;
	public int startingLives;

	public List<GameObject> players = new List<GameObject>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}
}