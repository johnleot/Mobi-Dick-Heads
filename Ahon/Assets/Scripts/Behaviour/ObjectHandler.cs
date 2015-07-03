using UnityEngine;
using System.Collections;
using Assets.Scripts.Behaviour;

public class ObjectHandler : MonoBehaviour
{
	public enum objectType{
		empty = 0,
		Pabahay,
		NipaHut,
		BlueHouse,
		OrangeHouse,
		CityHall,
		BaranggayHall,
		Church,
		School
	}

	public ObjectHandler.objectType objectType_;
	private bool colliding = false;
	private bool selected = false;

	private IObject object_;

	void Start()
	{
		Debug.Log (gameObject + " object type is: " + objectType_);

		if (objectType_ != objectType.empty) 
		{
			object_ = (IObject)GetComponent(typeof(IObject));
			
			if (object_ == null) 
			{
				Debug.Log ("Getting Object type failed.");
			}
			else
				Debug.Log ("Getting object type instance success. Object: " + object_);
		}else 
			Debug.Log ("Object Type is Empty.");

	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Draggable") 
		{
			colliding = true;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Draggable")
		{
			colliding = false;
		}
	}

	public void showObjectGUI()
	{
		if(object_ != null)
		{
			object_.showUI();
		}
	}

	public void hideObjectGUI()
	{
		if (object_ != null) 
		{
			object_.hideUI();
		}
	}

	public void removeObjectGUI()
	{
		if(object_ != null)
		{
			object_.removeUI();
		}
	}

	public void selectObject()
	{
		selected = true;
	}

	public void deselectObject()
	{
		selected = false;
	}
}

