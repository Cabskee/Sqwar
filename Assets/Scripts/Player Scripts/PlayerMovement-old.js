#pragma strict

//MOVEMENT SPEED & FORCE VARIABLES
var moveSpeed: float; //THE SPEED YOU MOVE
var jumpForce: float; //THE FORCE AT WHICH YOU JUMP
var jumpGravity: float; //GRAVITY TO APPLY WHEN JUMPING
var afterJumpGravity: float; //GRAVITY TO APPLY WHEN FALLING AFTER A JUMP

var facingRight: boolean;
var isGrounded: boolean;

var carryingBox: Transform;

//iPHONE MOVEMENT VARIABLES
var IPhoneMovement: float;
var IPhoneJump: float;

private var thisPlayerHandler: PlayerHandler;
private var thisSoundPlayer: SoundPlayer;

private var physicsJumping: boolean;
private var physicsPickup: boolean;
private var physicsThrow: boolean;

function Start() {
	thisPlayerHandler = GetComponent(PlayerHandler);
	thisSoundPlayer = GetComponent(SoundPlayer);

	thisSoundPlayer.ClientNoise(0, 1.0, 1.0, false); //PLAY THE SPAWNING NOISE
}

function OnCollisionEnter2D(other:Collision2D) {
	if (GetComponent.<NetworkView>().isMine) {
		if (other.collider.CompareTag('Platform') || other.collider.CompareTag('Falling Box')) { //IF COLLIDED WITH THE PLATFORM OR ANOTHER BOX
			if (transform.position.y >= Mathf.Round(other.transform.position.y)) { //IF THIS PLAYER IS ABOVE THE BOX/PLATFORM
				isGrounded = true; //ALLOW HIM TO JUMP AGAIN
			}
		}
		if (other.collider.CompareTag('Catcher Platform')) { //IF THE PLAYER FALLS OFF THE MAP
			if (Network.isClient) { //IF IT'S THE CLIENT, REQUEST FOR THE STOCK TO BE REMOVED FROM THE SERVER
				//networkView.RPC('RemovePlayerStock', RPCMode.Server, networkView.viewID, 1);
			} else { //IF IT'S THE SERVER, REMOVE THE STOCK AND SEND IT TO THE CLIENTS
				//thisPlayerHandler.RemovePlayerStock(networkView.viewID, 1);
			}
		}
	}
}

private var stuckOnLeft: boolean;
private var stuckOnRight: boolean;

function OnCollisionStay2D(other:Collision2D) {
	if (other.transform.CompareTag('Falling Box') && !other.collider.isTrigger) { //ONLY FIX THE "STUCK" ISSUE FOR FALLING BOXES (THAT ARE NOT BEING CARRIED)
		if (transform.position.y < Mathf.Round(other.transform.position.y)+0.9) { //IF THIS PLAYER IS NOT ONTOP OF THE BOX
			if (transform.position.x > other.transform.position.x) { //IF THIS PLAYER IS ON THE RIGHT OF THE BOX
				stuckOnRight = true;
				stuckOnLeft = false;
			} else if (transform.position.x < other.transform.position.x) { //IF THIS PLAYER IS ON THE LEFT OF THE BOX
				stuckOnLeft = true;
				stuckOnRight = false;
			} else {
				stuckOnRight = false;
				stuckOnLeft = false;
			}
		}
	}
}

function OnCollisionExit2D(other:Collision2D) {
	if (other.transform.CompareTag('Falling Box')) { //IF STOPPING CONTACT WITH THE BOX, ALLOW FREE MOVEMENT AGAIN
		stuckOnLeft = false;
		stuckOnRight = false;
	}
}

private var IPhoneLerpTimer: float = 0.0;

