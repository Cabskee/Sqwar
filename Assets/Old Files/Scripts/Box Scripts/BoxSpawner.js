#pragma strict

var boxParentObj: Transform;
var spawnInterval: float; //INTERVAL AT WHICH TO SPAWN THE BOXES
var fallingBoxPrefab: GameObject;

function OnServerInitialized() {
	GetComponent.<NetworkView>().viewID = Network.AllocateViewID();

	Debug.Log('You are the Server, so you should start spawning Falling Boxes.');
	InvokeRepeating('ServerSpawnBox', 0.0, spawnInterval);
}

function ServerSpawnBox() {
	var thisBox = Network.Instantiate(fallingBoxPrefab, Vector3(Random.Range(-29, 29), 30, 0), Quaternion.identity, 0);
	thisBox.transform.parent = boxParentObj;
}