using UnityEngine;
using System.Collections;
using Assets.Scripts.Classes;

namespace Assets.Scripts.Behaviour
{
	public class ResourceSlidingMenu : MonoBehaviour
	{

		//public PrefabManager prefabManger;
		public Resource resource;
		public Animator contentPanel;
		public GameObject GameObject;
		GameObject thePrefab;
		
		void Start()
		{
			RectTransform transform = contentPanel.gameObject.transform as RectTransform;
			Vector2 position = transform.anchoredPosition;
			position.x -= transform.rect.height;
			transform.anchoredPosition = position;
		}
		
		public void ToggleMenu()
		{
			contentPanel.enabled = true;
			bool isHidden = contentPanel.GetBool ("isHidden");
			contentPanel.SetBool ("isHidden",!isHidden);
		}
		
		public void createResource()
		{
			
			ObjectSpawner objSpawner = new ObjectSpawner ();
			objSpawner.SpawnObject ();
		}
	}
}

