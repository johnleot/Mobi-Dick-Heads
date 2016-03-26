using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
    class Settings : MonoBehaviour
    {
		public GameObject mainMenu,settingsMenu;
		public GameObject audioPanel, helpPanel, aboutPanel;
		public Text help, about;
		SoundManager bgMusic, soundFX;


		public void OnClick(int buttonClicked)
		{
			switch (buttonClicked)
			{
			case 1:
				CloseSettingsMenu();
				break;
			case 2:
				HelpButton();
				break;
			case 3:
				AudioButton();
				break;
			case 4:
				AboutButton();
				break;
			}
		}

		void CloseSettingsMenu ()
		{
			mainMenu.SetActive(true);
			settingsMenu.SetActive (false);
		}

		void HelpButton ()
		{
			helpPanel.SetActive (true);
			audioPanel.SetActive(false);
			aboutPanel.SetActive(false);
			help.fontSize = 20;
			help.text = "This is your game help menu. Good luck!";
		}

		void AudioButton ()
		{
			helpPanel.SetActive (false);
			audioPanel.SetActive(true);
			aboutPanel.SetActive(false);
		}

		public void onClickSounds (int btn)
		{
			switch (btn)
			{
			case 1:
				if(SoundManager.instance.bgMusic.isPlaying)
				{
					SoundManager.instance.bgMusic.Stop();
				}
				else
				{
					SoundManager.instance.bgMusic.Play();
				}
				break;
			case 2:
				if(SoundManager.instance.soundFX.isPlaying)
				{
					SoundManager.instance.soundFX.Stop();
				}
				else
				{
					SoundManager.instance.soundFX.Play();
				}
				break;
			}
		}

		void AboutButton ()
		{
			helpPanel.SetActive (false);
			audioPanel.SetActive(false);
			aboutPanel.SetActive(true);
			about.fontSize = 20;
			about.text = "This game is cute and cuddly. Hope you like it!";
		}
    }
}
