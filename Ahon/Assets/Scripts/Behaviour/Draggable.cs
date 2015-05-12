using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour
{
	private float maxPickingDistance = 1000;
	
	//private Vector3 startPos;
	
	private Transform pickedObject = null;
	
	void Start()
	{
		Debug.Log ("Vector3.up: " + Vector3.up);
		Debug.Log ("Vector3.zero: " + Vector3.zero);
	}
	void Update()
	{
		foreach(Touch touch in Input.touches)
		{
			//Create horizontal plane
			Plane horPlane = new Plane(Vector3.up, -1.0f);
			
			//gets the ray at postion where the screen is touche
			Ray ray = Camera.main.ScreenPointToRay(touch.position);
			
			if(touch.phase == TouchPhase.Began)
			{
				RaycastHit hit = new RaycastHit();
				if(Physics.Raycast(ray, out hit, maxPickingDistance))
				{
					pickedObject = hit.transform;
					//startPos = touch.position;
				}
				else
				{
					pickedObject = null;
				}
			}
			else if (touch.phase ==  TouchPhase.Moved)
			{
				if(pickedObject != null)
				{
					float distance1 = 0f;
					if (horPlane.Raycast(ray, out distance1))
					{
						pickedObject.transform.position = ray.GetPoint(distance1);
					}
				}
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				pickedObject = null;
			}
		}
	}
}
