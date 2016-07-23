using UnityEngine;
using System.Collections;

public class PolarConversionDebugger : MonoBehaviour {
	public GFPolarGrid grid;
	public bool rotateAroundGrid = false;
	public GFAngleMode angleMode = GFAngleMode.radians;

	void OnGUI () {
		if (!grid){
			Debug.LogWarning ("No grid assigned, cannot debug");
			return;
		}
		GUI.TextArea(
			new Rect (10, 10, 600, 150),
			"world position:\t" + transform.position.x +" / "+ transform.position.y +" / "+ transform.position.z + "\n"
			+"grid position:\t" +
			grid.WorldToGrid(transform.position).x +" / "+ grid.WorldToGrid(transform.position).y +" / "+ grid.WorldToGrid(transform.position).z +"\n"
			+"polar position:\t" + grid.WorldToPolar(transform.position).x +" / "+ grid.WorldToPolar(transform.position).y +" / "+ grid.WorldToPolar(transform.position).z +"\n\n"
			+"angle :\t" + grid.World2Angle(transform.position, GFAngleMode.radians) +" = "+ (grid.World2Angle(transform.position, GFAngleMode.radians) / Mathf.PI) +"\u03c0 = " + grid.World2Angle(transform.position, GFAngleMode.degrees) + "\u00b0\n"
			+"sector: \t" + grid.World2Sector(transform.position) +"\n\n"
			+"sector converted from angle:\t" + grid.Angle2Sector(grid.World2Angle(transform.position, angleMode), angleMode)+"\n"
			+"angle converted from Sector:\t" + grid.Sector2Angle(grid.World2Sector(transform.position), GFAngleMode.radians) +" = "+ grid.Sector2Angle(grid.World2Sector(transform.position), GFAngleMode.degrees) +"\n"
		);
		if(rotateAroundGrid)
			transform.rotation = grid.World2Rotation(transform.position);
	}
}
