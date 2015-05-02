using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
//using System;
using System.Reflection;
#endif

//#define BAKE_REFRACTIONS

using System.Xml;
using System.Xml.Serialization;

namespace WaterPlusEditorInternal {
	#if UNITY_EDITOR
	public static class WaterPlusBaker {
		public static string waterSystemPath = "Assets/WaterPlus/";
		
		public static Transform waterSurfaceTransform = null;
	
		public static LayerMask terrainLayerMask = 1<<0;
		public static LayerMask refractionLayerMask = 1<<0;
		
		public static float foamDistance = 4f;
		
		private const float exportDepth = 25.0f;
		private static float maxDepth01 = 0.0f;
		
		public static float bakeStartTime = -100.0f;
		public static int bakeStage = -1;
		
		public static string waterMapResString = "1024";
		public static string refractionMapResString = "1024";
		
		public static float bakeProgress = -1.0f;
		
		//public static WaterPlus editorWindow;
		
		public static string bakingTask = "Bake progress";
		
		private static System.DateTime refrMapStartTime;
		
		public static bool shouldProjectRefractionMap = false;
		
		public static string refractionMapScaleString = "1.0";
		
		public static string lightmapWetnessHeightString = "1.0";
		public static string lightmapWetnessAmountString = ".2";
		
		//private static float coverPlaneY = -10000.0f;
		
		public static void EditorUpdate() {
			
			//Read config from file
			if (waterSurfaceTransform == null) {
				if ( !ReadBakeSettings() )
					return;
			} else {
				if (waterSurfaceTransform.gameObject == null) {
					if ( !ReadBakeSettings() )
					return;
				}
			}
			
			//Start baking in playmode
			if (bakeStartTime > -1.0f) {
				if ( !LocateSystem(false) ) {
					bakeStartTime = -1.0f;
					UpdateBakeStage(-1);
					return;
				}
				
				if (waterSurfaceTransform == null) {
					Debug.LogError("Please assign a water surface first.");
					bakeStartTime = -1.0f;
					UpdateBakeStage(-1);
					return;
				}
				
				if (Time.realtimeSinceStartup >= bakeStartTime) {
					if ( !LocateSystem(false) ) {
						bakeStartTime = -1.0f;
						UpdateBakeStage(-1);
						return;
					}
					
					//Disable water static lightmapping flags
					StaticEditorFlags waterFlags = GameObjectUtility.GetStaticEditorFlags( waterSurfaceTransform.gameObject );
					waterFlags = waterFlags & ~StaticEditorFlags.LightmapStatic;
					GameObjectUtility.SetStaticEditorFlags( waterSurfaceTransform.gameObject, waterFlags );
					
					WPHelper.CreateWaterSystemDirs();
					//return;
					bakeStartTime = -100.0f;
					WPHelper.waterSystemPath = waterSystemPath;
					
					//Bake watermaps right away, skip baking refraction map
#if !BAKE_REFRACTIONS
					UpdateBakeStage(13);
					return;
#else
					
					bakeProgress = .0f;
					bakingTask = "Baking refraction map";
					
					UpdateBakeStage(0);
					StartBakeInPlaymode();
#endif
					//UpdateBakeStage(-1);
					//bakeStartTime = -100.0f;
					return;
				}
			}
			
			//Debug.Log("bakeStage: " + bakeStage);
			
			switch (bakeStage) {
			case 0:	//Done baking in playmode
				ReadBakeSettings();
				
				//UpdateBakeStage(-1);
				//return;
#if BAKE_REFRACTIONS
				bakeProgress = 0.1f;
				CheckIfDoneBakingInPlayMode();
				
				bakeProgress = .2f;
				bakingTask = "Scaling refraction map";
				editorWindow.Repaint();
#endif
				break;
				
#if BAKE_REFRACTIONS
			case 1:
				refrMapStartTime = System.DateTime.Now;
				if ( PostProcessPlaymodeBake_ScaleAndCrop() )
					//UpdateBakeStage(-1);
					UpdateBakeStage(2);
				else {
					Debug.LogError("PostProcessPlaymodeBake_ScaleAndCrop failed");
					UpdateBakeStage(-1);
				}
				
				//UpdateBakeStage(-1);
				//return;
				
				bakeProgress = .3f;
				if (shouldProjectRefractionMap)
					bakingTask = "Projecting refraction map";
				else
					bakingTask = "Finalizing processing refraction maps";
				editorWindow.Repaint();
				//UpdateBakeStage(-1);
				break;
			
			case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11:
				UpdateBakeStage(12);
				/*if (shouldProjectRefractionMap) {
					Debug.Log("shouldProjectRefractionMap");
					PostProcessPlaymodeBake_Project(bakeStage - 1, 10);
					bakeProgress += 0.3f / 10.0f;
					editorWindow.Repaint();
				} else {
					UpdateBakeStage(12);
				}*/
				break;
			
			case 12:
				FinalizeProcessingRefractionMap();
				
				Debug.Log("Successfully baked refraction map at a resolution of " + refractionMapResolution + " in " + (System.DateTime.Now - refrMapStartTime).TotalSeconds + " seconds.");
				
				//UpdateBakeStage(15);
				//return;
				
				bakeProgress = .6f;
				bakingTask = "Baking water maps";
				editorWindow.Repaint();
				UpdateBakeStage(13);
				
				break;
#endif
				
			case 13:
				BuildWaterMapWrapper();
				
				bakeProgress = .9f;
				bakingTask = "Baking anisotropy map";
				//editorWindow.Repaint();
				UpdateBakeStage(14);
				break;
			
			case 14:
				//bakeProgress = 1.7f;
				//bakingTask = "Baking anisotropy map";
				BakeAnisotropyMap();
				UpdateBakeStage(15);
				break;
			
			case 15:
				Debug.Log("Successfully baked water maps.");
				bakeProgress = 1.0f;
				bakingTask = "Bake progress";
				//editorWindow.Repaint();
				UpdateBakeStage(-1);
				WPHelper.CleanupTempFiles();
				break;
			}
		}
		
		private static bool LocateSystem(bool _isSilent) {
			//if (null != systemRootAssetPath)
			//	return;
			
			if ( !EditorPrefs.HasKey( "WaterPlusSystemPath" ) )
				EditorPrefs.SetString( "WaterPlusSystemPath", "Assets/WaterPlus/" );
			
			waterSystemPath = EditorPrefs.GetString( "WaterPlusSystemPath" );
			
			//Debug.Log("systemRootAssetPath: " + systemRootAssetPath);
			
			if (!Directory.Exists( WPHelper.AssetPathToFilePath( waterSystemPath ) ) ) {
				if (!_isSilent)
					Debug.Log("Unable to locate the WaterPlus system at path: '" + waterSystemPath + "'. Relocating.");
				
				string[] wpDirs = Directory.GetDirectories( WPHelper.AssetPathToFilePath("Assets/"), "WaterPlus", SearchOption.AllDirectories );
				
				bool wasSystemFound = false;
				
				foreach (string dir in wpDirs) {
					if ( Directory.Exists( Path.Combine(dir, "Shaders") ) ) {
						waterSystemPath = WPHelper.FilePathToAssetPath( dir ) + "/";
						wasSystemFound = true;
						break;
					}
				}
				
				if (wasSystemFound) {
					Debug.Log("The system was relocated to: '" + waterSystemPath + "'");
					
					EditorPrefs.SetString( "WaterPlusSystemPath", waterSystemPath );
					
					Debug.Log("Editor WaterPlusSystemPath key: " + EditorPrefs.GetString( "WaterPlusSystemPath" ) );
				} else {
					if (!_isSilent)
						Debug.LogError("Unable to locate WaterPlus system. Have you renamed the root system directory?");	
					
					return false;
				}
			}
			
			WPHelper.waterSystemPath = waterSystemPath;
			
			return true;
		}
		
		public static void UpdateLightmaps() {
			WPLightmapping.UpdateLightmaps( waterSurfaceTransform, lightmapWetnessHeightString, lightmapWetnessAmountString );
		}
		
		#region Cubemaps
		
		public static void BakeCubemap() {
			if (waterSurfaceTransform == null) {
				Debug.LogError("Please assign a water surface first.");
				return;
			}
			
			DuplicateMaterial();
			
			//return;
			
			Skybox levelSkybox = null;
			Material skyboxMaterial;
		
			//Look for a camera with a skybox
			foreach (Camera cam in Camera.allCameras) {
				if ( cam.GetComponent<Skybox>()	!= null) {
					levelSkybox = cam.GetComponent<Skybox>();
					break;
				}
			}
			
			if (levelSkybox == null) {
				skyboxMaterial = RenderSettings.skybox;
			} else {
				skyboxMaterial = levelSkybox.material;	
			}
			
			if (skyboxMaterial == null) {
				Debug.LogError("Cannot bake the cubemap - no skybox found. Please attach a skybox to the scene.");
				return;
			}
			
			//GameObject tempCameraGO = new GameObject("CubemapCamera");
			//Skybox tempCameraSkybox = tempCameraGO.AddComponent<Skybox>();
			
			//tempCameraSkybox.material = levelSkybox.material;
			
			//Camera cubemapCamera = tempCameraGO.AddComponent<Camera>();
			
			//cubemapCamera.cullingMask = 0;	//Render nothing apart from the skybox
			
			Texture2D frontTexture = skyboxMaterial.GetTexture("_FrontTex") as Texture2D;
			Texture2D backTexture = skyboxMaterial.GetTexture("_BackTex") as Texture2D;
			Texture2D leftTexture = skyboxMaterial.GetTexture("_LeftTex") as Texture2D;
			Texture2D rightTexture = skyboxMaterial.GetTexture("_RightTex") as Texture2D;
			Texture2D upTexture = skyboxMaterial.GetTexture("_UpTex") as Texture2D;
			Texture2D downTexture = skyboxMaterial.GetTexture("_DownTex") as Texture2D;	//Optional
			
			if (frontTexture == null || backTexture == null || leftTexture == null || rightTexture == null || upTexture == null) {
				Debug.LogError("Cannot bake the cubemap - one or more of the skybox textures is missing. Skybox name: " + skyboxMaterial.name);	
				return;
			}
			
			Texture2D[] srcTextures = {frontTexture, backTexture, leftTexture, rightTexture, upTexture, downTexture};
			
			WPHelper.MakeTexturesReadable(srcTextures, true);
			WPHelper.CompressTextures(srcTextures, false);
			
			Cubemap tempCubemap = new Cubemap(frontTexture.width, TextureFormat.RGB24, true);
			
			/*frontTexture = Helper.ResizeImage(frontTexture, resolution, resolution);
			backTexture = Helper.ResizeImage(backTexture, resolution, resolution);
			leftTexture = Helper.ResizeImage(leftTexture, resolution, resolution);
			rightTexture = Helper.ResizeImage(rightTexture, resolution, resolution);
			upTexture = Helper.ResizeImage(upTexture, resolution, resolution);
			downTexture = Helper.ResizeImage(downTexture, resolution, resolution);*/
			
			frontTexture = WPHelper.FlipImage(frontTexture, false, true);
			backTexture = WPHelper.FlipImage(backTexture, false, true);
			leftTexture = WPHelper.FlipImage(leftTexture, false, true);
			rightTexture = WPHelper.FlipImage(rightTexture, false, true);
			upTexture = WPHelper.FlipImage(upTexture, false, true);
			downTexture = WPHelper.FlipImage(downTexture, false, true);
			
			//Gradients
			Color cubemapTint = Color.blue;
			if (waterSurfaceTransform.GetComponent<Renderer>()) {
				if (waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial) {
					//cubemapTint = waterSurfaceTransform.renderer.sharedMaterial.GetColor("_ShallowWaterTint");
					cubemapTint = Color.white;
					//cubemapTint = Color.blue;
				}
					
			}
			
			//Less = higher
			float gradientStart = .5f;
			float gradientEnd = .4f;
			
			frontTexture = ApplyLinearYGradientToTexture(frontTexture, cubemapTint, gradientStart, gradientEnd);
			backTexture = ApplyLinearYGradientToTexture(backTexture, cubemapTint, gradientStart, gradientEnd);
			leftTexture = ApplyLinearYGradientToTexture(leftTexture, cubemapTint, gradientStart, gradientEnd);
			rightTexture = ApplyLinearYGradientToTexture(rightTexture, cubemapTint, gradientStart, gradientEnd);
			
			if (downTexture)
				downTexture = ApplyLinearYGradientToTexture(downTexture, cubemapTint, 1.0f, 0.0f);
			
			//Helper.SaveTextureAsPngAtAssetPath(frontTexture, "Assets/test.png", false);
			
			Color[] frontPixels = frontTexture.GetPixels();
			Color[] backPixels = backTexture.GetPixels();
			Color[] leftPixels = leftTexture.GetPixels();
			Color[] rightPixels = rightTexture.GetPixels();
			Color[] upPixels = upTexture.GetPixels();
			Color[] downPixels = null;
			
			if (downTexture)
				downPixels = downTexture.GetPixels();
			
			tempCubemap.SetPixels(frontPixels, CubemapFace.PositiveZ);
			tempCubemap.SetPixels(backPixels, CubemapFace.NegativeZ);
			tempCubemap.SetPixels(rightPixels, CubemapFace.NegativeX);
			tempCubemap.SetPixels(leftPixels, CubemapFace.PositiveX);
			tempCubemap.SetPixels(upPixels, CubemapFace.PositiveY);
			
			if (downTexture)
				tempCubemap.SetPixels(downPixels, CubemapFace.NegativeY);
			
			tempCubemap.Apply();
			
			//cubemapCamera.RenderToCubemap( tempCubemap );
			
			System.IO.Directory.CreateDirectory(WPHelper.AssetPathToFilePath(waterSystemPath) + "Cubemaps/" );
			//string cubemapPath = waterSystemPath + "Cubemaps/" + System.IO.Path.GetFileNameWithoutExtension( EditorApplication.currentScene ) + ".cubemap";
			
			Material mat = waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;
			
			if (mat == null) {
				Debug.LogError("No material assigned to the water surface.");
				return;
			}
			
			string cubemapPath = waterSystemPath + "Cubemaps/" + mat.name + ".cubemap";

			
			if (AssetDatabase.LoadAssetAtPath( cubemapPath, typeof(Cubemap) ) != null) {
				Debug.LogWarning("Asset at path " + cubemapPath + " already exists. Deleting.");
				AssetDatabase.DeleteAsset(cubemapPath);
			}
			
			AssetDatabase.Refresh();
			
			AssetDatabase.CreateAsset(tempCubemap, cubemapPath);
			
			AssetDatabase.Refresh();
			
			//Object.DestroyImmediate(tempCameraGO);
			Debug.Log("Successfully saved the cubemap to " + cubemapPath);
			
			//Try to assign cubemap to water gameobject
			if (waterSurfaceTransform) {
				mat = waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;
				
				if (mat) {
					mat.SetTexture("_Cube", tempCubemap);	
				}	
			}
			
			WPHelper.CleanupTempFiles();
			
			WPHelper.MakeTexturesReadable(srcTextures, false);
			WPHelper.CompressTextures(srcTextures, true);
		}
		
