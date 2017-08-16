#pragma strict

//IMPORT THE PROPER LIBS FOR GENERIC LISTS
import System.Collections.Generic;

//OBJECT CACHING
var playerStocks: int; //HOW MANY STOCKS THIS PLAYER HAS (UPDATED ON ALL CLIENTS)
var playerColor: Color; //THE COLOR OF THE PLAYER (SENT TO ALL CLIENTS UPON CONNECTING)
var playerNameMesh: TextMesh; //TEXTMESH OF THE PLAYER NAME OBJECT
var playerNameTag: Transform; //THE BACKGROUND OF THE PLAYER NAME OBJECT

//VARIABLES TO HANDLE SPAWNING AND TEMPORARY NAME HANDLING
private var immortalityCountdown: int; //HOW LONG THE PLAYER SHOULD BE "IMMORTAL" FOR (UPON SPAWNING/DEATH/RESPAWN)
private var randomPlayerNames: List.<String> = new List.<String>(); //ARRAY OF RANDOM NAMES TO CHOOSE FROM, UNTIL NAMING IS IMPLEMENTED

//DEFINE THESE TWO INSTEAD OF SETTING EACH SEPERATELY, JUST INCASE I MISPELL ONE
private var immortalPlayer: String = 'Immortal Player';
private var regularPlayer: String = 'Player';

//THE GLOBAL MUSIC PLAYER, FOR PLAYING MUSIC SPECIFICALLY ON THIS CLIENT
private var globalMusicPlayer: SoundPlayer;

function Awake() {
	if (GetComponent.<NetworkView>().isMine) {
		//FOR NOW A RANDOM COLOR IS GENERATED WHEN A NEW PLAYER SPAWNS
		//WILL BE REPLACED WITH PLAYERS BEING ABLE TO CHOOSE THEIR OWN COLOR & NAME (PLAYERPREFS)
		playerColor.r = Random.Range(0.0,1.0);
		playerColor.g = Random.Range(0.0,1.0);
		playerColor.b = Random.Range(0.0,1.0);
		playerColor.a = 1.0;

		var testVector4: Vector4 = playerColor;
		var vector4to3: Vector3 = testVector4;

		GetComponent.<Renderer>().material.color = playerColor;
		GetComponent.<NetworkView>().RPC('SendPlayerColor', RPCMode.OthersBuffered, GetComponent.<NetworkView>().viewID, vector4to3);

		globalMusicPlayer = GameObject.Find('Global Music Player').GetComponent(SoundPlayer);
	}
}

function Start() {
	if (GetComponent.<NetworkView>().isMine) {
		BeginImmortality();
		RandomizeName();

		globalMusicPlayer.ClientMusic(0, 1.0, 0.0, true, true);
	}
}

private var lastStockCount: int;

function Update() {
	/* TEMPORARY HANDLE FOR STOCKS CHANGING */
	if (lastStockCount != playerStocks) {
		lastStockCount = playerStocks;

		//CHANGE THE TEMPORARY "LIFE" TEXT IN THE TOP RIGHT TO YOUR CURRENT LIVES
		GameObject.Find('Player Stock (Testing)').GetComponent(TextMesh).text = ''+playerStocks+'';

		if (playerStocks == 1) { //IF THE PLAYER HAS ONE LIFE LEFT
			globalMusicPlayer.ClientMusic(2, 1.0, 1.0, false, true); //CHANGE THE MUSIC TO THE "ONE LIFE" MUSIC
		}
	}
}

//FOR NOW WE RANDOMIZE THE NAME FROM A SHORT ARRAY, EVENTUALLY THIS WILL BE REPLACED WITH CUSTOMIZABLE NAMES
function RandomizeName() {
	var test: String[] = ['Colton', 'Sqwar', 'Kendrick', 'Ivan', 'Jenny', 'DarKTower', 'Porkins', 'goose', 'Levi', 'Blockhead', 'George Sr.', 'GOB', 'Badspot', 'builder man', 'I am Bob', 'Tetris', 'Liam'];
	randomPlayerNames.AddRange(test);

	var randomNameChosen: String = randomPlayerNames[Random.Range(0, randomPlayerNames.Count)];

	playerNameMesh.text = randomNameChosen;
	var nameTagWidth: float = 0.6;
	for (var i=0;i<randomNameChosen.Length;i++) {
		if (i > 0) { nameTagWidth += 0.4; }
	};

	playerNameTag.transform.localScale = Vector3(nameTagWidth,0.8,0);
	GetComponent.<NetworkView>().RPC('SetPlayerName', RPCMode.OthersBuffered, GetComponent.<NetworkView>().viewID, randomNameChosen);
}

//CALLED WHEN A PLAYER SPAWNS/RESPAWNS, TO BEGIN THEIR ~4 SECONDS OF IMMORTALITY
function BeginImmortality() {
	immortalityCountdown = 0;
	gameObject.tag = immortalPlayer;
	gameObject.layer = LayerMask.NameToLayer(immortalPlayer);

	GetComponent.<NetworkView>().RPC('BlinkCharacter', RPCMode.Others, GetComponent.<NetworkView>().viewID, immortalPlayer, true);
	InvokeRepeating('ImmortalityCountdown', 0.25, 0.25);
}

