using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu: MonoBehaviour {
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
