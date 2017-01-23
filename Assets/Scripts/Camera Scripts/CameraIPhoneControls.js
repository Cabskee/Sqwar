#pragma strict

var IPhoneLeftMovepad: GameObject; //GAMEOBJECT OF THE LEFT MOVEPAD
var IPhoneRightMovepad: GameObject; //GAMEOBJECT OF THE RIGHT MOVEPAD
var IPhoneJumpButton: GameObject; //GAMEOBJECT OF THE "A" BUTTON (JUMP)
var IPhoneThrowButton: GameObject; //GAMEOBJECT OF THE "B" BUTTON (PICKUP/THROW)
var IPhoneCharacter: GameObject; //THE CHARACTER OF THIS CAMERA (FOR MOVEMENT)

var IPhoneJoystick: GameObject; //
var IPhoneJoystickKnob: GameObject;

var backgroundParticleSystem: ParticleSystem;

private var thisPlayerMovement: PlayerMovement;

//MOVEPAD VARIABLES
private var LeftMovepadLocation: Vector3;
private var RightMovepadLocation: Vector3;
private var MovepadColliderWidth: float; //WIDTH OF THE PADS (25% OF SCREEN WIDTH)
private var MovepadColliderHeight: float; //HEIGHT OF THE PADS (100% OF SCREEN HEIGHT)

private var IPhoneUICamera: Camera;

function Awake() {
	IPhoneUICamera = GetComponent(Camera);
}

function Start () {
	if (Application.platform == RuntimePlatform.IPhonePlayer) { //ONLY SETUP THE CONTROLS IF IT'S ON AN IPHONE
		thisPlayerMovement = IPhoneCharacter.GetComponent(PlayerMovement);
		//MOVEPAD VARIABLES & POSITIONING
		yield WaitForSeconds(0.1);/*
		LeftMovepadLocation = IPhoneUICamera.ScreenToWorldPoint(Vector3(0,0,0));
		RightMovepadLocation = IPhoneUICamera.ScreenToWorldPoint(Vector3(IPhoneUICamera.pixelWidth*0.2,IPhoneUICamera.pixelHeight,0));
		MovepadColliderWidth = Mathf.Abs(LeftMovepadLocation.x-RightMovepadLocation.x);
		MovepadColliderHeight = Mathf.Abs(LeftMovepadLocation.y-RightMovepadLocation.y);*/

		SetUpControls(); //ENABLE THE A/B BUTTONS
		//SetUpPads(); //CALCULATE THE POSITIONS & SIZES OF THE MOVEPADS
	}

	backgroundParticleSystem.transform.localPosition = Vector3(0,5,50);
	backgroundParticleSystem.Play();
}

function SetUpControls() {
	IPhoneJumpButton.GetComponent.<Renderer>().enabled = true;
	IPhoneJumpButton.GetComponent.<Collider2D>().enabled = true;
	IPhoneThrowButton.GetComponent.<Renderer>().enabled = true;
	IPhoneThrowButton.GetComponent.<Collider2D>().enabled = true;
	IPhoneJoystick.GetComponent.<Renderer>().enabled = true;
	IPhoneJoystick.GetComponent.<Collider2D>().enabled = true;
	IPhoneJoystickKnob.GetComponent.<Renderer>().enabled = true;

	//MOVE THEM ALL TO BE AT 0, TO ALLOW TOUCH EVENTS TO OCCUR
	IPhoneJumpButton.transform.localPosition.z = 0;
	IPhoneThrowButton.transform.localPosition.z = 0;
	IPhoneJoystick.transform.localPosition.z = 0;
	IPhoneJoystickKnob.transform.localPosition.z = 0;
}

function SetUpPads() {
	IPhoneLeftMovepad.transform.localPosition = Vector3(LeftMovepadLocation.x,0,0);
	IPhoneLeftMovepad.GetComponent(BoxCollider2D).size = Vector2(MovepadColliderWidth,MovepadColliderHeight);
	IPhoneLeftMovepad.GetComponent(BoxCollider2D).offset = Vector2(MovepadColliderWidth/2,0);

	IPhoneRightMovepad.transform.localPosition = Vector3(RightMovepadLocation.x,0,0);
	IPhoneRightMovepad.GetComponent(BoxCollider2D).size = Vector2(MovepadColliderWidth,MovepadColliderHeight);
	IPhoneRightMovepad.GetComponent(BoxCollider2D).offset = Vector2(MovepadColliderWidth/2,0);
}

