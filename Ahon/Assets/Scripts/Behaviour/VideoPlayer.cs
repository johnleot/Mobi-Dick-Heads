using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Behaviour
{
	[RequireComponent (typeof (AudioSource)) ]

	public class VideoPlayer : MonoBehaviour
	{
		public MovieTexture movie;
	
		// Use this for initialization
		void Awake ()
		{
			GetComponent<Renderer>().material.mainTexture = movie as MovieTexture;
			movie.Play();
			GetComponent<AudioSource>().clip = movie.audioClip;
			GetComponent<AudioSource>().Play();
		}

		void OnMouseDown()
		{
			movie.Stop();
			Application.LoadLevel("Main");
		}

		void Update()
		{
			if(!movie.isPlaying) Application.LoadLevel("Main");
		}
	}
}
