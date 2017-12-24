using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class ScoreHandler: NetworkBehaviour {
	public List<PlayerScore> scoreObjects = new List<PlayerScore>();

	void Awake() {
		hideScoreboard();
	}

	void Start() {
		scoreObjects.ForEach(delegate(PlayerScore obj) {
			//obj.updatePlayerInfo("Player "+Random.Range(0,scoreObjects.Count), Random.Range(0, 7), new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f)));
		});
	}

	void hideScoreboard() {
		scoreObjects.ForEach(delegate(PlayerScore playerScore) {
			playerScore.hide();
		});
	}
}