function Update() {
	if (Input.touchCount > 0) { //IF THERE WAS A TOUCH
		if (Input.touchCount < 4) { //IF THERE WERE LESS THAN 4 TOUCHES AT ONCE
			for (var t=0;t<Input.touchCount;t++) {
				var thisTouch = Input.GetTouch(t); //GET THIS TOUCH
				var thisTouchPos: Vector3 = IPhoneUICamera.ScreenToWorldPoint(thisTouch.position);
				var thisTouchIs: int;

				Debug.Log('The index of this touch is: '+t);

				if (IPhoneJoystick.GetComponent.<Collider2D>().bounds.Contains(thisTouchPos)) {
					var knobPositionX: float = thisTouchPos.x-IPhoneJoystick.transform.position.x;
					var knobPositionY: float = thisTouchPos.y-IPhoneJoystick.transform.position.y;

					//KEEP THEM CLAMPED TO INSIDE THE JOYSTICK CIRCLE
					knobPositionX = Mathf.Clamp(knobPositionX, -1.0, 1.0);
					knobPositionY = Mathf.Clamp(knobPositionY, -1.0, 1.0);
					IPhoneJoystickKnob.transform.localPosition = Vector3(knobPositionX, knobPositionY, 0);

					if (knobPositionX > 0.0) { //MOVING RIGHT
						thisPlayerMovement.IPhoneMovement = knobPositionX;
					} else { //MOVING LEFT
						thisPlayerMovement.IPhoneMovement = knobPositionX;
					}
					
					thisTouchIs = 1;
				}
				/*
				//ALLOW THE USER TO HOLD THE LEFT/RIGHT/JUMP BUTTONS FOR FREEFORM MOVING/JUMPING
				if (IPhoneLeftMovepad.collider2D.bounds.Contains(thisTouchPos)) {
					thisPlayerMovement.IPhoneMovement = -1.0;
					thisTouchIs = 1;
				}
				if (IPhoneRightMovepad.collider2D.bounds.Contains(thisTouchPos)) {
					thisPlayerMovement.IPhoneMovement = 1.0;
					thisTouchIs = 1;
				}*/
				if (IPhoneJumpButton.GetComponent.<Collider2D>().bounds.Contains(thisTouchPos)) {
					thisPlayerMovement.IPhoneJump = 1.0;
					thisTouchIs = 2;
				}

				//ONLY ALLOW PICKUP/THROW WHEN THE BUTTON HAS BEEN PUSHED FRESH
				if (thisTouch.phase == TouchPhase.Began) {
					if (IPhoneThrowButton.GetComponent.<Collider2D>().bounds.Contains(thisTouchPos)) {
						if (thisPlayerMovement.carryingBox) {
							thisPlayerMovement.ThrowBoxClient();
						} else {
							thisPlayerMovement.DetectNearestBox();
						}
					}
				}

				//IF THE USER LETS GO OF THE LEFT/RIGHT/JUMP BUTTONS, STOP THEIR MOVEMENT
				if (thisTouch.phase == TouchPhase.Ended || thisTouch.phase == TouchPhase.Canceled) {
					Debug.Log('Touch ended, its TouchIs: '+thisTouchIs);
					if (thisTouchIs == 1) { //MOVING LEFT OR RIGHT
						thisPlayerMovement.IPhoneMovement = 0.0;
						IPhoneJoystickKnob.transform.localPosition = Vector3(0,0,0);
					}
					if (thisTouchIs == 2) { //JUMPING
						thisPlayerMovement.IPhoneJump = 0.0;
					}
				}
			};
		} else { //IF THERE ARE 4 TOUCHES GOING ON
			Debug.LogWarning('You should be taken back to the Main Menu after 2 seconds.');
		}
	}
}