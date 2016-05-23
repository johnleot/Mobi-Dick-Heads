using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(GFHexGrid))]
public class GFHexGridEditor : GFGridEditor {
	protected GFHexGrid hGrid {get{return (GFHexGrid)grid;}}
	
	protected override void SpacingFields () {
		hGrid.radius = EditorGUILayout.FloatField("Radius", hGrid.radius);
		hGrid.depth = EditorGUILayout.FloatField("Depth", hGrid.depth);
		hGrid.gridPlane = (GFGrid.GridPlane) EditorGUILayout.EnumPopup("Grid Plane", hGrid.gridPlane);
		hGrid.hexSideMode = (GFHexGrid.HexOrientation) EditorGUILayout.EnumPopup("Hex Side Mode", hGrid.hexSideMode);
		hGrid.gridStyle = (GFHexGrid.HexGridShape)EditorGUILayout.EnumPopup("Grid Style", hGrid.gridStyle);
	}
}
