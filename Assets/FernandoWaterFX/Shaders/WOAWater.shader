// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "FernandoWaterFX/Water" {
    Properties {
    	
        [NoScaleOffset] _BumpMap ("Normal map ", 2D) = "bump" {}
        _WaveScale ("Normal Map Scale", Range (0.001,1.5)) = 0.063
        _ReflDistort ("Refl distort (Normal Map)", Range (0,1.5)) = 0.44
        _WavesReflDistort ("Refl distort (Waves)", Range(0,5)) = 1

        WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)

    	[HideInInspector] _ReflectionTex ("Internal Reflection", 2D) = "" {}

        _LightingOpacity ("Global Lighting", Range(0,1)) = 1
        _ShadowOpacity ("Shadow Opacity", Range(0,1)) = 1
        _SkyTint ("Sky color", COLOR) = ( .34, .85, .92, 1)
        _ReflectionTint ("Reflection Tint", COLOR) = (0, 0, 0)


        _WavesColor1 ("Wave Color (min)", COLOR) = (0, 0, 0)
        _WavesColor2 ("Wave Color (max)", COLOR) = (1, 1, 1)
        [Toggle] _WavesShowNoise("Debug waves", float) = 0

        _WavesVisibility ("Waves (blending, min range, max range, steps)", Vector) = (0.57,0.01,0.21,4)
        _WavesScale ("Waves (zoom, secondary scale)", Vector) = (19.4,0.8,0,0)
        _WavesSpeed ("Waves Speed (x, y, noise movement)", Vector) = (1.2,0,0.4,0)
        [Toggle] _FoamShowNoise("Debug foam", float) = 0
        _FoamTint ("Foam color", COLOR) = ( 1, 1, 1)
        _FoamVisibility ("Foam (blending, min range, max range, steps)", Vector) = (-0.71,-0.29,0.56,5)
        _FoamScale ("Foam scale", Range(0,7)) = 3.28

        _Clamping ("Clamping (waves min, waves max, foam min, foam max)", Vector) = (0,1,0,1)
    }


    // -----------------------------------------------------------
    // Fragment program cards


    Subshader {
    	Tags { "WaterMode"="Reflective" "RenderType"="Opaque" "LightMode"="ForwardBase"}
    	Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0


            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            uniform float    _WaveScale;
            uniform float4 _WaveOffset;

            uniform float _ReflDistort;
            uniform float _WavesReflDistort;
            float _ShadowOpacity;
            float _LightingOpacity;

            struct appdata {
            	float4 vertex : POSITION;
            	float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;

            };

            struct v2f {
            	float4 pos : SV_POSITION;
                
            	float4 ref : TEXCOORD0;
                float2 bumpuv0 : TEXCOORD1;
                float2 bumpuv1 : TEXCOORD2;
            	float3 viewDir : TEXCOORD3;
                float4 normaluv : TEXCOORD4;
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;


            	UNITY_FOG_COORDS(5)
                SHADOW_COORDS(6) // put shadows data into TEXCOORD#
            };

            v2f vert(appdata v)
            {
            	v2f o;
            	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.normaluv = float4( v.texcoord.xy, 0, 0);

            	// scroll bump waves
            	float4 temp;
            	float4 wpos = mul (unity_ObjectToWorld, v.vertex);
                float4 wavescale = float4(_WaveScale, _WaveScale, _WaveScale * 0.4, _WaveScale * 0.45) * 0.01;
            	temp.xyzw = wpos.xzxz * wavescale + _WaveOffset;
            	o.bumpuv0 = temp.xy;
            	o.bumpuv1 = temp.wz;
            	
            	// object space view direction (will normalize per pixel)
            	o.viewDir.xzy = WorldSpaceViewDir(v.vertex);
            	
            	o.ref = ComputeScreenPos(o.pos);

                // lighting
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));

                TRANSFER_SHADOW(o)
            	UNITY_TRANSFER_FOG(o,o.pos);
            	return o;
            }

            sampler2D _ReflectionTex;
            sampler2D _ReflectiveColor;
            sampler2D _BumpMap;

            float4 _SkyTint;
            float4 _ReflectionTint;
            float4 _WavesColor1;
            float4 _WavesColor2;


            float _WavesShowNoise;
            uniform float4 _WavesVisibility;
            uniform float4 _WavesScale;
            uniform float4 _WavesSpeed;

            float _FoamShowNoise;
            uniform float4 _FoamVisibility;
            float _FoamScale;
            half4 _FoamTint;
            float4 _Clamping;

            // voronoi noise
            float2 hash2(float2 p ) {
               return frac(sin(float2(dot(p, float2(123.4, 748.6)), dot(p, float2(547.3, 659.3))))*5232.85324);   
            }
            float hash(float2 p) {
              return frac(sin(dot(p, float2(43.232, 75.876)))*4526.3257);   
            }
            float voronoi(float2 p, float speed) {
                float2 n = floor(p);
                float2 f = frac(p);
                float md = 5.0;
                float2 m = float2(0.0,0.0);
                for (int i = -1;i<=1;i++) {
                    for (int j = -1;j<=1;j++) {
                        float2 g = float2(i, j);
                        float2 o = hash2(n+g);
                        o = 0.5+0.5*
                            sin(_Time * speed  // speed
                                + 5.038*o);
                        float2 r = g + o - f;
                        float d = dot(r, r);
                        if (d<md) {
                          md = d;
                          m = n+g+o;
                        }
                    }
                }
                return md;
            }
            float noise(float2 p, float scale, float4 visibility, float speed) {
                float v = visibility.y * .5; // clamp min
                float a = visibility.z * .5; // clamp max
                int steps = visibility.w;

                for (int i = 0;i< min(steps,6);i++) { // steps
                    v+= voronoi(p,speed)*a;
                    p*= scale; // scale
                    a*= visibility.x; // secondary alpha
                }
                return v;
            }


            half4 frag( v2f i ) : SV_Target
            {   

            	i.viewDir = normalize(i.viewDir);

                // water waves voronoi noise
                float4 waves_uv = i.normaluv * _WavesScale.x;
                waves_uv.x += _Time * _WavesSpeed.x;
                waves_uv.y += _Time * _WavesSpeed.y;
                float4 wavesresult = smoothstep(0.0, 0.5, noise(waves_uv, _WavesScale.y, _WavesVisibility, _WavesSpeed.z));
                // clamp and adjust to 0-1
                wavesresult.rgb = (clamp(wavesresult.r, _Clamping.x, _Clamping.y) - _Clamping.x) / (_Clamping.y-_Clamping.x);
                if (_WavesShowNoise > 0 && _FoamShowNoise < 1)
                    return wavesresult;

                // second noise pass for foam on top
                float4 foamresult = smoothstep(0.0, 0.5, noise(waves_uv, _FoamScale, _FoamVisibility, _WavesSpeed.z));
                // clamp and adjust to 0-1
                foamresult.rgb = (clamp(foamresult.r, _Clamping.z, _Clamping.w) - _Clamping.z) / (_Clamping.w-_Clamping.z);
                if (_FoamShowNoise > 0)
                    return foamresult;
                foamresult.rgb *= _FoamTint;
            	
            	// combine two scrolling bumpmaps into one
                i.bumpuv0.xy *= wavesresult;
                i.bumpuv1.xy *= wavesresult;

            	half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
            	half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
            	half3 bump = (bump1 + bump2) * 0.5;
            	
            	// fresnel factor
            	half fresnelFac = dot( i.viewDir, bump );
            	   
                
                // perturb reflection/refraction UVs by bumpmap, and lookup colors
                float4 uv1 = i.ref; 
                uv1.xy += bump * _ReflDistort;
                // perturb reflection UVs by waves
                uv1.xy += bump * clamp(smoothstep(0,0.5, wavesresult * _WavesReflDistort), 0, 0.6);
                half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );


                // resulting color
            	half4 color;
                
            	//half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
            	//color.rgb = lerp( water.rgb * _SkyTint.rgb, refl.rgb * _ReflectionTint.rgb, water.a * _ReflectionTint.a );
            	//color.a = refl.a * water.a;

                //color.rgb = lerp(_SkyTint.rgb, refl.rgb * _ReflectionTint.rgb, refl.a * _ReflectionTint.a);
                half3 sky = _SkyTint.rgb;
                half3 reflection = refl.rgb;
                reflection += _ReflectionTint.rgb;
                reflection *= refl.a;
                color.rgb = sky + reflection * refl.a * _ReflectionTint.a; // premultiply alpha
                color.rgb = lerp(sky, reflection, refl.a * _ReflectionTint.a);
                color.a = _ReflectionTint.a;

            	half4 wavescolor = lerp(_WavesColor1,_WavesColor2,wavesresult.r);
                color.rgb = color.rgb * wavescolor.rgb * 2;
                //color = color + wavesresult;
                color = color + foamresult * _FoamTint.a;

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // lighting
                float3 lighting = i.diff + i.ambient;
                color.rgb *= lerp(float3(1,1,1),shadow,_ShadowOpacity);
                color.rgb *= lerp(float3(1,1,1),lighting,_LightingOpacity);
                //color.rgb *= lighting;

                UNITY_APPLY_FOG(i.fogCoord, color);

            	return color;
            }
            ENDCG

        }
    } 

    Fallback "Diffuse"
    CustomEditor "WOAWaterEditor"
} 