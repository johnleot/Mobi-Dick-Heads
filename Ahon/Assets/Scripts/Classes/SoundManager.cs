using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Classes 
{
	public class SoundManager : MonoBehaviour
	{
		public AudioSource bgMusic;
		public AudioSource soundFX;
		public static SoundManager instance = null;
	
		public float lowPitch = .95f;
		public float highPitch = 1.05f:
	
		// Use this for initialization
		void Awake ()
		{
	
			if (instance == null)
				instance = this;
			else if (instance != null)
				Destroy (gameObject);
	
			DontDestroyOnLoad (gameObject);
		
		}
	
		public void PlaySingle(AudioClip clip)
		{
			soundFX.clip = clip;
			soundFX.Play ();
		}
	
		public void RandomizeFX (params AudioClip [] clips)
		{
			int randomIndex = Random.Range (0, clips.Length);
			float randomPitch = Random.Range (lowPitch, highPitch);
	
			soundFX.pitch = randomPitch;
			soundFX.clip = clips [randomIndex];
	
			soundFX.Play ();
	
		}
	}
}
