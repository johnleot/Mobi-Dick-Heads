using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using Assets;
using Assets.Scripts.DataSource;

namespace Assets.Scripts.Behaviour
{
    class LevelSelection : MonoBehaviour
    {
        public void OnLevelSelect(int level)
        {
            //insert code here where it gets the text content of the button selected
            //and is the one saved in playerprefs
            PlayerPrefs.SetInt("LevelSelected", level);
            Application.LoadLevel("Level_Info");
        }

    }
}
