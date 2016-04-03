using UnityEngine;
using System.Collections;

public class VideoLoaderScript : MonoBehaviour {
	public GameObject videoHolder;
	// Use this for initialization
	void Start () {
	/*#if UNITY_EDITOR
		{
			videoHolder.SetActive(true);
			MovieTexture movieTexture_ = (MovieTexture)videoHolder.GetComponent<Renderer>().material.mainTexture;
			movieTexture_.Play();
			Invoke ("LoadMain", 20f);
		}
	#elif UNITY_ANDROID
		{
			Debug.Log("Android stuff here");
			StartCoroutine(LoadVideo ("animation.mp4"));
			LoadMain ();
		}
	#else
		LoadMain();
	#endif*/
		Debug.Log("Android stuff here");
		StartCoroutine(LoadVideo ("AhonBook.mp4"));
		LoadMain ();
	}

	IEnumerator LoadVideo(string path)
	{
		//WWW link = new WWW (path);
		//yield return link;
		Handheld.PlayFullScreenMovie (path, Color.black, FullScreenMovieControlMode.Hidden,
		                             FullScreenMovieScalingMode.AspectFill);
		yield return new WaitForEndOfFrame ();
		yield return new WaitForEndOfFrame ();
		Debug.Log("Slideshow Done");
	}

	public void LoadMain()
	{
		Application.LoadLevel ("Main");
	}
}
