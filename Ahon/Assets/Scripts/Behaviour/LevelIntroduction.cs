using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Classes;
using Assets.Scripts.DataSource;

namespace Assets.Scripts.Behaviour
{
    public class LevelIntroduction : MonoBehaviour
    {
		public Slider loadingBar;
		public GameObject loadingImage;

		private AsyncOperation async;

        void Start()
        {
            Text location;
            Text description;

            Component[] comps = GetComponentsInChildren<Text>();
            location = (Text)comps[0];
            description = (Text)comps[1];
            int selectedLevel = PlayerPrefs.GetInt("LevelSelected");

            LevelManager lm = new LevelManager();
            Level level = lm.GetLevelIntroduction(selectedLevel);

            location.text = level.StrLocation;
            description.text = level.StrDescription;
        }
        public GameObject infoWindow;
        public void OnClickPlay()
        {
			loadingImage.SetActive (true);
            //Application.LoadLevel("Game_Level");
			StartCoroutine(loadLevelBehind("Game_Level"));
        }

        public void OnClickCancel()
        {
            Application.LoadLevel("LevelSelection");
        }

        public void CloseInfoWindow()
        {
            infoWindow.SetActive(false);
        }

        public void OpenInfoWindow()
        {
            if (!infoWindow.activeInHierarchy)
                infoWindow.SetActive(true);
        }

		IEnumerator loadLevelBehind(string level)
		{
			async = Application.LoadLevelAsync (level);
			while(!async.isDone)
			{
				loadingBar.value = async.progress;
				yield return null;
			}
		}
    }
}
