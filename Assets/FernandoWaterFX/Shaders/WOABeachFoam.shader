// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "FernandoWaterFX/Beach Foam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Color", COLOR) = (1,1,1)
        _NoiseAmount("Waviness", float) = 1
        _NoiseScale("Noise scale", float) = 1
        _NoiseSpeed("Noise speed", float) = 1
        _Offset("X offset", float) = 1
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                UNITY_FOG_COORDS(2)
                fixed3 diff : TEXCOORD2;
                fixed3 ambient : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _NoiseAmount;
            float _NoiseSpeed;
            float _NoiseScale;
            sampler2D _NoiseTex;
            float _Offset;

            
            v2f vert (appdata v)
            {
                v2f o;


                v.vertex = mul(unity_ObjectToWorld, v.vertex);
                v.vertex.xz += _Offset;
                float2 noiseuv = v.vertex.xz * _NoiseScale * 0.01 + _Time * _NoiseSpeed * 0.01;
                v.vertex.x += ((tex2Dlod(_NoiseTex,float4(noiseuv,1.0,1.0)).r - 0.5) * 2) * (_NoiseAmount * 0.1);
                v.vertex = mul(unity_WorldToObject, v.vertex);

                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.pos);

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
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                col.rgb *= lerp(0.5,1,shadow); // make a little darker

                return col;
            }
            ENDCG
        }
    }
}
