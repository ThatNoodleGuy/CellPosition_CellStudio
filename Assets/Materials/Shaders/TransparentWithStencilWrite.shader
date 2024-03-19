Shader "Custom/TransparentWithStencilWrite" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {} // Add a noise texture
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            AlphaTest Greater 0.5

            Stencil {
                Ref 1
                Comp always
                Pass replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD1; // Use local position
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex; // Reference to the noise texture
            float4 _MainTex_ST;
            float4 _Color;

            // Pass local position to fragment shader
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.localPos = v.vertex.xyz; // Pass the local position directly
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Sample the noise texture
                float noise = tex2D(_NoiseTex, i.uv * 20.0).r; // Adjust UV scaling for noise

                // Calculate distance in local space
                float dist = length(i.localPos); // Use local position for distance calculation

                // Incorporate noise into the fade effect for a more cloud-like appearance
                float alphaFade = saturate(1.0 - dist / 2.0 + noise * 0.1); // Mix distance and noise for fade

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // Apply the modified alpha fade effect
                col.a *= alphaFade * 0.85f; // Apply both the distance-based and noise-modulated fade
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
