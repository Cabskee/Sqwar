using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class NetworkHandler: MonoBehaviour {
	public GameObject playerObject;

	void Awake() {
		if (!uLink.Network.isServer && !uLink.Network.isClient) {
			uLink.Network.InitializeServer(24, 25000, !uLink.Network.HavePublicAddress());
			Debug.Log("Started up new Test server from starting on this Scene.");
		}
	}

	void OnEnable() {
		if (uLink.Network.isServer) {
			SceneManager.sceneLoaded += onServerSceneLoaded;
		}
	}

	void OnDisable() {
		if (uLink.Network.isServer) {
			SceneManager.sceneLoaded -= onServerSceneLoaded;
		}
	}

	// Scene loaded for the Server, 
	void onServerSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (uLink.Network.isServer) {
			uLink.Network.Instantiate(uLink.Network.player, playerObject, Vector3.zero, Quaternion.identity, 0);
		}
	}

	// Client connected to server, create their player object
	void uLink_OnPlayerConnected(uLink.NetworkPlayer player) {
		uLink.Network.Instantiate(player, playerObject, Vector3.zero, Quaternion.identity, 0);
	}

	// Client disconnected from server, go back to the Main Menu
	void uLink_OnDisconnectedFromServer() {
		SceneManager.LoadScene("Menu");
	}
}
