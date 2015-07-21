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
        public float originalCtc;
		public Image calamity;
		public Image duration;
		public Text over;
		
		LevelManager levelManager;
		Calamity[] calamities;

        int level = 1;
		GameObject ResultsWindow;
		public AudioClip gameOverSound;


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
			calamityTimeRemaining = firstCalamity.TimeOfArrival;
            //calamityTimeRemaining = 10;
            calamityTimeToComplete = firstCalamity.Duration;
            //calamityTimeToComplete = 10;
            originalCtc = calamityTimeToComplete; 
			//ResultsWindow.SetActive (false);
			//instantiate calamity
			
		}
		void Update()
		{
			if (calamityTimeRemaining > 0.0f)
			{
				calamity.fillAmount = Mathf.MoveTowards(calamity.fillAmount, 1.0f, (Time.deltaTime / calamityTimeRemaining) / 4f);
                calamityTimeRemaining -= Time.deltaTime;
			}
			else
			{
				if (calamityTimeToComplete > 0.0f)
				{

                    int checker_1 = (int)calamityTimeToComplete;

                    duration.fillAmount = Mathf.MoveTowards(duration.fillAmount, 1.0f, (Time.deltaTime / calamityTimeToComplete) / 4f);
					calamityTimeToComplete -= Time.deltaTime;
                    //Debug.Log("calamityTimeToComplete - " + calamityTimeToComplete);
            
                    int checker_2 = (int)calamityTimeToComplete;
                    if (checker_1 != checker_2)
                    {
                        //Debug.Log("1: " + checker_1 + " 2: " + checker_2);
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
				}
			}
		}

        void StartCalamity(string type, float num)
        {
            switch (type)
            {
                case "flooding":
                            //Vector3 yAxis = this.transform.position;
               // amount = 7 / amount;

                //while (yAxis.y < 7)
                //{
                   // yAxis = this.transform.position;
                    //Debug.Log("yAxis - " + yAxis);
                   // yAxis.y += 7 / calamityTimeToComplete;
                    //Debug.Log("yAxis.y - " + yAxis.y);
                    //transform.position = yAxis;
                    //Thread.Sleep(1000);
                //}  
                    float amount = 8/num;
                    Debug.Log("amount - " + amount);
                   // while (amount < 7)
                   // {
                        (FindObjectOfType(typeof(WaterPlusScript)) as WaterPlusScript).InvokeFlooding(amount);
                        //Debug.Log("amount - " + amount);
                     //   amount+= .001f;
                        
                   // }
                    break;
                case "drought":
                    break;
            }
        }
	}
}
