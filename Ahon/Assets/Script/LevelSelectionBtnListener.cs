using UnityEngine;
using System.Collections;

public class LevelSelectionBtnListener : MonoBehaviour {

	public void onLevelSelected () {
		Application.LoadLevel ("Level_Info");
	}
}