function FixedUpdate() {
	if (GetComponent.<NetworkView>().isMine) {
		//JUMPING SCRIPT
		if (Application.platform != RuntimePlatform.IPhonePlayer) { //EDITOR/WEBPLAYER CODE FOR PLAYER INTERACTIONS
			if (physicsJumping) {
				isGrounded = false;
				physicsJumping = false;
				GetComponent.<Rigidbody2D>().AddForce(Vector2(0, jumpForce)); //JUMP THE CHARACTER USING FORCE

				var newYPosition = transform.position.y;
				GetComponent.<NetworkView>().RPC('SimulateYPosition', RPCMode.Others, jumpForce, newYPosition);

				thisSoundPlayer.ClientNoise(1, 0.6, 0.25, true); //PLAY THE JUMPING NOISE ACROSS THE NETWORK
			}
			//HORIZONTAL MOVEMENT SCRIPT
			if (Input.GetAxis('Horizontal')) { //IF INPUT IS DETECTING (KEYBOARD)
				if (Input.GetAxis('Horizontal') > 0.0 && !stuckOnLeft) { //ONLY ALLOW RIGHT-SIDE MOVEMENT IF THE PLAYER IS NOT STUCK
					GetComponent.<Rigidbody2D>().velocity.x = Input.GetAxis('Horizontal') * moveSpeed;
				} else if (Input.GetAxis('Horizontal') < 0.0 && !stuckOnRight) { //ONLY ALLOW LEFT-SIDE MOVEMENT IF THE PLAYER IS NOT STUCK
					GetComponent.<Rigidbody2D>().velocity.x = Input.GetAxis('Horizontal') * moveSpeed;
				}
				var newXPosition = transform.position.x; //SEND THE NEW POSITION (FOR THIS FRAME) TO ALL OTHER CLIENTS

				GetComponent.<NetworkView>().RPC('SimulateXPosition', RPCMode.Others, Input.GetAxis('Horizontal') * moveSpeed, newXPosition);
			} else { GetComponent.<Rigidbody2D>().velocity.x = 0.0; }

			//PICKING UP BOX SYSTEM
			if (!carryingBox && physicsPickup) {
				DetectNearestBox();
				physicsPickup = false;
			} else {
				physicsPickup = false;
			}

			//THROWING BOX SYSTEM
			if (carryingBox && physicsThrow) {
				ThrowBoxClient();
				physicsThrow = false;
			} else {
				physicsThrow = false;
			}
		} else { //IPHONE CODE FOR PLAYER INTERACTIONS
			if (IPhoneMovement) {
				if (IPhoneMovement > 0.0 && !stuckOnLeft) {
					GetComponent.<Rigidbody2D>().velocity.x = IPhoneMovement * moveSpeed;
				} else if (IPhoneMovement < 0.0 && !stuckOnRight) {
					GetComponent.<Rigidbody2D>().velocity.x = IPhoneMovement * moveSpeed;
				}
				var IPhoneNewXPos = transform.position.x;

				GetComponent.<NetworkView>().RPC('SimulateXPosition', RPCMode.Others, Input.GetAxis('Horizontal') * moveSpeed, IPhoneNewXPos);
			} else {
				IPhoneLerpTimer = 0.0;
				GetComponent.<Rigidbody2D>().velocity.x = 0.0;
			}

			if (IPhoneJump > 0.0 && isGrounded && GetComponent.<Rigidbody2D>().velocity.y <= 0) { //IF PLAYER IS JUMPING
				isGrounded = false;
				GetComponent.<Rigidbody2D>().AddForce(Vector2(0, jumpForce));

				var IPhoneNewYPos = transform.position.y;
				GetComponent.<NetworkView>().RPC('SimulateYPosition', RPCMode.Others, jumpForce, IPhoneNewYPos);

				thisSoundPlayer.ClientNoise(1, 0.6, 0.25, true); //PLAY THE JUMPING NOISE ACROSS THE NETWORK
			}
		}
	}

	//CHANGE THE ORIENTATION OF THE SERVER'S PLAYER BASED ON THE VELOCITY (FOR DETECTING PICKUPS/THROWS)
	if (GetComponent.<Rigidbody2D>().velocity.x > 0.0) {
		facingRight = true;
	} else if (GetComponent.<Rigidbody2D>().velocity.x < 0.0) {
		facingRight = false;
	}

	//CHANGE THE GRAVITY BASED ON THE VELOCITY OF THE PLAYER
	if (GetComponent.<Rigidbody2D>().velocity.y > 10) { //IN THE AIR WHILE JUMPING
		GetComponent.<Rigidbody2D>().gravityScale = jumpGravity;
	} else if (GetComponent.<Rigidbody2D>().velocity.y < 10) { //IN THE AIR FALLING AFTER JUMPING
		GetComponent.<Rigidbody2D>().gravityScale = afterJumpGravity;
	}
}

