#pragma strict

var connectButton: Collider2D;
var textFieldText: TextMesh;
var textFieldCollider: Collider2D;
var backButton: Collider2D;
var enterIPTitle: Transform;

var joinMenuAnimator: Animator;
var directIPMenuAnimator: Animator;

//private var IPhoneKeyboard: TouchScreenKeyboard;
private var IPhoneKeyboardValue: String;
private var thisSoundPlayer: SoundPlayer;

function Awake() {
	thisSoundPlayer = GetComponentInParent(SoundPlayer);
}

function ClickedConnectToIPAddress() {
	Network.Connect(textFieldText.text, 25501);
}

function ClickedTextField() {
	if (Application.platform == RuntimePlatform.IPhonePlayer) {
		//IPhoneKeyboard = TouchScreenKeyboard.Open(IPhoneKeyboardValue, TouchScreenKeyboardType.Default, false, false, false, false);
	}
}

function ClickedBack() {
	directIPMenuAnimator.SetBool('DirectIPIn', false);
	yield WaitForSeconds(Menu_Main.fadeTime);
	Camera.main.transform.position = Vector3(10,-50,-50);
	joinMenuAnimator.SetBool('JoinServerIn', true);
}

function Update () {
	var touchPos: Vector3;

	if (Input.touchCount >= 1) {
		touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
	} else if (Input.GetMouseButtonDown(0)) {
		touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	touchPos.z = 0;

	if (Input.touchCount >= 1 || Input.GetMouseButtonDown(0)) {
		if (connectButton.bounds.Contains(touchPos)) {
			ClickedConnectToIPAddress();
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
		if (textFieldCollider.bounds.Contains(touchPos)) {
			ClickedTextField();
		}
		if (backButton.bounds.Contains(touchPos)) {
			ClickedBack();
			thisSoundPlayer.ClientNoise(0, 0.75, 1.0, false);
		}
	}

	if (directIPMenuAnimator.GetBool('DirectIPIn') && Network.peerType == NetworkPeerType.Disconnected) {
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			/*if (IPhoneKeyboard) {
				textFieldText.text = IPhoneKeyboard.text;
			}*/
		} else { //FOR THE WEBPLAYER/COMPUTER CLIENT
			for (var c:char in Input.inputString) {
				if (c == "\b"[0]) { //IF BACKSPACED, REMOVE THE LAST CHARACTER OF THE STRING
					if (textFieldText.text.Length > 0) {
						textFieldText.text = textFieldText.text.Substring(0, textFieldText.text.Length-1);
					}
				} else if (c == "\n"[0] || c == "\r"[0]) { //\N FOR MAC, \R FOR WINDOWS IS "ENTER"
					ClickedConnectToIPAddress();
				} else {
					textFieldText.text += c;
				}
			}
		}
	}
}