#pragma strict

var soundsFX: AudioClip[]; //ARRAY OF ALL THE CLIPS THE PLAYER WILL USE
var useGlobalSound: boolean; //WHETHER TO PLAY ON LOCAL OBJECT OR THROUGH THE GLOBAL AUDIOSOURCE
var isTesting: boolean; //THIS IS TRUE WHEN TESTING, AND I DON'T WANT A MILLION AUDIO SOURCES GOING ON

private var audioPlayer: AudioSource; //THIS CHARACTERS'S AUDIOPLAYER

function Awake() {
	if (!useGlobalSound) {
		audioPlayer = GetComponent(AudioSource);
	} else {
		audioPlayer = GameObject.Find('Global Audio Player').GetComponent(AudioSource);
	}

	if (isTesting) {
		audioPlayer.mute = true;
	}
}

private var fadeVolume: float;
function fadeInterval() {
	fadeVolume += 0.25;

	audioPlayer.volume = fadeVolume;

	if (fadeVolume >= 1.0) {
		CancelInvoke('fadeInterval');
		fadeVolume = 0.0;
	}
}

function ClientMusic(noiseID:int, setPitch:float, setVolume:float, setFade:boolean, setLoop:boolean) {
	audioPlayer.pitch = setPitch;
	if (!setFade) {
		audioPlayer.volume = setVolume;
	} else {
		audioPlayer.volume = 0;
	}
	audioPlayer.loop = setLoop;
	//audioPlayer.clip = soundsFX[noiseID];

	audioPlayer.Play();

	if (setFade) {
		InvokeRepeating('fadeInterval', 0.0, 1);
	}
}

function ClientNoise(noiseID:int, setPitch:float, setVolume:float, networkIt:boolean) {
	audioPlayer.pitch = setPitch;
	audioPlayer.PlayOneShot(soundsFX[noiseID], setVolume);

	if (networkIt) {
		GetComponent.<NetworkView>().RPC('RPCNoise', RPCMode.Others, GetComponent.<NetworkView>().viewID, noiseID, setPitch, setVolume);
	}
}

@RPC
function RPCNoise(viewID: NetworkViewID, noiseID:int, setPitch:float, setVolume:float) {
	var thisPlayer = NetworkView.Find(viewID);
	var thisAudioPlayer: AudioSource;

	if (thisPlayer.GetComponent(SoundPlayer).useGlobalSound) {
		thisAudioPlayer = GameObject.Find('Global Audio Player').GetComponent(AudioSource);
	} else {
		thisAudioPlayer = thisPlayer.GetComponent(AudioSource);
	}

	thisAudioPlayer.pitch = setPitch;
	thisAudioPlayer.PlayOneShot(thisPlayer.GetComponent(SoundPlayer).soundsFX[noiseID], setVolume);
}