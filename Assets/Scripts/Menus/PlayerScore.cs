using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class PlayerScore: NetworkBehaviour {
	[SyncVar] public Color color;
	[SyncVar] public string playerName;
	public int lives;

	Text playerNameText;
	public List<Image> liveObjects = new List<Image>();

	void Awake() {
		playerNameText = GetComponentInChildren<Text>();
	}

	void Start() {
		if (!isServer)
			transform.SetParent(ScoreHandler.Instance.scoreboard.transform, false);
	}

	void Update() {
		playerNameText.text = playerName;
		playerNameText.color = color;
		liveObjects.ForEach(delegate(Image life) {
			life.color = color;
		});
	}

	public void updatePlayerInfo(Player playerObj) {
		playerName = playerObj.controller.name;
		lives = playerObj.controller.livesLeft;
		color = playerObj.controller.color;
	}
}
