#pragma strict

var moveSpeed: float; //MOVESPEED OF THE BOX WHILE IT'S BEING THROWN
private var spawnedX: float; //THE X POSITION OF THE BOX WHEN IT SPAWNED (KEEP IT AT THIS UNTIL PICKED UP)
private var numberOfBoxesHit: int; //THE NUMBER OF BOXES THIS BOX HAS BEEN HIT WHILE BEING THROWN
var maxNumberOfHits: int; //THE MAX NUMBER OF BOXES THIS BOX CAN HIT WHILE BEING THROWN BEFORE IT DESTROYS ITSELF
var isPickedUp: boolean; //WHETHER THE BOX HAS BEEN PICKED UP OR NOT
var isThrown: boolean; //WHETHER THE BOX HAS BEEN THROWN OR NOT
var thrownRight: boolean; //THE DIRECTION THAT THE BOX WAS THROWN AT (TRUE = RIGHT, FALSE = LEFT)
var playerOwnerID: NetworkViewID; //THE NETWORKVIEWID OF THE CHARACTER, TO MAKE SURE HE CAN'T KILL HIMSELF

var thisParticleSys: ParticleSystem;
private var thisSoundPlayer: SoundPlayer;

private var destroyTimer: int = 0;

function Awake() {
	thisSoundPlayer = GetComponent(SoundPlayer);
}

function Start () {
	spawnedX = transform.position.x;
}

function OnTriggerExit2D(other:Collider2D) {
	if (Network.isServer) {
		thrownTriggerDetection(other);
	}
}

function OnTriggerEnter2D(other:Collider2D) {
	if (Network.isServer) {
		thrownTriggerDetection(other);
	}
}

function thrownTriggerDetection(other:Collider2D) {
	if (isThrown) { //ONLY DO ANYTHING WITH COLLISIONS IF THIS BOX IS BEING THROWN
		if (transform.position.y < other.transform.position.y+1 && transform.position.y > other.transform.position.y-1) {
			//IF THIS BOX IS WITHIN THE HORIZONTAL RANGE OF THE OTHER BOX, ALLOW THE BOX TO BE HIT
			if (other.transform.CompareTag('Falling Box')) { //IF THIS BOX HITS A BOX WHILE FLYING
				if (other.GetComponent.<Rigidbody2D>().isKinematic) { //IF THE BOX IS GROUNDED
					numberOfBoxesHit++; //INCREASE THE NUMBER OF BOXES THAT HAVE BEEN HIT
					Destroy(other.gameObject);
					GetComponent.<NetworkView>().RPC('DestroyThisBox', RPCMode.OthersBuffered, other.transform.GetComponent.<NetworkView>().viewID);
					thisSoundPlayer.ClientNoise(0, 1.0, 1.0, true); //SEND THE "BOX DESTROYED" NOISE ACROSS THE NETWORK

					if (numberOfBoxesHit >= maxNumberOfHits) { //IF IT'S HIT THE MAX NUMBER OF BOXES
						Destroy(gameObject);
						GetComponent.<NetworkView>().RPC('DestroyThisBox', RPCMode.OthersBuffered, GetComponent.<NetworkView>().viewID);
					};
				}
			}
			if (other.transform.CompareTag('Player')) { //IF THIS BOX COLLIDES WITH A PLAYER WHILE FLYING
				if (other.transform.GetComponent.<NetworkView>().viewID != playerOwnerID) { //IF THE PLAYER IS NOT THE ONE WHO THREW IT
					other.GetComponent.<NetworkView>().RPC('RemovePlayerStock', RPCMode.Server, other.transform.GetComponent.<NetworkView>().viewID, 1);
					other.GetComponent(SoundPlayer).ClientNoise(2, 1.25, 0.6, true);

					GetComponent.<NetworkView>().RPC('DestroyThisBox', RPCMode.OthersBuffered, GetComponent.<NetworkView>().viewID);
					Destroy(gameObject);
				}
			}
		}
	}
}

@RPC
function DestroyThisBox(boxViewID: NetworkViewID) {
	Destroy(NetworkView.Find(boxViewID).gameObject);
}

function OnCollisionEnter2D(other:Collision2D) {
	if (other.collider.CompareTag('Platform')) { //IF THE BOX COLLIDES WITH THE PLATFORM WHILE FALLING
		if (!isPickedUp && !isThrown) { //IF THE BOX IS NOT BEING THROWN/PICKED UP
			if (transform.position.y >= other.transform.position.y) { //IF THIS BOX IS ABOVE THE PLATFORM (ALWAYS SHOULD BE)
				transform.position.x = spawnedX;
				transform.position.y = Mathf.Round(other.transform.position.y+1);
				GetComponent.<Rigidbody2D>().isKinematic = true; //SET IT TO BE GROUNDED, NOT MOVEABLE
				if (Network.isServer) {
					GetComponent.<NetworkView>().RPC('SendThisBoxLocation', RPCMode.Others, GetComponent.<NetworkView>().viewID, transform.position, true);
				}
			}
		}
	}
	if (other.transform.CompareTag('Falling Box') && !GetComponent.<Rigidbody2D>().isKinematic) { //IF THIS BOX COLLIDES WITH ANOTHER BOX WHILE FALLING
		if (!GetComponent.<Collider2D>().isTrigger) { //IF THE BOX THIS BOX IS COLLIDING WITH IS NOT BEING CARRIED
			if (other.collider.GetComponent.<Rigidbody2D>().isKinematic && !other.transform.GetComponent(BoxMovement).isThrown && !other.transform.GetComponent(BoxMovement).isPickedUp) {
				//IF THE BOX THIS BOX COLLIDED WITH IS NOT BEING THROWN/PICKED UP AND IS SITTING ON THE GROUND
				if (transform.position.y >= other.transform.position.y) { //IF THIS BOX IS ABOVE THE OTHER BOX
					transform.position.x = spawnedX;
					transform.position.y = Mathf.Round(other.transform.position.y+1);
					GetComponent.<Rigidbody2D>().isKinematic = true; //SET IT TO BE GROUNDED, NOT MOVEABLE
					if (Network.isServer) {
						GetComponent.<NetworkView>().RPC('SendThisBoxLocation', RPCMode.Others, GetComponent.<NetworkView>().viewID, transform.position, true);
					}
				}
			}
		}
	}
}