		private static Texture2D ApplyLinearYGradientToTexture(Texture2D _texture, Color toColor, float fromY, float toY) {
			if (!_texture)
				return null;
			
			int fromYPixels = (int) (fromY * _texture.height);
			int toYPixels = (int) (toY * _texture.height);
		
			//Debug.Log("fromYPixels: " + fromYPixels + " toYPixels: " + toYPixels);
			
			Color[] srcPixels = _texture.GetPixels();
			Color[] resPixels = _texture.GetPixels();
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					/*if (y < fromY || y > toY)
						continue;*/
					
					//0.1 .. 0.8
					float alpha = (float)(y - fromYPixels) / (float)(toYPixels - fromYPixels);
					
					/*if (x == _texture.width/2)
						Debug.Log("alpha: " + alpha + " y: " + y);*/
					
					alpha = Mathf.Clamp01(alpha);
					
					
					
					//if (alpha < 0.0f || alpha > 1.0f)
					//	continue;
					
					resPixels[y * _texture.width + x] = Color.Lerp(toColor, srcPixels[y * _texture.width + x], alpha);
				}
			}
			
			Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, true);
			resultTexture.SetPixels( resPixels );
			resultTexture.Apply();
			
			return resultTexture;
		}
		
		#endregion
		
		#region Water maps
		private static void DuplicateMaterial() {
			if (waterSurfaceTransform == null)
				return;
			
			if (waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial.name == "WaterPlusMaterial") {
				//string surfaceName = waterSurfaceTransform.name;
				
				string targetMaterialName = waterSurfaceTransform.name;
				
				string targetPath;
				
				int i = 0;
				
				while (true) {
					targetPath = waterSystemPath + "Materials/" + targetMaterialName + ".mat";
					Debug.Log("Duplicating the material. Path: " + waterSystemPath + "Materials/" + targetMaterialName + ".mat");
					
					if ( !File.Exists( WPHelper.AssetPathToFilePath(targetPath)  ) )
						break;
					
					i++;
					
					targetMaterialName = waterSurfaceTransform.name + "_" + i;
				}
				
				string srcPath = waterSystemPath + "Materials/WaterPlusMaterial.mat";
				string dstPath = targetPath;
				
				bool copied = AssetDatabase.CopyAsset(srcPath, dstPath);
				if (!copied) {
					Debug.LogError("Failed to copy the material from " + srcPath + " to " + dstPath);
					return;
				}
				
				AssetDatabase.ImportAsset( dstPath );
				AssetDatabase.Refresh();
				
				Material mat = AssetDatabase.LoadAssetAtPath(dstPath, typeof( Material )) as Material;
				
				if (mat == null) {
					Debug.LogError("Failed to load material at path " + dstPath);
					return;
				}
				
				waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial = mat;
			}
		}
		
		private static bool CheckUVsConsistency(Transform transform) {
			MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter>();
			
			if (!meshFilter) {
				Debug.LogError("The water surface has no MeshFilter. Aborting.");
				return false;
			}
				
			Mesh mesh = meshFilter.sharedMesh;
			
			if (!mesh) {
				Debug.LogError("The water surface has no Mesh. Aborting.");
				return false;
			}
			
			Vector2[] uvs = mesh.uv;
			
			bool foundUVs = true;
			
			if (uvs == null) {
				foundUVs = false;	
			} else {
				if (uvs.Length <= 0)
					foundUVs = false;
			}
			
			if (!foundUVs) {
				Debug.LogError("The water surface has no UVs. Aborting.");
				return false;
			}
			
			foreach (Vector2 uv in uvs) {
				if (uv.x < 0.0f || uv.x > 1.0f	|| uv.y < 0.0f || uv.y > 1.0f) {
					Debug.LogError("The UVs of the water surface are not in 0..1 space. Unable to continue. Please fix the UVs and try again.");
					return false;
				}
			}
			
			return true;
		}
		
		private static void BuildWaterMapWrapper() {
			if (waterSurfaceTransform == null) {
				UpdateBakeStage(-1);
				Debug.LogError("Please assign a water surface first.");
				return;
			}
			
			if ( !LocateSystem(false) ) {
				UpdateBakeStage(-1);
				return;
			}
			
			if ( !CheckUVsConsistency(waterSurfaceTransform) ) {
				UpdateBakeStage(-1);
				return;
			}
			
			DuplicateMaterial();
			
			WriteBakeSettings();
			
			//System.DateTime startTime = System.DateTime.Now;
			
			int waterMapResolution;
			if ( !int.TryParse(waterMapResString, out waterMapResolution) ) {
				UpdateBakeStage(-1);
				Debug.LogError("Please enter a correct value into water map resolution");
				return;
			}
			
			waterMapResolution = Mathf.Clamp( WPHelper.GetNearestPOT(waterMapResolution), 64, 4096);
			
			//Debug.Log("Baking water maps at the resolution of " + waterMapResolution);
			//return;
			
			//return;
			
			Texture2D waterMapTexture = BuildDepthMapAndMask(waterMapResolution, waterMapResolution, waterSurfaceTransform.gameObject);
			System.GC.Collect();
			
			WPGrayscaleImage depthMap = new WPGrayscaleImage(waterMapTexture, WPColorChannels.r);
			
			waterMapTexture = WPGrayscaleImage.MakeTexture2D(depthMap, depthMap, depthMap, null);
			
			WPGrayscaleImage terrainMask = new WPGrayscaleImage(waterMapTexture, WPColorChannels.a);
			
			
			WPGrayscaleImage foamMap = CalculateFoamMap( depthMap, foamDistance );	//Needs untouched red channel (depth)
			
			WPGrayscaleImage transparencyMap = CalculateTransparencyMap( depthMap, terrainMask ); //Needs untouched red channel (depth)
			
			waterMapTexture = WPGrayscaleImage.MakeTexture2D(depthMap, foamMap, transparencyMap, null);
			System.GC.Collect();
			
			WPGrayscaleImage refractionStrengthMap = CalculateRefractionStrengthMap(depthMap, terrainMask); //Needs untouched red channel (depth)
			System.GC.Collect();
			
			WPGrayscaleImage noiseGradient = ApplyGradientToDepthmap( depthMap, terrainMask, true );
			depthMap = ApplyGradientToDepthmap( depthMap, terrainMask, false );
			System.GC.Collect();
			
			//waterMapTexture = new Texture2D(waterMapResolution, waterMapResolution);
			
			int downsizeToRes = waterMapResolution / 2;
			
			depthMap = WPHelper.ResizeImage(depthMap, downsizeToRes, downsizeToRes, WPFilteringMethod.bilinear);
			//GrayscaleImage dsTerrainMask = Helper.ResizeImage(terrainMask, downsizeToRes, downsizeToRes);
			WPGrayscaleImage dsNoiseGradient = WPHelper.ResizeImage(noiseGradient, downsizeToRes, downsizeToRes, WPFilteringMethod.bilinear);
			depthMap = ApplyNoiseToDepthmap( depthMap, dsNoiseGradient, true );
			depthMap = WPHelper.Blur(depthMap, Mathf.Clamp(waterMapResolution / 128, 3, 16), WPBlurType.gaussian);
			depthMap = WPHelper.ResizeImage(depthMap, waterMapResolution, waterMapResolution, WPFilteringMethod.bilinear);
			depthMap = WPHelper.Blur(depthMap, Mathf.Clamp(waterMapResolution / 512, 3, 16), WPBlurType.gaussian);
			
			depthMap = WPHelper.NormalizeImage(depthMap);	
			
			depthMap = ApplyNoiseToDepthmap( depthMap, noiseGradient, false );
			
			waterMapTexture = WPGrayscaleImage.MakeTexture2D(depthMap, foamMap, transparencyMap, refractionStrengthMap);
			
			if (waterMapTexture == null) {
				Debug.LogError("waterMapTexture == null");
				UpdateBakeStage(-1);
				return;
			}
			
			//waterMapTexture = DiscardAlpha(waterMapTexture);
			
			Material mat = waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;
			
			if (mat == null) {
				Debug.LogError("No material assigned to the water surface.");
				return;
			}
			
			System.IO.Directory.CreateDirectory( WPHelper.AssetPathToFilePath(waterSystemPath) + "WaterMaps/" );
			string waterMapPath = waterSystemPath + "WaterMaps/" + mat.name + "_watermap.png";
			
			WPHelper.SaveTextureAsPng(waterMapTexture,  WPHelper.AssetPathToFilePath(waterMapPath));
		
			//Set watermap format to TrueColor
			TextureImporter tImporter = AssetImporter.GetAtPath(waterMapPath ) as TextureImporter;
	        if( tImporter != null ) {
				tImporter.textureType = TextureImporterType.Advanced;
				tImporter.linearTexture = true;
	            tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				tImporter.maxTextureSize = 4096;
				tImporter.wrapMode = TextureWrapMode.Clamp;
				tImporter.SetPlatformTextureSettings("iPhone", 512, TextureImporterFormat.AutomaticCompressed);
				tImporter.SetPlatformTextureSettings("Android", 512, TextureImporterFormat.AutomaticCompressed);
	            AssetDatabase.ImportAsset(waterMapPath);                
	        }
			
			//Debug.Log("Successfully baked water maps in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds and saved it to " + "Assets/" + waterMapPath);
			
			//Try to assign depthmap to water gameobject
			if (waterSurfaceTransform) {
				// mat = waterSurfaceTransform.renderer.sharedMaterial;
				
				if (mat) {
					mat.SetTexture("_WaterMap", AssetDatabase.LoadAssetAtPath( waterMapPath, typeof(Texture2D) ) as Texture2D );	
				}
			}
			
			//UpdateBakeStage(13);
		}
		
		private static Texture2D BuildDepthMapAndMask(int _width, int _height, GameObject _waterGameObject) {
			AttachMeshColliderToWater();
			
			//System.DateTime startTime = System.DateTime.Now;
			
			/*Transform coverPlaneTransform = null;
			
			foreach (Transform sibling in waterSurfaceTransform.parent) {
				if (sibling.name == "CoverPlane") {
					coverPlaneTransform = sibling;
					break;
				}
			}
			
			if (coverPlaneTransform == null) {
				Debug.LogWarning("No CoverPlane found. Please attach a cover plane as per instructions for best results.");
			} else {
				coverPlaneY = coverPlaneTransform.position.y;
			}*/
			
			Texture2D resultTexture = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
			//return resultTexture;
			
			Color[] resultsPixels = new Color[_width * _height];
			
			//
			//Get actual heightmap data
			
			int oldLayer = _waterGameObject.layer;
			//Set layer to water
			_waterGameObject.layer = 4;
			
			Bounds objBounds = _waterGameObject.GetComponent<Renderer>().bounds;
			
			float xIncrement = .9f * objBounds.size.x / (float) _width;
			float zIncrement = .9f * objBounds.size.z / (float) _height;
		
			float yOrigin = objBounds.max.y + 100.0f;
			
			//float maxHeight = 0.0f;
			
			float[,] heightsArray = new float[_width, _height];
			
			//Init the array
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					heightsArray[x, y] = -10.0f;	//Nothing
				}
			}
			
			for (float x = objBounds.min.x; x <= objBounds.max.x; x += xIncrement) {
				for (float z = objBounds.min.z; z <= objBounds.max.z; z += zIncrement) {
					Vector3 origin = new Vector3(x, yOrigin, z);
					RaycastHit waterHitInfo;
					
					if (Physics.Raycast(origin, Vector3.down, out waterHitInfo, 1000.0f, 1 << _waterGameObject.layer) ) {
						//Raycast all but water
						//RaycastHit[] raycasts = Physics.RaycastAll(origin, Vector3.down, 1000.0f, ~(1 << _waterGameObject.layer) );
						RaycastHit[] raycasts = Physics.RaycastAll(origin, Vector3.down, 1000.0f, terrainLayerMask);
						
						float heightValue = -1.0f;
						
						if (raycasts.Length > 0) {
							//Debug.Log("Hit something else.");
							//Find heighest point
							float heighestPoint = -100000.0f;
							foreach (RaycastHit hit in raycasts) {
								if (hit.point.y > heighestPoint && hit.point.y <= waterHitInfo.point.y)
									heighestPoint = hit.point.y;
							}
							
							//Did we hit something?
							if (heighestPoint > -100000.0f) {
								heightValue = waterHitInfo.point.y - heighestPoint;
								if (heightValue > exportDepth) {
									heightValue = exportDepth;
								} else {
									//Nothing should go below the cover plane
									/*if (heighestPoint < coverPlaneY)
										heightValue = coverPlaneY;*/
								}
								
								/*if (heightValue > maxHeight) {
									maxHeight = heightValue;
								}*/
							} else  //Hit nothing (terrain is above us)
								heightValue = -1.0f;
						} else {	//Hit nothing
							heightValue = -10.0f;
						}
						
						if (waterHitInfo.textureCoord.x >= 1.0f || waterHitInfo.textureCoord.y >= 1.0f)
							continue;
						
						heightsArray[(int) (waterHitInfo.textureCoord.x * _width), (int) (waterHitInfo.textureCoord.y * _height)] = heightValue;
					}
				}
			}
			
			//Set pixel values
			for (int x = 0; x < _width; x++) {
				for (int y = 0; y < _height; y++) {
					float alphaValue = 0.0f;
					float depthChannelValue = 0.0f;
					
					//Hit something
					if (heightsArray[x, y] > -1.0f) {
						depthChannelValue = (heightsArray[x, y] / exportDepth );
						
						maxDepth01 = Mathf.Max(maxDepth01, depthChannelValue);
						
						if (depthChannelValue > 1.0f) {
							depthChannelValue = 1.0f;
						}
						
						alphaValue = 1.0f;
					} else if ( heightsArray[x, y] == -1.0f ) {	//Hit from above
						depthChannelValue = 0.0f;
						alphaValue = 1.0f;
					} else {	//Nothing was hit
						depthChannelValue = 1.0f;
						alphaValue = 0.0f;
					}
					
					//depthChannelValue = Mathf.Max(depthChannelValue, 0.05f);
					
					Color resultColor = new Color(depthChannelValue, 0.0f, 0.0f, alphaValue);
						
					
					resultsPixels[y * _width + x] = resultColor;
				}
				
			}
			
			//Debug.Log("maxDepth: " + maxDepth01);
			
			resultTexture.SetPixels( resultsPixels );
			resultTexture.Apply();
			
			_waterGameObject.layer = oldLayer;
			
			RestoreOriginalWaterCollider();
			
			//Debug.Log("baked the depth map in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
			
			return resultTexture;
		}
		
		private static WPGrayscaleImage CalculateRefractionStrengthMap(WPGrayscaleImage _depthMap, WPGrayscaleImage _mask) {
			byte[] srcPixels = _depthMap.GetPixels();
			byte[] resPixels = _mask.GetPixels();
			byte[] maskPixels = _mask.GetPixels();
			
			//Debug.Log("maxDepth: " + maxDepth01);
			
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				//Outside of the mask no refraction
				if (maskPixels[i] <= 0)
					resPixels[i] = 255;
				else {
					float refrStrength = ( (float)srcPixels[i] / 255.0f) / (maxDepth01 * 0.5f);
					//resPixels[i] = Mathf.Pow(refrStrength, 2.0f);
					resPixels[i] = (byte) (Mathf.Clamp01( refrStrength ) * 255.0f);
					//resPixels[i] = srcPixels[i];
				}
			}
			
			WPGrayscaleImage resultTexture = new WPGrayscaleImage(_depthMap.width, _depthMap.height, resPixels);
			resultTexture = WPHelper.Blur(resultTexture, 3, WPBlurType.box);
			
			return resultTexture;
		}
		
		private static WPGrayscaleImage BuildShoreBorder(WPGrayscaleImage _texture, bool borderAtZeroPixels) {
			byte[] sourcePixels = _texture.GetPixels();
			byte[] resultPixels = WPGrayscaleImage.ValuePixels(_texture.width, _texture.height, 0);
			
			int borderPixels = 0;
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					//Skip empty pixels
					if (borderAtZeroPixels) {
						if ( sourcePixels[y * _texture.width + x] > 0 )
								continue;
					} else {
						if ( sourcePixels[y * _texture.width + x] < 255 )
								continue;
					}
					
					bool isBorderPixel = false;
					for (int xx = x - 1; xx <= x + 1 && !isBorderPixel; xx++) {
						for (int yy = y - 1; yy <= y + 1 && !isBorderPixel; yy++) {
							//Debug.Log("xx: " + xx + " yy: " + yy);
							if (xx < 0 || yy < 0 || xx >= _texture.width || yy >= _texture.height)
								continue;
							
							//Skip self
							if (xx == x || yy == y)
								continue;
							
				
							
							//Is this pixel empty? If so, we're at border
							bool isPixelEmpty = false;
							
							if (borderAtZeroPixels) {
								if ( sourcePixels[yy * _texture.width + xx] > 0 )
											isPixelEmpty = true;
							} else {
								if ( sourcePixels[yy * _texture.width + xx] < 255 )
											isPixelEmpty = true;
							}
							
							if ( isPixelEmpty ) {
								resultPixels[y * _texture.width + x] = 255;
								
								isBorderPixel = true;
								borderPixels++;
								break;
							}
						}
						
						//if (isBorderPixel)
						//	break;
					}
				}
			}
			
			//Debug.Log("borderPixels: " + borderPixels);
			
			WPGrayscaleImage resultTexture = new WPGrayscaleImage(_texture.width, _texture.height, resultPixels);
			
			return resultTexture;
		}
		
		private static WPGrayscaleImage CalculateFoamMap(WPGrayscaleImage _depthMap, float _foamDistance) {
			
			int foamDistanceInPixels = Mathf.RoundToInt( _foamDistance * (_depthMap.width + _depthMap.height) /
				(waterSurfaceTransform.GetComponent<Renderer>().bounds.size.x + waterSurfaceTransform.GetComponent<Renderer>().bounds.size.z) );
			
			//Default value
			if (_foamDistance <= 0.0f)
				foamDistanceInPixels = 1;
			
			//Prevent from hanging by having too much foam to calculate
			foamDistanceInPixels = Mathf.Min(10, foamDistanceInPixels);
				
			//Debug.Log("foamDistanceInPixels: " + foamDistanceInPixels);
			
			//Build shore border
			WPGrayscaleImage shoreBorderTexture = BuildShoreBorder(_depthMap, true);
			
			//return shoreBorderTexture;
			
			//return shoreBorderTexture;
			
			//Create shore mask
			//GrayscaleImage shoreMask = Helper.Blur(shoreBorderTexture, (int)foamDistanceInPixels, BlurType.expand);
			
			//Fatten the border
			//shoreBorderTexture = Helper.Blur(shoreBorderTexture, 1, BlurType.box, ColorChannels.g);
			
			//return shoreMask;
			
			//GrayscaleImage resultTexture = Helper.Gradient(shoreBorderTexture, shoreMask, foamDistanceInPixels, GradientType.linear);
			WPGrayscaleImage resultTexture = WPHelper.Gradient(shoreBorderTexture, foamDistanceInPixels, 1.0f, 0.0f, WPGradientType.linear);
			
			//Reverse pixels color
			byte[] resultPixels = resultTexture.GetPixels();
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				resultPixels[i] = (byte) (255 - resultPixels[i]);
			}
			
			resultTexture.SetPixels( resultPixels );
			
			resultTexture = WPHelper.Blur(resultTexture, Mathf.Clamp(_depthMap.width / 2048, 1, 2), WPBlurType.gaussian);
			//resultTexture = Helper.Blur(resultTexture, 1, BlurType.gaussian);
			resultTexture = WPHelper.NormalizeImage(resultTexture);
			
			//Debug.Log("Successfully baked the foam map in " + (System.DateTime.Now - foamBakeStartTime).TotalSeconds + " seconds.");
			
			return resultTexture;
		}
		
		private static WPGrayscaleImage CalculateTransparencyMap(WPGrayscaleImage _depthMap, WPGrayscaleImage _terrainMask) {
			//System.DateTime bakeStartTime = System.DateTime.Now;
			
			//Build shore border
			WPGrayscaleImage shoreBorderTexture = BuildShoreBorder(_depthMap, true);
			
			//int transparencyDistance = 10;
			int transparencyDistance = _depthMap.width / 256;
			
			transparencyDistance = Mathf.Clamp(transparencyDistance, 2, 10);
			
			//Debug.Log("transparencyDistance: " + transparencyDistance);
			
			//Create shore mask
			//GrayscaleImage shoreMask = Helper.Blur(shoreBorderTexture, transparencyDistance, BlurType.expand);
			
			//GrayscaleImage shoreBorderExtended = Helper.Blur(shoreBorderTexture, 1, BlurType.expand);
			
			//Fatten the border
			//shoreBorderTexture = Helper.Blur(shoreBorderTexture, 1, BlurType.box, ColorChannels.g);
			
			//GrayscaleImage resultTexture = Helper.Gradient(shoreBorderTexture, shoreMask, transparencyDistance, GradientType.sqrOfOneMinusG);
			WPGrayscaleImage resultTexture = WPHelper.Gradient(shoreBorderTexture, transparencyDistance, 1.0f, 0.0f, WPGradientType.sqrOfOneMinusG);
			//GrayscaleImage resultTexture = Helper.Gradient(shoreBorderTexture, transparencyDistance, 1.0f, 0.0f, GradientType.linear);
			//GrayscaleImage resultTexture = Helper.ExpandBorder(shoreBorderTexture, 20);
			
			
			//Reverse transparency pixels
			/*Debug.Log("Reverse transparency pixels");
			byte[] resultPixels = resultTexture.GetPixels();
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				resultPixels[i] = (byte) (255 - resultPixels[i]);
			}
			
			resultTexture.SetPixels( resultPixels );
			*/
			
			resultTexture = WPHelper.Blur(resultTexture, Mathf.Clamp(_depthMap.width / 2048, 1, 2), WPBlurType.gaussian);
			resultTexture = WPHelper.NormalizeImage(resultTexture);
			
			
			//
			//Recover border after the blur
			byte[] resultPixels = resultTexture.GetPixels();
			
//			GrayscaleImage border1 = Helper.Blur(shoreBorderTexture, 1, BlurType.expand);
//			float[] border1Pixels = border1.GetPixels();
//			
//			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
//				if (border1Pixels[i] > 0.0f)
//					resultPixels[i] = Mathf.Min(resultPixels[i], 0.2f);//1.0f - borderPixels[i].b;
//			}
			
			//float[] borderExtendedPixels = shoreBorderExtended.GetPixels();
			byte[] borderPixels = shoreBorderTexture.GetPixels();
			
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				if (borderPixels[i] > 0)
					resultPixels[i] = 0;//1.0f - borderPixels[i].b;
			}
			
			//Remove transparency on the inside (where the terrain is)
			byte[] depthPixels = _depthMap.GetPixels();
			byte[] terrainMaskPixels = _terrainMask.GetPixels();
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				if (depthPixels[i] <= 0 && terrainMaskPixels[i] > 0)
					resultPixels[i] = 0;
			}
			
			resultTexture.SetPixels( resultPixels );

			//Debug.Log("Successfully baked the transparency map in " + (System.DateTime.Now - bakeStartTime).TotalSeconds + " seconds.");
			
			return resultTexture;
		}
		
		private static WPGrayscaleImage ApplyGradientToDepthmap(WPGrayscaleImage _depthMap, WPGrayscaleImage _terrainMask, bool isFullGradient) {
			//Debug.Log("ApplyGradientToDepthmap");
			//System.DateTime bakeStartTime = System.DateTime.Now;
			
			//return _depthMap;
			
			int downsampleToResolution = Mathf.Clamp(WPHelper.GetNearestPOT( _depthMap.width / 10 ), 32, 256);
			
			//downsampleToResolution = Helper.GetNearestPOT( _texture.width / 2 );
			
			//GrayscaleImage downsampledDepthmap = Helper.ResizeImage(_depthMap, downsampleToResolution, downsampleToResolution);
			WPGrayscaleImage downsampledTerrainMask = WPHelper.ResizeImage(_terrainMask, downsampleToResolution, downsampleToResolution, WPFilteringMethod.bilinear);
			
			//return downsampledTerrainMask;
			
			if (downsampledTerrainMask == null) {
				Debug.LogError("failed to resize image");
				return _depthMap;
			}
			
			//Build border for the gradient
			WPGrayscaleImage dsBorder = BuildShoreBorder( downsampledTerrainMask, true );
			//dsBorder = DiscardAlpha(dsBorder);
			
			//return dsBorder;

			float gradientFromValue;
			
			if (isFullGradient)
				gradientFromValue = 0.0f;
			else
				gradientFromValue = maxDepth01 * .9f;
			
			//coverPlaneY = Mathf.Clamp01(coverPlaneY);
			
			//Debug.Log("gradientFromValue: " + gradientFromValue + " maxDepth: " + maxDepth01);
			
			//Apply gradient
			WPGrayscaleImage gradientTexture = WPHelper.Gradient(dsBorder, (int) ( downsampleToResolution / (2.0f * 1.42f) ), 1.0f, gradientFromValue,
											WPGradientType.linear);
			//gradientTexture = DiscardAlpha( gradientTexture );
			
			//return gradientTexture;
			
			//Resize gradient back to the original size
			gradientTexture = WPHelper.ResizeImage(gradientTexture, _depthMap.width, _depthMap.height, WPFilteringMethod.bilinear);
			//return gradientTexture;
			
			//Texture2D usBorderBlurred = Helper.ResizeImage(dsBorderBlurred, _texture.width, _texture.height);
			//gradientTexture = Helper.Blur(gradientTexture, 4, BlurType.gaussian, ColorChannels.g);
			
			//return usBorderBlurred;
			
			if (gradientTexture == null) {
				Debug.LogError("failed to resize image");
				return _depthMap;
			}
			
			//Slow!!!!
			/*GrayscaleImage shoreBorder = BuildShoreBorder( _terrainMask, true );
			gradientTexture = Helper.Gradient(shoreBorder, (int) ( _terrainMask.width / (2.0f * 1.42f) ), 1.0f, gradientFromValue,
											GradientType.linear);*/
			
			//Recover the actual depthmap
			byte[] gradientPixels = gradientTexture.GetPixels();
			byte[] maskPixels = _terrainMask.GetPixels();
			byte[] resultPixels = _depthMap.GetPixels();
			//Color[] usBorderBlurredPixels = usBorderBlurred.GetPixels();
			
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				//resultPixels[i] = (byte) ( ( (float)gradientPixels[i] / 255.0f ) * (1.0f - (float)maskPixels[i] / 255.0f) + ( (float)resultPixels[i] / 255.0f) * ( (float)maskPixels[i] / 255.0f) );
				
				resultPixels[i] = (byte) ( Mathf.Lerp( (float)gradientPixels[i] / 255.0f,
														(float)resultPixels[i] / 255.0f,
														(float)maskPixels[i] / 255.0f ) * 255.0f );
				
				//resultPixels[i].g = gradientPixels[i].g;// * (1.0f - usBorderBlurredPixels[i].g) +
				//					+ resultPixels[i].r * usBorderBlurredPixels[i].g;
				//resultPixels[i].r = resultPixels[i].r;
				//Pixels that are on the mask - blend; outside - choose the correct one
				//if (
			}
			
			/*//Recover the border
			GrayscaleImage shoreBorder = BuildShoreBorder( _terrainMask, true );
			float[] shoreBorderPixels = shoreBorder.GetPixels();
			for (int i = 0; i < _depthMap.width * _depthMap.height; i++) {
				if ( shoreBorderPixels[i] > 0.0f )
					resultPixels[i] = 1.0f;
				//resultPixels[i]
			}*/
			
			//Debug.Log("Successfully applied gradient to depthmap in " + (System.DateTime.Now - bakeStartTime).TotalSeconds + " seconds.");
			
			return new WPGrayscaleImage(_depthMap.width, _depthMap.height, resultPixels);
		}
		
		/*private static Texture2D ApplyGradientOldToTexture(Texture2D _texture) {
			Color gradientToColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			
			Color[] sourcePixels = _texture.GetPixels();
			Color[] resultPixels = sourcePixels;
			
			//Init results pixels with clear color
			for (int i = 0; i < _texture.width * _texture.height; i++) {
				resultPixels[i].r = 0.0f;
			}
			
			//
			//First apply radial gradient to empty pixels
			//
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					
					//Is this pixel filled?
					if (sourcePixels[y * _texture.width + x].a > 0.001f || resultPixels[y * _texture.width + x].a > 0.001f)
						continue;
					
					//Look for nearest non-empty pixel towards center
					Vector2 searchDirection = new Vector2( (float)_texture.width / 2.0f, (float)_texture.height / 2.0f) - new Vector2( (float)x, (float)y );
					searchDirection.Normalize();
					
					Vector2 initialPixel = new Vector2(x, y);
					
					Vector2 lookupPixel = initialPixel;
					
					Color nearestFilledPixelColor = Color.clear;
					
					float distanceToFilledPixel = 1.0f; Vector2 filledPixel = Vector2.zero;
					
					for (float i = 0; ; i += 1.0f) {
						lookupPixel = initialPixel + searchDirection * i;
						if ( (int)lookupPixel.x < 0 || (int)lookupPixel.x >= _texture.width || (int)lookupPixel.y < 0 || (int)lookupPixel.y >= _texture.height)
							break;
						
						Color lookupPixelColor = sourcePixels[ (int)lookupPixel.y * (int)_texture.width + (int)lookupPixel.x ];
						
						if (lookupPixelColor.a > 0.0f) {
							if (lookupPixelColor.r < 0.1f) {	//Dark border = no terrain underneath the water
								nearestFilledPixelColor = new Color(0.5f, 0.0f, 0.0f, 1.0f);
							} else {
								nearestFilledPixelColor = lookupPixelColor;
							}
							filledPixel = lookupPixel;
							distanceToFilledPixel = (filledPixel - initialPixel).magnitude;
							break;
						}
					}
					
					//No filled pixels found
					if (nearestFilledPixelColor.a == 0.0f) {
						nearestFilledPixelColor = new Color(0.5f, 0.0f, 0.0f, 1.0f);
					}
					
					//Fill
					lookupPixel = initialPixel;
					for (float i = 0; ; i += 1.0f) {
						lookupPixel = initialPixel + searchDirection * i;
						
						if ( (int)lookupPixel.x < 0 || (int)lookupPixel.x >= _texture.width || (int)lookupPixel.y < 0 || (int)lookupPixel.y >= _texture.height)
							break;
						
						Color lookupPixelColor = sourcePixels[(int) lookupPixel.y * _texture.width + (int) lookupPixel.x];
						
						if (lookupPixelColor.a > 0.0f) {
							break;
						}
						
						float gradientAmount = (lookupPixel - filledPixel).magnitude / distanceToFilledPixel;
						
						//gradientAmount = Mathf.Sqrt( gradientAmount );
						//gradientAmount = 1.0f - gradientAmount;
						//gradientAmount = 1.0f - gradientAmount * gradientAmount;
						//-0.38+0.4/(sqrt(1.08-x))
						gradientAmount = -0.38f + 0.4f / ( Mathf.Sqrt(1.08f - gradientAmount) );
						//gradientAmount = gradientAmount * gradientAmount;
						//Color newColor = Color.red;
						//newColor.r = Mathf.Lerp(nearestFilledPixelColor.r, gradientToColor.r, gradientAmount);
						
						Color gradientColor = Color.Lerp(nearestFilledPixelColor, gradientToColor, gradientAmount);
						
						resultPixels[(int) lookupPixel.y * _texture.width + (int) lookupPixel.x].r = gradientColor.r;
					}
				}
				
			}
			
			Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
			resultTexture.SetPixels( resultPixels );
			resultTexture.Apply();
			//resultTexture = Helper.BlurTexture(resultTexture, 10, 0.0f, Helper.TextureBlurMode.BlurAll);
			resultTexture = Helper.Blur(resultTexture, 10, BlurType.gaussian, ColorChannels.r);
			
			resultPixels = resultTexture.GetPixels();
			//Combine with initial texture
			for (int i = 0; i < _texture.width * _texture.height; i++) {
				resultPixels[i].a = 1.0f;
				//resultPixels[i] = sourcePixels[i];
				resultPixels[i].r = sourcePixels[i].r * (1.0f - sourcePixels[i].r) + resultPixels[i].r * sourcePixels[i].r;
				//resultPixels[i] = sourcePixels[i] * sourcePixels[i].a  + resultPixels[i] * resultPixels[i].a;
				//resultPixels[i] = resultPixels[i] * sourcePixels[i] * (resultPixels[i].a + sourcePixels[i].a);
				//resultPixels[i].r += 0.05f;
			}
			
			resultTexture.SetPixels( resultPixels );
			resultTexture.Apply();
			
			return resultTexture;
		}*/
		
		private static float GetNoiseForPixel(int x, int y, int width, int height, int seed) {
			float noise0 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
															( (float)y ) / ( (float)height ),
															2.5f, 2.5f, (float)seed );
			
			float noise1 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
															( (float)y ) / ( (float)height ),
															5.0f, 5.0f, (float)seed );
					
			float noise2 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
													( (float)y ) / ( (float)height ),
													10.0f, 10.0f, (float)seed );
			
			float noise3 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
													( (float)y ) / ( (float)height ),
													20.0f, 20.0f, (float)seed );
			
			float noise4 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
													( (float)y ) / ( (float)height ),
													30.0f, 30.0f, (float)seed );
			
			/*float noise5 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)width ),
													( (float)y ) / ( (float)height ),
													100.0f, 100.0f, (float)seed );*/
			//float noiseAmount = 0.0f;
			float noise = 0.0f;
			
			noise += noise0 * 1.0f;// noiseAmount += 1.0f;
			noise += noise1 * .5f;// noiseAmount += .5f;
			noise += noise2 * .25f;//	noiseAmount += .25f;
			noise += noise3 * .12f;//	noiseAmount += .12f;
			noise += noise4 * .06f;//	noiseAmount += .06f;
			
			noise /= 2.0f;	//2 is the sum of all
			
			noise = Mathf.Pow(noise, 3.0f) * 2.5f;
			
			//noise += .5f;
			
			return noise;
		}
		
		private static WPGrayscaleImage ApplyNoiseToDepthmap(WPGrayscaleImage _texture, WPGrayscaleImage _noiseGradient, bool addLargeNoise) {
			byte[] sourcePixels = _texture.GetPixels();
			//byte[] maskPixels = _terrainMask.GetPixels();
			byte[] noiseGradientPixels = _noiseGradient.GetPixels();
			byte[] resultPixels = WPGrayscaleImage.ValuePixels(_texture.width, _texture.height, 0);
			//Color[] noiseGradientPixels = _noiseGradientTexture.GetPixels();
			
			int seed = System.DateTime.Now.Millisecond;
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					float sourceColor = ( (float)sourcePixels[y * _texture.width + x]) / 255.0f;
					
					/*float noise1 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
															( (float)y ) / ( (float)_texture.height ),
															2.5f, 2.5f, (float)seed );*/
					
					/*float noise0 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
															( (float)y ) / ( (float)_texture.height ),
															2.5f, 2.5f, (float)seed );*/
					
					
					
					//float noise = (noise0 + noise1 * .5f + noise2 * .25f + noise3 * .12f + noise4 * .06f) / (1.93f);
					
					/*//Convert to [0,5..1,5]
					noise += 0.5f;
					
					if (sourceColor.r > 0.8f) {	//Blend
						noise = Mathf.Lerp( 1.0f, noise, (1.0f - sourceColor.r) * 5.0f );	//0,0..1,0
					}*/
						
					//noise -= .5f;
						
					//resultPixels[y * _texture.width + x].r = GetNoiseForPixel(x, y, _texture.width, _texture.height, seed);
					
					//float noise = sourceColor.r + ( GetNoiseForPixel(x, y, _texture.width, _texture.height, seed) - .5f ) * 1.5f;
					//float noise = noiseGradientPixels[y * _texture.width + x].r * GetNoiseForPixel(x, y, _texture.width, _texture.height, seed);
					//float noise = GetNoiseForPixel(x, y, _texture.width, _texture.height, seed);
					//float noise = GetNoiseForPixel(x, y, _texture.width, _texture.height, seed) * .8f + .2f;
					float noise;
					if (addLargeNoise) {
						noise = GetNoiseForPixel(x, y, _texture.width, _texture.height, seed) * .9f + .1f;
						float noiseGradientAmount = ( (float)noiseGradientPixels[y * _texture.width + x]) / 255.0f;
					
						float noiseAmount = 1.0f - Mathf.Pow(1.0f - noiseGradientAmount, 2.0f);
						
						//noise = 1.0f - Mathf.Pow(1.0f - noise, Mathf.Clamp( 1.0f - Mathf.Pow(1.0f - noiseAmount, 2.0f) * 10.0f, 1.0f, 10.0f));// + noiseGradientAmount * .3f;	//Have more deep(white) colors
						//noise = 1.0f - Mathf.Pow(1.0f - noise, Mathf.Clamp(noiseAmount *, 1.0f, 5.0f));// + noiseGradientAmount * .3f;	//Have more deep(white) colors
						noise = 1.0f - Mathf.Pow(1.0f - noise, 5.0f);	//Have more deep(white) colors
						//noise = ( 1.0f - Mathf.Pow(1.0f - noise, 5.0f) ) * (1.0f - noiseAmount) + noiseAmount;
						
						noise = noise * 2.0f - 1.0f;
						
						//noise = Mathf.Lerp(sourceColor, noise, Mathf.Clamp01( noiseAmount ) );
						noise = sourceColor + noise * noiseAmount * .1f;
					} else {
						float noise1 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
													( (float)y ) / ( (float)_texture.height ),
													15.0f, 15.0f, (float)seed );
						float noise2 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
													( (float)y ) / ( (float)_texture.height ),
													50.0f, 50.0f, (float)seed );
						
						float noise3 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
													( (float)y ) / ( (float)_texture.height ),
													100.0f, 100.0f, (float)seed );
						
						noise1 = noise1 * 2.0f - 1.0f;
						noise2 = noise2 * 2.0f - 1.0f;
						noise3 = noise3 * 2.0f - 1.0f;
						
						noise = sourceColor + noise1 * .1f + noise2 * .05f + noise3 * .025f;
					}
					
					//noise = noise;
					
					//Mask - apply only to non-depthmap pixels
					//resultPixels[y * _texture.width + x] = (byte) ( Mathf.Lerp(noise, sourceColor, ( (float)maskPixels[y * _texture.width + x]) / 255.0f) * 255.0f );
					//resultPixels[y * _texture.width + x] = noiseGradientPixels[y * _texture.width + x];
					resultPixels[y * _texture.width + x] = (byte)( Mathf.Clamp01(noise) * 255.0f);
				}	
			}
			
			return new WPGrayscaleImage(_texture.width, _texture.height, resultPixels);
		}
		
		/*private static Texture2D ApplyWaveNoiseTexture(Texture2D _texture) {
			Color[] sourcePixels = _texture.GetPixels();
			Color[] resultPixels = _texture.GetPixels();
			
			int seed = System.DateTime.Now.Millisecond;
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					Color sourceColor = sourcePixels[y * _texture.width + x];
					resultPixels[y * _texture.width + x].r = sourceColor.r;	//Keep red channel (depthmap).
					
					//float noiseValue1 = Mathf.PerlinNoise( 50.0f * ( (float)x ) / ( (float)_texture.width ) + seed,
					//									50.0f * ( (float)y)  / ( (float)_texture.height ) + seed );
					
					float noise1 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
															( (float)y ) / ( (float)_texture.height ),
															5.0f, 5.0f, (float)seed );
					
					float noise2 = SimplexNoise.SeamlessNoise01( ( (float)x ) / ( (float)_texture.width ),
															( (float)y ) / ( (float)_texture.height ),
															10.0f, 10.0f, (float)seed );
					
					//float noiseValue2 = Mathf.PerlinNoise( 25.0f * ( (float)x ) / ( (float)_texture.width ) + seed,
					//									2.0f * ( (float)y)  / ( (float)_texture.height ) + seed );
	
					resultPixels[y * _texture.width + x].g = noise1;
					resultPixels[y * _texture.width + x].b = noise2;
	
				}	
			}
			
			Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
			resultTexture.SetPixels( resultPixels );
			resultTexture.Apply();
			
			return resultTexture;
		}*/
		#endregion
		
		//
		//Refractions
		//
		//private bool didJustBake = false;
		//private PlaymodeBaker playModeBakerScript = null;
		
		#region Refractions
		//private static float refractionCamAspect;
		//private static float refractionCamOrthoSize;
		
		private static void WriteBakeSettings() {
			if (waterSurfaceTransform == null)
				return;
			
			//Get resolution
			int refrRes = 1024; // compute the next highest power of 2 of 32-bit v
			
			if ( !int.TryParse(refractionMapResString, out refrRes) ) {
				Debug.LogError("Please enter a valid integer into the resolution field. Aborting.");
				return;
			}
			
			refrRes = Mathf.Clamp( WPHelper.GetNearestPOT(refrRes), 32, 4096);
			
			int watermapRes = 2048; // compute the next highest power of 2 of 32-bit v
			
			if ( !int.TryParse(waterMapResString, out watermapRes) ) {
				Debug.LogError("Please enter a valid integer into the resolution field. Aborting.");
				return;
			}
			
			watermapRes = Mathf.Clamp( WPHelper.GetNearestPOT(watermapRes), 32, 4096);
			
			//playModeBakerScript.refractionMapResolution = v;
			
			string[] bakeSettings = new string[10];
			bakeSettings[0] = GetGameObjectPath( waterSurfaceTransform.gameObject );
			bakeSettings[1] = bakeStage.ToString();
			bakeSettings[2] = "bake";
			bakeSettings[3] = terrainLayerMask.value.ToString();
			bakeSettings[4] = refractionLayerMask.value.ToString();
			bakeSettings[5] = refrRes.ToString();
			bakeSettings[6] = "not set";
			bakeSettings[7] = shouldProjectRefractionMap.ToString();
			bakeSettings[8] = refractionMapScaleString;
			bakeSettings[9] = watermapRes.ToString();
			//bakeSettings[7] = "not set";
			
			System.IO.File.WriteAllLines(WPHelper.AssetPathToFilePath(waterSystemPath) + "/bakesettings.txt", bakeSettings);	
		}
		
		private static bool ReadBakeSettings() {
			if ( !LocateSystem(true) ) {
					return false;
			}
			
			string bakeSettingsPath = WPHelper.AssetPathToFilePath(waterSystemPath) + "/bakesettings.txt";
				
			if ( !System.IO.File.Exists(bakeSettingsPath) )
				return false;
			
			WPHelper.CreateWaterSystemDirs();
			
			string[] bakeSettings = System.IO.File.ReadAllLines(bakeSettingsPath);
			
			GameObject waterSurfaceGameObject = GameObject.Find( bakeSettings[0] ) as GameObject;
			
			if (waterSurfaceGameObject)
				waterSurfaceTransform = waterSurfaceGameObject.transform;
			//else
			//	Debug.LogError("No water surface gameobject found at " + bakeSettings[0]);
			
			bakeStage = int.Parse(bakeSettings[1]);
			terrainLayerMask = int.Parse(bakeSettings[3]);
			refractionLayerMask = int.Parse(bakeSettings[4]);
			refractionMapResString = bakeSettings[5];
			shouldProjectRefractionMap = bool.Parse(bakeSettings[7]);
			refractionMapScaleString = bakeSettings[8];
			
			//float.TryParse(bakeSettings[6], out refractionCamAspect);
			
			waterMapResString = bakeSettings[9];
			//float.TryParse(bakeSettings[7], out refractionCamOrthoSize);
			
			return true;
		}
		
