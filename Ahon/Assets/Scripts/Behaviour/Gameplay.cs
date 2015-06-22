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
		Text calamity;
		Text position;
		Text resource;
		Text tools;
		Text scores;

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
		private int score;				//Player's total score

        void Start()
        {
            Component[] comps = GetComponentsInChildren<Text>();

            calamity = (Text)comps[1];
            position = (Text)comps[2];
            resource = (Text)comps[3];
            tools = (Text)comps[4];
			scores = (Text)comps[5];

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
            resource.text = resources[0].Name;
            tools.text = solutions[0].Name;
			playerName = "Username";
			activeLevel = "";
			playerHealth = 100;
			peopleResponse = 0;
			natureResponse = 0;
			score = 0;
			scores.text = score.ToString ();
            Debug.Log("Calamity - " + calamity.text);
        }

        // Update is called once per frame
        void Update()
        {
			scores.text = getPlayerScore ().ToString();
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
		
		public int getPlayerScore()
		{
			return score;
		}

		public void setPlayerScore(int playerScore)
		{
			score += playerScore;
		}
    }
}
