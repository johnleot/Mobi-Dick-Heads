using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CountdownWipe : MonoBehaviour
{
	float calamityTimeRemaining = 5.0f; //eta of calamity is fetched from database
	float calamityTimeToComplete = 10.0f; // calamity duration is fetched from database
	public Image calamity;
	public Image duration;

	void Update ()
	{
		if (calamityTimeRemaining > 0.0f)
		{
			calamity.fillAmount = Mathf.MoveTowards (calamity.fillAmount, 1.0f, Time.deltaTime / calamityTimeRemaining);
			calamityTimeRemaining -= Time.deltaTime;
		}
		else
		{
			if (calamityTimeToComplete > 0.0f)
			{
				duration.fillAmount = Mathf.MoveTowards (duration.fillAmount, 1.0f, Time.deltaTime / calamityTimeToComplete);
				calamityTimeToComplete -= Time.deltaTime;
			}
			else
			{
				GUI.Label(new Rect(100,100,200,100), "Game Over");
			}
		}
	}
}
