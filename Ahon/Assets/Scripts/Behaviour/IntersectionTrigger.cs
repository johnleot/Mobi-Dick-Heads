using UnityEngine;
using System.Collections;

public class IntersectionTrigger : MonoBehaviour {
	
	private SnappingUnits snappingScript;
	
	public void SetSnappingScript(SnappingUnits script){
		snappingScript = script;
	}

	void OnTriggerEnter(Collider other){
		snappingScript.SetIntersecting(true);
		if(other.gameObject.tag == "StationaryGO")
			snappingScript.alterYOffset (other.transform.lossyScale.y);
	}
	
	void OnTriggerStay(Collider other){
		if(other.gameObject.tag == "StationaryGO")
			snappingScript.alterYOffset (other.transform.lossyScale.y);
	}

	void OnTriggerExit(Collider other){
		snappingScript.SetIntersecting(false);
		if(other.gameObject.tag == "StationaryGO")
			snappingScript.resetObstacleHeightTotheGround();
	}
}