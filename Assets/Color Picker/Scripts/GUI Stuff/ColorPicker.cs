using UnityEngine;
using System;
using System.Collections;

using System.Runtime.CompilerServices;

public class ColorPicker : MonoBehaviour {
	public Texture2D colorPicker;
	public Texture2D colorPickerFlipped;
	public int ImageWidth = 256;
	public int ImageHeight = 256;
	
	public delegate void OnColorPickedHandler(Color c);
    private OnColorPickedHandler m_onColorPickedEvent= delegate{ };
    
	private static ColorPicker instance;
    
    public static ColorPicker Instance{
        get{
            if (instance == null)
                instance = new GameObject ("ColorPicker").AddComponent<ColorPicker> ();
 
            return instance;
        }
    }
	
    public static event OnColorPickedHandler OnColorPickedEvent
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            Instance.m_onColorPickedEvent -= value;
            Instance.m_onColorPickedEvent += value;
        }
         
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            Instance.m_onColorPickedEvent -= value;   
        }
    }
 
	public void OnColorPicked(Color c){
	    m_onColorPickedEvent(c);
	}
	
	bool m_pickColor;
	
	public static bool PickColor {
		get {
			return Instance.m_pickColor;
		}
		set {
			Instance.m_pickColor = value;
		}
	}	
	
	#region Monobehaviours
	
	public void Awake(){
      instance = this;
    }
    
    public void OnApplicationQuit (){
        instance = null;
    }
	
	public float locX = 10;
	public float locY = 10;
	
	public GUISkin guiSkin;
	
	void OnGUI ()
	{
		GUI.skin = guiSkin;
		
		if (m_pickColor && GUI.RepeatButton (new Rect (locX, locY, ImageWidth, ImageHeight), colorPicker, "Color Picker")) {
			Vector2 pickpos = Event.current.mousePosition;
			int aaa = Convert.ToInt32 (pickpos.x - locX);
			int bbb = Convert.ToInt32 ( pickpos.y - locY);
			
			
			//Debug.Log(pickpos.x - locX - 20);
			//Debug.Log(pickpos.y - locY);
			//Debug.Log (aaa + "," + bbb);
			
			Color col = colorPickerFlipped.GetPixel (aaa, bbb);
				
			// "col" is the color value that Unity is returning.
			// Here you would do something with this color value, like
			// set a model's material tint value to this color to have it change
			// colors, etc, etc.

				OnColorPicked (col);
			
				if (Input.GetMouseButtonUp (0)) {
				    m_pickColor = false;
					m_onColorPickedEvent = null; //Clear subscribed events
			    }
		}
	}
	
	#endregion
}