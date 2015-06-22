using UnityEngine;
using System.Collections;
using Assets.Scripts.Classes;

public class ObjectSpawner : MonoBehaviour
{ 
	private static Resource resourceTool;
	GameObject thePrefab;
	
	void Start()
	{
		resourceTool = new Resource ("resource","resource");
	}
	
	public void resourceFilter(string resource)
	{
		
	}
	//spawn/instantiate object
	public void SpawnObject()
	{
		// initialize class and variables
		
		Quaternion rot = Quaternion.identity;
		thePrefab = Instantiate(Resources.Load("PrefabToInstantiate/GameResource"),new Vector3(110,28,146),Quaternion.Euler(270,236,0)) as GameObject;
	}
}

