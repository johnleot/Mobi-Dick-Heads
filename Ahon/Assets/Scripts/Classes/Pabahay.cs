using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts.Behaviour;

public class Pabahay :MonoBehaviour, IObject {

	private GameObject infoBtnGO_;
	private GameObject panel;

	private GameObject MainUI;

	private string title_ = "Pabahay";
	private string contentText_ = "Pabahy: The quick brown fox jumps over the lazy dog.";

	void Start()
	{
		MainUI = GameObject.FindWithTag ("MainUI");
		if (MainUI == null)
			Debug.Log ("Cant find MAIN UI.");
	}

	public void showUI()
	{
		panel = Instantiate(Resources.Load ("Level1/ObjectUIPanel")) as GameObject;
		infoBtnGO_ = Instantiate(Resources.Load ("Level1/ObjectUIButton")) as GameObject;
		panel.transform.SetParent (MainUI.transform, false);
		infoBtnGO_.transform.SetParent (panel.transform, true);
		Text buttonText = (Text)infoBtnGO_.transform.FindChild("Text").GetComponent<Text>();
		buttonText.text = gameObject.ToString();

		panel.gameObject.SetActive (true);
		infoBtnGO_.gameObject.SetActive (true);
		Debug.Log ("Showing Pabahay UI..." + gameObject);

		Button infoBtn_ = infoBtnGO_.GetComponent<Button> ();
		infoBtn_.onClick.RemoveAllListeners();
		infoBtn_.onClick.AddListener (() => showInfoWindow());
	}

	public void hideUI()
	{
		panel.gameObject.SetActive (false);
		infoBtnGO_.gameObject.SetActive (false);
		Debug.Log ("Hiding Pabahay UI..." + gameObject);
	}

	public void removeUI()
	{
		if (panel) 
		{
			Destroy (panel);
			Destroy (infoBtnGO_);
		}

		Debug.Log ("Deleting Pabahy UI...");
	}

	private void showInfoWindow()
	{

		GameObject modalPanel = Instantiate(Resources.Load("ModalPanel")) as GameObject;
		GameObject infoWindow = Instantiate(Resources.Load("InfoWindow")) as GameObject;

		modalPanel.transform.SetParent(MainUI.transform, false);
		infoWindow.transform.SetParent(modalPanel.transform, false);

		Text title = infoWindow.transform.FindChild ("Header").transform.FindChild ("Title").GetComponent<Text> ();
		Image image =  infoWindow.transform.FindChild("Content").transform.FindChild("Image").GetComponent<Image>();
		Text contentText = infoWindow.transform.FindChild("Content").transform.FindChild("ContentText").GetComponent<Text>();

		title.text = title_.ToString();
		image.sprite = Resources.Load("Pabahay",typeof(Sprite)) as Sprite;
		contentText.text = contentText_.ToString ();

		Button exitBtn = infoWindow.transform.FindChild ("Header").FindChild("ExitButton").GetComponent<Button> ();
		exitBtn.onClick.RemoveAllListeners ();
		exitBtn.onClick.AddListener(() => exit(exitBtn.transform));
		
		modalPanel.GetComponent<Button> ().onClick.RemoveAllListeners ();
		modalPanel.GetComponent<Button> ().onClick.AddListener (() => exit(exitBtn.transform));
	}

	void exit(Transform btntransform)
	{
		Debug.Log ("ObjectTO close: " + btntransform.parent.parent.parent.gameObject);
		Destroy (btntransform.parent.parent.parent.gameObject);
	}
}
