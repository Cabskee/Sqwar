#pragma strict

private var characterToFollow: Transform;
private var aspectRatio: float;
private var yOffset: float;

function Start() {
	aspectRatio = Mathf.Round(Camera.main.aspect*10);

	//MOVE THE CAMERA UP A COUPLE UNITS TO MAKE UP FOR THE WIDER/SMALLER ORTHO SIZE
	if (aspectRatio == 13) { //4:3, IPAD WIDE
		yOffset = 4.25;
	} else if (aspectRatio == 15) { //3:4, IPHONE WIDE
		yOffset = 3.25;
	} else if (aspectRatio == 18) { //16:9, IPHONE 5 WIDE
		yOffset = 2;
	}
}

function Update () {
	if (characterToFollow) { //IF CHARACTER TO FOLLOW HAS BEEN SET
		transform.position = Vector3.Lerp(transform.position, Vector3(characterToFollow.position.x, characterToFollow.position.y+yOffset, -10), Time.deltaTime*30);
		//transform.position = Vector3(characterToFollow.position.x, characterToFollow.position.y+yOffset, -10);
	}
}

function SetCharacter(t:Transform) {
	characterToFollow = t;
}