//CALLED EVERY 0.25 SECONDS AND SENT ACROSS THE NETWORK TO ALL OTHER CLIENTS, TO BLINK THE PLAYER
function ImmortalityCountdown() {
	if (immortalityCountdown % 2 == 0) {
		GetComponent.<Renderer>().enabled = true;
		GetComponent.<NetworkView>().RPC('BlinkCharacter', RPCMode.Others, GetComponent.<NetworkView>().viewID, immortalPlayer, true);
	} else {
		GetComponent.<Renderer>().enabled = false;
		GetComponent.<NetworkView>().RPC('BlinkCharacter', RPCMode.Others, GetComponent.<NetworkView>().viewID, immortalPlayer, false);
	}

	if (immortalityCountdown < 8) { //GIVE IMMORTALITY FOR 4 SECONDS
		immortalityCountdown++;
	} else { //IMMORTALITY IS OVER, SO SET CHARACTER TO ENABLED
		GetComponent.<Renderer>().enabled = true;
		gameObject.tag = regularPlayer;
		gameObject.layer = LayerMask.NameToLayer(regularPlayer);

		GetComponent.<NetworkView>().RPC('BlinkCharacter', RPCMode.OthersBuffered, GetComponent.<NetworkView>().viewID, regularPlayer, true);
		CancelInvoke('ImmortalityCountdown');
	}
}

//CALLED WHEN THE PLAYER IS BLINKING
@RPC
function BlinkCharacter(playerViewID: NetworkViewID, tagAndLayerToSet: String, RendEnabled:boolean) {
	var thisPlayer = NetworkView.Find(playerViewID);

	thisPlayer.GetComponent.<Renderer>().enabled = RendEnabled;
	thisPlayer.gameObject.tag = tagAndLayerToSet;
	thisPlayer.gameObject.layer = LayerMask.NameToLayer(tagAndLayerToSet);
}

//CALLED ON ALL CLIENTS BY THE SERVER WHEN A PLAYER DIES, RESETS THEIR POSITION TO THE CENTER OF THE MAP
@RPC
function ResetPlayersPosition(playerViewID: NetworkViewID) {
	var playerToReset = NetworkView.Find(playerViewID);

	playerToReset.transform.position = Vector3(0,8,0);
	playerToReset.GetComponent.<Rigidbody2D>().velocity = Vector2(0,0);

	BeginImmortality();
}

//SENT FROM THE SERVER TO ALL CLIENTS TO SET A CERTAIN PLAYERS STOCKS
@RPC
function SetPlayerStock(playerViewID: NetworkViewID, stock:int) {
	NetworkView.Find(playerViewID).GetComponent(PlayerHandler).playerStocks = stock;
}

//CALLED ON THE SERVER WHEN A CLIENT DIES TO CALCULATE THE PLAYERS NEW STOCKS AND RESPAWN LOCATION, WHICH IS THEN SENT TO ALL CLIENTS
@RPC
function RemovePlayerStock(amount:int, info:NetworkMessageInfo) {
	var currentPlayerStocks = info.networkView.GetComponent(PlayerHandler).playerStocks;
	//var currentPlayerStocks = NetworkView.Find(playerViewID).GetComponent(PlayerHandler).playerStocks;
	var newPlayerStocks: int = currentPlayerStocks-amount;

	//SEND THE NEW PLAYER STOCK TO ALL CLIENTS (SET IT, CALCULATIONS DONE ON SERVER)
	GetComponent.<NetworkView>().RPC('SetPlayerStock', RPCMode.AllBuffered, info.networkView.viewID, newPlayerStocks);

	//SEND RPC TO ALL CLIENT'S TO RESET THIS PLAYER'S LOCATION TO SPAWNLOC
	GetComponent.<NetworkView>().RPC('ResetPlayersPosition', RPCMode.All, info.networkView.viewID);
}

//SENT FROM CLIENTS TO ALL OTHER CLIENTS TO SET THE SENDER'S PLAYER COLOR
@RPC
function SendPlayerColor(charViewID: NetworkViewID, theirColor:Vector3) {
	var asVector4: Vector4 = theirColor;
	var thisPlayer = NetworkView.Find(charViewID);

	thisPlayer.GetComponent(PlayerHandler).playerColor = asVector4;
	thisPlayer.GetComponent(PlayerHandler).playerColor.a = 1;
	thisPlayer.GetComponent.<Renderer>().material.color = asVector4;
	thisPlayer.GetComponent.<Renderer>().material.color.a = 1;
}

//SENT FROM CLIENTS TO ALL OTHER CLIENTS TO SET THE SENDER'S PLAYER NAME
@RPC
function SetPlayerName(playerViewID: NetworkViewID, playerName:String) {
	var player = NetworkView.Find(playerViewID).GetComponent(PlayerHandler);

	player.playerNameMesh.text = playerName;
	var nameTagWidth: float = 0.6;
	for (var i=0;i<playerName.Length;i++) {
		if (i > 0) { nameTagWidth += 0.4; }
	};

	player.playerNameTag.transform.localScale = Vector3(nameTagWidth,0.75,0);
}


/*
THIS FUNCTION SEARCHES A LIST FOR THE CONTENTS OF ANOTHER LIST. MAYBE POSSIBLY USE THIS FOR THE SCOREBOARD, IF NECESSARY.

function ListIsSameAsArray(searchList:GameObject[], fromList:List.<GameObject>) {
	var theSame = true;
	for (var s=0;s<searchList.Length;s++) {
		if (!fromList.Contains(searchList[s])) {
			theSame = false;
			Debug.Log('This LIST does not contain anything from this ARRAY.');
		}
	};

	if (theSame) {
		return true;
	} else {
		return false;
	}
}*/