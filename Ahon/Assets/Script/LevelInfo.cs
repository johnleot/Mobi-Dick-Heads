using UnityEngine;
using System.Collections;

public class LevelInfo : MonoBehaviour {

	public void OnClickPlay()
	{
		Application.LoadLevel ("Game_Level");
	}
	
	public void OnClickCancel()
	{
		Application.LoadLevel("Main");
	}
}
