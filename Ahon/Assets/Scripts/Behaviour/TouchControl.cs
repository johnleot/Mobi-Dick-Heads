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
	public float minZoom = 1.0f;
	public float maxZoom = 20.0f;
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

	private Gameplay gameplay;
	
	private bool  longPressDetected;
	private bool  newTouch;
	private float touchTime;

	public GameObject gui; // for accessing Gameplay Script

	private ObjectHandler objectHandler_;

	void Start () {
		gameplay = gui.GetComponent<Gameplay> ();
		if (gameplay) {
			Debug.Log ("Success Getting gameplay !");
		} else {
			Debug.Log ("Failed getting gamplay instance. ");
		}
		cameraInitialPosition = _camera.transform.position;
		maxZoom = 0.5f * (mapWidth / _camera.aspect);

		if (mapWidth > mapHeight)
			maxZoom = 0.5f * mapHeight;
		//Debug.Log ("MaxZOOM : " + maxZoom);
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
					if (Physics.Raycast (ray, out hit) && hit.collider.tag == "Draggable") {
						if (touches [0].phase == TouchPhase.Began) {
							if (Physics.Raycast (ray, out hit, maxPickingDistance)) {
								if (pickedObject) {
									if (hit.transform.gameObject == pickedObject.gameObject ||
										hit.transform.gameObject != pickedObject.gameObject) {
										deselectGameObject ();
									}
								}

								touchTime = Time.time;
								pickedObject = hit.transform;
								/**/
								if (selectedObjectHasObjectHandler ()) {
									objectHandler_ = pickedObject.GetComponent<ObjectHandler> ();
									if (objectHandler_.objectType_ != ObjectHandler.objectType.empty) {
										objectHandler_.showObjectGUI ();
									}
								}
								//startPos = touches[0].position;
							} else {
								deselectGameObject ();
								pickedObject = null;
							}
						} else if (touches [0].phase == TouchPhase.Moved) {
							if (pickedObject != null) {
								float distance1 = 0f;
								if (horPlane.Raycast (ray, out distance1)) {
									pickedObject.transform.position = ray.GetPoint (distance1);
								}

								if (selectedObjectHasObjectHandler ()) {
									//objectHandler_.hideObjectGUI();
								}
							}
						} else if (touches [0].phase == TouchPhase.Ended) {
							// check if position is not overlap with other object
							//pickedObject = null;
						}
						/*
					else if(touches[0].phase == TouchPhase.Stationary)
					{
						if(pickedObject && (Time.time - touchTime) > 1)
						{
							gameplay.setPlayerScore(10);
							Destroy (pickedObject.gameObject);
						}
					}*/
					} else {
						deselectGameObject ();

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
	
	void CalculateMapBounds()
	{
		verticalExtent = _camera.orthographicSize;
		horizontalExtent = verticalExtent * _camera.aspect;
		minX = horizontalExtent - mapWidth / 2.0f;
		maxX = mapWidth / 2.0f - horizontalExtent;
		minY = verticalExtent - mapHeight / 2.0f;
		maxY = mapHeight / 2.0f - verticalExtent;

		minZ = horizontalExtent - mapWidth / 2.0f;
		maxZ = mapWidth / 2.0f - horizontalExtent;

		minX += cameraInitialPosition.x;
		maxX += cameraInitialPosition.x;
		minY += cameraInitialPosition.y;
		maxY += cameraInitialPosition.y;
		minZ += cameraInitialPosition.z;
		maxZ += cameraInitialPosition.z;

		//Print MaxHeight, MaxWidth;
		/*
		Debug.Log ("VerticalExtent" + verticalExtent);
		Debug.Log ("HorizontalExtent" + horizontalExtent);
		Debug.Log ("minX" + minX);
		Debug.Log ("maxX" + maxX);
		Debug.Log ("minY" + minY);
		Debug.Log ("maxY" + maxY);
		*/
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
		gameplay.setPlayerScore(10);
		Destroy(gameObject);
	}

	bool selectedObjectHasObjectHandler()
	{
		return pickedObject.gameObject.GetComponent<ObjectHandler> ();
	}

	void deselectGameObject()
	{
		if(pickedObject)
		{
			if(selectedObjectHasObjectHandler())
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

	private bool IsPointerOverUIObject(Vector2 touchPosition) {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = touchPosition;
		
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}

