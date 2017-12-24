using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class PlayerScore: NetworkBehaviour {
	[SyncVar] public Color color;
	[SyncVar] public string playerName;
	[SyncVar] public int lives;

	// TODO: Make this better...

	Text playerNameText;
	public List<GameObject> liveObjects = new List<GameObject>();

	void Awake() {
		playerNameText = GetComponentInChildren<Text>();
	}

	void Update() {
		if (!gameObject.activeSelf)
			return;

		playerNameText.text = playerName;
		playerNameText.color = color;
		liveObjects.ForEach(delegate(GameObject life) {
			life.GetComponent<Image>().color = color;
			life.SetActive(false);
		});
		liveObjects.GetRange(0, lives).ForEach(delegate(GameObject life) {
			life.SetActive(true);
		});
	}

	public void updatePlayerInfo(string name, int currentLives, Color newColor) {
		if (!gameObject.activeSelf)
			gameObject.SetActive(true);

		playerName = name;
		lives = currentLives;
		color = newColor;
	}

	public void hide() {
		gameObject.SetActive(false);
	}
}
