#pragma strict

var joinServerObj: Collider2D;
var hostServerObj: Collider2D;
var settingsObj: Collider2D;

var startMenuAnimator: Animator;
var joinServerAnimator: Animator;
var hostServerAnimator: Animator;

private var thisSoundPlayer: SoundPlayer;
private var clickedSomething: boolean = false;

static var fadeTime: float = 0.5;

function Awake() {
	thisSoundPlayer = GetComponentInParent(SoundPlayer);
}

//QUICK METHOD TO RESET THE CAMERA WHEN QUICKTESTING
function Start() {
	Camera.main.transform.position = Vector3(0,-50,-50);
	Camera.main.orthographicSize = 3.2;
}

function ClickedJoinServer() {
	startMenuAnimator.SetBool('StartMenuOut', true);
	yield WaitForSeconds(fadeTime);
	Camera.main.transform.position = Vector3(10,-50,-50);
	joinServerAnimator.SetBool('JoinServerIn', true);
	clickedSomething = false;
}

function ClickedHostServer() {
	startMenuAnimator.SetBool('StartMenuOut', true);
	yield WaitForSeconds(fadeTime);
	Camera.main.transform.position = Vector3(-10,-50,-50);
	hostServerAnimator.SetBool('HostServerIn', true);
	clickedSomething = false;
}

function ClickedSettings() {
	Debug.Log('Settings.');
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
		if (joinServerObj.bounds.Contains(touchPos)) {
			ClickedJoinServer();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (hostServerObj.bounds.Contains(touchPos)) {
			ClickedHostServer();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (settingsObj.bounds.Contains(touchPos)) {
			ClickedSettings();
			clickedSomething = true;
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
	}
}