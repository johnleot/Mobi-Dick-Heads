using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts.Behaviour;

public class Pabahay : IObject {
	
	//UI
	private GameObject infoBtnGO_;
	private GameObject relocateBtnGO_;
	private GameObject feedBtnGO_;
	private GameObject evacuateBtnGO_;
	
	GameObject okBtnGO_;
	GameObject cancelBtnGO_;
	ObjectHandler objectHandler_;
	Gameplay gameplay;
	
	void Awake()
	{
		objectHandler_ = gameObject.GetComponent<ObjectHandler>();
		MainUI = GameObject.FindWithTag ("MainUI");
		if (MainUI == null)
			Debug.Log ("Cant find MAIN UI.");
		
		gameplay = MainUI.GetComponent<Gameplay> ();
		title_ = "Pabahay";
		contentText_ = "Pabahy: The quick brown fox jumps over the lazy dog.";
	}
	
	override public void insertUI()
	{
		objectBtnHolder_ = MainUI.transform.FindChild ("ObjectUIPanel").gameObject;
		objectBtnHolder_.SetActive (true);
		
		generateInfoButton ();
		generateRelocateButton ();
		generateFeedButton ();
		generateEvacuateButton ();
	}
	
	void generateInfoButton()
	{
		if (!infoBtnGO_) {
			infoBtnGO_ = Instantiate(Resources.Load ("UI/InGame/ObjectButtons/InfoBtn")) as GameObject;
			infoBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
			
//			Text buttonText = (Text)infoBtnGO_.transform.FindChild("Text").GetComponent<Text>();
//			buttonText.text = gameObject.ToString(); //change this to image.
			
			objectBtnHolder_.gameObject.SetActive (true);
			infoBtnGO_.gameObject.SetActive (true);
			Debug.Log ("Showing Pabahay UI..." + gameObject);
			
			Button infoBtn_ = infoBtnGO_.GetComponent<Button> ();
			infoBtn_.onClick.RemoveAllListeners();
			infoBtn_.onClick.AddListener (() => showInfoWindow());
		}
	}
	
	void generateRelocateButton()
	{
		if (!relocateBtnGO_) {
			relocateBtnGO_ = Instantiate(Resources.Load ("UI/InGame/ObjectUIButton")) as GameObject;
			relocateBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
			
			Text buttonText = (Text)relocateBtnGO_.transform.FindChild("Text").GetComponent<Text>();
			buttonText.text = "Relocate"; //change this to image.
			
			objectBtnHolder_.gameObject.SetActive (true);
			relocateBtnGO_.gameObject.SetActive (true);
			
			Button relocateBtn_ = relocateBtnGO_.GetComponent<Button> ();
			relocateBtn_.onClick.RemoveAllListeners();
			relocateBtn_.onClick.AddListener (() => enableRelocate());
		}
	}
	
	void generateFeedButton()
	{
		if (!feedBtnGO_) {
			feedBtnGO_ = Instantiate(Resources.Load ("UI/InGame/ObjectButtons/FeedBtn")) as GameObject;
			feedBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
			
//			Text buttonText = (Text)feedBtnGO_.transform.FindChild("Text").GetComponent<Text>();
//			buttonText.text = "Feed"; //change this to image.
			
			objectBtnHolder_.gameObject.SetActive (true);
			feedBtnGO_.gameObject.SetActive (true);
			
			Button feedBtn_ = feedBtnGO_.GetComponent<Button> ();
			feedBtn_.onClick.RemoveAllListeners();
			feedBtn_.onClick.AddListener (() => enableFeed());
		}
	}
	
	void generateEvacuateButton()
	{
		if (!evacuateBtnGO_) {
			evacuateBtnGO_ = Instantiate(Resources.Load ("UI/InGame/ObjectButtons/EvacuateBtn")) as GameObject;
			evacuateBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
			
//			Text buttonText = (Text)evacuateBtnGO_.transform.FindChild("Text").GetComponent<Text>();
//			buttonText.text = "Evacuate"; //change this to image.
			
			objectBtnHolder_.gameObject.SetActive (true);
			evacuateBtnGO_.gameObject.SetActive (true);
			
			Button evacuateBtn_ = evacuateBtnGO_.GetComponent<Button> ();
			evacuateBtn_.onClick.RemoveAllListeners();
			evacuateBtn_.onClick.AddListener (() => enableEvacuate());
		}
	}
	
	override public void hideUI()
	{
		objectBtnHolder_.gameObject.SetActive (false);
		infoBtnGO_.gameObject.SetActive (false);
		Debug.Log ("Hiding Pabahay UI..." + gameObject);
	}
	
