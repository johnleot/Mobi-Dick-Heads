using UnityEngine;
using System.Collections;
using Assets.Scripts.Behaviour;
using UnityEngine.UI;

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
		School,
		EvacuationCenter,
		Palay,
        Terrain
	}
    private Canvas actionCanvas;
    private Image actionCanvasImg;
    private Text actionCanvasNoOcc;
	public ObjectHandler.objectType objectType_;
	private bool colliding = false;
	private Vector3 originalPosition;
	private bool selected = false;

	private IObject object_;

	void Awake()
	{
		originalPosition = gameObject.transform.position;
	}

	void Start()
	{
        actionCanvas = GameObject.Find("ActionCanvas").GetComponent<Canvas>();
        actionCanvasImg = GameObject.Find("ActionCanvasImg").GetComponent<Image>();
        actionCanvasNoOcc =  GameObject.Find("NoOcc").GetComponent<Text>();
		//Debug.Log (gameObject + " object type is: " + objectType_);
        actionCanvas.enabled = false;
		if (objectType_ != objectType.empty) 
		{
			object_ = (IObject)GetComponent(typeof(IObject));
			
			if (object_ == null) 
			{
				//Debug.Log ("Getting Object type failed.");
			}
			//else
				//Debug.Log ("Getting object type instance success. Object: " + object_);
		}else 
			Debug.Log ("Object Type is Empty.");

	}

	void OnTriggerEnter(Collider other)
	{
        //Debug.Log("entering " + other.gameObject.tag);
		if (other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Untagged") 
		{
			colliding = true;
		}
        if (other.gameObject.tag == "Building")
        {
            Destroy(gameObject);
        }
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "Obstacle" || other.gameObject.tag == "Untagged")
		{
			colliding = false;
		}
	}

	public void insertObjectGUI()
	{
		if(object_ != null)
		{
			object_.insertUI();
		}
	}
	
	public void showObjectGUI()
	{
		if (object_ != null) 
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

	
	public bool Colliding {
		get {
			return colliding;
		}
		set {
			colliding = value;
		}
	}

	public void resetPosition ()
	{
		//throw new System.NotImplementedException ();
		gameObject.transform.position = originalPosition;
	}

	public Vector3 OriginalPosition {
		get {
			return originalPosition;
		}
		set {
			originalPosition = value;
		}
	}
    
    public void OnMouseDown()
    {
        string obj = this.gameObject.transform.name;
        string name = obj.Split(',')[0];
        string noOcc = obj.Split(',')[1];
        if (name == "Nipahut" ||
            name == "Pabahay" ||
            name == "Blue House" ||
            name == "Orange House")
        {
            actionCanvas.enabled = true;
            actionCanvasNoOcc.text = noOcc;
        }
        else
        {
            actionCanvas.enabled = false; 
        }
    }

    public void FeedPopulation(int value)
    {

    }

    public void EvacuatePopulation(int value)
    {

    }

    public void HideActionCanvas()
    {
        actionCanvas.enabled = false; 
    }

}

