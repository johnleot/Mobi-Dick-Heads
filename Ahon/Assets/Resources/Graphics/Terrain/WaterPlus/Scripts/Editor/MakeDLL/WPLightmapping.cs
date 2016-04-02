using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using MiniEXR;

namespace WaterPlusEditorInternal {
	public class ImageData {
		public int width;
		public int height;

		public Color[] srcPixels;
		public Color[] dstPixels;

		public string filePath;
		public string assetPath;
		
		public ImageData(string _filePath) {
			filePath = _filePath;
			
			if ( !File.Exists (filePath) ) {
				Debug.LogError("lightmap doesn't exist at path: '" + filePath + "'");
			}
			
			assetPath = WPHelper.FilePathToAssetPath(filePath);

			TextureImporter tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
	        if( tImporter != null ) {
				tImporter.textureType = TextureImporterType.Image;
				tImporter.linearTexture = true;
	            tImporter.textureFormat = TextureImporterFormat.RGBA32;
				tImporter.isReadable = true;
	            AssetDatabase.ImportAsset(assetPath);                
	        }
			
			Texture2D tex = AssetDatabase.LoadAssetAtPath( assetPath, typeof( Texture2D ) ) as Texture2D;
			
			if (null == tex) {
				Debug.LogError("Failed to load the lightmap at path: " + assetPath);
				return;
			}
			
			width = tex.width;
			height = tex.height;
			
			srcPixels = tex.GetPixels();
			dstPixels = tex.GetPixels();
			
			if( tImporter != null ) {
				tImporter.textureType = TextureImporterType.Lightmap;
				tImporter.linearTexture = true;
	            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				tImporter.isReadable = false;
	            AssetDatabase.ImportAsset(assetPath);                
	        }
			
			//WPHelper.MakeTextureReadable( tex, false );
			
//			dstPixels = new Color[width * height];
//			
//			for (int i = 0; i < width*height; i++) {
//				dstPixels[i] = new Color(.5f, .5f, .5f, 1f);
//			}
			
//			for (int x = 0; x < width; x++) {
//				//Debug.Log(dstPixels[x].r * 255f + " " + dstPixels[x].g * 255f + " " + dstPixels[x].b * 255f);	
//			}
			
			Save();
		}
		
		public void Save() {
			//Flip on Y axis first
			Color[] flippedPixels = new Color[width * height];
			
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					int index = y * width + x;
					int indexFlipped = (height - y - 1) * width + x;
					
					flippedPixels[indexFlipped] = dstPixels[index];
				}
			}
			