	public override void showUI ()
	{
		objectBtnHolder_.gameObject.SetActive (true);
		Debug.Log ("Hiding Pabahay UI..." + gameObject);
	}
	
	override public void removeUI()
	{
		if (objectBtnHolder_) 
		{
			foreach (Transform child in objectBtnHolder_.transform)
				GameObject.Destroy(child.gameObject);
		}
		
		Debug.Log ("Deleting Pabahy UI...");
	}
	
	void showInfoWindow()
	{
		GameObject modalPanel = MainUI.transform.FindChild("ModalPanel").gameObject;
		GameObject infoWindow = modalPanel.transform.FindChild("InfoWindow").gameObject;
		
		modalPanel.gameObject.SetActive (true);
		infoWindow.gameObject.SetActive (true);
		
		//populate
		Text title = infoWindow.transform.FindChild ("Header").transform.FindChild ("Title").GetComponent<Text> ();
		Image image =  infoWindow.transform.FindChild("Content").transform.FindChild("Image").GetComponent<Image>();
		Text contentText = infoWindow.transform.FindChild("Content").transform.FindChild("ContentText").GetComponent<Text>();
		
		title.text = title_.ToString();
		image.sprite = Resources.Load("Pabahay",typeof(Sprite)) as Sprite;
		contentText.text = contentText_.ToString ();
		
		//adding eventListener to a button
		Button exitBtn = infoWindow.transform.FindChild ("Header").FindChild("ExitButton").GetComponent<Button> ();
		exitBtn.onClick.RemoveAllListeners ();
		exitBtn.onClick.AddListener(() => exit(exitBtn.transform));
		
		modalPanel.GetComponent<Button> ().onClick.RemoveAllListeners ();
		modalPanel.GetComponent<Button> ().onClick.AddListener (() => exit(exitBtn.transform));
	}
	
	void exit(Transform btnTransform)
	{
		//Debug.Log ("ObjectTO close: " + btntransform.parent.parent.parent.gameObject);
		//Destroy (btntransform.parent.parent.parent.gameObject);
		btnTransform.parent.parent.gameObject.SetActive (false);
		btnTransform.parent.parent.parent.gameObject.SetActive (false);
	}
	
	void enableRelocate()
	{
		gameObject.tag = "Draggable";
		
		//removeUI ();
		// create ok and cancel button
		
		okBtnGO_ = Instantiate (Resources.Load ("Level1/ObjectUIButton")) as GameObject;
		okBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
		Text buttonText1 = (Text)okBtnGO_.transform.FindChild("Text").GetComponent<Text>();
		buttonText1.text = "OK"; //change this to image.
		
		cancelBtnGO_ = Instantiate (Resources.Load ("Level1/ObjectUIButton")) as GameObject;
		cancelBtnGO_.transform.SetParent (objectBtnHolder_.transform, false);
		Text buttonText2 = (Text)cancelBtnGO_.transform.FindChild("Text").GetComponent<Text>();
		buttonText2.text = "X"; //change this to image.
		
		Button okBtn = okBtnGO_.GetComponent<Button>();
		okBtn.onClick.RemoveAllListeners ();
		okBtn.onClick.AddListener (() => okRelocate());
		
		Button cancelBtn = cancelBtnGO_.GetComponent<Button> ();
		cancelBtn.onClick.RemoveAllListeners ();
		cancelBtn.onClick.AddListener (() => cancelRelocate());
	}
	
	void okRelocate()
	{
		if (objectHandler_.Colliding) {
			objectHandler_.resetPosition();
		}
		objectHandler_.OriginalPosition = gameObject.transform.position;
		
		gameObject.tag = "Building";
		Destroy (okBtnGO_);
		Destroy (cancelBtnGO_);
	}
	
	void cancelRelocate()
	{
		objectHandler_.resetPosition ();
		gameObject.tag = "Building";
		Destroy (okBtnGO_);
		Destroy (cancelBtnGO_);
	}
	
	void enableFeed ()
	{
		Debug.Log ("FEEDING FUNCTION ENABLED !");
		gameplay.setPeopleResponse (1);
		gameplay.setResourcesScore (-1);
		gameplay.setPlayerMoney (1000);

	}
	
	void enableEvacuate ()
	{
		Debug.Log ("EVACUATE FUNCTION ENABLED !");
		gameplay.setPeopleResponse (1);
		gameplay.setPlayerMoney (10000);
		//gameplay.setNumberOfOccupants (2);
	}
}
