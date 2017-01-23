#pragma strict

private var thisParticleSys: ParticleSystem;

function Start () {
	thisParticleSys = GetComponent(ParticleSystem);
}

function Update () {
	if (thisParticleSys.time >= thisParticleSys.duration) {
		Destroy(gameObject);
	}
}