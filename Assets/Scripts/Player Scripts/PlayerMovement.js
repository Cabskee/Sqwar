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
			//EVENTUALLY THIS WILL REMOVE THE PLAYER'S STOCK
			uLink.NetworkView.Get(this).RPC('RemovePlayerStock', uLink.RPCMode.Server, 1);
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

function FixedUpdate() {
	Debug.Log(uLink.NetworkRole.Owner);
	if (uLink.NetworkRole.Owner) {
		if (Application.platform != RuntimePlatform.IPhonePlayer) { //EDITOR/WEBPLAYER CODE FOR PLAYER INTERACTIONS
			//ALL MOVEMENT (LEFT/RIGHT/JUMP) IS HANDLED BELOW
			if (Input.GetAxis('Horizontal')) { //IF HORIZONTAL (LEFT/RIGHT) INPUT IS DETECTED
				if (Input.GetAxis('Horizontal') > 0.0 && !stuckOnLeft) { //IF MOVING RIGHT AND NOT STUCK
					facingRight = true;
					this.GetComponent.<Rigidbody>().velocity.x = Input.GetAxis('Horizontal')*moveSpeed;
					/*
					if (Network.isServer) {
						ServerSimulateHorizontalMovement(networkView.viewID, Input.GetAxis('Horizontal')*moveSpeed);
					} else if (Network.isClient) {
						networkView.RPC('ServerSimulateHorizontalMovement', uLink.RPCMode.Server, networkView.viewID, Input.GetAxis('Horizontal')*moveSpeed);
					}*/
				} else if (Input.GetAxis('Horizontal') < 0.0 && !stuckOnRight) { //IF MOVING LEFT AND NOT STUCK
					facingRight = false;
					this.GetComponent.<Rigidbody>().velocity.x = Input.GetAxis('Horizontal')*moveSpeed;
					/*
					if (Network.isServer) {
						ServerSimulateHorizontalMovement(networkView.viewID, Input.GetAxis('Horizontal')*moveSpeed);
					} else if (Network.isClient) {
						networkView.RPC('ServerSimulateHorizontalMovement', uLink.RPCMode.Server, networkView.viewID, Input.GetAxis('Horizontal')*moveSpeed);
					}*/
				}
			} else { //IF NO HORIZONTAL MOVEMENT (LEFT/RIGHT) IS DETECTED
				this.GetComponent.<Rigidbody>().velocity.x = 0.0; //STOP THE MOVEMENT INSTANTLY
			}

			//JUMPING SCRIPT
			if (physicsJumping) { //IF VERTICAL (JUMPING) INPUT IS DETECTED
				isGrounded = false; //SET THE PLAYER TO NOT BE GROUNDED
				physicsJumping = false; //DON'T ALLOW ANOTHER JUMP
				//rigidbody2D.AddForce(Vector2(0, jumpForce)); //JUMP YOUR CHARACTER

				if (Network.isServer) {
					ServerSimulateJump(GetComponent.<NetworkView>().viewID);
				} else {
					//uLink.NetworkView.Get(this).RPC('RemovePlayerStock', uLink.RPCMode.Server, 1);
				}

				thisSoundPlayer.ClientNoise(1, 0.6, 0.25, true); //PLAY THE JUMPING NOISE FOR THIS PLAYER ACROSS ALL CLIENTS
			}

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

				//networkView.RPC('SimulateXPosition', uLink.RPCMode.Others, Input.GetAxis('Horizontal') * moveSpeed, IPhoneNewXPos);
			} else {
				GetComponent.<Rigidbody2D>().velocity.x = 0.0;
			}

			if (IPhoneJump > 0.0 && isGrounded && GetComponent.<Rigidbody2D>().velocity.y <= 0) { //IF PLAYER IS JUMPING
				isGrounded = false;
				GetComponent.<Rigidbody2D>().AddForce(Vector2(0, jumpForce));

				var IPhoneNewYPos = transform.position.y;
				//networkView.RPC('SimulateYPosition', uLink.RPCMode.Others, jumpForce, IPhoneNewYPos);

				thisSoundPlayer.ClientNoise(1, 0.6, 0.25, true); //PLAY THE JUMPING NOISE ACROSS THE NETWORK
			}
		}
	}

	if (Network.isServer) {
		//CHANGE THE ORIENTATION OF THE SERVER'S PLAYER BASED ON THE VELOCITY (FOR DETECTING PICKUPS/THROWS)
		if (GetComponent.<Rigidbody2D>().velocity.x > 0.0) {
			//networkView.RPC('ChangePlayerOrientation', uLink.RPCMode.Others, true);
			facingRight = true;
		} else if (GetComponent.<Rigidbody2D>().velocity.x < 0.0) {
			//networkView.RPC('ChangePlayerOrientation', uLink.RPCMode.Others, false);
			facingRight = false;
		}
	}

	/*
	//CHANGE THE GRAVITY BASED ON THE VELOCITY OF THE PLAYER
	if (this.rigidbody.velocity.y > 10) { //IN THE AIR WHILE JUMPING
		this.rigidbody.gravityScale = jumpGravity;
	} else if (this.rigidbody.velocity.y < 10) { //IN THE AIR FALLING AFTER JUMPING
		this.rigidbody.gravityScale = afterJumpGravity;
	}*/
}

