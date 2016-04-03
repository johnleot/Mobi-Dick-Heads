using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
    class Settings : MonoBehaviour
    {
		public GameObject mainMenu,settingsMenu;
		public GameObject audioPanel, helpPanel, aboutPanel, prohibitMusic, prohibitSFX;
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
			prohibitMusic.SetActive(false);
			prohibitSFX.SetActive(false);
		}

		public void onClickSounds (int btn)
		{
			switch (btn)
			{
			case 1:
				if(SoundManager.instance.bgMusic.isPlaying)
				{
					SoundManager.instance.bgMusic.Stop();
					prohibitMusic.SetActive(true);
				}
				else
				{
					SoundManager.instance.bgMusic.Play();
					prohibitMusic.SetActive(false);
				}
				break;
			case 2:
				if(SoundManager.instance.soundFX.isPlaying)
				{
					SoundManager.instance.soundFX.Stop();
					prohibitSFX.SetActive(true);
				}
				else
				{
					SoundManager.instance.soundFX.Play();
					prohibitSFX.SetActive(false);
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
