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

        // Use this for initialization
        UserManager userManager;
        LevelManager levelManager;

        void Start()
        {
            Component[] comps = GetComponentsInChildren<Text>();

            calamity = (Text)comps[1];
            position = (Text)comps[2];
            resource = (Text)comps[3];
            tools = (Text)comps[4];

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
        }

        // Update is called once per frame
        void Update()
        {
            //print ("Hi");
        }
    }
}
