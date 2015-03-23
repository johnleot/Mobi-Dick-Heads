using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.DataSource;
using Assets.Scripts.Classes;

namespace Assets.Scripts.Behaviour
{
    class CalamityTimer : MonoBehaviour
    {
        public float calamityTimeRemaining; //eta of calamity is fetched from database
        public float calamityTimeToComplete; // calamity duration is fetched from database
        public Image calamity;
        public Image duration;
        public Text over;

        public LevelManager levelManager;
        public Calamity[] calamities;

        void Start()
        {
            int level = PlayerPrefs.GetInt("LevelSelected");
            levelManager = new CalamityManager();
            calamities = levelManager.GetLevel(level).Calamities.ToArray();

            Calamity firstCalamity = calamities[0];
            calamityTimeRemaining = firstCalamity.TimeOfArrival;
            calamityTimeToComplete = firstCalamity.Duration;

            //instantiate calamity

        }
        void Update()
        {
            if (calamityTimeRemaining > 0.0f)
            {
                calamity.fillAmount = Mathf.MoveTowards(calamity.fillAmount, 1.0f, Time.deltaTime / calamityTimeRemaining);
                calamityTimeRemaining -= Time.deltaTime;
            }
            else
            {
                if (calamityTimeToComplete > 0.0f)
                {
                    duration.fillAmount = Mathf.MoveTowards(duration.fillAmount, 1.0f, Time.deltaTime / calamityTimeToComplete);
                    calamityTimeToComplete -= Time.deltaTime;
                }
                else
                {
                    over.text = "Game Over";
                }
            }
        }
    }
}
