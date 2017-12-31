using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class GameHandler: NetworkBehaviour {
	public static GameHandler Instance = null;

	[Header("Player")]
	public GameObject playerObject;
	public int startingLives;

	public List<Player> players = new List<Player>();

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != null) {
			Destroy(this);
		}
	}

	public void addPlayer(PlayerController controller) {
		Player newPlayer = new Player(controller);
		newPlayer.name = controller.playerName;
		players.Add(newPlayer);

		Debug.Log("addPlayer called");
		ScoreHandler.Instance.updatePlayerInfo(players);
	}
}

[System.Serializable]
public class Player {
	public string name; //readonly
	public short controllerId; //readonly
	public PlayerController controller; //readonly
	public GameObject obj; //readonly

	public Player(PlayerController newController) {
		controller = newController;
		obj = newController.gameObject;

		controllerId = obj.GetComponent<NetworkIdentity>().playerControllerId;
	}
}