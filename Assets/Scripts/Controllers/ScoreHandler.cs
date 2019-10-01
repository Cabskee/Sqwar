using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler: MonoBehaviour {
	public static ScoreHandler Instance = null;

	public GameObject scoreboard;
	public GameObject scoreboardObject;
	public Dictionary<int, ScoreboardItem> currentScoreboard = new Dictionary<int, ScoreboardItem>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	public void addPlayerToScoreboard(int playerID, Player player) {
		if (doesScoreboardExistForPlayer(playerID)) {
			return;
		}

		GameObject newScoreboardObj = Instantiate(scoreboardObject, Vector3.zero, Quaternion.identity, scoreboard.transform);
		ScoreboardItem newScoreboardItem = new ScoreboardItem(newScoreboardObj, player);
		currentScoreboard.Add(playerID, newScoreboardItem);
	}

	public void removePlayerFromScoreboard(int playerID) {
		// TODO: Hide scoreboard object for this player
		if (doesScoreboardExistForPlayer(playerID)) {
			ScoreboardItem playerScoreboardObject = findScoreboardByPlayer(playerID);
			Destroy(playerScoreboardObject.obj);
			currentScoreboard.Remove(playerID);
		}
	}

	ScoreboardItem findScoreboardByPlayer(int playerID) {
		if (doesScoreboardExistForPlayer(playerID)) {
			return currentScoreboard[playerID];
		}

		return null;
	}

	bool doesScoreboardExistForPlayer(int playerID) => currentScoreboard.ContainsKey(playerID);
}

[System.Serializable]
public class ScoreboardItem {
	public PlayerScore score; //readonly
	public GameObject obj;

	public ScoreboardItem(GameObject scoreboardObj, Player player) {
		obj = scoreboardObj;
		score = scoreboardObj.GetComponent<PlayerScore>();

		scoreboardObj.GetComponent<PlayerScore>().setPlayerInfo(player);
	}
}