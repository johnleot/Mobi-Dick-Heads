using UnityEngine;
using System.Collections;

public class LevelInfo : MonoBehaviour {

	public GameObject infoWindow;

	public void OnClickPlay()
	{
		Application.LoadLevel ("Game_Level");
	}
	
	public void OnClickCancel()
	{
		Application.LoadLevel("Main");
	}

	public void disableInfoWindow()
	{
		infoWindow.SetActive (false);
	}

	public void enableInfoWindow()
	{
		if (!infoWindow.activeInHierarchy)
			infoWindow.SetActive (true);
	}
}
