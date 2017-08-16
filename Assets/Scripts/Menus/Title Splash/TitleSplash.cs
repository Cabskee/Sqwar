using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleSplash: MonoBehaviour {
	public void closeSplash() {
		SceneManager.LoadScene("Menu");
	}
}