function Update() {
	//IF CARRYING A BOX, ORIENT IT AROUND THE PLAYER BASED ON ORIENTATION
	if (carryingBox) {
		if (facingRight) { //ORIENTATE RIGHT
			carryingBox.localPosition.x = 1.1;
			carryingBox.localPosition.y = 0;
		} else { //ORIENTATE LEFT
			carryingBox.localPosition.x = -1.1;
			carryingBox.localPosition.y = 0;
		}
	}
	if (GetComponent.<NetworkView>().isMine) { //IF THIS IS YOUR PLAYER
		//HANDLE THE INPUTS IN UPDATE() TO NOT MISS ANY FRAMES
		if (Input.GetButton('Jump') && isGrounded && GetComponent.<Rigidbody2D>().velocity.y <= 5) { //ONLY ALLOW JUMPING IF THE PLAYER IS GROUNDED
			physicsJumping = true; //SEND TO FIXEDUPDATE() TO HANDLE PHYSICS
		}
		if (Input.GetButtonDown('Pickup')) {
			physicsPickup = true; //SEND TO FIXEDUPDATE() TO HANDLE PHYSICS
		}
		if (Input.GetButtonDown('Throw')) {
			physicsThrow = true; //SEND TO FIXEDUPDATE() TO HANDLE PHYSICS
		}
	}
	if (isGrounded) { //ON THE GROUND
		GetComponent.<Rigidbody2D>().gravityScale = jumpGravity;
	}
}

//THROW THE BOX YOU ARE CURRENTLY HOLDING FIRST ON THE CLIENT, THEN SEND TO OTHER CLIENTS
function ThrowBoxClient() {
	var boxMovementScriptComp = carryingBox.GetComponent(BoxMovement);

	//TEMPORARY CACHES OF THE DATA (TO MAKE SURE THIS & OTHER CLIENTS ARE THE SAME)
	var tempFacingRight = facingRight;

	stuckOnLeft = false;
	stuckOnRight = false;

	boxMovementScriptComp.isPickedUp = false;
	boxMovementScriptComp.isThrown = true;
	boxMovementScriptComp.thrownRight = tempFacingRight;
	boxMovementScriptComp.playerOwnerID = GetComponent.<NetworkView>().viewID;

	GetComponent.<NetworkView>().RPC('ThrowBoxRPC', RPCMode.Others, tempFacingRight);
	carryingBox = null;
}

@RPC
function ThrowBoxRPC(facing:boolean, info:NetworkMessageInfo) {
	var theChar = info.networkView.GetComponent(PlayerMovement);
	var theBox = theChar.carryingBox.GetComponent(BoxMovement);

	//SET THE BOX TO BE THROWN
	theBox.isPickedUp = false;
	theBox.isThrown = true;
	theBox.thrownRight = facing;
	theBox.playerOwnerID = info.networkView.viewID;

	theChar.carryingBox = null;
}

//DETECT THE NEAREST BOX THE PICKUP
function DetectNearestBox() {
	var raycastDirection: Vector2;
	if (facingRight) {
		raycastDirection = Vector2.right;
	} else {
		raycastDirection = -Vector2.right;
	}

	var raycastHit: RaycastHit2D = Physics2D.Raycast(transform.position, raycastDirection, 1.0, 1 << LayerMask.NameToLayer('Falling Box'));

	if (raycastHit) { //IF RAYCAST HITS A FALLING BOX
		if (raycastHit.rigidbody.isKinematic) { //IF THE BOX IS SITTING
			PickupBoxClient(raycastHit.transform);
		}
	}
}

//PICKUP THE BOX ON THE CLIENT FIRST, THEN SEND TO OTHER CLIENTS
function PickupBoxClient(boxToPickup: Transform) {
	carryingBox = boxToPickup; //SET THE CARRYINGBOX VARIABLE SO THE PLAYER KNOWS WHICH BOX IS THEIRS
	var carryingBoxMovement = carryingBox.GetComponent(BoxMovement); //CACHE TO ONLY MAKE ON GETCOMPONENT CALL

	carryingBoxMovement.isThrown = false;
	carryingBoxMovement.isPickedUp = true;
	carryingBoxMovement.playerOwnerID = GetComponent.<NetworkView>().viewID;

	stuckOnLeft = false;
	stuckOnRight = false;
	carryingBox.GetComponent.<Rigidbody2D>().isKinematic = false; //SET KINEMATIC SO THE PLAYER DOESN'T GET AFFECTED BY THE BOX
	carryingBox.GetComponent.<Collider2D>().isTrigger = true;
	carryingBox.transform.parent = transform; //SET TO BE PARENTED TO THE PLAYER FOR EASY MOVEMENT

	GetComponent.<NetworkView>().RPC('PickupBoxRPC', RPCMode.Others, boxToPickup.GetComponent.<NetworkView>().viewID);
}