			MiniEXR.MiniEXR.MiniEXRWrite(filePath, (uint)width, (uint)height, flippedPixels);
		}
	}
		
	public class WPLightmapping {
		private static Transform waterSurfaceTransform;
		private static string lightmapWetnessHeightString, lightmapWetnessAmountString;
		
		
		private static void BakeLightmaps (WPLightmapData lightmapData)
		{
//			Debug.Log ("waterLevel: " + lightmapData.waterLevel);
//			Debug.Log ("wetnessHeight: " + lightmapData.wetnessHeight);
//			Debug.Log ("wetnessAmount: " + lightmapData.wetnessAmount);
			//
			//Load all of the lightmaps
			ImageData[] imagesData = new ImageData[lightmapData.lightmapFiles.Length];
			
			for (int lightmapIndex = 0; lightmapIndex < lightmapData.lightmapFiles.Length; lightmapIndex++) {
				bool shouldSkipFile = false;
				//Don't load non-used lightmaps
				if (lightmapData.lightmapFiles [lightmapIndex] == null)
					shouldSkipFile = true;
				else if (lightmapData.lightmapFiles.Length <= 0)
					shouldSkipFile = true;
				
				if (shouldSkipFile) {
					Debug.Log ("Skipping lightmap " + lightmapIndex + " because the lightmap is non-used");
					continue;
				}
				
				string filePath = lightmapData.lightmapFiles [lightmapIndex];
				//string filePath = "C:/Users/Me/Desktop/Water+/iPhone4_hq_1.png";
				//string filePath = "1/LightmapFar-0.exr";
				//string filePath = "C:/Users/Me/waterplus/Assets/WaterPlus/Water_Island/LightmapFar-0.exr";
				//Debug.Log("lightmap path: " + filePath);
				imagesData [lightmapIndex] = new ImageData (filePath);
				
				/*for (int i = 0; i < imagesData[lightmapIndex].width * imagesData[lightmapIndex].height; i++) {
					imagesData[lightmapIndex].pixels[i].r = imagesData[lightmapIndex].pixels[i].r * imagesData[lightmapIndex].pixels[i].r * .5f;
					imagesData[lightmapIndex].pixels[i].g = imagesData[lightmapIndex].pixels[i].g * imagesData[lightmapIndex].pixels[i].g * 2.0f;
					imagesData[lightmapIndex].pixels[i].b = imagesData[lightmapIndex].pixels[i].b * imagesData[lightmapIndex].pixels[i].b;
				}*/
				
				//Debug.Log("Successfully loaded lightmap at path " + filePath);
				//break;
			}
			
			//Debug.Log ("Successfully loaded lightmaps.");

			//return;
			
			System.DateTime bakeStartTime = System.DateTime.Now;
			
			//int count = 0;
			
			//1. Go over all pixels of the mesh
			//2. Convert UV to vertex
			//3. If the vertex is within water line, update the lightmap.
			//Debug.Log ("Updating lightmaps.");
			int bakeCount = 0;
			int totalObjectsToBake = lightmapData.meshes.Length + lightmapData.terrains.Length;

			//
			//Regular meshes
			foreach (WPMesh obj in lightmapData.meshes) {
				if (obj.lightmapIndex < 0 || obj.lightmapIndex >= imagesData.Length) {
					Debug.Log("skipping " + obj + " because of a wrong lightmapIndex: " + obj.lightmapIndex);
					continue;
				}
				
				//Calculate object's texel size
				ImageData lightmapUsed = imagesData [obj.lightmapIndex];
				int lightmapResolution = lightmapUsed.width;
				Vector4 tilingOffset = obj.tilingOffset;

				if (lightmapResolution == 0) {
					Debug.Log("error: lightmapResolution == 0");
					continue;
				}

				if (tilingOffset.x == 0) {
					Debug.LogError("error: tilingOffset.x == 0");
					continue;
				}
				
				float pixelSize = 1.0f / (tilingOffset.x * (float)lightmapResolution);

				//Debug.Log("tilingOffset: " + tilingOffset.ToString() + " " + lightmapResolution);
				
				//Console
				
				//Debug.Log("2");
				
				Vector2[] meshUVs = obj.uvs;
				if (meshUVs == null) {
					Debug.Log ("No UVs found for " + obj.name);
					continue;
				}
				
				bakeCount++;
				
				//long searchIterations = ((long)(1.0f / (pixelSize * pixelSize)) * (long)obj.vertexCount);
				
				
				//Debug.Log ("baking " + obj.name + " with " + searchIterations +
				//                  " search iterations; progress: " + (100.0f * bakeCount / lightmapData.meshes.Length).ToString("0") + "%" );
				//Debug.Log ("baking " + obj.name + ". total progress: " + (100.0f * bakeCount / totalObjectsToBake).ToString("0") + "%" );
				
				//Debug.Log(obj.name + " at position " + obj.);
				//Debug.Log ("pixelSize: " + pixelSize + " iterations: " + (1.0f / (pixelSize * pixelSize)));
				
				
				//int verticesFound = 0;
				//int verticesNotFound = 0;
				//Iterate over all UVs in the mesh
				//float previousProgress = 0.0f;
				//int totalPixels = 0;
				//int pixelsProcessed = 0;

				int updatedPixels = 0;

				//Debug.Log("pixelSize: " + pixelSize);
				Matrix4x4 tempMatrix = new Matrix4x4();
				tempMatrix.SetRow(0, obj.localToWorldMatrix[0]);
				tempMatrix.SetRow(1, obj.localToWorldMatrix[1]);
				tempMatrix.SetRow(2, obj.localToWorldMatrix[2]);
				tempMatrix.SetRow(3, obj.localToWorldMatrix[3]);
				//Debug.Log( tempMatrix.GetRow(3) );
				//Debug.Log("1 * matrix: " + tempMatrix.MultiplyPoint(Vector3.one) );


				for (float u = 0.0f; u < 1.0f; u += pixelSize) {
					//Progress for large objects
					//if (1.0f / (pixelSize * pixelSize) >= 10000.0f) {
					//Make sure to log only every percent, not less
//					if (u - previousProgress >= .01f) {
//						Console.Write("\r\t" + obj.name + " progress: " + (u * 100.0f).ToString("0") + "%");
//						previousProgress = u;
//					}
					//}
					
					for (float v = 0.0f; v < 1.0f; v += pixelSize) {
						//totalPixels++;

						//Convert UV to vertex position
						Vector2 currentUV = new Vector2 (u, v);
						bool vertexFound = false;
						Vector3 vertexPos;
						WPHelper.UVToVertex (currentUV, obj, meshUVs, out vertexFound, out vertexPos);
						
						//Update the lightmap
						//vertexFound = true;
						if (vertexFound)
						{

							vertexPos = tempMatrix.MultiplyPoint (vertexPos);

							//if (u >= .45f && u <= .55f)
							//if (1.0f / (pixelSize * pixelSize) >= 10000.0f)
							//	Console.Write(vertexPos.y + "\t");

							//Debug.Log("vertexPos.y: " + vertexPos.y);
							if (vertexPos.y <= lightmapData.waterLevel + lightmapData.wetnessHeight)
							{
								//Convert object's UV to lightmap's UV
								Vector2 lightmapUV = currentUV;
								lightmapUV.x *= tilingOffset.x;
								lightmapUV.y *= tilingOffset.y;
								
								lightmapUV.x += tilingOffset.z;
								lightmapUV.y += tilingOffset.w;
								
								int lightmapX = (int)(lightmapUV.x * (float)lightmapResolution);
								int lightmapY = (int)(lightmapUV.y * (float)lightmapResolution);
								
								
								if (lightmapX < 0 || lightmapX >= lightmapResolution || lightmapY < 0 || lightmapY >= lightmapResolution) {
									//Debug.LogWarning("lightmapX: " + lightmapX + " lightmapY: " + lightmapY);
								} else {
									//lightmapPixels[lightmapY * lightmapResolution + lightmapX] = Color.yellow;
									float gradientAmount = 0.0f;
									
									if (vertexPos.y <= lightmapData.waterLevel) {
										gradientAmount = 1.0f;	
									} else if (vertexPos.y > lightmapData.waterLevel + lightmapData.wetnessHeight) {
										gradientAmount = 0.0f;
									} else {
										gradientAmount = 1.0f - (vertexPos.y - lightmapData.waterLevel) / lightmapData.wetnessHeight;
									}
									
									gradientAmount = 1.0f - gradientAmount * lightmapData.wetnessAmount;
									
									gradientAmount = Mathf.Clamp01( gradientAmount );	//Just in case
									
									lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].r = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].r * gradientAmount;
									lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].g = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].g * gradientAmount;
									lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].b = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].b * gradientAmount;
									//pixelsProcessed++;

									updatedPixels++;

									//lightmapPixels [lightmapY * lightmapResolution + lightmapX].r = 1.0f;
									//lightmapPixels [lightmapY * lightmapResolution + lightmapX].g = 1.0f;
									//lightmapPixels [lightmapY * lightmapResolution + lightmapX].b = 0.0f;
								}
							}
							
							//verticesFound++;
						}
					}
					
					
				}

