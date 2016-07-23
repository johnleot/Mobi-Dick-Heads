using UnityEngine;
using UnityEditor;
using System.Collections;

public abstract class GFGridEditor : Editor {
	protected GFGrid grid;
	protected bool showDrawSettings;
	
	#region enable <-> disable
	void OnEnable () {
		grid = target as GFGrid;
		showDrawSettings = EditorPrefs.HasKey("GFGridShowDraw") ? EditorPrefs.GetBool("GFGridShowDraw") : true;
	}
	
	void OnDisable(){
		EditorPrefs.SetBool("GFGridShowDraw", showDrawSettings);
	}
	#endregion
	
	#region OnInspectorGUI()
	public override void OnInspectorGUI () {
		SizeFields();
		SpacingFields();
		
		EditorGUILayout.Space();
		ColourFields();
		
		EditorGUILayout.Space();
		DrawRenderFields();
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}
	#endregion
	
	#region groups of common fields
	protected virtual void SizeFields () {
		grid.relativeSize = EditorGUILayout.Toggle("Relative Size", grid.relativeSize);
		grid.size = EditorGUILayout.Vector3Field("Size", grid.size);
	}
	
	protected void ColourFields () {		
		GUILayout.Label("Axis Colors");
		
		EditorGUILayout.BeginHorizontal();
		++EditorGUI.indentLevel;
		grid.axisColors.x = EditorGUILayout.ColorField(grid.axisColors.x);
		grid.axisColors.y = EditorGUILayout.ColorField(grid.axisColors.y);
		grid.axisColors.z = EditorGUILayout.ColorField(grid.axisColors.z);
		--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		
		grid.useSeparateRenderColor = EditorGUILayout.Foldout(grid.useSeparateRenderColor, "Use Separate Render Color");
		if(grid.useSeparateRenderColor){
			EditorGUILayout.BeginHorizontal();
			++EditorGUI.indentLevel;
			grid.renderAxisColors.x = EditorGUILayout.ColorField(grid.renderAxisColors.x);
			grid.renderAxisColors.y = EditorGUILayout.ColorField(grid.renderAxisColors.y);
			grid.renderAxisColors.z = EditorGUILayout.ColorField(grid.renderAxisColors.z);
			--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		}
		
		grid.vertexColor = EditorGUILayout.ColorField("Vertex Color", grid.vertexColor);
		
		
	}
	
	protected void DrawRenderFields () {
		showDrawSettings = EditorGUILayout.Foldout(showDrawSettings, "Draw & Render Settings");
		++EditorGUI.indentLevel;
		if(showDrawSettings){
			grid.renderGrid = EditorGUILayout.Toggle("Render Grid", grid.renderGrid);
			
			grid.useCustomRenderRange = EditorGUILayout.Foldout(grid.useCustomRenderRange, "Custom Rendering Range");
			if(grid.useCustomRenderRange){
				grid.renderFrom = EditorGUILayout.Vector3Field("Render From", grid.renderFrom);
				grid.renderTo = EditorGUILayout.Vector3Field("Render To", grid.renderTo);
			}
			
			grid.renderMaterial = (Material) EditorGUILayout.ObjectField("Render Material", grid.renderMaterial, typeof(Material), false);
			grid.renderLineWidth = EditorGUILayout.IntField("Rendered Line Width", grid.renderLineWidth);
			
			grid.hideGrid = EditorGUILayout.Toggle("Hide Drawing", grid.hideGrid);
			grid.hideOnPlay = EditorGUILayout.Toggle("Hide While playing", grid.hideOnPlay);
			++EditorGUI.indentLevel;
			GUILayout.Label("Hide Axis (Render & Draw)");
			grid.hideAxis.x = EditorGUILayout.Toggle("X", grid.hideAxis.x);
			grid.hideAxis.y = EditorGUILayout.Toggle("Y", grid.hideAxis.y);
			grid.hideAxis.z = EditorGUILayout.Toggle("Z", grid.hideAxis.z);
			--EditorGUI.indentLevel;
			
			grid.drawOrigin = EditorGUILayout.Toggle("Draw Origin", grid.drawOrigin);
		}
		--EditorGUI.indentLevel;
	}
	#endregion
	
	#region groups of specific fields (abstract)
	protected abstract void SpacingFields ();
	#endregion
}
