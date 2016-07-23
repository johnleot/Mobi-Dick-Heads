using UnityEngine;
using System.Collections;

public class VideoLoaderScript : MonoBehaviour {
//	public GameObject videoHolder;
	// Use this for initialization
	void Start () {
//	/*#if UNITY_EDITOR
//		{
//			videoHolder.SetActive(true);
//			MovieTexture movieTexture_ = (MovieTexture)videoHolder.GetComponent<Renderer>().material.mainTexture;
//			movieTexture_.Play();
//			Debug.Log("Played in Editor");
//			Invoke ("LoadMain", 20f);
//		}
//	#elseif UNITY_ANDROID
//		{
//			Debug.Log("Android stuff here");
//			StartCoroutine(LoadVideo ("AhonBook.mp4"));
//			LoadMain ();
//		}
//	#else
//		LoadMain();
//	#endif*/
		Debug.Log("Android stuff here");
		Handheld.PlayFullScreenMovie ("AhonVideo3.mp4", Color.black, FullScreenMovieControlMode.Hidden,
		                              FullScreenMovieScalingMode.AspectFit);
//		StartCoroutine(LoadVideo ("AhonVideo3.mp4"));
		Debug.Log ("Slideshow Done");
		LoadMain ();
	}

//	IEnumerator LoadVideo(string path)
//	{
//
//		Debug.Log ("More Stuff of Android");
//		Handheld.PlayFullScreenMovie (path, Color.black, FullScreenMovieControlMode.Hidden,
//		                             FullScreenMovieScalingMode.AspectFit);
//		yield return new WaitForEndOfFrame ();
////		yield return new WaitForEndOfFrame ();
//		Debug.Log("Slideshow Done");
//	}

	public void LoadMain()
	{
		Application.LoadLevel ("Main");
	}
}
