using UnityEngine;
using System.Collections;
using Assets.Scripts.Behaviour;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class TouchControl : MonoBehaviour {
	
	public float moveSensitivityX = 20.0f;
	public float moveSensitivityY = 20.0f;
	public bool updateZoomSensitivity = true;
	public float orthoZoomspeed = 0.05f;
	public float minZoom = 1.0f; // zoomest in
	public float maxZoom = 20.0f; // zoomest out
	public bool invertMoveX = false;
	public bool invertMoveY = false;
	public float mapWidth = 40.0f;
	public float mapHeight = 40.0f;
	public Camera _camera = null;
	public Transform pickedObject = null;
	private float horizontalExtent, verticalExtent;
	private float minX, maxX, minY, maxY, minZ, maxZ;
	private float maxPickingDistance = 1000;
	private Vector3 cameraInitialPosition;
	float highestX = 169f;
	float highestY = 103f;

	private Gameplay gameplay;
	
	private bool  longPressDetected;
	private bool  newTouch;
	private float touchTime;

	public GameObject gui; // use for accessing Gameplay Script

	private ObjectHandler objectHandler_;

	void Start () {
		gameplay = gui.GetComponent<Gameplay> ();
		if (gameplay) {
			Debug.Log ("Success Getting gameplay !");
		} else {
			Debug.Log ("Failed getting gamplay instance. ");
		}
		cameraInitialPosition = _camera.transform.position;
//		maxZoom = 0.5f * (mapWidth / _camera.aspect);
//
//		if (mapWidth > mapHeight)
//			maxZoom = 0.5f * mapHeight;
		//Debug.Log ("MaxZOOM : " + maxZoom);
//		_camera.orthographicSize = maxZoom; // initial camera zoom.
		CalculateMapBounds ();
	}
	
	void FixedUpdate () {
		if (updateZoomSensitivity) {
			moveSensitivityX = _camera.orthographicSize / 5.0f;
			moveSensitivityY = _camera.orthographicSize / 5.0f;
		
			Touch[] touches = Input.touches;

			Plane horPlane;
			if (pickedObject) {
				float height = Terrain.activeTerrain.SampleHeight (pickedObject.position) + 2.25f;
				//Debug for TerrainObjects elevation.
				//Debug.Log ("Terrain y Position = " + Terrain.activeTerrain.transform.position.y);
				//Debug.Log ("pickedObject.position.y = " + pickedObject.position.y);
				//Debug.Log ("Height = " + height);
				horPlane = new Plane (Vector3.up, -height);
			} else {
				horPlane = new Plane (Vector3.up, -10.0f);
			}
		
			if (touches.Length > 0) {
				Ray ray = _camera.ScreenPointToRay (touches [0].position);
				RaycastHit hit;
			
				if(!IsPointerOverUIObject(touches[0].position))
				{
					if (Physics.Raycast (ray, out hit) && hit.collider.tag == "Building")
					{
						if (Physics.Raycast (ray, out hit, maxPickingDistance)) {
							if (!pickedObject) 
							{
								touchTime = Time.time;
								pickedObject = hit.transform;
								selectGameObject ();
							}
							else
							{
								if (hit.transform.gameObject == pickedObject.gameObject ||
								    hit.transform.gameObject != pickedObject.gameObject) 
								{
									deselectGameObject ();
								}
							}
							
						} else {
							deselectGameObject ();
							pickedObject = null;
						}
					}
					else if (Physics.Raycast (ray, out hit) && hit.collider.tag == "Draggable") {
						if (touches [0].phase == TouchPhase.Began) {
							if (Physics.Raycast (ray, out hit, maxPickingDistance)) {

								pickedObject = hit.transform;
								if(isSelectedObjectHasObjectHandler())
								{
									objectHandler_ = pickedObject.GetComponent<ObjectHandler>();
									if(objectHandler_.objectType_ != ObjectHandler.objectType.empty)
									{
										objectHandler_.OriginalPosition = pickedObject.transform.position;
									}
								}
								//selectGameObject ();

							} else {
								//deselectGameObject ();
								pickedObject = null;
							}
						} else if (touches [0].phase == TouchPhase.Moved) {
							if (pickedObject != null) {

								//hideSelectedObjetUI (pickedObject);

								float distance1 = 0f;
								if (horPlane.Raycast (ray, out distance1)) {
									pickedObject.transform.position = ray.GetPoint (distance1);
								}
							}
						} else if (touches [0].phase == TouchPhase.Ended) {
							//showSelectedObjectUI ();
							// check if position is not overlap with other object
							//pickedObject = null;
						}
					} 
					else {

						if (touches [0].phase == TouchPhase.Moved) {
							Vector2 delta = touches [0].deltaPosition;
							float positionX = delta.x * moveSensitivityX * Time.fixedDeltaTime;
							positionX = invertMoveX ? positionX : positionX * -1;
						
							float positionY = delta.y * moveSensitivityY * Time.fixedDeltaTime;
							positionY = invertMoveY ? positionY : positionY * -1;
						
							_camera.transform.position += new Vector3 (positionX, positionY, -positionX);
						}
					}
				
					if (touches.Length == 2) {
						Vector2 cameraViewSize = new Vector2 (_camera.pixelWidth, _camera.pixelHeight);
					
						Touch touchOne = touches [0];
						Touch touchTwo = touches [1];
					
						Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
						Vector2 touchTwoPrevPos = touchTwo.position - touchTwo.deltaPosition;
					
						float prevTouchDeltaMag = (touchOnePrevPos - touchTwoPrevPos).magnitude;
						float touchDeltaMag = (touchOne.position - touchTwo.position).magnitude;
					
						float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;
					
						_camera.transform.position += _camera.transform.TransformDirection ((touchOnePrevPos + touchTwoPrevPos - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);
					
						_camera.orthographicSize += deltaMagDiff * orthoZoomspeed;
						_camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize, minZoom, maxZoom); //- 0.001f;
					
						_camera.transform.position -= _camera.transform.TransformDirection ((touchOne.position + touchTwo.position - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);

						CalculateMapBounds ();
					}
				}
			}
		}
	}

	void selectGameObject ()
	{
		if (isSelectedObjectHasObjectHandler ()) {
			objectHandler_ = pickedObject.GetComponent<ObjectHandler> ();
			if (objectHandler_.objectType_ != ObjectHandler.objectType.empty) {
				objectHandler_.insertObjectGUI ();
			}
		}
	}
	
	void CalculateMapBounds()
	{
		float xPerOrthoSize;
		float yPerOrthoSize;
		
		xPerOrthoSize = (highestX - cameraInitialPosition.x) / (maxZoom);
		yPerOrthoSize = (highestY - cameraInitialPosition.y) / (maxZoom);
		
		Debug.Log ("xPerOrthoSize: " + xPerOrthoSize + " yPerOrthoSize: " + yPerOrthoSize);
		
		minX = cameraInitialPosition.x - (xPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		maxX = cameraInitialPosition.x + (xPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		
		minY = cameraInitialPosition.y - (yPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		maxY = cameraInitialPosition.y + (yPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		
		minZ = cameraInitialPosition.z - (xPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		maxZ = cameraInitialPosition.z + (xPerOrthoSize * maxZoom - _camera.orthographicSize + 1);
		
		Debug.Log ("minX:" + minX + " maxX:" + maxX + "-minY:" + minY + " maxY:" + maxY + "-minZ:" + minZ + " maxZ:" + maxZ);
	}
	
	void LateUpdate()
	{
		Vector3 limitedCameraPosition = _camera.transform.position;
		limitedCameraPosition.x = Mathf.Clamp (limitedCameraPosition.x, minX, maxX);
		limitedCameraPosition.y = Mathf.Clamp (limitedCameraPosition.y, minY, maxY);
		limitedCameraPosition.z = Mathf.Clamp (limitedCameraPosition.z, minZ, maxZ);
		_camera.transform.position = limitedCameraPosition;
	}
	
	void destroyObject()
	{
		//gameplay.setPlayerScore(10);
		Destroy(gameObject);
	}

	bool isSelectedObjectHasObjectHandler()
	{
		return pickedObject.gameObject.GetComponent<ObjectHandler> ();
	}

	bool isSelectedObjectHasObjectHandler(Transform selectedObject)
	{
		return selectedObject.gameObject.GetComponent<ObjectHandler> ();
	}
	
	void deselectGameObject()
	{
		if(pickedObject)
		{
			if(isSelectedObjectHasObjectHandler())
			{
				objectHandler_ = pickedObject.GetComponent<ObjectHandler>();
				if(objectHandler_.objectType_ != ObjectHandler.objectType.empty)
				{
					objectHandler_.removeObjectGUI();
				}
			}
			pickedObject = null;
		}
	}

	void deselectGameObject(Transform object_)
	{
		if(object_)
		{
			if(isSelectedObjectHasObjectHandler())
			{
				objectHandler_ = object_.GetComponent<ObjectHandler>();
				if(objectHandler_.objectType_ != ObjectHandler.objectType.empty)
				{
					objectHandler_.removeObjectGUI();
				}
			}
			object_ = null;
		}
	}

	bool IsPointerOverUIObject(Vector2 touchPosition) {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = touchPosition;
		
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	void hideSelectedObjetUI (Transform object_)
	{
		if (isSelectedObjectHasObjectHandler (object_)) {
			objectHandler_ = object_.GetComponent<ObjectHandler> ();
			if (objectHandler_.objectType_ != ObjectHandler.objectType.empty) {
				objectHandler_.hideObjectGUI ();
			}
		}
	}

	void showSelectedObjectUI ()
	{
		if (isSelectedObjectHasObjectHandler ()) {
			objectHandler_ = pickedObject.GetComponent<ObjectHandler> ();
			if (objectHandler_.objectType_ != ObjectHandler.objectType.empty) {
				objectHandler_.showObjectGUI ();
			}
		}
	}
	
	public void onclickZoomIn()
	{
		float zoomValue = (maxZoom - minZoom) * 0.1f;
		/*Debug.Log ("MaxZoom: " + maxZoom);
		Debug.Log ("MinZoom: " + minZoom);
		Debug.Log ("ZoomValue: " + zoomValue);*/
		_camera.orthographicSize -= zoomValue;
		_camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);
		CalculateMapBounds ();
	}

	public void onclickZoomOut()
	{
		float zoomValue = (maxZoom - minZoom) * 0.1f;
		_camera.orthographicSize += zoomValue;
		_camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);
		CalculateMapBounds ();
	}
}

