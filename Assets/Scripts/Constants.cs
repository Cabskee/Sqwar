using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Constants {
	public static class Constant {
		// LAYERS
		public const string LAYER_PLAYER = "Player";
		public const string LAYER_BOUNDARY = "Boundary";
		public const string LAYER_PLATFORM = "Platform";
		public const string LAYER_PLACEDBLOCK = "Placed Block";
		public const string LAYER_FALLINGBLOCK = "Falling Block";
		public const string LAYER_SHOOTINGBLOCK = "Shooting Block";

		// TAGS
		public const string TAG_FALLINGBLOCK = "Falling Block";

		// Direction for shooting
		public enum FacingDirection { Left, Right, Up, Down };

		// Player States
		public enum PlayerState { Alive, Dead, Invulnerable };
	}
}