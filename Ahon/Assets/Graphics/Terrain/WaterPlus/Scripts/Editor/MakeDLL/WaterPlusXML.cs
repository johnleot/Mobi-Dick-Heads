using UnityEngine;
using System.Collections;

using System.Xml;
using System.Xml.Serialization;

namespace WaterPlusEditorInternal {
	public class WPMesh
	{
		public int vertexCount;
		public string name;
		public int lightmapIndex;
		public Vector4 tilingOffset;
		public Vector4[] localToWorldMatrix;
		public Vector3[] vertices;
		public int[] triangles;
		public Vector2[] uvs;
	}
	
	public class WPTerrainData {
		public string name;
		public int width;
		public int height;
		public int lightmapIndex;
		public Vector3 position;
		public float[] heightmap;
	}
	
	public class WPLightmapData
	{	
		public string[] lightmapFiles;
		public float waterLevel;
		public float wetnessHeight;
		public float wetnessAmount;
		public WPMesh[] meshes;
		public WPTerrainData[] terrains;
	}
}