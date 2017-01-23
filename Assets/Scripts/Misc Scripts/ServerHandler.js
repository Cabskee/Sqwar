#pragma strict

var playerPrefab: GameObject;
var GUICamera: Camera;

private var starBackground: GameObject;

function Start() {
	starBackground = GameObject.Find('Sqwar Logo Background');
}

function SetUpCamera() {
	var aspectRatio: float = Mathf.Round(Camera.main.aspect*10);

	//IF THE BACKGROUND IS ACTIVE, DISABLE IT NOW
	if (starBackground.activeSelf) {
		starBackground.SetActive(false);
	}

	var thisPlayer = SpawnPlayer(); //SPAWN THIS CLIENTS PLAYER
	Camera.main.GetComponent(CameraMovement).enabled = true; //ALLOW MOVEMENT OF THE CAMERA
	Camera.main.GetComponent(CameraMovement).SetCharacter(thisPlayer.transform); //SET THE CAMERA TO FOLLOW THIS CLIENTS PLAYER

	GUICamera.enabled = true; //SET THE CAMERA TO ACTIVE
	GUICamera.GetComponent(CameraIPhoneControls).enabled = true; //SET THE GUI CONTROLS TO ACTIVE
	GUICamera.GetComponent(CameraIPhoneControls).IPhoneCharacter = thisPlayer; //SET THE PLAYER OBJECT FOR IPHONE MOVEMENT

	yield WaitForEndOfFrame();
	if (aspectRatio == 13) { //4:3, IPAD WIDE
		Camera.main.orthographicSize = 9.25;
	} else if (aspectRatio == 15) { //3:2, IPHONE NON-RETINA WIDE
		Camera.main.orthographicSize = 8.6;
	} else if (aspectRatio == 18) { //16:9, IPHONE 5 WIDE
		Camera.main.orthographicSize = 8;
	}
}

function SpawnPlayer() {
	var spawnPlayer = uLink.Network.Instantiate(playerPrefab, Vector3(0, 8, 0), Quaternion.identity, 0); //SPAWN THE PLAYER OVER THE NETWORK

	return spawnPlayer;
}

//CALLED ON THE CLIENT WHEN IT CONNECTS TO A SERVER
function uLink_OnConnectedToServer() {
	Debug.Log('You have connected to a server, and you are a Client.');
	SetUpCamera(); //SETUP THE CAMERA FOR GAMEPLAY
}

//CALLED ON THE SERVER WHEN IT IS INITIALIZED
function uLink_OnServerInitialized() {
	Debug.Log('Server has been initialized, and you are the Server.');
	SetUpCamera(); //SET UP THE CAMERA FOR GAMEPLAY
}

//CALLED ON THE SERVER WHEN A PLAYER DISCONNECTS FROM IT
function uLink_OnPlayerDisconnected(player:NetworkPlayer) {
	Debug.Log('A player has disconnected for some reason, so remove all their RPCs and objects.');
	Network.RemoveRPCs(player);
	Network.DestroyPlayerObjects(player);
}

//CALLED ON THE CLIENT WHEN IT DISCONNECTS OR IS DISCONNECTED FROM A SERVER
function uLink_OnDisconnectedFromServer() {
	Application.LoadLevel(0);
}

//CALLED ON THE SERVER WHEN A CLIENT HAS CONNECTED
function uLink_OnPlayerConnected(newPlayer:NetworkPlayer) {
	Debug.Log('You are the Server, and a player has connected to you.');
}

//CALLED ON THE CLIENT WHEN THE CONNECTION FAILS FOR SOME REASON
function uLink_OnFailedToConnect(error:NetworkConnectionError) {
	Debug.LogWarning('You have failed to connect to a server: '+error);
}

//CALLED ON BOTH CLIENT AND SERVER WHEN A CONNECTION COULD NOT BE MADE TO THE UNITY MASTER SERVER
function uLink_OnFailedToConnectToMasterServer(error:NetworkConnectionError) {
	Debug.LogWarning('Failed to connect to MasterServer: '+error);
}