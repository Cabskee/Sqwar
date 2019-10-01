using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerScore: MonoBehaviour {
	public Color color;
	public string playerName;

	Text nameTextObject;
	public GameObject lifeObject;

	List<GameObject> lives;

	void Awake() {
		nameTextObject = GetComponentInChildren<Text>();

		transform.SetParent(ScoreHandler.Instance.scoreboard.transform, false);
	}

	public void setPlayerInfo(Player player) {
		playerName = player.controller.playerName;
		color = player.controller.color;

		nameTextObject.text = playerName;
		nameTextObject.color = color;
	}
}
