using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts.Behaviour;

public class Pabahay : IObject {

	//UI
	private GameObject infoBtnGO_;
	private GameObject relocateBtnGO_;

	void Awake()
	{
		MainUI = GameObject.FindWithTag ("MainUI");
		if (MainUI == null)
			Debug.Log ("Cant find MAIN UI.");
				
		title_ = "Pabahay";
		contentText_ = "Pabahy: The quick brown fox jumps over the lazy dog.";
	}

	override public void showUI()
	{
		objectBtnHolder_ = Instantiate(Resources.Load ("Level1/ObjectUIPanel")) as GameObject;
		objectBtnHolder_.transform.SetParent (MainUI.transform, false);

		generateInfoButton ();
		generateRelocateButton ();
	}

	void generateInfoButton()
	{
		if (!infoBtnGO_) {
			infoBtnGO_ = Instantiate(Resources.Load ("Level1/ObjectUIButton")) as GameObject;
			infoBtnGO_.transform.SetParent (objectBtnHolder_.transform, true);

			Text buttonText = (Text)infoBtnGO_.transform.FindChild("Text").GetComponent<Text>();
			buttonText.text = gameObject.ToString(); //change this to image.
			
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
			relocateBtnGO_ = Instantiate(Resources.Load ("Level1/ObjectUIButton")) as GameObject;
			relocateBtnGO_.transform.SetParent (objectBtnHolder_.transform, true);
			
			Text buttonText = (Text)relocateBtnGO_.transform.FindChild("Text").GetComponent<Text>();
			buttonText.text = "Relocate"; //change this to image.
			
			objectBtnHolder_.gameObject.SetActive (true);
			relocateBtnGO_.gameObject.SetActive (true);
			
			Button relocateBtn_ = infoBtnGO_.GetComponent<Button> ();
			//relocateBtn_.onClick.RemoveAllListeners();
			//relocateBtn_.onClick.AddListener (() => showInfoWindow());
		}
	}

	override public void hideUI()
	{
		objectBtnHolder_.gameObject.SetActive (false);
		infoBtnGO_.gameObject.SetActive (false);
		Debug.Log ("Hiding Pabahay UI..." + gameObject);
	}

	override public void removeUI()
	{
		if (objectBtnHolder_) 
		{
			Destroy (objectBtnHolder_);
			Destroy (infoBtnGO_);
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
}
