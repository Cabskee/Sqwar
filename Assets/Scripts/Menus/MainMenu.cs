using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu: MonoBehaviour {
	public void hostGame() {
		uLink.Network.isAuthoritativeServer = true;
		uLink.Network.InitializeServer(24, 25000, !uLink.Network.HavePublicAddress());
	}

	public void joinGame() {
		uLink.Network.isAuthoritativeServer = true;
		uLink.Network.Connect("127.0.0.1", 25000);
	}

	public void openCustomization() {
		Debug.Log("Customization");
	}

	void uLink_OnServerInitialized() {
		SceneManager.LoadScene("Game");
	}

	void uLink_OnConnectedToServer() {
		SceneManager.LoadScene("Game");
	}
}
