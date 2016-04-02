Shader "Water+/Mobile Opaque" {

Properties {
	_MainTex("Main texture", 2D) = "bump" {}
	_Cube ("Cubemap", CUBE) = "" {}
	_WaterMap ("Depth (R), Foam (G), Transparency(B) Refr strength(A)", 2D) = "white" {}
	_SecondaryRefractionTex("Refraction texture", 2D) = "bump" {}
	_AnisoMap ("AnisoDir(RGB), AnisoLookup(A)", 2D) = "bump" {}
	_Reflectivity("Reflectivity", Range (.0, 1.0)) = .3
	_Refractivity("Refractivity", Range (1.0, 5.0)) = 1.0
	_WaterAttenuation("Water attenuation", Range (0.0, 2.0)) = 1.0
	_ShallowWaterTint("Shallow water tint", Color) = (.0, .26, .39, 1.0)
	_DeepWaterTint("Deep water tint", Color) = (.0, .26, .39, 1.0)
	_Shininess ("Shininess", Range (.05, 20.0)) = 1.0
	_Gloss("Gloss", Range(0.0, 20.0)) = 10.0
	_Fresnel0 ("fresnel0", Float) = 0.1
	_EdgeFoamStrength ("Edge foam strength", Range (.0, 3.0) ) = 1.0
	
	_normalStrength("Normal strength",  Range (.01, 5.0)) = 1.0
	_refractionsWetness("Refractions wetness", Range (.0, 1.0)) = .8

	_Opaqueness("Opaqueness", Range(.0, 1.0)) = .9
}

	Category {
		Tags {"Queue"="Geometry" "IgnoreProjector"="True" "LightMode" = "ForwardBase" "ForceNoShadowCasting" = "True"}
	    
	
		 SubShader {		 
		 	Pass {
				CGPROGRAM
				#pragma exclude_renderers xbox360 flash
				
				#pragma target 2.0
				#pragma glsl
				#pragma glsl_no_auto_normalization	 //Important for mobile
				
				#pragma vertex vert
				#pragma fragment frag
				
				#undef FLOWMAP_ANIMATION_ON
				
				#define LIGHT_MODEL_ANISOTROPIC
				#define BAKED_ANISOTROPY_DIR		//Suitable only for flat horizontal surfaces
				#define BAKED_ANISOTROPIC_LIGHTING
				
				#define LIGHTING_ON
				//#define PERPIXEL_SPECULARITY_ON
				//#define CAUSTICS_ON
				//#define CAUSTICS_ALL
				#define FOAM_ON
				#define REFLECTIONS_ON
				#define WATERMAPS_ON
				//#define CALCULATE_NORMALS_ON
				
				#define REFRACTIONS_ON
				#define USE_SECONDARY_REFRACTION
				
				#define FRESNEL_ON
				#define SIMPLIFIED_FRESNEL
				
				#define OPAQUE_SURFACE
				
				#include "WaterPlusInclude.cginc"
				
				
				ENDCG
		
			}
		
		 }
		 
	}
	
	Fallback "Mobile/VertexLit"
 }