using UnityEngine;
using System.Collections;

public class IntersectionTrigger : MonoBehaviour {
	
	private SnappingUnits snappingScript;
	
	public void SetSnappingScript(SnappingUnits script){
		snappingScript = script;
	}

	void OnTriggerEnter(Collider other){
		snappingScript.SetIntersecting(true);
	}
	
	void OnTriggerExit(Collider other){
		snappingScript.SetIntersecting(false);
	}
}