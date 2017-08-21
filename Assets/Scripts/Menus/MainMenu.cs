using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu: MonoBehaviour {
	public void hostGame() {
		uLink.Network.InitializeServer(24, 25000, !uLink.Network.HavePublicAddress());
	}

	public void joinGame() {
		uLink.Network.Connect("127.0.0.1", 25000);
	}

	public void openSettings() {
		Debug.Log("Settings");
	}

	void uLink_OnServerInitialized() {
		SceneManager.LoadScene("Game");
	}

	void uLink_OnConnectedToServer() {
		SceneManager.LoadScene("Game");
	}
}
