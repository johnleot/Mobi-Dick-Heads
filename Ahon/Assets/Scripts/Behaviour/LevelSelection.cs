using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using Assets;
using Assets.Scripts.DataSource;

namespace Assets.Scripts.Behaviour
{
    class LevelSelection : MonoBehaviour
    {
        public void OnLevelSelect(int level)
        {
            //insert code here where it gets the text content of the button selected
            //and is the one saved in playerprefs
            PlayerPrefs.SetInt("LevelSelected", level);
            Application.LoadLevel("Level_Info");
        }

		public void backBtnClick()
		{
			Application.LoadLevel("Main");
		}

		public void forLevel2()
		{
			Application.LoadLevel ("Level_Info");

		}

		public void forLevel3(){
			//Application.LoadLevel ("LevelSelected");
		}



		public void disabledBtn() 
		{ 
			gameObject.GetComponent<Button>().interactable = false; 
		
		}

		public void enabledBtn() 
		{ 
			gameObject.GetComponent<Button>().interactable = true; 
			
		}
    }
}
