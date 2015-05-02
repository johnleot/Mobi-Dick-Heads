Shader "Water+/Mobile with Shadows" {

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

	
	_normalStrength("Normal strength",  Range (.01, 5.0)) = .5
	_refractionsWetness("Refractions wetness", Range (.0, 1.0)) = .8

	_Opaqueness("Opaqueness", Range(.0, 1.0)) = .9
}

	Category {
		 SubShader {
			Tags {"Queue"="Geometry" "RenderType"="Opaque" }
		 
		 	Pass {
		 		Tags {"LightMode" = "ForwardBase"}
			    
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
				
				#define ENABLE_SHADOWS
				
				#ifdef ENABLE_SHADOWS
					#define OPAQUE_SURFACE	
					#pragma multi_compile_fwdbase
					#define UNITY_PASS_FORWARDBASE
				#endif
				
				#include "WaterPlusInclude.cginc"
				
				
				ENDCG
		
			}
			
			// Pass to render object as a shadow collector
			Pass {
				Name "ShadowCollector"
				Tags { "LightMode" = "ShadowCollector" }
				
				Fog {Mode Off}
				ZWrite On ZTest Less
		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_shadowcollector
				
				#define SHADOW_COLLECTOR_PASS
				#include "UnityCG.cginc"
				
				struct appdata {
					float4 vertex : POSITION;
				};
				
				struct v2f {
					V2F_SHADOW_COLLECTOR;
				};
				
				v2f vert (appdata v)
				{
					v2f o;
					TRANSFER_SHADOW_COLLECTOR(o)
					return o;
				}
				
				half4 frag (v2f i) : COLOR
				{
					//return 0.5;
					SHADOW_COLLECTOR_FRAGMENT(i)
				}
				ENDCG
		
			}
		
		 }
		 
	}
	
	Fallback "Water+/Mobile Opaque"
 }