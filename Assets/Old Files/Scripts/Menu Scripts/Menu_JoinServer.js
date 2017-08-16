#pragma strict

var directIPObj: Collider2D;
var serverListObj: Collider2D;
var backObj: Collider2D;

var joinMenuAnimator: Animator;
var startMenuAnimator: Animator;
var directIPMenuAnimator: Animator;

private var thisSoundPlayer: SoundPlayer;
private var clickedSomething: boolean = false;

function Awake() {
	thisSoundPlayer = GetComponentInParent(SoundPlayer);
}

function ConnectToServer(serverData: HostData) {
	Network.Connect(serverData);
}

function SetupServerListings() {
	var listingData: HostData[] = MasterServer.PollHostList();

	if (listingData.Length > 0) {
		ConnectToServer(listingData[0]);
	}
}

function ClickedDirectIP() {
	joinMenuAnimator.SetBool('JoinServerIn', false);
	yield WaitForSeconds(Menu_Main.fadeTime);
	Camera.main.transform.position = Vector3(10,-60,-50);
	directIPMenuAnimator.SetBool('DirectIPIn', true);
	clickedSomething = false;
}

function ClickedServerListing() {
	MasterServer.ClearHostList();
	MasterServer.RequestHostList('SqwarServer');

	clickedSomething = false;
	SetupServerListings();
}

function ClickedBack() {
	joinMenuAnimator.SetBool('JoinServerIn', false);
	yield WaitForSeconds(Menu_Main.fadeTime);
	Camera.main.transform.position = Vector3(0,-50,-50);
	startMenuAnimator.SetBool('StartMenuOut', false);
	clickedSomething = false;
}

function Update() {
	var thisTouchPos: Vector3;

	if (Input.touchCount > 0) {
		thisTouchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
	} else if (Input.GetMouseButtonDown(0)) {
		thisTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	thisTouchPos.z = 0;

	if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) && !clickedSomething) {
		if (directIPObj.bounds.Contains(thisTouchPos)) {
			clickedSomething = true;
			ClickedDirectIP();
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (serverListObj.bounds.Contains(thisTouchPos)) {
			clickedSomething = true;
			ClickedServerListing();
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (backObj.bounds.Contains(thisTouchPos)) {
			clickedSomething = true;
			ClickedBack();
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
	}
}