using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
    class Settings : MonoBehaviour
    {
		public AudioMixerSnapshot noBG;
		public AudioMixerSnapshot withBG;
		
		public AudioMixerSnapshot noSFX;
		public AudioMixerSnapshot withSFX;

		bool bgSound = true;
		bool sfxSound = true;

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

		public void MuteBG()
		{
			if (bgSound)
			{
				noBG.TransitionTo (0.1f);
				bgSound = false;
			}
			else
			{
				withBG.TransitionTo(0.1f);
				bgSound = true;
			}
		}
		
		public void MuteSFX()
		{
			if (sfxSound)
			{
				noSFX.TransitionTo (0.1f);
				sfxSound = false;
			}
			else
			{
				withSFX.TransitionTo(0.1f);
				sfxSound = true;
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
