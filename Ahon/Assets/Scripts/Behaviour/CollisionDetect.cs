using UnityEngine;
using System.Collections;

public class CollisionDetect : MonoBehaviour {

	public static bool colliding = false;
	void Start()
	{
		//this.transform.FindChild ("Plane1").gameObject.SetActive (false);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Draggable") 
		{
			//TouchControl.selectedObject.transform.FindChild ("Plane2").gameObject.SetActive(false);
			//TouchControl.selectedObject.transform.FindChild ("Plane1").gameObject.SetActive(true);
			//this.transform.FindChild ("Plane1").gameObject.SetActive (true);
			colliding = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Draggable")
		{
			//TouchControl.selectedObject.transform.FindChild ("Plane2").gameObject.SetActive(true);
			//TouchControl.selectedObject.transform.FindChild ("Plane1").gameObject.SetActive(false);
			colliding = false;
		}
	}
}
