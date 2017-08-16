#pragma strict

var hostObject: Collider2D;
var hostPrivateObj: Collider2D;
var backObj: Collider2D;

var startMenuAnimator: Animator;
var hostMenuAnimator: Animator;

private var thisSoundPlayer: SoundPlayer;
private var clickedSomething: boolean = false;

function Awake() {
	thisSoundPlayer = GetComponentInParent(SoundPlayer);
}

//HOST AND DISPLAY SERVER ON THE MASTERSERVER LIST
function ClickedHostPublicServer() {
	uLink.Network.InitializeServer(7, Random.Range(25000,25100), !Network.HavePublicAddress());
	uLink.MasterServer.RegisterHost("SqwarServer", "Sqwar Server #"+Random.Range(1,11), "Testing");
	clickedSomething = false;
}

//HOST PRIVATELY BY IP, IGNORING THE MASTERSERVER
function ClickedHostPrivateServer() {
	uLink.Network.InitializeServer(7, 25501);
	clickedSomething = false;
}

function ClickedBack() {
	hostMenuAnimator.SetBool('HostServerIn', false);
	yield WaitForSeconds(Menu_Main.fadeTime);
	Camera.main.transform.position = Vector3(0,-50,-50);
	startMenuAnimator.SetBool('StartMenuOut', false);
	clickedSomething = false;
}

function Update() {
	var touchPos: Vector3;

	if (Input.touchCount > 0) {
		touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
	} else if (Input.GetMouseButtonDown(0)) {
		touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	touchPos.z = 0;

	if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) && !clickedSomething) {
		if (hostObject.bounds.Contains(touchPos)) {
			ClickedHostPublicServer();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (hostPrivateObj.bounds.Contains(touchPos)) {
			ClickedHostPrivateServer();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (backObj.bounds.Contains(touchPos)) {
			ClickedBack();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
	}
}