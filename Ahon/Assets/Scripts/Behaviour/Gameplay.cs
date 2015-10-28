using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Classes;
using Assets.Scripts.DataSource;

namespace Assets.Scripts.Behaviour
{
    public class Gameplay : MonoBehaviour
	{
		public Slider natureResponseBar;
		public Slider peopleResponseBar;
		public Slider playerHealthBar;
		public Slider resourcesBar;

		Text calamity;
		Text position;
		Text resource;
		Text tools;
		public Text scores;
		public Text moneyText;

		// Use this for initialization
		UserManager userManager;
		LevelManager levelManager;
		
		//Initialize game state properties
		private Gameplay gamePlay;
		private string playerName;		//Player name
		private string activeLevel;		//Player's current level

		private int playerHealth;		//Player's current health
		private int peopleResponse;		//People's satisfaction score
		private int natureResponse;		//Nature score
		private int resourcesScore;		//Resources score
		private int score;				//Player's total score
		private int money;				//Player's money

        void Start()
        {
            Component[] comps = GetComponentsInChildren<Text>();

            calamity = (Text)comps[1];
            tools = (Text)comps[2];
            //resource = (Text)comps[3];
            position = (Text)comps[4];
			//scores = (Text)comps[5];

            userManager = new UserManager();
            levelManager = new LevelManager();

            User user = userManager.GetUser();

            int levelSelected = PlayerPrefs.GetInt("LevelSelected");

            Level level = levelManager.GetLevel(levelSelected);
            Resource[] resources = level.Resources.ToArray();
            Calamity[] calamities = level.Calamities.ToArray();
            Location location = level.Location;
            Solution[] solutions = level.Solutions.ToArray();

            calamity.text = calamities[0].Name;
            position.text = user.PositionName;

            
            //resource.text = resources[0].Name;
            tools.text = solutions[0].Name;
            scores.text = "Test";
			playerName = "Username";
			activeLevel = "";
			playerHealth = 100;
			peopleResponse = 32;
			natureResponse = 12;
			resourcesScore = 50;
			money = 1000000;
			score = 10;
			scores.text = score.ToString ();
			moneyText.text = money.ToString ();
			//Debug.Log ("NatureResponse: " + natureResponse);
        }

        // Update is called once per frame
        void Update()
        {
			//playerHealthBar.value = (float)(getPlayerHealth () / 100);
			natureResponseBar.value = (float)(getNatureResponse() / 100f);
			peopleResponseBar.value = (float)(getPeopleResponse() / 100f);
			resourcesBar.value = (float)(getResourcesScore() / 100f);

			calculateOverallScore ();
			scores.text = getPlayerScore ().ToString();
        }

		void calculateOverallScore()
		{
			/* 
			 * Calculation not yet final.
			 */
			score = (natureResponse + peopleResponse + resourcesScore);
			//Debug.Log ("SSSSSSSSCORE: " + score);
		}

		public string getPlayerName()
		{
			return playerName;
		}
		
		public void setPlayerName(string userName)
		{
			playerName = userName;
		}
		
		public string getActiveLevel()
		{
			return activeLevel;
		}
		
		public void setActiveLevel(string level)
		{
			activeLevel = level;
		}
		
		public int getPlayerHealth()
		{
			return playerHealth;
		}
		
		public int getPeopleResponse()
		{
			return peopleResponse;
		}
		
		public int getNatureResponse()
		{
			return natureResponse;
		}

		public int getResourcesScore()
		{
			return resourcesScore;
		}
		
		public int getPlayerScore()
		{
			return score;
		}

		public int getPlayerMoney()
		{
			return money;
		}

		public void setPeopleResponse(int value)
		{
			peopleResponse += value;	// food deficit + illegal activity stopped
			peopleResponseBar.value = (float)(peopleResponse / 100);
		}

		public void setNatureResponse(int value)
		{
			natureResponse += value;	// illegal activity stopped + calamity damage on crops
			natureResponseBar.value = (float)(peopleResponse / 100);
		}

		public void setResourcesScore(int value)
		{
			resourcesScore += value;	// crops remaining / population
			resourcesBar.value = (float)(resourcesScore / 100);
		}

		public void setPlayerMoney(int value)
		{
			money = value;
		}
    }
}
