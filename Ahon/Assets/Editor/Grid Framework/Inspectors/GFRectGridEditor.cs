using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(GFRectGrid))]
public class GFRectGridEditor : GFGridEditor {
	protected GFRectGrid rGrid {get{return (GFRectGrid)grid;}}
	
	protected override void SpacingFields () {
		rGrid.spacing = EditorGUILayout.Vector3Field("Spacing", rGrid.spacing);
	}
}