using UnityEngine;
using System.Collections;

/*
	ABOUT THIS SCRIPT
	
This script makes objects stick to the ground of a grid, similar to how
buildings are placed in stategy games. Objects will always be placed
with the bottom touching the grid. Mouse input is being handled by shooting
a ray from the cursor and letting it hit the grid's collider. If there is
no collider nothing will happen. In this example I used a plane collider on
the grid.

This script demonstrates the snapping feature during runtime and
conversions from world space to grid space and back.

NOTE: Due to popular request this example now also detects when the block
is intersecting with another block. In that case the active block will be
tinted red until you move out. if you let go while intersecting it will
snap back to the last non-intersecting position. This is achieved by using
a child object with a trigger that checks for intersections and reports back
to this script. This is just standard unity physics and has nothing to do
with Grid Framework itself, but it is still a question people ask often.
*/

public class SnappingUnits : MonoBehaviour {

	public GFGrid grid; // the grid we want to snap to
	private Collider gridCollider; // a collider attached to the grid, will be used for handling mouse input
	
	private bool beingDragged; // true while the player is dragging the block around, otherwise false
	private Vector3 oldPosition; // the previous valid position

	private Transform cachedTransform; //cache the transform for performance
	
	void Awake () {
		cachedTransform = transform;
		
		//always make a sanity check
		if(grid){
			gridCollider = grid.gameObject.GetComponent<Collider> ();
			if (gridCollider) {
				//perform an initial align and snap the objects to the bottom
				grid.AlignTransform (cachedTransform);
				cachedTransform.position = CalculateOffsetY ();
			}
		}
		//store the first safe position
		oldPosition = cachedTransform.position;
		// setup the rigidbody for collision and contstruct a trigger
		SetupRigidbody();
		ConstructTrigger();
	}
	
	// these two methods toggle dragging
	void OnMouseDown(){
		beingDragged = true;
	}
	void OnMouseUp(){
		beingDragged = false;
		cachedTransform.position = oldPosition; // place on the last valid position
		_intersecting = 0; // stationary block do not intersect anymore
		TintRed(_intersecting); // set the tint one last time
	}
	
	// use FixedUpdate instead of Update to allow colision detection to catch up with movement
	void FixedUpdate(){
		if(beingDragged){
			//store the position if it is valid
			if(_intersecting == 0)
				oldPosition = cachedTransform.position;
			DragObject();
		}
	}
	
	//this function gets called every frame while the object (its collider) is being dragged with the mouse
	void DragObject(){
		if(!grid || !gridCollider) // if there is no grid or no collider there is nothing more we can do
			return;

		//handle mouse input to convert it to world coordinates
		Vector3 cursorWorldPoint = ShootRay();
    	
		//change the X and Z coordinates according to the cursor (the Y coordinate stays the same after the last step)
		cachedTransform.position = cursorWorldPoint;
		
		//now align the object and snap it to the bottom.
		grid.AlignTransform(cachedTransform);
		cachedTransform.position = CalculateOffsetY(); // this forces the Y-coordinate into position
	}
	
	// makes the object snap to the bottom of the grid, respecting the grid's rotation
	Vector3 CalculateOffsetY(){
		//first store the objects position in grid coordinates
		Vector3 gridPosition = grid.WorldToGrid(cachedTransform.position);
		//then change only the Y coordinate
		gridPosition.y = 0.5f * cachedTransform.lossyScale.y;
		
		//convert the result back to world coordinates
		return grid.GridToWorld(gridPosition);
	}

	// shoots a ray, which can only hit the grid plane, from the mouse cursor via the camera and returns the hit position
	Vector3 ShootRay () {
		RaycastHit hit;
		gridCollider.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity);
		//this is where the player's cursor is pointing towards (if nothing was hit return the current position => no movement)
		return hit.collider != null ? hit.point : cachedTransform.position;
	}


// --------------- the following is just for the red tint when intersecting, it has nothing to do with snapping ---------------

	#region Intersection handling
	public Material defaultMaterial;
	public Material redMaterial;
	//this keeps track of how many other cubes we are intersecting with
	private int _intersecting = 0;

	// this method will be called by the trigger
	public void SetIntersecting(bool intersecting){
		if(!beingDragged) // ignore sitting objects, only moving ones should respond
			return;
		// if true we entered another object, increment the value; if false we exited another object, decrease the value
		_intersecting = intersecting ? _intersecting + 1 : _intersecting - 1;
		//update the colour
		TintRed(_intersecting);
	}

	void TintRed(int intersections){
		if(intersections > 0){
			GetComponent<Renderer>().material = redMaterial;
		} else{
			GetComponent<Renderer>().material = defaultMaterial;
		}
	}

	// set up the rigidbody component for intersection recognition
	private void SetupRigidbody(){
		Rigidbody rb = GetComponent<Rigidbody>(); //the rigidbody component of this object
		if(!rb) // if there is no Rigidbody yet create a new one
			rb = gameObject.AddComponent<Rigidbody>();
		// non-kinematic to allow collision detection, no gravity and all rotations and movement frozen
		rb.isKinematic=false;
		rb.useGravity=false;
		rb.constraints = RigidbodyConstraints.FreezeAll; //prevents physics from moving the object
	}

	// create a child GameObject and add a trigger to it to do the intersection detection
	private void ConstructTrigger(){
		GameObject go = new GameObject();
		go.name = "IntersectionTrigger";
		// attach it to this block and make it exactly the same, except slightly smaller
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero; //exactly at the centre of the actual object
		go.transform.localScale = 0.9f * Vector3.one; //slightly smaller than the actual object
		go.transform.localRotation = Quaternion.identity; // same rotation as the actual object
		// add the same type of collider as the block has and make it a trigger
		Collider col = (Collider) go.AddComponent(GetComponent<Collider>().GetType());
		col.isTrigger = true;
		// attach the script to the collider and connect it to this script
		IntersectionTrigger script = go.AddComponent<IntersectionTrigger>();
		script.SetSnappingScript(this);
	}
	#endregion
}
