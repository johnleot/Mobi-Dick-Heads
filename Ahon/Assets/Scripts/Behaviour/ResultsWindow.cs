//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
	public class ResultsWindow:MonoBehaviour
	{
		public GameObject resultWindow;
		private Text text;

		void Start()
		{
			text = resultWindow.GetComponent<Text> ();
		}

		public ResultsWindow ()
		{
		}

		public void OnReTryButtonClicked(){
			Application.LoadLevel ("Level_Info");
		}

		public void OnNextLevelButtonClicked(){
			Application.LoadLevel ("Level2");
		}

		public void OnShareToFBButtonClicked(){
		}

		public void OnCloseButtonClicked()
		{
			Application.LoadLevel ("LevelSelection");
		}
	}
}

