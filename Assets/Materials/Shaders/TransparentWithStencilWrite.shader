Shader "Custom/TransparentWithStencilWrite" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= 0.4f; // Apply the alpha of the color property
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