function StartTimer() {
	InvokeRepeating('Timer', 0.0, 1.0);
}

function Timer() {
	destroyTimer++;
}

function FixedUpdate() {
	if (!isPickedUp && isThrown) { //IF THE BOX IS BEING THROWN
		if (!IsInvoking('Timer')) { //IF TIMER HAS NOT ALREADY BEEN STARTED
			StartTimer(); //START THE DESTRUCTION TIMER
		}

		transform.parent = null; //SHOULDN'T BE PARENTED TO ANYTHING WHILE BEING THROWN
		GetComponent.<Rigidbody2D>().gravityScale = 0; //SHOULD NOT BE AFFECTED BY GRAVITY (TO KEEP IT THROWN IN A LINE)
		GetComponent.<Rigidbody2D>().isKinematic = true; //SHOULD NOT BE AFFECTED BY GRAVITY (TO KEEP IT THROWN IN A LINE)
		GetComponent.<Collider2D>().isTrigger = true; //SHOULD NOT BE AFFECTED BY HITTING ANY OTHER BLOCK
		gameObject.tag = 'Thrown Box'; //CHANGE IT'S TAG FOR DETECTING REASONS

		//MOVE THE PHYSICS BASED ON DIRECTION PLAYER IS FACING
		if (thrownRight) {
			GetComponent.<Rigidbody2D>().velocity.x = Vector2.right.x * moveSpeed;
		} else {
			GetComponent.<Rigidbody2D>().velocity.x = -Vector2.right.x * moveSpeed;
		}
		transform.position.y = Mathf.Round(transform.position.y); //MAKE SURE THIS BOX GETS THROWN ON THE "GRID"

		if (destroyTimer >= 3) { //DESTROY THE BOX AFTER BEING THROWN FOR 3 SECONDS
			var thisExplosion = Instantiate(thisParticleSys.gameObject, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
	if (!isPickedUp && !isThrown && !GetComponent.<Rigidbody2D>().isKinematic) { //IF THE BOX IS SIMPLY FALLING
		transform.position.x = spawnedX; //DON'T ALLOW IT TO BE "SHOVED" SIDEWAYS
		GetComponent.<Rigidbody2D>().velocity.x = 0.0; //DON'T ALLOW IT TO BE "SHOVED" SIDEWAYS
	}
}

//IF A PLAYER CONNECTS, THIS BOX SHOULD SEND THEM IT'S LOCATION
function OnPlayerConnected(newPlayer:NetworkPlayer) {
	if (isThrown) { //IF THE BOX IS BEING THROWN
		GetComponent.<NetworkView>().RPC('SendThisBoxThrown', newPlayer, transform.position, GetComponent.<NetworkView>().viewID, isThrown, isPickedUp, thrownRight, playerOwnerID);
	} else { //IF THE BOX IS NOT BEING THROWN
		if (!GetComponent.<Rigidbody2D>().isKinematic) { //IF THE BOX IS STILL FALLING
			GetComponent.<NetworkView>().RPC('SendThisBoxLocation', newPlayer, GetComponent.<NetworkView>().viewID, transform.position, false);
		} else { //IF THE BOX IS SITTING ON THE GROUND, POSSIBLY "FLOATING"
			GetComponent.<NetworkView>().RPC('SendThisBoxLocation', newPlayer, GetComponent.<NetworkView>().viewID, transform.position, true);
		}
	}
}

@RPC
function SendThisBoxThrown(startingPos:Vector3, boxViewID:NetworkViewID, isThrown:boolean, isPickedUp:boolean, thrownRight:boolean, ownerViewID:NetworkViewID) {
	var moveComponent = NetworkView.Find(boxViewID).GetComponent(BoxMovement);

	moveComponent.transform.position = startingPos;
	moveComponent.isThrown = isThrown;
	moveComponent.isPickedUp = isPickedUp;
	moveComponent.thrownRight = thrownRight;
	moveComponent.playerOwnerID = ownerViewID;
}

@RPC
function SendThisBoxLocation(boxViewID:NetworkViewID, location:Vector3, sitting:boolean) {
	NetworkView.Find(boxViewID).transform.position = location;
	if (sitting) { //IF THE BOX IS ALREADY SITTING (USED FOR "FLYING" BOXES)
		NetworkView.Find(boxViewID).GetComponent.<Rigidbody2D>().isKinematic = true; //SET KINEMATIC TO TRUE
	}
}