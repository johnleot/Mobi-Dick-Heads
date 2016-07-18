using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SlideShow : MonoBehaviour {

	public Sprite[] images = new Sprite[16];
	public Image page;
	int alphaIn = 1;
	int alphaOut = 0;
	public AudioMixerSnapshot cinematics;
	public AudioMixerSnapshot unMuteBG;

	// Use this for initialization
	void Start () 
	{
		cinematics.TransitionTo (0.1f);
		page = GetComponent<Image> ();
		page.sprite = images [0];
		StartCoroutine (FadeInFadeOut ());
	}
	
	public IEnumerator FadeInFadeOut()
	{
		for (int i = 0; i < images.Length; i++)
		{
			yield return new WaitForSeconds(1);
			page.sprite = images [i];
			page.CrossFadeAlpha(alphaIn, 1f, false);
			yield return new WaitForSeconds(5);
			page.CrossFadeAlpha(alphaOut, 1f, false);
		}

	}

	public void skipShow()
	{
		unMuteBG.TransitionTo (0.1f);
		Application.LoadLevel ("Main");
	}
}
