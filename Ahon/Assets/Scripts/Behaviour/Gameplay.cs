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
		public GameObject MainUI;
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

		Resource[] resources;
		Solution[] solutions;

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
            resources = level.Resources.ToArray();
            Calamity[] calamities = level.Calamities.ToArray();
            Location location = level.Location;
            solutions = level.Solutions.ToArray();

            calamity.text = calamities[0].Name;
            position.text = user.PositionName;

			Debug.Log ("RESOURCES SIZE: " + resources.Length);
			Debug.Log ("SOLUTION SIZE: " + solutions.Length);

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
			
			MainUI = GameObject.FindWithTag ("MainUI");
			if (MainUI == null)
				Debug.Log ("Cant find MAIN UI.");

			setResourcseItems ();
			setToolsItem ();
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

		void setResourcseItems()
		{
			foreach (Resource resource in resources) 
			{
				GameObject resourcesSlider_ = MainUI.transform.FindChild("ResourcesWindow")
																.FindChild("ResourcesPanel")
																.FindChild("ResourcesSlider").gameObject;

				GameObject itemExcerpt_ = Instantiate(Resources.Load ("ItemExcerpt")) as GameObject;
				itemExcerpt_.transform.SetParent (resourcesSlider_.transform, false);

				Text itemLabel_ = itemExcerpt_.transform.FindChild("LabelBackground")
														.FindChild("Text")
														.GetComponent<Text>();

				/*check label if null*/
				if(!itemLabel_){ Debug.Log ("ERROR ACCESSING ITEMLABEL"); }
				itemLabel_.text = resource.Name;

				Image itemImage_ = itemExcerpt_.transform.FindChild("Image").GetComponent<Image>();
				/*check image_ if null*/
				if(!itemImage_){ Debug.Log ("ERROR ACCESSING ITEMIMAGE"); }
				itemImage_.sprite = Resources.Load(resource.Image,typeof(Sprite)) as Sprite;

				Text itemPrice_ = itemExcerpt_.transform.FindChild("PriceBackground")
														.FindChild("Text")
														.GetComponent<Text>();
				itemPrice_.text = resource.Price.ToString(); // TO DO in database;

				/*add event listener here. prefabToINstantiate*/

				if(resource.PrefabToInstantiate != "")
				{
					try
					{
						Debug.Log ("PREFABTOINSTANTIATE: " + resource.PrefabToInstantiate);
						Button itemExcerptBtn_ = itemExcerpt_.GetComponent<Button>();
						itemExcerptBtn_.onClick.RemoveAllListeners();
						string path_ = resource.PrefabToInstantiate;
						itemExcerptBtn_.onClick.AddListener (
							() => { 
							Debug.Log ("PATH: " + path_);
							GameObject instance = Instantiate (Resources.Load (path_)) as GameObject; 
						}
						);
					}
					catch(UnityException e)
					{
						Debug.Log (e);
					}
				}
			}
		}

		void setToolsItem()
		{
			foreach (Solution solution in solutions) 
			{				
				GameObject toolsSlider_ = MainUI.transform.FindChild("ResourcesWindow")
																.FindChild("ToolsPanel")
																.FindChild("ToolsSlider").gameObject;
				
				GameObject itemExcerpt_ = Instantiate(Resources.Load ("ItemExcerpt")) as GameObject;
				itemExcerpt_.transform.SetParent (toolsSlider_.transform, false);
				
				Text itemLabel_ = itemExcerpt_.transform.FindChild("LabelBackground")
														.FindChild("Text")
														.GetComponent<Text>();
				
				/*check label if null*/
				if(!itemLabel_){ Debug.Log ("ERROR ACCESSING ITEMLABEL"); }
				itemLabel_.text = solution.Name;
				
				Image itemImage_ = itemExcerpt_.transform.FindChild("Image").GetComponent<Image>();
				/*check image_ if null*/
				if(!itemImage_){ Debug.Log ("ERROR ACCESSING ITEMIMAGE"); }
				itemImage_.sprite = Resources.Load(solution.Image,typeof(Sprite)) as Sprite;
				
				Text itemPrice_ = itemExcerpt_.transform.FindChild("PriceBackground")
					.FindChild("Text")
						.GetComponent<Text>();
				itemPrice_.text = "0000"; // TO DO in database;

			}
		}

		public void closeResourcesWindow()
		{
			MainUI.transform.FindChild ("ResourcesWindow").gameObject.SetActive(false);
		}

		public void onclickResourcesBtn()
		{
			MainUI.transform.FindChild("ResourcesWindow")
							.FindChild("ToolsPanel").gameObject.SetActive(false);
			MainUI.transform.FindChild("ResourcesWindow")
							.FindChild("ResourcesPanel").gameObject.SetActive(true);
		}

		public void onclickToolsBtn()
		{
			MainUI.transform.FindChild("ResourcesWindow")
							.FindChild("ResourcesPanel").gameObject.SetActive(false);
			MainUI.transform.FindChild("ResourcesWindow")
							.FindChild("ToolsPanel").gameObject.SetActive(true);
		}

		public void openShop()
		{
			MainUI.transform.FindChild ("ResourcesWindow").gameObject.SetActive(true);
		}
	}
}
