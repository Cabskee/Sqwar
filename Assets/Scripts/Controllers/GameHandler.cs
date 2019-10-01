using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler: MonoBehaviour {
	public static GameHandler Instance = null;

	[Header("Player")]
	public GameObject playerObject;
	public GameObject playerObjectParent;
	public int startingLives;

	public List<string> names = new List<string>();
	public Dictionary<int, Player> players = new Dictionary<int, Player>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}

		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		// TODO: Tie this into main menu
		// For now, just start game with some players
		StartGame(4);
	}

	void StartGame(int numPlayers) {
		for (int i=0;i<numPlayers;i++) {
			Vector3 spawnPos = new Vector3(i+(i*3), 0, 0);
			GameObject newPlayer = TrashMan.Instantiate(playerObject, spawnPos, Quaternion.identity, playerObjectParent.transform);
			addPlayerToGame(newPlayer, i + 1);
		}
	}

	public void addPlayerToGame(GameObject playerObject, int playerID) {
		Player newPlayer = new Player(playerObject, startingLives);
		newPlayer.controller.initialize(playerID, ChooseRandomName(), GenerateRandomColor());
		players.Add(playerID, newPlayer);

		// Add this player to the scoreboard
		ScoreHandler.Instance.addPlayerToScoreboard(playerID, newPlayer);
	}

	bool DoesPlayerExist(int playerID) => players.ContainsKey(playerID);
	Color GenerateRandomColor() => new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	string ChooseRandomName() {
		int randomIndex = Random.Range(0, names.Count);
		string chosenName = names[randomIndex];
		names.RemoveAt(randomIndex);
		return chosenName;
	}

	public void PlayerGotAKill(int playerID) {
		if (!DoesPlayerExist(playerID)) {
			return;
		}

		players[playerID].score.kills += 1;
	}

	public void PlayerLostALife(int playerID) {
		if (!DoesPlayerExist(playerID)) {
			return;
		}

		players[playerID].score.lives -= 1;
	}
}

[System.Serializable]
public class Player {
	public GameObject obj; //readonly
	public PlayerController controller; //readonly

	public Score score;

	public Player(GameObject playerObj, int startingLives) {
		obj = playerObj;
		controller = playerObj.GetComponent<PlayerController>();

		score = new Score(startingLives);
	}

	public int getPlayerID() {
		return controller.playerID;
	}
}

[System.Serializable]
public class Score {
	public int kills = 0;
	public int lives;

	public Score(int startingLives) {
		lives = startingLives;
	}
}