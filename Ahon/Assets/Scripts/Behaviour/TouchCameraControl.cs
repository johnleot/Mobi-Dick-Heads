using UnityEngine;
using System.Collections;

public class TouchCameraControl : MonoBehaviour 
{
	public float moveSensitivityX = 1.0f;
	public float moveSensitivityY = 1.0f;
	public bool updateZoomSensitivity = true;
	public float orthoZoomSpeed = 0.05f;
	public float minZoom = 1.0f;
	public float maxZoom = 20.74f;
	public bool invertMoveX = false;
	public bool invertMoveY = false;
	public float mapWidth = 60.0f;
	public float mapHeight = 40.0f;
	
	public float inertiaDuration = 1.0f;
	
	private Camera _camera;
	private Vector3 cameraInitialPosition;
	private float highestX = 57.6f, highestY = 49.44f;
	
	private float minX, maxX, minY, maxY, minZ, maxZ;
	private float horizontalExtent, verticalExtent;
	
	private float scrollVelocity = 0.0f;
	private float timeTouchPhaseEnded;
	private Vector2 scrollDirection = Vector2.zero;
	
	void Start () 
	{
		_camera = Camera.main;
		cameraInitialPosition = _camera.transform.position;
		/*
		maxZoom = 0.5f * (mapWidth / _camera.aspect);

		if (mapWidth > mapHeight)
			maxZoom = 0.5f * mapHeight;
		*/
		
		if (_camera.orthographicSize > maxZoom)
			_camera.orthographicSize = maxZoom;
		
		CalculateLevelBounds ();
	}
	
	void Update () 
	{
		if (updateZoomSensitivity)
		{
			moveSensitivityX = _camera.orthographicSize / 5.0f;
			moveSensitivityY = _camera.orthographicSize / 5.0f;
		}
		
		Touch[] touches = Input.touches;
		
		if (touches.Length < 1)
		{
			//if the camera is currently scrolling
			if (scrollVelocity != 0.0f)
			{
				//slow down over time
				float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
				float frameVelocity = Mathf.Lerp (scrollVelocity, 0.0f, t);
				_camera.transform.position += -(Vector3)scrollDirection.normalized * (frameVelocity * 0.05f) * Time.deltaTime;
				
				if (t >= 1.0f)
					scrollVelocity = 0.0f;
			}
		}
		
		if (touches.Length > 0)
		{
			//Single touch (move)
			if (touches.Length == 1)
			{
				if (touches[0].phase == TouchPhase.Began)
				{
					scrollVelocity = 0.0f;
				}
				else if (touches[0].phase == TouchPhase.Moved)
				{
					Vector2 delta = touches[0].deltaPosition;
					
					float positionX = delta.x * moveSensitivityX * Time.deltaTime;
					positionX = invertMoveX ? positionX : positionX * -1;
					
					float positionY = delta.y * moveSensitivityY * Time.deltaTime;
					positionY = invertMoveY ? positionY : positionY * -1;
					
					_camera.transform.position += new Vector3 (-positionX, positionY, positionX);
					
					//					scrollDirection = touches[0].deltaPosition.normalized;
					//					scrollVelocity = touches[0].deltaPosition.magnitude / touches[0].deltaTime;
					//					
					//					
					//					if (scrollVelocity <= 100)
					//						scrollVelocity = 0;
					
				}
				else if (touches[0].phase == TouchPhase.Ended)
				{
					timeTouchPhaseEnded = Time.time;
				}
			}
			
			
			//Double touch (zoom)
			if (touches.Length == 2)
			{
				Vector2 cameraViewsize = new Vector2 (_camera.pixelWidth, _camera.pixelHeight);
				
				Touch touchOne = touches[0];
				Touch touchTwo = touches[1];
				
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				Vector2 touchTwoPrevPos = touchTwo.position - touchTwo.deltaPosition;
				
				float prevTouchDeltaMag = (touchOnePrevPos - touchTwoPrevPos).magnitude;
				float touchDeltaMag = (touchOne.position - touchTwo.position).magnitude;
				
				float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;
				
				_camera.transform.position += _camera.transform.TransformDirection ((touchOnePrevPos + touchTwoPrevPos - cameraViewsize) * _camera.orthographicSize / cameraViewsize.y);
				
				_camera.orthographicSize += deltaMagDiff * orthoZoomSpeed;
				_camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize, minZoom, maxZoom) - 0.001f;
				
				_camera.transform.position -= _camera.transform.TransformDirection ((touchOne.position + touchTwo.position - cameraViewsize) * _camera.orthographicSize / cameraViewsize.y);
				
				CalculateLevelBounds ();
			}
		}
	}
	
	void CalculateLevelBounds ()
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
		
	void LateUpdate ()
	{
		Vector3 limitedCameraPosition = _camera.transform.position;
		limitedCameraPosition.x = Mathf.Clamp (limitedCameraPosition.x, minX, maxX);
		limitedCameraPosition.y = Mathf.Clamp (limitedCameraPosition.y, minY, maxY);
		limitedCameraPosition.z = Mathf.Clamp (limitedCameraPosition.z, minZ, maxZ);
		_camera.transform.position = limitedCameraPosition;
	}
}