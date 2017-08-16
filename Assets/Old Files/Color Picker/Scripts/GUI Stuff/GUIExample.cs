using UnityEngine;
using System.Collections;

public class GUIExample : MonoBehaviour {
	
	public GameObject cube;
	
	// Use this for initialization
	void Start () {
		ColorPicker.OnColorPickedEvent += new ColorPicker.OnColorPickedHandler( OnPickColor );
	}
	
	void OnGUI(){
		if(GUILayout.RepeatButton("Choose color"))
		{
			ColorPicker.PickColor = true;
		}
	}
	
	void OnPickColor(Color c){
		Debug.Log(c);
		
		cube.GetComponent<Renderer>().material.color = c;		
	}
}
