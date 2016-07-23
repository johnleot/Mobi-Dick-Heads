using UnityEngine;
using System.Collections;

/*
	ABOUT THIS SCRIPT
	
This script works similar to SnappingUnits, except instead of
aligning an entire Transform it aligns just a Vector3. You can
use this approach if you want more control over the point itself
instead of manipulatingt he Transform directly.

The scale variable decides whether to align to vertices or edges,
depending on whether the components are even or odd multiples
of the grid's spacing.

Make sure gizmos are turned on in the game view to see the results.
*/

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(GFGrid))]
public class PointDebug : MonoBehaviour {
	// the "scale" we use to determine how exactly to align the point (see the scripting reference for AlignVector3)
	public Vector3 scale = new Vector3 (1, 2, 1);
	
	private bool debugPoints = false; //this will be true while the player is holding down the mouse button above the grid
	private Vector3 point; // the point the pleyer is pointing to

	private Collider col; // the collider of the grid (used for handling mouse input)
	private GFGrid grid; // the grid component

	void Awake () { // store components for later reference
		col = GetComponent<Collider>();
		grid = GetComponent<GFGrid> ();
	}

	void OnMouseDown () {
		debugPoints = true; // start the debugging process
	}

	void OnMouseUp () {
		debugPoints = false; // stop the debugging process
	}

	void Update () {
		if (!debugPoints) //only debug while the player is gragging the mouse over the grid
			return;

		//handle mouse input here
		RaycastHit hit;
		col.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity);
		point = hit.collider != null ? hit.point : transform.position;
	}

	void OnDrawGizmos () {
		if (!debugPoints)
			return;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (point, 0.3f); // where the plyer is pointing at
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (grid.AlignVector3(point, scale), 0.3f); // the aligned point
	}
}