//				Console.Write("\r\t" + obj.name + " progress:  100%");
//				Console.Write("\n");
//				Debug.Log("lowestVertexLocal: " + lowestVertexLocal + "; heighestVertexLocal: " + heighestVertexLocal +
//				                  "; lowestVertexWorld: " + lowestVertexWorld + "; heighestVertexWorld: " + heighestVertexWorld);
			}

			//
			//Terrains
			foreach (WPTerrainData obj in lightmapData.terrains) {
				if (obj.lightmapIndex < 0 || obj.lightmapIndex >= imagesData.Length) {
					Debug.Log("skipping " + obj + " because of a wrong lightmapIndex: " + obj.lightmapIndex);
					continue;
				}
				
				//Calculate object's texel size
				ImageData lightmapUsed = imagesData [obj.lightmapIndex];
				int lightmapResolution = lightmapUsed.width;

				if (lightmapResolution == 0) {
					Debug.Log("error: lightmapResolution == 0");
					continue;
				}
				
				float pixelSize = 1.0f / (float)lightmapResolution;
				
				bakeCount++;

//				Debug.Log ("baking " + obj.name + ". total progress: " + (100.0f * bakeCount / totalObjectsToBake).ToString("0") + "%" );

//				float previousProgress = 0.0f;
				
				for (float u = 0.0f; u < 1.0f; u += pixelSize) {
					//Progress for large objects
					//if (1.0f / (pixelSize * pixelSize) >= 10000.0f) {
					//Make sure to log only every percent, not less
//					if (u - previousProgress >= .01f) {
//						Console.Write("\r\t" + obj.name + " progress: " + (u * 100.0f).ToString("0") + "%");
//						previousProgress = u;
//					}
					
					for (float v = 0.0f; v < 1.0f; v += pixelSize) {
						//Update the lightmap
						//int heightmapX = obj.width - (int)(u * (float)obj.width);
						//int heightmapY = obj.height - (int)(v * (float)obj.height);
						int heightmapX = (int)(v * (float)obj.height);
						int heightmapY = (int)(u * (float)obj.width);

						if (heightmapX < 0 || heightmapX >= obj.width || heightmapY < 0 || heightmapY >= obj.height)
							continue;

						int lightmapX = (int)(u * (float)lightmapResolution);
						int lightmapY = (int)(v * (float)lightmapResolution);

						if (lightmapX < 0 || lightmapX >= lightmapResolution || lightmapY < 0 || lightmapY >= lightmapResolution)
							continue;

						float yPos = obj.position.y + obj.heightmap[heightmapY * obj.height + heightmapX];

						if (yPos <= lightmapData.waterLevel + lightmapData.wetnessHeight)
						{
							//if (yPos > 0)
							//	Debug.Log("yPos: " + yPos + " waterLevel: " + lightmapData.waterLevel);

							float gradientAmount = 0.0f;
							
							if (yPos <= lightmapData.waterLevel) {
								gradientAmount = 1.0f;	
							} else if (yPos > lightmapData.waterLevel + lightmapData.wetnessHeight) {
								gradientAmount = 0.0f;
							} else {
								gradientAmount = 1.0f - (yPos - lightmapData.waterLevel) / lightmapData.wetnessHeight;
							}
							
							gradientAmount = 1.0f - gradientAmount * lightmapData.wetnessAmount;
							
							gradientAmount = Mathf.Clamp01( gradientAmount );	//Just in case
							
							lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].r = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].r * gradientAmount;
							lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].g = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].r * gradientAmount;
							lightmapUsed.dstPixels [lightmapY * lightmapResolution + lightmapX].b = lightmapUsed.srcPixels[lightmapY * lightmapResolution + lightmapX].r * gradientAmount;
						}
						
						//verticesFound++;
					}
					
					
				}
