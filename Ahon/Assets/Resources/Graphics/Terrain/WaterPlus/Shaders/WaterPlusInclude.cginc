#ifndef WATERPLUS_CG_INCLUDED
#define WATERPLUS_CG_INCLUDED
	
	#if !defined(WATERMAPS_ON)
		#undef BAKED_ANISOTROPY_DIR
	#endif
	
	#ifdef FLOWMAP_ANIMATION_ON
		#define FLOWMAP_ALL_ON
	#endif
	
	#define FLAT_HORIZONTAL_SURFACE

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	
	#ifdef FOAM_ON
	half _EdgeFoamStrength;
	#endif
	
	#ifdef USE_REFR_ADJUSTMENT
	half _RefrAdj;
	#endif
	
	sampler2D _HeightGlossMap;
	sampler2D _DUDVFoamMap;
	
	#ifdef WATERMAPS_ON
	sampler2D _WaterMap;
	#endif
	//Required anyways for proper UVs
	float4 _WaterMap_ST;

	
	#ifdef REFRACTIONS_ON
		#ifdef USE_SECONDARY_REFRACTION
		sampler2D _SecondaryRefractionTex;
		float4 _SecondaryRefractionTex_ST;
		half _refractionsWetness;
		#endif
	#endif
	
	half _Refractivity;
	
	#ifdef FLOWMAP_ANIMATION_ON
		sampler2D _FlowMap;
		half flowMapOffset0, flowMapOffset1, halfCycle;
	#endif
	
	
	half _normalStrength;
	#ifdef CALCULATE_NORMALS_ON
	sampler2D _NormalMap;
	#endif
	
	samplerCUBE _Cube;
	
	#ifdef BLEND_CUBEMAPS
	samplerCUBE _Cube2;
	half _CubemapBlend;
	#endif
	
	half _Reflectivity;
	half _WaterAttenuation;
	
	fixed3 _DeepWaterTint;
	fixed3 _ShallowWaterTint;
	
	half _Shininess;
	half _Gloss;
	
	half _Fresnel0;
	
	#ifdef LIGHT_MODEL_ANISOTROPIC
	sampler2D _AnisoMap;
	#endif
	
	#ifdef CAUSTICS_ON
	half _CausticsStrength;
	half _CausticsScale;
	
	sampler2D _CausticsAnimationTexture;
	half3 causticsOffsetAndScale;
	half4 causticsAnimationColorChannel;
	#endif

	half _Opaqueness;
	
	struct v2f {
    	float4  pos : SV_POSITION;
    	float2	uv_MainTex : TEXCOORD0;
    	
    	half2	uv_WaterMap : TEXCOORD1;
    	
    	fixed3	viewDir	: COLOR;
    	
    	#if defined(PERPIXEL_SPECULARITY_ON) || defined(LIGHTING_ON)
    	fixed3	lightDir : TEXCOORD2;
    	#endif
    	
    	#ifdef ENABLE_SHADOWS
    	LIGHTING_COORDS(4,3)
		#else
			#ifdef VERTEX_COLOR_INPUT
			fixed4 vertexColor : TEXCOORD3;
			#endif
    	#endif
    	
    	#ifdef REFRACTIONS_ON
	    	float2 uv_SecondaryRefrTex : TEXCOORD5;
    	#endif
    	
    	#ifdef LIGHT_MODEL_ANISOTROPIC
	    	#ifdef BAKED_ANISOTROPY_DIR
	    	#else
		    	#ifndef PERPIXEL_SPECULARITY_ON
		    	fixed3 anisoDir : TEXCOORD6;
		    	#endif
	    	#endif
    	#endif
    	
    	#ifndef FLAT_HORIZONTAL_SURFACE
    	fixed3	normal;		//In world space
    	fixed3  tangent;		//In world space
		fixed3  binormal : TEXCOORD7;
    	#endif
	};
	
#ifdef VERTEX_COLOR_INPUT
	v2f vert (appdata_full v)
#else
	v2f vert (appdata_tan v)