@RPC
function ServerSimulateHorizontalMovement(clientViewID:NetworkViewID, input:float) {
	var clientToMove: Transform = NetworkView.Find(clientViewID).transform;

	clientToMove.GetComponent.<Rigidbody2D>().velocity.x = input;
	yield WaitForFixedUpdate();
	
	//networkView.RPC('SendClientHorizontalMovement', uLink.RPCMode.Others, clientViewID, clientToMove.position.x);
}

@System.NonSerialized
var newXPosition: float;

@RPC
function SendClientHorizontalMovement(playerViewID:NetworkViewID, xPos:float) {
	var playerToMove = NetworkView.Find(playerViewID).transform;

	playerToMove.GetComponent(PlayerMovement).newXPosition = xPos;
}

@RPC
function ServerSimulateJump(viewID:NetworkViewID) {
	var thePlayer = NetworkView.Find(viewID).GetComponent.<Rigidbody2D>();

	thePlayer.AddForce(Vector2(0, jumpForce));
	if (Network.isServer) {
		//networkView.RPC('ServerSimulateJump', uLink.RPCMode.Others, jumpForce);
	}
}

function Update() {
	if (newXPosition) {
		transform.position.x = newXPosition;
	}
	//IF CARRYING A BOX, ORIENT IT AROUND THE PLAYER BASED ON THAT PLAYER'S ORIENTATION
	if (carryingBox) {
		if (facingRight) { //ORIENTATE RIGHT
			carryingBox.localPosition.x = 1.1;
			carryingBox.localPosition.y = 0;
		} else { //ORIENTATE LEFT
			carryingBox.localPosition.x = -1.1;
			carryingBox.localPosition.y = 0;
		}
	}
	if (uLink.NetworkRole.Owner) { //IF THIS IS YOUR PLAYER
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

	uLink.NetworkView.Get(this).RPC('ThrowBoxRPC', uLink.RPCMode.Others, tempFacingRight);
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

	uLink.NetworkView.Get(this).RPC('PickupBoxRPC', uLink.RPCMode.Others, boxToPickup.GetComponent.<NetworkView>().viewID);
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
			//networkView.RPC('SendCharPosition', conPlayer, allCharacters[i].networkView.viewID, allCharacters[i].transform.position);
			if (allCharacters[i].GetComponent(PlayerMovement).carryingBox) { //IF THIS CHARACTER IS CARRYING A BOX
				//networkView.RPC('PickupBoxRPC', conPlayer, allCharacters[i].networkView.viewID, allCharacters[i].GetComponent(PlayerMovement).carryingBox.networkView.viewID);
			}
		};
	}
}

//SENT FROM THE SERVER (BASED ON VELOCITY) TO ALL CLIENTS TO UPDATE THE DIRECTION THEY ARE FACING
@RPC
function ChangePlayerOrientation(isFacingRight:boolean, info:NetworkMessageInfo) {
	var sendPlayer = info.networkView.GetComponent(PlayerMovement);

	sendPlayer.facingRight = isFacingRight;
}

//WHEN A NEW PLAYER JOINS, THE SERVER SENDS THEM ALL OTHER ALREADY-CONNECTED PLAYERS LOCATIONS
@RPC
function SendCharPosition(charViewID: NetworkViewID, charPos: Vector3) {
	NetworkView.Find(charViewID).transform.position = charPos;
}