#if BAKE_REFRACTIONS
		private static void CheckIfDoneBakingInPlayMode() {
			//Debug.Log("CheckIfDoneBakingInPlayMode");
			//Clean up
			//if (EditorApplication.isPaused) {
			{
				string bakeSettingsPath = WPHelper.AssetPathToFilePath(waterSystemPath) + "/bakesettings.txt";
				
				if ( !System.IO.File.Exists(bakeSettingsPath) )
					return;
			
				string[] bakeSettings = System.IO.File.ReadAllLines(bakeSettingsPath);
				
				if ( bakeSettings[2].Equals("done baking") ) {
					Debug.Log("Done baking in playmode.");
				
					EditorApplication.isPlaying = false;
					
					//editorWindow.Repaint();
					UpdateBakeStage(1);
				}
			}
		}
		private static string pathToRefractionMapFile, pathToRefractionMapAsset;
		private static int refractionMapResolution;
		
		private static bool PostProcessPlaymodeBake_ScaleAndCrop() {
			//System.DateTime startTime = System.DateTime.Now;
			
			//ReadBakeSettings();
			if (!waterSurfaceTransform)	//Watersurface GameObject not available yet, wait for the next cycle;
				return false;
			
			
			//UpdateBakeStage(-1);	//Important to do it here in case if the PostProcess fails.
			
			//Load PlaymodeBaker GameObject to read data
			GameObject playModeBaker = GameObject.Find("PlaymodeBaker") as GameObject;
			PlaymodeBaker playModeBakerScript = null;
			
			if (playModeBaker == null) {
				Debug.LogError("Cannot find playModeBaker GameObject. Aborting.");
				UpdateBakeStage(-1);
				return false;
			} else {
				playModeBakerScript = playModeBaker.GetComponent<PlaymodeBaker>();
			}
			
			//Load refraction map
			pathToRefractionMapFile = WPHelper.AssetPathToFilePath(waterSystemPath) + "WaterMaps/" + playModeBakerScript.refractionMapFilename;
			
			//Path without the ".temp" extension
			pathToRefractionMapAsset = waterSystemPath + "WaterMaps/" + playModeBakerScript.refractionMapFilename;//.Substring(0, playModeBakerScript.refractionMapFilename.Length - 5);
			
			//Debug.Log("refractionMapResolution: " + refractionMapResolution);
			refractionMapResolution = int.Parse(refractionMapResString);
			//int newTextureWidth = (int) (refractionCamAspect * (float)refractionMapResolution);
			
			//
			//Resize and crop the refraction map even before importing to be able to fit more pixels in height (refractionMapResolution)
			if (!File.Exists( pathToRefractionMapFile ) ) {
				Debug.LogError("Cannot find temp refraction map file at path " + pathToRefractionMapFile + "; Aborting.");
				//UpdateBakeStage(-1);
				return false;
			}
			
			//int deltaX = (int) ( (refractionCamAspect - 1.0) * (float)refractionMapResolution * 0.5f); //First pixel where refraction data is actually contained
			//Debug.Log("deltaX: " + deltaX);
			
//			using (FileStream imageFileStream = new FileStream(pathToRefractionMapFile, FileMode.Open) ) {
//				System.Drawing.Image refrImage = System.Drawing.Image.FromStream( imageFileStream );
//				refrImage = Helper.ResizeImage(refrImage, newTextureWidth, refractionMapResolution);
//				
//				System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(deltaX, 0, refractionMapResolution, refractionMapResolution);
//				
//				System.Drawing.Bitmap refrBitmap = new System.Drawing.Bitmap(refrImage);
//				
//				Debug.Log( "cropRect: " + srcRect.ToString() );
//				
//				//Crop the image
//				System.Drawing.Bitmap croppedBitmap = refrBitmap.Clone(srcRect, refrBitmap.PixelFormat);
//				System.Drawing.Image croppedImage = (System.Drawing.Image)croppedBitmap;
//				
//				
//				
//				//Resize to fit the extra pixels
//				System.Drawing.Bitmap destBitmap = new System.Drawing.Bitmap(refractionMapResolution, refractionMapResolution);
//				System.Drawing.Graphics g = System.Drawing.Graphics.FromImage( (System.Drawing.Image)destBitmap );
//				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
//				
//				System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, refractionMapResolution, refractionMapResolution);
//				
//				g.DrawImage(croppedImage, destRect);
//				g.Dispose();
//				
//				croppedImage.Dispose();
//				refrImage.Dispose();
//				refrImage = (System.Drawing.Image)destBitmap;
//				
//				//Save as a new file, remove ".temp" extension from the path
//				pathToRefractionMapFile = pathToRefractionMapFile.Substring(0, pathToRefractionMapFile.Length - 5);
//				//Debug.Log("pathToRefractionMapFile: " + pathToRefractionMapFile);
//				
//				refrImage.Save( pathToRefractionMapFile, System.Drawing.Imaging.ImageFormat.Png );
//			}
			
			AssetDatabase.Refresh();
			
			TextureImporter tImporter = AssetImporter.GetAtPath( pathToRefractionMapAsset ) as TextureImporter;
	        if( tImporter != null ) {
				tImporter.textureType = TextureImporterType.Advanced;
	            tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				tImporter.mipmapEnabled = true;
				tImporter.isReadable = true;
				tImporter.maxTextureSize = 4096;
				
				tImporter.wrapMode = TextureWrapMode.Clamp;
	            AssetDatabase.ImportAsset( pathToRefractionMapAsset );
				AssetDatabase.Refresh();
	        }
			
			Texture2D refractionMap = AssetDatabase.LoadAssetAtPath( pathToRefractionMapAsset, typeof( Texture2D ) ) as Texture2D;
			
			if (refractionMap == null) {
				Debug.LogError("refractionMap == null. path: " + pathToRefractionMapAsset);
				return false;
			}
			
			//Helper.MakeTextureReadable( refractionMap, true );
			Texture2D resizedRefraction = WPHelper.ResizeImage(refractionMap, refractionMapResolution, refractionMapResolution);
			
			//Debug.Log( "resizedRefraction: " + resizedRefraction.ToString() );
			
			WPHelper.SaveTextureAsPngAtAssetPath(resizedRefraction, pathToRefractionMapAsset, false);
			
			WPHelper.CompressTexture(resizedRefraction, true);
			
			//Helper
			
			float refrMapScale = float.Parse( refractionMapScaleString );
			
			waterSurfaceTransform.renderer.sharedMaterial.SetTextureScale("_RefractionMap", Vector2.one * 1.0f / refrMapScale );
			waterSurfaceTransform.renderer.sharedMaterial.SetTextureOffset("_RefractionMap", Vector2.one * (refrMapScale * 0.5f - 0.5f) / refrMapScale );
			//Debug.Log("PostProcessPlaymodeBake_ScaleAndCrop done");
			//Debug.Log("bake stage before: " + bakeStage);
			//UpdateBakeStage(2);
			return true;
			//Debug.Log("bake stage after: " + bakeStage);
			//Debug.Log("Successfully baked refraction map at resolution of " + refractionMapResolution + " in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		}
		
		static private int bakeAttempts = 0;
		
		private static void PostProcessPlaymodeBake_Project(int _currentPart, int _totalParts) {
			//
			//Import the image into unity

			//Debug.Log("Path to asset: " + pathToRefractionMapAsset);
			
			Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath( pathToRefractionMapAsset, typeof(Texture2D) ) as Texture2D;
			Texture2D resultTexture;
			
			Debug.Log("Bake part: " + _currentPart);
			_currentPart--;
			
			if (_currentPart == 0) {
				//Main map importer
				TextureImporter tImporter = AssetImporter.GetAtPath( pathToRefractionMapAsset ) as TextureImporter;
		        if( tImporter != null ) {
					tImporter.mipmapEnabled = false;
					tImporter.isReadable = true;
					tImporter.maxTextureSize = 4096;
					tImporter.textureType = TextureImporterType.Advanced;
		            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
					tImporter.wrapMode = TextureWrapMode.Clamp;
					
		            AssetDatabase.ImportAsset( pathToRefractionMapAsset );                
		        }
				
				resultTexture = new Texture2D(refractionMapResolution, refractionMapResolution, TextureFormat.RGB24, false);
			} else {
				//Debug.LogError("Importing texture zzzz");
				//Part map importer
				TextureImporter tImporter = AssetImporter.GetAtPath( WPHelper.AddSuffixToFilename(pathToRefractionMapAsset, "_part") ) as TextureImporter;
		        if( tImporter != null ) {
					tImporter.textureType = TextureImporterType.Advanced;
		            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
					
					tImporter.mipmapEnabled = false;
					tImporter.isReadable = true;
					tImporter.maxTextureSize = 4096;
					
					tImporter.wrapMode = TextureWrapMode.Clamp;
		            AssetDatabase.ImportAsset( pathToRefractionMapAsset );                
		        } else {
					Debug.LogError("Cannot apply import settings for part texture (unity bug). Please try rebaking and make sure that the unity editor is in focus during the entire process.");
					UpdateBakeStage(-1);
					return;
				}
				
				if (!tImporter.isReadable) {
					Debug.LogError("Failed setting the texture to readable (unity bug). Please try rebaking and make sure that the unity editor is in focus during the entire process.");
					UpdateBakeStage(-1);
					return;
				}
				
				resultTexture = AssetDatabase.LoadAssetAtPath( WPHelper.AddSuffixToFilename(pathToRefractionMapAsset, "_part"), typeof(Texture2D) ) as Texture2D;
				if (!resultTexture) {
					bakeAttempts++;
					if (bakeAttempts < 10) {
						Debug.Log("Cannot load part refraction texture at path " + WPHelper.AddSuffixToFilename(pathToRefractionMapAsset, "_part") + ". Reattempting.");
					} else {
						Debug.LogError("Cannot load part refraction texture at path " + WPHelper.AddSuffixToFilename(pathToRefractionMapAsset, "_part") +". Please try rebaking.");
						UpdateBakeStage(-1);
					}
					//UpdateBakeStage(-1);
					return;
				}
			}
			
			if (!sourceTexture) {
				bakeAttempts++;
				if (bakeAttempts < 10) {
					Debug.Log("Cannot load refraction texture at path " + pathToRefractionMapAsset + ". Reattempting.");
					UpdateBakeStage(0);
				} else {
					Debug.LogError("Cannot load refraction texture at path " + pathToRefractionMapAsset + ". Please try rebaking.");
					UpdateBakeStage(-1);
				}
				
				return;
			}
	
			if (!waterSurfaceTransform)
				Debug.LogError("waterSurfaceTransform");
			
			if (!waterSurfaceTransform.gameObject)
				Debug.LogError("waterSurfaceTransform.gameObject");
				
			int oldLayer = waterSurfaceTransform.gameObject.layer;
			//Set layer to water
			waterSurfaceTransform.gameObject.layer = 4;
			
			Bounds objBounds = waterSurfaceTransform.renderer.bounds;
			
			float xIncrement = .9f * objBounds.size.x / (float) refractionMapResolution; // / srcOverDstScaleFactor);
			float zIncrement = .9f * objBounds.size.z / (float) refractionMapResolution; // / srcOverDstScaleFactor);
			
			float yOrigin = objBounds.max.y;
					
			AttachMeshColliderToWater();
			
			float startZ, endZ, startX, endX;
			float zPartSize = objBounds.size.z / (float) (_totalParts);
			
			startZ = objBounds.min.z + zPartSize * (float) _currentPart;
			endZ = Mathf.Min(objBounds.max.z, startZ + zPartSize);
			//Debug.Log("z: {" + startZ + "; " + endZ + "}");
			
			startX = objBounds.min.x;
			endX = objBounds.max.x;
			
			float waterSurfX01, waterSurfY01;
			int sourceTextureX, sourceTextureY;
			
			Color resultPixelColor;
			
			try {
				for (float z = startZ; z <= endZ; z += zIncrement) {
					
					for (float x = startX; x <= endX; x += xIncrement) {
						waterSurfX01 = (x - objBounds.min.x) / objBounds.size.x;
						waterSurfY01 = (z - objBounds.min.z) / objBounds.size.z;
						
						sourceTextureX = (int) (waterSurfX01 * (float) refractionMapResolution);
						sourceTextureY = (int) (waterSurfY01 * (float) refractionMapResolution);
						
						if (sourceTextureX < 0 || sourceTextureX >= sourceTexture.width || sourceTextureY < 0 || sourceTextureY >= sourceTexture.height)
							continue;
						
						resultPixelColor = sourceTexture.GetPixel(sourceTextureX, sourceTextureY);
						//Color resultPixelColor = sourcePixels[sourceTextureY * sourceTexture.width + sourceTextureX];
						//Debug.Log("i: " + (sourceTextureY * sourceTexture.width + sourceTextureX) + "; tex:(" + sourceTextureX + "; " + sourceTextureY + ") color: " + resultPixelColor);
						
						Vector3 origin = new Vector3(x, yOrigin + 100.0f, z);
						//Project the pixel onto water UVs.
						
						RaycastHit[] raycasts = Physics.RaycastAll(origin, Vector3.down, 1000.0f, 1 << 4);
						foreach (RaycastHit raycast in raycasts)
						{
							
	//						int pixelI = (int) ( raycast.textureCoord.y * (float) (refractionMapResolution) ) * refractionMapResolution
	//													+ (int) (raycast.textureCoord.x * (float)refractionMapResolution );
	//						
		//					Debug.Log("uv x: " + raycast.textureCoord.x + " uv y: " + raycast.textureCoord.y
		//						+ " i: " + pixelI
		//						+ " out of " + (refractionMapResolution * refractionMapResolution)
		//						+ " x: " + (int)(raycast.textureCoord.x * (float) refractionMapResolution )
		//						+ " y: " + (int)(raycast.textureCoord.y * (float) refractionMapResolution ) );
							
	//						if ( pixelI >= refractionMapResolution * refractionMapResolution )
	//							break;
							
							resultTexture.SetPixel( (int) (raycast.textureCoord.x * (float)refractionMapResolution), (int) (raycast.textureCoord.y * (float)refractionMapResolution), resultPixelColor);
							//resultPixels[pixelI] = resultPixelColor;
						}
					}
				}
			} catch (System.Exception exc) {
				Debug.LogError("Aborting because of exception. Please rebake. " + exc.ToString());
				UpdateBakeStage(-1);
				return;
			}
			//Copy pixels
			//sourceTexture.SetPixels(resultPixels);
			//sourceTexture.Apply();
			resultTexture.Apply();
			
			//
			//Overwrite the old texture with the projected one
			//pathToRefractionMapFile = "C:/Users/Me/Desktop/refr.png";
			
//			if (_currentPart < _totalParts - 1) {
//				//Debug.Log("Saving texture as part to: " + Helper.Add_SuffixToFilename(pathToRefractionMapFile, "_part"));
//				SaveTextureAsPng(resultTexture, Helper.Add_SuffixToFilename(pathToRefractionMapFile, "_part") );
//			} else {
//				
//			}
			
			WPHelper.SaveTextureAsPng(resultTexture, WPHelper.AddSuffixToFilename(pathToRefractionMapFile, "_part") );
			
			UpdateBakeStage(bakeStage + 1);
			
			RestoreOriginalWaterCollider();
			waterSurfaceTransform.gameObject.layer = oldLayer;
			
			//Reset
			bakeAttempts = 0;
			//Debug.Log("Successfully baked refraction map at resolution of " + refractionMapResolution + " in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		}
		
		private static void FinalizeProcessingRefractionMap() {
			
			
			//Replace the original texture with final part texture
			//SaveTextureAsPng(resultTexture, Helper.Add_SuffixToFilename(pathToRefractionMapFile, "_part") );
			
			//Delete the temp image file
			//Debug.Log("deleting temp refraction map: " + pathToRefractionMapFile + ".temp");
			//System.IO.File.Delete(pathToRefractionMapFile + ".temp");
			
			//Delete the part texture
			File.Delete( WPHelper.AddSuffixToFilename(pathToRefractionMapFile, "_part") );
			if (File.Exists( WPHelper.AddSuffixToFilename(pathToRefractionMapFile, "_part") + ".meta" ) )
				File.Delete( WPHelper.AddSuffixToFilename(pathToRefractionMapFile, "_part") + ".meta" );
			
			//Try to assign refraction map to water gameobject
			if (waterSurfaceTransform) {
				Material mat = waterSurfaceTransform.renderer.sharedMaterial;
				
				if (mat) {
					AssetDatabase.ImportAsset(pathToRefractionMapAsset);
					AssetDatabase.Refresh();
					
					Texture2D refractionMapTexture = AssetDatabase.LoadAssetAtPath( pathToRefractionMapAsset, typeof(Texture2D) ) as Texture2D;
					
					WPHelper.MakeTextureReadable(refractionMapTexture, true);
					
					refractionMapTexture = AddLinearGradientToRefractionMap( refractionMapTexture );
					if (refractionMapTexture == null) {
						Debug.LogError("refractionMapTexture is null. path: " + pathToRefractionMapAsset + " Please try rebaking.");
						return;
					}
					
					WPHelper.SaveTextureAsPngAtAssetPath(refractionMapTexture, pathToRefractionMapAsset, false);
					
					AssetDatabase.Refresh();
					
					//
					//Compress the final texture
					TextureImporter tImporter = AssetImporter.GetAtPath( pathToRefractionMapAsset ) as TextureImporter;
			        if( tImporter != null ) {
						tImporter.textureType = TextureImporterType.Advanced;
			            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
						tImporter.mipmapEnabled = true;
						tImporter.isReadable = true;
						tImporter.maxTextureSize = 4096;
						
						tImporter.wrapMode = TextureWrapMode.Clamp;
			            AssetDatabase.ImportAsset( pathToRefractionMapAsset );
						
						AssetDatabase.Refresh();
			        }
					
					refractionMapTexture = AssetDatabase.LoadAssetAtPath( pathToRefractionMapAsset, typeof(Texture2D) ) as Texture2D;
					
					mat.SetTexture("_RefractionMap", refractionMapTexture );	
				}
			}
		}
		
		private static Texture2D AddLinearGradientToRefractionMap(Texture2D _texture) {
			if (_texture == null) {
				Debug.LogError( "AddLinearGradientToRefractionMap: the texture is null. Please try rebaking.");
				return null;
			}
			
			Color[] resPixels = _texture.GetPixels();
			
			for (int x = 0; x < _texture.width; x++) {
				float dX = (float)Mathf.Abs( _texture.width / 2 - x) / (float)(_texture.width / 2);
				for (int y = 0; y < _texture.height; y++) {
					float dY = (float)Mathf.Abs( _texture.height / 2 - y) / (float)(_texture.height / 2);
					
					float gradientAmount;
					
					if (dX >= .9f || dY >= .9f) {
						gradientAmount = 1.0f - Mathf.Max( (dX - .9f) * 10.0f, (dY - .9f) * 10.0f);
						//gradientAmount = Mathf.Pow(1.0f - (dX - .9f) * 10.0f, 2.0f) + Mathf.Pow( 1.0f - (dY - .9f) * 10.0f, 2.0f);
						//gradientAmount = Mathf.Sqrt( gradientAmount );
					} else {
						gradientAmount = 1.0f;	
					}
					
					resPixels[y * _texture.width + x].a = gradientAmount;
				}
			}
			
			Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, TextureFormat.ARGB32, false);
			
			resultTexture.SetPixels(resPixels);
			resultTexture.Apply();
			
			return resultTexture;
		}
#endif
		
		private static void UpdateBakeStage(int _stage) {
			//Debug.Log("UpdateBakeStage: " + _stage);
			//Debug.Log("new bake stage: " + _stage);
			bakeStage = _stage;
			
			string bakeSettingsPath = WPHelper.AssetPathToFilePath(waterSystemPath) + "/bakesettings.txt";
			if (!File.Exists( bakeSettingsPath ) )
				return;
			
			string[] bakeSettings = System.IO.File.ReadAllLines(bakeSettingsPath);
			bakeSettings[1] = _stage.ToString();
			System.IO.File.WriteAllLines(bakeSettingsPath, bakeSettings);
		}
		
		/*
		private static void StartBakeInPlaymode() {
			Debug.Log("StartBakeInPlaymode");
			
			//Delete the old refraction map
			Texture2D oldRefrTexture = null;
			if (waterSurfaceTransform.gameObject.renderer)
				oldRefrTexture = waterSurfaceTransform.gameObject.renderer.sharedMaterial.GetTexture("_RefractionMap") as Texture2D;
			
			if (oldRefrTexture) {
				string oldRefrAssetPath = AssetDatabase.GetAssetPath(oldRefrTexture);
				
				if (oldRefrAssetPath != null)
					AssetDatabase.DeleteAsset( oldRefrAssetPath );
				//string oldRefrFilePath = Helper.AssetPathToFilePath(oldRefrAssetPath);
				
//				if ( File.Exists( oldRefrFilePath ) ) {
//					File.Delete( oldRefrFilePath );
//					Debug.Log("Successfully deleted the old refraction map.");
//				}
			}
			
			GameObject tempGameObject = new GameObject("TempGameObject");
			tempGameObject.transform.position = waterSurfaceTransform.renderer.bounds.center + Vector3.up * 100.0f;
			tempGameObject.transform.LookAt( waterSurfaceTransform.renderer.bounds.center );
			
			UnityEditor.SceneView.lastActiveSceneView.AlignViewToObject( tempGameObject.transform );
			
			System.Threading.Thread.Sleep(1000);
			
			Object.DestroyImmediate( tempGameObject );
			
			GameObject playModeBaker = GameObject.Find("PlaymodeBaker") as GameObject;
			PlaymodeBaker playModeBakerScript;
			
			if (playModeBaker == null) {
				playModeBaker = new GameObject("PlaymodeBaker");
			
				playModeBakerScript = playModeBaker.AddComponent("PlaymodeBaker") as PlaymodeBaker;
			} else {
				playModeBakerScript = playModeBaker.GetComponent<PlaymodeBaker>();
			}
			
			//playModeBakerScript.isBaking = true;
			playModeBakerScript.bakeSettingsPath = WPHelper.AssetPathToFilePath(waterSystemPath) + "/bakesettings.txt";
			playModeBakerScript.waterSystemPath = waterSystemPath;
			playModeBakerScript.refractionMapFilename = System.IO.Path.GetFileNameWithoutExtension( EditorApplication.currentScene ) + "_refrmap.png";
			playModeBakerScript.waterSurfaceGameObject = waterSurfaceTransform.gameObject;
			playModeBakerScript.waterSurfaceBounds = waterSurfaceTransform.gameObject.renderer.bounds;
			playModeBakerScript.refractionLayerMask = refractionLayerMask;
			playModeBakerScript.refractionMapScale = float.Parse(refractionMapScaleString);
			
			//Debug.Log("waterSurfaceGameObject.renderer.bounds.center: " + waterSurfaceTransform.gameObject.renderer.bounds.center);
			//Debug.Log("waterSurfaceGameObject.renderer.bounds.extents: " + waterSurfaceTransform.gameObject.renderer.bounds.extents);
			//return;
			
			WriteBakeSettings();
			
			//didJustBake = true;
			
			EditorApplication.isPlaying = true;
	   		//EditorApplication.NewScene();
			//= _waterGameObject.renderer.bounds.center + Vector3.up * 10.0f;
			//UnityEditor.SceneView.lastActiveSceneView.LookAt( _waterGameObject.renderer.bounds.center );
			//editorCamera.transform.LookAt(_waterGameObject.renderer.bounds.center, 
			
			//return resultTexture;
		}*/
		#endregion
		
	#region Anisotropy
		public static void BakeAnisotropyMap() {
			//Debug.Log("BakeAnisotropyMap");
			if (!waterSurfaceTransform)
				return;
			
			if (!waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial)
				return;
			
			
			Light dirLight = FindTheBrightestDirectionalLight();
			if (!dirLight) {
				Debug.LogError("Cannot bake anisotropic map - no directional light found.");
				return;
			}
			
			Vector3 lightDirection = dirLight.transform.forward;
			
			Material mat = waterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;
			
			if (mat == null) {
				Debug.LogError("No material assigned to the water surface.");
				return;
			}
			
			string anisoMapAssetPath = waterSystemPath + "WaterMaps/" + mat.name + "_anisomap.png";
			
			Texture2D anisoMapTexture = new Texture2D(512, 512, TextureFormat.ARGB32, true);
			
			//Texture2D anisoMapTexture = waterSurfaceTransform.gameObject.renderer.sharedMaterial.GetTexture("_AnisoMap") as Texture2D;
			
			/*TextureImporter tImporter;
			
			if (!anisoMapTexture) {
				anisoMapTexture = new Texture2D(512, 512, TextureFormat.ARGB32, true);
			} else {
				tImporter = AssetImporter.GetAtPath( anisoMapAssetPath ) as TextureImporter;
		        if( tImporter != null ) {
					tImporter.mipmapEnabled = true;
					tImporter.isReadable = true;
					//tImporter.maxTextureSize = 4096;
					tImporter.textureType = TextureImporterType.Advanced;
		            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
					tImporter.wrapMode = TextureWrapMode.Repeat;
					
		            AssetDatabase.ImportAsset( anisoMapAssetPath );
					AssetDatabase.Refresh();
		        }
			}*/
			
			Color[] anisoMapPixels = new Color[anisoMapTexture.width * anisoMapTexture.height];
			
			//Works only for flat surfaces!!!
			Vector3 surfaceDir = waterSurfaceTransform.up;
			
			//Vector3 anisoSurfaceDir = lightDirection - surfaceDir * Vector3.Dot(lightDirection, surfaceDir);
			
			//int seed = System.DateTime.Now.Millisecond;
			
			//SimplexNoise simplexNoise = new SimplexNoise();
			
			float shininess = waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetFloat("_Shininess");
			//float gloss = waterSurfaceTransform.gameObject.renderer.sharedMaterial.GetFloat("_Gloss");
			
			for (int x = 0; x < anisoMapTexture.width; x++) {
				for (int y = 0; y < anisoMapTexture.height; y++) {
					int mapIndex = y * anisoMapTexture.width + x;
					
					//
					//Aniso dir
					
					Vector3 anisoDirection = Vector3.Cross(surfaceDir, lightDirection);
					
					//float noise = Mathf.PerlinNoise( 10.0f * ( (float)x ) / ( (float)anisoMapTexture.width ) + seed, 10.0f * ( (float)y ) / ( (float)anisoMapTexture.height ) + seed);
					
					/*float noise = SimplexNoise.BlurredNoise(10.0f / (float)anisoMapTexture.width,
															10.0f * ( (float)x ) / ( (float)anisoMapTexture.width ) + seed,
															10.0f * ( (float)y ) / ( (float)anisoMapTexture.height ) + seed,
													  		0.5f, 0.5f);*/
					
					/*float noise = SimplexNoise.BlurredNoise(10.0f / (float)anisoMapTexture.width,
															10.0f * ( (float)x ) / ( (float)anisoMapTexture.width ) + seed,
															10.0f * ( (float)y ) / ( (float)anisoMapTexture.height ) + seed);*/
					
					//float noise = SimplexNoise.Noise(		25.0f * ( (float)x ) / ( (float)anisoMapTexture.width ) + seed,
					//										25.0f * ( (float)y ) / ( (float)anisoMapTexture.height ) + seed,
					//								  		0.5f, 0.5f);
					
					//
					//Noise
					/*float noise = SimplexNoise.SeamlessNoise( ( (float)x ) / ( (float)anisoMapTexture.width ),
															( (float)y ) / ( (float)anisoMapTexture.height ),
															15.0f, 15.0f, (float)seed );
					
					//float rotationAmount = noise * 2.0f - 1.0f;
					
					float rotationAmount = noise * 3.0f;
					
					Quaternion rotation = Quaternion.Euler(0.0f, rotationAmount, 0.0f);
					
					anisoDirection = rotation * anisoDirection;
					
					anisoDirection.Normalize();*/
					
					/*if (x == anisoMapTexture.width / 2) {
						Debug.Log("rotationAmount: " + rotationAmount + " anisoDirection: " + anisoDirection);	
					}*/
					
					anisoDirection = ( anisoDirection + new Vector3(1.0f, 1.0f, 1.0f) ) * .5f;
					
					//Vector3 noiseVector = ( new Vector3(noise, noise, noise) + Vector3.one ) * .5f;
					
					//noiseVector.Normalize();
					
					//noiseVector = ( noiseVector + Vector3.one ) * .5f;
					//float noiseLength = Mathf.Sqrt( 3 * noise * noise );
					
					//anisoMapPixels[mapIndex] = new Color(noiseVector.x, noiseVector.y, noiseVector.z, 1.0f);
					
					//
					//Aniso lookup
					float lightDotT = ( (float)x / (float)anisoMapTexture.width ) * 2.0f - 1.0f;
					float viewDotT = ( (float)y / (float)anisoMapTexture.height ) * 2.0f - 1.0f;
					
					float anisoLookup = Mathf.Sqrt(1.0f - lightDotT * lightDotT) * Mathf.Sqrt(1.0f - viewDotT * viewDotT) - lightDotT * viewDotT;
					anisoLookup = Mathf.Pow(anisoLookup, shininess * 128.0f);// * gloss;
					
					anisoMapPixels[mapIndex] = new Color(anisoDirection.x, anisoDirection.y, anisoDirection.z, anisoLookup);
				}
			}
			
			anisoMapTexture.SetPixels(anisoMapPixels);
			anisoMapTexture.Apply();
			
			System.IO.Directory.CreateDirectory(WPHelper.AssetPathToFilePath(waterSystemPath) + "WaterMaps/" );
			//string anisoMapFilePath = WPHelper.SaveTextureAsPngAtAssetPath(anisoMapTexture, anisoMapAssetPath, true);
			
			//Debug.Log("anisoMapFilePath: " + anisoMapFilePath);
			
			anisoMapTexture = AssetDatabase.LoadAssetAtPath(anisoMapAssetPath, typeof(Texture2D) ) as Texture2D;
			
			waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_AnisoMap", anisoMapTexture);
			
			//Debug.Log("Successfully baked the aniso map");
			
			TextureImporter tImporter = AssetImporter.GetAtPath( anisoMapAssetPath ) as TextureImporter;
	        if( tImporter != null ) {
				tImporter.textureType = TextureImporterType.Advanced;
				tImporter.linearTexture = true;
	            tImporter.textureFormat = TextureImporterFormat.Automatic16bit;
				tImporter.wrapMode = TextureWrapMode.Repeat;
				//tImporter.mipmapEnabled = false;
				tImporter.SetPlatformTextureSettings("iPhone", 512, TextureImporterFormat.Automatic16bit);
				tImporter.SetPlatformTextureSettings("Android", 512, TextureImporterFormat.Automatic16bit);
	            AssetDatabase.ImportAsset(anisoMapAssetPath);                
	        }
		}
	#endregion
		
	#region Specularity
		//private static float surfaceSpecularity = 1.0f;
		//private static Vector3 viewerPosition;
		//private static Vector3 lightDir;
		
//		private static Color SpecularityCalculator(RaycastHit hitInfo, int _width, int _height) {
//			Vector3 viewDir = (hitInfo.point - viewerPosition).normalized;
//				
//			Vector3 h = -(lightDir + viewDir).normalized; 
//			float nh = Mathf.Max (0.0f, Vector3.Dot (hitInfo.normal, h) );
//			//float nh = Mathf.Max (0.0f, Vector3.Dot (Vector3.up, h) );
//			float spec = Mathf.Pow (nh, surfaceSpecularity * 128.0f);
//			
//			//Debug.Log("viewDir: " + viewDir.ToString() + " lightDir: " + lightDir.ToString() + " h: " + h.ToString() + " nh: " + nh + " spec: " + spec);
//			
//			return ( new Color(spec, spec, spec, 1.0f) * 0.5f );
//		}
		
		/*
		private static Texture2D BakeSpecularity(Texture2D _texture) {
			Light directionalLight = FindTheBrightestDirectionalLight();
			
			if (directionalLight == null) {
				Debug.LogError("No directional lights found in the scene. Specularity wasn't baked.");
				return _texture;
			}
			
			lightDir = directionalLight.transform.forward;
			
			GameObject[] terrainObjects = FindGameObjectsWithLayerMask(terrainLayerMask);
			
			viewerPosition = waterSurfaceTransform.renderer.bounds.center;
			viewerPosition.y = GetMaxHeight(terrainObjects) / 3.0f;	//Y-position of the viewer is a third of the heighest point of the terrain
			Debug.Log("viewerPosition: " + viewerPosition.ToString() );
			
			surfaceSpecularity = waterSurfaceTransform.gameObject.renderer.sharedMaterial.GetFloat("_Shininess");
			
			Texture2D resultTexture = PerpixelCalculations.Calculate(1 << waterSurfaceTransform.gameObject.layer,
														waterSurfaceTransform.renderer.bounds,
														_texture.width, _texture.height, SpecularityCalculator);
			
			Color[] sourcePixels = _texture.GetPixels();
			Color[] resultPixels = resultTexture.GetPixels();
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					float specValue = resultPixels[y * _texture.width + x].r;
					resultPixels[y * _texture.width + x] = sourcePixels[y * _texture.width + x];
					resultPixels[y * _texture.width + x].a = specValue;
				}
			}
			
			resultTexture.SetPixels(resultPixels);
			resultTexture.Apply();
			
			return resultTexture;
		}*/
		
		private static float GetMaxHeight(GameObject[] gameObjects) {
			float maxHeight = -100000.0f;
			
			foreach (GameObject go in gameObjects) {
				if (go.GetComponent<Renderer>() != null) {
					if (go.GetComponent<Renderer>().bounds.max.y > maxHeight)
						maxHeight = go.GetComponent<Renderer>().bounds.max.y;
				} else {
					foreach (Transform child in go.transform) {
						if (child.gameObject.GetComponent<Renderer>() != null) {
							if (child.gameObject.GetComponent<Renderer>().bounds.max.y > maxHeight)
								maxHeight = child.gameObject.GetComponent<Renderer>().bounds.max.y;
						}
					}
				}
			}
			
			return Mathf.Max(0.0f, maxHeight);
		}
		
		private static GameObject[] FindGameObjectsWithLayerMask(LayerMask layerMask) {
			GameObject[] allGameObjects = GameObject.FindObjectsOfType( typeof(GameObject) ) as GameObject[];
			
			List<GameObject> resultObjects = new List<GameObject>();
			foreach (GameObject go in allGameObjects) {
				if ( (1 << go.layer & layerMask.value) != 0) {
					//Debug.Log("Gameobject " + go.name + " is on the desired layermask.");
					resultObjects.Add( go );
				}
			}
			
			return resultObjects.ToArray();
		}
		
		private static Light FindTheBrightestDirectionalLight() {
			//Find the brightest directional light
			Light resultLight = null;
			
			Light[] lights = GameObject.FindObjectsOfType( typeof(Light) ) as Light[];
			List<Light> directionalLights = new List<Light>();
			
			foreach (Light light in lights) {
				if (light.type == LightType.Directional)
					directionalLights.Add( light );
			}
			
			if (directionalLights.Count <= 0)
				return null;
			
			resultLight = directionalLights[0];
			
			foreach (Light light in directionalLights) {
				if (light.intensity > resultLight.intensity)
					resultLight = light;
			}
			
			return resultLight;
		}
	#endregion
		
	#region Flow Maps
		/*private static Color FlowmapCalculator(RaycastHit hitInfo, int _width, int _height) {
			Vector3 viewDir = (hitInfo.point - viewerPosition).normalized;
				
			Vector3 h = -(lightDir + viewDir).normalized; 
			float nh = Mathf.Max (0.0f, Vector3.Dot (hitInfo.normal, h) );
			//float nh = Mathf.Max (0.0f, Vector3.Dot (Vector3.up, h) );
			float spec = Mathf.Pow (nh, surfaceSpecularity * 128.0f);
			
			//Debug.Log("viewDir: " + viewDir.ToString() + " lightDir: " + lightDir.ToString() + " h: " + h.ToString() + " nh: " + nh + " spec: " + spec);
			
			return ( new Color(spec, spec, spec, 1.0f) * 0.5f );
		}*/
		
		public static void AdjustFlowmap() {
			if (waterSurfaceTransform == null) {
				Debug.LogError("Please assign a water surface first.");
				return;
			}
			
			
			if (!waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial) {
				Debug.LogError("No material assigned to the water surface. Aborting.");
				return;
			}
			
			Texture2D flowmapTexture = waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_FlowMap") as Texture2D;
			
			if (!flowmapTexture) {
				Debug.LogError("No flow map texture assigned to the water surface. Aborting.");
				return;
			}
			
			//TextureImporterFormat initialFormat = flowmapTexture.format as TextureImporterFormat;
			
			//Helper.SetTextureImporterFormat(flowmapTexture, TextureImporterFormat.ARGB32, true);
			
			//
			//Make texture readable
			string flowmapTextureAssetPath = AssetDatabase.GetAssetPath( flowmapTexture );
			
			if (flowmapTextureAssetPath == null) {
				Debug.LogError("flowmapTexturePath is null");
				return;
			}
			
			TextureImporter tImporter = AssetImporter.GetAtPath( flowmapTextureAssetPath ) as TextureImporter;
	        if( tImporter != null ) {
				//tImporter.mipmapEnabled = false;
				tImporter.isReadable = true;
				//tImporter.maxTextureSize = 4096;
				tImporter.textureType = TextureImporterType.Advanced;
	            tImporter.textureFormat = TextureImporterFormat.ARGB32;
				tImporter.wrapMode = TextureWrapMode.Repeat;
				
	            AssetDatabase.ImportAsset( flowmapTextureAssetPath );
				AssetDatabase.Refresh();
	        }
			
			//
			//Normalize first (convert to min..1)
			Color[] flowmapPixels = flowmapTexture.GetPixels();
			//Color[] resultPixels = flowmapTexture.GetPixels();
			
			float minSpeed = 1000.0f;
			float maxSpeed = -1000.0f;
			for (int x = 0; x < flowmapTexture.width; x++) {
				for (int y = 0; y < flowmapTexture.height; y++) {
					int flowmapIndex = y * flowmapTexture.width + x;
					
					Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
					flowVelocity = flowVelocity * 2.0f - ( new Vector2(1.0f, 1.0f) );
					
					float currentSpeed = flowVelocity.magnitude;
					minSpeed = Mathf.Min(currentSpeed, minSpeed);
					maxSpeed = Mathf.Max(currentSpeed, maxSpeed);
					//resultPixels[y * _texture.width + x].a = specValue;
				}
			}
			
			
			Debug.Log("minSpeed: " + minSpeed + " maxSpeed " + maxSpeed);
			
			for (int x = 0; x < flowmapTexture.width; x++) {
				for (int y = 0; y < flowmapTexture.height; y++) {
					int flowmapIndex = y * flowmapTexture.width + x;
					
					Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
					flowVelocity = flowVelocity * 2.0f - ( new Vector2(1.0f, 1.0f) );
					
					float currentSpeed = flowVelocity.magnitude;
					
					//Convert from min..max to min..1
					currentSpeed = (currentSpeed - minSpeed) / (maxSpeed - minSpeed);
					currentSpeed *= 1.0f - minSpeed;
					currentSpeed += minSpeed;
					
					//Update the flowmap
					if (x == flowmapTexture.width / 2) 
						Debug.Log("currentSpeed: " + currentSpeed);
					
					//if (currentSpeed <= 0.0f)
					//	Debug.Log("currentSpeed: " + currentSpeed);

					flowVelocity = flowVelocity.normalized * currentSpeed;
					
//					if (x == flowmapTexture.width / 2) 
//						Debug.Log("flowVelocity norm: " + flowVelocity * 100.0f);
					
					flowmapPixels[flowmapIndex].r = (flowVelocity.x + 1.0f) * .5f;
					flowmapPixels[flowmapIndex].g = (flowVelocity.y + 1.0f) * .5f;
				}
			}
			
			flowmapTexture.SetPixels(flowmapPixels);
			flowmapTexture.Apply();
			
			string newFlowmapPath = WPHelper.SaveTextureAsPngAtAssetPath(flowmapTexture, flowmapTextureAssetPath, true);
			
			Debug.Log("newFlowmapPath: " + newFlowmapPath);
			
			flowmapTexture = AssetDatabase.LoadAssetAtPath(newFlowmapPath, typeof(Texture2D) ) as Texture2D;
			
			waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_FlowMap", flowmapTexture);
			
			Debug.Log("Successfully normalized the flowmap");
			
			//
			//Adjust speed
			//return;
			Texture2D watermapTexture = waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_WaterMap") as Texture2D;
			
			string watermapTextureAssetPath = AssetDatabase.GetAssetPath( watermapTexture );
			
			tImporter = AssetImporter.GetAtPath( watermapTextureAssetPath ) as TextureImporter;
	        if( tImporter != null ) {
				//tImporter.mipmapEnabled = false;
				tImporter.isReadable = true;
				//tImporter.maxTextureSize = 4096;
				tImporter.textureType = TextureImporterType.Advanced;
	            //tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				//tImporter.wrapMode = TextureWrapMode.Repeat;
				
	            AssetDatabase.ImportAsset( watermapTextureAssetPath );
				AssetDatabase.Refresh();
	        }
			
			//Color[] watermapPixels = watermapTexture.GetPixels();
			
			if (watermapTexture == null) {
				Debug.LogError("Cannot adjust the flowmap, because the water map cannot be found.");
				return;
			}
			
			float widthAspect = watermapTexture.width / flowmapTexture.width;
			float heightAspect = watermapTexture.height / flowmapTexture.height;
			
			const float minFlowSpeed = 0.1f;
			const float maxFlowSpeed = .7f;
			const float maxAdjustmentDepth = exportDepth * .5f;	//After 50% deep speed won't change
			
			for (int x = 0; x < flowmapTexture.width; x++) {
				for (int y = 0; y < flowmapTexture.height; y++) {
					float currentDepth = watermapTexture.GetPixel( (int)(x * widthAspect), (int)(y * heightAspect) ).r * exportDepth;
					
					int flowmapIndex = y * flowmapTexture.width + x;
					
					Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
					flowVelocity = flowVelocity * 2.0f - ( new Vector2(1.0f, 1.0f) );
					
					float currentSpeed = flowVelocity.magnitude;
					
					//Calculate new adjusted speed
					//speed = depth01 * deltaSpeed + minSpeed
					
					//Convert to 0..1
					float newSpeed;
					
					//If the pixel is too deep, don't change its speed
					if (currentDepth <= maxAdjustmentDepth) {
						newSpeed = 1.0f - (maxAdjustmentDepth - currentDepth) / maxAdjustmentDepth;
					} else {
						newSpeed = 1.0f;
					}
					
//					if (x == flowmapTexture.width / 2)
//						Debug.Log("newSpeed 1: " + newSpeed);
					
					//Convert to 0..1
					newSpeed *= (currentSpeed - minSpeed) / (1.0f - minSpeed);
					
//					if (x == flowmapTexture.width / 2)
//						Debug.Log("newSpeed 2: " + newSpeed);
					
					newSpeed = newSpeed * (maxFlowSpeed - minFlowSpeed) + minFlowSpeed;
					
//					if (x == flowmapTexture.width / 2)
//						Debug.Log("currentSpeed " + currentSpeed);
//					
//					if (x == flowmapTexture.width / 2)
//						Debug.Log("newSpeed 3: " + newSpeed);
					
					//Update the flowmap
					flowVelocity = flowVelocity.normalized * newSpeed;
					
					flowmapPixels[flowmapIndex].r = (flowVelocity.x + 1.0f) * .5f;
					flowmapPixels[flowmapIndex].g = (flowVelocity.y + 1.0f) * .5f;
				}
			}
			
			flowmapTexture.SetPixels(flowmapPixels);
			flowmapTexture.Apply();
			
			//Save flowmap
			newFlowmapPath = WPHelper.SaveTextureAsPngAtAssetPath(flowmapTexture, flowmapTextureAssetPath, true);
			Debug.Log("newFlowmapPath: " + newFlowmapPath);
			
			flowmapTexture = AssetDatabase.LoadAssetAtPath(newFlowmapPath, typeof(Texture2D) ) as Texture2D;
			waterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_FlowMap", flowmapTexture);
			
			//Revert watermap settings
			/*tImporter = AssetImporter.GetAtPath( watermapTextureAssetPath ) as TextureImporter;
	        if( tImporter != null ) {
	            //tImporter.textureFormat = flowmapTexture.format;
				//tImporter.wrapMode = TextureWrapMode.Clamp;
				tImporter.SetPlatformTextureSettings("iPhone", 512, TextureImporterFormat.AutomaticCompressed);
				tImporter.SetPlatformTextureSettings("Android", 512, TextureImporterFormat.AutomaticCompressed);
	            AssetDatabase.ImportAsset(watermapTextureAssetPath);                
	        }*/
			
			Debug.Log("Successfully adjusted the flowmap to the terrain.");
			
			/*GameObject[] terrainObjects = FindGameObjectsWithLayerMask(terrainLayerMask);

			Texture2D resultTexture = PerpixelCalculations.Calculate(1 << waterSurfaceTransform.gameObject.layer,
														waterSurfaceTransform.renderer.bounds,
														_texture.width, _texture.height, SpecularityCalculator);
			
			Color[] sourcePixels = _texture.GetPixels();
			Color[] resultPixels = resultTexture.GetPixels();
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					float specValue = resultPixels[y * _texture.width + x].r;
					resultPixels[y * _texture.width + x] = sourcePixels[y * _texture.width + x];
					resultPixels[y * _texture.width + x].a = specValue;
				}
			}
			
			resultTexture.SetPixels(resultPixels);
			resultTexture.Apply();
			
			return resultTexture;*/
			
			//
			//Reimport flowmap
			
			tImporter = AssetImporter.GetAtPath( flowmapTextureAssetPath ) as TextureImporter;
	        if( tImporter != null ) {
				//tImporter.mipmapEnabled = false;
				tImporter.isReadable = true;
				//tImporter.maxTextureSize = 4096;
				tImporter.textureType = TextureImporterType.Image;
	            tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				tImporter.wrapMode = TextureWrapMode.Repeat;
				
	            AssetDatabase.ImportAsset( flowmapTextureAssetPath );
				AssetDatabase.Refresh();
	        }
		}
	#endregion
	
		#region Parallax
		/*private static Color ParallaxCalculator(RaycastHit hitInfo, int _width, int _height) {
			Vector3 viewDir = (hitInfo.point - viewerPosition).normalized;
			
			//if ( Mathf.Abs(viewDir.x) > 0.1f || Mathf.Abs(viewDir.z) > 0.1f )
			//	return Color.black;
			
			float sinAlpha = Vector3.Dot( Vector3.up, viewDir);	//cos (beta) = n * b / (|n| * |b|)
			sinAlpha = Mathf.Abs( sinAlpha );		//!!!!!!!!!!!!
			
			float cosAlpha = Mathf.Sqrt( 1.0f - sinAlpha * sinAlpha);
			
			float tanAlpha = sinAlpha / cosAlpha;
			
			Vector2 maxDeltaUV = ( new Vector3(viewDir.x, viewDir.z) ).normalized / tanAlpha;	//Max height = 1.0
			
			Vector2 startUV = hitInfo.textureCoord;
			
			float mapHeight = 0.0f;
			float step = 0.5f;
			float currentOffset = 0.5f;
			Vector2 currentUV;
			
			bool shouldLog = ( Mathf.Abs( (hitInfo.point - viewerPosition).x) <= 10.0f && Mathf.Abs( (hitInfo.point - viewerPosition).z) <= 10.0f );
			
			/*for (float i = .0f; i <= 1.0f; i += .01f) {
				currentUV = startUV + maxDeltaUV * i;
				if (currentUV.x > 1.0f || currentUV.x < 0.0f || currentUV.y < 0.0f || currentUV.y > 1.0f) {
					if (shouldLog)
						Debug.Log("Breaking because of bounds at i " + i + " and UV " + currentUV);
					break;
				}
				
				//mapHeight = tex2D (_WaterMap, ).r;
				mapHeight = sourcePixels[ (int) (currentUV.y * (float)_width) + (int)currentUV.x].r;
				if (mapHeight >= 1.0f - i) {	//searchHeight = 1.0 - currentOffset
					if (shouldLog)
						Debug.Log("Breaking because of height at i " + i);
					break;
				}
			}
			
			for (int i = 0; i < 10; i++ ) {
				currentUV = startUV + maxDeltaUV * currentOffset;
				if (currentUV.x > 1.0f || currentUV.x < 0.0f || currentUV.y < 0.0f || currentUV.y > 1.0f) {
					if (shouldLog)
						Debug.Log("Breaking because of bounds at i " + i + " and UV " + currentUV);
					break;
				}
				
				//mapHeight = tex2D (_WaterMap, ).r;
				mapHeight = sourcePixels[ (int) (currentUV.y * (float)_width) + (int)currentUV.x].r;
				//heightsArray[(int) (waterHitInfo.textureCoord.x * _width), (int) (waterHitInfo.textureCoord.y * _height)]
				step *= 0.5f;
				if (mapHeight < 1.0f - currentOffset) {	//searchHeight = 1.0 - currentOffset
					currentOffset += step;
				} else {
					currentOffset -= step;
				}
			}
			
			if (shouldLog)
				Debug.Log("point: " + hitInfo.point + " viewDir: " + (hitInfo.point - viewerPosition) + " sinAlpha: " + sinAlpha + " maxDeltaUV: " + maxDeltaUV + " mapHeight: " + mapHeight);
			
			//-22 
			
			
			return ( new Color(mapHeight, mapHeight, mapHeight, 1.0f) );
		}
		
		Color[] sourcePixels;
		
		private static Texture2D TestParallaxMap(Texture2D _texture) {
			GameObject[] terrainObjects = FindGameObjectsWithLayerMask(terrainLayerMask);
			
			viewerPosition = waterSurfaceTransform.renderer.bounds.center;
			viewerPosition.y = GetMaxHeight(terrainObjects);	//Y-position of the viewer is a third of the heighest point of the terrain
			Debug.Log("viewerPosition: " + viewerPosition.ToString() );
			
			sourcePixels = _texture.GetPixels();
			
			Debug.Log("waterSurfaceTransform.renderer.bounds: " + waterSurfaceTransform.renderer.bounds);
			
			Texture2D resultTexture = PerpixelCalculations.Calculate(1 << waterSurfaceTransform.gameObject.layer,
														waterSurfaceTransform.renderer.bounds,
														_texture.width, _texture.height, ParallaxCalculator);
			
			
			Color[] resultPixels = resultTexture.GetPixels();
			
			for (int x = 0; x < _texture.width; x++) {
				for (int y = 0; y < _texture.height; y++) {
					float parallaxValue = resultPixels[y * _texture.width + x].r;
					resultPixels[y * _texture.width + x] = sourcePixels[y * _texture.width + x];
					resultPixels[y * _texture.width + x].g = parallaxValue;
					resultPixels[y * _texture.width + x].a = 1.0f;
				}
			}
			
			resultTexture.SetPixels(resultPixels);
			resultTexture.Apply();
			
			return resultTexture;
		}*/
		#endregion
		
		
	#region Helpers
		//private static bool wasMeshColliderAttached = true;
		//private static Collider originalWaterCollider = null;
		//private static MeshCollider waterMeshCollider = null;
		
		public static void AttachMeshColliderToWater() {
			//Debug.Log("AttachMeshColliderToWater");
			//wasMeshColliderAttached = true;
			MeshCollider waterMeshCollider = waterSurfaceTransform.GetComponent<MeshCollider>();
			//originalWaterCollider = null;
			
			//Collider[] colliders = waterSurfaceTransform.GetComponents<Collider>();
			
			//Disable all colliders apart from mesh colliders
			foreach (Collider collider in waterSurfaceTransform.GetComponents<Collider>())
			{
				if (collider.GetType() == typeof(MeshCollider) ) {
					//Debug.Log("Mesh collider found.");
					continue;
				}
				
				//Debug.Log("Disabling " + collider.GetType().ToString() );
				collider.enabled = false;
			}
			
			//If no mesh collider found, attach one.
			if ( null == waterMeshCollider ) {
				//Debug.Log("null == waterMeshCollider");
				
				//wasMeshColliderAttached = false;
				
				//Temporarily disable non-mesh colliders
				//originalWaterCollider = waterSurfaceTransform.GetComponent<Collider>();
				
				//if (originalWaterCollider)
				//	originalWaterCollider.enabled = false;
				
				waterMeshCollider = waterSurfaceTransform.gameObject.AddComponent<MeshCollider>();
			}
			
			if (waterMeshCollider.enabled == false)
				waterMeshCollider.enabled = true;
			
		}
		
		private static void RestoreOriginalWaterCollider() {
			//return;
			int nonMeshColliders = 0;
			foreach (Collider collider in waterSurfaceTransform.GetComponents<Collider>())
			{
				if (collider.GetType() == typeof(MeshCollider) ) {
					//Debug.Log("Mesh collider found.");
					continue;
				}
				
				nonMeshColliders++;
			}
			
			//Debug.Log("nonMeshColliders: " + nonMeshColliders);
			
			if (nonMeshColliders > 0) {
				foreach (Collider collider in waterSurfaceTransform.GetComponents<Collider>())
				{
					if (collider.GetType() == typeof(MeshCollider) ) {
						//Debug.Log("Mesh collider found.");
						collider.enabled = false;
						continue;
					}
					
					collider.enabled = true;
				}
			}
			
			//If there was a non-mesh collider attached, recover it
			/*if (originalWaterCollider != null)
				originalWaterCollider.enabled = true;
			
			if (!wasMeshColliderAttached)
				Object.DestroyImmediate(waterMeshCollider);
			
			wasMeshColliderAttached = true;
			originalWaterCollider = null;
			waterMeshCollider = null;*/
		}
		
		private static Texture2D DiscardAlpha(Texture2D _texture) {
			Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, false);
			resultTexture.SetPixels( _texture.GetPixels() );
			resultTexture.Apply();
			
			return resultTexture;
		}
		
		public static string GetGameObjectPath(GameObject obj)
		{
		    string path = "/" + obj.name;
		    while (obj.transform.parent != null)
		    {
		        obj = obj.transform.parent.gameObject;
		        path = "/" + obj.name + path;
		    }
		    return path;
		}
	#endregion
	}
	#endif
}