#endif
	{
	    v2f o;
	    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	    o.uv_MainTex = TRANSFORM_TEX (v.texcoord, _MainTex);
	    
		o.uv_WaterMap = v.texcoord;// TRANSFORM_TEX(v.texcoord, _WaterMap);
		
	    o.viewDir = WorldSpaceViewDir(v.vertex);

		#ifdef VERTEX_COLOR_INPUT
		o.vertexColor = v.color;
		#endif

		#ifndef FLAT_HORIZONTAL_SURFACE
	    o.tangent = mul( _Object2World, half4(v.tangent.xyz, 0.0) );
	    o.normal = mul( half4(v.normal.xyz, 0.0), _World2Object );
	    o.binormal = cross(o.normal, o.tangent) * v.tangent.w;
	    
	    o.tangent = normalize(o.tangent);
	    o.normal = normalize(o.normal);
	    o.binormal = normalize(o.binormal);
		#endif

	    #if defined(PERPIXEL_SPECULARITY_ON) || defined(LIGHTING_ON)
	    o.lightDir = normalize(WorldSpaceLightDir( v.vertex ));
	    #endif
	    
	    #ifdef LIGHT_MODEL_ANISOTROPIC
	   		#ifdef BAKED_ANISOTROPY_DIR
	    	#else
	    		#ifndef PERPIXEL_SPECULARITY_ON
			    	#ifndef FLAT_HORIZONTAL_SURFACE
			    	o.anisoDir = normalize( cross(o.lightDir, o.normal) );
			    	#else
			    	o.anisoDir = normalize( cross(o.lightDir, fixed3(0.0, 1.0, 0.0) ) );
			    	#endif
			    #endif
	    	#endif
    	#endif
	    
	    #ifdef REFRACTIONS_ON
			#ifdef USE_SECONDARY_REFRACTION
			o.uv_SecondaryRefrTex = TRANSFORM_TEX (v.texcoord, _SecondaryRefractionTex);
			#endif
	    #endif
	    
	    #ifdef ENABLE_SHADOWS
	    TRANSFER_VERTEX_TO_FRAGMENT(o);
	    #endif
	    
	    return o;
	}
	
	#ifdef CAUSTICS_ON
	inline half CalculateCaustics(float2 uv, half waterAttenuationValue) {
		half4 causticsFrame = tex2D(_CausticsAnimationTexture, frac(uv * _CausticsScale) * causticsOffsetAndScale.zz + causticsOffsetAndScale.xy );

		return (causticsAnimationColorChannel.x * causticsFrame.r
				+ causticsAnimationColorChannel.y * causticsFrame.g
				+ causticsAnimationColorChannel.z * causticsFrame.b) * _CausticsStrength * (1.0 - waterAttenuationValue);
	}
	#endif
	
	inline half CalculateAttenuation(half sinAlpha, fixed3 normViewDir, half4 waterMapValue) {		
			#ifndef BINARY_SEARCH_PARALLAX_ATTEN
			    float heightValue = waterMapValue.r;

			    return heightValue;
			    
		    #endif
	}
	
	inline fixed3 CalculateNormalInTangentSpace(half2 uv_MainTex, out half2 _displacedUV, fixed3 normViewDir,
												half4 waterMapValue
												#ifdef FLOWMAP_ANIMATION_ON
												,half2 flowmapValue, half flowLerp, half flowSpeed
													#ifdef FLOWMAP_ADD_NOISE_ON
													,half flowmapCycleOffset
													#endif
												#endif
												)
	{
		#ifdef CALCULATE_NORMALS_ON
			float2 normalmapUV = uv_MainTex;
			
			_displacedUV = normalmapUV;

			#ifdef FLOWMAP_ANIMATION_ON	
				#ifdef FLOWMAP_ADD_NOISE_ON
				flowmapCycleOffset *= .2;

				fixed3 normalT0 = UnpackNormal( tex2D(_NormalMap, normalmapUV + flowmapValue * (flowmapCycleOffset * .5 + flowMapOffset0) ) );
				fixed3 normalT1 = UnpackNormal( tex2D(_NormalMap, normalmapUV + flowmapValue * (flowmapCycleOffset * .5 + flowMapOffset1) ) );
				#else
				fixed3 normalT0 = UnpackNormal( tex2D(_NormalMap, normalmapUV + flowmapValue * flowMapOffset0 ) );
				fixed3 normalT1 = UnpackNormal( tex2D(_NormalMap, normalmapUV + flowmapValue * flowMapOffset1 ) );
				#endif
					
			fixed3 pNormal = lerp( normalT0, normalT1, flowLerp );
				
			//Account for speed
			pNormal.z /= max(flowSpeed, .1);	//Account for flow map average velocity
			#else
			fixed3 pNormal = UnpackNormal( tex2D(_NormalMap, normalmapUV) );
			#endif
			
			pNormal.z /= _normalStrength;
			pNormal = normalize(pNormal);	//Very very important to normalize!!!
			
			return pNormal;
		#else
		
			return fixed3(0.0, 0.0, 1.0);
		#endif
	}
	
	#ifdef FLOWMAP_ANIMATION_ON
	inline fixed4 SampleTextureWithRespectToFlow(sampler2D _tex, float2 _uv, half2 flowmapValue, half flowLerp
													#ifdef FLOWMAP_ADD_NOISE_ON
													, flowmapCycleOffset
													#endif
													) {
		#ifdef FLOWMAP_ADD_NOISE_ON
			fixed4 texT0 = tex2D(_tex, _uv + flowmapValue * (flowmapCycleOffset * .5 + flowMapOffset0) );
			fixed4 texT1 = tex2D(_tex, _uv + flowmapValue * (flowmapCycleOffset * .5 + flowMapOffset1) );
		#else
		  	fixed4 texT0 = tex2D(_tex, _uv + flowmapValue * flowMapOffset0 );
			fixed4 texT1 = tex2D(_tex, _uv + flowmapValue * flowMapOffset1 );
		#endif
		
		return lerp( texT0, texT1, flowLerp );
	}
	#endif
	
	#ifdef REFRACTIONS_ON
		inline fixed3 CalculateRefraction(
											float2 uv_Caustics,
											#ifdef USE_SECONDARY_REFRACTION
											half refrStrength,
											float2 uv_SecondaryRefrTex,
											#endif
												
											#ifdef CAUSTICS_ON
											half waterAttenuationValue,
											#endif
											fixed3 normViewDir,
											// half sinAlpha,
											float2 _dudvValue)
		{
			//Unpack and scale
			float2 dudvValue = _dudvValue * _Refractivity / 100000.0;
				
			fixed3 refractionColor;
				
			//Flat
			#ifdef USE_SECONDARY_REFRACTION
				refractionColor = tex2D(_SecondaryRefractionTex, uv_SecondaryRefrTex + dudvValue * _SecondaryRefractionTex_ST.x).rgb * _refractionsWetness;
			#endif
				
			#ifdef CAUSTICS_ON
				refractionColor += CalculateCaustics(uv_Caustics + dudvValue, waterAttenuationValue);
			#endif

			return refractionColor;
		}
	#endif
	
	inline fixed3 CombineEffectsWithLighting(
							#ifdef REFRACTIONS_ON
								fixed3 refraction, half refrStrength,
							#endif
							#ifdef REFLECTIONS_ON
								fixed3 reflection,
							#endif	
								fixed3 pNormal,
									
							fixed3 normViewDir,
							#ifdef LIGHTING_ON
							fixed3 normLightDir,
							#endif
							half2 uv_MainTex, half waterAttenuationValue
							#ifdef FOAM_ON
							,inout half foamAmount,
								fixed foamValue
							#endif
							#ifdef LIGHTING_ON
								#ifdef LIGHT_MODEL_ANISOTROPIC
									#ifndef PERPIXEL_SPECULARITY_ON
										#ifdef BAKED_ANISOTROPY_DIR
										,half2 anisoDirUV
										#else
										,fixed3 anisoDir
										#endif
									#else
										,fixed3 lightDir
									#endif
								#endif
							#endif
							)
	{
		half nDotView = dot(pNormal, normViewDir);		//Masking
		#ifdef LIGHTING_ON
			#ifndef LIGHT_MODEL_ANISOTROPIC
		    fixed3 halfView = normalize ( normLightDir + normViewDir );	//No need in anisotropic
		    half nDotHalf = saturate( dot (pNormal, halfView) );
		    half spec = pow (nDotHalf, _Shininess * 128.0) * _Gloss * 10;
		    #endif
		    
		    
		    #ifndef LIGHT_MODEL_FASTEST
		    half nDotLight = dot(pNormal, normLightDir);	//Shadows (diffuse)
		    #endif
		    
			#ifdef LIGHT_MODEL_ANISOTROPIC
		    
				#ifndef PERPIXEL_SPECULARITY_ON
					#ifdef BAKED_ANISOTROPY_DIR
					fixed3 anisoDir = tex2D(_AnisoMap, anisoDirUV).rgb * 2.0 - 1.0;
					#endif
				#else
				fixed3 anisoDir = normalize( cross(pNormal, lightDir) );
				#endif
		    
				half lightDotT = dot(normLightDir, anisoDir);
				half viewDotT = dot(normViewDir, anisoDir);
		    
			    #ifdef BAKED_ANISOTROPIC_LIGHTING
			    half spec = tex2D(_AnisoMap, ( float2(lightDotT, viewDotT) + 1.0 ) * .5).a;
			    #else
			    half spec = sqrt(1.0 - lightDotT * lightDotT) * sqrt(1.0 - viewDotT * viewDotT) - lightDotT * viewDotT;
			    spec = pow(spec, _Shininess * 128.0);
			    #endif
			    
			    spec *= _Gloss;
		    
				//Masking & self-shadowing
				spec *= max(.0, nDotView) * max(.0, nDotLight);
		    
				//Prevent highlights from leaking to the wrong side of the light
				spec *= max(sign(dot(normViewDir, -normLightDir)), 0.0);
		   	#endif
		    
			fixed specularComponent = spec;

		    #ifndef ENABLE_SHADOWS
		    specularComponent *= _LightColor0.r / 2;
		    #endif
	    #endif

	    #ifdef FRESNEL_ON
		    #ifdef SIMPLIFIED_FRESNEL
		    half fresnel = .5 - nDotView;
		    	#ifdef USE_FRESNEL0_IN_SIMPLIFIED
		    	fresnel += _Fresnel0;
		    	#endif
		    fresnel = max(0.0, fresnel);
		    #else
		  	half fresnel = _Fresnel0 + (1.0 - _Fresnel0) * pow( (1.0 - nDotView ), 5.0);
		  	fresnel = max(0.0, fresnel - .1);
		  	#endif

		   	#ifdef LIGHTING_ON
		    specularComponent *= fresnel;
		    #endif
		#endif
	    
	    #ifdef LIGHTING_ON
	    specularComponent = specularComponent * specularComponent * 10.0;
	    #endif
	    	    
		fixed3 finalColor;

	    finalColor = lerp(_ShallowWaterTint, _DeepWaterTint, waterAttenuationValue );
	   
	    #ifdef REFRACTIONS_ON
	    	#ifdef USE_SECONDARY_REFRACTION
		    	
		    	//!!!!!!!!!!!!!!!!!!!!
		    	//!Magic! Don't touch!
		    	//!!!!!!!!!!!!!!!!!!!!
		    	
		    	refraction = lerp(refraction, _ShallowWaterTint, refrStrength * .5);
		    	#ifndef USE_REFR_ADJUSTMENT
			    	finalColor = lerp(refraction, finalColor, saturate( max(waterAttenuationValue, refrStrength * .5) * .8 ) );
			    #else
			    	half refrAmount = saturate( max(waterAttenuationValue, refrStrength * .5) * .8 );
			    	refrAmount = lerp(refrAmount, 1.0, _RefrAdj);
			    	finalColor = lerp(refraction, finalColor, refrAmount);
		    	#endif
		    #else
		    	finalColor = lerp(refraction, finalColor, refrStrength); 
	    	#endif
	    #else
			//Fallback to default texture if no refractions and no watermaps
	    	#ifndef WATERMAPS_ON
	   		finalColor = tex2D(_MainTex, uv_MainTex).rgb;
	   		#endif
	    #endif
	    
	    //Add reflection
	    #ifdef REFLECTIONS_ON
		    #ifdef FRESNEL_ON
		    finalColor = lerp(finalColor, reflection, clamp(fresnel, 0.0, _Reflectivity) );
		    #else
		    finalColor = lerp(finalColor, reflection, _Reflectivity);
		    #endif
		#endif
	    
	    //Foam isn't reflective, it goes on top of everything
	    #ifdef FOAM_ON
	    	foamAmount = saturate(foamAmount * foamValue);
			finalColor.rgb = lerp(finalColor, fixed3(foamValue, foamValue, foamValue), foamAmount);
	    #endif
	    
	    #ifdef LIGHTING_ON
		    #if defined(ADD_DIFFUSE_LIGHT_COMPONENT)
			float3 diffuseComponent = nDotLight *_LightColor0.rgb;
		    
		    return (finalColor * diffuseComponent + specularComponent) + UNITY_LIGHTMODEL_AMBIENT.rgb;
		    #else
		    return (finalColor * _LightColor0.rgb + specularComponent) + UNITY_LIGHTMODEL_AMBIENT.rgb;
		    #endif
		#else
			return finalColor;
		#endif
	}

	fixed4 frag (v2f i) : COLOR
	{
		fixed4 outColor;

		#ifdef WATERMAPS_ON
		fixed4 waterMapValue = tex2D (_WaterMap, i.uv_WaterMap);
		#else
			#ifdef VERTEX_COLOR_INPUT
			fixed4 waterMapValue = i.vertexColor;
			//return waterMapValue;
			#else
			fixed4 waterMapValue = fixed4(0.5, 0.0, 0.0, 0.0);
			#endif
		#endif
		
		fixed3 normViewDir = normalize(i.viewDir);
	    
	    #ifdef FLOWMAP_ANIMATION_ON
	    	half2 flowmapValue = tex2D (_FlowMap, i.uv_WaterMap).rg * 2.0 - 1.0;
	    	half flowSpeed = length(flowmapValue);
	    	
	    	half flowLerp = ( abs( halfCycle - flowMapOffset0 ) / halfCycle );
	    	
	    	#ifdef FLOWMAP_ADD_NOISE_ON
	    	half flowmapCycleOffset = 0.0;// * tex2D( _WaterMap, i.uv_MainTex * .3).g;	//Noise
	    	#endif
	    #endif
	    
	    #ifdef CALCULATE_NORMALS_ON
	    	half2 displacedUV;
			fixed3 pNormal = CalculateNormalInTangentSpace(i.uv_MainTex, displacedUV, normViewDir,
														//sinAlpha,
														waterMapValue
														#ifdef FLOWMAP_ANIMATION_ON
														,flowmapValue, flowLerp, flowSpeed
															#ifdef FLOWMAP_ADD_NOISE_ON
															,flowmapCycleOffset
															#endif
														#endif
														);
	    	
	    	#ifndef FLAT_HORIZONTAL_SURFACE
	    	pNormal = (i.tangent * pNormal.x) + (i.binormal * pNormal.y) + (i.normal * pNormal.z);
	    	#else
	    		pNormal = fixed3(-pNormal.x, pNormal.z, -pNormal.y);
	    	#endif						
		#else
			#ifndef FLAT_HORIZONTAL_SURFACE
				fixed3 pNormal = i.normal;
			#else
				fixed3 pNormal = fixed3(0.0, 1.0, 0.0);
			#endif
	    #endif
	    
		half waterAttenuationValue = saturate( waterMapValue.r * _WaterAttenuation );
	    
	    //
	    //Sample dudv/foam texture
	    #if defined(REFRACTIONS_ON) || defined(FOAM_ON)
		    #if defined(FLOWMAP_ANIMATION_ON) && defined(FLOWMAP_ALL_ON)
				fixed3 dudvFoamValue = SampleTextureWithRespectToFlow(_DUDVFoamMap, i.uv_MainTex, flowmapValue, flowLerp
																		#ifdef FLOWMAP_ADD_NOISE_ON
																		, flowmapCycleOffset
																		#endif
																		).rgb;				
			#else
				fixed3 dudvFoamValue = tex2D(_DUDVFoamMap, i.uv_MainTex).rgb;
			#endif
		#endif
	     
     	#ifdef REFRACTIONS_ON
	     	float2 dudvValue = dudvFoamValue.rg;
			dudvValue = dudvValue * 2.0 - float2(1.0, 1.0);
		
			fixed3 refrColor = CalculateRefraction(
													i.uv_MainTex,
													#ifdef USE_SECONDARY_REFRACTION
													waterMapValue.a,
													i.uv_SecondaryRefrTex,
													#endif
													#ifdef CAUSTICS_ON
													waterAttenuationValue,
													#endif
													normViewDir,
													dudvValue);
			
		#else
			#ifdef CAUSTICS_ON
				fixed3 refrColor = CalculateCaustics(i.uv_MainTex, waterAttenuationValue);
			#else
				fixed3 refrColor = tex2D(_MainTex, i.uv_MainTex).rgb;
			#endif
		#endif
	    
	    #if !defined(CALCULATE_NORMALS_ON) && defined(REFRACTIONS_ON)
	    float2 _dudvValue = dudvValue * _normalStrength / 50.0;
	    
	    pNormal.xz += _dudvValue.xy;
	    #endif
	    
	    //
	    //Reflectivity
	    #if defined(REFLECTIONS_ON)	    
			fixed3 refl = reflect( -normViewDir, pNormal);

			#ifndef BLEND_CUBEMAPS
				fixed3 reflectCol = texCUBE( _Cube , refl ).rgb;
			#else
				fixed3 reflectCol = lerp( texCUBE( _Cube , refl ).rgb, texCUBE( _Cube2 , refl ).rgb, _CubemapBlend);
			#endif
			
		#endif
		
		#ifdef FOAM_ON
			fixed foamValue = dudvFoamValue.b;
			half foamAmount = waterMapValue.g * _EdgeFoamStrength;
		
			#ifdef FLOWMAP_ANIMATION_ON
			//Have foam in the undefined areas
			foamAmount = max(foamAmount, flowSpeed * foamValue * .5);
			#endif
		#endif
		
		#ifdef ENABLE_SHADOWS
	    refrColor *= SHADOW_ATTENUATION(i);
	    #endif
		
		outColor.rgb = CombineEffectsWithLighting(
									#ifdef REFRACTIONS_ON
									refrColor, waterMapValue.a,
									#endif
									#ifdef REFLECTIONS_ON
									reflectCol,
									#endif
									pNormal,
									normViewDir,
									#ifdef LIGHTING_ON
									i.lightDir,
									#endif
									i.uv_MainTex, waterAttenuationValue
									#ifdef FOAM_ON
									,foamAmount,
									foamValue
									#endif
									#ifdef LIGHTING_ON
										#ifdef LIGHT_MODEL_ANISOTROPIC
										#ifdef LIGHT_MODEL_ANISOTROPIC
											#ifndef PERPIXEL_SPECULARITY_ON
												#ifdef BAKED_ANISOTROPY_DIR
												,i.uv_WaterMap
												#else
													#if !defined(CALCULATE_NORMALS_ON) && defined(REFRACTIONS_ON)
													,fixed3(i.anisoDir.xy, i.anisoDir.z)
													#else
													,i.anisoDir
													#endif
												#endif
											#else
												,i.lightDir
											#endif
										#endif
										#endif
									#endif
									);
		//
		//Alpha
		#ifndef UNIFORM_ALPHA
			#ifndef OPAQUE_SURFACE
				#if defined(WATERMAPS_ON) || defined(VERTEX_COLOR_INPUT)
					outColor.a = waterMapValue.b;
				#else
				    outColor.a = 1.0;
				#endif
				
			#else
			outColor.a = 1;
			#endif
		#else
			outColor.a = 1;
		#endif

		outColor.a *= _Opaqueness;
		
	    return outColor;
	}	
#endif