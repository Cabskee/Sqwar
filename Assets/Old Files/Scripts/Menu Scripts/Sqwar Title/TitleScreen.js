#pragma strict

var scaleToGoTo: float;
private var aspectRatio: float;

function Awake() {
	transform.localScale = Vector3(0,0,0);
}

function Start() {
	aspectRatio = Mathf.Round(Camera.main.aspect*10);

	if (aspectRatio == 13) { //4:3, IPAD WIDE
		Camera.main.orthographicSize = 4.25;
	} else if (aspectRatio == 15) { //3:2, IPHONE NON-RETINA WIDE
		Camera.main.orthographicSize = 3.8;
	} else if (aspectRatio == 18) { //16:9, IPHONE 5 WIDE
		Camera.main.orthographicSize = 3.2;
	}
}

function Update() {
	if (transform.localScale.x < scaleToGoTo) {
		transform.localScale.x += 0.05;
		transform.localScale.y += 0.05;
	}

	if (Input.GetMouseButtonDown(0) || Input.touchCount >= 1) {
		Network.minimumAllocatableViewIDs = 5000; //SET TO 1000 POSSIBLE VIEW IDS
		Application.LoadLevel(1); //LOAD THE GAME
	}
}