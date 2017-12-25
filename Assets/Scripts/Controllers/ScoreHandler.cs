using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class ScoreHandler: NetworkBehaviour {
	public static ScoreHandler Instance = null;

	public List<PlayerScore> scoreObjects = new List<PlayerScore>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}

		hideScoreboard();
	}

	void Start() {
		scoreObjects.ForEach(delegate(PlayerScore obj) {
			//obj.updatePlayerInfo("Player "+Random.Range(0,scoreObjects.Count), Random.Range(0, 7), new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f)));
		});
	}

	public void updatePlayerInfo(List<PlayerController> players) {
		hideScoreboard();

		for (int i=0;i<players.Count;i++) {
			scoreObjects[i].updatePlayerInfo(players[i].name, players[i].livesLeft, players[i].color);
		};
	}

	void hideScoreboard() {
		scoreObjects.ForEach(delegate(PlayerScore playerScore) {
			playerScore.hide();
		});
	}
}
