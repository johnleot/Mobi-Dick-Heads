using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts.Behaviour;

public class EvacuationCenter : IObject {
	
	private GameObject infoBtnGO_;
	UnityEngine.UI.Text text1;
	void Awake()
	{
		MainUI = GameObject.FindWithTag ("MainUI");
		if (MainUI == null)
			Debug.Log ("Cant find MAIN UI.");
		
		title_ = "Evacuation Center";
		contentText_ = "EvacuationCenter: The quick brown fox jumps over the lazy dog.";
	}
	
	override public void insertUI()
	{
		objectBtnHolder_ = Instantiate(Resources.Load ("Level1/ObjectUIPanel")) as GameObject;
		infoBtnGO_ = Instantiate (Resources.Load ("Level1/ObjectUIButton")) as GameObject;
		objectBtnHolder_.transform.SetParent (MainUI.transform, false);
		infoBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
		Text buttonText = (Text)infoBtnGO_.transform.FindChild("Text").GetComponent<Text>();
		buttonText.text = gameObject.ToString();
		
		objectBtnHolder_.gameObject.SetActive (true);
		infoBtnGO_.gameObject.SetActive (true);
		Debug.Log ("Showing EvacuationCenter UI..." + gameObject);
		
		Button infoBtn_ = infoBtnGO_.GetComponent<Button> ();
		infoBtn_.onClick.RemoveAllListeners();
		infoBtn_.onClick.AddListener (() => showInfoWindow());
	}
	
	override public void hideUI()
	{
		objectBtnHolder_.gameObject.SetActive (false);
		infoBtnGO_.gameObject.SetActive (false);
		Debug.Log ("Hiding EvacuationCenter UI..." + gameObject);
	}
	
	public override void showUI ()
	{
		objectBtnHolder_.gameObject.SetActive (true);
		Debug.Log ("Hiding EvacuationCenter UI..." + gameObject);
	}

	override public void removeUI()
	{
		if (objectBtnHolder_) 
		{
			Destroy (objectBtnHolder_);
			Destroy (infoBtnGO_);
		}
		
		Debug.Log ("Deleting EvacuationCenter UI...");
	}
	
	void showInfoWindow()
	{
		
		GameObject modalPanel = MainUI.transform.FindChild("ModalPanel").gameObject;
		GameObject infoWindow = modalPanel.transform.FindChild("InfoWindow").gameObject;
		
		modalPanel.gameObject.SetActive (true);
		infoWindow.gameObject.SetActive (true);
		
		Text title = infoWindow.transform.FindChild ("Header").transform.FindChild ("Title").GetComponent<Text> ();
		Image image =  infoWindow.transform.FindChild("Content").transform.FindChild("Image").GetComponent<Image>();
		Text contentText = infoWindow.transform.FindChild("Content").transform.FindChild("ContentText").GetComponent<Text>();
		
		title.text = title_.ToString();
		image.sprite = Resources.Load("1_Evacuation_300x230",typeof(Sprite)) as Sprite;
		contentText.text = contentText_.ToString ();
		
		Button exitBtn = infoWindow.transform.FindChild ("Header").FindChild("ExitButton").GetComponent<Button> ();
		exitBtn.onClick.RemoveAllListeners ();
		exitBtn.onClick.AddListener(() => exit(exitBtn.transform));
		
		modalPanel.GetComponent<Button> ().onClick.RemoveAllListeners ();
		modalPanel.GetComponent<Button> ().onClick.AddListener (() => exit(exitBtn.transform));
	}
	
	void exit(Transform btnTransform)
	{
		btnTransform.parent.parent.gameObject.SetActive (false);
		btnTransform.parent.parent.parent.gameObject.SetActive (false);
	}
}
