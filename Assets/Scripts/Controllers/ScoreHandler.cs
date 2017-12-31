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
			ScoreboardObject playerScoreboardObject = findScoreboardByPlayer(player.controllerId);
			if (playerScoreboardObject == null) {
				GameObject newScoreboardObjectObj = Instantiate(scoreboardObject, Vector3.zero, Quaternion.identity, scoreboard.transform);
				ScoreboardObject newScoreboardObject = new ScoreboardObject(newScoreboardObjectObj, player.controllerId);
				newScoreboardObject.score.updatePlayerInfo(player);

				scoreboardObjects.Add(newScoreboardObject);

				NetworkServer.Spawn(newScoreboardObjectObj);
			} else {
				playerScoreboardObject.score.updatePlayerInfo(player);
			}
		});
	}

	[ServerCallback]
	public void removePlayerFromScoreboard(short playerControllerId) {
		// TODO: Hide scoreboard object for this player
		if (doesScoreboardExistForPlayer(playerControllerId)) {
			ScoreboardObject playerScoreboardObject = findScoreboardByPlayer(playerControllerId);
			Destroy(playerScoreboardObject.obj);
			scoreboardObjects.Remove(playerScoreboardObject);
		}
	}

	[ServerCallback]
	ScoreboardObject findScoreboardByPlayer(short playerControllerId) {
		if (doesScoreboardExistForPlayer(playerControllerId)) {
			return scoreboardObjects.Find(delegate(ScoreboardObject obj) {
				return obj.playerNetworkId == playerControllerId;
			});
		}
		return null;
	}

	[ServerCallback]
	bool doesScoreboardExistForPlayer(short playerControllerId) {
		return scoreboardObjects.Exists(delegate(ScoreboardObject obj) {
			return obj.playerNetworkId == playerControllerId;
		});
	}
}

[System.Serializable]
public class ScoreboardObject {
	public GameObject obj; //readonly
	public PlayerScore score; //readonly
	public short playerNetworkId; //readonly

	public ScoreboardObject(GameObject newObj, short networkId) {
		obj = newObj;
		score = newObj.GetComponent<PlayerScore>();

		playerNetworkId = networkId;
	}
}