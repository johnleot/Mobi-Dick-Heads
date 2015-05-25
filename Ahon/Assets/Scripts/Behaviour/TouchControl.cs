using UnityEngine;
using System.Collections;

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
	private float horizontalExtent, verticalExtent;
	private float minX, maxX, minY, maxY;
	
	private float maxPickingDistance = 1000;
	//private Vector3 startPos;
	private Transform pickedObject = null;
	private bool colliding = false;
	
	void Start () {
		maxZoom = (mapWidth > mapHeight)? 0.5f * mapHeight : 0.5f * (mapWidth / _camera.aspect);
		CalculateMapBounds ();
	}
	
	void FixedUpdate () {
		if(updateZoomSensitivity)
		{
			moveSensitivityX = _camera.orthographicSize / 5.0f;
			moveSensitivityY = _camera.orthographicSize / 5.0f;
			
			Touch[] touches = Input.touches;
			
			Plane horPlane = new Plane(Vector3.up, -1.0f);
			
			Ray ray = _camera.ScreenPointToRay(touches[0].position);
			RaycastHit hit;
			
			if(touches.Length > 0)
			{
				
				if(Physics.Raycast(ray, out hit) && hit.collider.tag == "Draggable")
				{
					if(touches[0].phase == TouchPhase.Began)
					{
						if(Physics.Raycast(ray, out hit, maxPickingDistance))
						{
							pickedObject = hit.transform;
							//startPos = touches[0].position;
						}
						else
						{
							pickedObject = null;
						}
					}
					else if (touches[0].phase ==  TouchPhase.Moved)
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
					else if (touches[0].phase == TouchPhase.Ended)
					{
						// check if position is not overlap with other object
						pickedObject = null;
					}
				}
				else
				{
					if(touches[0].phase == TouchPhase.Moved)
					{
						
						Vector2 delta = touches[0].deltaPosition;
						float positionX = delta.x * moveSensitivityX * Time.fixedDeltaTime;
						positionX = invertMoveX ? positionX : positionX * -1;
						
						float positionY = delta.y * moveSensitivityY * Time.fixedDeltaTime;
						positionY = invertMoveY ? positionY : positionY * -1;
						
						_camera.transform.position += new Vector3(positionX, positionY, -positionX);
						
					}
				}
				
				if(touches.Length == 2)
				{
					Vector2 cameraViewSize = new Vector2 (_camera.pixelWidth, _camera.pixelHeight);
					
					Touch touchOne = touches[0];
					Touch touchTwo = touches[1];
					
					Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
					Vector2 touchTwoPrevPos = touchTwo.position - touchTwo.deltaPosition;
					
					float prevTouchDeltaMag = (touchOnePrevPos - touchTwoPrevPos).magnitude;
					float touchDeltaMag = (touchOne.position - touchTwo.position).magnitude;
					
					float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;
					
					_camera.transform.position += _camera.transform.TransformDirection((touchOnePrevPos + touchTwoPrevPos - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);
					
					_camera.orthographicSize += deltaMagDiff * orthoZoomspeed;
					_camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom); //- 0.001f;
					
					_camera.transform.position -= _camera.transform.TransformDirection((touchOne.position + touchTwo.position - cameraViewSize) * _camera.orthographicSize / cameraViewSize.y);
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
	}
	
	void LateUpdate()
	{
		Vector3 limitedCameraPosition = _camera.transform.position;
		limitedCameraPosition.x = Mathf.Clamp (limitedCameraPosition.x, minX, maxX);
		limitedCameraPosition.y = Mathf.Clamp (limitedCameraPosition.y, minY, maxY);
		_camera.transform.position = limitedCameraPosition;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other)
		{
			colliding = true;
		}
	}
}





















