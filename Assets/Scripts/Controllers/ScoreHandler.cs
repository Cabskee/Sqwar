using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class ScoreHandler: NetworkBehaviour {
	public static ScoreHandler Instance = null;

	public GameObject scoreboard;
	public GameObject scoreboardObject;
	public List<ScoreboardObject> scoreboardObjects = new List<ScoreboardObject>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	[ServerCallback]
	public void updatePlayerInfo(List<Player> players) {
		players.ForEach(delegate(Player player) {
			ScoreboardObject playerScoreboardObject = findScoreboardByPlayer(player.networkId);
			if (playerScoreboardObject == null) {
				GameObject newScoreboardObjectObj = Instantiate(scoreboardObject, Vector3.zero, Quaternion.identity, scoreboard.transform);
				ScoreboardObject newScoreboardObject = new ScoreboardObject(newScoreboardObjectObj, player.networkId);
				newScoreboardObject.score.updatePlayerInfo(player);

				scoreboardObjects.Add(newScoreboardObject);

				NetworkServer.Spawn(newScoreboardObjectObj);
			} else {
				playerScoreboardObject.score.updatePlayerInfo(player);
			}
		});
	}

	[ServerCallback]
	public void removePlayerFromScoreboard(uint playerNetworkId) {
		// TODO: Hide scoreboard object for this player
		if (doesScoreboardExistForPlayer(playerNetworkId)) {
			ScoreboardObject playerScoreboardObject = findScoreboardByPlayer(playerNetworkId);
			Destroy(playerScoreboardObject.obj);
			scoreboardObjects.Remove(playerScoreboardObject);
		}
	}

	[ServerCallback]
	ScoreboardObject findScoreboardByPlayer(uint playerNetworkId) {
		if (doesScoreboardExistForPlayer(playerNetworkId)) {
			return scoreboardObjects.Find(delegate(ScoreboardObject obj) {
				return obj.playerNetworkId == playerNetworkId;
			});
		}
		return null;
	}

	[ServerCallback]
	bool doesScoreboardExistForPlayer(uint playerNetworkId) {
		return scoreboardObjects.Exists(delegate(ScoreboardObject obj) {
			return obj.playerNetworkId == playerNetworkId;
		});
	}
}

[System.Serializable]
public class ScoreboardObject {
	public GameObject obj; //readonly
	public PlayerScore score; //readonly
	public uint playerNetworkId; //readonly

	public ScoreboardObject(GameObject newObj, uint networkId) {
		obj = newObj;
		score = newObj.GetComponent<PlayerScore>();

		playerNetworkId = networkId;
	}
}