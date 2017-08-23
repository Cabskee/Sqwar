using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Constants {
	public static class Constant {
		public const string LAYER_PLAYER = "Player";
		public const string LAYER_BOUNDARY = "Boundary";
		public const string LAYER_PLATFORM = "Platform";
		public const string LAYER_FALLINGBLOCK = "Falling Block";
		public const string LAYER_SHOOTINGBLOCK = "Shooting Block";

		// For playing shooting
		public enum FacingDirection { Left, Right, Up, Down };
	}
}