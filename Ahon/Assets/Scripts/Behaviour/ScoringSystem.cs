using UnityEngine;
using System.Collections;

public class ScoringSystem : MonoBehaviour {
    private int environmentRating;

    public int EnvironmentRating
    {
        get { return environmentRating; }
        set { environmentRating = value; }
    }
    private int peopleRating;

    public int PeopleRating
    {
        get { return peopleRating; }
        set { peopleRating = value; }
    }
    private int foodConsumption;

    public int FoodConsumption
    {
        get { return foodConsumption; }
        set { foodConsumption = value; }
    }
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
