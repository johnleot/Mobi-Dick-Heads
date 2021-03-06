﻿using UnityEngine;
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
        public float originalCtc;
		public Image calamity;
		public Image duration;
		public Text over;
		
		LevelManager levelManager;
		Calamity[] calamities;

        int level = 1;
		GameObject ResultsWindow;
		public AudioClip gameOverSound;
		private bool aired = false;

        Calamity firstCalamity;
        string calamityName;
        bool hasCalamityStarted;
		void Awake(){
			//ResultsWindow = GameObject.FindGameObjectWithTag ("ResultsWindow");
		}
		void Start()
		{
			level = PlayerPrefs.GetInt("LevelSelected");
			levelManager = new CalamityManager();
			calamities = levelManager.GetLevel(level).Calamities.ToArray();
			
			firstCalamity = calamities[0];
            calamityName = firstCalamity.Name;
			//calamityTimeRemaining = firstCalamity.TimeOfArrival;
            calamityTimeRemaining = 30;
            calamityTimeToComplete = firstCalamity.Duration;
            //calamityTimeToComplete = 10;
            originalCtc = calamityTimeToComplete; 
			//ResultsWindow.SetActive (false);
			//instantiate calamity
		}
		void Update()
		{
			/**
			 * Checks the calamityTimeRemaining and calamityTimetoComplete everytime if they have reached zero already
			 */
			if (calamityTimeRemaining > 0.0f)
			{

				calamity.fillAmount = Mathf.MoveTowards(calamity.fillAmount, 1.0f, (Time.deltaTime / calamityTimeRemaining) / 4f);
                calamityTimeRemaining -= Time.deltaTime;
			} 
			else if(calamityTimeRemaining == 0.0f){
			/**
			 * 
			 */

			} 
			else
			{
				if (calamityTimeToComplete > 0.0f)
				{
					/**
					 * Start news cast here, also place a 'flag' to indicate that the newscast already aired
					 */

					if (!aired){
						ShowNewsCast();
						aired = true;
					}
                    int checker_1 = (int)calamityTimeToComplete;

                    duration.fillAmount = Mathf.MoveTowards(duration.fillAmount, 1.0f, (Time.deltaTime / calamityTimeToComplete) / 4f);
					calamityTimeToComplete -= Time.deltaTime;
                    //Debug.Log("calamityTimeToComplete - " + calamityTimeToComplete);
            
                    int checker_2 = (int)calamityTimeToComplete;
                    if (checker_1 != checker_2)
                    {
                        StartCalamity(calamityName.ToLower(), originalCtc);
                    }
                    
				}
				else
				{
					over.text = "Game Over";
                    SoundManager.instance.bgMusic.Stop();
					SoundManager.instance.PlaySingle(gameOverSound);
					
					transform.FindChild("ResultsWindow").gameObject.SetActive(true);
					
					/**
                     * Place score card here
					/*
					if(!ResultsWindow.activeInHierarchy){
						ResultsWindow.SetActive(true);
					}*/

					GameObject resultsWindow = transform.FindChild("ResultsWindow").gameObject;

					Text score = resultsWindow.transform.FindChild("ScoreText").GetComponent<Text>();
					//Text money = resultsWindow.transform.FindChild("moneyText").GetComponent<Text>();
					Text natureRating = resultsWindow.transform.FindChild("NatureRatingText").GetComponent<Text>();
					Text peopleResponse = resultsWindow.transform.FindChild("PeopleRatingText").GetComponent<Text>();
					Text resourcesScore = resultsWindow.transform.FindChild("ResourcesText").GetComponent<Text>();

					Gameplay gameplay = GetComponent<Gameplay>();
					if(!gameplay) { Debug.Log ("Error: Gameplay Not Fetched."); }
					else { Debug.Log("SUCCESSS !!!!! gameplay fetched."); }

					score.text = "Score: " + gameplay.getPlayerScore().ToString();
					natureRating.text = "Nature Rating: " + gameplay.getNatureResponse().ToString();
					peopleResponse.text = "People Rating: " + gameplay.getPeopleResponse().ToString();
					resourcesScore.text = "Resource Rating: " + gameplay.getResourcesScore().ToString();

				}
			}
		}
		void ShowNewsCast(){
			Debug.Log ("MAY BAGYOOOOOo!");
			/**
			 * Show graphics/window/notification here 
			 */ 
		}
        void StartCalamity(string type, float num)
        {
            switch (type)
            {
                case "flooding":
                    float amount = 8/num;
                    (FindObjectOfType(typeof(WaterPlusScript)) as WaterPlusScript).InvokeFlooding(amount);
                    break;
                case "drought":
                    break;
            }
        }
	}
}
