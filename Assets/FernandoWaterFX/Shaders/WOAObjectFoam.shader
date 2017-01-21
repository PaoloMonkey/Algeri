Shader "FernandoWaterFX/Object Foam"
{
        // Foam effect that moves vertices outwards
	Properties
	{
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Color", COLOR) = (1,1,1)
        _Extrusion("Extrusion", float) = 1
        _NoiseAmount("Waviness", float) = 1
        _NoiseScale("Noise scale", float) = 1
        _NoiseSpeed("Noise speed", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True" "LightMode"="ForwardBase"}
        
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
            #pragma target 3.0
			
			#include "UnityCG.cginc"
            #include "Lighting.cginc"
            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 color : COLOR;
                float3 normal : NORMAL;
			};

			struct v2f
			{
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
				UNITY_FOG_COORDS(2)
                float3 diff : TEXCOORD2;
                float3 ambient : TEXCOORD3;
			};
   
            float4 _Color;
            float _NoiseAmount;
            float _NoiseSpeed;
            float _NoiseScale;
            sampler2D _NoiseTex;
            float _Extrusion;

			v2f vert (appdata v)
			{
                v2f o;

                float3 ringnormal = float3( (-1 + v.color.r * 2), 0, (-1 + v.color.b * 2));

                float2 noiseuv = v.vertex.xz * _NoiseScale * 0.01 + _Time * _NoiseSpeed * 0.01;
                float4 noisetex = tex2Dlod(_NoiseTex,float4(noiseuv,1.0,1.0));
                float n = noisetex.r;
                v.vertex.xz += ringnormal * (_Extrusion * 0.1 * (lerp(1,n,_NoiseAmount)));

                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);

                // lighting
                half3 worldNormal = half3(0,1,0);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                // compute shadows data
                TRANSFER_SHADOW(o)

                return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed4 col = fixed4(1,1,1,1) * _Color;
                //col *= i.color; //debug normals

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                col.rgb *= lerp(0.5,1,shadow); // make a little darker

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
