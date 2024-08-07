Shader "Unlit/PerspectiveLinesShader"
{
	Properties
	{
		[PerRendererData] _FadeInZ ("FadeInZ", Float) = 10
		[PerRendererData] _CutOffZ ("CutOffZ", Float) = 20
		[PerRendererData] _SmoothingPx ("SmoothingPx", Float) = 2
		[PerRendererData] _ActualViewSize ("ActualViewSize", Vector) = (1280, 720, 0, 0)
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType"="Transparent"
		}
		LOD 100
		
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 positions : POSITION;
				float4 color: COLOR;
				float width : TEXCOORD0;
			};
			
			appdata vert(const appdata v) { return v; }

			float4 _ActualViewSize;
			float _SmoothingPx;

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float4 color: COLOR;
				float4 ndcLine : TEXCOORD0; // xy coords of line in NDC
				float z : TEXCOORD1;
				float width : TEXCOORD2; // negative for bevel triangle (no smoothing)
			};

			[maxvertexcount(9)]
			void geom(line appdata p[2], inout TriangleStream<g2f> tris)
			{
				if (all(p[1].positions.xy == p[1].positions.zw)) return; // "primitive restart"

				const float2 aspect = _ActualViewSize.xy / 2;
				const float2 aspectRatio = aspect / aspect.y;

				const float4 clip0 = UnityObjectToClipPos(float3(p[0].positions.x, 0, p[0].positions.y));
				const float4 clip1 = UnityObjectToClipPos(float3(p[1].positions.x, 0, p[1].positions.y));
				const float2 ndc0 = clip0.xy / clip0.w;
				const float2 ndc1 = clip1.xy / clip1.w;
				float2 dir = (ndc1 - ndc0) * aspectRatio;
				dir = normalize(float2(-dir.y, dir.x));
				const float2 offset = dir / aspect * ((p[1].width + _SmoothingPx + 1) / 2);
				const float2 offset0 = offset * clip0.w;
				const float2 offset1 = offset * clip1.w;
				const g2f p0g2f = {
					clip0,
					p[0].color,
					float4(ndc0, ndc1),
					p[0].positions.y,
					p[0].width
				};
				const g2f p1g2f = {
					clip1,
					p[1].color,
					float4(ndc0, ndc1),
					p[1].positions.y,
					p[1].width
				};
				g2f rect[4] = {p0g2f, p0g2f, p1g2f, p1g2f};
				rect[0].vertex.xy -= offset0;
				rect[1].vertex.xy += offset0;
				rect[2].vertex.xy += offset1;
				rect[3].vertex.xy -= offset1;
				tris.Append(rect[0]);
				tris.Append(rect[1]);
				tris.Append(rect[2]);
				tris.Append(rect[2]);
				tris.Append(rect[3]);
				tris.Append(rect[0]);

				// bevel
				if (any(p[0].positions.xy != p[0].positions.zw))
				{
					const float4 clipp = UnityObjectToClipPos(float3(p[0].positions.z, 0, p[0].positions.w));
					const float2 ndcpp = clipp.xy / clipp.w;
					// TODO: implement this
				}
			}

			float _CutOffZ, _FadeInZ;
			
			fixed4 frag(g2f i) : SV_Target
			{
				fixed alpha = clamp((_CutOffZ - i.z) / (_CutOffZ - _FadeInZ), 0, 1) * i.color.a;
				if (i.width > 0)
				{
					const float2 aspect = _ActualViewSize.xy / 2;
					const float2 aspectRatio = aspect / aspect.y;
					float2 dir = (i.ndcLine.zw - i.ndcLine.xy) * aspectRatio;
					dir = normalize(float2(-dir.y, dir.x));
					float2 ndc = (i.vertex.xy / _ScreenParams.xy) * 2 - 1;
					ndc.y = -ndc.y;
					const float2 frag2p0 = (i.ndcLine.zw - ndc) * aspect;
					const float2 dist = abs(dot(frag2p0, dir));
					alpha *= clamp((i.width / 2 - dist) / _SmoothingPx + 0.5, 0, 1);
				}
				return fixed4(i.color.rgb, alpha);
			}
			ENDCG
		}
	}
}