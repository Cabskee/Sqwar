using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMovement: MonoBehaviour {
	public Transform cameraTarget;
	public float cameraSpeed;
	Menu targetMenu;
	bool hasArrived = false;

	public List<Menu> menus = new List<Menu>();
	public const string MENU_MAINMENU = "Main Menu";
	public const string MENU_LEADERBOARDS = "Leaderboards";
	public const string MENU_SETTINGS = "Settings";
	public const string MENU_CREDITS = "Credits";
	public const string MENU_HOSTGAME = "Host Game";
	public const string MENU_JOINGAME = "Join Game";
	public const string MENU_CUSTOMIZATION = "Customization";

	void Awake() {
		// Hide all menus
		hideAllMenus();
	}

	void Start () {
		animateToMenu(MENU_MAINMENU);
	}

	void animateToMenu(string menuName) {
		Menu newMenu = menuByName(menuName);
		newMenu.toggleMenu(true);
		targetMenu = newMenu;
	}

	Menu menuByName(string name) {
		return menus.Find(delegate(Menu obj) {
			return obj.name == name;
		});
	}

	void hideAllMenus() {
		menus.ForEach(delegate(Menu menu) {
			menu.toggleMenu(false);
		});
	}
	void hideAllMenus(string name) {
		menus.ForEach(delegate(Menu menu) {
			if (menu.name != name) {
				menu.toggleMenu(false);
			}
		});
	}

	void Update() {
		if (Vector2.Distance(cameraTarget.position, targetMenu.cameraPosition) > 2.5f) {
			cameraTarget.position = Vector2.Lerp(cameraTarget.position, targetMenu.cameraPosition, cameraSpeed*Time.deltaTime);
			hasArrived = false;
		} else {
			if (!hasArrived) {
				hasArrived = true;
				hideAllMenus(targetMenu.name);
			}
		}
	}
}

[System.Serializable]
public class Menu {
	public string name;
	public Vector2 cameraPosition;
	public RectTransform canvas;

	public void toggleMenu(bool enabled) {
		canvas.gameObject.SetActive(enabled);
	}
}