//				Console.Write("\r\t" + obj.name + " progress:  100%");
//				Console.Write("\n");
			}
			
			
			//
			//Save all the lightmaps
			for (int i = 0; i < imagesData.Length; i++) {
				//string newPath = AppDomain.CurrentDomain.BaseDirectory + "Lightmap_" + i + ".exr";
				//imagesData[i].Save( newPath );
				if (null == imagesData[i])
					continue;
				
				imagesData[i].Save();
			}
			
			Debug.Log("Successfully updated the lightmaps in " + (System.DateTime.Now - bakeStartTime).TotalSeconds + " seconds.");
			
		}
		
		private static WPLightmapData PrepareLightmapData(string[] lightmapPaths, GameObject[] affectedObjects, float waterLevel, float wetnessHeight, float wetnessAmount) {
			//Debug.Log("waterLevel: " + waterLevel);
			WPLightmapData wpLightmapData = new WPLightmapData();
			
			wpLightmapData.waterLevel = waterLevel;
			wpLightmapData.wetnessHeight = wetnessHeight;
			wpLightmapData.wetnessAmount = wetnessAmount;
			
			
			wpLightmapData.lightmapFiles = new string[ lightmapPaths.Length ];
			for (int i = 0; i < lightmapPaths.Length; i++) {
				wpLightmapData.lightmapFiles[i] = lightmapPaths[i];
				//Debug.Log("Adding lightmap: " + lightmapPaths[i]);
			}
			
			//
			//Normal meshes
			List<WPMesh> wpMeshes = new List<WPMesh>();
			
			foreach (GameObject obj in affectedObjects) {
				Renderer rend = obj.GetComponent<Renderer>();
				
				if (rend == null)
					continue;
				
				//Skip terrains for now
				Terrain terrain = obj.GetComponent<Terrain>();
				if (terrain != null) {
					Debug.LogWarning("skipping terrain");
					continue;
				}
				
				if (rend.lightmapIndex < 0 || rend.lightmapIndex >= LightmapSettings.lightmaps.Length)
					continue;
				
				MeshFilter meshFilter = rend.gameObject.GetComponent<MeshFilter>();
			
				if (meshFilter == null)
					continue;
				
				Mesh mesh = meshFilter.sharedMesh;
				
				WPMesh wpMesh = new WPMesh();
				
				string objName = rend.name;
				if (objName == "default") {
					objName = obj.gameObject.name;
				}
				
				if (objName == "default") {
					objName = obj.gameObject.transform.parent.gameObject.name;
				}
				
				if (null == mesh) {
					Debug.LogError("mesh of " + objName + " is null. Skipping.");
					continue;
				}
				
				wpMesh.name = objName;
				
				wpMesh.lightmapIndex = rend.lightmapIndex;
				wpMesh.tilingOffset = rend.lightmapScaleOffset;
				
				wpMesh.localToWorldMatrix = new Vector4[4];
				wpMesh.localToWorldMatrix[0] = obj.transform.localToWorldMatrix.GetRow(0);
				wpMesh.localToWorldMatrix[1] = obj.transform.localToWorldMatrix.GetRow(1);
				wpMesh.localToWorldMatrix[2] = obj.transform.localToWorldMatrix.GetRow(2);
				wpMesh.localToWorldMatrix[3] = obj.transform.localToWorldMatrix.GetRow(3);
				
				//Debug.LogWarning("1 * matrix: " + obj.transform.localToWorldMatrix.MultiplyVector(Vector3.one) );
				
				wpMesh.vertexCount = mesh.vertexCount;
				wpMesh.vertices = mesh.vertices;
				
				if (mesh.uv2 == null) {
					wpMesh.uvs = mesh.uv;	
				} else {
					if (mesh.uv2.Length <= 0)
						wpMesh.uvs = mesh.uv;
					else
						wpMesh.uvs = mesh.uv2;	
				}
				
				wpMesh.triangles = mesh.triangles;
				
				if (wpMesh.vertices == null) {
					Debug.LogWarning("No vertices found for " + objName + ". Skipping.");
					continue;
				} else {
					if (wpMesh.vertices.Length <= 0) {
						Debug.LogWarning("No vertices found for " + objName + ". Skipping.");
						continue;
					}
				}
				
				if (wpMesh.uvs == null) {
					Debug.LogWarning("No UVs found for " + objName + ". Skipping.");
					continue;
				} else {
					if (wpMesh.uvs.Length <= 0) {
						Debug.LogWarning("No UVs found for " + objName + ". Skipping.");
						continue;
					}
				}
				
				wpMeshes.Add(wpMesh);
			}
			
			wpLightmapData.meshes = wpMeshes.ToArray();
			
			//
			//Terrains meshes
			List<WPTerrainData> wpTerrains = new List<WPTerrainData>();
			
			foreach (GameObject obj in affectedObjects) {
				//break;
				
				Terrain terrain = obj.GetComponent<Terrain>();
				
				if (terrain == null)
					continue;
				
				//Debug.LogWarning("Adding terrain");
				
				/*Renderer rend = obj.renderer;
				
				if (rend == null) {
					Debug.LogWarning("No renderer found for the terrain.");
					continue;
				}*/
				
				TerrainData terrainData = terrain.terrainData;
				
				//Debug.Log("terrainData.size.y: " + terrainData.size.y);
				
				//Debug.Log("terrain resolution: " + terrainData.heightmapResolution);
				
				if (terrain.lightmapIndex < 0 || terrain.lightmapIndex >= LightmapSettings.lightmaps.Length)
					continue;
				
				string objName = obj.gameObject.name;
				if (objName == "default") {
					objName = obj.gameObject.name;
				}
				
				if (objName == "default") {
					objName = obj.gameObject.transform.parent.gameObject.name;
				}
				
				WPTerrainData wpTerrainData = new WPTerrainData();
				wpTerrainData.name = objName;
				wpTerrainData.width = terrainData.heightmapWidth;
				wpTerrainData.height = terrainData.heightmapHeight;
				wpTerrainData.lightmapIndex = terrain.lightmapIndex;
				//wpTerrainData.tilingOffset = rend.lightmapTilingOffset;
				wpTerrainData.position = obj.transform.position;
				
				float[,] tempHeightmap2d = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
				
				if (tempHeightmap2d == null) {
					Debug.LogWarning("No heightmap found for " + objName + ". Skipping.");
					continue;
				} else {
					if (tempHeightmap2d.Length <= 0) {
						Debug.LogWarning("No heightmap found for " + objName + ". Skipping.");
						continue;
					}
				}
				
				//Convert the heightmap
				float[] tempHeightmap1d = new float[wpTerrainData.width * wpTerrainData.height];
				
				for (int x = 0; x < wpTerrainData.width; x++) {
					for (int y = 0; y < wpTerrainData.height; y++) {
						tempHeightmap1d[y * wpTerrainData.width + x] = tempHeightmap2d[x, y] * terrainData.size.y;
					}
				}
				
				wpTerrainData.heightmap = tempHeightmap1d;
				
				wpTerrains.Add(wpTerrainData);
			}
			
			wpLightmapData.terrains = wpTerrains.ToArray();
			
			return wpLightmapData;
		}
		
		private static GameObject[] BuildListOfAffectedObjects(float wetnessLineY) {
			List<GameObject> affectedObjects = new List<GameObject>();
			
			foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType( typeof(GameObject) ) ) {
				//Skip non-static objects
				if ( !obj.gameObject.isStatic )
					continue;
				
				Renderer rend = obj.GetComponent<Renderer>();
				
				if (rend == null) {
					if (obj.GetComponent<Terrain>() == null)
						continue;
				} else {
					//Skip objects above the water line
					if ( rend.bounds.min.y > wetnessLineY )
						continue;
				}
				
				affectedObjects.Add(obj);
			}
			
			return affectedObjects.ToArray();
		}
		
		private static string[] PrepareLightmaps(GameObject[] affectedObjects) {
			//
			//Duplicate original lightmaps
			List<int> lightmapsToBackup = new List<int>();
			
			foreach (GameObject obj in affectedObjects) {
				int lightmapIndex = -1;
				
				if (obj.GetComponent<Renderer>() != null) {
					lightmapIndex = obj.GetComponent<Renderer>().lightmapIndex;
				} else {
					Terrain terrain = obj.GetComponent<Terrain>();
					if ( terrain != null ) {
						lightmapIndex = terrain.lightmapIndex;	
					}
				}
				
				if (lightmapIndex < 0)
					continue;
				
				//Make sure that we keep only lightmapped objects
				if (lightmapIndex < 0 || lightmapIndex >= LightmapSettings.lightmaps.Length)
					continue;
				
				if ( !lightmapsToBackup.Contains( lightmapIndex ) ) {
					if ( LightmapSettings.lightmaps[ lightmapIndex ] != null )
						lightmapsToBackup.Add( lightmapIndex );
				}
				
				//Find to what triangle does the point belong to
				//UpdateLightmap(obj);
			}
			
			string[] lightmapPaths = new string[ LightmapSettings.lightmaps.Length ];
			
			int lightmapsCount = 0;
			
			foreach (int lightmapIndex in lightmapsToBackup) {
				Texture2D lightmapFar = LightmapSettings.lightmaps[lightmapIndex].lightmapFar;
				
				if (lightmapFar == null)
					continue;
				
				string lightMapAssetPath = AssetDatabase.GetAssetPath(lightmapFar);
				
				//Debug.Log("lightMapAssetPath for " + lightmapIndex + " is " + lightMapAssetPath);
				
				if (lightMapAssetPath == null)
					continue;
				
				if (lightMapAssetPath.Length <= 0)
					continue;
				
				if ( WPHelper.HasSuffix(lightMapAssetPath, "__WP") ) {
					string originalLightmapPath = WPHelper.RemoveSuffixFromFilename(lightMapAssetPath, "__WP");
					//Debug.Log("WaterPlus lightmap already exists. Will be using the original one at path " + originalLightmapPath);
					
					if (!File.Exists( originalLightmapPath ) ) {
						Debug.LogError("Cannot find the original lightmap at path " + originalLightmapPath + ". Aborting");
						return null;
					}
					
					lightMapAssetPath = originalLightmapPath;
				}
				
				string waterPlusLightmapAssetPath = WPHelper.AddSuffixToFilename( lightMapAssetPath, "__WP" );
				
				AssetDatabase.DeleteAsset ( waterPlusLightmapAssetPath );
				AssetDatabase.CopyAsset( lightMapAssetPath, waterPlusLightmapAssetPath );
				
				AssetDatabase.ImportAsset( waterPlusLightmapAssetPath, ImportAssetOptions.ForceUpdate );
				AssetDatabase.Refresh();
				
				lightmapPaths[ lightmapIndex ] = WPHelper.AssetPathToFilePath(waterPlusLightmapAssetPath);
				
				lightmapsCount++;
				
				//LightmapSettings.lightmaps[ lightmapIndex ].lightmapFar = AssetDatabase.LoadAssetAtPath( waterPlusLightmapAssetPath,
				//																		typeof(Texture2D) ) as Texture2D;
			}
			
			//Load the new lightmaps
			LightmapData[] lightmapsData = new LightmapData[LightmapSettings.lightmaps.Length];
			
			for (int i = 0; i < lightmapPaths.Length; i++) {
				bool shouldUseOriginalLM = false;
				
				if (lightmapPaths[i] == null)
					shouldUseOriginalLM = true;
				else if (lightmapPaths[i].Length <= 0)
					shouldUseOriginalLM = true;
					
				if (!shouldUseOriginalLM) {
					//Debug.Log("!shouldUseOriginalLM");
					LightmapData lmData = new LightmapData();
					
					//AssetDatabase.Refresh();
					//AssetDatabase.ImportAsset( lightmapPaths[i], ImportAssetOptions.ForceSynchronousImport );
					//AssetDatabase.Refresh();
					
					lmData.lightmapFar = AssetDatabase.LoadAssetAtPath( WPHelper.FilePathToAssetPath( lightmapPaths[i] ), typeof(Texture2D) ) as Texture2D;
					
					if (lmData.lightmapFar == null) {
						Debug.LogWarning("lmData.lightmapFar == null for " + WPHelper.FilePathToAssetPath( lightmapPaths[i] ));
						lmData.lightmapFar = LightmapSettings.lightmaps[i].lightmapFar;
					}
					
					//if (LightmapSettings.lightmaps[i].lightmapNear != null)
					lmData.lightmapNear = LightmapSettings.lightmaps[i].lightmapNear;
					
					lightmapsData[i] = lmData;
				} else {
					lightmapsData[i] =	LightmapSettings.lightmaps[i];
				}
			}
			
			LightmapSettings.lightmaps = lightmapsData;
			
			if (lightmapsCount <= 0) {
				Debug.LogError("Nothing to bake - no lightmaps found.");
				return null;
			}
			
			return lightmapPaths;
		}
		
		public static void UpdateLightmaps(Transform _waterSurfaceTransform, string _lightmapWetnessHeightString, string _lightmapWetnessAmountString) {
			waterSurfaceTransform = _waterSurfaceTransform;
			lightmapWetnessHeightString = _lightmapWetnessHeightString;
			lightmapWetnessAmountString = _lightmapWetnessAmountString;
			
			if (waterSurfaceTransform == null) {
				Debug.LogError("Please assign a water surface first.");
				return;
			}
			
			float wetnessHeight;
			float wetnessAmount;
			
			
			if ( !float.TryParse(lightmapWetnessHeightString, out wetnessHeight) ) {
				Debug.LogError("Please enter a correct value into the 'Lightmap wetness height' field.");
				return;
			}
			
			if ( !float.TryParse(lightmapWetnessAmountString, out wetnessAmount) ) {
				Debug.LogError("Please enter a correct value into the 'Lightmap wetness amount' field.");
				return;
			}
			
			if (LightmapSettings.lightmaps.Length <= 0) {
				Debug.LogError("No lightmaps found. Please bake lightmaps first before updating them in Water+.");
				return;
			}
			
			//WriteBakeSettings();
			wetnessAmount = Mathf.Clamp(wetnessAmount, 0.0f, 0.7f);
			
			
			//Debug.Log("waterHeight: " + wetnessHeight + " maxWetness: " + wetnessAmount);
			
			float wetnessLineY = waterSurfaceTransform.GetComponent<Renderer>().bounds.max.y + wetnessHeight;
			
			GameObject[] affectedObjects = BuildListOfAffectedObjects(wetnessLineY);
			
			string[] lightmapPaths = PrepareLightmaps(affectedObjects);
			
			if (null == lightmapPaths)
				return;
			
			WPLightmapData wpLightmapData = PrepareLightmapData(lightmapPaths, affectedObjects, waterSurfaceTransform.GetComponent<Renderer>().bounds.max.y, wetnessHeight, wetnessAmount);
			
			BakeLightmaps(wpLightmapData);
			
			
		//Debug.Log("lightmapPaths.Length: " + lightmapPaths.Length);
//			ExportLightmapsDataToXML(xmlPath, lightmapPaths, affectedObjects.ToArray(), waterSurfaceTransform.renderer.bounds.max.y, wetnessHeight, wetnessAmount);
//			
//			Debug.Log("Sending lightmaps data to the external baker. xmlPath:" + xmlPath);
//			
			
			waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial.SetFloat("_refractionsWetness", 1.0f - wetnessAmount * .5f);
		}
		
	}
}