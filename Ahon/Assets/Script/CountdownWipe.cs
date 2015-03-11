using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CountdownWipe : MonoBehaviour
{
	public float calamityTimeRemaining; //eta of calamity is fetched from database
	public float calamityTimeToComplete; // calamity duration is fetched from database
	public Image calamity;
	public Image duration;
	public Text over;

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
				over.text = "Game Over";
			}
		}
	}
}
