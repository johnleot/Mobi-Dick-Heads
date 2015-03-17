using UnityEngine;
using System.Collections;

public class SettingsMenu : MonoBehaviour
{

	// center main window
	private Rect windowRect = new Rect((Screen.width/2)-100,(Screen.height/2)-60,200,130);
	private Rect innerRect; // inner window

	// hide initially
	private bool showWindow = false;
	private float musicValue = 2.0f;
	private float musicMax = 5.0f;
	public Texture2D close; // close button

	// tabs
	private int settingsToolbar = 0;
	private string[] settingsStrings = new string[]{"Audio","Help","About"};

	// sound effects on off switch
	private int onOff = 0;
	private string[] onOffStrings = new string[]{"ON","OFF"};

	public GUISkin customSkin;

	void OnGUI()
	{

		if (showWindow)
		{
			innerRect = new Rect (windowRect.x,windowRect.y,200,130);
			doWindow();
		}
	}
	
	public void openSettings()
	{
		showWindow = true;
	}

	void doWindow()
	{
		GUILayout.BeginArea(new Rect(windowRect.x,windowRect.y,200,130));
		GUI.Box( new Rect(0, 0, 200, 130), "Settings" );
		settingsToolbar = GUI.Toolbar(new Rect(5,25,175,20),settingsToolbar, settingsStrings);
		GUILayout.EndArea();
		
		GUI.skin = customSkin;
		if (GUI.Button(new Rect(windowRect.x + 180,windowRect.y -20,30,30),close))
			showWindow = false;

		if (settingsToolbar==0)
		{
			GUI.skin = null;
			GUILayout.BeginArea (new Rect (innerRect.x, innerRect.y, 200, 130));
				GUILayout.BeginHorizontal ();
					GUI.Label (new Rect (5, 55, 100, 35), "Music");
					GUI.Label (new Rect (5, 80, 100, 35), "Sound Effects");
				GUILayout.BeginVertical ();
					musicValue = GUI.HorizontalSlider (new Rect (50, 60, 145, 30), musicValue, 0.0f, musicMax);
					AudioListener.volume = musicValue;
					onOff = GUI.Toolbar (new Rect (100, 85, 95, 20), onOff, onOffStrings);
				GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
		} 
		else if (settingsToolbar==1)
		{
			GUI.skin = null;
			GUILayout.BeginArea (new Rect (innerRect.x, innerRect.y, 200, 130));
			GUI.Label (new Rect (5, 55, 190, 90), "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore.");
			GUILayout.EndArea ();
		}
		else
		{
			GUI.skin = null;
			GUILayout.BeginArea(new Rect(innerRect.x,innerRect.y,200,130));
			GUI.Label(new Rect(5, 55, 190, 90), "Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo.");
			GUILayout.EndArea();
		}

	}

}
