using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler: MonoBehaviour {
	public static GameHandler Instance = null;

	[Header("Player")]
	public GameObject playerObject;
	public int startingLives;

	public List<PlayerController> players = new List<PlayerController>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	public void addPlayer(PlayerController controller) {
		players.Add(controller);

		ScoreHandler.Instance.updatePlayerInfo(players);
	}
}