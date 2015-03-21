using UnityEngine;
using System.Collections;

public class MainMenuBtnEventListener : MonoBehaviour {

	public void onClick(int buttonClicked) {
		switch (buttonClicked) {
		case 1:
			loadLevelSelectionScreen();
			break;
		case 2:
			loadCinematicsScreen();
			break;
		default:
			loadFacebookScreen();
			break;
		}
	}
	
	void loadCinematicsScreen () {
		Application.LoadLevel("Cinematics");
	}
	
	void loadLevelSelectionScreen(){
		Application.LoadLevel ("LevelSelection");
	}
	
	void loadFacebookScreen(){
		Application.LoadLevel ("FacebookScreen");
	}
}
