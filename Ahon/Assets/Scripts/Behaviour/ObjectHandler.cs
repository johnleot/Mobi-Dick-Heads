using UnityEngine;
using System.Collections;
using Assets.Scripts.Behaviour;
public class ObjectHandler : MonoBehaviour
{
	// Use this for initialization
	private Gameplay gameplay;
	private bool  longPressDetected;
	private bool  newTouch;
	private float touchTime;
	Touch touch;
	void Start ()
	{
		gameplay = new Gameplay ();
		longPressDetected = false;
		newTouch = false;
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		Debug.Log("Update");
		if(Input.GetMouseButtonDown(0))
		{
			Debug.Log("GetMouseButton");
			DestroyObject();
		}
		if(Input.touchCount == 1)
		{
			
			if(touch.phase == TouchPhase.Began)
			{
				newTouch = true;
				touchTime = Time.time;
				Debug.Log("touchTime="+touchTime);
			}
			else if(touch.phase == TouchPhase.Stationary)
			{
				if(newTouch == true && Time.time-touchTime>1)
				{
					Debug.Log("longpress detected");
					newTouch = false;
					longPressDetected = true;
					DestroyObject();
					Debug.Log("Destroying Object");
				}
				else if(newTouch == false && longPressDetected == false)
				{
					newTouch = true;
					touchTime = Time.time;
					Debug.Log("touchTime"+touchTime);
				}
			}
			else
			{
				newTouch = true;
				touchTime = Time.time;
				Debug.Log("Reset");
			}
			
			
		}
	}
	
	void DestroyObject()
	{
		gameplay.setPlayerScore(10);
		Destroy(gameObject);
	}
	
}

