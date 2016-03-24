using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Behaviour
{
    class MainMenu : MonoBehaviour
    {
		public GameObject mainMenu;
		public GameObject settingsMenu;

		public void OnClick(int buttonClicked)
        {
            switch (buttonClicked)
            {
                case 1:
                    LoadLevelSelectionScreen();
                    break;
                case 2:
                    LoadCinematicsScreen();
                    break;
				case 3:
					OpenSettingsMenu();
					break;
                default:
                    LoadFacebookScreen();
                    break;
            }
        }

		void Update()
		{
			if (Input.GetKeyDown (KeyCode.Escape))
				showExitConfirmationWindow ();
		}

        void LoadCinematicsScreen()
        {
            Application.LoadLevel("Cinematics");
        }

        void LoadLevelSelectionScreen()
        {
            Application.LoadLevel("LevelSelection");
        }

		void OpenSettingsMenu ()
		{
			settingsMenu.SetActive (true);
		}

        void LoadFacebookScreen()
        {
            Application.LoadLevel("FacebookScreen");
        }

		public void ExitGame()
		{
			Application.Quit ();
		}

		public void HideExitConfirmationWindow()
		{
			gameObject.transform.FindChild ("ModalPanel").gameObject.SetActive (false);
		}

		void showExitConfirmationWindow()
		{
			gameObject.transform.FindChild ("ModalPanel").gameObject.SetActive (true);
		}



    }
}
