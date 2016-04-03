using UnityEngine;
using System.Collections;

public class InitScript : MonoBehaviour {

	void LoadMain()
	{
		if (isNewPlayer ())
			Application.LoadLevel ("Cinematics");
		else
			Application.LoadLevel ("Main");
	}

	bool isNewPlayer()
	{
		// some logic
//		return true;
		return false;
	}

	void Awake()
	{
		// some code to check/fetch user information (database querying)
		DontDestroyOnLoad (gameObject);
		Invoke ("LoadMain", 3);
	}

    public void LoadMainMenu()
    {
        Application.LoadLevel("Main");
    }
}
