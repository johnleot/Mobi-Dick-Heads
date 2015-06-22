using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
	public class ToolsSlidingMenu : MonoBehaviour
	{

		public Animator contentPanel;
		
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

		public void createTools()
		{
			ObjectSpawner objSpawner = new ObjectSpawner ();
			objSpawner.SpawnObject ();
		}
	}
}