@RPC
function PickupBoxRPC(boxViewID: NetworkViewID, info:NetworkMessageInfo) {
	var theChar = info.networkView.transform; //THE PLAY THAT PICKED UP THE BOX
	var theBox = NetworkView.Find(boxViewID).transform; //THE BOX THAT THE PLAYER PICKED UP

	theChar.GetComponent(PlayerMovement).carryingBox = theBox;
	theBox.GetComponent(BoxMovement).isThrown = false; //MAKE SURE ISTHROWN IS NOT TRUE
	theBox.GetComponent(BoxMovement).isPickedUp = true; //MAKE SURE ISPICKEDUP IS TRUE
	theBox.GetComponent(BoxMovement).playerOwnerID = info.networkView.viewID; //FOR DETECTING COLLISIONS

	theBox.GetComponent.<Rigidbody2D>().isKinematic = false; //SET KINEMATIC SO THE PLAYER DOESN'T GET AFFECTED BY THE BOX
	carryingBox.GetComponent.<Collider2D>().isTrigger = true;
	theBox.transform.parent = theChar; //SET TO BE PARENTED TO THE PLAYER FOR EASY MOVEMENT
}

//WHEN A NEW PLAYER CONNECTS, THE SERVER SHOULD SEND ALL PLAYER'S LOCATIONS TO THE NEW PLAYER
function OnPlayerConnected(conPlayer: NetworkPlayer) {
	var allCharacters: GameObject[] = GameObject.FindGameObjectsWithTag('Player');

	if (allCharacters.Length > 0) {
		for (var i=0;i<allCharacters.Length;i++) { //SEND EACH PLAYER'S LOCATION TO THE NEW PLAYER
			GetComponent.<NetworkView>().RPC('SendCharPosition', conPlayer, allCharacters[i].GetComponent.<NetworkView>().viewID, allCharacters[i].transform.position);
			if (allCharacters[i].GetComponent(PlayerMovement).carryingBox) { //IF THIS CHARACTER IS CARRYING A BOX
				GetComponent.<NetworkView>().RPC('PickupBoxRPC', conPlayer, allCharacters[i].GetComponent.<NetworkView>().viewID, allCharacters[i].GetComponent(PlayerMovement).carryingBox.GetComponent.<NetworkView>().viewID);
			}
		};
	}
}

//WHEN A NEW PLAYER JOINS, THE SERVER SENDS THEM ALL OTHER ALREADY-CONNECTED PLAYERS LOCATIONS
@RPC
function SendCharPosition(charViewID: NetworkViewID, charPos: Vector3) {
	NetworkView.Find(charViewID).transform.position = charPos;
}

//NEED TO DESCRIBE THE THINGS THE TWO FUNCTIONS BELOW DO
@RPC
function SimulateYPosition(yJumpForce: float, thatClientYPos:float, info:NetworkMessageInfo) {
	var thatPlayer = info.networkView.transform;

	thatPlayer.GetComponent.<Rigidbody2D>().AddForce(Vector2(0, yJumpForce));
	var newYPosition = thatPlayer.transform.position.y;

	if (newYPosition > (thatClientYPos+1) || newYPosition < (thatClientYPos-1)) {
		thatPlayer.transform.position.y = thatClientYPos;
	}
}

@RPC
function SimulateXPosition(xVelocity:float, thatClientXPos:float, info:NetworkMessageInfo) {
	var thisPlayer = info.networkView.transform;

	thisPlayer.GetComponent.<Rigidbody2D>().velocity.x = xVelocity;
	var newPosition = thisPlayer.transform.position.x;

	//Debug.Log('Send location: '+thatClientPos.x+'\nThis location:'+newPosition.x);
	if (newPosition > (thatClientXPos+1.5) || newPosition < (thatClientXPos-1.5)) {
		thisPlayer.transform.position.x = thatClientXPos;
	}
}