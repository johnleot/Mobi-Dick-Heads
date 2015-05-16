using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour {

	public string nextLevel;
	public bool goNext; //Check if next level should be loaded

	public GameObject background, text, progressbar;

	private int loadProgress = 0;

	// Use this for initialization
	void Start () {

		background.SetActive (false);
		text.SetActive (false);
		progressbar.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {

		if (goNext == true) {
			StartCoroutine(DisplayLoadingScreen(nextLevel));
		}
	
	}

	IEnumerator DisplayLoadingScreen(string level)
	{
		background.SetActive (true);
		text.SetActive (true);
		progressbar.SetActive (true);

		progressbar.transform.localScale = new Vector3 (progressbar, progressbar.transform.localScale.y, progressbar.transform.localScale.z);

		text.Equals = "Loading Progress" + loadProgress + "%";

		AsyncOperation async = Application.LoadLevelAsync (nextLevel);

		while (!async.isDone) {
			loadProgress = (int)(async.progress * 100);
			text.Equals = "Loading Progress" + loadProgress + "%";
			progressbar.transform.localScale = new Vector3 (async.progress, progressbar.transform.localScale.y, progressbar.transform.localScale.z);
			yield return null;
		}
	}
}
