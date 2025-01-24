Shader "Custom/HoldBodyCullingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CullMaxZ ("Cull Max Z", float) = 10000
        _CullMinZ ("Cull Min Z", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

		Lighting Off
		ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CullMaxZ;
            float _CullMinZ;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float wz=i.worldPos.z;
                if (wz > _CullMaxZ || wz < _CullMinZ)
                {
                    discard;
                    // return fixed4(0, 0, 0, 0);
                }

                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
